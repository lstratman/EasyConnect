/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TerminalBase.cs,v 1.2 2005/04/20 08:45:47 okajima Exp $
*/
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

using Poderosa.Log;
using Poderosa.Config;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Text;
using Poderosa.Communication;

namespace Poderosa.Terminal
{
	/// <summary>
	/// ターミナル
	/// データを受信してドキュメントを操作する機能をもつ。
	/// </summary>
	internal abstract class AbstractTerminal : ITerminal {
		public abstract void ProcessChar(char ch);
		public abstract ProcessCharResult State { get; }
		public abstract byte[] SequenceKeyData(Keys modifier, Keys body);

		protected StringBuilder _bufferForMacro;
		protected AutoResetEvent _signalForMacro; //マクロスレッドにデータの読み取り可能を知らせる
		protected ICharDecoder     _decoder;
		protected GLineManipulator _manipulator;
		protected ITerminalTextLogger _logger; //Loggerプロパティとの重複を何とかしたい

		public ITerminalTextLogger Logger {
			get {
				return _tag.Connection.TextLogger;
			}
		}

		protected ConnectionTag  _tag;
		protected TextDecoration _currentdecoration;
		
		protected TerminalMode _terminalMode;
		protected TerminalMode _cursorKeyMode; //_terminalModeは別物。AIXでのviで、カーソルキーは不変という例が確認されている

		public TerminalMode TerminalMode {
			get {
				return _terminalMode;
			}
		}
		public TerminalMode CursorKeyMode {
			get {
				return _cursorKeyMode;
			}
		}
		protected TerminalConnection GetConnection() {
			return _tag.Connection;
		}
		protected TerminalDocument GetDocument() {
			return _tag.Document;
		}

		protected abstract void ChangeMode(TerminalMode tm);
		protected abstract void ResetInternal();

		protected virtual void ChangeCursorKeyMode(TerminalMode tm) {
			_cursorKeyMode = tm;
		}

		/// <summary>
		/// 操作の対象になるドキュメントと文字のエンコーディングを指定して構築
		/// </summary>
		public AbstractTerminal(ConnectionTag tag, ICharDecoder decoder) {
			_tag = tag;
			_decoder = decoder;
			_terminalMode = TerminalMode.Normal;
			_currentdecoration = TextDecoration.Default;
			_manipulator = new GLineManipulator(80);
			_bufferForMacro = new StringBuilder();
			_signalForMacro = new AutoResetEvent(false);
		}
		public void Input(byte[] data, int offset, int length) {
			_manipulator.Load(GetDocument().CurrentLine, 0);
			_manipulator.CaretColumn = GetDocument().CaretColumn;
			_manipulator.DefaultDecoration = _currentdecoration;
			
			_decoder.Input(this, data, offset, length);

			GetDocument().ReplaceCurrentLine(_manipulator.Export());
			GetDocument().CaretColumn = _manipulator.CaretColumn;
		}
		public void Input(char[] data, int offset, int length) {
			_manipulator.Load(GetDocument().CurrentLine, 0);
			_manipulator.CaretColumn = GetDocument().CaretColumn;
			_manipulator.DefaultDecoration = _currentdecoration;
			
			for(int i=0; i<length; i++)
				ProcessChar(data[offset+i]);

			GetDocument().ReplaceCurrentLine(_manipulator.Export());
			GetDocument().CaretColumn = _manipulator.CaretColumn;
		}

