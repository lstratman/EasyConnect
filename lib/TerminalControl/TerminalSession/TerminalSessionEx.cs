/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSessionEx.cs,v 1.2 2011/10/27 23:21:59 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

using Poderosa.Commands;
using Poderosa.Protocols;
using Poderosa.Terminal;
using Poderosa.Forms;

namespace Poderosa.Sessions {

    //ターミナル接続のセッション
    //  TerminalEmulatorプラグイン内のコマンドは、CommandTarget->View->Document->Session->TerminalSession->Terminalと辿ってコマンド実行対象を得る。
    /// <summary>
    /// <ja>
    /// ターミナルセッションを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that show the terminal session.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// Poderosaを標準のターミナルエミュレータとして用いる場合には、<seealso cref="ISession">ISession</seealso>の実態は、
    /// このITerminalSessionであり、GetAdapterを使って取得できます。
    /// </para>
    /// <para>
    /// <para>
    /// ターミナルセッションは、次の方法で取得できます。
    /// </para>
    /// <list type="number">
    /// <item>
    /// <term>アクティブなウィンドウ／ビューから取得する場合</term>
    /// <description>
    /// <para>
    /// ウィンドウマネージャのActiveWindowプロパティは、アクティブウィンドウを示します。
    /// このアクティブウィンドウからドキュメント、そして、セッションへとたどることでターミナルセッションを取得できます。 
    /// </para>
    /// <code>
    /// // ウィンドウマネージャを取得
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// IWindowManager wm = cs.WindowManager;
    ///
    /// // アクティブウィンドウを取得
    /// IPoderosaMainWindow window = wm.ActiveWindow;
    ///
    /// // ビューを取得
    /// IPoderosaView view = window.LastActivatedView;
    /// 
    /// // ドキュメントを取得
    /// IPoderosaDocument doc = view.Document;
    /// 
    /// // セッションを取得
    /// ISession session = doc.OwnerSession;
    /// 
    /// // ターミナルセッションへと変換
    /// ITerminalSession termsession = 
    ///   (ITerminalSession)session.GetAdapter(typeof(ITerminalSession));
    /// </code>
    /// </description>
    /// </item>
    /// <item><term>メニューやツールバーのターゲットからターミナルセッションを得る場合</term>
    /// <description>
    /// <para>
    /// メニューやツールバーからコマンドが引き渡されるときには、ターゲットとして操作対象のウィンドウが得られます。
    /// このターゲットを利用してターミナルセッションを得ることができます。 
    /// </para>
    /// <para>
    /// <seealso cref="CommandTargetUtil">CommandTargetUtil</seealso>には、アクティブなドキュメントを得るためのAsDocumentOrViewOrLastActivatedDocumentメソッドがあります。
    /// このメソッドを使ってドキュメントを取得し、そこからITerminalSessionへと変換することで、ターゲットになっているターミナルセッションを取得できます。 
    /// </para>
    /// <code>
    /// // targetはコマンドに渡されたターゲットであると想定します
    /// // ドキュメントを取得
    /// IPoderosaDocument doc = 
    ///   CommandTargetUtil.AsDocumentOrViewOrLastActivatedDocument(target);
    /// if (doc != null)
    /// {
    ///   // セッションを取得
    ///   ISession session = doc.OwnerSession;
    ///   // ターミナルセッションへと変換
    ///   ITerminalSession termsession = 
    ///     (ITerminalSession)session.GetAdapter(typeof(ITerminalSession));
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// The realities of <seealso cref="ISession">ISession</seealso> are these ITerminalSession when Poderosa is used as a standard terminal emulator, and it is possible to acquire it by using GetAdapter. 
    /// </para>
    /// <para>
    /// <para>
    /// The terminal session can be got in the following method. 
    /// </para>
    /// <list type="number">
    /// <item>
    /// <term>Get from active window or view.</term>
    /// <description>
    /// <para>
    /// Window manager's ActiveWindow property shows the active window. The terminal session can be got by tracing it from this active window to the document and the session. 
    /// </para>
    /// <code>
    /// // Get the window manager.
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// IWindowManager wm = cs.WindowManager;
    ///
    /// // Get the active window.
    /// IPoderosaMainWindow window = wm.ActiveWindow;
    ///
    /// // Get the view.
    /// IPoderosaView view = window.LastActivatedView;
    /// 
    /// // Get the document.
    /// IPoderosaDocument doc = view.Document;
    /// 
    /// // Get the session
    /// ISession session = doc.OwnerSession;
    /// 
    /// // Convert to terminal session.
    /// ITerminalSession termsession = 
    ///   (ITerminalSession)session.GetAdapter(typeof(ITerminalSession));
    /// </code>
    /// </description>
    /// </item>
    /// <item><term>Get the terminal session from the target of menu or toolbar.</term>
    /// <description>
    /// <para>
    /// When the command is handed over from the menu and the toolbar, the window to be operated as a target is obtained. 
    /// The terminal session can be obtained by using this target. 
    /// </para>
    /// <para>
    /// In <seealso cref="CommandTargetUtil">CommandTargetUtil</seealso>, there is AsDocumentOrViewOrLastActivatedDocument method to obtain an active document. 
    /// </para>
    /// <code>
    /// // It is assumed that target is a target passed to the command. 
    /// // Get the document.
    /// IPoderosaDocument doc = 
    ///   CommandTargetUtil.AsDocumentOrViewOrLastActivatedDocument(target);
    /// if (doc != null)
    /// {
    ///   // Get the session.
    ///   ISession session = doc.OwnerSession;
    ///   // Convert to terminal session.
    ///   ITerminalSession termsession = 
    ///     (ITerminalSession)session.GetAdapter(typeof(ITerminalSession));
    /// }
    /// </code>
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </en>
    /// </remarks>
    public interface ITerminalSession : ISession {
        /// <summary>
        /// <ja>ターミナルを管理するオブジェクトです。</ja>
        /// <en>Object that manages terminal.</en>
        /// </summary>
        /// <remarks>
        /// <ja>このオブジェクトは、送受信をフックしたい場合やログをとりたい場合などに用います。</ja><en>This object uses transmitting and receiving to hook and to take the log. </en>
        /// </remarks>
        AbstractTerminal Terminal {
            get;
        }
        /// <summary>
        /// <ja>
        /// ターミナルのユーザーインターフェイスを提供するコントロールです。
        /// </ja>
        /// <en>
        /// Control that offers user interface of terminal.
        /// </en>
        /// </summary>
        TerminalControl TerminalControl {
            get;
        }
        /// <summary>
        /// <ja>ターミナル設定を示すオブジェクトです。</ja>
        /// <en>Object that shows terminal setting.</en>
        /// </summary>
        ITerminalSettings TerminalSettings {
            get;
        }
        /// <summary>
        /// <ja>ターミナルの接続を示すオブジェクトです。</ja>
        /// <en>Object that shows connection of terminal.</en>
        /// </summary>
        ITerminalConnection TerminalConnection {
            get;
        }
        /// <summary>
        /// <ja>所有するウィンドウを示します。</ja>
        /// <en>The owned window is shown. </en>
        /// </summary>
        IPoderosaMainWindow OwnerWindow {
            get;
        }
        /// <summary>
        /// <ja>ターミナルへの送信機能を提供します。</ja>
        /// <en>The transmission function to the terminal is offered. </en>
        /// </summary>
        TerminalTransmission TerminalTransmission {
            get;
        }
    }


