/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TerminalInput.cs,v 1.2 2005/04/20 08:45:47 okajima Exp $
*/
using System;
using System.Resources;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.Log;
using Poderosa.Forms;
using Poderosa.Config;
using Poderosa.Text;
using Poderosa.Communication;

namespace Poderosa.Terminal
{
	public interface ITerminal {

		/// <summary>
		/// ホストからの入力処理
		/// </summary>
		void Input(byte[] data, int offset, int length);

		/// <summary>
		/// ホストからの入力処理(デコード済み)
		/// </summary>
		void Input(char[] data, int offset, int length);

		/// <summary>
		/// １文字を処理（ふつうはInput内でDecoder経由で処理される）
		/// </summary>
		void ProcessChar(char ch);
		
		ProcessCharResult State { get; }

		void Reset();
		void FullReset();


		/// <summary>
		/// 主にISO2022において、未サポートのキャラクタセットが指定された
		/// </summary>
		void UnsupportedCharSetDetected(char code);

		/// <summary>
		/// byte->char変換に失敗したときの通知
		/// </summary>
		void InvalidCharDetected(Encoding enc, byte[] data);

		/// <summary>
		/// カーソルキーに対応する送信データを取得
		/// </summary>
		byte[] SequenceKeyData(Keys modifier, Keys body);

		/// <summary>
		/// 現在のモード
		/// </summary>
		TerminalMode TerminalMode { get; }

		void   ClearMacroBuffer();
		void   SignalData();
		string ReadLineFromMacroBuffer();
		string ReadAllFromMacroBuffer();

		ITerminalTextLogger Logger { get; }

		/// <summary>
		/// for debug
		/// </summary>
		void DumpCurrentText();
	}

	internal interface ICharDecoder {
		void Input(ITerminal terminal, byte[] data, int offset, int length);
		void Reset(EncodingProfile enc);
		EncodingProfile Encoding { get; }
	}

	public enum ProcessCharResult {
		Processed,
		Unsupported,
		Escaping
	}



	/// <summary>
	/// 
	/// </summary>
	internal class JapaneseCharDecoder : ICharDecoder {
		private static Encoding JIS = System.Text.Encoding.GetEncoding("iso-2022-jp"); //英語環境ではこれは失敗するかも

		//次の入力に繋げるための状態
		private enum State {
			Normal, //標準
			ESC,    //ESCが来たところ
			ESC_DOLLAR,    //ESC $が来たところ
			ESC_BRACKET,   //ESC (が来たところ
			ESC_ENDBRACKET    //ESC )が来たところ
		}
		private State _state;

		//挿入された日本語(JISエンコーディング)のためのバッファ
		private MemoryStream _jisbuf;

		//MBCSの状態管理
		private EncodingProfile _encoding;

		private static char[] DEC_SPECIAL_LINES;

		//現在繋がっているコネクション
		private TerminalConnection _connection;
		//文字を処理するターミナル
		private ITerminal _terminal;

		public JapaneseCharDecoder(TerminalConnection con) {
			_jisbuf = new MemoryStream(0x1000);
			_state = State.Normal;
			_connection = con;
			_encoding = con.Param.EncodingProfile;

			_iso2022jpByteProcessor   = new ByteProcessor(this.ProcessByteAsISO2022JP);
			_DECLineByteProcessor     = new ByteProcessor(this.ProcessByteAsDECLine);
			_currentByteProcessor = null;
			_G0ByteProcessor = null;
			_G1ByteProcessor = null;
		}
		public EncodingProfile Encoding {
			get {
				return _encoding;
			}
		}
	

		private delegate void ByteProcessor(byte b);
		private ByteProcessor _currentByteProcessor;
		private ByteProcessor _G0ByteProcessor; //iso2022のG0,G1
		private ByteProcessor _G1ByteProcessor;
		private ByteProcessor _iso2022jpByteProcessor; 
		private ByteProcessor _DECLineByteProcessor;


		public void Input(ITerminal terminal, byte[] data, int offset, int count) {
			_terminal = terminal;
			//処理本体
			int last = offset+count;
			while(offset < last) {
				byte b = data[offset++];
				ProcessByte(b);
			}
		}

