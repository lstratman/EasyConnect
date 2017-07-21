/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PoderosaForm.cs,v 1.3 2011/10/27 23:21:55 kzmi Exp $
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Poderosa.Commands;
using Poderosa.View;
using Poderosa.Util.Collections;

namespace Poderosa.Forms {
    //メインウィンドウとポップアップウィンドウの基底
#if UIDESIGN
    internal class PoderosaForm : Form, IPoderosaForm {
#else
    internal abstract class PoderosaForm : Form, IPoderosaForm {
#endif
        private System.ComponentModel.IContainer components = null;
        private Timer _contextMenuDisposeTimer;

        private List<ContextMenuStrip> _contextMenusToDispose;

        protected KeyboardHandlerManager _commandKeyHandler;

        private delegate DialogResult MessageBoxInternalDelegate(string msg, MessageBoxButtons buttons, MessageBoxIcon icon);
        private MessageBoxInternalDelegate _messageBoxInternalDelegate;

        public PoderosaForm() {
            _contextMenusToDispose = new List<ContextMenuStrip>();

            components = new System.ComponentModel.Container();
            _contextMenuDisposeTimer = new Timer(components);
            _contextMenuDisposeTimer.Tick += new EventHandler(ContextMenuDisposeTimerTick);

            _messageBoxInternalDelegate = new MessageBoxInternalDelegate(this.MessageBoxInternal);

            IPoderosaAboutBoxFactory aboutBoxFactory = AboutBoxUtil.GetCurrentAboutBoxFactory();
            if (aboutBoxFactory != null)
                this.Icon = aboutBoxFactory.ApplicationIcon;

            //ショートカットキーは共通
            _commandKeyHandler = new KeyboardHandlerManager();
            _commandKeyHandler.AddLastHandler(new CommandShortcutKeyHandler(this));
        }

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public Form AsForm() {
            return this;
        }

        public Control AsControl() {
            return this;
        }

        //コンテキストメニュー表示
        public void ShowContextMenu(IPoderosaMenuGroup[] menus, ICommandTarget target, Point point_screen, ContextMenuFlags flags) {
            //まずソート
            ICollection sorted = PositionDesignationSorter.SortItems(menus);
            ContextMenuStrip cm = new ContextMenuStrip();
            MenuUtil.BuildContextMenu(cm, new ConvertingEnumerable<IPoderosaMenuGroup>(sorted), target);
            if (cm.Items.Count == 0) {
                cm.Dispose();
                return;
            }

            //キーボード操作をトリガにメニューを出すときは、選択があったほうが何かと操作しやすい。
            if ((flags & ContextMenuFlags.SelectFirstItem) != ContextMenuFlags.None)
                cm.Items[0].Select();

            // ContextMenuStrip is not disposed automatically and
            // its instance sits in memory till application end.
            // To release a document object related with some menu items,
            // we need to dispose ContextMenuStrip explicitly soon after it disappeared.
            cm.VisibleChanged += new EventHandler(ContextMenuStripVisibleChanged);

            try {
                cm.Show(this, this.PointToClient(point_screen));
            }
            catch (Exception ex) {
                RuntimeUtil.ReportException(ex);
            }
        }

        private void ContextMenuStripVisibleChanged(object sender, EventArgs e) {
            ContextMenuStrip cm = sender as ContextMenuStrip;
            if (cm != null && !cm.Visible) {
                _contextMenusToDispose.Add(cm);
                if (!_contextMenuDisposeTimer.Enabled) {
                    _contextMenuDisposeTimer.Interval = 500;
                    _contextMenuDisposeTimer.Start();
                }
            }
        }

        void IPoderosaForm.Warning(string msg) {
            MessageBoxInternal(msg, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        void IPoderosaForm.Information(string msg) {
            MessageBoxInternal(msg, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        DialogResult IPoderosaForm.AskUserYesNo(string msg) {
            return MessageBoxInternal(msg, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
        private DialogResult MessageBoxInternal(string msg, MessageBoxButtons buttons, MessageBoxIcon icon) {
            if (this.InvokeRequired) {
                return (DialogResult)this.Invoke(_messageBoxInternalDelegate, new object[] { msg, buttons, icon });
            }
            else
                return MessageBox.Show(msg, "Poderosa", buttons, icon);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            //Debug.WriteLine("ProcessCmdKey " + keyData.ToString());
            if (base.ProcessCmdKey(ref msg, keyData))
                return true;
            else if (_commandKeyHandler.Process(keyData) == UIHandleResult.Stop)
                return true;
            else
                return false;
        }

        protected bool _closeCancelled;
        public CommandResult CancellableClose() {
            _closeCancelled = false;
            this.Close(); //キャンセルしたときはOnClosing内で上のフラグをセットする
            return _closeCancelled ? CommandResult.Cancelled : CommandResult.Succeeded;
        }

        private void ContextMenuDisposeTimerTick(object sender, EventArgs e) {
            _contextMenuDisposeTimer.Stop();
            foreach (ContextMenuStrip cm in _contextMenusToDispose) {
                cm.Dispose();
            }
            _contextMenusToDispose.Clear();
        }

        #region IAdaptable
        public IAdaptable GetAdapter(Type adapter) {
            return WindowManagerPlugin.Instance.PoderosaWorld.AdapterManager.GetAdapter(this, adapter);
        }
        #endregion
    }
}
