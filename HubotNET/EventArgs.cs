using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HubotNET
{
    public sealed class MessageEventArgs : EventArgs
    {
        public string Message { get; private set; }


        public MessageEventArgs( string message )
        {
            this.Message = message;
        }
    }

    public sealed class TopicEventArgs : EventArgs
    {
        public string Topic { get; private set; }


        public TopicEventArgs( string topic )
        {
            this.Topic = topic;
        }
    }

    public sealed class DisconnectEventArgs : EventArgs
    {
        public bool IsProtocolError { get; private set; }
        public SocketError SocketErrorCode { get; private set; }


        public DisconnectEventArgs()
        {
            this.IsProtocolError = true;
        }
        public DisconnectEventArgs( SocketError sockError )
        {
            this.IsProtocolError = false;
            this.SocketErrorCode = sockError;
        }
    }
}
