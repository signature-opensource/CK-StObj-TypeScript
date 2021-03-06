using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace CK.Setup
{
    /// <summary>
    /// Configures TypeScript generation.
    /// Each <see cref="BinPathConfiguration"/> that requires TypeScript code to be generated must
    /// contain a &lt;TypeScript&gt; element with an &lt;OutputPath&gt; child element.
    /// This OutputPath can be absolute or start with a {BasePath}, {OutputPath} or {ProjectPath} prefix: the
    /// final path will be resolved.
    /// </summary>
    public class TypeScriptAspectConfiguration : IStObjEngineAspectConfiguration
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
        /// Initializes a new default configuration.
        /// </summary>
        public TypeScriptAspectConfiguration()
        {
            GenerateDocumentation = true;
        }

        /// <summary>
        /// Initializes a new configuration from a Xml element.
        /// </summary>
        /// <param name="e"></param>
        public TypeScriptAspectConfiguration( XElement e )
        {
            PascalCase = (bool?)e.Element( xPascalCase ) ?? false;
            GenerateDocumentation = (bool?)e.Element( xGenerateDocumentation ) ?? true;
        }

        /// <summary>
        /// Fills the given Xml element with this configuration values.
        /// </summary>
        /// <param name="e">The element to fill.</param>
        /// <returns>The element.</returns>
        public XElement SerializeXml( XElement e )
        {
            e.Add( new XAttribute( StObjEngineConfiguration.xVersion, "1" ),
                        new XElement( xPascalCase, PascalCase ),
                        GenerateDocumentation == false
                            ? new XAttribute( xGenerateDocumentation, GenerateDocumentation )
                            : null );
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
