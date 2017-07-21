/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: StartCommands.cs,v 1.5 2011/10/27 23:21:59 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Plugins;
using Poderosa.Forms;
using Poderosa.Protocols;
using Poderosa.Commands;
using Poderosa.Terminal;

namespace Poderosa.Sessions {

    internal class ConnectCommandCategory : ICommandCategory, IPositionDesignation {
        public string Name {
            get {
                return TEnv.Strings.GetString("CommandCategory.Connection");
            }
        }

        public bool IsKeybindCustomizable {
            get {
                return true;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        public static ConnectCommandCategory _instance = new ConnectCommandCategory();

        public IAdaptable DesignationTarget {
            get {
                return TerminalSessionsPlugin.Instance.CommandManager.CommandCategories.File;
            }
        }

        public PositionType DesignationPosition {
            get {
                return PositionType.NextTo;
            }
        }
    }

    //新しいTerminalSessionをスタートするためのコマンド
    internal class StartCommand : ITerminalSessionStartCommand {
        private enum StartCommandIcon {
            NewConnection,
            Cygwin,
        }

        public StartCommand(IExtensionPoint pt) {
            pt.RegisterExtension(new CygwinConnectionFactory());
            pt.RegisterExtension(new SSHConnectionFactory());
            pt.RegisterExtension(new TelnetConnectionFactory());
        }

        //IPoderosaCommand
        public bool CanExecute(ICommandTarget target) {
            //targetの型チェックくらいはしたほうがいいか？
            return true;
        }
        public CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
            if (args.Length != 2)
                StartCommandArgError();
            ITerminalParameter param = (ITerminalParameter)args[0].GetAdapter(typeof(ITerminalParameter));
            ITerminalConnection connection = (ITerminalConnection)args[0].GetAdapter(typeof(ITerminalConnection));
            ITerminalSettings settings = (ITerminalSettings)args[1].GetAdapter(typeof(ITerminalSettings));
            if ((param == null && connection == null) || settings == null)
                StartCommandArgError();

            ITerminalSession result = null;
            if (connection != null)
                result = StartTerminalSession(target, connection, settings);
            else
                result = StartTerminalSession(target, param, settings);

            return result != null ? CommandResult.Succeeded : CommandResult.Failed;
        }

        //TODO ましなエラーメッセージ
        private void StartCommandArgError() {
            throw new ArgumentException("TerminalSessionStartCommand error");
        }

        //基本のスタートセッション
        public ITerminalSession StartTerminalSession(ICommandTarget target, ITerminalConnection connection, ITerminalSettings settings) {
            Debug.Assert(connection != null);
            Debug.Assert(settings != null);
            //ここでターミナルエミュレータの遅延初期化
            TerminalSessionsPlugin.Instance.TerminalEmulatorService.LaterInitialize();

            ISessionManager sm = (ISessionManager)TerminalSessionsPlugin.Instance.PoderosaWorld.PluginManager.FindPlugin("org.poderosa.core.sessions", typeof(ISessionManager));

            IPoderosaView view = ToPoderosaView(target);
            Debug.Assert(view != null);

            TerminalSession session = new TerminalSession(connection, settings);
            sm.StartNewSession(session, view);
            sm.ActivateDocument(session.Terminal.IDocument, ActivateReason.InternalAction);

            IAutoExecMacroParameter autoExecParam = connection.Destination.GetAdapter(typeof(IAutoExecMacroParameter)) as IAutoExecMacroParameter;
            if (autoExecParam != null && autoExecParam.AutoExecMacroPath != null && TelnetSSHPlugin.Instance.MacroEngine != null) {
                TelnetSSHPlugin.Instance.MacroEngine.RunMacro(autoExecParam.AutoExecMacroPath, session);
            }

            return session;
        }

        //Parameterからスタートするタイプ 今はtargetはwindow強制だがViewでも可能にしたい
        public ITerminalSession StartTerminalSession(ICommandTarget target, ITerminalParameter destination, ITerminalSettings settings) {
            IPoderosaMainWindow window = (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
            if (window == null) {
                IPoderosaView view = (IPoderosaView)target.GetAdapter(typeof(IPoderosaView));
                window = (IPoderosaMainWindow)view.ParentForm.GetAdapter(typeof(IPoderosaMainWindow));
            }
            Debug.Assert(window != null);

            ITerminalConnection connection = OpenConnection(window, destination, settings);
            if (connection == null)
                return null;

            return StartTerminalSession(target, connection, settings);

        }

        public ITerminalConnection OpenConnection(IPoderosaMainWindow owner, ITerminalParameter destination, ITerminalSettings settings) {
            //NOTE 同時接続数チェックあたりあってもいい

            ITerminalConnectionFactory[] fs = (ITerminalConnectionFactory[])TerminalSessionsPlugin.Instance.PoderosaWorld.PluginManager.FindExtensionPoint(TerminalSessionsPlugin.TERMINAL_CONNECTION_FACTORY_ID).GetExtensions();
            //後に登録されたやつを優先するため、逆順に舐める
            for (int i = fs.Length - 1; i >= 0; i--) {
                ITerminalConnectionFactory f = fs[i];
                if (f.IsSupporting(destination, settings)) {
                    return f.EstablishConnection(owner, destination, settings);
                }
            }
            throw new ArgumentException("Failed to make an ITerminalConnection using extension point."); //ましなエラーメッセージ
        }

        public void OpenShortcutFile(ICommandTarget target, string filename) {
            ShortcutFileCommands.OpenShortcutFile(target, filename);
        }

        private class CygwinConnectionFactory : ITerminalConnectionFactory {
            public bool IsSupporting(ITerminalParameter param, ITerminalSettings settings) {
                ICygwinParameter cygwin = (ICygwinParameter)param.GetAdapter(typeof(ICygwinParameter));
                return cygwin != null;
            }

            public ITerminalConnection EstablishConnection(IPoderosaMainWindow window, ITerminalParameter destination, ITerminalSettings settings) {
                ICygwinParameter cygwin = (ICygwinParameter)destination.GetAdapter(typeof(ICygwinParameter));
                IProtocolService ps = TerminalSessionsPlugin.Instance.ProtocolService;
                ISynchronizedConnector sc = ps.CreateFormBasedSynchronozedConnector(window);
                IInterruptable t = ps.AsyncCygwinConnect(sc.InterruptableConnectorClient, cygwin);
                ITerminalConnection con = sc.WaitConnection(t, TerminalSessionsPlugin.Instance.TerminalSessionOptions.TerminalEstablishTimeout);

                AdjustCaptionAndText(settings, cygwin.ShellBody, StartCommandIcon.Cygwin);
                return con;
            }
        }

        private class TelnetConnectionFactory : ITerminalConnectionFactory {
            public bool IsSupporting(ITerminalParameter param, ITerminalSettings settings) {
                ITCPParameter tcp = (ITCPParameter)param.GetAdapter(typeof(ITCPParameter));
                ISSHLoginParameter ssh = (ISSHLoginParameter)param.GetAdapter(typeof(ISSHLoginParameter));
                return tcp != null && ssh == null; //SSHならSSHを使う。
            }
            public ITerminalConnection EstablishConnection(IPoderosaMainWindow window, ITerminalParameter destination, ITerminalSettings settings) {
                ITCPParameter tcp = (ITCPParameter)destination.GetAdapter(typeof(ITCPParameter));
                IProtocolService ps = TerminalSessionsPlugin.Instance.ProtocolService;
                ISynchronizedConnector sc = ps.CreateFormBasedSynchronozedConnector(window);
                IInterruptable t = ps.AsyncTelnetConnect(sc.InterruptableConnectorClient, tcp);
                ITerminalConnection con = sc.WaitConnection(t, TerminalSessionsPlugin.Instance.TerminalSessionOptions.TerminalEstablishTimeout);

                AdjustCaptionAndText(settings, tcp.Destination, StartCommandIcon.NewConnection);
                return con;
            }
        }

        private class SSHConnectionFactory : ITerminalConnectionFactory {
            public bool IsSupporting(ITerminalParameter destination, ITerminalSettings settings) {
                ISSHLoginParameter ssh = (ISSHLoginParameter)destination.GetAdapter(typeof(ISSHLoginParameter));
                return ssh != null;
            }
            public ITerminalConnection EstablishConnection(IPoderosaMainWindow window, ITerminalParameter destination, ITerminalSettings settings) {
                ISSHLoginParameter ssh = (ISSHLoginParameter)destination.GetAdapter(typeof(ISSHLoginParameter));
                if (ssh.LetUserInputPassword && ssh.AuthenticationType != Granados.AuthenticationType.KeyboardInteractive) { //ダイアログで入力を促して接続
                    SSHShortcutLoginDialog dlg = new SSHShortcutLoginDialog(window, ssh, settings);
                    if (dlg.ShowDialog(window.AsForm()) == DialogResult.OK) {
                        ITerminalConnection con = dlg.Result;
                        AdjustCaptionAndText(settings, ((ITCPParameter)con.Destination.GetAdapter(typeof(ITCPParameter))).Destination, StartCommandIcon.NewConnection);
                        return con;
                    }
                    else
                        return null;
                }
                else { //主にReproduceやマクロ。設定済みのパスワードで接続
                    IProtocolService protocolservice = TerminalSessionsPlugin.Instance.ProtocolService;
                    ISynchronizedConnector conn = protocolservice.CreateFormBasedSynchronozedConnector(window);
                    IInterruptable r = protocolservice.AsyncSSHConnect(conn.InterruptableConnectorClient, ssh);
                    AdjustCaptionAndText(settings, ((ITCPParameter)destination.GetAdapter(typeof(ITCPParameter))).Destination, StartCommandIcon.NewConnection);
                    return conn.WaitConnection(r, TerminalSessionsPlugin.Instance.TerminalSessionOptions.TerminalEstablishTimeout); //時間？
                }
            }
        }


        private static void AdjustCaptionAndText(ITerminalSettings terminal_settings, string caption, StartCommandIcon icon) {
            terminal_settings.BeginUpdate();
            if (terminal_settings.Caption == null || terminal_settings.Caption.Length == 0)
                terminal_settings.Caption = caption; //長さ０はいかん
            if (terminal_settings.Icon == null) {
                switch (icon) {
                    case StartCommandIcon.NewConnection:
                        terminal_settings.Icon = Poderosa.TerminalSession.Properties.Resources.NewConnection16x16;
                        break;
                    case StartCommandIcon.Cygwin:
                        terminal_settings.Icon = Poderosa.TerminalSession.Properties.Resources.Cygwin16x16;
                        break;
                }
            }
            terminal_settings.EndUpdate();

        }

        private static IPoderosaView ToPoderosaView(ICommandTarget target) {
            IPoderosaMainWindow window = (IPoderosaMainWindow)target.GetAdapter(typeof(IPoderosaMainWindow));
            IPoderosaView view;
            if (window != null)
                view = window.ViewManager.GetCandidateViewForNewDocument();
            else {
                view = (IPoderosaView)target.GetAdapter(typeof(IPoderosaView));
                Debug.Assert(view != null);
            }

            IContentReplaceableView rv = (IContentReplaceableView)view.GetAdapter(typeof(IContentReplaceableView));
            if (rv != null)
                view = rv.AssureViewClass(typeof(TerminalView));
            return view;
        }

        #region IAdaptable
        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion
    }
}
