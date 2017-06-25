/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Session.cs,v 1.2 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Util.Collections;
using Poderosa.Plugins;
using Poderosa.Forms;
using Poderosa.Document;
using Poderosa.Util;

[assembly: PluginDeclaration(typeof(Poderosa.Sessions.SessionManagerPlugin))]

namespace Poderosa.Sessions {
    //PoderosaMainWindowが呼ぶためのインタフェース
    internal interface ISessionManagerForPoderosaWindow {
        PrepareCloseResult CloseMultipleDocuments(ClosingContext context, IPoderosaDocument[] documents);
    }

    internal interface ISessionManagerForViewSplitter {
        void ChangeLastAttachedViewForAllDocuments(IPoderosaView closing_view, IPoderosaView alternative);
        void ChangeLastAttachedViewForWindow(IPoderosaMainWindow window, IPoderosaView alternative);
        void SetLastAttachedViewForAllDocuments(IPoderosaView view);
    }

    [PluginInfo(ID = SessionManagerPlugin.PLUGIN_ID, Version = VersionInfo.PODEROSA_VERSION, Author = VersionInfo.PROJECT_NAME, Dependencies = "org.poderosa.core.window")]
    internal class SessionManagerPlugin :
        PluginBase,
        ISessionManager,
        ISessionManagerForViewSplitter {
        public const string PLUGIN_ID = "org.poderosa.core.sessions";
        private static SessionManagerPlugin _instance;
        private TypedHashtable<ISession, SessionHost> _sessionMap;
        private TypedHashtable<IPoderosaDocument, DocumentHost> _documentMap;
        private ActivateContext _activateContext;
        private IExtensionPoint _docViewRelationHandler;
        private ListenerList<IActiveDocumentChangeListener> _activeDocumentChangeListeners;
        private ListenerList<ISessionListener> _sessionListeners;

        public override void InitializePlugin(IPoderosaWorld poderosa) {
            _instance = this;
            base.InitializePlugin(poderosa);
            _sessionMap = new TypedHashtable<ISession, SessionHost>();
            _documentMap = new TypedHashtable<IPoderosaDocument, DocumentHost>();
            _docViewRelationHandler = poderosa.PluginManager.CreateExtensionPoint("org.poderosa.core.sessions.docViewRelationHandler", typeof(IDocViewRelationEventHandler), this);
            _activeDocumentChangeListeners = new ListenerList<IActiveDocumentChangeListener>();
            _activeDocumentChangeListeners.Add(new WindowCaptionManager());

            _sessionListeners = new ListenerList<ISessionListener>();
        }
        public override void TerminatePlugin() {
            base.TerminatePlugin();
            //これは満たされていないと！
            Debug.Assert(_sessionMap.Count == 0);
            Debug.Assert(_documentMap.Count == 0);
        }

        public static SessionManagerPlugin Instance {
            get {
                return _instance;
            }
        }

        public IEnumerable<ISession> AllSessions {
            get {
                List<ISession> r = new List<ISession>();
                foreach (ISession session in _sessionMap.Keys)
                    r.Add(session);
                return r;
            }
        }

        public IPoderosaDocument[] GetDocuments(IPoderosaMainWindow window) {
            List<IPoderosaDocument> r = new List<IPoderosaDocument>();
            foreach (DocumentHost dh in _documentMap.Values) {
                if (dh.LastAttachedView != null && dh.LastAttachedView.ParentForm == window)
                    r.Add(dh.Document);
            }
            return r.ToArray();
        }

        public void StartNewSession(ISession session, IPoderosaView firstView) {
            firstView = AdjustToOuterView(firstView);
            SessionHost host = new SessionHost(this, session);
            _sessionMap.Add(session, host);
            session.InternalStart(host);
            foreach (ISessionListener listener in _sessionListeners)
                listener.OnSessionStart(session);

            //この時点で、少なくとも一つドキュメントがないといけない。２つ以上は不可、でもいいかもしれない
            if (host.DocumentCount == 0)
                throw new InvalidOperationException("session must register at least one document in InternalStart()");
            AttachDocumentAndView(host.DocumentAt(0), firstView);
        }

        public void AttachDocumentAndView(IPoderosaDocument document, IPoderosaView view) {
            view = AdjustToOuterView(view);
            DocumentHost dh = FindDocumentHost(document);
            Debug.Assert(dh != null, "the document must be registered by calling ISessionHost#RegisterDocument");

            if (view.Document == document) {
                Debug.Assert(dh.CurrentView == view);
                return; //何もしない
            }

            IPoderosaView previous_view = dh.CurrentView; //関連づけを指定するドキュメントがもともと見えていたビュー
            IPoderosaForm last_window = ViewToForm(dh.LastAttachedView); //もともとの所有ウィンドウ。初めてのAttachではnullであることにちゅうい

            //現在の関連を一旦切る
            if (previous_view != null) {
                Debug.WriteLineIf(DebugOpt.DumpDocumentRelation, "Detach Prev View " + ViewName(previous_view));
                dh.DetachView();
            }
            Debug.Assert(dh.CurrentView == null);

            //接続先にドキュメントが存在していればそれを切り離す
            IPoderosaDocument existing_doc = view.Document;
            if (existing_doc != null) { //対象のビューに古いのがひっついていたら外す
                DocumentHost eh = FindDocumentHost(existing_doc);
                Debug.Assert(eh.CurrentView == view);
                Debug.WriteLineIf(DebugOpt.DumpDocumentRelation, String.Format("Detach Destination View doc={0} view={1}", existing_doc.GetType().Name, ViewName(view)));
                eh.DetachView();
            }

            //新規の接続
            Debug.Assert(view.Document == null && dh.CurrentView == null); //Attach準備ができていること確認
            dh.AttachView(view);

            //移動することで新規に見えるようになるドキュメントを探索
            if (previous_view != null && previous_view != view) {
                DocumentHost new_visible_doc = ShowBackgroundDocument(previous_view);
                Debug.Assert(new_visible_doc != dh);
            }

            //ドキュメントを保有するウィンドウが変化したら通知。初回Attachではlast_mainwindow==nullであることに注意
            if (last_window != view.ParentForm) {
                if (last_window != null)
                    NotifyRemove(last_window, document);
                NotifyAdd(ViewToForm(view), document);
            }

            FireDocViewRelationChange();
        }

        //ISessionManagerの終了系　細かいのはドキュメントあり
        public PrepareCloseResult CloseDocument(IPoderosaDocument document) {
            DocumentHost dh = FindDocumentHost(document);
            Debug.Assert(dh != null);
            SessionHost sh = dh.SessionHost;
            PrepareCloseResult r = sh.Session.PrepareCloseDocument(document);
            if (r == PrepareCloseResult.Cancel)
                return r;

            CleanupDocument(dh);
            if (r == PrepareCloseResult.TerminateSession || sh.DocumentCount == 0)
                CleanupSession(sh);
            return r;
        }

        public PrepareCloseResult TerminateSession(ISession session) {
            SessionHost sh = FindSessionHost(session);
            PrepareCloseResult r = sh.Session.PrepareCloseSession();
            Debug.Assert(r != PrepareCloseResult.ContinueSession, "sanity");
            if (r == PrepareCloseResult.Cancel)
                return r;

            CleanupSession(sh);
            return r;
        }

        public PrepareCloseResult CloseMultipleDocuments(ClosingContext context, IPoderosaDocument[] documents) {
            List<SessionHost> sessions = CreateSessionHostCollection();
            foreach (SessionHost sh in sessions)
                sh.CMP_ClosingDocumentCount = 0; //カウントリセット

            foreach (IPoderosaDocument doc in documents) {
                DocumentHost dh = FindDocumentHost(doc);
                dh.SessionHost.CMP_ClosingDocumentCount++;
            }
            //ここまでで、各SessionHostごとに何個のドキュメントを閉じようとしているかがカウントされた。
            //次にそれぞれについて処理をはじめる
            PrepareCloseResult result = PrepareCloseResult.TerminateSession;
            foreach (SessionHost sh in sessions) {
                if (sh.CMP_ClosingDocumentCount == 0)
                    continue; //影響なし

                if (sh.CMP_ClosingDocumentCount == sh.DocumentCount) { //セッションの全ドキュメントを閉じる場合
                    PrepareCloseResult r = TerminateSession(sh.Session);
                    sh.CMP_PrepareCloseResult = r;
                    if (r == PrepareCloseResult.TerminateSession)
                        context.AddClosingSession(sh);
                    else if (r == PrepareCloseResult.Cancel)
                        result = PrepareCloseResult.Cancel; //一個でもキャンセルがあれば全体をキャンセルとする
                }
                else { //一部のドキュメントを閉じる。これが面倒
                    //TODO unsupported
                    Debug.Assert(false, "unsupported");
                }
            }

            //それらについてセッションを閉じる
            foreach (SessionHost sh in context.ClosingSessions) {
                CleanupSession(sh);
            }

            return result;
        }

        public void RefreshDocumentStatus(IPoderosaDocument document) {
            DocumentHost dh = FindDocumentHost(document);
            Debug.Assert(dh != null);
            IPoderosaForm f = ViewToForm(dh.LastAttachedView);

            //ちょっと汚い分岐
            IPoderosaMainWindow mw = (IPoderosaMainWindow)f.GetAdapter(typeof(IPoderosaMainWindow));
            if (mw != null)
                mw.DocumentTabFeature.Update(document);
            else
                ((IPoderosaPopupWindow)f.GetAdapter(typeof(IPoderosaPopupWindow))).UpdateStatus();
        }

        private void CleanupDocument(DocumentHost dh) {
            IPoderosaForm owner_window = ViewToForm(dh.LastAttachedView);
            IPoderosaView visible_view = dh.CurrentView;
            bool was_active = false;
            if (visible_view != null) {
                was_active = visible_view.AsControl().Focused;
                dh.DetachView();
                FireDocViewRelationChange();
            }

            if (owner_window != null) {
                NotifyRemove(owner_window, dh.Document);
            }

            dh.SessionHost.CloseDocument(dh.Document);
            _documentMap.Remove(dh.Document);

            //閉じたドキュメントのビューが見えていた場合は、その位置の別のドキュメントを見せる
            //TODO ウィンドウを閉じるときはこの処理は不要
            if (visible_view != null && visible_view.ParentForm.GetAdapter(typeof(IPoderosaMainWindow)) != null) {
                ShowBackgroundDocument(visible_view);
                if (was_active && visible_view.Document != null)
                    ActivateDocument(visible_view.Document, ActivateReason.InternalAction);
            }
        }
        internal void CleanupSession(SessionHost sh) { //SessionHostからも呼ばれるのでinternal
            foreach (ISessionListener listener in _sessionListeners)
                listener.OnSessionEnd(sh.Session);
            foreach (IPoderosaDocument doc in sh.ClonedDocuments) {
                CleanupDocument(FindDocumentHost(doc));
            }
            sh.Session.InternalTerminate();
            _sessionMap.Remove(sh.Session);
        }

        /**
         * Activate処理のルート
         * NOTE 重複コールはここでブロックするようにする。
         * アクティブなドキュメントが変化するのは、
         *   - Viewをクリックしてフォーカスが変わるとき
         *   - タブをクリックしたとき
         *   - キーボードショートカット等、Poderosaのコードが発動するとき
         * の３つ。
         * そのうちのどれであるかを指定してここを呼ぶ。例えば、Focus移動のときは改めてFocus()を呼ばないなど内部で場合分けがなされる
         */
        public void ActivateDocument(IPoderosaDocument document, ActivateReason reason) {
            Debug.Assert(document != null);

            //ネストの防止 Focus系イベントハンドラがあるとどうしても呼ばれてしまうので
            if (_activateContext != null)
                return;

            try {
                _activateContext = new ActivateContext(document, reason);

                DocumentHost dh = FindDocumentHost(document);
                Debug.Assert(dh != null);

                if (dh.CurrentView != null) { //既に見えている場合
                    if (reason != ActivateReason.ViewGotFocus)
                        SetFocusToView(dh.CurrentView); //ユーザのフォーカス指定だった場合はそれに任せる
                }
                else { //見えてはいなかった場合
                    IPoderosaView view = dh.LastAttachedView;
                    Debug.Assert(view != null); //これを強制する仕組みをどこかにほしいかも。今はすべてのDocumentが最初にAttachDocumentAndViewされることを想定している
                    AttachDocumentAndView(document, view);
                    Debug.Assert(dh.CurrentView == view);
                    if (!view.AsControl().Focused)
                        view.AsControl().Focus();
                }

                Debug.Assert(dh.CurrentView.Document == document);


                //通知
                NotifyActivation(ViewToForm(dh.CurrentView), document, reason);
            }
            finally {
                _activateContext = null;
                if (DebugOpt.DumpDocumentRelation)
                    DumpDocumentRelation();
            }
        }

        //SessionHostから呼ばれる系列
        public void RegisterDocument(IPoderosaDocument document, SessionHost sessionHost) {
            _documentMap.Add(document, new DocumentHost(this, sessionHost, document));
        }

        public DocumentHost FindDocumentHost(IPoderosaDocument document) {
            return _documentMap[document];
        }
        public SessionHost FindSessionHost(ISession session) {
            return _sessionMap[session];
        }

        internal IEnumerable<DocumentHost> GetAllDocumentHosts() {
            return new ConvertingEnumerable<DocumentHost>(_documentMap.Values);
        }

        //ViewのマージでのActivate処理
        public void ChangeLastAttachedViewForAllDocuments(IPoderosaView closing_view, IPoderosaView alternative) {
            closing_view = AdjustToOuterView(closing_view);
            alternative = AdjustToOuterView(alternative);
            foreach (DocumentHost dh in _documentMap.Values) {
                if (dh.LastAttachedView == closing_view) {
                    dh.AlternateView(alternative);
                    FireDocViewRelationChange();
                }
            }
        }
        public void ChangeLastAttachedViewForWindow(IPoderosaMainWindow window, IPoderosaView alternative) {
            alternative = AdjustToOuterView(alternative);
            foreach (DocumentHost dh in _documentMap.Values) {
                if (dh.LastAttachedView.ParentForm == window) {
                    dh.AlternateView(alternative);
                    FireDocViewRelationChange();
                }
            }
        }

        public void SetLastAttachedViewForAllDocuments(IPoderosaView view) {
            view = AdjustToOuterView(view);
            foreach (DocumentHost dh in _documentMap.Values) {
                dh.AlternateView(view);
            }
            FireDocViewRelationChange();
        }

        //viewの位置にある新規のドキュメントを見えるようにする。ドキュメントの表示位置を変えたとき、閉じたときに実行する
        private DocumentHost ShowBackgroundDocument(IPoderosaView view) {
            DocumentHost new_visible_doc = FindNewVisibleDoc(view);
            if (new_visible_doc != null) {
                new_visible_doc.AttachView(view);
            }
            return new_visible_doc;
        }

        private List<SessionHost> CreateSessionHostCollection() {
            List<SessionHost> r = new List<SessionHost>();
            foreach (object t in _sessionMap.Values)
                r.Add((SessionHost)t);
            return r;
        }

        private DocumentHost FindNewVisibleDoc(IPoderosaView view) {
            view = AdjustToOuterView(view);
            //TODO これはいい加減。このルールも、タブでの順番、アクティブになった順番を記憶、など複数の手段が考えられるし、プラグインで拡張すべきところ
            foreach (DocumentHost dh in _documentMap.Values) {
                if (dh.LastAttachedView == view)
                    return dh;
            }
            return null;
        }

        private IPoderosaForm ViewToForm(IPoderosaView view) {
            if (view == null)
                return null;
            else {
                return (IPoderosaForm)view.ParentForm.GetAdapter(typeof(IPoderosaForm));
            }
        }

        private void FireDocViewRelationChange() {
            foreach (IDocViewRelationEventHandler eh in _docViewRelationHandler.GetExtensions())
                eh.OnDocViewRelationChange();
        }

        //Listener
        public void AddActiveDocumentChangeListener(IActiveDocumentChangeListener listener) {
            _activeDocumentChangeListeners.Add(listener);
        }
        public void RemoveActiveDocumentChangeListener(IActiveDocumentChangeListener listener) {
            _activeDocumentChangeListeners.Remove(listener);
        }
        public void AddSessionListener(ISessionListener listener) {
            _sessionListeners.Add(listener);
        }
        public void RemoveSessionListener(ISessionListener listener) {
            _sessionListeners.Remove(listener);
        }

        private void NotifyActivation(IPoderosaForm form, IPoderosaDocument document, ActivateReason reason) {
            Debug.Assert(document != null);
            IPoderosaMainWindow window = (IPoderosaMainWindow)form.GetAdapter(typeof(IPoderosaMainWindow));

            if (window != null) {
                //Tabへの通知。TabClickのときはTabが自前で処理してるのでOK
                if (reason != ActivateReason.TabClick)
                    window.DocumentTabFeature.Activate(document);
                //listenerへの通知
                foreach (IActiveDocumentChangeListener listener in _activeDocumentChangeListeners)
                    listener.OnDocumentActivated(window, document);
            }
        }

        private void NotifyAdd(IPoderosaForm form, IPoderosaDocument document) {
            IPoderosaMainWindow window = (IPoderosaMainWindow)form.GetAdapter(typeof(IPoderosaMainWindow));
            if (window != null)
                window.DocumentTabFeature.Add(document);
        }

        private void NotifyRemove(IPoderosaForm form, IPoderosaDocument document) {
            IPoderosaMainWindow window = (IPoderosaMainWindow)form.GetAdapter(typeof(IPoderosaMainWindow));
            if (window != null) {
                IPoderosaDocument former = window.DocumentTabFeature.ActiveDocument;
                window.DocumentTabFeature.Remove(document);
                //TODO アクティブなのを記憶する場所を変えることでタブへの通知を先にする制約から解放される
                if (former == document) {
                    foreach (IActiveDocumentChangeListener listener in _activeDocumentChangeListeners)
                        listener.OnDocumentDeactivated(window);
                }
            }
        }

        private IPoderosaDocument ViewToActiveDocument(IPoderosaView view) {
            IPoderosaForm form = view.ParentForm;
            IPoderosaMainWindow window = (IPoderosaMainWindow)form.GetAdapter(typeof(IPoderosaMainWindow));
            if (window != null)
                return window.DocumentTabFeature.ActiveDocument;
            else
                return view.Document;
        }

        //ビューにフォーカスをセットした状態にする。ポップアップウィンドウの場合、まだウィンドウがロードされていないケースもあるのでそこに注意！
        private void SetFocusToView(IPoderosaView view) {
            IPoderosaForm form = view.ParentForm;
            IPoderosaPopupWindow popup = (IPoderosaPopupWindow)form.GetAdapter(typeof(IPoderosaPopupWindow));
            if (popup != null) {
                if (!popup.AsForm().Visible) {
                    popup.UpdateStatus();
                    popup.AsForm().Show();
                }
            }

            if (!view.AsControl().Focused)
                view.AsControl().Focus(); //既にウィンドウは見えている
        }

        private static IPoderosaView AdjustToOuterView(IPoderosaView view) {
            //ContentReplaceableSiteがあればその親を使用する
            IContentReplaceableViewSite s = (IContentReplaceableViewSite)view.GetAdapter(typeof(IContentReplaceableViewSite));
            if (s != null)
                return s.CurrentContentReplaceableView;
            else
                return view;
        }

        private void DumpDocumentRelation() {
            Debug.WriteLine("[DocRelation]");
            foreach (DocumentHost dh in _documentMap.Values) {
                Debug.WriteLine(String.Format("  doc {0}, current={1}, last={2}", dh.Document.GetType().Name, ViewName(dh.CurrentView), ViewName(dh.LastAttachedView)));
            }
        }
        private static string ViewName(IPoderosaView view) {
            if (view == null)
                return "null";
            else {
                IContentReplaceableView rv = (IContentReplaceableView)view.GetAdapter(typeof(IContentReplaceableView));
                if (rv != null)
                    return rv.GetCurrentContent().GetType().Name;
                else
                    return view.GetType().Name;
            }
        }
    }

