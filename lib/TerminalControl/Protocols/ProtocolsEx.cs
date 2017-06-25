/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ProtocolsEx.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using Poderosa.Forms;
using Granados;

namespace Poderosa.Protocols {

    /// <summary>
    /// <ja>
    /// 新規にターミナル接続をしたとき、それをキャンセルするためのインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to cancel it when terminal was newly connected.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインターフェイスは、<seealso cref="IProtocolService">IProtocolService</seealso>の
    /// <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnectメソッド</see>、
    /// <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnectメソッド</see>、
    /// <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnectメソッド</see>の戻り値として使われます。
    /// </ja>
    /// <en>
    /// This interface is used as a return value of the <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnect method</see> and the method of <seealso cref="IProtocolService">IProtocolService</seealso> of <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnect method</see>, <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnect method</see>. 
    /// </en>
    /// </remarks>
    public interface IInterruptable {
        /// <summary>
        /// <ja>
        /// 接続を中止します。
        /// </ja>
        /// <en>
        /// Interrupt the connection.
        /// </en>
        /// <remarks>
        /// <ja>
        /// このメソッドを呼び出すと、<seealso cref="IInterruptableConnectorClient">IInterruptableConnectorClient</seealso>に
        /// 実装したメソッドは呼び出されずに、接続が中止されます。
        /// </ja>
        /// <en>
        /// The connection is discontinued without calling the method of implementing on <seealso cref="IInterruptableConnectorClient">IInterruptableConnectorClient</seealso> when this method is called. 
        /// </en>
        /// </remarks>
        /// </summary>
        void Interrupt();
    }

    /// <summary>
    /// <ja>
    /// 新規にターミナルコネクションを非同期で作成するとき、接続の成功や失敗の状態を受け取るためのインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface to receive state of success and failure of connection when terminal connection is asynchronously newly made
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// このインターフェイスは、<seealso cref="IProtocolService">IProtocolService</seealso>の
    /// <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnectメソッド</see>、
    /// <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnectメソッド</see>、
    /// <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnectメソッド</see>を呼び出して、非同期の接続をする際、
    /// 成功や失敗の状態を受け取るために用います。
    /// </para>
    /// <para>
    /// 簡易的な同期接続をするのであれば、このインターフェイスを実装したオブジェクトを用意する代わりに、
    /// <seealso cref="IProtocolService">IProtocolService</seealso>の<see cref="IProtocolService.CreateFormBasedSynchronozedConnector">CreateFormBasedSynchronozedConnectorメソッド</see>
    /// を呼び出し、その<see cref="ISynchronizedConnector.InterruptableConnectorClient">InterruptableConnectorClientプロパティ</see>の
    /// 値を使うこともできます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// This interface is used to receive the state of the success and the failure when the <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnect method</see>, the <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnect method</see>, and the <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnect method</see> of <seealso cref="IProtocolService">IProtocolService</seealso> are called, and the asynchronous system is connected. 
    /// </para>
    /// <para>
    /// The <see cref="IProtocolService.CreateFormBasedSynchronozedConnector">CreateFormBasedSynchronozedConnector method</see> of <seealso cref="IProtocolService">IProtocolService</seealso> can be called instead of preparing the object that mounts this interface, and the value of the <see cref="ISynchronizedConnector.InterruptableConnectorClient">InterruptableConnectorClient property </see>be used if it simplicity and synchronous connects it. 
    /// </para>
    /// </en>
    /// </remarks>
    public interface IInterruptableConnectorClient {
        /// <summary>
        /// <ja>
        /// 接続が成功したときに呼び出されます。
        /// </ja>
        /// <en>
        /// Called when the connection is succeeded.
        /// </en>
        /// </summary>
        /// <param name="result"><ja>接続が完了したコネクションです。</ja><en>Connection that connection is completed.</en></param>
        /// <remarks>
        /// <ja>
        /// このメソッドが呼び出されたら接続は完了しています。以降、<paramref name="result"/>を通じてデータを送受信できます。
        /// </ja>
        /// <en>
        /// If this method is called, the connection is completed. Data can be sent and received at the following through <paramref name="result"/>. 
        /// </en>
        /// </remarks>
        void SuccessfullyExit(ITerminalConnection result);
        /// <summary>
        /// <ja>
        /// 接続が失敗したときに呼び出されます。
        /// </ja>
        /// <en>
        /// Called when the connection is failed.
        /// </en>
        /// </summary>
        /// <param name="message"><ja>失敗を告げるメッセージです。</ja><en>Message to report failure</en></param>
        void ConnectionFailed(string message);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connection"></param>
    /// <exclude/>
    public delegate void SuccessfullyExitDelegate(ITerminalConnection connection);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <exclude/>
    public delegate void ConnectionFailedDelegate(string message);

