/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SessionEx.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Poderosa.Forms;
using Poderosa.Document;
using Poderosa.Commands;

namespace Poderosa.Sessions {
    /// <summary>
    /// <ja>
    /// セッションマネージャを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows session manager.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// このインターフェイスはセッションマネージャ（SessionManagerPluginプラグイン：プラグインID「org.poderosa.core.sessions」）
    /// によって提供されるインターフェイスであり、セッション情報を操作します。
    /// </para>
    /// <para>
    /// このインターフェイスは、<seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>の
    /// <see cref="Poderosa.Plugins.ICoreServices.SessionManager">SessionManagerプロパティ</see>
    /// を使って取得できます。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// This interface is an interface offered by the session manager (SessionManagerPlugin plug-in : Plug-inID "org.poderosa.core.sessions") , and session information is operated. 
    /// </para>
    /// <para>
    /// This interface can be acquired by using the <see cref="Poderosa.Plugins.ICoreServices.SessionManager">SessionManager property</see> of <seealso cref="Poderosa.Plugins.ICoreServices">ICoreServices</seealso>. 
    /// </para>
    /// </en>
    /// </remarks>
    /// <example>
    /// <ja>
    /// ISessionManagerを取得します。
    /// <code>
    /// // ICoreServicesを取得
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // ISessionManagerを取得
    /// ISessionManager sessionman = cs.SessionManager;
    /// </code>
    /// </ja>
    /// <en>
    /// Get the ISessionManager.
    /// <code>
    /// // Get the ICoreServices.
    /// ICoreServices cs = (ICoreServices)PoderosaWorld.GetAdapter(typeof(ICoreServices));
    /// // Get the ISessionManager.
    /// ISessionManager sessionman = cs.SessionManager;
    /// </code>
    /// </en>
    /// </example>
    public interface ISessionManager : IAdaptable {
        //Structure
        /// <summary>
        /// <ja>
        /// すべてのセッションを列挙します。
        /// </ja>
        /// <en>
        /// Enumerate all sessions.
        /// </en>
        /// </summary>
        IEnumerable<ISession> AllSessions {
            get;
        }
        /// <summary>
        /// <ja>
        /// ウィンドウに結びつけられたドキュメントを配列として得ます。
        /// </ja>
        /// <en>
        /// The document tie to the window is obtained as an array. 
        /// </en>
        /// </summary>
        /// <param name="window"><ja>対象となるウィンドウです。</ja><en>It is a window that becomes an object. </en></param>
        /// <returns><ja>ウィンドウに含まれるドキュメントの配列が返されます。</ja><en>The array of the document included in the window is returned. </en></returns>
        IPoderosaDocument[] GetDocuments(IPoderosaMainWindow window);

        //Start/End
        /// <summary>
        /// <ja>
        /// 新しいセッションを開始します。
        /// </ja>
        /// <en>
        /// Start a new session.
        /// </en>
        /// </summary>
        /// <param name="session"><ja>開始するセッション</ja><en>Session to start.</en></param>
        /// <param name="firstView"><ja>セッションに割り当てるビュー</ja><en>View allocated in session</en></param>
        /// <remarks>
        /// <ja>
        /// 新しくセッションを作成するためのビューは、<seealso cref="IViewManager">IViewManager</seealso>の
        /// <see cref="IViewManager.GetCandidateViewForNewDocument">GetCandidateViewForNewDocumentメソッド</see>
        /// で作ることができます。
        /// </ja>
        /// <en>
        /// The view to make the session newly can be made by the <see cref="IViewManager.GetCandidateViewForNewDocument">GetCandidateViewForNewDocument method</see> of IViewManager. 
        /// </en>
        /// </remarks>
        void StartNewSession(ISession session, IPoderosaView firstView);

