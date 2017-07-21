/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: AutoShellExecutor.cs,v 1.2 2011/10/27 23:21:58 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

using Poderosa.Plugins;
using Poderosa.View;
using Poderosa.Forms;
using Poderosa.Document;
using Poderosa.Commands;
using Poderosa.Terminal;

namespace Poderosa.Sessions {
    //コマンドの逐次実行。マクロで実行するパターンの進化形。
    public class AutoShellExecutionCommand : IPoderosaCommand, ICommandResultProcessor {
        //メインスレッドで実行するコマンド結果の処理
        public delegate void MainThreadAction(string[] command_result);
        //受信スレッドで実行するコマンド結果の処理
        public delegate void ReceiverThreadAction(string[] command_result);

        //コマンド実行単位
        public class Action {
            private string _command;
            private ITerminalSession _target;
            private MainThreadAction _mainThreadAction; //これら２つは片方のみに値をセット
            private ReceiverThreadAction _receiverThreadAction;

            public Action(ITerminalSession target, string command, MainThreadAction ma) {
                _target = target;
                _command = command;
                _mainThreadAction = ma;
            }
            public Action(ITerminalSession target, string command, ReceiverThreadAction ra) {
                _target = target;
                _command = command;
                _receiverThreadAction = ra;
            }

            public ITerminalSession TargetSession {
                get {
                    return _target;
                }
            }

            public string CommandString {
                get {
                    return _command;
                }
            }

            internal MainThreadAction MainThreadAction {
                get {
                    return _mainThreadAction;
                }
            }
            internal ReceiverThreadAction ReceiverThreadAction {
                get {
                    return _receiverThreadAction;
                }
            }
        }

        private IPoderosaMainWindow _window;
        private LinkedList<Action> _actions;
        private Action _currentAction;

        public AutoShellExecutionCommand() {
            _actions = new LinkedList<Action>();
        }
        //アクションを通知
        public void AddAction(Action action) {
            _actions.AddLast(action);
        }
        public Action CurrentAction {
            get {
                return _currentAction;
            }
        }

        #region IPoderosaCommand
        public CommandResult InternalExecute(ICommandTarget target, params IAdaptable[] args) {
            _window = CommandTargetUtil.AsWindow(target);
            Debug.Assert(_window != null);

            if (_actions.Count == 0)
                return CommandResult.Ignored;
            else {
                //すぐに終わるものでない
                ProcessNextAction();
                return CommandResult.Succeeded;
            }
        }

        public bool CanExecute(ICommandTarget target) {
            return true;
        }
        public IAdaptable GetAdapter(Type adapter) {
            return TerminalSessionsPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion

        private void ProcessNextAction() {
            Action act = _actions.First.Value;
            _actions.RemoveFirst();
            ProcessAction(act);
        }
        private void ProcessAction(Action act) {
            _currentAction = act;

            //TODO ユーザによる操作のロック
            //コマンド実行開始
            _currentAction.TargetSession.Terminal.ShellCommandExecutor.StartCommandResultProcessor(this, _currentAction.CommandString, true);
        }

        #region ICommandResultProcessor
        public void StartCommand(AbstractTerminal terminal, string command_text, GLine prompt_line) {
        }

        public void EndCommand(List<GLine> command_result) {
            string[] stringarray_result = AsStringArrayResult(command_result);
            Debug.Assert(_window.AsForm().InvokeRequired);
            //この処理中に次のアクションがセットされることもある
            if (_currentAction.ReceiverThreadAction != null)
                _currentAction.ReceiverThreadAction(stringarray_result);
            else {
                _window.AsControl().Invoke(_currentAction.MainThreadAction, stringarray_result);
            }

            if (_actions.Count > 0)
                ProcessNextAction();
        }

        #endregion

        public static string[] AsStringArrayResult(List<GLine> command_result) {
            List<string> r = new List<string>();
            bool continuing = false;
            for (int i = 0; i < command_result.Count; i++) {
                string t = command_result[i].ToNormalString();
                if (continuing)
                    r[r.Count - 1] = r[r.Count - 1] + t;
                else
                    r.Add(t);

                continuing = command_result[i].EOLType == EOLType.Continue;
            }
            return r.ToArray();
        }
    }
}