		private void ProcessByte(byte b) {
			if(_terminal.State==ProcessCharResult.Escaping)
				_terminal.ProcessChar((char)b);
			else {
				if(_state==State.Normal && !IsControlChar(b) && _encoding.IsInterestingByte(b)) {
					PutMBCSByte(b);
				}
				else {
					switch(_state) {
						case State.Normal:
							if(b==0x1B) //ESC
								_state = State.ESC;
							else if(b==14) //SO
								ChangeProcessor(_G1ByteProcessor);
							else if(b==15) //SI
								ChangeProcessor(_G0ByteProcessor);
							else
								ConsumeByte(b);
							break;
						case State.ESC:
							if(b==(byte)'$')
								_state = State.ESC_DOLLAR;
							else if(b==(byte)'(')
								_state=State.ESC_BRACKET;
							else if(b==(byte)')')
								_state=State.ESC_ENDBRACKET;
							else {
								ConsumeByte(0x1B);
								ConsumeByte(b);
								_state = State.Normal;
							}
							break;
						case State.ESC_BRACKET:
							if(b==(byte)'0')
								_G0ByteProcessor = _DECLineByteProcessor;
							else if(b==(byte)'B' || b==(byte)'J' || b==(byte)'~') //!!lessでssh2architecture.txtを見ていたら来た。詳細はまだ調べていない。
								_G0ByteProcessor = null;
							else
								_terminal.UnsupportedCharSetDetected((char)b);
							ChangeProcessor(_G0ByteProcessor);
							break;
						case State.ESC_ENDBRACKET:
							if(b==(byte)'0')
								_G1ByteProcessor = _DECLineByteProcessor;
							else if(b==(byte)'B' || b==(byte)'J' || b==(byte)'~') //!!lessでssh2architecture.txtを見ていたら来た。詳細はまだ調べていない。
								_G1ByteProcessor = null;
							_state = State.Normal;
							break;
						case State.ESC_DOLLAR:
							if(b==(byte)'B' || b==(byte)'@') {
								ChangeProcessor(_iso2022jpByteProcessor);
							}
							else {
								_terminal.ProcessChar((char)0x1B);
								_terminal.ProcessChar('$');
								_terminal.ProcessChar((char)b);
								_state = State.Normal;
							}
							break;
						default:
							Debug.Assert(false, "unexpected state transition");
							break;
					}
				}
			}
		}
		private void ProcessByteAsISO2022JP(byte b) {
			_jisbuf.WriteByte(b);
		}
		private void ProcessByteAsDECLine(byte b) {
			char ch = (char)b;
			if(0x60<=ch && ch<=0x7F) {
				if(DEC_SPECIAL_LINES==null) {
					ResourceManager m = new ResourceManager("Poderosa.Terminal.rulechars", typeof(GEnv).Assembly);
					DEC_SPECIAL_LINES = m.GetString("DECLineChars").ToCharArray();
					//リソースで?になっていたところを埋める
					DEC_SPECIAL_LINES[2] = (char)0x09;
					DEC_SPECIAL_LINES[3] = (char)0x0C;
					DEC_SPECIAL_LINES[4] = (char)0x0D;
					DEC_SPECIAL_LINES[5] = (char)0x0A;
					DEC_SPECIAL_LINES[8] = (char)0x0A; //NLとあるがよくわからない。LFにしておく。
					DEC_SPECIAL_LINES[9] = (char)0x0B;
					DEC_SPECIAL_LINES[31] = (char)0x7F;
				}
				char linechar = DEC_SPECIAL_LINES[(int)(ch - 0x60)];
				_terminal.ProcessChar(linechar);
			}
			else
				_terminal.ProcessChar(ch);
		}

		private void ChangeProcessor(ByteProcessor newprocessor) {
			//既存のやつがあればリセット
			if(_currentByteProcessor==_iso2022jpByteProcessor) {
				FlushJISBuffer();
			}

			if(newprocessor==_iso2022jpByteProcessor) {
				InitJISBuffer();
			}

			_currentByteProcessor = newprocessor;
			_state = State.Normal;
		}

		private void ConsumeByte(byte b) {
			if(_currentByteProcessor==null)
				_terminal.ProcessChar((char)b);
			else
				_currentByteProcessor(b);
		}


		public void Reset(EncodingProfile enc) {
			_encoding.Reset();
			_encoding = enc;
			_encoding.Reset();
		}

		private static bool IsControlChar(byte b) {
			return b<=0x1F;
		}
		private void InitJISBuffer() {
			_jisbuf.SetLength(0);
			_jisbuf.WriteByte(0x1B);
			_jisbuf.WriteByte((byte)'$');
			_jisbuf.WriteByte((byte)'B');
		}
		private void FlushJISBuffer() {
			_jisbuf.WriteByte(0x1B);
			_jisbuf.WriteByte((byte)'(');
			_jisbuf.WriteByte((byte)'B');

			char[] x = JIS.GetString(_jisbuf.ToArray()).ToCharArray();
			for(int i=0; i<x.Length; i++)
				_terminal.ProcessChar(x[i]);
			_jisbuf.SetLength(0);
		}
		private void PutMBCSByte(byte b) {
			char[] t = new char[2];
			try {
				char ch = _encoding.PutByte(b);
				if(ch!='\0')
					_terminal.ProcessChar(ch);
			}
			catch(Exception) {
				_terminal.InvalidCharDetected(_encoding.Encoding, _encoding.Buffer);
				_encoding.Reset();
			}
		}
	}

	internal class UnknownEscapeSequenceException : Exception {
		public UnknownEscapeSequenceException(string msg) : base(msg) {}
	}

	internal struct IntPair {
		public int first;
		public int second;

		public IntPair(int f, int s) {
			first = f;
			second = s;
		}
	}
}
