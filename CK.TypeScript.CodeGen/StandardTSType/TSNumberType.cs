using System.Globalization;
using System.Numerics;

namespace CK.TypeScript.CodeGen
{
    class TSNumberType : TSType
    {
        public TSNumberType()
            : base( "Number", null, "0" )
        {
        }

        protected override bool DoTryWriteValue( ITSCodeWriter writer, object value )
        {
            switch( value )
            {
                case double v: WriteValue( writer, v.ToString( CultureInfo.InvariantCulture ) ); break;
                case float v: WriteValue( writer, v.ToString( CultureInfo.InvariantCulture ) ); break;
                case int v: WriteValue( writer, v.ToString( CultureInfo.InvariantCulture ) ); break;
                case uint v: WriteValue( writer, v.ToString( CultureInfo.InvariantCulture ) ); break;
                case byte v: WriteValue( writer, v.ToString( CultureInfo.InvariantCulture ) ); break;
                case sbyte v: WriteValue( writer, v.ToString( CultureInfo.InvariantCulture ) ); break;
                case short v: WriteValue( writer, v.ToString( CultureInfo.InvariantCulture ) ); break;
                case ushort v: WriteValue( writer, v.ToString( CultureInfo.InvariantCulture ) ); break;
                default: return false;
            }
            return true;

            static void WriteValue( ITSCodeWriter writer, string num )
            {
                writer.Append( num );
            }
        }

    }
}

