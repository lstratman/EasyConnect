using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Forms;
using EasyConnect.Properties;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;
using Stratman.Windows.Forms.TitleBarTabs;

namespace EasyConnect
{
    public partial class MainForm : TitleBarTabs
    {
        public delegate TitleBarTab ConnectToHistoryDelegate(Guid historyGuid);

        public delegate void ConnectToBookmarksDelegate(Guid[] bookmarkGuids);

        public delegate TitleBarTab ConnectionDelegate(RdpConnection connection);

        public static MainForm ActiveInstance = null;
        public static ConnectToHistoryDelegate ConnectToHistoryMethod = null;
        public static ConnectToBookmarksDelegate ConnectToBookmarksMethod = null;

        protected bool _addingWindow = false;
        protected BookmarksWindow _bookmarks = null;
        protected HistoryWindow _history = null;
        protected static IpcServerChannel _ipcChannel = null;
        protected JumpList _jumpList = null;
        protected SecureString _password = null;
        protected Dictionary<RdpWindow, Bitmap> _previews = new Dictionary<RdpWindow, Bitmap>();
        protected RdpWindow _previousActiveDocument = null;
        protected JumpListCustomCategory _recentCategory = new JumpListCustomCategory("Recent");

        protected Queue<HistoryWindow.HistoricalConnection> _recentConnections =
            new Queue<HistoryWindow.HistoricalConnection>();

        public MainForm() : this(null, null)
        {
        }

        public MainForm(Guid[] openToBookmarks, SecureString password)
        {
            InitializeComponent();

            OpenToBookmarks = openToBookmarks;
            _password = password;

            if (
                !Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                  "\\EasyConnect"))
            {
                MessageBox.Show(Resources.FirstRunPasswordText, Resources.FirstRunPasswordTitle, MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                          "\\EasyConnect");
            }

            while (Bookmarks == null || _history == null)
            {
                if (_password == null)
                {
                    PasswordWindow passwordWindow = new PasswordWindow();
                    passwordWindow.ShowDialog();

                    _password = passwordWindow.Password;
                    _password.MakeReadOnly();
                }

                try
                {
                    _bookmarks = new BookmarksWindow(this);
                    _history = new HistoryWindow(this);
                }

                catch (CryptographicException)
                {
                    DialogResult result = MessageBox.Show(Resources.IncorrectPasswordText, Resources.ErrorTitle,
                                                          MessageBoxButtons.OKCancel, MessageBoxIcon.Error);

                    if (result != DialogResult.OK)
                    {
                        Closing = true;
                        return;
                    }

                    _password = null;
                }
            }

            if (_ipcChannel == null)
            {
                _ipcChannel = new IpcServerChannel("EasyConnect");
                ChannelServices.RegisterChannel(_ipcChannel, false);
                RemotingConfiguration.RegisterWellKnownServiceType(typeof (HistoryMethods), "HistoryMethods",
                                                                   WellKnownObjectMode.SingleCall);
            }

            TabSelected += MainForm_TabSelected;
            TabDeselecting += MainForm_TabDeselecting;

            ActiveInstance = this;
            ConnectToHistoryMethod = ConnectToHistory;
            ConnectToBookmarksMethod = ConnectToBookmarks;

            TabRenderer = new ChromeTabRenderer(this);

            if (OpenToBookmarks == null && OpenToHistory == Guid.Empty)
            {
                Tabs.Add(new TitleBarTab(this)
                             {
                                 Content = new RdpWindow(_password)
                             });
                SelectedTabIndex = 0;
            }
        }

