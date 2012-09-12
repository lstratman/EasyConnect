/*
* Copyright (c) 2005 Poderosa Project, All Rights Reserved.
* $Id: TestUtil.cs,v 1.2 2005/04/20 08:45:48 okajima Exp $
*/
using System;
using System.Xml;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

using Poderosa.Text;
using Poderosa.Terminal;
using Poderosa.Connection;

//Debug namespaceだとSystem.Diagnostics.Debugとコンパイラが混同してしまう
namespace Poderosa.Debugging
{
	/// <summary>
	/// テストに必要な各種のメソッド収録
	/// </summary>
	public class TestUtil
	{
		public static void EmulateWithLog(string filename, ConnectionTag tag) {
			_filename = filename;
			_tag = tag;
			//XmlTextReader r = new XmlTextReader(filename);
			//EmulateWithLog(r, tag);
			//r.Close();
			Thread th = new Thread(new ThreadStart(Run));
			th.Start();

		}

		private static string _filename;
		private static ConnectionTag _tag;
		private static void Run() {
			XmlTextReader r = new XmlTextReader(_filename);
			lock(_tag.Document) {
				EmulateWithLog(r, _tag);
			}
			r.Close();
		}
		

		//デバッグ用に、外部のXML形式ログを読んでエミュレートをする
		public static void EmulateWithLog(XmlReader reader, ConnectionTag tag) {
			ITerminal term = tag.Terminal;
			TerminalDocument doc = tag.Document;
			StringBuilder buf = new StringBuilder();
			reader.ReadStartElement("terminal-log");
			try {
				do {
					if(reader.NodeType==XmlNodeType.Text || reader.NodeType==XmlNodeType.Whitespace)
						buf.Append(reader.Value);
					else if(reader.NodeType==XmlNodeType.Element) {
						if(reader.Name=="ESC") {
							buf.Append((char)0x1B);
							buf.Append(reader.GetAttribute("seq"));
						}
						else if(reader.Name=="BS")
							buf.Append((char)0x8);
						else if(reader.Name=="BEL")
							buf.Append((char)0x7);
						else if(reader.Name=="dump") {
							buf = Flush(tag, buf);
							doc.Dump(reader.GetAttribute("title"));
						}
						else if(reader.Name=="comment") {
							buf = Flush(tag, buf);
							while(reader.NodeType!=XmlNodeType.EndElement || reader.Name!="comment") {
								reader.Read();
								if(reader.NodeType==XmlNodeType.Text)
									GEnv.InterThreadUIService.Warning(doc, reader.Value);
							}
						}
						else if(reader.Name=="break") {
							Debug.WriteLine("BREAK "+reader.GetAttribute("title"));
							Debugger.Break();
							buf = Flush(tag, buf);
						}
						else if(reader.Name=="PD") {
							buf = Flush(tag, buf);
						}
						else if(reader.Name=="pause") {
							buf = Flush(tag, buf);
							GEnv.InterThreadUIService.Warning(doc, reader.GetAttribute("title")); 
						}
						else if(reader.Name!="SI" && reader.Name!="SO" && reader.Name!="NUL" && reader.Name!="terminal-size")
							Debug.WriteLine("Unsupported element "+reader.Name);
					}
					else if(reader.NodeType==XmlNodeType.EndElement) {
						if(reader.Name=="terminal-log") {
							Flush(tag, buf);
						}
					}
				} while(reader.Read());
			}
			catch(Exception ex) {
				Debug.WriteLine(ex.Message);
				Debug.WriteLine(ex.StackTrace);
			}
		}

		private static StringBuilder Flush(ConnectionTag tag, StringBuilder buf) {
			char[] data = buf.ToString().ToCharArray();
			tag.Terminal.Input(data, 0, data.Length);
			tag.Pane.DataArrived();

			return new StringBuilder();
		}

	}

	internal class DebugUtil {
		public static string DumpByteArray(byte[] data) {
			return DumpByteArray(data, 0, data.Length);
		}
		public static string DumpByteArray(byte[] data, int offset, int length) {
			StringBuilder bld = new StringBuilder();
			for(int i=0; i<length; i++) {
				bld.Append(data[offset+i].ToString("X2"));
				if((i % 4)==3) bld.Append(' ');
			}
			return bld.ToString();
		}
	}
}
