using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HubotNET
{
    static class BinaryExtensions
    {
        public static async Task<int> ReadInt32Async( this BinaryReader reader )
        {
            var buff = new byte[ 4 ];
            await reader.BaseStream.ReadAsync( buff, 0, buff.Length );

            return BitConverter.ToInt32( buff, 0 );
        }

        public static async Task<byte[]> ReadBytesAsync( this BinaryReader reader, int numBytes )
        {
            // NB! this is really only friendly for small buffers

            var buff = new byte[ numBytes ];
            await reader.BaseStream.ReadAsync( buff, 0, buff.Length );

            return buff;
        }


        public static Task WriteAsync( this BinaryWriter writer, int value )
        {
            byte[] buff = BitConverter.GetBytes( value );
            return writer.WriteAsync( buff );
        }

        public static Task WriteAsync( this BinaryWriter writer, byte[] buffer )
        {
            return writer.BaseStream.WriteAsync( buffer, 0, buffer.Length );
        }


        public static void WriteSafeString( this BinaryWriter writer, string value )
        {
            // our own utf8 string-to-stream serialization:
            // int32 len
            // byte[] data of len length

            byte[] bytes = Encoding.UTF8.GetBytes( value );

            writer.Write( bytes.Length );
            writer.Write( bytes );
        }

        public static string ReadSafeString( this BinaryReader reader )
        {
            int byteCount = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes( byteCount );

            if ( bytes.Length != byteCount )
            {
                // the read buffer didn't countain as many character bytes as expected, likely a protocol error issue somewhere
                throw new IOException( "Mismatched read count in ReadSafeString!" );
            }

            return Encoding.UTF8.GetString( bytes );
        }
    }

    static class EventExtensions
    {
        public static void Raise<T>( this EventHandler<T> handler, object sender, T e )
        {
            if ( handler != null )
                handler( sender, e );
        }
        public static void Raise( this EventHandler handler, object sender, EventArgs e )
        {
            if ( handler != null )
                handler( sender, e );
        }

    }
}
