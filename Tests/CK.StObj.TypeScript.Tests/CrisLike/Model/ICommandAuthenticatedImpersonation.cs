using CK.Core;
using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved

namespace CK.StObj.TypeScript.Tests.CrisLike
{
    /// <summary>
    /// Extends the basic <see cref="ICommandAuthenticated"/> to add the <see cref="DeviceId"/> field.
    /// </summary>
    [CKTypeDefiner]
    public interface ICommandAuthenticatedImpersonation : ICommandAuthenticated
    {
        /// <summary>
        /// Gets or sets the actual actor identifier: the one that is connected, regardless of any impersonation.
        /// The default <see cref="CrisAuthenticationService"/> validates this field against the current <see cref="IAuthenticationInfo.ActualUser"/>.
        /// </summary>
        int ActualActorId { get; set; }
    }
}
