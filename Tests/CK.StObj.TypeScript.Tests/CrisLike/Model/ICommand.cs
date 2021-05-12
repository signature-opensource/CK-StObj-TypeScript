using CK.Core;
using System;

namespace CK.StObj.TypeScript.Tests.CrisLike
{
    /// <summary>
    /// The base command interface marker is a simple <see cref="IPoco"/>.
    /// Any type that extends this interface defines a new command type.
    /// </summary>
    [CKTypeDefiner]
    public interface ICommand : IPoco
    {
        /// <summary>
        /// Gets the <see cref="ICommandModel"/> that describes this command.
        /// This property is automatically implemented. 
        /// </summary>
        [AutoImplementationClaim]
        ICommandModel CommandModel { get; }
    }
}
