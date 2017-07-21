/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: TerminalSession.cs,v 1.4 2012/02/19 09:11:28 kzmi Exp $
 */
using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using System.Diagnostics;

using Poderosa.Document;
using Poderosa.Terminal;
using Poderosa.View;
using Poderosa.Protocols;
using Poderosa.ConnectionParam;
using Poderosa.Forms;
using Poderosa.Util;

namespace Poderosa.Sessions {
    //NOTE Invalidateに必要なパラメータ これも意図がいまいちだなあ
    internal class InvalidateParam {
        private Delegate _delegate;
        private object[] _param;
        private bool _set;
        public void Set(Delegate d, object[] p) {
            _delegate = d;
            _param = p;
            _set = true;
        }
        public void Reset() {
            _set = false;
        }
        public void InvokeFor(Control c) {
            if (_set)
                c.Invoke(_delegate, _param);
        }
    }

    //接続に対して関連付けるデータ
    public class TerminalSession : ITerminalSession, IAbstractTerminalHost, ITerminalControlHost {
        private delegate void HostCauseCloseDelagate(string msg);

        private ISessionHost _sessionHost;
        private TerminalTransmission _output;
        private AbstractTerminal _terminal;
        private ITerminalSettings _terminalSettings;
        private TerminalControl _terminalControl;
        private bool _terminated;

        public TerminalSession(ITerminalConnection connection, ITerminalSettings terminalSettings) {
            _terminalSettings = terminalSettings;
            //VT100指定でもxtermシーケンスを送ってくるアプリケーションが後をたたないので
            _terminal = AbstractTerminal.Create(new TerminalInitializeInfo(this, connection.Destination));
            _output = new TerminalTransmission(_terminal, _terminalSettings, connection);

            _terminalSettings.ChangeCaption += delegate(string caption) {
                this.OwnerWindow.DocumentTabFeature.Update(_terminal.IDocument);
            };

        }

        public void Revive(ITerminalConnection connection) {
            TerminalDocument doc = _terminal.GetDocument();
            _output.Revive(connection, doc.TerminalWidth, doc.TerminalHeight);
            this.OwnerWindow.DocumentTabFeature.Update(_terminal.IDocument);
            _output.Connection.Socket.RepeatAsyncRead(_terminal); //再受信
        }

        //IAdaptable
        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        #region ITerminalSession
        public AbstractTerminal Terminal {
            get {
                return _terminal;
            }
        }
        public TerminalControl TerminalControl {
            get {
                return _terminalControl;
            }
        }
        public IPoderosaMainWindow OwnerWindow {
            get {
                if (_terminated)
                    return TerminalSessionsPlugin.Instance.WindowManager.ActiveWindow; //終了しているときはSessionHost等も取得不能
                else
                {
                    IPoderosaForm parentForm = _sessionHost.GetParentFormFor(_terminal.IDocument);

                    if (parentForm == null)
                    {
                        return null;
                    }

                    return (IPoderosaMainWindow)parentForm.GetAdapter(typeof(IPoderosaMainWindow));
                }
            }
        }
        public ITerminalConnection TerminalConnection {
            get {
                return _output.Connection;
            }
        }
        public ITerminalSettings TerminalSettings {
            get {
                return _terminalSettings;
            }
        }
        public TerminalTransmission TerminalTransmission {
            get {
                return _output;
            }
        }
        public ISession ISession {
            get {
                return this;
            }
        }
        /*
        public ILogService LogService {
            get {
                return _terminal.ILogService;
            }
        }*/
        #endregion

        //受信スレッドから呼ぶ、Document更新の通知
        public void NotifyViewsDataArrived() {
            if (_terminalControl != null)
                _terminalControl.DataArrived();
        }
        //正常・異常とも呼ばれる
        public void CloseByReceptionThread(string msg) {
            if (_terminated)
                return;
            IPoderosaMainWindow window = this.OwnerWindow;
            if (window != null) {
                Debug.Assert(window.AsControl().InvokeRequired);
                //TerminalSessionはコントロールを保有しないので、ウィンドウで代用する
                window.AsControl().Invoke(new HostCauseCloseDelagate(HostCauseClose), msg);
            }
        }
        private void HostCauseClose(string msg) {
            if (TerminalSessionsPlugin.Instance.TerminalEmulatorService.TerminalEmulatorOptions.CloseOnDisconnect)
                _sessionHost.TerminateSession();
            else {
                IPoderosaMainWindow window = this.OwnerWindow;
                window.DocumentTabFeature.Update(_terminal.IDocument);
            }
        }

