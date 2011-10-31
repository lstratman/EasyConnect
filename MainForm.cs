using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Security;
using System.Windows;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using WeifenLuo.WinFormsUI.Docking;
using Microsoft.WindowsAPICodePack.Taskbar;
using Microsoft.WindowsAPICodePack.Shell;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace EasyConnect
{
    public partial class MainForm : Form
    {
        protected FavoritesWindow _favorites = null;
        protected HistoryWindow _history = null;
        protected SecureString _password = null;
        protected RDCWindow _previousActiveDocument = null;
        protected bool _addingWindow = false;
        protected Dictionary<RDCWindow, Bitmap> _previews = new Dictionary<RDCWindow, Bitmap>();
        protected JumpList _jumpList = null;
        protected JumpListCustomCategory _recentCategory = new JumpListCustomCategory("Recent");
        protected IpcServerChannel _ipcChannel = new IpcServerChannel("EasyConnect");
        protected Queue<HistoryWindow.HistoricalConnection> _recentConnections = new Queue<HistoryWindow.HistoricalConnection>();

        public static MainForm ActiveInstance = null;
        public static ConnectToHistoryDelegate ConnectToHistoryMethod = null;

        public delegate void ConnectionDelegate(RDCConnection connection);
        public delegate void ConnectToHistoryDelegate(Guid historyGuid);

        public MainForm()
        {
            InitializeComponent();

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect"))
            {
                MessageBox.Show("Since this is the first time that you have run this application,\nyou will be asked to enter an access password.  This password will\nbe used to protect any passwords that you associate with your\nconnections.", "Create password", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect");
            }

            while (_favorites == null || _history == null)
            {
                PasswordWindow passwordWindow = new PasswordWindow();
                passwordWindow.ShowDialog();

                _password = passwordWindow.Password;
                _password.MakeReadOnly();

                try
                {
                    _favorites = new FavoritesWindow(new ConnectionDelegate(Connect), _password);
                    _history = new HistoryWindow(new ConnectionDelegate(Connect), _favorites, _password);
                }

                catch (CryptographicException e)
                {
                    DialogResult result = MessageBox.Show("Password is incorrect.", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                    if (result != DialogResult.OK)
                    {
                        Closing = true;
                        return;
                    }
                }
            }

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Windows.xml"))
                dockPanel.LoadFromXml(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Windows.xml", new DeserializeDockContent(GetContentFromPersistString));

            else
            {
                _favorites.Show(dockPanel, DockState.DockLeftAutoHide);
                _history.Show(dockPanel, DockState.DockLeftAutoHide);
            }

            _favorites.Password = _password;
            _history.Password = _password;

            dockPanel.ActiveAutoHideContent = null;

            ChannelServices.RegisterChannel(_ipcChannel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(HistoryMethods), "HistoryMethods", WellKnownObjectMode.SingleCall);

            ActiveInstance = this;
            ConnectToHistoryMethod = new ConnectToHistoryDelegate(ConnectToHistory);
        }

        protected IDockContent GetContentFromPersistString(string persistString)
        {
            if (persistString == typeof(HistoryWindow).ToString())
                return _history;

            else if (persistString == typeof(FavoritesWindow).ToString())
                return _favorites;

            else
                return null;
        }

        public bool Closing
        {
            get;
            set;
        }

        public Guid OpenToHistory
        {
            get;
            set;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            dockPanel.Width = ClientSize.Width;
            dockPanel.Height = ClientSize.Height - dockPanel.Location.Y;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            Connect(new RDCConnection(_password) { Host = hostTextBox.Text });
        }

        public void ConnectToHistory(Guid historyGuid)
        {
            RDCConnection connection = _history.FindInHistory(historyGuid);

            if (connection != null)
                Connect(connection);
        }

        protected void Connect(RDCConnection connection)
        {
            _history.AddToHistory(connection);
            dockPanel.ActiveAutoHideContent = null;

            RDCWindow sessionWindow = new RDCWindow();

            _addingWindow = true;
            sessionWindow.Show(dockPanel, DockState.Document);
            _addingWindow = false;

            sessionWindow.FormClosed += new FormClosedEventHandler(sessionWindow_FormClosed);
            sessionWindow.Connected += new EventHandler(sessionWindow_Connected);
            sessionWindow.Connect(connection);

            TabbedThumbnail preview = new TabbedThumbnail(Handle, sessionWindow);
            
            preview.Title = sessionWindow.Text;
            preview.Tooltip = sessionWindow.Text;
            preview.SetWindowIcon(sessionWindow.Icon);
            preview.TabbedThumbnailActivated += new EventHandler<TabbedThumbnailEventArgs>(preview_TabbedThumbnailActivated);
            preview.TabbedThumbnailClosed += new EventHandler<TabbedThumbnailEventArgs>(preview_TabbedThumbnailClosed);
            preview.TabbedThumbnailBitmapRequested += new EventHandler<TabbedThumbnailBitmapRequestedEventArgs>(preview_TabbedThumbnailBitmapRequested);
            preview.PeekOffset = new Vector(sessionWindow.Location.X, sessionWindow.Location.Y);

            for (Control currentControl = sessionWindow.Parent; currentControl.Parent != null; currentControl = currentControl.Parent)
                preview.PeekOffset += new Vector(currentControl.Location.X, currentControl.Location.Y);

            TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(preview);
            TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(preview);

            if (_recentConnections.FirstOrDefault((HistoryWindow.HistoricalConnection c) => c.Guid == connection.Guid) == null)
            {
                _recentCategory.AddJumpListItems(new JumpListLink(Application.ExecutablePath, sessionWindow.Text) { Arguments = "/openHistory:" + connection.Guid.ToString(), IconReference = new IconReference(Application.ExecutablePath, 0) });
                _jumpList.Refresh();

                _recentConnections.Enqueue(_history.Connections.First((HistoryWindow.HistoricalConnection c) => c.Guid == connection.Guid));

                if (_recentConnections.Count > _jumpList.MaxSlotsInList)
                    _recentConnections.Dequeue();
            }
        }

        void sessionWindow_Connected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected void GenerateWindowPreview(RDCWindow sessionWindow)
        {
            if (dockPanel.ActiveDocument != sessionWindow)
                return;

            Bitmap bitmap = TabbedThumbnailScreenCapture.GrabWindowBitmap(sessionWindow.Handle, sessionWindow.Size);
            TabbedThumbnail preview = TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(sessionWindow);

            _previews[sessionWindow] = bitmap;
            preview.SetImage(bitmap);
        }

        void preview_TabbedThumbnailBitmapRequested(object sender, TabbedThumbnailBitmapRequestedEventArgs e)
        {
            foreach (IDockContent document in dockPanel.Documents)
            {
                RDCWindow rdcWindow = (RDCWindow)document;

                if (rdcWindow.Handle == e.WindowHandle && _previews.ContainsKey(rdcWindow))
                {
                    e.SetImage(_previews[rdcWindow]);
                    break;
                }
            }
        }

        void sessionWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_previews.ContainsKey((RDCWindow)sender))
            {
                _previews[(RDCWindow)sender].Dispose();
                _previews.Remove((RDCWindow)sender);
            }

            if (((RDCWindow)sender) == _previousActiveDocument)
                _previousActiveDocument = null;

            TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview((RDCWindow)sender);
        }

        void preview_TabbedThumbnailClosed(object sender, TabbedThumbnailEventArgs e)
        {
            foreach (IDockContent document in dockPanel.Documents)
            {
                RDCWindow rdcWindow = (RDCWindow)document;

                if (rdcWindow.Handle == e.WindowHandle)
                {
                    rdcWindow.Close();
                    TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(e.TabbedThumbnail);
                    break;
                }
            }
        }

        void preview_TabbedThumbnailActivated(object sender, TabbedThumbnailEventArgs e)
        {
            foreach (IDockContent document in dockPanel.Documents)
            {
                RDCWindow rdcWindow = (RDCWindow)document;

                if (rdcWindow.Handle == e.WindowHandle)
                {
                    rdcWindow.Activate();
                    TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(rdcWindow);
                    break;
                }
            }

            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
        }

        private void newConnectionButton_Click(object sender, EventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(_favorites, new ConnectionDelegate(Connect), _password);
            connectionWindow.ShowDialog(this);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _favorites.Save();
            dockPanel.SaveAsXml(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Windows.xml");
        }

        private void hostTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                connectButton_Click(null, null);
        }

        private void dockPanel_ActiveDocumentChanging(object sender, EventArgs e)
        {
            if (_previousActiveDocument != null)
            {
                TabbedThumbnail preview = TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(_previousActiveDocument);

                if (preview != null)
                {
                    Bitmap bitmap = TabbedThumbnailScreenCapture.GrabWindowBitmap(_previousActiveDocument.Handle, _previousActiveDocument.Size);
                    
                    preview.SetImage(bitmap);

                    if (_previews.ContainsKey(_previousActiveDocument))
                        _previews[_previousActiveDocument].Dispose();

                    _previews[_previousActiveDocument] = bitmap;
                }
            }
        }

        private void dockPanel_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (!_addingWindow && dockPanel.ActiveDocument != null)
                TaskbarManager.Instance.TabbedThumbnail.SetActiveTab((RDCWindow)dockPanel.ActiveDocument);

            _previousActiveDocument = (RDCWindow)dockPanel.ActiveDocument;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _jumpList = JumpList.CreateJumpList();
            _jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Neither;
            _jumpList.AddCustomCategories(_recentCategory);

            List<HistoryWindow.HistoricalConnection> historicalConnections = _history.Connections.OrderBy((HistoryWindow.HistoricalConnection c) => c.LastConnection).ToList();
            historicalConnections = historicalConnections.GetRange(0, Math.Min(historicalConnections.Count, Convert.ToInt32(_jumpList.MaxSlotsInList)));

            foreach (HistoryWindow.HistoricalConnection historicalConnection in historicalConnections)
            {
                _recentCategory.AddJumpListItems(new JumpListLink(Application.ExecutablePath, (!String.IsNullOrEmpty(historicalConnection.Name) ? historicalConnection.Name : historicalConnection.Host)) { Arguments = "/openHistory:" + historicalConnection.Guid.ToString(), IconReference = new IconReference(Application.ExecutablePath, 0) });
                _recentConnections.Enqueue(historicalConnection);
            }

            _jumpList.Refresh();

            if (OpenToHistory != Guid.Empty)
                Connect(_history.FindInHistory(OpenToHistory));
        }
    }
}
