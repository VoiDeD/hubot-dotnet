using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace HubotNET
{
    public sealed class HubotClient
    {
        TcpClient client;

        NetworkStream netStream;

        BinaryReader binReader;
        BinaryWriter binWriter;

        readonly AsyncLock writeLock = new AsyncLock();


        public event EventHandler<DisconnectEventArgs> Disconnected;

        public event EventHandler<MessageEventArgs> Chat;
        public event EventHandler<MessageEventArgs> Emote;
        public event EventHandler<TopicEventArgs> Topic;


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

        public Task SendTopic( string user, string topic )
        {
            return SendPayload( PacketType.Topic, user, topic );
        }


        [SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times" )]
        async Task SendPayload( PacketType type, params string[] data )
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

            using ( await writeLock.LockAsync() )
            {
                // these are synchronous, but the await will hoist this into a continuation
                binWriter.Write( payload.Length );
                binWriter.Write( payload );
            }
        }


        async Task Read()
        {
            while ( true )
            {
                byte[] payload = null;

                try
                {
                    // read packet length off the stream
                    int packetLen = await binReader.ReadInt32Async();
                    // now the payload
                    payload = await binReader.ReadBytesAsync( packetLen );
                }
                catch ( IOException ex )
                {
                    var sockEx = ex.InnerException as SocketException;

                    if ( sockEx != null )
                    {
                        // if we failed due to a socket error, provide the error code
                        Disconnected.Raise( this, new DisconnectEventArgs( sockEx.SocketErrorCode ) );
                    }
                    else
                    {
                        // otherwise, likely some protocol error (bad length, etc)
                        Disconnected.Raise( this, new DisconnectEventArgs() );
                    }

                    break;
                }
                catch ( ObjectDisposedException )
                {
                    // this will get thrown on read attempts following us performing a disconnection.
                    // since we've already fired the disconnected event in this case, we do nothing 
                    break;
                }

                ReadPayload( payload );
            }
        }

        [SuppressMessage( "Microsoft.Usage", "CA2202:Do not dispose objects multiple times" )]
        void ReadPayload( byte[] payload )
        {
            var dispatch = new Dictionary<PacketType, Action<BinaryReader>>
            {
                { PacketType.Chat, br => ReadChat( br, false ) },
                { PacketType.Emote, br => ReadChat( br, true ) },
                { PacketType.Topic, ReadTopic },

                // todo: play, run, close?
            };

            using ( var ms = new MemoryStream( payload ) )
            using ( var br = new BinaryReader( ms ) )
            {
                PacketType type = (PacketType)br.ReadByte();

                Action<BinaryReader> readerFunc;
                if ( !dispatch.TryGetValue( type, out readerFunc ) )
                {
                    Debug.Assert( false, "Received an unknown packet type from the hubot server" );
                    return;
                }

                // dispatch to the associated reader
                readerFunc( br );
            }
        }

        void ReadChat( BinaryReader reader, bool isEmote )
        {
            string message = null;

            try
            {
                message = reader.ReadSafeString();
            }
            catch ( IOException )
            {
                client.Close();
                Disconnected.Raise( this, new DisconnectEventArgs() );

                return;
            }

            if ( isEmote )
            {
                Emote.Raise( this, new MessageEventArgs( message ) );
            }
            else
            {
                Chat.Raise( this, new MessageEventArgs( message ) );
            }
        }

        void ReadTopic( BinaryReader reader )
        {
            string topic = null;

            try
            {
                topic = reader.ReadSafeString();
            }
            catch ( IOException )
            {
                client.Close();
                Disconnected.Raise( this, new DisconnectEventArgs() );

                return;
            }

            Topic.Raise( this, new TopicEventArgs( topic ) );
        }
    }
}