    /// <summary>
    /// <ja>
    /// ターミナルサービスを提供するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that provides terminal service.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// このインターフェイスは、新規Telnet／SSH／Cygwin接続の機能を提供します。
    /// </para>
    /// <para>
    /// TerminalSessionPluginプラグイン（プラグインID：「org.poderosa.terminalsessions」）によって
    /// 提供されており、次のようにして取得できます。
    /// </para>
    /// <code>
    /// ITerminalSessionsService termservice = 
    ///  (ITerminalSessionsService)PoderosaWorld.PluginManager.FindPlugin(
    ///     "org.poderosa.terminalsessions", typeof(ITerminalSessionsService));
    /// Debug.Assert(termservice != null);
    /// </code>
    /// </ja>
    /// <en>
    /// <para>
    /// This interface offers the function of a new Telnet/SSH/Cygwin connection. 
    /// </para>
    /// <para>
    /// It is offered by the TerminalSessionPlugin plug-in (plugin ID[org.poderosa.terminalsessions]) , and it is possible to get it as follows. 
    /// </para>
    /// <code>
    /// ITerminalSessionsService termservice = 
    ///  (ITerminalSessionsService)PoderosaWorld.PluginManager.FindPlugin(
    ///     "org.poderosa.terminalsessions", typeof(ITerminalSessionsService));
    /// Debug.Assert(termservice != null);
    /// </code>
    /// </en>
    /// </remarks>
    public interface ITerminalSessionsService : IAdaptable {
        /// <summary>
        /// <ja>
        /// 新規ターミナル接続をするためのインターフェイスを示します。
        /// </ja>
        /// <en>
        /// The interface to connect a new terminal is shown. 
        /// </en>
        /// </summary>
        ITerminalSessionStartCommand TerminalSessionStartCommand {
            get;
        }
        /// <summary>
        /// <ja>
        /// 接続コマンドのカテゴリを示します。
        /// </ja>
        /// <en>
        /// The category of connected command is shown. 
        /// </en>
        /// </summary>
        ICommandCategory ConnectCommandCategory {
            get;
        }
    }