    internal class SessionHost : ISessionHost {
        private SessionManagerPlugin _parent;
        private ISession _session;
        private List<IPoderosaDocument> _documents;

        //以下のメンバはSessionManager#CloseMultipleDocumentsにのみ使用。
        //スレッドセーフではなくなるがさすがに問題はないだろう
        private int _closingDocumentCount;
        private PrepareCloseResult _prepareCloseResult;

        public SessionHost(SessionManagerPlugin parent, ISession session) {
            _parent = parent;
            _session = session;
            _documents = new List<IPoderosaDocument>();
        }

        public ISession Session {
            get {
                return _session;
            }
        }
        public int DocumentCount {
            get {
                return _documents.Count;
            }
        }
        public IEnumerable<IPoderosaDocument> Documents {
            get {
                return _documents;
            }
        }
        public IPoderosaDocument DocumentAt(int index) {
            return _documents[index];
        }
        public IEnumerable<IPoderosaDocument> ClonedDocuments {
            get {
                return new List<IPoderosaDocument>(_documents);
            }
        }

        #region ISessionHost
        public void RegisterDocument(IPoderosaDocument document) {
            _parent.RegisterDocument(document, this);
            _documents.Add(document);
        }
        //ホストしているセッションが自発的に終了する場合
        public void TerminateSession() {
            _parent.CleanupSession(this);
        }
        public IPoderosaForm GetParentFormFor(IPoderosaDocument document) {
            DocumentHost dh = _parent.FindDocumentHost(document);
            Debug.Assert(dh != null, "document must be alive");
            IPoderosaView view = dh.LastAttachedView;
            if (view != null)
                return view.ParentForm; //これが存在するならOK
            else
                return WindowManagerPlugin.Instance.ActiveWindow; //ちょっと反則気味の取り方だが
        }
        #endregion

