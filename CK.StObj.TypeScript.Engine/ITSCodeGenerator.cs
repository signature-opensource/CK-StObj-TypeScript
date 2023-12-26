using CK.Core;
using CK.StObj.TypeScript;
using CK.StObj.TypeScript.Engine;
using CK.TypeScript.CodeGen;
using System;

namespace CK.Setup
{
    /// <summary>
    /// Global Type Script code generator.
    /// This can be used to generate code for a set of types without the need of explicit attributes
    /// or any independent TypeScript code.
    /// <para>
    /// The <see cref="ITSCodeGeneratorType"/> should be used if the TypeScript generation can be driven
    /// by an attribute on a Type.
    /// </para>
    /// <para>
    /// A global code generator like this coexists with other global and other <see cref="ITSCodeGeneratorType"/> on a type.
    /// It is up to the implementation to handle the collaboration (or to raise an error).
    /// </para>
    /// </summary>
    public interface ITSCodeGenerator : ITSCodeGeneratorAutoDiscovery
    {
        /// <summary>
        /// Optional extension point called once all the <see cref="TypeScriptContext.GlobalGenerators"/>
        /// have been discovered.
        /// Typically used to subscribe to <see cref="TSTypeManager.TypeBuilderRequired"/>, <see cref="TypeScriptRoot.BeforeCodeGeneration"/>
        /// or <see cref="TypeScriptRoot.AfterCodeGeneration"/> events.
        /// This can also be used to subscribe to other events that may be raised by other global generators.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="context">The generation context.</param>
        /// <returns>True on success, false on error (errors must be logged).</returns>
        bool Initialize( IActivityMonitor monitor, TypeScriptContext context );

        /// <summary>
        /// Configures the <see cref="TypeBuilderRequiredEventArgs"/> (this is called for each <see cref="TSTypeManager.TypeBuilderRequired"/> event).
        /// If a <see cref="TypeScriptAttribute"/> decorates the type, its properties have been applied to the builder.
        /// <para>
        /// Note that this method may be called after the single call to <see cref="GenerateCode"/> because other generators
        /// can call <see cref="TSTypeManager.ResolveTSType(IActivityMonitor, object)"/>.
        /// In practice this should not be an issue and if it is, it is up to this global code generator to correctly handle
        /// these "after my GenerateCode call" new incoming types.
        /// </para>
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="context">The TypeScript context.</param>
        /// <param name="builder">The builder with the <see cref="TypeBuilderRequiredEventArgs.Type"/> that is handled.</param>
        /// <returns>True on success, false on error (errors must be logged).</returns>
        bool ConfigureBuilder( IActivityMonitor monitor, TypeScriptContext context, TypeBuilderRequiredEventArgs builder );

        /// <summary>
        /// Configures the <see cref="TypeBuilderRequiredEventArgs"/> (this is called for each <see cref="TSTypeManager.TypeBuilderRequired"/> event).
        /// If a <see cref="TypeScriptAttribute"/> decorates the type, its properties have been applied to the builder.
        /// <para>
        /// Note that this method may be called after the single call to <see cref="GenerateCode"/> because other generators
        /// can call <see cref="TSTypeManager.ResolveTSType(IActivityMonitor, object)"/>.
        /// In practice this should not be an issue and if it is, it is up to this global code generator to correctly handle
        /// these "after my GenerateCode call" new incoming types.
        /// </para>
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="context">The TypeScript context.</param>
        /// <param name="e">
        /// The event with the <see cref="TSTypeRequiredEventArgs.KeyType"/> that is handled and
        /// the <see cref="TSTypeRequiredEventArgs.Resolved"/> to set.
        /// </param>
        /// <returns>True on success, false on error (errors must be logged).</returns>
        bool OnResolveObjectKey( IActivityMonitor monitor, TypeScriptContext context, TSTypeRequiredEventArgs e );

        /// <summary>
        /// Generates any TypeScript in the provided context.
        /// This is called once and only once before any type bound methods <see cref="ITSCodeGeneratorType.GenerateCode"/>
        /// are called.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="context">The generation context.</param>
        /// <returns>True on success, false on error (errors must be logged).</returns>
        bool GenerateCode( IActivityMonitor monitor, TypeScriptContext context );

    }
}
