using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using EasyConnect.Properties;
using EasyConnect.Protocols;
using EasyConnect.Protocols.Rdp;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;
using Stratman.Windows.Forms.TitleBarTabs;
using wyDay.Controls;

namespace EasyConnect
{
    public partial class MainForm : TitleBarTabs
    {
        public delegate TitleBarTab ConnectToHistoryDelegate(Guid historyGuid);

        public delegate void ConnectToBookmarksDelegate(Guid[] bookmarkGuids);

        public delegate TitleBarTab ConnectionDelegate(IConnection connection);

        public static MainForm ActiveInstance = null;
        public static ConnectToHistoryDelegate ConnectToHistoryMethod = null;
        public static ConnectToBookmarksDelegate ConnectToBookmarksMethod = null;

        protected bool _addingWindow = false;
        protected BookmarksWindow _bookmarks = null;
        protected HistoryWindow _history = null;
        protected static IpcServerChannel _ipcChannel = null;
        protected JumpList _jumpList = null;
        protected Dictionary<Form, Bitmap> _previews = new Dictionary<Form, Bitmap>();
        protected TitleBarTab _previousActiveTab = null;
        protected JumpListCustomCategory _recentCategory = new JumpListCustomCategory("Recent");
        protected AutomaticUpdater _automaticUpdater;
        protected Options _options;

        protected Queue<HistoryWindow.HistoricalConnection> _recentConnections =
            new Queue<HistoryWindow.HistoricalConnection>();

        protected bool ctrlDown = false;

        protected bool shiftDown = false;

        /// <summary>
        /// Pointer to the low-level mouse hook callback (<see cref="KeyboardHookCallback"/>).
        /// </summary>
        protected IntPtr _hookId;

        /// <summary>
        /// Delegate of <see cref="KeyboardHookCallback"/>; declared as a member variable to keep it from being garbage collected.
        /// </summary>
        protected HOOKPROC _hookproc = null;

        private bool _updateAvailable;

        public AutomaticUpdater AutomaticUpdater
        {
            get
            {
                return _automaticUpdater;
            }
        }

        public Options Options
        {
            get
            {
                if (_options == null)
                    _options = Options.Load();

                return _options;
            }
        }

        protected IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                Key key = KeyInterop.KeyFromVirtualKey(Marshal.ReadInt32(lParam));

                switch (wParam.ToInt32())
                {
                    case Win32Messages.WM_KEYDOWN:
                    case Win32Messages.WM_SYSKEYDOWN:
                        switch (key)
                        {
                            case Key.RightCtrl:
                            case Key.LeftCtrl:
                                ctrlDown = true;
                                break;

                            case Key.RightShift:
                            case Key.LeftShift:
                                shiftDown = true;
                                break;

                            case Key.T:
                                if (ctrlDown)
                                    AddNewTab();

                                break;

                            case Key.Tab:
                                if (Tabs.Count > 1)
                                {
                                    if (ctrlDown && shiftDown)
                                    {
                                        if (SelectedTabIndex == 0)
                                            SelectedTabIndex = Tabs.Count - 1;

                                        else
                                            SelectedTabIndex--;
                                    }

                                    else if (ctrlDown)
                                    {
                                        if (SelectedTabIndex == Tabs.Count - 1)
                                            SelectedTabIndex = 0;

                                        else
                                            SelectedTabIndex++;
                                    }
                                }

                                break;
                        }

                        break;

                    case Win32Messages.WM_KEYUP:
                    case Win32Messages.WM_SYSKEYUP:
                        switch (key)
                        {
                            case Key.RightCtrl:
                            case Key.LeftCtrl:
                                ctrlDown = false;
                                break;

                            case Key.RightShift:
                            case Key.LeftShift:
                                shiftDown = false;
                                break;
                        }

                        break;
                }
            }

