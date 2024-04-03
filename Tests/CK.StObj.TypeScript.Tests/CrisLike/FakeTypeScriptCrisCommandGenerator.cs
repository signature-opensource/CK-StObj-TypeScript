using CK.Core;
using CK.CrisLike;
using CK.Setup;
using CK.TypeScript.CodeGen;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace CK.StObj.TypeScript.Tests.CrisLike
{
    // This static class is only here to trigger the global FakeTypeScriptCrisCommandGeneratorImpl ITSCodeGenerator.
    // This is the same as the static class TypeScriptCrisCommandGenerator in CK.Cris.TypeScript package.
    [ContextBoundDelegation( "CK.StObj.TypeScript.Tests.CrisLike.FakeTypeScriptCrisCommandGeneratorImpl, CK.StObj.TypeScript.Tests" )]
    public static class FakeTypeScriptCrisCommandGenerator {}

    // Hard coded Cris-like TypeScriptCrisCommandGeneratorImpl.
    public class FakeTypeScriptCrisCommandGeneratorImpl : ITSCodeGenerator
    {
        TSBasicType? _crisPoco;
        TSBasicType? _abstractCommand;
        TSBasicType? _command;
        TypeScriptFile? _modelFile;

        public virtual bool Initialize( IActivityMonitor monitor, ITypeScriptContextInitializer initializer )
        {
            // This can be called IF multiple contexts must be generated:
            // we reset the cached instance here.
            _command = null;

            return initializer.EnsureRegister( monitor, typeof( IAspNetCrisResult ), mustBePocoType: true )
                   && initializer.EnsureRegister( monitor, typeof( IAspNetCrisResultError ), mustBePocoType: true );
        }

        public virtual bool StartCodeGeneration( IActivityMonitor monitor, TypeScriptContext context )
        {
            context.PocoCodeGenerator.PrimaryPocoGenerating += OnPrimaryPocoGenerating;
            context.PocoCodeGenerator.AbstractPocoGenerating += OnAbstractPocoGenerating;
            return true;
        }

        // We don't add anything to the default IPocoType handling.
        public virtual bool OnResolveObjectKey( IActivityMonitor monitor, TypeScriptContext context, RequireTSFromObjectEventArgs e ) => true;

        void OnAbstractPocoGenerating( object? sender, GeneratingAbstractPocoEventArgs e )
        {
            // Filtering out redundant ICommand, ICommand<T>: in TypeScript type name is
            // unique (both are handled by ICommand<TResult = void>).
            // On the TypeScript side, we have always a ICommand<T> where T can be void.

            // By filtering out the base interface it doesn't appear in the base interfaces
            // nor in the branded type. 
            IPocoType? typedResult = null;
            bool hasICommand = false;
            foreach( var i in e.ImplementedInterfaces )
            {
                if( i.GenericTypeDefinition?.Type == typeof( ICommand<> ) )
                {
                    var tResult = i.GenericArguments[0].Type;
                    if( typedResult != null )
                    {
                        e.Monitor.Error( $"{typedResult} and {tResult}" );
                    }
                    typedResult = tResult;
                }
                if( i.Type == typeof( ICommand ) )
                {
                    hasICommand = true;
                }
            }
            if( hasICommand && typedResult != null )
            {
                e.ImplementedInterfaces = e.ImplementedInterfaces.Where( i => i.Type != typeof( ICommand ) );
            }
        }

        void OnPrimaryPocoGenerating( object? sender, GeneratingPrimaryPocoEventArgs e )
        {
            if( e.PrimaryPocoType.AbstractTypes.Any( a => a.Type == typeof(IAbstractCommand) ) )
            {
                e.PocoTypePart.File.Imports.EnsureImport( EnsureCrisCommandModel( e.Monitor, e.TypeScriptContext ), "ICommandModel" );
                e.PocoTypePart.NewLine()
                    .Append( "get commandModel(): ICommandModel { return " ).Append( e.TSGeneratedType.TypeName ).Append( ".#m; }" ).NewLine()
                    .NewLine()
                    .Append( "static #m = " )
                    .OpenBlock()
                        .Append( "applyAmbientValues( command: any, a: any, o: any )" )
                        .OpenBlock()
                        .Append( "/*Apply code comes HERE but FakeTypeScriptCrisCommandGeneratorImpl doesn't handle the ambient values.*/" )
                        .CloseBlock()
                    .CloseBlock();
            }
        }

        public virtual bool OnResolveType( IActivityMonitor monitor,
                                           TypeScriptContext context,
                                           RequireTSFromTypeEventArgs builder )
        {
            var t = builder.Type;
            // Hooks:
            //   - ICommand and ICommand<TResult>: they are both implemented by ICommand<TResult = void> in Model.ts.
            //   - IAbstractCommand and ICrisPoco.
            // 
            // Model.ts also implements ICommandModel, ExecutedCommand<T>, and CrisError.
            //
            if( t.Namespace == "CK.CrisLike" )
            {
                if( t.Name == "ICommand" || (t.IsGenericTypeDefinition && t.Name == "ICommand`1") )
                {
                    EnsureCrisCommandModel( monitor, context );
                    builder.ResolvedType = _command;
                }
                else if( t.Name == "IAbstractCommand" )
                {
                    EnsureCrisCommandModel( monitor, context );
                    builder.ResolvedType = _abstractCommand;
                }
                else if( t.Name == "ICrisPoco" )
                {
                    EnsureCrisCommandModel( monitor, context );
                    builder.ResolvedType = _crisPoco;
                }
            }
            return true;
        }

        [MemberNotNull(nameof(_command), nameof( _abstractCommand ), nameof( _crisPoco ) )]
        TypeScriptFile EnsureCrisCommandModel( IActivityMonitor monitor, TypeScriptContext context )
        {
            if( _modelFile == null )
            {
                _modelFile = context.Root.Root.FindOrCreateFile( "CK/Cris/Model.ts" );
                GenerateCrisModelFile( monitor, context, _modelFile );
                //GenerateCrisEndpoint( monitor, modelFile.Folder.FindOrCreateFile( "CrisEndpoint.ts" ) );
                //GenerateCrisHttpEndpoint( monitor, modelFile.Folder.FindOrCreateFile( "HttpCrisEndpoint.ts" ) );
                _crisPoco = new TSBasicType( context.Root.TSTypes, "ICrisPoco", imports => imports.EnsureImport( _modelFile, "ICrisPoco" ), null );
                _abstractCommand = new TSBasicType( context.Root.TSTypes, "IAbstractCommand", imports => imports.EnsureImport( _modelFile, "IAbstractCommand" ), null );
                _command = new TSBasicType( context.Root.TSTypes, "ICommand", imports => imports.EnsureImport( _modelFile, "ICommand" ), null );
            }
            Throw.DebugAssert( _command != null && _abstractCommand != null && _crisPoco != null );
            return _modelFile;

            static void GenerateCrisModelFile( IActivityMonitor monitor, TypeScriptContext context, TypeScriptFile fModel )
            {
                fModel.Imports.EnsureImport( monitor, typeof( SimpleUserMessage ) );
                fModel.Imports.EnsureImport( monitor, typeof( UserMessageLevel ) );
                var pocoType = context.Root.TSTypes.ResolveTSType( monitor, typeof(IPoco) );
                // Imports the IPoco itself...
                pocoType.EnsureRequiredImports( fModel.Imports );

                fModel.Body.Append( """
                                /**
                                 * Describes a Command type. 
                                 **/
                                export interface ICommandModel {
                                    /**
                                     * This supports the CrisEndpoint implementation. This is not to be used directly.
                                     **/
                                    readonly applyAmbientValues: (command: any, a: any, o: any ) => void;
                                }

                                /** 
                                 * Abstraction of any Cris objects (currently only commands).
                                 **/
                                export interface ICrisPoco extends IPoco
                                {
                                    readonly _brand: IPoco["_brand"] & {"ICrisPoco": any};
                                }

                                /** 
                                 * Command abstraction.
                                 **/
                                export interface IAbstractCommand extends ICrisPoco
                                {
                                    /** 
                                     * Gets the command model.
                                     **/
                                    get commandModel(): ICommandModel;

                                    readonly _brand: ICrisPoco["_brand"] & {"ICommand": any};
                                }

                                /** 
                                 * Command with or without a result.
                                 * The C# ICommand (without result) is the TypeScript ICommand<void>.
                                 **/
                                export interface ICommand<out TResult = void> extends IAbstractCommand {
                                    readonly _brand: IAbstractCommand["_brand"] & {"ICommandResult": void extends TResult ? any : TResult};
                                }
                                                                
                                /** 
                                 * Captures the result of a command execution.
                                 **/
                                export type ExecutedCommand<T> = {
                                    /** The executed command. **/
                                    readonly command: ICommand<T>,
                                    /** The execution result. **/
                                    readonly result: CrisError | T,
                                    /** Optional correlation identifier. **/
                                    readonly correlationId?: string
                                };

                                /**
                                 * Captures communication, validation or execution error.
                                 **/
                                export class CrisError extends Error {
                                   /**
                                    * Get this error type.
                                    */
                                    public readonly errorType : "CommunicationError"|"ValidationError"|"ExecutionError";
                                    /**
                                     * Gets the messages. At least one message is guaranteed to exist.
                                     */
                                    public readonly messages: ReadonlyArray<SimpleUserMessage>; 
                                    /**
                                     * The Error.cause support is a mess. This replaces it at this level. 
                                     */
                                    public readonly innerError?: Error; 
                                    /**
                                     * When defined, enables to find the backend log entry.
                                     */
                                    public readonly logKey?: string; 
                                    /**
                                     * Gets the command that failed.
                                     */
                                    public readonly command: ICommand<unknown>;

                                    constructor( command: ICommand<unknown>, 
                                                 message: string, 
                                                 isValidationError: boolean,
                                                 innerError?: Error, 
                                                 messages?: ReadonlyArray<SimpleUserMessage>,
                                                 logKey?: string ) 
                                    {
                                        super( message );
                                        this.command = command;   
                                        this.errorType = isValidationError 
                                                            ? "ValidationError" 
                                                            : innerError ? "CommunicationError" : "ExecutionError";
                                        this.innerError = innerError;
                                        this.messages = messages && messages.length > 0 
                                                        ? messages
                                                        : [new SimpleUserMessage(UserMessageLevel.Error,message,0)];
                                        this.logKey = logKey;
                                    }
                                }
                                
                                """ );
            }
        }
    }
}