        public void CloseDocument(IPoderosaDocument document) {
            Debug.Assert(_documents.Contains(document));
            _session.InternalCloseDocument(document);
            _documents.Remove(document);
        }

        //以下はCloseMultipleDocument内で使用する
        public int CMP_ClosingDocumentCount {
            get {
                return _closingDocumentCount;
            }
            set {
                _closingDocumentCount = value;
            }
        }
        public PrepareCloseResult CMP_PrepareCloseResult {
            get {
                return _prepareCloseResult;
            }
            set {
                _prepareCloseResult = value;
            }
        }
    }

    internal class DocumentHost {
        private SessionManagerPlugin _manager;
        private SessionHost _sessionHost;
        private IPoderosaDocument _document;
        private IPoderosaView _currentView;
        private IPoderosaView _lastAttachedView;

        public DocumentHost(SessionManagerPlugin manager, SessionHost sessionHost, IPoderosaDocument document) {
            _manager = manager;
            _sessionHost = sessionHost;
            _document = document;
        }

        public IPoderosaView LastAttachedView {
            get {
                return _lastAttachedView;
            }
        }

        public IPoderosaView CurrentView {
            get {
                return _currentView;
            }
        }
        public SessionHost SessionHost {
            get {
                return _sessionHost;
            }
        }
        public IPoderosaDocument Document {
            get {
                return _document;
            }
        }