            return Win32Interop.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public MainForm(Guid[] openToBookmarks)
        {
            InitializeComponent();

            _automaticUpdater = new AutomaticUpdater();

            (_automaticUpdater as ISupportInitialize).BeginInit();
            _automaticUpdater.ContainerForm = this;
            _automaticUpdater.Name = "_automaticUpdater";
            _automaticUpdater.TabIndex = 0;
            _automaticUpdater.wyUpdateCommandline = null;
            _automaticUpdater.Visible = false;
            _automaticUpdater.KeepHidden = true;
            _automaticUpdater.GUID = "752f8ae7-47f3-4299-adcc-8be32d63ec7a";
            _automaticUpdater.DaysBetweenChecks = 2;
            _automaticUpdater.UpdateType = UpdateType.Automatic;
            _automaticUpdater.ReadyToBeInstalled += _automaticUpdater_ReadyToBeInstalled;
            _automaticUpdater.UpToDate += _automaticUpdater_UpToDate;
            _automaticUpdater.CheckingFailed += _automaticUpdater_CheckingFailed;
            (_automaticUpdater as ISupportInitialize).EndInit();

            Controls.Add(_automaticUpdater);

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
            TabClicked += MainForm_TabClicked;

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

            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookproc = KeyboardHookCallback;
                _hookId = Win32Interop.SetWindowsHookEx(Win32Messages.WH_KEYBOARD_LL, _hookproc, Win32Interop.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        void MainForm_TabClicked(object sender, TitleBarTabEventArgs e)
        {
            if (e.Tab.Content is ConnectionWindow && e.Tab == SelectedTab)
                (e.Tab.Content as ConnectionWindow).ShowToolbar();
        }

        void _automaticUpdater_CheckingFailed(object sender, FailArgs e)
        {
            Debug.WriteLine("fucked up");
        }

        void _automaticUpdater_UpToDate(object sender, SuccessArgs e)
        {
            UpdateAvailable = false;
        }

        public bool UpdateAvailable
        {
            get
            {
                return _updateAvailable;
            }

            set
            {
                _updateAvailable = value;

                foreach (TitleBarTab tab in Tabs)
                {
                    if (tab.Content is ConnectionWindow)
                        (tab.Content as ConnectionWindow).SetUpdateAvailableState(_updateAvailable);
                }
            }
        }

        public void InstallUpdate()
        {
            if (UpdateAvailable)
                _automaticUpdater.InstallNow();
        }

        void _automaticUpdater_ReadyToBeInstalled(object sender, EventArgs e)
        {
            UpdateAvailable = true;
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
            TitleBarTab tab = Tabs.FirstOrDefault(t => t.Content is OptionsWindow);

            if (tab != null)
            {
                SelectedTab = tab;
                return;
            }

            OptionsWindow optionsWindow = new OptionsWindow(this);
            optionsWindow.OptionsForms.Add(new GlobalOptionsWindow());

            foreach (IProtocol protocol in ConnectionFactory.GetProtocols())
            {
                Form optionsForm = protocol.GetOptionsFormInDefaultsMode();

                optionsForm.Closed +=
                    (sender, args) => ConnectionFactory.SetDefaults(((IOptionsForm) optionsForm).Connection);
                optionsWindow.OptionsForms.Add(optionsForm);
            }

            ShowInEmptyTab(optionsWindow);
        }

        protected void ShowInEmptyTab(Form form)
        {
            if (SelectedTab != null && SelectedTab.Content is ConnectionWindow && !(SelectedTab.Content as ConnectionWindow).IsConnected)
            {
                Form oldWindow = SelectedTab.Content;

                SelectedTab.Content = form;
                ResizeTabContents();

                oldWindow.Close();
            }

            else
            {
                TitleBarTab newTab = new TitleBarTab(this)
                {
                    Content = form
                };

                Tabs.Add(newTab);
                ResizeTabContents(newTab);
                SelectedTabIndex = _tabs.Count - 1;
            }

            form.Show();

            if (_overlay != null)
                _overlay.Render(true);
        }

        public void OpenHistory()
        {
            TitleBarTab tab = Tabs.FirstOrDefault(t => t.Content is HistoryWindow);

            if (tab != null)
            {
                SelectedTab = tab;
                return;
            }

            History.FormClosed += History_FormClosed;
            ShowInEmptyTab(History);
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

            Bookmarks.FormClosed += Bookmarks_FormClosed;
            ShowInEmptyTab(Bookmarks);
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

            if (_previews.ContainsKey(_previousActiveTab.Content))
                _previews[_previousActiveTab.Content].Dispose();

            _previews[_previousActiveTab.Content] = bitmap;
        }

        protected void MainForm_TabSelected(object sender, TitleBarTabEventArgs e)
        {
            if (!_addingWindow && SelectedTabIndex != -1 && _previews.ContainsKey(SelectedTab.Content))
                TaskbarManager.Instance.TabbedThumbnail.SetActiveTab(SelectedTab.Content);

            _previousActiveTab = SelectedTab;
        }

        public TitleBarTab ConnectToHistory(Guid historyGuid)
        {
            IConnection connection = _history.FindInHistory(historyGuid);

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
            return Connect(connection, false);
        }

        public TitleBarTab Connect(IConnection connection, bool focusNewTab)
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

            if (focusNewTab)
                SelectedTab = newTab;

            connectionWindow.Connect();

            return newTab;
        }

        public void RegisterConnection(ConnectionWindow connectionWindow, IConnection connection)
        {
            _history.AddToHistory(connection);

            if (!_previews.ContainsKey(connectionWindow))
            {
                connectionWindow.FormClosing += sessionWindow_FormClosing;

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
            }

            if (_recentConnections.FirstOrDefault((HistoryWindow.HistoricalConnection c) => c.Connection.Guid == connection.Guid) == null)
            {
                _recentCategory.AddJumpListItems(
                    new JumpListLink(Application.ExecutablePath, connectionWindow.Text)
                        {
                            Arguments = "/openHistory:" + connection.Guid.ToString(),
                            IconReference =
                                new IconReference(Application.ExecutablePath, 0)
                        });
                _jumpList.Refresh();

                _recentConnections.Enqueue(
                    _history.Connections.First((HistoryWindow.HistoricalConnection c) => c.Connection.Guid == connection.Guid));

                if (_recentConnections.Count > _jumpList.MaxSlotsInList)
                    _recentConnections.Dequeue();
            }
        }

        private void preview_TabbedThumbnailBitmapRequested(object sender, TabbedThumbnailBitmapRequestedEventArgs e)
        {
            foreach (TitleBarTab rdcWindow in Tabs.Where(tab => tab.Content is IConnectionForm).Where(rdcWindow => rdcWindow.Content.Handle == e.WindowHandle && _previews.ContainsKey(rdcWindow.Content)))
            {
                e.SetImage(_previews[rdcWindow.Content]);
                break;
            }
        }

        private void sessionWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form window = (Form) sender;

            if (_previews.ContainsKey(window))
            {
                _previews[window].Dispose();
                _previews.Remove(window);
            }

            if (_previousActiveTab != null && window == _previousActiveTab.Content)
                _previousActiveTab = null;

            if (!window.IsDisposed)
                TaskbarManager.Instance.TabbedThumbnail.RemoveThumbnailPreview(window);
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
                                                                      historicalConnection.Connection.DisplayName)
                                                         {
                                                             Arguments =
                                                                 "/openHistory:" + historicalConnection.Connection.Guid.ToString(),
                                                             IconReference = new IconReference(Application.ExecutablePath, 0)
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

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x0021:
                    if (SelectedTab != null && SelectedTab.Content is ConnectionWindow)
                    {
                        if ((SelectedTab.Content as ConnectionWindow).IsCursorOverContent)
                            (SelectedTab.Content as ConnectionWindow).FocusContent();
                    }

                    base.WndProc(ref m);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        public bool CheckForUpdate()
        {
            return _automaticUpdater.ForceCheckForUpdate(true);
        }
    }
}