		public void UnsupportedCharSetDetected(char code) {
			string desc;
			if(code=='0')
				desc = "0 (DEC Special Character)"; //これはよくあるので但し書きつき
			else
				desc = new String(code, 1);

			if(GEnv.Options.WarningOption!=WarningOption.Ignore) {
				GEnv.InterThreadUIService.UnsupportedCharSetDetected(GetDocument(), String.Format(GEnv.Strings.GetString("Message.AbstractTerminal.UnsupportedCharSet"), desc));
			}
		}
		public void InvalidCharDetected(Encoding enc, byte[] buf) {
			if(GEnv.Options.WarningOption!=WarningOption.Ignore) {
				GEnv.InterThreadUIService.InvalidCharDetected(GetDocument(), String.Format(GEnv.Strings.GetString("Message.AbstractTerminal.UnexpectedChar"), enc.WebName));
			}
		}
		public void Reset() {
			//Encodingが同じ時は簡単に済ませることができる
			if(_decoder.Encoding.Type==_tag.Connection.Param.Encoding)
				_decoder.Reset(_decoder.Encoding);
			else
				_decoder = new JapaneseCharDecoder(_tag.Connection);
		}
		//public void SetEncoding(EncodingProfile enc) {
		//	_decoder.SetEncoding(enc);
		//}

		public void ClearMacroBuffer() {
			_bufferForMacro.Remove(0, _bufferForMacro.Length);
			_signalForMacro.Reset();
		}
		public void SignalData() {
			_signalForMacro.Set();
		}
		protected void AppendMacroBuffer(char ch) {
			if(ch!='\r' && ch!='\0') {
				lock(_bufferForMacro) {
					_bufferForMacro.Append(ch); //!!長さに上限をつけたほうが安全かも
				}
			}
		}
		//マクロ実行スレッドから呼ばれる１行読み出しメソッド
		public string ReadLineFromMacroBuffer() {
			do {
				int l = _bufferForMacro.Length;
				int i=0;
				for(i=0; i<l; i++) {
					if(_bufferForMacro[i]=='\n') break;
				}

				if(l>0 && i<l) { //めでたく行末がみつかった
					int j=i;
					if(i>0 && _bufferForMacro[i-1]=='\r') j=i-1; //CRLFのときは除いてやる
					string r;
					lock(_bufferForMacro) {
						r = _bufferForMacro.ToString(0, j);
						_bufferForMacro.Remove(0, i+1);
					}
					return r;
				}
				else {
					_signalForMacro.Reset();
					_signalForMacro.WaitOne();
				}
			} while(true);
		}
		//マクロ実行スレッドから呼ばれる、「何かデータがあれば全部もっていく」メソッド
		public string ReadAllFromMacroBuffer() {
			if(_bufferForMacro.Length==0) {
				_signalForMacro.Reset();
				_signalForMacro.WaitOne();
			}

			lock(_bufferForMacro) {
				string r = _bufferForMacro.ToString();
				_bufferForMacro.Remove(0, _bufferForMacro.Length);
				return r;
			}
		}

		//これはメインスレッドから呼び出すこと
		public virtual void FullReset() {
			lock(_tag.Document) {
				ChangeMode(TerminalMode.Normal);
				_tag.Document.ClearScrollingRegion();
				ResetInternal();
				_decoder = new JapaneseCharDecoder(_tag.Connection);
			}
		}

		public void DumpCurrentText() {
			Debug.WriteLine(_manipulator.ToString());
		}

	}
	
	//Escape Sequenceを使うターミナル
	internal abstract class EscapeSequenceTerminal : AbstractTerminal {
		public EscapeSequenceTerminal(ConnectionTag tag, ICharDecoder decoder) : base(tag, decoder) {
			_escapeSequence = new StringBuilder();
			_processCharResult = ProcessCharResult.Processed;
		}

		private StringBuilder _escapeSequence;
		private ProcessCharResult _processCharResult;

		public override ProcessCharResult State {
			get {
				return _processCharResult;
			}
		}
		protected override void ResetInternal() {
			_escapeSequence = new StringBuilder();
			_processCharResult = ProcessCharResult.Processed;
		}