    /// <summary>
    /// <ja>
    /// 新規ターミナルの接続機能を提供するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that offers connected function of new terminal.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>このインターフェイスは、<seealso cref="Poderosa.Sessions.ITerminalSessionsService">ITerminalSessionsServicen</seealso>の
    /// <see cref="Poderosa.Sessions.ITerminalSessionsService.TerminalSessionStartCommand">TerminalSessionStartCommandプロパティ</see>
    /// から取得できます。</ja>
    /// <en>This interface can be got from the <see cref="Poderosa.Sessions.ITerminalSessionsService.TerminalSessionStartCommand">TerminalSessionStartCommand property</see> of ITerminalSessionsServicen. </en>
    /// </remarks>
    public interface ITerminalSessionStartCommand : IPoderosaCommand {
        /// <summary>
        /// <ja>既存の接続を用いて新規ターミナルセッションを開始します。</ja><en>A new terminal session is begun by using an existing connection. </en>
        /// </summary>
        /// <param name="target"><ja>ターミナルに結びつけるビューまたはウィンドウ</ja><en>View or window that ties to terminal</en></param>
        /// <param name="existing_connection"><ja>既存の接続オブジェクト</ja><en>Existing connected object</en></param>
        /// <param name="settings"><ja>ターミナル設定が格納されたオブジェクト</ja><en>Object where terminal setting is stored</en></param>
        /// <returns><ja>開始されたターミナルセッションが返されます</ja><en>The begun terminal session is returned. </en></returns>
        /// <overloads>
        /// <summary>
        /// <ja>新規ターミナル接続を開始します。</ja><en>Start a new terminal session.</en>
        /// </summary>
        /// </overloads>
        ITerminalSession StartTerminalSession(ICommandTarget target, ITerminalConnection existing_connection, ITerminalSettings settings);
        //ITerminalParameterは、Telnet/SSH/Cygwinのいずれかである必要がある。
        /// <summary>
        /// <ja>
        /// 接続パラメータを用いて新規接続をし、そのターミナルセッションを開始します。
        /// </ja>
        /// <en>
        /// It newly connects by using connected parameter, and the terminal session is begun. 
        /// </en>
        /// </summary>
        /// <param name="target"><ja>ターミナルに結びつけるビューまたはウィンドウ</ja><en>View or window that ties to terminal</en></param>
        /// <param name="destination">
        /// <ja>接続時のパラメータが格納されたオブジェクト。
        /// <seealso cref="ICygwinParameter">ICygwinParameter</seealso>、
        /// <seealso cref="ISSHLoginParameter">ISSHLoginParameter</seealso>、
        /// <seealso cref="ITCPParameter">ITCPParameter</seealso>のいずれかでなければなりません。</ja>
        /// <en>
        /// Object where parameter when connecting it is stored. 
        /// It should be either <seealso cref="ICygwinParameter">ICygwinParameter</seealso>, <seealso cref="ISSHLoginParameter">ISSHLoginParameter</seealso> or <seealso cref="ITCPParameter">ITCPParameter</seealso>. </en>
        /// </param>
        /// <param name="settings"><ja>ターミナル設定が格納されたオブジェクト</ja><en>Object where terminal setting is stored</en></param>
        /// <returns><ja>開始されたターミナルセッションが返されます</ja><en>The begun terminal session is returned. </en></returns>
        ITerminalSession StartTerminalSession(ICommandTarget target, ITerminalParameter destination, ITerminalSettings settings);

