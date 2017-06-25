/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: CommandResultPopup.cs,v 1.3 2011/12/10 09:59:39 kzmi Exp $
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

namespace Poderosa.Terminal {

    /// <summary>
    /// <ja>
    /// シェルへコマンドを自動実行する起点です。
    /// </ja>
    /// <en>
    /// Starting point to the shell that executes the command automatically. 
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインタフェースの解説は、まだありません。
    /// </ja>
    /// <en>
    /// It has not explained this interface yet. 
    /// </en>
    /// </remarks>
    public interface IShellCommandExecutor : IAdaptable {
        void StartCommandResultProcessor(ICommandResultProcessor processor, bool start_with_linebreak);
        void StartCommandResultProcessor(ICommandResultProcessor processor, string command_body, bool start_with_linebreak);
        bool IsPromptReady {
            get;
        }
    }


    //コマンドのベース
    internal abstract class CommandResultProcessorBase : ICommandResultProcessor, ICommandResultProcessorMenuItem {
        protected string _textID;
        protected string _executingCommand;
        protected AbstractTerminal _terminal;

        public void StartCommand(AbstractTerminal terminal, string command_text, GLine prompt_line) {
            _executingCommand = command_text;
            _terminal = terminal;
        }
        public abstract void EndCommand(List<GLine> command_result);

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }

        public string Text {
            get {
                return GEnv.Strings.GetString(_textID);
            }
        }
        public bool IsEnabled(AbstractTerminal control_host) {
            return true;
        }
        public ICommandResultProcessor CommandBody {
            get {
                return this;
            }
        }

