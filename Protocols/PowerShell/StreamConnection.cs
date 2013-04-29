using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Poderosa.Communication;
using Poderosa.Connection;
using Poderosa.ConnectionParam;

namespace EasyConnect.Protocols.PowerShell
{
	public class StreamConnection : TerminalConnection
	{
		private Queue<byte> _outputQueue = new Queue<byte>();
		private object _synchronizationObject = new object();
		protected bool _capture = false;

		public StreamConnection(TerminalParam param)
			: base(param, 80, 25)
		{
			Capture = true;
		}

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

		public override string ProtocolDescription
		{
			get
			{
				return "Stream-based terminal output";
			}
		}

		public override string[] ConnectionParameter
		{
			get
			{
				return new string[] { "Stream connection" };
			}
		}

		public override bool Available
		{
			get
			{
				return false;
			}
		}

		public object SynchronizationObject
		{
			get
			{
				return _synchronizationObject;
			}
		}

		public Queue<byte> OutputQueue
		{
			get
			{
				return _outputQueue;
			}
		}

		public override ConnectionTag Reproduce()
		{
			throw new NotImplementedException();
		}

		public override void RepeatAsyncRead(IDataReceiver cb)
		{
		}

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
