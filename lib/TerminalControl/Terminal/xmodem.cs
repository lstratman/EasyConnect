/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: xmodem.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using Poderosa.Connection;

namespace Poderosa.Terminal
{
	/// <summary>
	/// xmodem の概要の説明です。
	/// </summary>
	public abstract class XModem : IModalTerminalTask {
		public const byte SOH = 1;
		public const byte STX = 2;
		public const byte EOT = 4;
		public const byte ACK = 6;
		public const byte NAK = 21;
		public const byte CAN = 24;
		public const byte SUB = 26;

		//状態通知
		public const int NOTIFY_SUCCESS = 0;
		public const int NOTIFY_PROGRESS = 1;
		public const int NOTIFY_ERROR = 2;
		public const int NOTIFY_TIMEOUT = 3;


		//CRC
		public static ushort CalcCRC(byte[] data, int offset, int length) {
			ushort crc = 0;
			for(int i=0; i<length; i++) {
				byte d = data[offset+i];
				/*
				int count = 8;
				while(--count>=0) {
					if((crc & 0x8000)!=0) {
						crc <<= 1;
						crc += (((d<<=1) & 0x0400) != 0);
						crc ^= 0x1021;
					}
					else {
						crc <<= 1;
						crc += (((d<<=1) & 0x0400) != 0);
					}
				}
				*/
				crc ^= (ushort)((ushort)d << 8);
				for(int j=1; j<=8; j++) {
					if((crc & 0x8000)!=0)
						crc = (ushort)((crc<<1) ^ (ushort)0x1021);
					else
						crc <<= 1;
				}
			}
			return crc;
		}

		protected ConnectionTag _tag;
		protected string _fileName;
		protected byte _sequenceNumber;
		protected long _processedLength;
		protected bool _crcEnabled;
		protected Stream _stream;
		protected Timer _timer;
		protected IntPtr _notifyTarget;

		public XModem(ConnectionTag tag, string fn) {
			_tag = tag;
			_fileName = fn;
			_sequenceNumber = 1;
		}
		public bool CRCEnabled {
			get {
				return _crcEnabled;
			}
		}
		public IntPtr NotifyTarget {
			get {
				return _notifyTarget;
			}
			set {
				_notifyTarget = value;
			}
		}
		public abstract void Start();
		public abstract void Abort();

		public abstract void Input(byte[] data, int offset, int count);
		public bool CanReceive {
			get {
				return true;
			}
		}
		public string Caption {
			get {
				return "XMODEM";
			}
		}

		protected void NotifyStatus(int wparam, int lparam) {
			if(_notifyTarget==IntPtr.Zero) return;
			Win32.SendMessage(_notifyTarget, GConst.WMG_XMODEM_UPDATE_STATUS, new IntPtr(wparam), new IntPtr(lparam));
		}
	}

	public class XModemReceiver : XModem {
		private int _retryCount;
		private MemoryStream _buffer;

		//private FileStream _debugStream;

		private const int CRC_TIMEOUT = 1;
		private const int NEGOTIATION_TIMEOUT = 2;

		public XModemReceiver(ConnectionTag tag, string filename) : base(tag, filename) {
			_stream = new FileStream(_fileName, FileMode.Create, FileAccess.Write);
		}
		public override void Start() {
			_tag.ModalTerminalTask = this;
			_timer = new Timer(new TimerCallback(OnTimeout), CRC_TIMEOUT, 3000, Timeout.Infinite);
			_crcEnabled = true;
			_tag.Connection.Write(new byte[] { (byte)'C' }); //CRCモードでトライ

			//_debugStream = new FileStream("C:\\IOPort\\xmodemtest.bin", FileMode.Create, FileAccess.Write);
		}


		private void OnTimeout(object state) {
			_timer.Dispose();
			_timer = null;
			switch((int)state){ 
				case CRC_TIMEOUT:
					_crcEnabled = false;
					_timer = new Timer(new TimerCallback(OnTimeout), NEGOTIATION_TIMEOUT, 5000, Timeout.Infinite);
					_tag.Connection.Write(new byte[] { NAK });
					break;
				case NEGOTIATION_TIMEOUT:
					_tag.Connection.Write(new byte[] { CAN });
					GEnv.InterThreadUIService.Warning(GEnv.Strings.GetString("Message.XModem.StartTimedOut"));
					NotifyStatus(NOTIFY_TIMEOUT, 0);
					Exit();
					break;
			}

		}
		private void Exit() {
			_tag.ModalTerminalTask = null;
			_stream.Close();
			if(_timer!=null) _timer.Dispose();
		}
		public override void Abort() {
			_tag.Connection.Write(new byte[] { CAN });
			Exit();
		}