        /// <summary>
        /// <ja>
        /// セッションを閉じます。
        /// </ja>
        /// <en>
        /// Close the session.
        /// </en>
        /// </summary>
        /// <param name="session"><ja>閉じたいセッションです。</ja><en>Session to close.</en></param>
        /// <returns><ja>セッションが閉じられたかどうかを示す値です。</ja><en>It is a value in which it is shown whether the session was closed. </en></returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// このメソッドを呼び出すと、セッションを構成する<seealso cref="ISession">ISession</seealso>
        /// の<see cref="ISession.PrepareCloseSession">PrepareCloseSessionメソッド</see>
        /// が呼び出されます。<see cref="ISession.PrepareCloseSession">PrepareCloseSessionメソッド</see>がPrepareCloseResult.Cancel
        /// を返したときには、セッションを閉じる動作は中止されます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When this method is called, the <see cref="ISession.PrepareCloseSession">PrepareCloseSession method</see> of <seealso cref="ISession">ISession</seealso> that composes the session is called. When the <see cref="ISession.PrepareCloseSession">PrepareCloseSession method</see> returns PrepareCloseResult.Cancel, operation that shuts the session is discontinued. 
        /// </para>
        /// </en>
        /// </remarks>
        PrepareCloseResult TerminateSession(ISession session);
        /// <summary>
        /// <ja>
        /// ドキュメントを閉じます。
        /// </ja>
        /// <en>
        /// Close the document.
        /// </en>
        /// </summary>
        /// <param name="document"><ja>閉じたいドキュメントです。</ja><en>Document to close.</en></param>
        /// <returns><ja>ドキュメントが閉じられたかどうかを示す値です。</ja><en>It is a value in which it is shown whether the document was closed. </en></returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// このメソッドを呼び出すと、セッションを構成する<seealso cref="ISession">ISession</seealso>
        /// の<see cref="ISession.PrepareCloseDocument">PrepareCloseDocumentメソッド</see>
        /// が呼び出されます。<see cref="ISession.PrepareCloseDocument">PrepareCloseDocumentメソッド</see>がPrepareCloseResult.Cancel
        /// を返したときには、ドキュメントを閉じる動作は中止されます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When this method is called, the <see cref="ISession.PrepareCloseDocument">PrepareCloseDocument method</see> of 
        /// <seealso cref="ISession">ISession</seealso> that composes the session is called. 
        /// When the <see cref="ISession.PrepareCloseDocument">PrepareCloseDocument method</see> 
        /// returns PrepareCloseResult.Cancel, operation that shuts the document is discontinued. 
        /// </para>
        /// </en>
        /// </remarks>
        PrepareCloseResult CloseDocument(IPoderosaDocument document);

        //Document Management
        /// <summary>
        /// <ja>ドキュメントをアクティブ化します。</ja><en>Activate the document.</en>
        /// </summary>
        /// <param name="document"><ja>アクティブ化するドキュメント</ja><en>Document to activate.</en></param>
        /// <param name="reason"><ja>アクティブ化する理由を格納したオブジェクト</ja><en>Object that stored reason made active</en></param>
        void ActivateDocument(IPoderosaDocument document, ActivateReason reason);
        /// <summary>
        /// <ja>ドキュメントとビューとを結びつけます。</ja><en>Tie the document and the view.</en>
        /// </summary>
        /// <param name="document"><ja>対象となるドキュメント</ja>
        /// <en>Document to target.</en>
        /// </param>
        /// <param name="view"><ja>割り当てるビュー</ja>
        /// <en>View to assign.</en>
        /// </param>
        void AttachDocumentAndView(IPoderosaDocument document, IPoderosaView view);
        /// <summary>
        /// <ja>
        /// ドキュメントのステータスを更新します。
        /// </ja>
        /// <en>
        /// Update the status of the document.
        /// </en>
        /// </summary>
        /// <param name="document"><ja>更新するドキュメント</ja><en>Document to update.</en></param>
        void RefreshDocumentStatus(IPoderosaDocument document);

