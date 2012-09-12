/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: Telnet.cs,v 1.2 2005/04/20 08:45:47 okajima Exp $
*/
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Diagnostics;
using Poderosa.Toolkit;
using Poderosa.Terminal;

using Poderosa.ConnectionParam;

namespace Poderosa.Communication
{

	/// <summary>
	/// TelnetOptionの送受信をする。あまり複雑なサポートをするつもりはない。
	/// Guevaraで必要なのはSuppressGoAhead(双方向), TerminalType, NAWSの３つだけで、これらが成立しなければ例外を投げる。
	/// それ以外のTelnetOptionは拒否するが、拒否が成立しなくても_refusedOptionに格納するだけでエラーにはしない。
	/// オプションのネゴシエーションが終了したら、最後に受信したパケットはもうシェル本体であるので、呼び出し側はこれを使うようにしないといけない。
	/// </summary>
	internal class TelnetNegotiator
	{
		//必要ならここから情報を読む
		private TerminalParam _param;
		private int _width;
		private int _height;

		private TelnetCode _state;
		private MemoryStream _sequenceBuffer;
		private TelnetOptionWriter _optionWriter;
		private bool _defaultOptionSent;

		internal enum ProcessResult {
			NOP,
			REAL_0xFF
		}

		//接続を中断するほどではないが期待どおりでなかった場合に警告を出す
		private ArrayList _warnings;
		public ArrayList Warnings {
			get {
				return _warnings;
			}
		}

		private ArrayList   _refusedOptions;
		/*
		public TelnetCode[] RefusedOptions {
			get {
				return (TelnetCode[])_refusedOptions.ToArray(typeof(TelnetCode));
			}
		}
		*/

		public TelnetNegotiator(TerminalParam param, int width, int height) {
			_param = param;
			_refusedOptions = new ArrayList();
			_width = width;
			_height = height;
			_warnings = new ArrayList();
			_state = TelnetCode.NA;
			_sequenceBuffer = new MemoryStream();
			_optionWriter = new TelnetOptionWriter();
			_defaultOptionSent = false;
		}

		public void Flush(AbstractGuevaraSocket s) {
			if(!_defaultOptionSent) {
				WriteDefaultOptions();
				_defaultOptionSent = true;
			}

			if(_optionWriter.Length > 0) {
				_optionWriter.WriteTo(s);
				s.Flush();
				_optionWriter.Clear();
			}
		}

		private void WriteDefaultOptions() {
			_optionWriter.Write(TelnetCode.WILL, TelnetOption.TerminalType);
			_optionWriter.Write(TelnetCode.DO,   TelnetOption.SuppressGoAhead);
			_optionWriter.Write(TelnetCode.WILL, TelnetOption.SuppressGoAhead);
			_optionWriter.Write(TelnetCode.WILL, TelnetOption.NAWS);
		}

		public bool InProcessing {
			get {
				return _state!=TelnetCode.NA;
			}
		}
		public void StartNegotiate() {
			_state = TelnetCode.IAC;
		}

		public ProcessResult Process(byte data) {
			Debug.Assert(_state!=TelnetCode.NA);
			switch(_state) {
				case TelnetCode.IAC:
					if(data==(byte)TelnetCode.SB || ((byte)TelnetCode.WILL<=data && data<=(byte)TelnetCode.DONT))
						_state = (TelnetCode)data;
					else if(data==(byte)TelnetCode.IAC) {
						_state = TelnetCode.NA;
						return ProcessResult.REAL_0xFF;
					}
					else
						_state = TelnetCode.NA;
					break;
				case TelnetCode.SB:
					if(data!=(byte)TelnetCode.SE)
						_sequenceBuffer.WriteByte(data);
					else {
						ProcessSequence(_sequenceBuffer.ToArray());
						_state = TelnetCode.NA;
						_sequenceBuffer.SetLength(0);
					}
					break;
				case TelnetCode.DO:
				case TelnetCode.DONT:
				case TelnetCode.WILL:
				case TelnetCode.WONT:
					ProcessOptionRequest(data);
					_state = TelnetCode.NA;
					break;
			}

			return ProcessResult.NOP;
		}

