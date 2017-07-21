/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CommandResultPopupSession.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;

using Poderosa.Forms;
using Poderosa.Sessions;
using Poderosa.View;
using Poderosa.Document;
using Poderosa.Plugins;
using Poderosa.Commands;

namespace Poderosa.Terminal {
    internal class CommandResultSession : ISession {
        public delegate void StartDelegate(AbstractTerminal termianl, CommandResultDocument document);

        private CommandResultViewerControl _view;
        private CommandResultDocument _document;
        private RenderProfile _renderProfile;
        private ISessionHost _host;
        private static StartDelegate _start = new StartDelegate(SessionEntryPoint);

        public CommandResultSession(CommandResultDocument doc, RenderProfile prof) {
            _document = doc;
            _renderProfile = prof;
        }

        public string Caption {
            get {
                return _document.Caption;
            }
        }

        public Image Icon {
            get {
                return _document.Icon;
            }
        }

        public void InternalStart(ISessionHost host) {
            _host = host;
            _host.RegisterDocument(_document);
        }

        public void InternalTerminate() {
        }

        public PrepareCloseResult PrepareCloseDocument(IPoderosaDocument document) {
            return PrepareCloseResult.TerminateSession;
        }

        public PrepareCloseResult PrepareCloseSession() {
            return PrepareCloseResult.TerminateSession;
        }

        public void InternalAttachView(IPoderosaDocument document, IPoderosaView view) {
            _view = (CommandResultViewerControl)view.GetAdapter(typeof(CommandResultViewerControl));
            Debug.Assert(_view != null);
            _view.SetParent(this);
        }

        public void InternalDetachView(IPoderosaDocument document, IPoderosaView view) {
            _view = null;
        }

        public void InternalCloseDocument(IPoderosaDocument document) {
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        public CommandResultViewerControl CurrentView {
            get {
                return _view;
            }
        }
        public bool IsWindowVisible {
            get {
                return _view != null;
            }
        }

        public CommandResultDocument Document {
            get {
                return _document;
            }
        }
        public RenderProfile RenderProfile {
            get {
                return _renderProfile;
            }
        }

        //開始点
        public static StartDelegate Start {
            get {
                return _start;
            }
        }

        private static void SessionEntryPoint(AbstractTerminal terminal, CommandResultDocument document) {
            try {
                TerminalControl tc = terminal.TerminalHost.TerminalControl;
                Debug.Assert(tc != null);
                RenderProfile rp = (RenderProfile)tc.GetRenderProfile().Clone();
                CommandResultSession session = new CommandResultSession(document, rp); //現在のRenderProfileを使ってセッションを作る
                TerminalDocument terminaldoc = terminal.GetDocument();
                PopupViewCreationParam cp = new PopupViewCreationParam(_viewFactory);
                //結果のサイズに合わせる。ただし高さは20行を上限とする
                cp.InitialSize = new Size(tc.ClientSize.Width, (int)(RuntimeUtil.AdjustIntRange(document.Size, 0, 20) * rp.Pitch.Height) + 2);
                cp.OwnedByCommandTargetWindow = GEnv.Options.CommandPopupAlwaysOnTop;
                cp.ShowInTaskBar = GEnv.Options.CommandPopupInTaskBar;

                IWindowManager wm = TerminalEmulatorPlugin.Instance.GetWindowManager();
                ISessionManager sm = TerminalEmulatorPlugin.Instance.GetSessionManager();
                IPoderosaPopupWindow window = wm.CreatePopupView(cp);
                sm.StartNewSession(session, window.InternalView);
                sm.ActivateDocument(session.Document, ActivateReason.InternalAction);
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }

        //起動時の初期化
        private static CommandResultViewerFactory _viewFactory;

        public static void Init(IPoderosaWorld world) {
            _viewFactory = new CommandResultViewerFactory();
            world.PluginManager.FindExtensionPoint(WindowManagerConstants.VIEW_FACTORY_ID).RegisterExtension(_viewFactory);
        }

    }

    //ViewClass
    internal class CommandResultViewerControl : CharacterDocumentViewer, IPoderosaView, IGeneralViewCommands {
        private IPoderosaForm _form;
        private CommandResultSession _session;

        public CommandResultViewerControl(IPoderosaForm form) {
            _form = form;
            _caret.Enabled = false;
            _caret.Blink = false;
        }

        public void SetParent(CommandResultSession session) {
            _session = session;
            this.SetPrivateRenderProfile(session.RenderProfile);
            this.SetContent(_session.Document);
        }

        public IPoderosaDocument Document {
            get {
                return _session == null ? null : _session.Document;
            }
        }

        public ISelection CurrentSelection {
            get {
                return this.ITextSelection;
            }
        }

        public IPoderosaForm ParentForm {
            get {
                return this.FindForm() as IPoderosaForm;
            }
        }

        //Command
        public IPoderosaCommand Copy {
            get {
                return TerminalEmulatorPlugin.Instance.GetWindowManager().SelectionService.DefaultCopyCommand;
            }
        }

        public IPoderosaCommand Paste {
            get {
                return null; //ペースト不可
            }
        }

        //ESCで閉じる
        protected override bool ProcessDialogKey(Keys keyData) {
            if (keyData == Keys.Escape) {
                this.FindForm().Close();
                return true;
            }
            else
                return base.ProcessDialogKey(keyData);
        }



    }

    //DocClass
    internal class CommandResultDocument : CharacterDocument {

        public CommandResultDocument(string title) {
            _caption = title;
        }
    }

    //ViewFactory
    internal class CommandResultViewerFactory : IViewFactory {
        public IPoderosaView CreateNew(IPoderosaForm parent) {
            return new CommandResultViewerControl(parent);
        }

        public Type GetViewType() {
            return typeof(CommandResultViewerControl);
        }

        public Type GetDocumentType() {
            return typeof(CommandResultDocument);
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }
}
