/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: Reproduce.cs,v 1.2 2011/10/27 23:21:58 kzmi Exp $
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
    internal class ReproduceCommand : IGeneralCommand {
        public string CommandID {
            get {
                return "org.poderosa.terminalsession.reproduce";
            }
        }

        public string Description {
            get {
                return TEnv.Strings.GetString("Command.TerminalReproduce");
            }
        }

        public Keys DefaultShortcutKey {
            get {
                return Keys.Alt | Keys.R;
            }
        }

        public ICommandCategory CommandCategory {
            get {
                return TerminalSessionsPlugin.Instance.TerminalEmulatorService.TerminalCommandCategory;
            }
        }

        public CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
            ITerminalSession ts = AsTerminalSession(target);
            if (ts == null)
                return CommandResult.Failed;

            //閉じてるかどうかで再接続か複製
            if (ts.TerminalTransmission.Connection.IsClosed)
                return ConnectAgain(ts);
            else
                return Reproduce(ts);

        }
        private CommandResult Reproduce(ITerminalSession ts) {
            //TODO SSH2では同一コネクションでもう一本張るショートカット、シリアルでは複製できないことを通知するショートカットがあるが...
            ITerminalParameter param = (ITerminalParameter)ts.TerminalTransmission.Connection.Destination.Clone();
            ISSHLoginParameter ssh = (ISSHLoginParameter)param.GetAdapter(typeof(ISSHLoginParameter));
            if (ssh != null)
                ssh.LetUserInputPassword = false;
            ITerminalSettings settings = ts.TerminalSettings.Clone();

            ITerminalSession session = TerminalSessionsPlugin.Instance.TerminalSessionStartCommand.StartTerminalSession(ts.OwnerWindow, param, settings);
            return session != null ? CommandResult.Succeeded : CommandResult.Failed;
        }
        private CommandResult ConnectAgain(ITerminalSession ts0) {
            TerminalSession ts = (TerminalSession)ts0.GetAdapter(typeof(TerminalSession));
            ITerminalParameter param = (ITerminalParameter)ts.TerminalTransmission.Connection.Destination.Clone();
            ISSHLoginParameter ssh = (ISSHLoginParameter)param.GetAdapter(typeof(ISSHLoginParameter));
            if (ssh != null)
                ssh.LetUserInputPassword = false;

            ITerminalConnection connection = TerminalSessionsPlugin.Instance.TerminalSessionStartCommand.OpenConnection(ts.OwnerWindow, param, ts.TerminalSettings);
            if (connection == null)
                return CommandResult.Failed;

            ts.Revive(connection); //接続を復活
            return CommandResult.Succeeded;
        }

        public bool CanExecute(ICommandTarget target) {
            ITerminalSession ts = AsTerminalSession(target);
            return ts != null/* && !ts.TerminalTransmission.Connection.IsClosed*/;
        }


        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        internal static ITerminalSession AsTerminalSession(ICommandTarget target) {
            IPoderosaDocument doc = CommandTargetUtil.AsDocumentOrViewOrLastActivatedDocument(target);
            if (doc == null)
                return null;
            return doc.OwnerSession.GetAdapter(typeof(ITerminalSession)) as ITerminalSession;
        }
    }

    internal class ReproduceMenuGroup : IPoderosaMenuGroup, IPositionDesignation {
        public IPoderosaMenu[] ChildMenus {
            get {
                return new IPoderosaMenu[] {
                    new ReproduceMenuItem()
                };
            }
        }
        public bool IsVolatileContent {
            get {
                return true; //テキストが可変
            }
        }

        public bool ShowSeparator {
            get {
                return true;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        public IAdaptable DesignationTarget {
            get {
                return null;
            }
        }

        public PositionType DesignationPosition {
            get {
                return PositionType.Last;
            }
        }

        private class ReproduceMenuItem : IPoderosaMenuItem {
            private ICommandTarget _lastCommandTarget;
            public IPoderosaCommand AssociatedCommand {
                get {
                    return TerminalSessionsPlugin.Instance.ReproduceCommand;
                }
            }

            public string Text {
                get {
                    //テキストが可変なのは想定外なので、IsEnabledが先に呼ばれるはずという隠し仕様に依存
                    ITerminalSession ts = ReproduceCommand.AsTerminalSession(_lastCommandTarget);
                    return TEnv.Strings.GetString((ts != null && !ts.TerminalConnection.IsClosed) ? "Menu.ReproduceTerminal" : "Menu.ReviveTerminal");
                }
            }

            public bool IsEnabled(ICommandTarget target) {
                _lastCommandTarget = target;
                return this.AssociatedCommand.CanExecute(target);
            }

            public bool IsChecked(ICommandTarget target) {
                return false;
            }
            public IAdaptable GetAdapter(Type adapter) {
                return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
            }
        }
    }

}