        //Listener
        /// <summary>
        /// <ja>
        /// アクティブなドキュメントが変化したときの通知を受け取るリスナを登録します。
        /// </ja>
        /// <en>
        /// The listener that receives the notification when an active document is changed is registered. 
        /// </en>
        /// </summary>
        /// <param name="listener"><ja>登録するリスナ</ja><en>Registered listener</en></param>
        void AddActiveDocumentChangeListener(IActiveDocumentChangeListener listener);
        /// <summary>
        /// <ja>
        /// アクティブなドキュメントが変化したときの通知を受け取るリスナを解除します。
        /// </ja>
        /// <en>
        /// The listener that receives the notification when an active document is changed is released. 
        /// </en>
        /// </summary>
        /// <param name="listener"><ja>解除するリスナ</ja><en>Listener to release.</en></param>
        void RemoveActiveDocumentChangeListener(IActiveDocumentChangeListener listener);
        /// <summary>
        /// <ja>
        /// セッションが開始されたり切断されたときの通知を受け取るリスナを登録します。
        /// </ja>
        /// <en>
        /// The listener that is begun the session and receives the notification when close is registered. 
        /// </en>
        /// </summary>
        /// <param name="listener"><ja>登録するリスナ</ja><en>Listener to regist</en></param>
        void AddSessionListener(ISessionListener listener);
        /// <summary>
        /// <ja>セッションが開始されたり切断されたときの通知を受け取るリスナを解除します。</ja>
        /// <en>The listener that is begun the session and receives the notification when close is released.</en>
        /// </summary>
        /// <param name="listener"><ja>解除するリスナ</ja><en>Listener to release.</en></param>
        void RemoveSessionListener(ISessionListener listener);
    }

    /// <summary>
    /// <ja>
    /// セッションホストオブジェクトを示すインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that shows session host object.
    /// </en>
    /// </summary>
    public interface ISessionHost {
        /// <summary>
        /// <ja>
        /// ドキュメントを登録します。
        /// </ja>
        /// <en>
        /// Regist the document.
        /// </en>
        /// </summary>
        /// <param name="document"><ja>登録するドキュメント</ja><en>Document to regist.</en></param>
        void RegisterDocument(IPoderosaDocument document);
        //TODO RemoveDocumentあってよい
        /// <summary>
        /// <ja>
        /// セッションをプラグイン側から終了させます。
        /// </ja>
        /// <en>
        /// Terminate the sessoin from plug-in side.
        /// </en>
        /// </summary>
        void TerminateSession();
        /// <summary>
        /// <ja>
        /// ドキュメントに結びつけられたフォームを得ます。
        /// </ja>
        /// <en>
        /// Get the form tied with document.
        /// </en>
        /// </summary>
        /// <param name="document"><ja>対象となるドキュメントです。</ja><en>Targeted document.</en></param>
        /// <returns><ja>ドキュメントに結びつけられたフォームが返されます。</ja><en>The form tie to the document is returned. </en></returns>
        IPoderosaForm GetParentFormFor(IPoderosaDocument document);
    }

    //アクティブにする操作の開始条件
    /// <summary>
    /// <ja>
    /// ドキュメントがアクティブになったときの理由を示します。
    /// </ja>
    /// <en>
    /// The reason when the document becomes active is shown. 
    /// </en>
    /// </summary>
    public enum ActivateReason {
        /// <summary>
        /// <ja>内部動作によりアクティブになった</ja>
        /// <en>It became active by internal operation. </en>
        /// </summary>
        InternalAction,
        /// <summary>
        /// <ja>タブクリックによりアクティブになった</ja>
        /// <en>It became active by the tab click. </en>
        /// </summary>
        TabClick,
        /// <summary>
        /// <ja>ビューがフォーカスを受け取ったためにアクティブになった</ja>
        /// <en>Because the view had got focus, it became active. </en>
        /// </summary>
        ViewGotFocus,
        /// <summary>
        /// <ja>ドラッグ＆ドロップ操作によりアクティブになった</ja>
        /// <en>It became active by the drag &amp; drop operation. </en>
        /// </summary>
        DragDrop
    }

