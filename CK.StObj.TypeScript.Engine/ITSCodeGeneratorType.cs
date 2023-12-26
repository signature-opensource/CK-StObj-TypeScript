using CK.Core;
using CK.StObj.TypeScript;
using CK.StObj.TypeScript.Engine;
using CK.TypeScript.CodeGen;

namespace CK.Setup
{
    /// <summary>
    /// Type Script code generator for a type. This interface is typically implemented by delegated attribute classes
    /// (that could specialize <see cref="TypeScriptAttributeImpl"/>).
    /// <para>
    /// Note that nothing forbids more than one ITSCodeGeneratorType to exist on a type: if it happens, it is up
    /// to the multiple implementations to synchronize their works on the final file along with any global <see cref="ITSCodeGenerator"/>
    /// work.
    /// </para>
    /// </summary>
    public interface ITSCodeGeneratorType : ITSCodeGeneratorAutoDiscovery
    {
        /// <summary>
        /// Configures the <see cref="TypeBuilderRequiredEventArgs"/>.
        /// If a <see cref="TypeScriptAttribute"/> decorates the type, its properties have been applied to the builder.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="context">The global TypeScript context.</param>
        /// <param name="builder">The <see cref="ITSGeneratedType"/> builder to configure.</param>
        /// <returns>True on success, false on error (errors must be logged).</returns>
        bool ConfigureBuilder( IActivityMonitor monitor, TypeScriptContext context, TypeBuilderRequiredEventArgs builder );

        /// <summary>
        /// Generates TypeScript code. The <paramref name="tsType"/> gives access to its file.
        /// When <see cref="TypeBuilderRequiredEventArgs.Implementor"/> has been used by <see cref="ConfigureBuilder(IActivityMonitor, TypeScriptContext, TypeBuilderRequiredEventArgs)"/>
        /// this can perfectly be a no-op and simply return true.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="context">The global TypeScript context.</param>
        /// <param name="tsType">The type that must be generated (<see cref="ITSGeneratedType.EnsureTypePart(string, bool)"/> can be called).</param>
        /// <returns>True on success, false on error (errors must be logged).</returns>
        bool GenerateCode( IActivityMonitor monitor, TypeScriptContext context, ITSGeneratedType tsType );

    }
}
