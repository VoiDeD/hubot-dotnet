using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HubotNET
{
    /// <summary>
    /// Event data for message based events.
    /// </summary>
    public sealed class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the message associated with the event.
        /// Could be a chat or emote message, depending on the event source.
        /// </summary>
        public string Message { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEventArgs"/> class.
        /// </summary>
        /// <param name="message">The message associated with the event.</param>
        public MessageEventArgs( string message )
        {
            this.Message = message;
        }
    }

    /// <summary>
    /// Event data for topic events.
    /// </summary>
    public sealed class TopicEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the topic string associated with the event.
        /// </summary>
        public string Topic { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="TopicEventArgs"/> class.
        /// </summary>
        /// <param name="topic">The topic string associated with the event.</param>
        public TopicEventArgs( string topic )
        {
            this.Topic = topic;
        }
    }

    /// <summary>
    /// Event data for network disconnection events.
    /// </summary>
    public sealed class DisconnectEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether this event is due to an underlying protocol error.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is due to a protocol error; otherwise, <c>false</c>.
        /// </value>
        public bool IsProtocolError { get; private set; }
        /// <summary>
        /// Gets the socket error code associated with the event.
        /// This value is only correctly populated when the disconnection was not due to a protocol error.
        /// </summary>
        public SocketError SocketErrorCode { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectEventArgs"/> class that is the result of a protocol error.
        /// </summary>
        public DisconnectEventArgs()
        {
            this.IsProtocolError = true;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectEventArgs"/> class that is the result of a network socket error.
        /// </summary>
        /// <param name="sockError">The socket error associated with the event.</param>
        public DisconnectEventArgs( SocketError sockError )
        {
            this.IsProtocolError = false;
            this.SocketErrorCode = sockError;
        }
    }
}
