using System;
using System.Collections.Generic;
using Poderosa.Communication;
using Poderosa.Connection;
using Poderosa.ConnectionParam;

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

		/// <summary>
		/// Queue of bytes that we have received from the terminal.
		/// </summary>
		protected Queue<byte> _outputQueue = new Queue<byte>();

		/// <summary>
		/// Locking semaphore for writing to <see cref="_outputQueue"/>
		/// </summary>
		protected object _synchronizationObject = new object();

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="parameters">Parameters describing the terminal.</param>
		public StreamConnection(TerminalParam parameters)
			: base(parameters, 80, 25)
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
		/// Description of the protocol for this connection.
		/// </summary>
		public override string ProtocolDescription
		{
			get
			{
				return "Stream-based terminal output";
			}
		}

		/// <summary>
		/// Parameters for this connection.
		/// </summary>
		public override string[] ConnectionParameter
		{
			get
			{
				return new string[]
					       {
						       "Stream connection"
					       };
			}
		}

		/// <summary>
		/// Flag indicating whether or not the connection is available.
		/// </summary>
		public override bool Available
		{
			get
			{
				return false;
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

		/// <summary>
		/// Clones this connection.
		/// </summary>
		/// <returns>Throws a <see cref="NotImplementedException"/>.</returns>
		public override ConnectionTag Reproduce()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Sets up an async read listener on this connection.  Doesn't actually do anything.
		/// </summary>
		/// <param name="cb">Async receiver for the data.</param>
		public override void RepeatAsyncRead(IDataReceiver cb)
		{
		}

		/// <summary>
		/// Writes data to <see cref="_outputQueue"/> as long as <see cref="Capture"/> is set to true.
		/// </summary>
		/// <param name="data">Data that is to be written.</param>
		public override void Write(byte[] data)
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

		/// <summary>
		/// Writes data to <see cref="_outputQueue"/> as long as <see cref="Capture"/> is set to true.
		/// </summary>
		/// <param name="data">Data that is to be written.</param>
		/// <param name="offset">Offset within <see cref="data"/> that we should start writing from.</param>
		/// <param name="length">Number of bytes that should be written.</param>
		public override void Write(byte[] data, int offset, int length)
		{
			if (Capture)
			{
				lock (_synchronizationObject)
				{
					for (int i = offset; i < length; i++)
						_outputQueue.Enqueue(data[i]);
				}
			}
		}
	}
}