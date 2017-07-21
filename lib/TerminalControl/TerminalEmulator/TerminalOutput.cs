/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalOutput.cs,v 1.5 2012/02/25 04:47:05 kzmi Exp $
 */
using System;
using System.Resources;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.Protocols;
using Poderosa.Forms;

namespace Poderosa.Terminal {
    //もとTerminalControlとAbstractTerminalにごちゃごちゃしていた送信機能を抜き出し
    /// <summary>
    /// <ja>
    /// ターミナルへと送信する機能を提供します。
    /// </ja>
    /// <en>
    /// Offer the function to transmit to the terminal.
    /// </en>
    /// </summary>
    public class TerminalTransmission {
        private AbstractTerminal _host;
        private ITerminalSettings _settings;
        private ITerminalConnection _connection;
        private ByteDataFragment _dataForLocalEcho;
        private readonly object _transmitSync = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="settings"></param>
        /// <param name="connection"></param>
        /// <exclude/>
        public TerminalTransmission(AbstractTerminal host, ITerminalSettings settings, ITerminalConnection connection) {
            _host = host;
            _settings = settings;
            _connection = connection;
            _dataForLocalEcho = new ByteDataFragment();
        }

        /// <summary>
        /// <ja>
        /// ターミナルのコネクションを示します。
        /// </ja>
        /// <en>
        /// Show the connection of terminal.
        /// </en>
        /// </summary>
        public ITerminalConnection Connection {
            get {
                return _connection;
            }
        }

        //改行は入っていない前提で
        /// <summary>
        /// <ja>
        /// Char型の配列を送信します。
        /// </ja>
        /// <en>
        /// Send a array of Char type.
        /// </en>
        /// </summary>
        /// <param name="chars"><ja>送信する文字配列</ja><en>String array to send</en></param>
        /// <remarks>
        /// <ja>
        /// 文字は現在のエンコード設定によりエンコードされてから送信されます。
        /// </ja>
        /// <en>
        /// After it is encoded by a present encode setting, the character is transmitted. 
        /// </en>
        /// </remarks>
        public void SendString(char[] chars) {
            byte[] data = EncodingProfile.Get(_settings.Encoding).GetBytes(chars);
            Transmit(data);
        }
        /// <summary>
        /// <ja>
        /// 改行を送信します。
        /// </ja>
        /// <en>
        /// Transmit line feed.
        /// </en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// 実際に送るデータは改行設定により、「CR」「LF」「CR+LF」のいずれかになります。
        /// </ja>
        /// <en>
        /// The data actually sent becomes either of "CR" "LF" "CR+LF" by the changing line setting. 
        /// </en>
        /// </remarks>
        public void SendLineBreak() {
            byte[] t = TerminalUtil.NewLineBytes(_settings.TransmitNL);
            Transmit(t);
        }
        /// <summary>
        /// <ja>
        /// ターミナルのサイズを変更します。
        /// </ja>
        /// <en>
        /// Change terminal size.
        /// </en>
        /// </summary>
        /// <param name="width"><ja>ターミナルの幅</ja><en>Width of terminal.</en></param>
        /// <param name="height"><ja>ターミナルの高さ</ja><en>Height of terminal</en></param>
        public void Resize(int width, int height) {
            //TODO Transmit()と同様のtry...catch
            if (_connection.TerminalOutput != null) //keyboard-interactive認証中など、サイズ変更できない局面もある
                _connection.TerminalOutput.Resize(width, height);
        }

        /// <summary>
        /// <ja>
        /// バイト配列を送信します。
        /// </ja>
        /// <en>
        /// Send array of byte.
        /// </en>
        /// </summary>
        /// <param name="data"><ja>送信するデータが格納されたバイト配列</ja><en>Byte array that contains data to send.</en></param>
        public void Transmit(byte[] data) {
            TransmitInternal(data, 0, data.Length, true);
        }

