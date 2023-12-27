using System;
using System.Collections.Generic;
using System.Text;

namespace CK.TypeScript.CodeGen
{
    class BaseCodeWriter : ITSCodeWriter
    {
        /// <summary>
        /// Heterogeneous list of BaseCodeWriter and string.
        /// </summary>
        internal readonly List<object> _content;
        Dictionary<object, object?>? _memory;

        public BaseCodeWriter( TypeScriptFile f )
        {
            File = f;
            _content = new List<object>();
        }

        public TypeScriptFile File { get; }

        public void DoAdd( string? code )
        {
            if( !String.IsNullOrEmpty( code ) ) _content.Add( code );
        }

        internal virtual void Clear()
        {
            _memory?.Clear();
            _content.Clear();
        }

        internal virtual SmarterStringBuilder Build( SmarterStringBuilder b )
        {
            foreach( var c in _content )
            {
                if( c is BaseCodeWriter p ) p.Build( b );
                else b.Append( (string)c );
            }
            return b;
        }

        public StringBuilder Build( StringBuilder b, bool closeScope ) => Build( new SmarterStringBuilder( b ) ).Builder!;

        public IDictionary<object, object?> Memory => _memory ??= new Dictionary<object, object?>();

        public override string ToString() => Build( new SmarterStringBuilder( new StringBuilder() ) ).ToString();
    }
}