		public override void Input(byte[] data, int offset, int count) {
			if(_timer!=null) {
				_timer.Dispose();
				_timer = null;
			}
			
			//Debug.WriteLine(String.Format("Received {0}", count));
			//_debugStream.Write(data, offset, count);
			//_debugStream.Flush();
			AdjustBuffer(ref data, ref offset, ref count);

			byte head = data[offset];
			if(head==EOT) { //successfully exit
				_tag.Connection.Write(new byte[] { ACK });
				GEnv.InterThreadUIService.Information(GEnv.Strings.GetString("Message.XModem.ReceiveComplete"));
				NotifyStatus(NOTIFY_SUCCESS, 0);
				//_debugStream.Close();
				Exit();
			}
			else {
				int required = 3+(head==STX? 1024 : 128)+(_crcEnabled? 2 : 1);
				if(required>count) {
					ReserveBuffer(data, offset, count); //途中で切れていた場合
					//Debug.WriteLine(String.Format("Reserving #{0} last={1} offset={2} count={3}", seq, last, offset, count));
					return;
				}

				byte seq  = data[offset+1];
				byte neg  = data[offset+2];
				if(seq!=_sequenceNumber || seq+neg!=255) {
					Abort();
					GEnv.InterThreadUIService.Warning(GEnv.Strings.GetString("Message.XModem.SequenceError"));
					NotifyStatus(NOTIFY_ERROR, 0);
				}
				else {
					//Debug.WriteLine(String.Format("Processing #{0}", seq));
					bool success;
					int  body_offset = offset+3;
					int  body_len = head==STX? 1024 : 128;
					int  checksum_offset = offset+3+body_len;
					if(_crcEnabled) {
						ushort sent = (ushort)( (((ushort)data[checksum_offset])<<8) + (ushort)data[checksum_offset+1] );
						ushort sum  = CalcCRC(data, body_offset, body_len);
						success = (sent==sum);
					}
					else {
						byte sent = data[checksum_offset];
						byte sum = 0;
						for(int i=body_offset; i<checksum_offset; i++) sum += data[i];
						success = (sent==sum);
					}

					_buffer = null; //ブロックごとにACKを待つ仕様なので、もらってきたデータが複数ブロックにまたがることはない。したがってここで破棄して構わない。
					if(success) {
						_tag.Connection.Write(new byte[] { ACK });
						_sequenceNumber++;

						int t = checksum_offset-1;
						while(t>=body_offset && data[t]==26) t--; //Ctrl+Zで埋まっているところは無視
						int len = t+1 - body_offset;
						_stream.Write(data, body_offset, len);
						_processedLength += len;
						NotifyStatus(NOTIFY_PROGRESS, (int)_processedLength);
						_retryCount = 0;
					}
					else {
						//_debugStream.Close();
						if(++_retryCount==3) { //もうあきらめる
							Abort();
							GEnv.InterThreadUIService.Warning(GEnv.Strings.GetString("Message.XModem.CheckSumError"));
							NotifyStatus(NOTIFY_ERROR, 0);
						}
						else {
							_tag.Connection.Write(new byte[] { NAK });
						}
					}

				}
			}
		}

		private void ReserveBuffer(byte[] data, int offset, int count) {
			_buffer = new MemoryStream();
			_buffer.Write(data, offset, count);
		}
		private void AdjustBuffer(ref byte[] data, ref int offset, ref int count) {
			if(_buffer==null || _buffer.Position==0) return;

			_buffer.Write(data, offset, count);
			count = (int)_buffer.Position;
			_buffer.Close();
			data = _buffer.ToArray();
			Debug.Assert(data.Length==count);
			offset = 0;
		}

	}

