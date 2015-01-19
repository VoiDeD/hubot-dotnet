﻿using System;
using System.Collections.Generic;
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


        public event EventHandler<MessageEventArgs> OnChat;
        public event EventHandler<MessageEventArgs> OnEmote;
        public event EventHandler<TopicEventArgs> OnTopic;


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
                binWriter.Write( payload.Length );
                binWriter.Write( payload );
            }
        }


        async Task Read()
        {
            while ( true )
            {
                // read packet length off the stream
                int packetLen = await binReader.ReadInt32Async();
                byte[] payload = await binReader.ReadBytesAsync( packetLen );

                ReadPayload( payload );
            }
        }

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
                    // todo: unknown message type, handle this?
                    return;
                }

                // dispatch to the associated reader
                readerFunc( br );
            }
        }

        void ReadChat( BinaryReader reader, bool isEmote )
        {
            string message = reader.ReadSafeString();

            if ( isEmote )
            {
                OnEmote.Raise( this, new MessageEventArgs( message ) );
            }
            else
            {
                OnChat.Raise( this, new MessageEventArgs( message ) );
            }
        }

        void ReadTopic( BinaryReader reader )
        {
            string topic = reader.ReadSafeString();

            OnTopic.Raise( this, new TopicEventArgs( topic ) );
        }
    }
}
