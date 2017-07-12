using System;
using System.Collections.Generic;
using Poderosa.Protocols;

namespace EasyConnect.Protocols.PowerShell
{
    /// <summary>
    /// Non-network based connection class that enables a terminal and a client to interact via a <see cref="Queue{T}"/> instead of over a socket.
    /// </summary>
    public class StreamConnection : TerminalConnection
    {
        /// <summary>
        /// Flag indicating whether we should capture data (i.e. place it in <see cref="_outputQueue"/>.
        /// </summary>
        protected bool _capture = false;

        protected StreamSocket _streamSocket = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StreamConnection(ITerminalParameter parameters) : base(parameters)
        {
            _streamSocket = new StreamSocket();
            _socket = _streamSocket;
            _terminalOutput = _streamSocket;

            _streamSocket.Capture = true;
        }

        /// <summary>
        /// Flag indicating whether we should capture data (i.e. place it in <see cref="_outputQueue"/>.
        /// </summary>
        public bool Capture
        {
            get
            {
                return _streamSocket.Capture;
            }

            set
            {
                _streamSocket.Capture = value;
            }
        }

        /// <summary>
        /// Queue of bytes that we have received from the terminal.
        /// </summary>
        public Queue<byte> OutputQueue
        {
            get
            {
                return _streamSocket.OutputQueue;
            }
        }

        public void Receive(byte[] data, int offset, int length)
        {
            _streamSocket.Receive(data, offset, length);
        }

        public void Transmit(byte[] data, int offset, int length)
        {
            _streamSocket.Transmit(data, offset, length);
        }
    }
}