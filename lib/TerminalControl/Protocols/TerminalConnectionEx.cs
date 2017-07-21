/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalConnectionEx.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Poderosa.Protocols {
    /// <summary>
    /// <ja>
    /// 通信するためのソケットとなるインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to became a  socket to connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>このインターフェイスは、接続を示す<seealso cref="ITerminalConnection">ITerminalConnection</seealso>の<see cref="ITerminalConnection.Socket">Socketプロパティ</see>として取得できます。</ja><en>This interface can be got <see cref="ITerminalConnection.Socket">Socket property</see> that show connection on <seealso cref="ITerminalConnection">ITerminalConnection</seealso>.</en>
    /// </remarks>
    public interface IPoderosaSocket : IByteOutputStream {
        /// <summary>
        /// <ja>
        /// データを受信するためのインターフェイスを登録します。
        /// </ja>
        /// <en>
        /// Regist the interface to recieve data.
        /// </en>
        /// </summary>
        /// <param name="receiver"><ja>データを受信するときに呼び出すインターフェイス</ja><en>Interface called when recieve the data.</en></param>
        /// <remarks>
        /// <ja>
        /// このメソッドは、複数回呼び出して、多数のインターフェイスを登録することはできません。また登録したインターフェイスを解除する方法も
        /// 用意されていません。
        /// </ja>
        /// <en>
        /// This method cannot register a lot of interfaces by calling it two or more times. Moreover, the method of releasing the registered interface is not prepared. 
        /// </en>
        /// </remarks>
        void RepeatAsyncRead(IByteAsyncInputStream receiver);
        /// <summary>
        /// <ja>
        /// データを受信することができるかどうかを示します。falseのときにはデータを受信できません。
        /// </ja>
        /// <en>
        /// It shows whether to receive the data. At false, it is not possible to receive the data. 
        /// </en>
        /// </summary>
        bool Available {
            get;
        }
        /// <summary>
        /// <ja>
        /// 最終的なクリーンアップをします。ソケットAPIにはDisconnect, Shutdown, Close等がありますがそれによらずに完全な破棄を実行します。
        /// </ja>
        /// <en>
        /// A final cleanup is done. A complete annulment is executed without depending on it though socket API includes Disconnect, Shutdown, and Close, etc.
        /// </en>
        /// </summary>
        void ForceDisposed();
    }

    //端末としての出力。旧TerminalConnectionのいくつかのメソッドを抜き出した
    /// <summary>
    /// <ja>
    /// 端末固有のデータを出力する機能を提供します。
    /// </ja>
    /// <en>
    /// Offer the function to output peculiar data to the terminal.
    /// </en>
    /// </summary>
    public interface ITerminalOutput {
        /// <summary>
        /// <ja>
        /// ブレーク信号を送信します。
        /// </ja>
        /// <en>
        /// Send break.
        /// </en>
        /// </summary>
        void SendBreak();
        /// <summary>
        /// <ja>
        /// キープアライブデータを送信します。
        /// </ja>
        /// <en>
        /// Send keep alive data.
        /// </en>
        /// </summary>
        void SendKeepAliveData();
        /// <summary>
        /// <ja>
        /// AreYouThereを送信します。
        /// </ja>
        /// <en>
        /// Send AreYouThere.
        /// </en>
        /// </summary>
        void AreYouThere(); //Telnet onlyかもよ
        /// <summary>
        /// <ja>
        /// 端末のサイズを変更するコマンドを送信します。
        /// </ja>
        /// <en>
        /// Send the command to which the size of the terminal is changed.
        /// </en>
        /// </summary>
        /// <param name="width"><ja>変更後の幅（文字単位）</ja><en>Width after it changes(unit of character)</en></param>
        /// <param name="height"><ja>変更後の高さ（文字単位）</ja><en>Height after it changes(unit of character)</en></param>
        void Resize(int width, int height);
    }

    /// <summary>
    /// <ja>
    /// ターミナルコネクションを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that show the terminal connection.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインターフェイスは、<seealso cref="Poderosa.Sessions.ITerminalSession">ITerminalSession</seealso>の
    /// <see cref="Poderosa.Sessions.ITerminalSession.TerminalConnection">TerminalConnectionプロパティで</see>
    /// 取得できます。
    /// </ja>
    /// <en>
    /// This interface can be got in the <see cref="Poderosa.Sessions.ITerminalSession.TerminalConnection">TerminalConnection property</see> of <seealso cref="Poderosa.Sessions.ITerminalSession">ITerminalSession</seealso>. 
    /// </en>
    /// </remarks>
    public interface ITerminalConnection : IAdaptable {
        /// <summary>
        /// <ja>
        /// 接続先情報を示すインターフェイスです。
        /// </ja>
        /// <en>
        /// Interface that show the connection information.
        /// </en>
        /// </summary>
        ITerminalParameter Destination {
            get;
        }
        /// <summary>
        /// <ja>
        /// ブレーク信号の送信やAreYouThere、
        /// ターミナルサイズ変更の通知など、ターミナルに特殊制御するメソッドをもつITerminalOutputです。
        /// </ja>
        /// <en>
        /// It is ITerminalOutput with the method of the special control in terminals of the transmission of the break, AreYouThere, and the notification of the change of the size of the terminal, etc.
        /// </en>
        /// </summary>
        ITerminalOutput TerminalOutput {
            get;
        }
        /// <summary>
        /// <ja>
        /// ターミナルへの送受信機能をもつIPoderosaSocketです。
        /// </ja>
        /// <en>
        /// IPoderosaSocket with transmitting and receiving function to terminal.
        /// </en>
        /// </summary>
        IPoderosaSocket Socket {
            get;
        }
        /// <summary>
        /// <ja>
        /// 接続が閉じているかどうかを示します。trueのとき接続は閉じています。
        /// </ja>
        /// <en>
        /// It is shown whether the connection closes. The connection close at true. 
        /// </en>
        /// </summary>
        bool IsClosed {
            get;
        }

        /// <summary>
        /// <ja>接続を閉じます。</ja>
        /// <en>Close the connection.</en>
        /// </summary>
        /// <remarks>
        /// <ja>
        /// このコネクションがターミナルセッションとして使われている場合には、直接このメソッドを呼び出さず、
        /// ターミナルセッション側から切断してください。
        /// </ja>
        /// <en>
        /// Please do not call this method directly when this connection is used as a terminal session, and cut it from the terminal session side. 
        /// </en>
        /// </remarks>
        void Close();

    }
}
