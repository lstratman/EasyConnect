/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: XmlLogger.cs,v 1.3 2011/12/19 17:14:35 kzmi Exp $
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Poderosa.Document;
using Poderosa.Protocols;
using Poderosa.Util;
using Poderosa.ConnectionParam;

//うろ覚えだが、参照先アセンブリの参照は１つのソースファイル単位だったような気がするので、
//System.Xml.dllの読み込みを極力遅らせるためにファイルを分離

namespace Poderosa.Terminal {
    internal class XmlLogger : LoggerBase, IXmlLogger {

        private readonly XmlWriter _writer;
        private readonly object _sync = new object();
        private readonly char[] _buffer;
        private bool _closed = false;

        public XmlLogger(ISimpleLogSettings log, StreamWriter w)
            : base(log) {
            _writer = new XmlTextWriter(w);
            _writer.WriteStartDocument();
            _writer.WriteStartElement("terminal-log");

            //接続時のアトリビュートを書き込む
            _writer.WriteAttributeString("time", DateTime.Now.ToString());
            _buffer = new char[1];
        }

        public void Write(char ch) {
            lock (_sync) {
                if (_closed)
                    return;

                switch (ch) {
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
                        _buffer[0] = ch;
                        _writer.WriteChars(_buffer, 0, 1);
                        break;
                }

                Wrote();
            }
        }

        public void EscapeSequence(char[] body) {
            lock (_sync) {
                if (!_closed) {
                    _writer.WriteStartElement("ESC");
                    _writer.WriteAttributeString("seq", new string(body));
                    _writer.WriteEndElement();

                    Wrote();
                }
            }
        }

        public override void Flush() {
            // note that Flush() may be called by AutoFlush()
            // even if output stream has been already closed.
            lock (_sync) {
                if (!_closed) {
                    _writer.Flush();
                }
            }
        }

        public void Close() {
            lock (_sync) {
                if (!_closed) {
                    _writer.WriteEndElement();
                    _writer.WriteEndDocument();
                    _writer.Close();
                    _closed = true;
                }
            }
        }

        public void Comment(string comment) {
            lock (_sync) {
                if (!_closed) {
                    _writer.WriteElementString("comment", comment);
                    Wrote();
                }
            }
        }

        private void WriteSPChar(string name) {
            _writer.WriteElementString(name, "");
        }
    }
}