    /// <summary>
    /// <ja>
    /// 簡易的な同期接続機能のための<see cref="IInterruptableConnectorClient">IInterruptableConnectorClient</see>を提供し、
    /// 接続の完了またはエラーの発生またはタイムアウトまで、接続完了を待つ機能を提供します。
    /// </ja>
    /// <en>
    /// <see cref="IInterruptableConnectorClient">IInterruptableConnectorClient</see> for a simple synchronization and the connection functions is offered, and the function to wait for connected completion is offered until generation or the time-out of completion or the error of the connection. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// 開発者は次のようにすることで、接続が完了するまで待つことができます。
    /// </para>
    /// <code>
    /// // <value>form</value>はユーザーに表示するフォームです
    /// ISynchronizedConnector sc = protocolservice.CreateFormBasedSynchronozedConnector(<value>form</value>);
    /// // <value>sshparam</value>はSSH接続のパラメータです
    /// IInterrutable t = protocol_service.AsyncSSHConnect(sc.InterruptableConnectorClient, sshparam);
    /// // 30秒間待つ
    /// int timeout = 30 * 1000;
    /// ITerminalConnection connection = sc.WaitConnection(t, timeout);
    /// </code>
    /// </ja>
    /// <en>
    /// <para>
    /// The developer can wait until the connection is completed by doing as follows. 
    /// </para>
    /// <code>
    /// // <value>form</value> is the form that show to user.
    /// ISynchronizedConnector sc = protocolservice.CreateFormBasedSynchronozedConnector(<value>form</value>);
    /// // <value>sshparam</value> is a parameter for SSH connection.
    /// IInterrutable t = protocol_service.AsyncSSHConnect(sc.InterruptableConnectorClient, sshparam);
    /// // Wait 30second
    /// int timeout = 30 * 1000;
    /// ITerminalConnection connection = sc.WaitConnection(t, timeout);
    /// </code>
    /// </en>
    /// </remarks>
    public interface ISynchronizedConnector {
        /// <summary>
        /// <ja>
        /// 接続を待つ機能をもつ<see cref="IInterruptableConnectorClient">IInterruptableConnectorClient</see>を返します。
        /// </ja>
        /// <en>
        /// <see cref="IInterruptableConnectorClient">IInterruptableConnectorClient</see> that waits for the connection is returned. 
        /// </en>
        /// </summary>
        IInterruptableConnectorClient InterruptableConnectorClient {
            get;
        }
        /// <summary>
        /// <ja>
        /// 接続完了または接続エラーまたはタイムアウトが発生するまで待ちます。
        /// </ja>
        /// <en>
        /// It waits until connected completion or connected error or the time-out occurs. 
        /// </en>
        /// </summary>
        /// <param name="connector"><ja>接続を止めるためのインターフェイス</ja><en>Interface to stop connection</en></param>
        /// <param name="timeout"><ja>タイムアウト値（ミリ秒）。System.Threading.Timeout.Infiniteを指定して、無期限に待つこともできます。</ja><en>Time-out value (millisecond). It is possible to wait indefinitely by specifying System.Threading.Timeout.Infinite. </en></param>
        /// <returns><ja>接続が完了した<seealso cref="ITerminalConnection">ITerminalConnection</seealso>。接続に失敗したときにはnull</ja><en><seealso cref="ITerminalConnection">ITerminalConnection</seealso> that completes connection. When failing in the connection, return null. </en></returns>
        /// <remarks>
        /// <para>
        /// <ja>
        /// <paramref name="connector"/>には、<seealso cref="IProtocolService">IProtocolService</seealso>の
        /// <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnectメソッド</see>、
        /// <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnectメソッド</see>、
        /// <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnectメソッド</see>
        /// からの戻り値を渡します。
        /// </ja>
        /// <en>
        /// The return value from the <see cref="IProtocolService.AsyncCygwinConnect">AsyncCygwinConnect method</see>, the <see cref="IProtocolService.AsyncTelnetConnect">AsyncTelnetConnect method</see>, and the <see cref="IProtocolService.AsyncSSHConnect">AsyncSSHConnect method</see> of <seealso cref="IProtocolService">IProtocolService</seealso> is passed to connector. 
        /// </en>
        /// </para>
        /// </remarks>
        ITerminalConnection WaitConnection(IInterruptable connector, int timeout);
    }

