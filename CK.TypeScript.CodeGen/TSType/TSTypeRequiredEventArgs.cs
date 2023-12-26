﻿using CK.Core;

namespace CK.TypeScript.CodeGen
{
    /// <summary>
    /// Event arguments that exposes an object for which a <see cref="ITSType"/> must be resolved.
    /// This is raised when a key type that is not a C# type must be resolved.
    /// </summary>
    public sealed class TSTypeRequiredEventArgs : EventMonitoredArgs
    {
        readonly object _keyType;
        ITSType? _resolved;

        internal TSTypeRequiredEventArgs( IActivityMonitor monitor, object keyType )
            : base( monitor )
        {
            _keyType = keyType;
        }

        /// <summary>
        /// Gets the key type to resolve.
        /// </summary>
        public object KeyType => _keyType;

        /// <summary>
        /// Gets the TypeScript type to used.
        /// </summary>
        public ITSType? Resolved
        {
            get => _resolved;
            set => _resolved = value;
        }
    }

}