        /// <summary>
        /// Sends bytes. Data may be repeated as local echo.
        /// </summary>
        /// <param name="data">Byte array that contains data to send.</param>
        /// <param name="offset">Offset in data</param>
        /// <param name="length">Length of bytes to transmit</param>
        internal void Transmit(byte[] data, int offset, int length) {
            TransmitInternal(data, offset, length, true);
        }

        /// <summary>
        /// Sends bytes without local echo.
        /// </summary>
        /// <param name="data">Byte array that contains data to send.</param>
        /// <param name="offset">Offset in data</param>
        /// <param name="length">Length of bytes to transmit</param>
        internal void TransmitDirect(byte[] data, int offset, int length) {
            TransmitInternal(data, offset, length, false);
        }

        /// <summary>
        /// Sends bytes.
        /// </summary>
        /// <param name="data">Byte array that contains data to send.</param>
        /// <param name="offset">Offset in data</param>
        /// <param name="length">Length of bytes to transmit</param>
        /// <param name="localEcho">Whether bytes can be repeated as local echo</param>
        private void TransmitInternal(byte[] data, int offset, int length, bool localEcho) {
            // Note:
            //  This method may be called from multiple threads.
            //  One is UI thread which is processing key events, and another is communication thread
            //  which is going to send back something.
            //
            //  Some IPoderosaSocket implementations have thread-safe Transmit() method, but not all.
            //  So we transmit data exclusively.
            lock (_transmitSync) {
                try {
                    if (localEcho && _settings.LocalEcho) {
                        _dataForLocalEcho.Set(data, 0, data.Length);
                        _host.OnReception(_dataForLocalEcho);
                    }
                    _connection.Socket.Transmit(data, offset, length);
                }
                catch (Exception) {
                    try {
                        _connection.Close();
                    }
                    catch (Exception ex) {
                        RuntimeUtil.ReportException(ex);
                    }

                    _host.TerminalHost.OwnerWindow.Warning(GEnv.Strings.GetString("Message.TerminalControl.FailedToSend"));
                }
            }
        }

        //主にPaste用複数行送信。終了後クローズ
        /// <summary>
        /// <ja>
        /// TextStreamから読み取ったデータを送信します。
        /// </ja>
        /// <en>
        /// Transmit the data read from TextStream.
        /// </en>
        /// </summary>
        /// <param name="reader"><ja>読み取るTextStream</ja><en>Read TextStream</en></param>
        /// <param name="send_linebreak_last"><ja>最後に改行を付けるかどうかを指定するフラグ。trueのとき、最後に改行が付与されます。</ja><en>Flag that specifies whether to put changing line at the end. Line feed is given at the end at true. </en></param>
        /// <remarks>
        /// <para>
        /// <ja>データは現在のエンコード設定により、エンコードされてから送信されます。</ja><en>After it is encoded by a present encode setting, data is transmitted. </en>
        /// </para>
        /// <para>
        /// <ja><paramref name="reader"/>はデータの送信後に閉じられます（Closeメソッドが呼び出されます）。</ja><en>After data is transmitted, <paramref name="reader"/> is closed (The Close method is called). </en>
        /// </para>
        /// </remarks>
        public void SendTextStream(TextReader reader, bool send_linebreak_last) {
            string line = reader.ReadLine();
            while (line != null) {
                SendString(line.ToCharArray());

                //つづきの行があるならば、改行は必ず送る。最終行であるならば、それが改行文字で終わっている場合のみ改行を送る。
                //送る改行はクリップボードの内容に関わらずターミナルの設定に基づくことに注意
                bool last = reader.Peek() == -1;
                bool linebreak = last ? send_linebreak_last : true;
                if (linebreak)
                    SendLineBreak();

                line = reader.ReadLine();
            }
            reader.Close();
        }

        //復活
        /// <exclude/>
        public void Revive(ITerminalConnection connection, int terminal_width, int terminal_height) {
            _connection = connection;
            Resize(terminal_width, terminal_height);
        }

    }

}