    /// <summary>
    /// <ja>
    /// 新規接続機能を提供するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that offers new connection function.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインターフェイスは、Protocolsプラグイン（プラグインID「org.poderosa.protocols」）が提供します。次のようにして取得できます。
    /// <code>
    /// IProtocolService protocolservice = 
    ///  (IProtocolService)PoderosaWorld.PluginManager.FindPlugin(
    ///   "org.poderosa.protocols", typeof(IProtocolService));
    /// Debug.Assert(protocolservice != null);
    /// </code>
    /// </ja>
    /// <en>
    /// This interface is offered by Protocols plug-in (plug-in ID [org.poderosa.protocols]). It is possible to get it as follows. 
    /// <code>
    /// IProtocolService protocolservice = 
    ///  (IProtocolService)PoderosaWorld.PluginManager.FindPlugin(
    ///   "org.poderosa.protocols", typeof(IProtocolService));
    /// Debug.Assert(protocolservice != null);
    /// </code>
    /// </en>
    /// </remarks>
    public interface IProtocolService {
        /// <summary>
        /// <ja>
        /// Cygwin接続のデフォルトパラメータを格納したオブジェクトを生成します。
        /// </ja>
        /// <en>
        /// Create the object stored default parameter of Cygwin connection.
        /// </en>
        /// </summary>
        /// <returns><ja>デフォルトパラメータが格納されたオブジェクト</ja><en>Object with default parameter.</en></returns>
        ICygwinParameter CreateDefaultCygwinParameter();
        /// <summary>
        /// <ja>
        /// Telnet接続のデフォルトパラメータを格納したオブジェクトを生成します。
        /// </ja>
        /// <en>
        /// Create the object stored default parameter of telnet connection.
        /// </en>
        /// </summary>
        /// <returns><ja>デフォルトパラメータが格納されたオブジェクト</ja>
        /// <en>Object that stored default parameter.</en>
        /// </returns>
        ITCPParameter CreateDefaultTelnetParameter();
        /// <summary>
        /// <ja>
        /// SSH接続のデフォルトパラメータを格納したオブジェクトを生成します。
        /// </ja>
        /// <en>
        /// Create the object stored default parameter of SSH connection.
        /// </en>
        /// </summary>
        /// <returns><ja>デフォルトパラメータが格納されたオブジェクト</ja><en>Object with default parameter.</en></returns>
        ISSHLoginParameter CreateDefaultSSHParameter();