        public SecureString Password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;
            }
        }

        public BookmarksWindow Bookmarks
        {
            get
            {
                if (_bookmarks == null && _password != null)
                    _bookmarks = new BookmarksWindow(this);

                return _bookmarks;
            }
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

        public Guid[] OpenToBookmarks
        {
            get;
            set;
        }

        protected void Bookmarks_FormClosed(object sender, FormClosedEventArgs e)
        {
            _bookmarks = null;
        }

        public void OpenBookmarkManager()
        {
            TitleBarTab tab = Tabs.FirstOrDefault(t => t.Content is BookmarksWindow);

            if (tab != null)
            {
                SelectedTab = tab;
                return;
            }

            TitleBarTab newTab = new TitleBarTab(this)
                                     {
                                         Content = Bookmarks
                                     };

            Tabs.Add(newTab);
            ResizeTabContents(newTab);

            SelectedTabIndex = _tabs.Count - 1;
            Bookmarks.FormClosed += Bookmarks_FormClosed;
        }

        protected void MainForm_TabDeselecting(object sender, TitleBarTabCancelEventArgs e)
        {
            if (_previousActiveDocument == null)
                return;

            TabbedThumbnail preview =
                TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(_previousActiveDocument);

            if (preview == null)
                return;

            Bitmap bitmap = TabbedThumbnailScreenCapture.GrabWindowBitmap(_previousActiveDocument.Handle,
                                                                          _previousActiveDocument.Size);

            preview.SetImage(bitmap);

            if (_previews.ContainsKey(_previousActiveDocument))
                _previews[_previousActiveDocument].Dispose();

            _previews[_previousActiveDocument] = bitmap;
        }

        protected void MainForm_TabSelected(object sender, TitleBarTabEventArgs e)
        {
            if (!_addingWindow && SelectedTabIndex != -1)
                TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(SelectedTab.Content);

            _previousActiveDocument = (RdpWindow) SelectedTab.Content;
        }

        public TitleBarTab ConnectToHistory(Guid historyGuid)
        {
            RdpConnection connection = _history.FindInHistory(historyGuid);

            if (connection != null)
                return Connect(connection);

            return null;
        }

        public void ConnectToBookmarks(Guid[] bookmarkGuids)
        {
            foreach (Guid bookmarkGuid in bookmarkGuids)
                Connect(_bookmarks.FindBookmark(bookmarkGuid));

            SelectedTabIndex = Tabs.Count - 1;
        }

        public TitleBarTab Connect(RdpConnection connection)
        {
            _history.AddToHistory(connection);

            RdpWindow sessionWindow = new RdpWindow(_password);

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

            TabbedThumbnail preview = new TabbedThumbnail(Handle, sessionWindow)
                                          {
                                              Title = sessionWindow.Text,
                                              Tooltip = sessionWindow.Text
                                          };

            preview.SetWindowIcon(sessionWindow.Icon);
            preview.TabbedThumbnailActivated += preview_TabbedThumbnailActivated;
            preview.TabbedThumbnailClosed += preview_TabbedThumbnailClosed;
            preview.TabbedThumbnailBitmapRequested += preview_TabbedThumbnailBitmapRequested;
            preview.PeekOffset = new Vector(sessionWindow.Location.X, sessionWindow.Location.Y);

            for (Control currentControl = sessionWindow.Parent;
                 currentControl.Parent != null;
                 currentControl = currentControl.Parent)
                preview.PeekOffset += new Vector(currentControl.Location.X, currentControl.Location.Y);

            TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(preview);
            TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(preview);

            if (
                _recentConnections.FirstOrDefault((HistoryWindow.HistoricalConnection c) => c.Guid == connection.Guid) ==
                null)
            {
                _recentCategory.AddJumpListItems(new JumpListLink(Application.ExecutablePath, sessionWindow.Text)
                                                     {
                                                         Arguments = "/openHistory:" + connection.Guid.ToString(),
                                                         IconReference =
                                                             new IconReference(Application.ExecutablePath, 0)
                                                     });
                _jumpList.Refresh();

                _recentConnections.Enqueue(
                    _history.Connections.First((HistoryWindow.HistoricalConnection c) => c.Guid == connection.Guid));

                if (_recentConnections.Count > _jumpList.MaxSlotsInList)
                    _recentConnections.Dequeue();
            }

            return newTab;
        }

        private void sessionWindow_Connected(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        protected void GenerateWindowPreview(RdpWindow sessionWindow)
        {
            if (SelectedTab.Content != sessionWindow)
                return;

            Bitmap bitmap = TabbedThumbnailScreenCapture.GrabWindowBitmap(sessionWindow.Handle, sessionWindow.Size);
            TabbedThumbnail preview = TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(sessionWindow);

            _previews[sessionWindow] = bitmap;
            preview.SetImage(bitmap);
        }

        private void preview_TabbedThumbnailBitmapRequested(object sender, TabbedThumbnailBitmapRequestedEventArgs e)
        {
            foreach (TitleBarTab tab in Tabs)
            {
                RdpWindow rdcWindow = (RdpWindow) tab.Content;

                if (rdcWindow.Handle == e.WindowHandle && _previews.ContainsKey(rdcWindow))
                {
                    e.SetImage(_previews[rdcWindow]);
                    break;
                }
            }
        }

        private void sessionWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_previews.ContainsKey((RdpWindow) sender))
            {
                _previews[(RdpWindow) sender].Dispose();
                _previews.Remove((RdpWindow) sender);
            }

            if (sender == _previousActiveDocument)
                _previousActiveDocument = null;

            if (!((RdpWindow)sender).IsDisposed)
                TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview((RdpWindow) sender);
        }

        private void preview_TabbedThumbnailClosed(object sender, TabbedThumbnailEventArgs e)
        {
            foreach (TitleBarTab tab in Tabs)
            {
                RdpWindow rdcWindow = (RdpWindow) tab.Content;

                if (rdcWindow.Handle == e.WindowHandle)
                {
                    rdcWindow.Close();
                    TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(e.TabbedThumbnail);
                    break;
                }
            }
        }

        private void preview_TabbedThumbnailActivated(object sender, TabbedThumbnailEventArgs e)
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_jumpList == null)
            {
                _jumpList = JumpList.CreateJumpList();
                _jumpList.KnownCategoryToDisplay = JumpListKnownCategoryType.Neither;
                _jumpList.AddCustomCategories(_recentCategory);

                List<HistoryWindow.HistoricalConnection> historicalConnections =
                    _history.Connections.OrderBy((HistoryWindow.HistoricalConnection c) => c.LastConnection).ToList();
                historicalConnections = historicalConnections.GetRange(0,
                                                                       Math.Min(historicalConnections.Count,
                                                                                Convert.ToInt32(_jumpList.MaxSlotsInList)));

                foreach (HistoryWindow.HistoricalConnection historicalConnection in historicalConnections)
                {
                    _recentCategory.AddJumpListItems(new JumpListLink(Application.ExecutablePath,
                                                                      historicalConnection.DisplayName)
                                                         {
                                                             Arguments =
                                                                 "/openHistory:" + historicalConnection.Guid.ToString(),
                                                             IconReference =
                                                                 new IconReference(Application.ExecutablePath, 0)
                                                         });
                    _recentConnections.Enqueue(historicalConnection);
                }

                _jumpList.Refresh();

                if (OpenToHistory != Guid.Empty)
                    SelectedTab = Connect(_history.FindInHistory(OpenToHistory));
            }

            if (OpenToHistory == Guid.Empty && OpenToBookmarks != null)
                ConnectToBookmarks(OpenToBookmarks);
        }

        public override TitleBarTab CreateTab()
        {
            return new TitleBarTab(this)
                       {
                           Content = new RdpWindow(_password)
                       };
        }
    }
}