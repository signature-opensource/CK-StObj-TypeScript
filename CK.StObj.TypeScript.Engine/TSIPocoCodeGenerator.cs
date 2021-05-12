using CK.CodeGen;
using CK.Core;
using CK.Setup;
using CK.TypeScript.CodeGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CK.StObj.TypeScript.Engine
{
    /// <summary>
    /// Handles TypeScript generation of <see cref="IPoco"/> by reproducing the IPoco interfaces
    /// in the TypeScript world.
    /// This generator doesn't generate any IPoco by default: they must be "asked" to be generated
    /// by instantiating a <see cref="TSTypeFile"/> for the IPoco interface, its <see cref="ConfigureTypeScriptAttribute"/>
    /// method only sets a <see cref="ITSTypeFileBuilder.Finalizer"/>.
    /// </summary>
    /// <remarks>
    /// This code generator is directly added by the <see cref="TypeScriptAspect"/> as the first <see cref="TypeScriptContext.GlobalGenerators"/>,
    /// it is not initiated by an attribute like other code generators (typically thanks to a <see cref="ContextBoundDelegationAttribute"/>).
    /// </remarks>
    internal class TSIPocoCodeGenerator : ITSCodeGenerator
    {
        readonly IPocoSupportResult _poco;

        internal TSIPocoCodeGenerator( IPocoSupportResult poco )
        {
            _poco = poco;
        }

        public bool ConfigureTypeScriptAttribute( IActivityMonitor monitor,
                                                  ITSTypeFileBuilder builder,
                                                  TypeScriptAttribute attr )
        {
            var type = builder.Type;
            bool isPoco = typeof( IPoco ).IsAssignableFrom( type );
            if( isPoco )
            {
                if( type.IsClass )
                {
                    var pocoClass = _poco.Roots.FirstOrDefault( root => root.PocoClass == type );
                    if( pocoClass != null )
                    {
                        attr.TypeName = TypeScriptContext.GetPocoClassNameFromPrimaryInterface( pocoClass );
                        attr.SameFileAs = pocoClass.PrimaryInterface;
                        builder.Finalizer = ( m, f ) => EnsurePocoClass( m, f, pocoClass ) != null;
                    }
                    else
                    {
                        monitor.Warn( $"Type {type} is a class that implements at least one IPoco but is not a registered PocoClass. It is ignored." );
                    }
                }
                else
                {
                    if( _poco.AllInterfaces.TryGetValue( type, out IPocoInterfaceInfo? itf ) )
                    {
                        if( itf.Root.PrimaryInterface != itf.PocoInterface )
                        {
                            if( attr.SameFileAs != null
                                || attr.SameFolderAs != null
                                || attr.Folder != null
                                || attr.FileName != null )
                            {
                                monitor.Warn( $"IPoco '{type.Name}' must not specify a SameFileAs, SameFolderAs, Folder or FileName since it will use the same file as the primary interface." );
                                attr.SameFolderAs = null;
                                attr.Folder = null;
                                attr.FileName = null;
                            }
                            attr.SameFileAs = itf.Root.PrimaryInterface;
                        }
                        else
                        {
                            if( attr.SameFileAs == null && attr.FileName == null )
                            {
                                string typeName = TypeScriptContext.GetPocoClassNameFromPrimaryInterface( itf.Root );
                                attr.FileName = TypeScriptContext.SafeFileChars( typeName ) + ".ts";
                            }
                        }
                        builder.Finalizer = ( m, f ) => EnsurePocoInterface( m, f, itf ) != null;
                    }
                    else
                    {
                        monitor.Warn( $"Type {type} extends IPoco but cannot be found in the registered interfaces. It is ignored." );
                    }
                }
            }
            return true;
        }

        bool ITSCodeGenerator.GenerateCode( IActivityMonitor monitor, TypeScriptContext context ) => true;

        TSTypeFile? EnsurePocoClass( IActivityMonitor monitor, TypeScriptContext g, IPocoRootInfo root )
        {
            var t = g.DeclareTSType( monitor, root.PocoClass, requiresFile: true );
            return t != null ? EnsurePocoClass( monitor, t, root ) : null;
        }

        TSTypeFile? EnsurePocoClass( IActivityMonitor monitor, TSTypeFile tsTypedFile, IPocoRootInfo root )
        {
            if( tsTypedFile.TypePart == null )
            {
                var b = tsTypedFile.EnsureTypePart();
                b.Append( "export class " ).Append( tsTypedFile.TypeName );
                List<TSTypeFile> interfaces = new();
                foreach( IPocoInterfaceInfo i in root.Interfaces )
                {
                    var itf = EnsurePocoInterface( monitor, tsTypedFile.Context, i );
                    if( itf == null ) return null;
                    if( interfaces.Count == 0 )
                    {
                        b.Append( " implements " );
                    }
                    else b.Append( ", " );
                    interfaces.Add( itf );
                    b.AppendImportedTypeName( itf );
                }
                b.OpenBlock();
                foreach( var itf in interfaces )
                {
                    var props = itf.File.Body.FindNamedPart( itf.TypeName )!.FindNamedPart( "Props" );
                    Debug.Assert( props != null );
                    b.Append( $"// Properties from {itf.TypeName}." ).NewLine()
                     .Append( props.ToString() );
                }
            }
            return tsTypedFile;
        }

        TSTypeFile? EnsurePocoInterface( IActivityMonitor monitor, TypeScriptContext g, IPocoInterfaceInfo i )
        {
            var t = g.DeclareTSType( monitor, i.PocoInterface, requiresFile: true );
            return t != null ? EnsurePocoInterface( monitor, t, i ) : null;
        }

        TSTypeFile? EnsurePocoInterface( IActivityMonitor monitor, TSTypeFile tsTypedFile, IPocoInterfaceInfo i )
        {
            if( tsTypedFile.TypePart == null )
            {
                // First, we ensure the class so that it appears at the beginning of the file.
                if( EnsurePocoClass( monitor, tsTypedFile.Context, i.Root ) == null )
                {
                    return null;
                }
                // Double check here since EnsurePocoClass may have called us already.
                if( tsTypedFile.TypePart == null )
                {
                    var b = tsTypedFile.EnsureTypePart();
                    b.AppendDocumentation( monitor, i.PocoInterface );
                    b.Append( "export interface " ).Append( tsTypedFile.TypeName );
                    bool hasInterface = false;
                    foreach( Type baseInterface in i.PocoInterface.GetInterfaces() )
                    {
                        // If the base interface is a "normal" IPoco interface, then we ensure that it is
                        // generated.
                        // If the base interface is "another interface", we'll consider it only if it has been
                        // declared.
                        var baseItf = i.Root.Interfaces.FirstOrDefault( p => p.PocoInterface == baseInterface );
                        if( baseItf != null )
                        {
                            var fInterface = EnsurePocoInterface( monitor, tsTypedFile.Context, baseItf );
                            if( fInterface == null ) return null;
                            AddBaseInterface( b, ref hasInterface, fInterface );
                        }
                        else
                        {
                            // If the base interface does not belong to the OtherInterfaces: we skip them (it should be
                            // the IPoco or IClosedPoco).
                            if( i.Root.OtherInterfaces.Contains( baseInterface ) )
                            {
                                // We handle the already declared base interfaces only.
                                var declared = tsTypedFile.Context.FindDeclaredTSType( baseInterface );
                                if( declared != null )
                                {
                                    b.AppendImportedTypeName( declared );
                                }
                            }
                        }
                    }
                    b.OpenBlock();
                    var props = b.CreateNamedPart( "Props" );
                    bool success = true;
                    foreach( var iP in i.PocoInterface.GetProperties() )
                    {
                        // Is this interface property implemented at the class level?
                        // If not (ExternallyImplemented property) we currently ignore it.
                        IPocoPropertyInfo? p = i.Root.Properties.GetValueOrDefault( iP.Name );
                        if( p != null )
                        {
                            success &= AppendProperty( monitor, tsTypedFile.Context, props, p );
                        }
                    }
                }
            }
            return tsTypedFile;

            static void AddBaseInterface( ITSNamedCodePart b, ref bool hasInterface, TSTypeFile fInterface )
            {
                if( !hasInterface )
                {
                    b.Append( " extends " );
                    hasInterface = true;
                }
                else b.Append( ", " );
                b.AppendImportedTypeName( fInterface );
            }
        }

        bool AppendProperty( IActivityMonitor monitor, TypeScriptContext g, ITSCodePart b, IPocoPropertyInfo p )
        {
            bool success = true;
            b.AppendIdentifier( p.PropertyName ).Append( p.IsEventuallyNullable ? "?: " : ": " );
            bool hasUnions = false;
            foreach( var (t, nullInfo) in p.PropertyUnionTypes )
            {
                if( hasUnions ) b.Append( "|" );
                hasUnions = true;
                success &= b.AppendComplexTypeName( monitor, g, t );
            }
            if( !hasUnions )
            {
                success &= b.AppendComplexTypeName( monitor, g, p.PropertyNullableTypeTree, withUndefined: false );
            }
            b.Append( ";" ).NewLine();
            return success;
        }

    }
}
