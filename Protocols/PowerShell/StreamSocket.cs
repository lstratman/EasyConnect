using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Poderosa.Protocols;

namespace EasyConnect.Protocols.PowerShell
{
    public class StreamSocket : IPoderosaSocket, ITerminalOutput
    {
        /// <summary>
        /// Flag indicating whether we should capture data (i.e. place it in <see cref="_outputQueue"/>.
        /// </summary>
        protected bool _capture = false;

        /// <summary>
        /// Queue of bytes that we have received from the terminal.
        /// </summary>
        protected Queue<byte> _outputQueue = new Queue<byte>();

        /// <summary>
        /// Locking semaphore for writing to <see cref="_outputQueue"/>
        /// </summary>
        protected object _synchronizationObject = new object();

        protected IByteAsyncInputStream _receiver = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public StreamSocket()
        {
            Capture = true;
        }

        /// <summary>
        /// Flag indicating whether we should capture data (i.e. place it in <see cref="_outputQueue"/>.
        /// </summary>
        public bool Capture
        {
            get
            {
                return _capture;
            }

            set
            {
                if (value && !Capture)
                {
                    lock (_synchronizationObject)
                    {
                        _outputQueue.Clear();
                    }
                }

                _capture = value;
            }
        }

        /// <summary>
        /// Queue of bytes that we have received from the terminal.
        /// </summary>
        public Queue<byte> OutputQueue
        {
            get
            {
                return _outputQueue;
            }
        }

        public bool Available
        {
            get
            {
                return false;
            }
        }

        public void Close()
        {
        }

        public void ForceDisposed()
        {
        }

        public void RepeatAsyncRead(IByteAsyncInputStream receiver)
        {
            _receiver = receiver;
        }

        public void Transmit(ByteDataFragment data)
        {
            Transmit(data.Buffer, data.Offset, data.Length);
        }

        public void Transmit(byte[] data, int offset, int length)
        {
            if (Capture)
            {
                lock (_synchronizationObject)
                {
                    foreach (byte datum in data)
                        _outputQueue.Enqueue(datum);
                }
            }
        }

        public void Receive(byte[] data, int offset, int length)
        {
            if (_receiver != null)
            {
                _receiver.OnReception(new ByteDataFragment(data, offset, length));
            }
        }

        public void SendBreak()
        {
        }

        public void SendKeepAliveData()
        {
        }

        public void AreYouThere()
        {
        }

        public void Resize(int width, int height)
        {
        }
    }
}
