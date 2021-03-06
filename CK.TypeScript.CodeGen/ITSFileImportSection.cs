using System;
using System.Collections.Generic;
using System.Text;

namespace CK.TypeScript.CodeGen
{
    /// <summary>
    /// The import section of a <see cref="TypeScriptFile"/> is a <see cref="ITSCodeWriter"/>
    /// that generates a first part with imports declarations from <see cref="EnsureImport(TypeScriptFile, string, string[])"/>.
    /// </summary>
    public interface ITSFileImportSection : ITSCodeWriter
    {
        /// <summary>
        /// Ensures that an import of one or more type names from the corresponding file exists.
        /// </summary>
        /// <param name="file">The referenced file.</param>
        /// <param name="typeName">The first type name to import.</param>
        /// <param name="typeNames">More types to import.</param>
        /// <returns>This section to enable fluent syntax.</returns>
        ITSFileImportSection EnsureImport( TypeScriptFile file, string typeName, params string[] typeNames );

        /// <summary>
        /// Gets the number of different <see cref="EnsureImport(TypeScriptFile, string, string[])"/>
        /// that have been done.
        /// </summary>
        int ImportCount { get; }

        /// <summary>
        /// Gets the current import code section.
        /// </summary>
        /// <returns>The import section. Can be empty.</returns>
        string ToString();
    }
}
