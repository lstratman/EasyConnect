// Copyright 2016 The Poderosa Project.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

using Poderosa.Forms;
using Poderosa.Protocols;
using Poderosa.Session;
using Poderosa.Terminal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Poderosa.Sessions {

    /// <summary>
    /// Open-Session dialog
    /// </summary>
    internal partial class OpenSessionDialog : Form, IInterruptableConnectorClient {

        private class LastStatus {
            public string SessionTypeName;
        }

        private static readonly StoragePerWindow<LastStatus> _lastStatusStorage = new StoragePerWindow<LastStatus>();

        private readonly IPoderosaMainWindow _mainWindow;
        private readonly LastStatus _lastStatus;
        private readonly List<IOpenSessionTabPage> _tabPages = new List<IOpenSessionTabPage>();
        private IInterruptable _interruptable;
        private ITerminalSettings _terminalSettings;
        private ITerminalConnection _terminalConnection;

        /// <summary>
        /// Default constructor for UI designer
        /// </summary>
        public OpenSessionDialog()
            : this(null) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mainWindow">main window</param>
        public OpenSessionDialog(IPoderosaMainWindow mainWindow) {
            _mainWindow = mainWindow;

            LastStatus lastStatus;
            if (_lastStatusStorage.Get(_mainWindow, out lastStatus)) {
                _lastStatus = lastStatus;
            }
            else {
                _lastStatus = new LastStatus();
            }

            SuspendLayout();
            InitializeComponent();
            Localize();
            SetupOpenSessionTabs();
            ResumeLayout();
        }

        public ITerminalSettings TerminalSettings {
            get {
                return _terminalSettings;
            }
        }

        public ITerminalConnection TerminalConnection {
            get {
                return _terminalConnection;
            }
        }

        private void Localize() {
            _loginButton.Text = TEnv.Strings.GetString("Common.OK");
            _cancelButton.Text = TEnv.Strings.GetString("Common.Cancel");
            this.Text = TEnv.Strings.GetString("Form.LoginDialog.Title");
        }

        private void SetupOpenSessionTabs() {
            // add to the private list
            _tabPages.Add(new OpenSessionTabPageSSH());
            _tabPages.Add(new OpenSessionTabPageTelnet());

            // add to the Tab control
            _sessionTypeTab.TabPages.Clear();
            foreach (var tabPage in _tabPages) {
                string tabTitle = tabPage.SessionTypeName;
                TabPage tabPageContainer = new TabPage(tabTitle);
                tabPageContainer.Controls.Add((Control)tabPage);
                _sessionTypeTab.TabPages.Add(tabPageContainer);
            }

            // initialize tab pages
            foreach (var tabPage in _tabPages) {
                tabPage.Initialize(_mainWindow);
            }

            // determine the initial tab
            int nextTabIndex = _tabPages.FindIndex(tabPage => {
                return tabPage.SessionTypeName == _lastStatus.SessionTypeName;
            });
            if (nextTabIndex < 0) {
                nextTabIndex = 0;
            }
            _sessionTypeTab.SelectedIndex = nextTabIndex;
        }

        private void UpdateFocus() {
            _sessionTypeTab.SelectedTab.Focus();
            if (!_tabPages[_sessionTypeTab.SelectedIndex].SetFocus()) {
                _loginButton.Focus();
            }
        }

        protected override bool ProcessDialogKey(Keys key) {
            if (_interruptable != null && (key == (Keys.Control | Keys.C) || key == Keys.Escape)) {
                Interrupt();
                return true;
            }

            return base.ProcessDialogKey(key);
        }

        private void ResumeFromOpeningSession() {
            _interruptable = null;
            _loginButton.Enabled = true;
            _cancelButton.Enabled = true;
            _sessionTypeTab.Enabled = true;
            this.Cursor = Cursors.Default;
            this.Text = TEnv.Strings.GetString("Form.LoginDialog.Text");
        }

        private void Interrupt() {
            if (_interruptable == null) {
                return;
            }
            _interruptable.Interrupt();
            ResumeFromOpeningSession();
        }

        #region IInterruptableConnectorClient

        public void SuccessfullyExit(ITerminalConnection result) {
            if (this.InvokeRequired) {
                this.Invoke((Action)(() => {
                    SuccessfullyExit(result);
                }));
                return;
            }

            _interruptable = null;  // allow dialog to close
            _terminalConnection = result;
            this.DialogResult = DialogResult.OK;
            this.Cursor = Cursors.Default;
            Close();
        }

        public void ConnectionFailed(string message) {
            if (this.InvokeRequired) {
                this.Invoke((Action)(() => {
                    ConnectionFailed(message);
                }));
                return;
            }

            ResumeFromOpeningSession();
            if (message != null) {
                GUtil.Warning(this, message, TEnv.Strings.GetString("Caption.LoginDialog.ConnectionError"));
            }
        }

        #endregion

        private void _loginButton_Click(object sender, EventArgs e) {
            int tabPageIndex = _sessionTypeTab.SelectedIndex;
            var tabPage = _tabPages[tabPageIndex];

            _loginButton.Enabled = false;
            _cancelButton.Enabled = false;
            _sessionTypeTab.Enabled = false;

            this.Cursor = Cursors.WaitCursor;

            ITerminalSettings terminalSettings;
            IInterruptable interruptable;
            bool started = tabPage.OpenSession(this, out terminalSettings, out interruptable);

            if (!started) {
                _loginButton.Enabled = true;
                _cancelButton.Enabled = true;
                _sessionTypeTab.Enabled = true;
                return;
            }

            this.Text = TEnv.Strings.GetString("Caption.LoginDialog.Connecting");
            _terminalSettings = terminalSettings;
            _interruptable = interruptable;
            _cancelButton.Enabled = true;
        }

        private void _cancelButton_Click(object sender, EventArgs e) {
            if (_interruptable != null) {
                Interrupt();
                return;
            }

            _terminalSettings = null;
            _terminalConnection = null;
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void OpenSessionDialog_Load(object sender, EventArgs e) {
            Size extend = new Size(0, 0);
            for (int i = 0; i < _tabPages.Count; ++i) {
                Size containerSize = _sessionTypeTab.TabPages[i].ClientSize;
                Size pageSize = ((Control)_tabPages[i]).Size;
                Size diff = pageSize - containerSize;
                extend.Width = Math.Max(diff.Width, extend.Width);
                extend.Height = Math.Max(diff.Height, extend.Height);
            }
            this.ClientSize += extend;
            this.Location -= new Size(extend.Width / 2, extend.Height / 2);

            foreach (var tabPage in _tabPages) {
                ((Control)tabPage).Dock = DockStyle.Fill;
            }
        }

        private void OpenSessionDialog_FormClosing(object sender, FormClosingEventArgs e) {
            if (_interruptable != null) {
                e.Cancel = true;
                return;
            }

            _lastStatus.SessionTypeName = _tabPages[_sessionTypeTab.SelectedIndex].SessionTypeName;
            _lastStatusStorage.Put(_mainWindow, _lastStatus);
        }

        private void OpenSessionDialog_Shown(object sender, EventArgs e) {
            UpdateFocus();
        }

        private void _sessionTypeTab_SelectedIndexChanged(object sender, EventArgs e) {
            UpdateFocus();
        }
    }
}
