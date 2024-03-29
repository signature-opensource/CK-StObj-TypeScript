
using CK.Core;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace CK.TypeScript.CodeGen
{
    /// <summary>
    /// Types adapter: this folder exposes a typed <see cref="Root"/> and
    /// works with similar typed folders <see cref="TypeScriptFile{TRoot}"/>.
    /// </summary>
    /// <typeparam name="TRoot">The actual type of the root.</typeparam>
    public sealed class TypeScriptFolder<TRoot> : TypeScriptFolder
        where TRoot : TypeScriptRoot
    {
        TypeScriptFolder( TRoot root )
            : base( root )
        {
        }

        static internal TypeScriptFolder Create( TRoot root ) => new TypeScriptFolder<TRoot>( root );

        internal TypeScriptFolder( TypeScriptFolder<TRoot> parent, string name )
            : base( parent, name )
        {
        }

        /// <inheritdoc cref="TypeScriptFolder.Root" />
        public new TRoot Root => Unsafe.As<TRoot>( base.Root );

        /// <inheritdoc cref="TypeScriptFolder.FindOrCreateFolder(NormalizedPath)" />
        public new TypeScriptFolder<TRoot> FindOrCreateFolder( NormalizedPath path ) => Unsafe.As<TypeScriptFolder<TRoot>>( base.FindOrCreateFolder( path ) );

        private protected override TypeScriptFolder CreateLocalFolder( string name )
        {
            var f = new TypeScriptFolder<TRoot>( this, name );
            Root.OnFolderCreated( f );
            return f;
        }

        /// <inheritdoc cref="TypeScriptFolder.FindFolder(NormalizedPath)" />
        public new TypeScriptFolder<TRoot>? FindFolder( NormalizedPath subPath ) => Unsafe.As<TypeScriptFolder<TRoot>?>( base.FindFolder( subPath ) );

        /// <inheritdoc cref="TypeScriptFolder.Folders" />
        public new IEnumerable<TypeScriptFolder<TRoot>> Folders => base.Folders.Cast<TypeScriptFolder<TRoot>>();

        private protected override TypeScriptFile CreateLocalFile( string name )
        {
            var f = new TypeScriptFile<TRoot>( this, name );
            Root.OnFileCreated( f );
            return f;
        }

        /// <inheritdoc cref="TypeScriptFolder.FindOrCreateFile(NormalizedPath)" />
        public new TypeScriptFile<TRoot> FindOrCreateFile( NormalizedPath path ) => Unsafe.As<TypeScriptFile<TRoot>>( base.FindOrCreateFile( path ) );

        /// <inheritdoc cref="TypeScriptFolder.FindOrCreateFile(NormalizedPath, out bool)" />
        public new TypeScriptFile<TRoot> FindOrCreateFile( NormalizedPath path, out bool created ) => Unsafe.As<TypeScriptFile<TRoot>>( base.FindOrCreateFile( path, out created ) );

        /// <inheritdoc cref="TypeScriptFolder.FindFile(NormalizedPath)" />
        public new TypeScriptFile<TRoot>? FindFile( NormalizedPath path ) => Unsafe.As<TypeScriptFile<TRoot>?>( base.FindFile( path ) );

        /// <inheritdoc cref="TypeScriptFolder.Files" />
        public new IEnumerable<TypeScriptFile<TRoot>> Files => base.Files.Cast<TypeScriptFile<TRoot>>();

        /// <inheritdoc cref="TypeScriptFolder.AllFilesRecursive" />
        public new IEnumerable<TypeScriptFile<TRoot>> AllFilesRecursive => Files.Concat( Folders.SelectMany( s => s.AllFilesRecursive ) );

    }

}
