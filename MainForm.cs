using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Security;
using System.Windows;
using System.Linq;
using System.Security.Cryptography;
using EasyConnect.Properties;
using Stratman.Windows.Forms.TitleBarTabs;
using Microsoft.WindowsAPICodePack.Taskbar;
using Microsoft.WindowsAPICodePack.Shell;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace EasyConnect
{
    public partial class MainForm : TitleBarTabs
    {
        protected SecureString _password = null;
        protected RDCWindow _previousActiveDocument = null;
        protected HistoryWindow _history = null;
        protected BookmarksWindow _bookmarks = null;
        protected bool _addingWindow = false;
        protected Dictionary<RDCWindow, Bitmap> _previews = new Dictionary<RDCWindow, Bitmap>();
        protected JumpList _jumpList = null;
        protected JumpListCustomCategory _recentCategory = new JumpListCustomCategory("Recent");
        protected IpcServerChannel _ipcChannel = new IpcServerChannel("EasyConnect");
        protected Queue<HistoryWindow.HistoricalConnection> _recentConnections = new Queue<HistoryWindow.HistoricalConnection>();

        public static MainForm ActiveInstance = null;
        public static ConnectToHistoryDelegate ConnectToHistoryMethod = null;

        public delegate TitleBarTab ConnectionDelegate(RDCConnection connection);
        public delegate TitleBarTab ConnectToHistoryDelegate(Guid historyGuid);

        public BookmarksWindow Bookmarks
        {
            get
            {
                return _bookmarks;
            }
        }

        public void OpenBookmarkManager()
        {
            TitleBarTab newTab = new TitleBarTab(this)
                                     {
                                         Content = _bookmarks
                                     };

            Tabs.Add(newTab);
            ResizeTabContents(newTab);

            SelectedTabIndex = _tabs.Count - 1;
        }

        public MainForm()
        {
            InitializeComponent();

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect"))
            {
                MessageBox.Show(Resources.FirstRunPasswordText, Resources.FirstRunPasswordTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect");
            }

            while (_bookmarks == null || _history == null)
            {
                PasswordWindow passwordWindow = new PasswordWindow();
                passwordWindow.ShowDialog();

                _password = passwordWindow.Password;
                _password.MakeReadOnly();

                try
                {
                    _bookmarks = new BookmarksWindow(Connect, _password);
                    _history = new HistoryWindow(Connect, _bookmarks, _password);
                }

                catch (CryptographicException)
                {
                    DialogResult result = MessageBox.Show(Resources.IncorrectPasswordText, Resources.ErrorTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                    if (result != DialogResult.OK)
                    {
                        Closing = true;
                        return;
                    }
                }
            }

            _bookmarks.Password = _password;
            _history.Password = _password;

            ChannelServices.RegisterChannel(_ipcChannel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(HistoryMethods), "HistoryMethods", WellKnownObjectMode.SingleCall);
            
            TabSelected += MainForm_TabSelected;
            TabDeselecting += MainForm_TabDeselecting;

            ActiveInstance = this;
            ConnectToHistoryMethod = ConnectToHistory;

            TabRenderer = new ChromeTabRenderer(this);
            Tabs.Add(new TitleBarTab(this)
                         {
                             Content = new RDCWindow(_password)
                         });
            SelectedTabIndex = 0;
        }

        protected void MainForm_TabDeselecting(object sender, TitleBarTabCancelEventArgs e)
        {
            if (_previousActiveDocument == null)
                return;

            TabbedThumbnail preview = TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(_previousActiveDocument);

            if (preview == null)
                return;

            Bitmap bitmap = TabbedThumbnailScreenCapture.GrabWindowBitmap(_previousActiveDocument.Handle, _previousActiveDocument.Size);

            preview.SetImage(bitmap);

            if (_previews.ContainsKey(_previousActiveDocument))
                _previews[_previousActiveDocument].Dispose();

            _previews[_previousActiveDocument] = bitmap;
        }

        protected void MainForm_TabSelected(object sender, TitleBarTabEventArgs e)
        {
            if (!_addingWindow && SelectedTabIndex != -1)
                TaskbarManager.Instance.TabbedThumbnail.SetActiveTab((RDCWindow)SelectedTab.Content);

            _previousActiveDocument = (RDCWindow)SelectedTab.Content;
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

        public TitleBarTab ConnectToHistory(Guid historyGuid)
        {
            RDCConnection connection = _history.FindInHistory(historyGuid);

            if (connection != null)
                return Connect(connection);

            return null;
        }

        protected TitleBarTab Connect(RDCConnection connection)
        {
            _history.AddToHistory(connection);

            RDCWindow sessionWindow = new RDCWindow(_password);

            _addingWindow = true;
            TitleBarTab newTab = new TitleBarTab(this)
                                     {
                                         Content = sessionWindow
                                     };
            Tabs.Insert(SelectedTabIndex + 1, newTab);
            ResizeTabContents(newTab);
            _addingWindow = false;

            sessionWindow.FormClosing += sessionWindow_FormClosing;
            sessionWindow.Connected += sessionWindow_Connected;
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

            return newTab;
        }

        void sessionWindow_Connected(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        protected void GenerateWindowPreview(RDCWindow sessionWindow)
        {
            if (SelectedTab.Content != sessionWindow)
                return;

            Bitmap bitmap = TabbedThumbnailScreenCapture.GrabWindowBitmap(sessionWindow.Handle, sessionWindow.Size);
            TabbedThumbnail preview = TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(sessionWindow);

            _previews[sessionWindow] = bitmap;
            preview.SetImage(bitmap);
        }

        void preview_TabbedThumbnailBitmapRequested(object sender, TabbedThumbnailBitmapRequestedEventArgs e)
        {
            foreach (TitleBarTab tab in Tabs)
            {
                RDCWindow rdcWindow = (RDCWindow)tab.Content;

                if (rdcWindow.Handle == e.WindowHandle && _previews.ContainsKey(rdcWindow))
                {
                    e.SetImage(_previews[rdcWindow]);
                    break;
                }
            }
        }

        void sessionWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_previews.ContainsKey((RDCWindow)sender))
            {
                _previews[(RDCWindow)sender].Dispose();
                _previews.Remove((RDCWindow)sender);
            }

            if (sender == _previousActiveDocument)
                _previousActiveDocument = null;

            TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview((RDCWindow)sender);
        }

        void preview_TabbedThumbnailClosed(object sender, TabbedThumbnailEventArgs e)
        {
            foreach (TitleBarTab tab in Tabs)
            {
                RDCWindow rdcWindow = (RDCWindow)tab.Content;

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
            foreach (TitleBarTab tab in Tabs)
            {
                if (tab.Content.Handle == e.WindowHandle)
                {
                    SelectedTabIndex = Tabs.IndexOf(tab);

                    TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(tab.Content);
                    break;
                }
            }

            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _bookmarks.Save();
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

        public override TitleBarTab CreateTab()
        {
            return new TitleBarTab(this)
                       {
                           Content = new RDCWindow(_password)
                       };
        }
    }
}