        /// <summary>
        /// <ja>セッションとは無関係に接続だけ開きます</ja>
        /// <en>Opens not any session but connection</en>
        /// </summary>
        /// <exclude/>
        ITerminalConnection OpenConnection(IPoderosaMainWindow window, ITerminalParameter destination, ITerminalSettings settings);

        void OpenShortcutFile(ICommandTarget target, string filename);
    }

    //ITerminalParameterをインスタンシエートしてITerminalConnectionにするExtensionPointのインタフェース
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITerminalConnectionFactory {
        bool IsSupporting(ITerminalParameter param, ITerminalSettings settings);
        ITerminalConnection EstablishConnection(IPoderosaMainWindow window, ITerminalParameter param, ITerminalSettings settings);
    }


    //ログインダイアログの使い勝手向上用のサポート
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITelnetSSHLoginDialogInitializeInfo : IAdaptable {
        //接続先候補
        void AddHost(string value);
        void AddAccount(string value);
        void AddIdentityFile(string value);
        void AddPort(int value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITelnetSSHLoginDialogInitializer {
        void ApplyLoginDialogInfo(ITelnetSSHLoginDialogInitializeInfo info);
    }

    /// <summary>
    /// Parameters about a terminal session.
    /// </summary>
    /// <typeparam name="T">Class or interface of the connection parameter</typeparam>
    public class TerminalSessionParameters<T> {
        public readonly T ConnectionParameter;
        public readonly ITerminalParameter TerminalParameter;
        public readonly ITerminalSettings TerminalSettings;

        public TerminalSessionParameters(T connectionParam, ITerminalParameter terminalParam, ITerminalSettings terminalSettings) {
            ConnectionParameter = connectionParam;
            TerminalParameter = terminalParam;
            TerminalSettings = terminalSettings;
        }
    }

    /// <summary>
    /// Provides saved terminal session parameters
    /// </summary>
    public interface ITerminalSessionParameterStore {
        /// <summary>
        /// Retrieves terminal parameters matched with the specified type.
        /// </summary>
        /// <typeparam name="T">Class or interface of the connection parameter</typeparam>
        /// <returns>
        /// Enumerable of <see cref="TerminalSessionParameters{T}"/>.
        /// </returns>
        IEnumerable<TerminalSessionParameters<T>> FindTerminalParameter<T>() where T : class;
    }

    //Extension Pointが提供
    //既に格納されている情報は壊さないようにするのがルール
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ILoginDialogUISupport {
        //２つの戻り値があるのでoutを使う。adapterは、TerminalParameterの種類を指定するための引数。対応するものがないときはnullを返す
        void FillTopDestination(Type adapter, out ITerminalParameter parameter, out ITerminalSettings settings);
        //ホスト名で指定する
        void FillCorrespondingDestination(Type adapter, string destination, out ITerminalParameter parameter, out ITerminalSettings settings);
    }

    //Terminal Session固有オプション
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface ITerminalSessionOptions {
        bool AskCloseOnExit {
            get;
            set;
        }

        //preference editor only
        int TerminalEstablishTimeout {
            get;
        }
        string GetDefaultLoginDialogUISupportTypeName(string logintype);
    }

}