    /// <summary>
    /// <ja>
    /// ドキュメントやセッションが閉じられようとするときの戻り値を示します。
    /// </ja>
    /// <en>
    /// The return value when the document and the session start being shut is shown. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// <para>
    /// この列挙体は、<seealso cref="ISession">ISession</seealso>の<see cref="ISession.PrepareCloseDocument">PrepareCloseDocumentメソッド</see>
    /// や<see cref="ISession.PrepareCloseSession">PrepareCloseSessionメソッド</see>の戻り値として使われます。
    /// </para>
    /// <para>
    /// PrepareCloseResult.ContinueSessionが使われるのは、<see cref="ISession.PrepareCloseDocument">PrepareCloseDocumentメソッド</see>のときだけです。
    /// </para>
    /// </ja>
    /// <en>
    /// <para>
    /// This enumeration is used as a return value of the <see cref="ISession.PrepareCloseDocument">PrepareCloseDocument method</see> and the <see cref="ISession.PrepareCloseSession">PrepareCloseSession method</see> of <seealso cref="ISession">ISession</seealso>. 
    /// </para>
    /// <para>
    /// Only PrepareCloseResult.ContinueSession is used on <see cref="ISession.PrepareCloseDocument">PrepareCloseDocument method</see>
    /// </para>
    /// </en>
    /// </remarks>
    public enum PrepareCloseResult {
        /// <summary>
        /// <ja>
        /// ドキュメントは閉じますが、セッションは閉じません。
        /// </ja>
        /// <en>
        /// Close the document, but session is not close.
        /// </en>
        /// </summary>
        ContinueSession,
        /// <summary>
        /// <ja>
        /// ドキュメントやセッションを閉じます。
        /// </ja>
        /// <en>
        /// Close the document and the session.
        /// </en>
        /// </summary>
        TerminateSession,
        /// <summary>
        /// <ja>
        /// ドキュメントやセッションを閉じる動作をキャンセルします。
        /// </ja>
        /// <en>
        /// Cancel closing the document and the session.
        /// </en>
        /// </summary>
        Cancel
    }

    /// <summary>
    /// <ja>
    /// セッションを示します。
    /// </ja>
    /// <en>
    /// The session is shown. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// 標準のターミナルエミュレータとして用いる場合、ISessionの実態は、ISessionから継承している
    /// <seealso cref="Poderosa.Sessions.ITerminalSession">ITerminalSession</seealso>であり、GetAdapterメソッドで変換できます。
    /// </ja>
    /// <en>
    /// The realities of ISession are <seealso cref="Poderosa.Sessions.ITerminalSession">ITerminalSession</seealso> that has been succeeded to, 
    /// and can be converted from ISession by the GetAdapter method when using it as a standard terminal emulator. 
    /// </en>
    /// </remarks>
    public interface ISession : IAdaptable {
        /// <summary>
        /// <ja>
        /// セッションのキャプションです。
        /// </ja>
        /// <en>
        /// Caption of the session.
        /// </en>
        /// </summary>
        string Caption {
            get;
        }
        /// <summary>
        /// <ja>
        /// セッションのアイコンです。
        /// </ja>
        /// <en>
        /// Icon of the session.
        /// </en>
        /// </summary>
        Image Icon {
            get;
        } //16*16

