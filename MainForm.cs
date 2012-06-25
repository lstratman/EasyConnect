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
using System.Xml.Serialization;
using EasyConnect.Properties;
using EasyConnect.Protocols;
using EasyConnect.Protocols.Rdp;
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
        protected Dictionary<TitleBarTab, Bitmap> _previews = new Dictionary<TitleBarTab, Bitmap>();
        protected TitleBarTab _previousActiveTab = null;
        protected JumpListCustomCategory _recentCategory = new JumpListCustomCategory("Recent");

        protected Queue<HistoryWindow.HistoricalConnection> _recentConnections =
            new Queue<HistoryWindow.HistoricalConnection>();

        public MainForm(Guid[] openToBookmarks)
        {
            InitializeComponent();

            OpenToBookmarks = openToBookmarks;

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
                if (ConnectionFactory.EncryptionPassword == null)
                {
                    PasswordWindow passwordWindow = new PasswordWindow();
                    passwordWindow.ShowDialog();

                    ConnectionFactory.EncryptionPassword = passwordWindow.Password;
                    ConnectionFactory.EncryptionPassword.MakeReadOnly();
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

                    ConnectionFactory.EncryptionPassword = null;
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

                OpenBookmarkManager();

                //TODO: Conditional logic for OpenBookmarkManager || new TitleBarTab

                //Tabs.Add(new TitleBarTab(this)
                //             {
                //                 Content = new RdpWindow(_password)
                //             });
                //SelectedTabIndex = 0;

            }

        }

        public BookmarksWindow Bookmarks
        {
            get
            {
                if (_bookmarks == null && ConnectionFactory.EncryptionPassword != null)
                    _bookmarks = new BookmarksWindow(this);

                return _bookmarks;
            }
        }

        public HistoryWindow History
        {
            get
            {
                if (_history == null && ConnectionFactory.EncryptionPassword != null)
                    _history = new HistoryWindow(this);

                return _history;
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

        public void OpenOptions()
        {
            //TitleBarTab tab = Tabs.FirstOrDefault(t => t.Content is OptionsWindow);

            //if (tab != null)
            //{
            //    SelectedTab = tab;
            //    return;
            //}

            //TitleBarTab newTab = new TitleBarTab(this)
            //                         {
            //                             Content = new OptionsWindow(this)
            //                         };

            //Tabs.Add(newTab);
            //ResizeTabContents(newTab);
            //SelectedTabIndex = _tabs.Count - 1;
        }

        public void OpenHistory()
        {
            TitleBarTab tab = Tabs.FirstOrDefault(t => t.Content is HistoryWindow);

            if (tab != null)
            {
                SelectedTab = tab;
                return;
            }

            TitleBarTab newTab = new TitleBarTab(this)
                                     {
                                         Content = History
                                     };

            Tabs.Add(newTab);
            ResizeTabContents(newTab);

            SelectedTabIndex = _tabs.Count - 1;
            History.FormClosed += History_FormClosed;
        }

        private void History_FormClosed(object sender, FormClosedEventArgs e)
        {
            _history = null;
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
            if (_previousActiveTab == null)
                return;

            TabbedThumbnail preview = TaskbarManager.Instance.TabbedThumbnail.GetThumbnailPreview(_previousActiveTab.Content);

            if (preview == null)
                return;

            Bitmap bitmap = TabbedThumbnailScreenCapture.GrabWindowBitmap(_previousActiveTab.Content.Handle, _previousActiveTab.Content.Size);

            preview.SetImage(bitmap);

            if (_previews.ContainsKey(_previousActiveTab))
                _previews[_previousActiveTab].Dispose();

            _previews[_previousActiveTab] = bitmap;
        }

        protected void MainForm_TabSelected(object sender, TitleBarTabEventArgs e)
        {
            if (!_addingWindow && SelectedTabIndex != -1)
                TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(SelectedTab.Content);

            _previousActiveTab = SelectedTab;
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

        public TitleBarTab Connect(IConnection connection)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(connection);

            _addingWindow = true;
            TitleBarTab newTab = new TitleBarTab(this)
                                     {
                                         Content = connectionWindow
                                     };
            Tabs.Insert(SelectedTabIndex + 1, newTab);
            ResizeTabContents(newTab);
            _addingWindow = false;

            connectionWindow.FormClosing += sessionWindow_FormClosing;
            connectionWindow.Connected += sessionWindow_Connected;
            connectionWindow.Connect();

            TabbedThumbnail preview = new TabbedThumbnail(Handle, connectionWindow)
                                          {
                                              Title = connectionWindow.Text,
                                              Tooltip = connectionWindow.Text
                                          };

            preview.SetWindowIcon(connectionWindow.Icon);
            preview.TabbedThumbnailActivated += preview_TabbedThumbnailActivated;
            preview.TabbedThumbnailClosed += preview_TabbedThumbnailClosed;
            preview.TabbedThumbnailBitmapRequested += preview_TabbedThumbnailBitmapRequested;
            preview.PeekOffset = new Vector(connectionWindow.Location.X, connectionWindow.Location.Y);

            for (Control currentControl = connectionWindow.Parent;
                 currentControl.Parent != null;
                 currentControl = currentControl.Parent)
                preview.PeekOffset += new Vector(currentControl.Location.X, currentControl.Location.Y);

            TaskbarManager.Instance.TabbedThumbnail.AddThumbnailPreview(preview);
            TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(preview);

            if (
                _recentConnections.FirstOrDefault((HistoryWindow.HistoricalConnection c) => c.Guid == connection.Guid) ==
                null)
            {
                _recentCategory.AddJumpListItems(new JumpListLink(Application.ExecutablePath, connectionWindow.Text)
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

        private void preview_TabbedThumbnailBitmapRequested(object sender, TabbedThumbnailBitmapRequestedEventArgs e)
        {
            foreach (TitleBarTab rdcWindow in Tabs.Where(tab => tab.Content is IConnectionPanel).Where(rdcWindow => rdcWindow.Content.Handle == e.WindowHandle && _previews.ContainsKey(rdcWindow)))
            {
                e.SetImage(_previews[rdcWindow]);
                break;
            }
        }

        private void sessionWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            TitleBarTab tab = Tabs.FirstOrDefault(t => t.Content == sender);

            if (tab == null)
                return;

            if (_previews.ContainsKey(tab))
            {
                _previews[tab].Dispose();
                _previews.Remove(tab);
            }

            if (sender == _previousActiveTab)
                _previousActiveTab = null;

            if (!tab.Content.IsDisposed)
                TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(tab.Content);
        }

        private void preview_TabbedThumbnailClosed(object sender, TabbedThumbnailEventArgs e)
        {
            foreach (TitleBarTab tab in Tabs.Where(tab => tab.Content.Handle == e.WindowHandle))
            {
                tab.Content.Close();
                TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(e.TabbedThumbnail);

                break;
            }
        }

        private void preview_TabbedThumbnailActivated(object sender, TabbedThumbnailEventArgs e)
        {
            foreach (TitleBarTab tab in Tabs.Where(tab => tab.Content.Handle == e.WindowHandle))
            {
                SelectedTabIndex = Tabs.IndexOf(tab);

                TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(tab.Content);
                break;
            }

            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_bookmarks != null)
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
                           Content = new ConnectionWindow()
                       };
        }
    }
}