        /// <summary>
        /// <ja>
        /// 非同期接続でCygwin接続のターミナルコネクションを作ります。
        /// </ja>
        /// <en>
        /// The terminal connection of the Cygwin connection is made for an asynchronous connection. 
        /// </en>
        /// </summary>
        /// <param name="result_client"><ja>接続の成否を受け取るインターフェイス</ja><en>Interface that receives success or failure of connection</en></param>
        /// <param name="destination"><ja>接続時のパラメータ</ja><en>Connecting parameter.</en></param>
        /// <returns><ja>接続操作をキャンセルするためのインターフェイス</ja><en>Interface to cancel connected operation</en></returns>
        /// <remarks>
        /// <ja>
        /// このメソッドはブロックせずに、ただちに制御を戻します。接続が成功すると<paramref name="result_client"/>の
        /// <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExitメソッド</see>が呼び出されます。
        /// </ja>
        /// <en>
        /// The control is returned at once without blocking this method. When the connection succeeds, the <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExit method</see> of <paramref name="result_client"/> is called. 
        /// </en>
        /// </remarks>
        IInterruptable AsyncCygwinConnect(IInterruptableConnectorClient result_client, ICygwinParameter destination);
        /// <summary>
        /// <ja>
        /// 非同期接続でTelnet接続のターミナルコネクションを作ります。
        /// </ja>
        /// <en>
        /// Create a terminal connection of the Telnet connection by an asynchronous connection. 
        /// </en>
        /// </summary>
        /// <en>
        /// The terminal connection of the telnet connection is made for an asynchronous connection. 
        /// </en>
        /// <param name="result_client"><ja>接続の成否を受け取るインターフェイス</ja><en>Interface that receives success or failure of connection</en></param>
        /// <param name="destination"><ja>接続時のパラメータ</ja><en>Connecting parameter.</en></param>
        /// <returns><ja>接続操作をキャンセルするためのインターフェイス</ja><en>Interface to cancel connected operation</en></returns>
        /// <remarks>
        /// <ja>
        /// このメソッドはブロックせずに、ただちに制御を戻します。接続が成功すると<paramref name="result_client"/>の
        /// <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExitメソッド</see>が呼び出されます。
        /// </ja>
        /// <en>
        /// The control is returned at once without blocking this method. When the connection succeeds, the <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExit method</see> of <paramref name="result_client"/> is called. 
        /// </en>
        /// </remarks>
        IInterruptable AsyncTelnetConnect(IInterruptableConnectorClient result_client, ITCPParameter destination);
        /// <summary>
        /// <ja>
        /// 非同期接続でSSH接続のターミナルコネクションを作ります。
        /// </ja>
        /// <en>
        /// The terminal connection of the SSH connection is made for an asynchronous connection. 
        /// </en>
        /// </summary>
        /// <param name="result_client"><ja>接続の成否を受け取るインターフェイス</ja><en>Interface that receives success or failure of connection</en></param>
        /// <param name="destination"><ja>接続時のパラメータ</ja><en>Connecting parameter.</en></param>
        /// <returns><ja>接続操作をキャンセルするためのインターフェイス</ja>
        /// <en>Interface to cancel connection operation.</en>
        /// </returns>
        /// <remarks>
        /// <ja>
        /// このメソッドはブロックせずに、ただちに制御を戻します。接続が成功すると<paramref name="result_client"/>の
        /// <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExitメソッド</see>が呼び出されます。
        /// </ja>
        /// <en>
        /// The control is returned at once without blocking this method. When the connection succeeds, the <see cref="IInterruptableConnectorClient.SuccessfullyExit">SuccessfullyExit method</see> of <paramref name="result_client"/> is called. 
        /// </en>
        /// </remarks>
        IInterruptable AsyncSSHConnect(IInterruptableConnectorClient result_client, ISSHLoginParameter destination);

        /// <summary>
        /// <ja>
        /// 簡易的な同期接続機能を提供するインターフェイスを返します。
        /// </ja>
        /// <en>
        /// Return the interface that offers a simple synchronization and the connection functions.
        /// </en>
        /// </summary>
        /// <param name="form"><ja>接続時にユーザーに表示するフォーム</ja><en>Form displayed to user when connecting it</en></param>
        /// <returns><ja>同期接続機能を提供するインターフェイス</ja><en>Interface that offers synchronization and connection functions</en></returns>
        ISynchronizedConnector CreateFormBasedSynchronozedConnector(IPoderosaForm form);

        /// <summary>
        /// <ja>
        /// プロトコルのオプションを示すインターフェイスです。
        /// </ja>
        /// <en>
        /// Interface that shows option of protocol
        /// </en>
        /// </summary>
        IProtocolOptions ProtocolOptions {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        IPassphraseCache PassphraseCache {
            get;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IPassphraseCache {
        void Add(string host, string account, string passphrase);
        string GetOrEmpty(string host, string account);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ISSHConnectionChecker {
        //SSH接続を張るときに介入するためのインタフェース　AgentForwarding用に導入したものだが拡張するかも
        void BeforeNewConnection(SSHConnectionParameter cp);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IProtocolTestService {
        ITerminalConnection CreateLoopbackConnection();
    }

}