		public override void ProcessChar(char ch) {
			
			_logger = Logger; //_loggerはこのProcessCharの処理内でのみ有効。
			
			if(_processCharResult != ProcessCharResult.Escaping) {
				if(ch==0x1B) {
					_processCharResult = ProcessCharResult.Escaping;
				} else {
					//!!久しぶりのこのあたりを見るとけっこう汚い分岐だな
					_logger.Append(ch);
					if(GEnv.Frame.MacroIsRunning) AppendMacroBuffer(ch);

					if(ch < 0x20 || (ch>=0x80 && ch<0xA0))
						_processCharResult = ProcessControlChar(ch);
					else
						_processCharResult = ProcessNormalChar(ch);
				}
			}
			else {
				if(ch=='\0') return; //シーケンス中にNULL文字が入っているケースが確認された
				_escapeSequence.Append(ch);
				bool end_flag = false; //escape sequenceの終わりかどうかを示すフラグ
				if(_escapeSequence.Length==1) { //ESC+１文字である場合
					end_flag = ('0'<=ch && ch<='9') || ('a'<=ch && ch<='z') || ('A'<=ch && ch<='Z') || ch=='>' || ch=='=' || ch=='|' || ch=='}' || ch=='~';
				}
				else if(_escapeSequence[0]==']') { //OSCの終端はBELかST(String Terminator)
					end_flag = ch==0x07 || ch==0x9c; 
				}
				else {
					end_flag = ('a'<=ch && ch<='z') || ('A'<=ch && ch<='Z') || ch=='@' || ch=='~' || ch=='|' || ch=='{';
				}
				
				if(end_flag) { //シーケンスのおわり
					char[] seq = _escapeSequence.ToString().ToCharArray();
					_logger.BeginEscapeSequence();
					_logger.Append(seq, 0, seq.Length);
					_logger.CommitEscapeSequence();
					_logger.Flush();
					try {
						char code = seq[0];
						_processCharResult = ProcessCharResult.Unsupported; //ProcessEscapeSequenceで例外が来た後で状態がEscapingはひどい結果を招くので
						_processCharResult = ProcessEscapeSequence(code, seq, 1);
						if(_processCharResult==ProcessCharResult.Unsupported)
							throw new UnknownEscapeSequenceException(String.Format("ESC {0}", new string(seq)));
					}
					catch(UnknownEscapeSequenceException ex) {
						if(GEnv.Options.WarningOption!=Poderosa.Config.WarningOption.Ignore)
							GEnv.InterThreadUIService.UnsupportedEscapeSequence(GetDocument(), GEnv.Strings.GetString("Message.EscapesequenceTerminal.UnsupportedSequence")+ex.Message);
					}
					finally {
						_escapeSequence.Remove(0, _escapeSequence.Length);
					}
				}
				else
					_processCharResult = ProcessCharResult.Escaping;
			}
		}

		protected virtual ProcessCharResult ProcessControlChar(char ch) {
			if(ch=='\n' || ch==0xB) { //Vertical TabはLFと等しい
				LineFeedRule rule = GetConnection().Param.LineFeedRule;
				if(rule==LineFeedRule.Normal || rule==LineFeedRule.LFOnly) {
					if(rule==LineFeedRule.LFOnly) //LFのみの動作であるとき
						DoCarriageReturn();
					DoLineFeed();
				}
				return ProcessCharResult.Processed;
			}
			else if(ch=='\r') {
				LineFeedRule rule = GetConnection().Param.LineFeedRule;
				if(rule==LineFeedRule.Normal || rule==LineFeedRule.CROnly) {
					DoCarriageReturn();
					if(rule==LineFeedRule.CROnly)
						DoLineFeed();
				}
				return ProcessCharResult.Processed;
			}
			else if(ch==0x07) {
				_tag.Receiver.IndicateBell();
				return ProcessCharResult.Processed;
			}
			else if(ch==0x08) {
				//行頭で、直前行の末尾が継続であった場合行を戻す
				if(_manipulator.CaretColumn==0) {
					TerminalDocument doc = GetDocument();
					int line = doc.CurrentLineNumber-1;
					if(line>=0 && doc.FindLineOrEdge(line).EOLType==EOLType.Continue) {
						doc.InvalidateLine(doc.CurrentLineNumber);
						doc.CurrentLineNumber = line;
						if(doc.CurrentLine==null)
							_manipulator.Clear(GetConnection().TerminalWidth);
						else
							_manipulator.Load(doc.CurrentLine, doc.CurrentLine.CharLength-1);
						doc.InvalidateLine(doc.CurrentLineNumber);
					}
				}
				else
					_manipulator.BackCaret();

				return ProcessCharResult.Processed;
			}
			else if(ch==0x09) {
				_manipulator.CaretColumn = GetNextTabStop(_manipulator.CaretColumn);
				return ProcessCharResult.Processed;
			}
			else if(ch==0x0E) {
				return ProcessCharResult.Processed; //以下２つはCharDecoderの中で処理されているはずなので無視
			}
			else if(ch==0x0F) {
				return ProcessCharResult.Processed;
			}
			else if(ch==0x00) {
				return ProcessCharResult.Processed; //null charは無視 !!CR NULをCR LFとみなす仕様があるが、CR LF CR NULとくることもあって難しい
			}
			else {
				//Debug.WriteLine("Unknown char " + (int)ch);
				//適当なグラフィック表示ほしい
				return ProcessCharResult.Unsupported;
			}
		}
		private void DoLineFeed() {
			GLine nl = _manipulator.Export();
			nl.EOLType = (nl.EOLType==EOLType.CR || nl.EOLType==EOLType.CRLF)? EOLType.CRLF : EOLType.LF;
			_logger.WriteLine(nl); //ログに行をcommit
			GetDocument().ReplaceCurrentLine(nl);
			GetDocument().LineFeed();
				
			//カラム保持は必要。サンプル:linuxconf.log
			int col = _manipulator.CaretColumn;
			_manipulator.Load(GetDocument().CurrentLine, col);
		}
		private void DoCarriageReturn() {
			_manipulator.CarriageReturn();
		}