        //以下はSessionManagerが呼ぶ。これ以外では呼んではいけない
        /// <summary>
        /// <ja>
        /// セッションマネージャから呼び出される初期化のメソッドです。
        /// </ja>
        /// <en>
        /// Initialization called from session manager method.
        /// </en>
        /// </summary>
        /// <param name="host"><ja>セッションを操作するためのセッションホストオブジェクトです。</ja>
        /// <en>Session host object to operate session.</en>
        /// </param>
        /// <remarks>
        /// <ja>
        /// <para>
        /// このメソッドは、<seealso cref="ISessionManager">ISessionManager</seealso>の<see cref="ISessionManager.StartNewSession">StartNewSessionメソッド</see>
        /// が呼び出されたときに、セッションマネージャによって間接的に呼び出されます。
        /// 開発者は、このメソッドを直接呼び出してはいけません。
        /// </para>
        /// <para>
        /// 開発者は一般に、このメソッドの処理においてドキュメントを作成し、<paramref name="host">host</paramref>として渡された<seealso cref="ISessionHost">ISessionHost</seealso>
        /// の<see cref="ISessionHost.RegisterDocument">RegisterDocumentメソッド</see>を呼び出してドキュメントを登録します。
        /// </para>
        /// <para>
        /// セッションの詳細については、<see href="chap04_02_04.html">セッションの操作</see>を参照してください。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the <see cref="ISessionManager.StartNewSession">StartNewSession method</see> of 
        /// <seealso cref="ISessionManager">ISessionManager</seealso> is called, this method is 
        /// indirectly called by the session manager. The developer must not call this method directly. 
        /// </para>
        /// <para>
        /// The developer makes the document in general in the processing of this method, calls the <see cref="ISessionHost.RegisterDocument">RegisterDocument method</see> of <seealso cref="ISessionHost">ISessionHost</seealso> passed as <paramref name="host">host</paramref>, and registers the document. 
        /// </para>
        /// <para>
        /// Please refer to <see href="chap04_02_04.html">Operation of session</see> for details of the session. 
        /// </para>
        /// </en>
        /// </remarks>
        void InternalStart(ISessionHost host);

        /// <summary>
        /// <ja>
        /// セッションが終了したときに呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// It is a method of the call when the session ends. 
        /// </en>
        /// </summary>
        void InternalTerminate();

        //Session側でTerminateを指定した場合でも、必ずTerminateされるとは限らない点に注意。
        /// <summary>
        /// <ja>
        /// ドキュメントを閉じてもよいかを決定します。
        /// </ja>
        /// <en>
        /// It is decided whether I may close the document. 
        /// </en>
        /// </summary>
        /// <param name="document"><ja>閉じる対象となるドキュメント</ja><en>Document that close object</en></param>
        /// <returns><ja>ドキュメントを閉じるかどうかを決定する値です。</ja><en>Value in which it is decided whether to close document</en></returns>
        /// <remarks>
        /// <ja>
        /// <para>
        /// このメソッドは、ドキュメントを閉じる操作が行われると呼び出されます。
        /// </para>
        /// <para>
        /// 開発者はドキュメントを閉じるかどうかを<seealso cref="PrepareCloseResult">PrepareCloseResult列挙体</seealso>
        /// として返してください。PrepareCloseResult.Cancelを返した場合、ドキュメントを閉じる動作は取り消されます。
        /// </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the operation that closes the document is done, this method is called. 
        /// </para>
        /// <para>
        /// The developer must return whether to close the document as 
        /// <seealso cref="PrepareCloseResult">PrepareCloseResult enumeration</seealso>. 
        /// When PrepareCloseResult.Cancel is returned, operation that closed the document is canceled. 
        /// </para>
        /// </en>
        /// </remarks>
        PrepareCloseResult PrepareCloseDocument(IPoderosaDocument document);
        /// <summary>
        /// <ja>
        /// セッションを閉じてもよいかを決定します。
        /// </ja>
        /// <en>
        /// It is decided whether I may close the session. 
        /// </en>
        /// </summary>
        /// <returns><ja>セッションを閉じるかどうかを決定する値です。</ja><en>It is a value in which it is decided whether to close the session. </en></returns>
        /// <remarks>
        /// <ja>
        ///    <para>
        ///    このメソッドは、セッションを閉じる操作が行われると呼び出されます。
        ///    </para>
        ///    <para>
        ///    開発者はセッションを閉じるかどうかを<seealso cref="T:Poderosa.Sessions.PrepareCloseResult">PrepareCloseResult列挙体</seealso>
        ///    として返してください。PrepareCloseResult.Cancelを返した場合、セッションを閉じる動作は取り消されます。
        ///    </para>
        /// </ja>
        /// <en>
        /// <para>
        /// When the operation that close the session is done, this method is called. 
        /// </para>
        /// <para>
        /// The developer must return whether to close the document as 
        /// <seealso cref="PrepareCloseResult">PrepareCloseResult enumeration</seealso>. 
        /// When PrepareCloseResult.Cancel is returned, operation that closed the document is canceled. 
        /// </para>
        /// </en>
        /// </remarks>
        PrepareCloseResult PrepareCloseSession();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="view"></param>
        /// <exclude/>
        void InternalAttachView(IPoderosaDocument document, IPoderosaView view);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="document"></param>
        /// <param name="view"></param>
        /// <exclude/>
        void InternalDetachView(IPoderosaDocument document, IPoderosaView view);

