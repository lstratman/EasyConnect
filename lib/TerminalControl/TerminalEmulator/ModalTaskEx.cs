/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: ModalTaskEx.cs,v 1.2 2011/10/27 23:21:58 kzmi Exp $
 */
using System;
using System.Collections.Generic;
using System.Text;

using Poderosa.Forms;
using Poderosa.Protocols;

namespace Poderosa.Terminal {
    /// <summary>
    /// <ja>
    /// 送受信データをフックして加工する際に用いるインターフェイスです。
    /// </ja>
    /// <en>
    /// Interface used when hook is done and transmitting and receiving data is processed.
    /// </en>
    /// </summary>
    /// <remarks>
    /// <ja>
    /// このインターフェイスの解説は、まだありません。
    /// </ja>
    /// <en>
    /// It has not explained this interface yet. 
    /// </en>
    /// </remarks>
    public interface IModalTerminalTask : IAdaptable, IByteAsyncInputStream {
        void InitializeModelTerminalTask(IModalTerminalTaskSite site, IByteAsyncInputStream default_handler, ITerminalConnection connection);
        string Caption {
            get;
        }
        bool ShowInputInTerminal {
            get;
        }
        void NotifyEndOfPacket();
    }
    //マクロなど、charベースのやりとりをするもの
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IModalCharacterTask : IModalTerminalTask {
        void ProcessChar(char ch);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public interface IModalTerminalTaskSite : IAdaptable {
        void Complete();
        void Cancel(string msg);
        void Update();
        void SendEnter(); //XMODEM/ZMODEMでは受信コマンドがEnterキーを送信すると向こうの再送を待たなくてよくなる
        IPoderosaMainWindow MainWindow {
            get;
        }
    }

    internal class ModalTerminalTaskSite : IModalTerminalTaskSite {
        private AbstractTerminal _terminal;
        private IModalTerminalTask _task;

        public ModalTerminalTaskSite(AbstractTerminal terminal) {
            _terminal = terminal;
        }
        public void Start(IModalTerminalTask task) {
            _task = task;
            _task.InitializeModelTerminalTask(this, _terminal, _terminal.TerminalHost.TerminalConnection);
        }

        public void Complete() {
            _terminal.EndModalTerminalTask();
        }

        public void Cancel(string msg) {
            _terminal.EndModalTerminalTask();
            if (msg != null)
                this.MainWindow.Warning(msg);
        }

        public void SendEnter() {
            _terminal.TerminalHost.TerminalTransmission.SendLineBreak();
        }

        public void Update() {
        }

        public IPoderosaMainWindow MainWindow {
            get {
                return _terminal.TerminalHost.OwnerWindow;
            }
        }

        public IAdaptable GetAdapter(Type adapter) {
            return TerminalEmulatorPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
    }
}