	public class XModemSender : XModem {
		private bool _negotiating;
		private int _retryCount;
		private byte[] _body;
		private int _offset;
		private int _nextOffset;

		//private FileStream _debugStream;

		private const int NEGOTIATION_TIMEOUT = 1;

		public int TotalLength {
			get {
				return _body.Length;
			}
		}

		public XModemSender(ConnectionTag tag, string filename) : base(tag, filename) {
			_body = new byte[new FileInfo(filename).Length];
			FileStream strm = new FileStream(filename, FileMode.Open, FileAccess.Read);
			strm.Read(_body, 0, _body.Length);
			strm.Close();
		}
		public override void Start() {
			_tag.ModalTerminalTask = this;
			_timer = new Timer(new TimerCallback(OnTimeout), NEGOTIATION_TIMEOUT, 60000, Timeout.Infinite);
			_negotiating = true;
			//_tag.Connection.WriteChars(TerminalUtil.NewLineChars(_tag.Connection.Param.TransmitNL));
		}

		private void OnTimeout(object state) {
			_timer.Dispose();
			_timer = null;
			switch((int)state){ 
				case NEGOTIATION_TIMEOUT:
					GEnv.InterThreadUIService.Warning(GEnv.Strings.GetString("Message.XModem.StartTimedOut"));
					NotifyStatus(NOTIFY_TIMEOUT, 0);
					Exit();
					break;
			}

		}
		private void Exit() {
			_tag.ModalTerminalTask = null;
			if(_timer!=null) _timer.Dispose();
		}
		public override void Abort() {
			_tag.Connection.Write(new byte[] { CAN });
			Exit();
		}

		public override void Input(byte[] data, int offset, int count) {
			if(_timer!=null) {
				_timer.Dispose();
				_timer = null;
			}

			if(_negotiating) {
				for(int i=0; i<count; i++) {
					byte t = data[offset+i];
					if(t==NAK || t==(byte)'C') {
						_crcEnabled = t==(byte)'C';
						_negotiating = false;
						_sequenceNumber = 1;
						_offset = _nextOffset = 0;
						break;
					}
				}
				if(_negotiating) return; //あたまがきていない
			}
			else {
				byte t = data[offset];
				if(t==ACK) {
					_sequenceNumber++;
					_retryCount = 0;
					if(_offset==_body.Length) { //successfully exit
						Exit();
						GEnv.InterThreadUIService.Information(GEnv.Strings.GetString("Message.XModem.SendComplete"));
						NotifyStatus(NOTIFY_SUCCESS, 0);
						return;
					}
					_offset = _nextOffset;
				}
				else if(t!=NAK || (++_retryCount==3)) {
					Abort();
					GEnv.InterThreadUIService.Warning(GEnv.Strings.GetString("Message.XModem.BlockStartError"));
					NotifyStatus(NOTIFY_ERROR, 0);
					return;
				}
			}

			if(_nextOffset>=_body.Length) { //last
				_tag.Connection.Write(new byte[] { EOT });
				_offset = _body.Length;
			}
			else {
				int len = 128;
				if(_crcEnabled && _offset+1024<=_body.Length) len = 1024;
				byte[] buf = new byte[3+len+(_crcEnabled? 2 : 1)];
				buf[0] = len==128? SOH : STX;
				buf[1] = (byte)_sequenceNumber;
				buf[2] = (byte)(255 - buf[1]);
				int body_len = Math.Min(len, _body.Length-_offset);
				Array.Copy(_body, _offset, buf, 3, body_len);
				for(int i=body_len; i<len; i++)
					buf[3+i] = 26; //padding
				if(_crcEnabled) {
					ushort sum  = CalcCRC(buf, 3, len);
					buf[3+len  ] = (byte)(sum >> 8);
					buf[3+len+1] = (byte)(sum & 0xFF);
				}
				else {
					byte sum = 0;
					for(int i=0; i<len; i++) sum += buf[3+i];
					buf[3+len] = sum;
				}

				_nextOffset = _offset + len;
				_tag.Connection.Write(buf, 0, buf.Length);
				NotifyStatus(NOTIFY_PROGRESS, _nextOffset);
				//Debug.WriteLine("Transmitted "+_sequenceNumber+" " +_offset);
			}

		}


	}
}
