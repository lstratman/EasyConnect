/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: Log.cs,v 1.2 2005/04/20 08:45:46 okajima Exp $
*/
using System;
using System.IO;
using System.Xml;
using System.Text;

using Poderosa.ConnectionParam;
using Poderosa.Communication;
using Poderosa.Text;

namespace Poderosa.Log {
	
	public interface ITerminalLogger {
		void Comment(string comment);
		void Flush();
		void Close();
		bool IsActive { get; }
	}

	public interface ITerminalTextLogger : ITerminalLogger {
		void Append(char ch);
		void Append(char[] ch);
		void Append(char[] ch, int offset, int length);
		void BeginEscapeSequence();
		void CommitEscapeSequence();
		void AbortEscapeSequence();
		void PacketDelimiter();
		void TerminalResized(int width, int height);

		//実のところ標準タイプのみ有効
		void WriteLine(GLine line);
	}

	public interface ITerminalBinaryLogger : ITerminalLogger {
		void Append(byte[] data, int offset, int length);
	}

	internal abstract class TerminalLoggerBase : ITerminalTextLogger {

		private StringBuilder _escapesequence;

		public void Append(char[] ch) {
			Append(ch, 0, ch.Length);
		}
		public void Append(char[] ch, int offset, int length) {
			for(int i=0; i<length; i++) {
				Append(ch[offset+i]);
			}
		}
		public void Append(char ch) {
			if(_escapesequence==null)
				WriteChar(ch);
			else
				_escapesequence.Append(ch);
		}
		
		//ITerminalLoggerではなくTerminalLoggerBase独自のメソッド
		public abstract void WriteChar(char ch);
		public abstract void WriteEscapeSequence(string seq);

		//!!行単位の書き込み 実は標準とそれ以外ではログの扱い方に共通点があまりないのに強引に一つのクラスにまとめようとしてボロがでた
		public virtual void WriteLine(GLine line) {} 

		public void BeginEscapeSequence() {
			if(_escapesequence!=null) throw new InvalidOperationException("duplicated BeginEscapeSequence");

			_escapesequence = new StringBuilder();
		}
		public void CommitEscapeSequence() {
			WriteEscapeSequence(_escapesequence.ToString());
			_escapesequence = null;
		}
		public void AbortEscapeSequence() {
			_escapesequence = null;
		}
		public abstract void Flush();
		public abstract void Close();
		public abstract void Comment(string comment);
		public virtual void PacketDelimiter() {}
		public virtual void TerminalResized(int width, int height) {}
		public virtual bool IsActive { get { return true; } } //Activeを返すのがデフォルトの挙動
	}

	internal class NullTextLogger : ITerminalTextLogger {
		public void Append(char ch) {}
		public void Append(char[] ch) {}
		public void Append(char[] ch, int offset, int length) {}
		public void BeginEscapeSequence() {}
		public void CommitEscapeSequence() {}
		public void AbortEscapeSequence() {}
		public void Flush() {}
		public void Close() {}
		public void Comment(string comment) {}
		public bool IsActive {
			get {
				return false;
			}
		}
		public void PacketDelimiter() {}
		public void TerminalResized(int width, int height) {}
		public void WriteLine(GLine line) {}
	}
	internal class NullBinaryLogger : ITerminalBinaryLogger {
		public void Append(byte[] data, int offset, int length) {}
		public void Comment(string comment) {}
		public void Flush() {}
		public void Close() {}
		public bool IsActive {
			get {
				return false;
			}
		}
	}

	internal class BinaryLogger : ITerminalBinaryLogger {
		private Stream _strm;
		
		public BinaryLogger(Stream s) {
			_strm = s;
		}
		public void Append(byte[] data, int offset, int length) {
			_strm.Write(data, offset, length);
		}
		public void Comment(string comment) {
			byte[] r = Encoding.Default.GetBytes(comment);
			_strm.Write(r,0,r.Length);
		}
		public void Flush() {
			_strm.Flush();
		}
		public void Close() {
			_strm.Close();
		}
		public bool IsActive {
			get {
				return true;
			}
		}
	}

	internal class DefaultLogger : TerminalLoggerBase {
		
		private StreamWriter _writer;

		public DefaultLogger(StreamWriter w) {
			_writer = w;
		}

