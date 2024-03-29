using CK.Core;
using CK.StObj.TypeScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CK.Setup
{
    /// <summary>
    /// Configures TypeScript generation.
    /// <para>
    /// Each <see cref="BinPathConfiguration"/> that requires TypeScript code to be generated must
    /// contain a &lt;TypeScript&gt; with the attribute &lt;OutputPath&gt;.
    /// These OutputPaths can be absolute or start with a {BasePath}, {OutputPath} or {ProjectPath} prefix: the
    /// final paths will be resolved.
    /// </para>
    /// <para>
    /// The &lt;TypeScript&gt; element can contain a &lt;Barrels&gt; element with &lt;Barrel Path="sub/folder"/&gt; elements:
    /// that will generate index.ts files in the specified folders (see https://basarat.gitbook.io/typescript/main-1/barrel).
    /// Use &lt;Barrel Path=""/&gt; to create a barrel at the root level.
    /// </para>
    /// See <see cref="TypeScriptAspectBinPathConfiguration"/> that models this required BinPathConfiguration.
    /// </summary>
    public sealed class TypeScriptAspectConfiguration : IStObjEngineAspectConfiguration
    {
        /// <summary>
        /// The <see cref="PascalCase"/> attribute name.
        /// </summary>
        public static readonly XName xPascalCase = XNamespace.None + "PascalCase";

        /// <summary>
        /// The <see cref="GenerateDocumentation"/> attribute name.
        /// </summary>
        public static readonly XName xGenerateDocumentation = XNamespace.None + "GenerateDocumentation";

        /// <summary>
        /// The element name of <see cref="TypeScriptAspectConfiguration.LibraryVersions"/>.
        /// </summary>
        public static readonly XName xLibraryVersions = XNamespace.None + "LibraryVersions";

        /// <summary>
        /// The element name of <see cref="TypeScriptAspectConfiguration.LibraryVersions"/> child elements.
        /// </summary>
        public static readonly XName xLibrary = XNamespace.None + "Library";

        /// <summary>
        /// The <see cref="TypeScriptAspectBinPathConfiguration.SkipTypeScriptTooling"/> attribute name.
        /// </summary>
        public static readonly XName xSkipTypeScriptTooling = XNamespace.None + "SkipTypeScriptTooling";

        /// <summary>
        /// The <see cref="TypeScriptAspectBinPathConfiguration.EnsureTestSupport"/> attribute name.
        /// </summary>
        public static readonly XName xEnsureTestSupport = XNamespace.None + "EnsureTestSupport";

        /// <summary>
        /// The <see cref="TypeScriptAspectBinPathConfiguration.AutomaticTypeScriptVersion"/> attribute name.
        /// </summary>
        public static readonly XName xAutomaticTypeScriptVersion = XNamespace.None + "AutomaticTypeScriptVersion";

        /// <summary>
        /// The <see cref="TypeScriptAspectBinPathConfiguration.AutoInstallYarn"/> attribute name.
        /// </summary>
        public static readonly XName xAutoInstallYarn = XNamespace.None + "AutoInstallYarn";

        /// <summary>
        /// The <see cref="TypeScriptAspectBinPathConfiguration.GitIgnoreCKGenFolder"/> attribute name.
        /// </summary>
        public static readonly XName xGitIgnoreCKGenFolder = XNamespace.None + "GitIgnoreCKGenFolder";

        /// <summary>
        /// The <see cref="TypeScriptAspectBinPathConfiguration.AutoInstallVSCodeSupport"/> attribute name.
        /// </summary>
        public static readonly XName xAutoInstallVSCodeSupport = XNamespace.None + "AutoInstallVSCodeSupport";

        /// <summary>
        /// The <see cref="TypeScriptAspectBinPathConfiguration"/> element name.
        /// </summary>
        public static readonly XName xTypeScript = XNamespace.None + "TypeScript";

        /// <summary>
        /// The attribute name of <see cref="TypeScriptAspectBinPathConfiguration.TargetProjectPath"/>.
        /// </summary>
        public static readonly XName xTargetProjectPath = XNamespace.None + "TargetProjectPath";

        /// <summary>
        /// The attribute name of <see cref="TypeScriptTypeConfiguration.TypeName"/>.
        /// </summary>
        public static readonly XName xTypeName = XNamespace.None + "TypeName";

        /// <summary>
        /// The attribute name of <see cref="TypeScriptTypeConfiguration.Folder"/>.
        /// </summary>
        public static readonly XName xFolder = XNamespace.None + "Folder";

        /// <summary>
        /// The attribute name of <see cref="TypeScriptTypeConfiguration.FileName"/>.
        /// </summary>
        public static readonly XName xFileName = XNamespace.None + "FileName";

        /// <summary>
        /// The attribute name of <see cref="TypeScriptTypeConfiguration.SameFileAs"/>.
        /// </summary>
        public static readonly XName xSameFileAs = XNamespace.None + "SameFileAs";

        /// <summary>
        /// The attribute name of <see cref="TypeScriptTypeConfiguration.SameFolderAs"/>.
        /// </summary>
        public static readonly XName xSameFolderAs = XNamespace.None + "SameFolderAs";

        /// <summary>
        /// The <see cref="TypeScriptAspectBinPathConfiguration.Barrels"/> element name.
        /// </summary>
        public static readonly XName xBarrels = XNamespace.None + "Barrels";

        /// <summary>
        /// The child element name of <see cref="TypeScriptAspectBinPathConfiguration.Barrels"/>.
        /// </summary>
        public static readonly XName xBarrel = XNamespace.None + "Barrel";

        /// <summary>
        /// Initializes a new default configuration.
        /// </summary>
        public TypeScriptAspectConfiguration()
        {
            GenerateDocumentation = true;
            LibraryVersions = new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new configuration from a Xml element.
        /// </summary>
        /// <param name="e">The configuration element.</param>
        public TypeScriptAspectConfiguration( XElement e )
        {
            PascalCase = (bool?)e.Element( xPascalCase ) ?? false;
            GenerateDocumentation = (bool?)e.Attribute( xGenerateDocumentation ) ?? true;
            LibraryVersions = e.Element( xLibraryVersions )?
                    .Elements( xLibrary )
                    .Select( e => (e.Attribute( StObjEngineConfiguration.xName )?.Value, e.Attribute( StObjEngineConfiguration.xVersion )?.Value) )
                    .Where( e => !string.IsNullOrWhiteSpace( e.Item1 ) && !string.IsNullOrWhiteSpace( e.Item2 ) )
                    .GroupBy( e => e.Item1 )
                    .ToDictionary( g => g.Key!, g => g.Last().Item2! )
                  ?? new Dictionary<string, string>();

        }

        /// <summary>
        /// Gets a dictionary that defines the version to use for an external package.
        /// The versions specified here override the ones specified in code while
        /// declaring an import.
        /// <para>
        /// Example:
        /// <code>
        ///     &lt;LibraryVersions&gt;
        ///        &lt;Library Name="axios" Version="^1.2.1" /&gt;
        ///        &lt;Library Name="luxon" Version="3.1.1" /&gt;
        ///     &lt;/LibraryVersions&gt;
        /// </code>
        /// </para>
        /// <para>
        /// It is empty by default.
        /// </para>
        /// </summary>
        public Dictionary<string, string> LibraryVersions { get; }

        /// <summary>
        /// Fills the given Xml element with this configuration values.
        /// </summary>
        /// <param name="e">The element to fill.</param>
        /// <returns>The element.</returns>
        public XElement SerializeXml( XElement e )
        {
            e.Add( new XAttribute( StObjEngineConfiguration.xVersion, "1" ),
                        PascalCase == false
                            ? new XAttribute( xPascalCase, false )
                            : null,
                        GenerateDocumentation == false
                            ? new XAttribute( xGenerateDocumentation, false )
                            : null,
                        LibraryVersions.Count > 0
                            ? new XElement( xLibraryVersions,
                                            LibraryVersions.Select( kv => new XElement( xLibrary,
                                                new XAttribute( StObjEngineConfiguration.xName, kv.Key ),
                                                new XAttribute( StObjEngineConfiguration.xVersion, kv.Value ) ) ) )
                            : null
                 );
            return e;
        }

        /// <summary>
        /// Gets or sets whether TypeScript generated properties should be PascalCased.
        /// Defaults to false (identifiers are camelCased).
        /// </summary>
        public bool PascalCase { get; set; }

        /// <summary>
        /// Gets or sets whether documentation should be generated.
        /// Defaults to true.
        /// </summary>
        public bool GenerateDocumentation { get; set; }

        /// <summary>
        /// Gets the "CK.Setup.TypeScriptAspect, CK.StObj.TypeScript.Engine" assembly qualified name.
        /// </summary>
        public string AspectType => "CK.Setup.TypeScriptAspect, CK.StObj.TypeScript.Engine";

    }

}