        /// <summary>
        /// <ja>
        /// ドキュメントが閉じられる際に呼び出されるメソッドです。
        /// </ja>
        /// <en>
        /// It is a method of the call when the document is closed. 
        /// </en>
        /// </summary>
        /// <param name="document"><ja>閉じられる対象となるドキュメント</ja><en>Document that becomes close object</en></param>
        /// <exclude/>
        void InternalCloseDocument(IPoderosaDocument document);

    }

    //Doc/Viewの関連付け変更の通知　変更内容を逐一取得できるようにするのは後の課題
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    /// 
    public interface IDocViewRelationEventHandler {
        void OnDocViewRelationChange();
    }

    /// <summary>
    /// <ja>
    /// ドキュメントがアクティブ化されたり非アクティブ化されたことを通知するインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that notifies for document to be made active and to have been made non-actively.
    /// </en>
    /// </summary>
    public interface IActiveDocumentChangeListener {
        /// <summary>
        /// <ja>
        /// ドキュメントがアクティブ化されたときに呼び出されます。
        /// </ja>
        /// <en>
        /// Called when the document is activated.
        /// </en>
        /// </summary>
        /// <param name="window"><ja>対象となるウィンドウ</ja><en>Window that becomes object.</en></param>
        /// <param name="document"><ja>対象となるドキュメント</ja><en>Window that becomes object</en></param>
        void OnDocumentActivated(IPoderosaMainWindow window, IPoderosaDocument document);
        /// <summary>
        /// <ja>
        /// ドキュメントが非アクティブ化されたときに呼び出されます。
        /// </ja>
        /// <en>
        /// When the document is made non-active, it is called. 
        /// </en>
        /// </summary>
        /// <param name="window"><ja>対象となるウィンドウ</ja><en>Window that becomes object</en>
        /// </param>
        void OnDocumentDeactivated(IPoderosaMainWindow window);
    }

    /// <summary>
    /// <ja>
    /// セッションが開始／切断されたときの通知を受け取るインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface that receives notification when session begin/finish
    /// </en>
    /// </summary>
    public interface ISessionListener {
        /// <summary>
        /// <ja>
        /// セッションが開始されたときに呼び出されます。
        /// </ja>
        /// <en>
        /// When the session is started, it is called. 
        /// </en>
        /// </summary>
        /// <param name="session"><ja>開始されたセッション</ja><en>Started session.</en></param>
        void OnSessionStart(ISession session);
        /// <summary>
        /// <ja>
        /// セッションが終了したときに呼び出されます。
        /// </ja>
        /// <en>
        /// When the session ends, it is called. 
        /// </en>
        /// </summary>
        /// <param name="session"><ja>終了したセッション</ja><en>Ended session</en></param>
        void OnSessionEnd(ISession session);
    }

}