		private void ProcessSequence(byte[] response) {
			if(response[1]==1) {
				if(response[0]==(byte)TelnetOption.TerminalType)
					_optionWriter.WriteTerminalName(EnumDescAttribute.For(typeof(TerminalType)).GetDescription(_param.TerminalType));
			}
		}

		private void ProcessOptionRequest(byte option_) {
			TelnetOption option = (TelnetOption)option_;
			switch(option) {
				case TelnetOption.TerminalType:
					if(_state==TelnetCode.DO)
						_optionWriter.Write(TelnetCode.WILL, option);
					else
						_warnings.Add(GEnv.Strings.GetString("Message.Telnet.FailedToSendTerminalType"));
					break;
				case TelnetOption.NAWS:
					if(_state==TelnetCode.DO)
						_optionWriter.WriteTerminalSize(_width, _height);
					else
						_warnings.Add(GEnv.Strings.GetString("Message.Telnet.FailedToSendWidnowSize"));
					break;
				case TelnetOption.SuppressGoAhead:
					if(_state!=TelnetCode.WILL && _state!=TelnetCode.DO) //!!両方が来たことを確認する
						_warnings.Add(GEnv.Strings.GetString("Message.Telnet.FailedToSendSuppressGoAhead"));
					break;
				case TelnetOption.LocalEcho:
					if(_state==TelnetCode.DO)
						_optionWriter.Write(TelnetCode.WILL, option);
					break;
				default: //上記以外はすべて拒否。DOにはWON'T, WILLにはDON'Tの応答を返す。 
					if(_state==TelnetCode.DO)
						_optionWriter.Write(TelnetCode.WONT, option);
					else if(_state==TelnetCode.WILL)
						_optionWriter.Write(TelnetCode.DONT, option);
					break;
			}
		}

	}


	internal class TelnetOptionWriter {
		private MemoryStream _strm;
		public TelnetOptionWriter() {
			_strm = new MemoryStream();
		}
		public long Length {
			get {
				return _strm.Length;
			}
		}
		public void Clear() {
			_strm.SetLength(0);
		}

		public void WriteTo(AbstractGuevaraSocket target) {
			byte[] data = _strm.ToArray();
			target.Transmit(data, 0, data.Length);
			target.Flush();
		}
		public void Write(TelnetCode code, TelnetOption opt) {
			_strm.WriteByte((byte)TelnetCode.IAC);
			_strm.WriteByte((byte)code);
			_strm.WriteByte((byte)opt);
		}
		public void WriteTerminalName(string name) {
			_strm.WriteByte((byte)TelnetCode.IAC);
			_strm.WriteByte((byte)TelnetCode.SB);
			_strm.WriteByte((byte)TelnetOption.TerminalType);
			_strm.WriteByte(0); //0 = IS
			byte[] t = Encoding.ASCII.GetBytes(name);
			_strm.Write(t, 0, t.Length);
			_strm.WriteByte((byte)TelnetCode.IAC);
			_strm.WriteByte((byte)TelnetCode.SE);
		}
		public void WriteTerminalSize(int width, int height) {
			_strm.WriteByte((byte)TelnetCode.IAC);
			_strm.WriteByte((byte)TelnetCode.SB);
			_strm.WriteByte((byte)TelnetOption.NAWS);
			//幅や高さが256以上になることはないだろうからこれで逃げる
			_strm.WriteByte(0);
			_strm.WriteByte((byte)width);
			_strm.WriteByte(0);
			_strm.WriteByte((byte)height);
			_strm.WriteByte((byte)TelnetCode.IAC);
			_strm.WriteByte((byte)TelnetCode.SE);
		}
	}

	internal enum TelnetCode {
		NA = 0,
		SE = 240,
		NOP = 241,
		Break = 243,
		AreYouThere = 246,
		SB = 250,
		WILL = 251,
		WONT = 252,
		DO = 253,
		DONT = 254,
		IAC = 255
	}
	internal enum TelnetOption {
		LocalEcho = 1,
		SuppressGoAhead = 3,
		TerminalType = 24,
		NAWS = 31
	}

	internal class TelnetNegotiationException : ApplicationException {
		public TelnetNegotiationException(string msg) : base(msg) {}
	}

}