        //ISession
        public string Caption {
            get {
                string s = _terminalSettings.Caption;
                if (_output.Connection.IsClosed)
                    s += TEnv.Strings.GetString("Caption.Disconnected");
                return s;
            }
        }
        public Image Icon {
            get {
                return _terminalSettings.Icon;
            }
        }
        //TerminalSessionの開始
        public void InternalStart(ISessionHost host) {
            _sessionHost = host;
            host.RegisterDocument(_terminal.IDocument);
            _output.Connection.Socket.RepeatAsyncRead(_terminal);
        }
        public void InternalTerminate() {
            _terminated = true;
            try {
                _output.Connection.Close();
                _output.Connection.Socket.ForceDisposed();
            }
            catch (Exception) { //ここでの例外は無視
            }
            _terminal.CloseBySession();
        }
        public PrepareCloseResult PrepareCloseDocument(IPoderosaDocument document) {
            Debug.Assert(document == _terminal.IDocument);
            return PrepareCloseResult.TerminateSession;
        }
        public PrepareCloseResult PrepareCloseSession() {
            if (TerminalSessionsPlugin.Instance.TerminalSessionOptions.AskCloseOnExit && !_output.Connection.IsClosed) {
                if (this.OwnerWindow.AskUserYesNo(String.Format(TEnv.Strings.GetString("Message.AskCloseTerminalSession"), this.Caption)) == DialogResult.Yes)
                    return PrepareCloseResult.TerminateSession;
                else
                    return PrepareCloseResult.Cancel;
            }
            else
                return PrepareCloseResult.TerminateSession;
        }

        public void InternalAttachView(IPoderosaDocument document, IPoderosaView view) {
            Debug.WriteLineIf(DebugOpt.ViewManagement, "ATTACH VIEW");
            Debug.Assert(document == _terminal.IDocument);
            TerminalView tv = (TerminalView)view.GetAdapter(typeof(TerminalView));
            Debug.Assert(tv != null);
            TerminalControl tp = tv.TerminalControl;
            Debug.Assert(tp != null);
            tp.Attach(this);

            _terminalControl = tp;
            _terminal.Attached(tp);
        }
        public void InternalDetachView(IPoderosaDocument document, IPoderosaView view) {
            Debug.WriteLineIf(DebugOpt.ViewManagement, "DETACH VIEW");
            Debug.Assert(document == _terminal.IDocument);
            TerminalView tv = (TerminalView)view.GetAdapter(typeof(TerminalView));
            Debug.Assert(tv != null);
            TerminalControl tp = tv.TerminalControl;
            Debug.Assert(tp != null); //Detachするときにはこのビューになっている必要あり

            if (!tp.IsDisposed) {
                _terminal.Detach(tp);
                tp.Detach();
            }

            _terminalControl = null;
        }
        public void InternalCloseDocument(IPoderosaDocument document) {
            //do nothing
        }
#if false //廃止
        //ビューからのコントロールの取得
        private static TerminalControl CastTerminalControl(IPoderosaView view) {
            IContentReplaceableView rv = (IContentReplaceableView)view.GetAdapter(typeof(IContentReplaceableView));
            Debug.Assert(rv!=null); //現状では分割方式でしか動作していないのでここまでは必ず成功
            IPoderosaView content = rv.GetCurrentContent();

            if(content is TerminalView)
                return ((TerminalView)content).TerminalControl;
            else
                return null;
        }
        private static TerminalControl CastOrCreateTerminalControl(IPoderosaView view) {
            TerminalControl c = CastTerminalControl(view);
            if(c!=null) return c; //キャストできればそれでOK。でなければ作る

            Debug.WriteLine("Creating New TerminalControl");
            IContentReplaceableView rv = (IContentReplaceableView)view.GetAdapter(typeof(IContentReplaceableView));
            TerminalControl tc = new TerminalControl();
            rv.ReplaceContent(new TerminalView(view.ParentForm, tc));
            return tc;
        }
#endif
    }

}