		public override void WriteLine(GLine line) {
			char[] t = line.Text;
			for(int i=0; i<line.CharLength; i++) {
				char ch = t[i];
				if(ch!=GLine.WIDECHAR_PAD) _writer.Write(ch);
			}
			_writer.WriteLine();

			//_writer.WriteLine(line.Text, 0, line.CharLength);
		}

		public override void WriteChar(char ch) {
			//ignore
		}
		public override void WriteEscapeSequence(string v) {
			//ignore
		}
		public override void Flush() {
			_writer.Flush();
		}
		public override void Close() {
			_writer.Close();
		}
		public override void Comment(string comment) {
			_writer.Write(comment);
		}
	}

	internal class XmlLogger : TerminalLoggerBase {
		
		private XmlWriter _writer;

		public XmlLogger(StreamWriter w, TerminalParam p) {
			_writer = new XmlTextWriter(w);
			_writer.WriteStartDocument();
			_writer.WriteStartElement("terminal-log");

			//接続時のアトリビュートを書き込む
			_writer.WriteAttributeString("time", DateTime.Now.ToString());
		}

		public override void WriteChar(char ch) {
			switch(ch) {
				case (char)0:
					WriteSPChar("NUL");
					break;
				case (char)1:
					WriteSPChar("SOH");
					break;
				case (char)2:
					WriteSPChar("STX");
					break;
				case (char)3:
					WriteSPChar("ETX");
					break;
				case (char)4:
					WriteSPChar("EOT");
					break;
				case (char)5:
					WriteSPChar("ENQ");
					break;
				case (char)6:
					WriteSPChar("ACK");
					break;
				case (char)7:
					WriteSPChar("BEL");
					break;
				case (char)8:
					WriteSPChar("BS");
					break;
				case (char)11:
					WriteSPChar("VT");
					break;
				case (char)12:
					WriteSPChar("FF");
					break;
				case (char)14:
					WriteSPChar("SO");
					break;
				case (char)15:
					WriteSPChar("SI");
					break;
				case (char)16:
					WriteSPChar("DLE");
					break;
				case (char)17:
					WriteSPChar("DC1");
					break;
				case (char)18:
					WriteSPChar("DC2");
					break;
				case (char)19:
					WriteSPChar("DC3");
					break;
				case (char)20:
					WriteSPChar("DC4");
					break;
				case (char)21:
					WriteSPChar("NAK");
					break;
				case (char)22:
					WriteSPChar("SYN");
					break;
				case (char)23:
					WriteSPChar("ETB");
					break;
				case (char)24:
					WriteSPChar("CAN");
					break;
				case (char)25:
					WriteSPChar("EM");
					break;
				case (char)26:
					WriteSPChar("SUB");
					break;
				case (char)27:
					WriteSPChar("ESC");
					break;
				case (char)28:
					WriteSPChar("FS");
					break;
				case (char)29:
					WriteSPChar("GS");
					break;
				case (char)30:
					WriteSPChar("RS");
					break;
				case (char)31:
					WriteSPChar("US");
					break;
				default:
					_writer.WriteChars(new char[1] {ch},0,1);
					break;
			}

		}
		public override void WriteEscapeSequence(string v) {
			lock(this) {
				_writer.WriteStartElement("ESC");
				_writer.WriteAttributeString("seq", v);
				_writer.WriteEndElement();
			}
		}
		public override void Flush() {
			_writer.Flush();
		}
		public override void Close() {
			_writer.WriteEndElement();
			_writer.WriteEndDocument();
			_writer.Close();
		}

		public override void Comment(string comment) {
			lock(this) {
				_writer.WriteElementString("comment", comment);
			}
		}
		public override void PacketDelimiter() {
			//WriteSPChar("EOP");
		}

		private void WriteSPChar(string name) {
			lock(this) {
				_writer.WriteElementString(name, "");
			}
		}

		//リサイズはメインスレッドから記入するので、同期しないと変なことが起こりかねない
		public override void TerminalResized(int width, int height) {
			lock(this) {
				_writer.WriteStartElement("terminal-size");
				_writer.WriteAttributeString("width", width.ToString());
				_writer.WriteAttributeString("height", height.ToString());
				_writer.WriteEndElement();
			}
		}
	}
}