		protected virtual int GetNextTabStop(int start) {
			int t = start;
			//tよりで最小の８の倍数へもっていく
			t += (8 - t % 8);
			if(t >= _tag.Connection.TerminalWidth) t = _tag.Connection.TerminalWidth-1;
			return t;
		}
		
		protected virtual ProcessCharResult ProcessNormalChar(char ch) {
			//既に画面右端にキャレットがあるのに文字が来たら改行をする
			int tw = _tag.Connection.TerminalWidth;
			if(_manipulator.CaretColumn+GLine.CalcDisplayLength(ch) > tw) {
				GLine l = _manipulator.Export();
				l.EOLType = EOLType.Continue;
				GetDocument().ReplaceCurrentLine(l);
				GetDocument().LineFeed();
				_manipulator.Load(GetDocument().CurrentLine, 0);
			}

			//画面のリサイズがあったときは、_manipulatorのバッファサイズが不足の可能性がある
			if(tw > _manipulator.BufferSize)
				_manipulator.ExpandBuffer(tw);

			//通常文字の処理
			_manipulator.PutChar(ch, _currentdecoration);
			
			return ProcessCharResult.Processed;
		}

		protected abstract ProcessCharResult ProcessEscapeSequence(char code, char[] seq, int offset);

		//FormatExceptionのほかにOverflowExceptionの可能性もあるので
		protected static int ParseInt(string param, int default_value) {
			try {
				if(param.Length>0)
					return Int32.Parse(param);
				else
					return default_value;
			}
			catch(Exception ex) {
				throw new UnknownEscapeSequenceException(String.Format("bad number format [{0}] : {1}", param, ex.Message));
			}
		}

		protected static IntPair ParseIntPair(string param, int default_first, int default_second) {
			IntPair ret = new IntPair(default_first, default_second);

			string[] s = param.Split(';');
			
			if(s.Length >= 1 && s[0].Length>0) {
				try {
					ret.first = Int32.Parse(s[0]);
				}
				catch(Exception ex) {
					throw new UnknownEscapeSequenceException(String.Format("bad number format [{0}] : {1}", s[0], ex.Message));
				}
			}

			if(s.Length >= 2 && s[1].Length>0) {
				try {
					ret.second = Int32.Parse(s[1]);
				}
				catch(Exception ex) {
					throw new UnknownEscapeSequenceException(String.Format("bad number format [{0}] : {1}", s[1], ex.Message));
				}
			}

			return ret;
		}
	}
}