        //ビューとの関連付け変更
        public void AttachView(IPoderosaView view) {
            _lastAttachedView = view;
            _currentView = view;

            IViewFactory vf = WindowManagerPlugin.Instance.ViewFactoryManager.GetViewFactoryByDoc(_document.GetType());
            IContentReplaceableView rv = (IContentReplaceableView)view.GetAdapter(typeof(IContentReplaceableView));
            IPoderosaView internalview = rv == null ? view : rv.AssureViewClass(vf.GetViewType()); //ContentReplaceableViewのときは中身を使用
            Debug.Assert(vf.GetViewType() == internalview.GetType());
            _sessionHost.Session.InternalAttachView(_document, internalview);
        }
        public void DetachView() {
            Debug.Assert(_currentView != null);
            IContentReplaceableView rv = (IContentReplaceableView)_currentView.GetAdapter(typeof(IContentReplaceableView));
            IPoderosaView internalview = rv == null ? _currentView : rv.GetCurrentContent(); //ContentReplaceableViewのときは中身を使用
            _sessionHost.Session.InternalDetachView(_document, internalview);

            if (rv != null && rv.AsControl().Visible)
                rv.AssureEmptyViewClass();

            _currentView = null;
        }

        //Viewが閉じられるなどで代替のビューに置換する
        public void AlternateView(IPoderosaView view) {
            if (_currentView != null)
                DetachView();
            _lastAttachedView = view;
        }



    }

    internal class ClosingContext {
        private enum CloseType {
            OneDocument,
            OneSession,
            OneWindow,
            AllWindows
        }

        private CloseType _type;
        private IPoderosaMainWindow _window; //_type==OneWindowのときのみセット、他のときはnull
        private List<SessionHost> _closingSessions;

        public ClosingContext(IPoderosaMainWindow window) {
            _type = CloseType.OneWindow;
            _window = window;
            _closingSessions = new List<SessionHost>();
        }

        public void AddClosingSession(SessionHost sh) {
            Debug.Assert(_type == CloseType.OneWindow);
            _closingSessions.Add(sh);
        }
        public IEnumerable<SessionHost> ClosingSessions {
            get {
                return _closingSessions;
            }
        }
    }

    internal class ActivateContext {
        private IPoderosaDocument _document;
        private ActivateReason _reason;

        public ActivateContext(IPoderosaDocument document, ActivateReason reason) {
            _document = document;
            _reason = reason;
        }
    }
}
