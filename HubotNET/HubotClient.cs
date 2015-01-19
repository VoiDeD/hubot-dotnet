using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HubotNET
{
    public sealed class HubotClient
    {
        TcpClient client;

        NetworkStream netStream;

        BinaryReader binReader;
        BinaryWriter binWriter;


        public async Task Connect( string host, int port )
        {
            // TcpClient instances can't be re-used, so we create a new one for every connect attempt
            client = new TcpClient();

            await client.ConnectAsync( host, port );

            netStream = client.GetStream();

            binReader = new BinaryReader( netStream );
            binWriter = new BinaryWriter( netStream );

            // begin read loop
            // variable assignment avoids CS4014 which we don't particularly care about right here
            var fireAndForget = Read();
        }


        public Task SendChat( string user, string message )
        {
            return SendPayload( PacketType.Chat, user, message );
        }

        public Task SendEnter( string user )
        {
            return SendPayload( PacketType.Enter, user );
        }
        public Task SendLeave( string user )
        {
            return SendPayload( PacketType.Leave, user );
        }


        [SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times" )]
        Task SendPayload( PacketType type, params string[] data )
        {
            byte[] payload;

            using ( var ms = new MemoryStream() )
            using ( var bw = new BinaryWriter( ms ) )
            {
                // serialize our payload into memory
                // we're only working with (usually) small buffers so this isn't a huge hit on the GC to always do this
                // and we have the benefit of knowing our buffer size without any calculations

                bw.Write( (byte)type );

                foreach ( string param in data )
                {
                    // we use our own encoding of strings
                    bw.WriteSafeString( param );
                }

                payload = ms.ToArray();
            }

            // now send the payload over the wire

            // todo: currently this is done serially to avoid any races
            // this should eventually be changed to an async write
            // but for now we're relying on the tcp stack to not block on a full send buffer

            binWriter.Write( payload.Length );
            binWriter.Write( payload );

            return Task.FromResult( true );
        }


        async Task Read()
        {
            while ( true )
            {
                // read packet length off the stream
                int packetLen = await binReader.ReadInt32Async();
                byte[] payload = await binReader.ReadBytesAsync( packetLen );

                // todo: process the payload
            }
        }
    }

    enum PacketType
    {
        Invalid = 0,

        Chat,
        Emote,

        Enter,
        Leave,

        Topic
    }
}