        protected static void AsyncResultQuickHack(TerminalControl tc, IAsyncResult ar) {
            if (ar.AsyncWaitHandle.WaitOne(100, false)) //IntelliSenseと同様の理由で時間制限つきのWait
                tc.EndInvoke(ar);
        }
    }

    //コマンド結果を新セッションとしてポップアップ
    internal class PopupCommandResult : CommandResultProcessorBase {
        public PopupCommandResult() {
            _textID = "Menu.PopupCommandResult";
        }
        public override void EndCommand(List<GLine> command_result) {
            CommandResultDocument doc = new CommandResultDocument(_executingCommand);
            foreach (GLine line in command_result)
                doc.AddLine(line.Clone());

            TerminalControl tc = _terminal.TerminalHost.TerminalControl;
            if (tc == null)
                return;

            Debug.Assert(tc.InvokeRequired);

            IAsyncResult ar = tc.BeginInvoke(CommandResultSession.Start, _terminal, doc);
            AsyncResultQuickHack(tc, ar);
        }
    }

    //コマンド結果をクリップボードに
    internal class CopyCommandResult : CommandResultProcessorBase {
        public CopyCommandResult() {
            _textID = "Menu.CopyCommandResult";
        }

        public override void EndCommand(List<GLine> command_result) {
            StringBuilder bld = new StringBuilder();
            foreach (GLine line in command_result) {
                line.WriteTo(
                    delegate(char[] buff, int len) {
                        bld.Append(buff, 0, len);
                    },
                    0);
                if (line.EOLType != EOLType.Continue)
                    bld.Append("\r\n");
            }

            if (bld.Length > 0) {
                //コピーはメインスレッドでやらんと
                TerminalControl tc = _terminal.TerminalHost.TerminalControl;
                if (tc == null)
                    return;

                Debug.Assert(tc.InvokeRequired);
                IAsyncResult ar = tc.BeginInvoke(new CopyToClipboardDelegate(CopyToClipboard), bld.ToString());
                AsyncResultQuickHack(tc, ar);
            }
        }
        private delegate void CopyToClipboardDelegate(string data);
        private void CopyToClipboard(string data) {
            try {
                Clipboard.SetDataObject(data, false);
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }
    }

    internal class CommandResultRecognizer : IPromptProcessor, IShellCommandExecutor {
        protected enum State {
            NotPrompt,
            Prompt, //プロンプト受付中
            Fetch   //コマンドの結果を受信中
        }
        protected AbstractTerminal _terminal;
        protected State _state;

        private GLine _lastPromptLine;
        protected string _lastCommand;

        private int _commandStartLineID;
        private ICommandResultProcessor _currentProcessor;

        public CommandResultRecognizer(AbstractTerminal terminal) {
            _terminal = terminal;
            _terminal.PromptRecognizer.AddListener(this);
            _state = State.NotPrompt;
        }

        public AbstractTerminal Terminal {
            get {
                return _terminal;
            }
        }

        #region IPromptRecognizer
        public void OnPromptLine(GLine line, string prompt, string command) {
            _lastPromptLine = line;
            _lastCommand = command;

            Debug.WriteLineIf(DebugOpt.CommandPopup, "OnPromptLine " + _state.ToString() + ";" + command);
            if (_currentProcessor != null && _state == State.Fetch && command.Trim().Length == 0) {
                ProcessCommandResult(line.ID - 1);
                _state = State.Prompt;
            }
            else if (_state != State.Fetch)
                _state = State.Prompt;
        }

        public void OnNotPromptLine() {
            if (_state != State.Fetch)
                _state = State.NotPrompt;
        }
        #endregion

        //ポップアップ対象の行を集めて構築。ここは受信スレッドでの実行であることに注意
        private void ProcessCommandResult(int end_line_id) {
            List<GLine> result = new List<GLine>();
            TerminalDocument doc = _terminal.GetDocument();
            GLine line = doc.FindLineOrNull(_commandStartLineID);
            while (line != null && line.ID <= end_line_id) {
                //Debug.WriteLine("P]"+new string(line.Text, 0, line.DisplayLength));
                result.Add(line);
                line = line.NextLine;
            }

            //何かとれていたら実行
            if (result.Count > 0)
                _currentProcessor.EndCommand(result);
            else
                Debug.WriteLineIf(DebugOpt.CommandPopup, String.Format("Ignored for 0-length, start={0} end={1}", _commandStartLineID, end_line_id));

            _currentProcessor = null;
        }

        #region IShellCommandExecutor
        //外部からも実行可能なコマンド処理ポイント
        public void StartCommandResultProcessor(ICommandResultProcessor processor, bool start_with_linebreak) {
            StartCommandResultProcessor(processor, null, start_with_linebreak); //改行のみでスタート
        }
        public void StartCommandResultProcessor(ICommandResultProcessor processor, string command_body, bool start_with_linebreak) {
            _currentProcessor = processor;
            _commandStartLineID = _lastPromptLine.ID + 1;
            string command = _lastCommand;
            if (!String.IsNullOrEmpty(command_body)) {
                command += command_body;
                _terminal.TerminalHost.TerminalTransmission.SendString(command.ToCharArray());
            }

            _state = State.Fetch; //コマンド本体送信した時点で取得を開始
            processor.StartCommand(_terminal, command, _lastPromptLine);
            //Enterを送信してコマンド実行開始
            if (start_with_linebreak)
                _terminal.TerminalHost.TerminalTransmission.SendLineBreak();
        }
        public bool IsPromptReady {
            get {
                return _state == State.Prompt;
            }
        }
        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion


    }

    //プロンプト認識して、ポップアップしたりコマンド実行したり
    internal class PopupStyleCommandResultRecognizer : CommandResultRecognizer {

        public const string POPUP_MENU_EXTENSION_POINT = "org.poderosa.terminalemulator.commandProcessorPopupMenu";

        //ExtP作成とデフォルトの登録
        public static void CreateExtensionPointAndDefaultCommands(IPluginManager pm) {
            IExtensionPoint pt = pm.CreateExtensionPoint(POPUP_MENU_EXTENSION_POINT, typeof(ICommandResultProcessorMenuItem), TerminalEmulatorPlugin.Instance);
            pt.RegisterExtension(new PopupCommandResult());
            pt.RegisterExtension(new CopyCommandResult());
        }


        public PopupStyleCommandResultRecognizer(AbstractTerminal terminal)
            : base(terminal) {
        }

        //TerminalControlから来るやつ
        public bool ProcessKey(Keys modifiers, Keys keybody) {
            if (_state == State.Prompt && _lastCommand.Length > 0 && TerminalEmulatorPlugin.Instance.TerminalEmulatorOptions.CommandPopupKey == (modifiers | keybody)) {
                ShowMenu();
                return true;
            }

            return false;
        }

        private void ShowMenu() {
            TerminalControl tc = _terminal.TerminalHost.TerminalControl;
            Debug.Assert(tc != null);
            TerminalDocument doc = _terminal.GetDocument();
            SizeF pitch = tc.GetRenderProfile().Pitch;
            Point popup = new Point((int)(doc.CaretColumn * pitch.Width), (int)((doc.CurrentLineNumber - doc.TopLineNumber + 1) * pitch.Height));

            IPoderosaForm f = tc.FindForm() as IPoderosaForm;
            Debug.Assert(f != null);
            //EXTPにしてもいいんだけど
            f.ShowContextMenu(new IPoderosaMenuGroup[] { new PoderosaMenuGroupImpl(CreatePopupMenuItems()) },
                (ICommandTarget)tc.GetAdapter(typeof(ICommandTarget)),
                tc.PointToScreen(popup),
                ContextMenuFlags.SelectFirstItem);
        }
        private IPoderosaMenu[] CreatePopupMenuItems() {
            List<IPoderosaMenu> result = new List<IPoderosaMenu>();
            foreach (ICommandResultProcessorMenuItem item in TerminalEmulatorPlugin.Instance.PoderosaWorld.PluginManager.FindExtensionPoint(POPUP_MENU_EXTENSION_POINT).GetExtensions()) {
                CommandAdapter ca = new CommandAdapter(this, item);
                result.Add(new PoderosaMenuItemImpl(
                    new PoderosaCommandImpl(new ExecuteDelegate(ca.Execute), new CanExecuteDelegate(ca.CanExecute)), item.Text));
            }
            return result.ToArray();
        }

        private class CommandAdapter {
            private PopupStyleCommandResultRecognizer _parent;
            private ICommandResultProcessorMenuItem _item;

            public CommandAdapter(PopupStyleCommandResultRecognizer parent, ICommandResultProcessorMenuItem proc) {
                _item = proc;
                _parent = parent;
            }
            public CommandResult Execute(ICommandTarget target) {
                _parent.StartCommandResultProcessor(_item.CommandBody, true);
                return CommandResult.Succeeded;
            }

            public bool CanExecute(ICommandTarget target) {
                return _item.IsEnabled(_parent.Terminal);
            }

        }


    }
}
