using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EasyConnect.Properties;
using EasyConnect.Protocols;
using System.Timers;
using Timer = System.Timers.Timer;

namespace EasyConnect
{
    public partial class ConnectionWindow : Form
    {
        protected List<IConnection> _autoCompleteEntries = new List<IConnection>();
        protected bool _connectClipboard = true;
        protected IConnection _connection = null;
        protected BaseConnectionForm _connectionForm = null;
        protected Timer _animationTimer = null;
        protected int _animationTicks = 0;
        protected bool _toolbarShown = true;
        protected bool _sizeSet = false;
        protected bool _suppressOmniBar = false;
        protected int _omniBarFocusIndex = -1;
        protected Regex _backgroundColorRegex = new Regex("background-color: #([A-F0-9]{6});");
        protected Regex _fontColorRegex = new Regex("; color: #([A-F0-9]{6});");

        protected Dictionary<ToolStripMenuItem, IConnection> _menuItemConnections =
            new Dictionary<ToolStripMenuItem, IConnection>();

        private bool? _autoHideToolbar = null;
        private List<IConnection> _validAutoCompleteEntries;

        public ConnectionWindow()
        {
            InitializeComponent();

            for (int i = 0; i < 6; i++)
            {
                HtmlPanel autoCompletePanel = new HtmlPanel
                    {
                        AutoScroll = false,
                        Width = _omniBarPanel.Width,
                        Height = 30,
                        Left = 0
                    };

                autoCompletePanel.Top = i * autoCompletePanel.Height;
                autoCompletePanel.Font = urlTextBox.Font;
                autoCompletePanel.MouseEnter += autoCompletePanel_MouseEnter;
                autoCompletePanel.MouseLeave += autoCompletePanel_MouseLeave;
                autoCompletePanel.Click += autoCompletePanel_Click;
                autoCompletePanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

                _omniBarPanel.Controls.Add(autoCompletePanel);
            }
        }

        void autoCompletePanel_Click(object sender, EventArgs e)
        {
            _omniBarPanel.Visible = false;
            _omniBarBorder.Visible = false;

            _connection = _validAutoCompleteEntries[_omniBarPanel.Controls.IndexOf((HtmlPanel)sender)];
            Connect();
        }

        public ConnectionWindow(IConnection connection)
            : this()
        {
            _connection = connection;

            Icon = ConnectionFactory.GetProtocol(connection).ProtocolIcon;
            Text = connection.DisplayName;
            _suppressOmniBar = true;
            urlTextBox.Text = connection.Host;
            _suppressOmniBar = false;
        }

        public bool IsCursorOverContent
        {
            get
            {
                Point relativePosition = _connectionContainerPanel.PointToClient(MousePosition);
                return (relativePosition.X >= 0 && relativePosition.Y >= 0 && relativePosition.X <= _connectionContainerPanel.Width &&
                        relativePosition.Y <= _connectionContainerPanel.Height);
            }
        }

        public void SetUpdateAvailableState(bool updateAvailable)
        {
            _updatesMenuItem.Text = updateAvailable
                                        ? "Install update"
                                        : "Check for update";
            _toolsButton.Image = updateAvailable
                                     ? Resources.ToolsActiveUpdate
                                     : Resources.ToolsActive;
        }

        public void FocusContent()
        {
            if (_connectionForm != null)
                _connectionForm.Focus();
        }

        protected MainForm ParentTabs
        {
            get
            {
                return (MainForm) Parent;
            }
        }

        private void urlTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (_omniBarFocusIndex == -1)
                {
                    IConnection newConnection = ConnectionFactory.GetConnection(urlTextBox.Text);
                    newConnection.Guid = Guid.NewGuid();

                    _connection = newConnection;
                }

                else
                    _connection = _validAutoCompleteEntries[_omniBarFocusIndex];

                Connect();
            }

            else if (e.KeyCode == Keys.Up && _omniBarPanel.Visible)
            {
                if (_omniBarFocusIndex > 0)
                {
                    UnfocusOmniBarItem((HtmlPanel)_omniBarPanel.Controls[_omniBarFocusIndex]);
                    FocusOmniBarItem((HtmlPanel)_omniBarPanel.Controls[--_omniBarFocusIndex]);
                }

                e.Handled = true;
            }

            else if (e.KeyCode == Keys.Down && _omniBarPanel.Visible)
            {
                if (_omniBarFocusIndex < _omniBarPanel.Controls.Count - 1)
                {
                    if (_omniBarFocusIndex > -1)
                        UnfocusOmniBarItem((HtmlPanel)_omniBarPanel.Controls[_omniBarFocusIndex]);

                    FocusOmniBarItem((HtmlPanel)_omniBarPanel.Controls[++_omniBarFocusIndex]);
                }

                e.Handled = true;
            }

            else if (e.KeyCode == Keys.Escape && _omniBarPanel.Visible)
            {
                urlTextBox_Leave(null, null);

                e.Handled = true;
            }
        }

        protected void FocusOmniBarItem(HtmlPanel omniBarItem)
        {
            omniBarItem.Text = _fontColorRegex.Replace(_backgroundColorRegex.Replace(omniBarItem.Text, "background-color: #3D9DFD;"), "; color: #9DCDFD;");
        }

        protected void UnfocusOmniBarItem(HtmlPanel omniBarItem)
        {
            omniBarItem.Text = _fontColorRegex.Replace(_backgroundColorRegex.Replace(omniBarItem.Text, "background-color: #FFFFFF;"), "; color: #9999BF;");
        }

        public bool IsConnected
        {
            get
            {
                if (_connectionForm == null)
                    return false;

                return _connectionForm.IsConnected;
            }
        }

        public event EventHandler Connected;

        protected bool AutoHideToolbar
        {
            get
            {
                if (_autoHideToolbar == null)
                    _autoHideToolbar = ParentTabs.Options.AutoHideToolbar;

                return _autoHideToolbar.Value;
            }
        }

        public void Connect()
        {
            if (AutoHideToolbar && !_sizeSet)
            {
                _connectionContainerPanel.Top = 5;
                _connectionContainerPanel.Height += 31;
            }

            _sizeSet = true;
            _connectionForm = ConnectionFactory.CreateConnectionForm(_connection, _connectionContainerPanel);
            Icon = ConnectionFactory.GetProtocol(_connection).ProtocolIcon;
            Text = _connection.DisplayName;

            _suppressOmniBar = true;
            urlTextBox.Text = _connection.Host;
            _suppressOmniBar = false;
            
            if (AutoHideToolbar)
                _connectionForm.ConnectionFormFocused += ConnectionFormFocused;

            _connectionForm.Connected += Connected;

            try
            {
                _connectionForm.Connect();
            }

            catch (Exception)
            {
                Close();
                return;
            }

            ParentTabs.RegisterConnection(this, _connection);
            HideToolbar();
        }

        private void ConnectionFormFocused(object sender, EventArgs eventArgs)
        {
            if (PointToClient(Cursor.Position).Y > 36)
            {
                _bookmarksMenu.Hide();
                _toolsMenu.Hide();

                HideToolbar();
            }
        }

        private void _bookmarksButton_MouseEnter(object sender, EventArgs e)
        {
            _bookmarksButton.BackgroundImage = Resources.ButtonHoverBackground;
        }

        private void _bookmarksButton_MouseLeave(object sender, EventArgs e)
        {
            _bookmarksButton.BackgroundImage = null;
        }

        private void _toolsButton_MouseEnter(object sender, EventArgs e)
        {
            _toolsButton.BackgroundImage = Resources.ButtonHoverBackground;
        }

        private void _toolsButton_MouseLeave(object sender, EventArgs e)
        {
            _toolsButton.BackgroundImage = null;
        }

        private void _exitMenuItem_Click(object sender, EventArgs e)
        {
            ((Form) Parent).Close();
        }

        private void _toolsButton_Click(object sender, EventArgs e)
        {
            _toolsMenu.DefaultDropDownDirection = ToolStripDropDownDirection.Left;
            _toolsMenu.Show(_toolsButton, -187 + _toolsButton.Width, _toolsButton.Height);
        }

        private void _bookmarksButton_Click(object sender, EventArgs e)
        {
            while (_bookmarksMenu.Items.Count > 2)
                _bookmarksMenu.Items.RemoveAt(2);

            if (ParentTabs.Bookmarks.RootFolder.ChildFolders.Count > 0 ||
                ParentTabs.Bookmarks.RootFolder.Bookmarks.Count > 0)
                _bookmarksMenu.Items.Add(new ToolStripSeparator());

            _menuItemConnections.Clear();
            
            PopulateBookmarks(ParentTabs.Bookmarks.RootFolder, _bookmarksMenu.Items, true);

            _bookmarksMenu.DefaultDropDownDirection = ToolStripDropDownDirection.Left;
            _bookmarksMenu.Show(_bookmarksButton, -1 * (_bookmarksMenu.Items.Count > 2 ? _bookmarksMenu.Width : 259) + _bookmarksButton.Width,
                                _bookmarksButton.Height);
        }

        private ToolStripItem PopulateBookmarks(BookmarksFolder currentFolder, ToolStripItemCollection menuItems, bool root)
        {
            ToolStripItemCollection addLocation = menuItems;
            ToolStripMenuItem folderMenuItem = null;

            if (!root)
            {
                folderMenuItem = new ToolStripMenuItem(currentFolder.Name, Resources.Folder)
                                                       {
                                                           DropDownDirection = ToolStripDropDownDirection.Left
                                                       };

                addLocation = folderMenuItem.DropDownItems;
            }

            List<ToolStripItem> addItems =
                currentFolder.ChildFolders.OrderBy(f => f.Name).Select(childFolder => PopulateBookmarks(childFolder, addLocation, false)).ToList();

            foreach (IConnection bookmark in currentFolder.Bookmarks.OrderBy(b => b.DisplayName))
            {
                ToolStripMenuItem bookmarkMenuItem = new ToolStripMenuItem(
                    bookmark.DisplayName, new Icon(ConnectionFactory.GetProtocol(bookmark).ProtocolIcon, 16, 16).ToBitmap(),
                    (object sender, EventArgs e) =>
                        {
                            if (_connectionForm != null)
                            {
                                _connectionForm.CloseParentFormOnDisconnect = false;
                                _connectionForm.Close();
                            }

                            _connection = _menuItemConnections[(ToolStripMenuItem)sender];
                            Connect();
                        });

                _menuItemConnections[bookmarkMenuItem] = bookmark;
                addItems.Add(bookmarkMenuItem);
            }
            
            addLocation.AddRange(addItems.ToArray());

            return folderMenuItem;
        }

        private void _bookmarkMenuItem_Click(object sender, EventArgs e)
        {
            if (_connection == null)
                return;

            SaveConnectionWindow saveWindow = new SaveConnectionWindow(ParentTabs, null);
            saveWindow.ShowDialog(this);

            if (saveWindow.DialogResult != DialogResult.OK)
                return;

            string[] pathComponents = saveWindow.DestinationFolderPath.Split('/');
            TreeNode currentNode = ParentTabs.Bookmarks.FoldersTreeView.Nodes[0];

            for (int i = 2; i < pathComponents.Length; i++)
                currentNode = currentNode.Nodes[Convert.ToInt32(pathComponents[i])];

            IConnection overwriteConnection =
                ParentTabs.Bookmarks.TreeNodeFolders[currentNode].Bookmarks.SingleOrDefault(
                    b =>
                    (b.Name == saveWindow.ConnectionName && !String.IsNullOrEmpty(b.Name)) ||
                    (String.IsNullOrEmpty(b.Name) && b.Host == _connection.Host));

            _connection.Name = saveWindow.ConnectionName;
            _connection.IsBookmark = true;

            Text = _connection.DisplayName;
            ParentTabs.Bookmarks.TreeNodeFolders[currentNode].Bookmarks.Add(_connection);

            if (overwriteConnection != null)
                ParentTabs.Bookmarks.TreeNodeFolders[currentNode].Bookmarks.Remove(overwriteConnection);

            ParentTabs.Bookmarks.Save();
        }

        private void _newTabMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.AddNewTab();
        }

        private void _bookmarksManagerMenuItem2_Click(object sender, EventArgs e)
        {
            ParentTabs.OpenBookmarkManager();
        }

        private void _optionsMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.OpenOptions();
        }

        private void _historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.OpenHistory();
        }

        private void ConnectionWindow_Shown(object sender, EventArgs e)
        {
            if (_connectionForm != null && _connectionForm.IsConnected && !_connectionForm.ContainsFocus)
                _connectionForm.Focus();
        }

        private void _updatesMenuItem_Click(object sender, EventArgs e)
        {
            if (ParentTabs.UpdateAvailable)
                ParentTabs.InstallUpdate();

            else
            {
                CheckForUpdatesWindow checkForUpdatesWindow = new CheckForUpdatesWindow();
                DialogResult result = checkForUpdatesWindow.ShowDialog(ParentTabs);

                if (result == DialogResult.OK)
                    ParentTabs.InstallUpdate();
            }
        }

        public void ShowToolbar()
        {
            if (_toolbarShown || !AutoHideToolbar || !IsHandleCreated || (_animationTimer != null && _animationTimer.Enabled))
                return;

            _animationTicks = 0;
            _animationTimer = new Timer(20);
            _animationTimer.Elapsed += (sender, args) =>
                {
                    if (_animationTicks >= 6 || !toolbarBackground.IsHandleCreated)
                    {
                        _animationTimer.Enabled = false;
                        return;
                    }

                    toolbarBackground.Invoke(
                        new Action(
                            () =>
                                {
                                    toolbarBackground.Height += 5;

                                    if (toolbarBackground.Height == 35)
                                        toolbarBackground.Height = 36;
                                }));
                    _animationTicks++;
                };
            _animationTimer.Enabled = true;
            _toolbarShown = true;
        }

        public void HideToolbar()
        {
            if (!_toolbarShown || !AutoHideToolbar || !IsHandleCreated || (_animationTimer != null && _animationTimer.Enabled))
                return;

            _animationTicks = 0;
            _animationTimer = new Timer(20);
            _animationTimer.Elapsed += (sender, args) =>
                {
                    if (_animationTicks >= 6 || !toolbarBackground.IsHandleCreated)
                    {
                        _animationTimer.Enabled = false;
                        return;
                    }

                    toolbarBackground.Invoke(
                        new Action(
                            () =>
                                {
                                    toolbarBackground.Height -= 5;

                                    if (toolbarBackground.Height == 6)
                                        toolbarBackground.Height = 5;
                                }));
                    _animationTicks++;
                };
            _animationTimer.Enabled = true;
            _toolbarShown = false;
        }

        private void toolbarBackground_Click(object sender, EventArgs e)
        {
            ShowToolbar();
        }

        private void urlTextBox_Enter(object sender, EventArgs e)
        {
            _autoCompleteEntries.Clear();
        }

        private void urlTextBox_TextChanged(object sender, EventArgs e)
        {
            if (urlTextBox.Text.Length == 0 || _suppressOmniBar)
            {
                _omniBarPanel.Visible = false;
                _omniBarBorder.Visible = false;
                return;
            }

            IConnection currentlyFocusedItem = null;

            if (_omniBarFocusIndex != -1)
                currentlyFocusedItem = _validAutoCompleteEntries[_omniBarFocusIndex];

            if (_autoCompleteEntries.Count == 0)
            {
                List<IConnection> bookmarks = new List<IConnection>();
                GetAllBookmarks(ParentTabs.Bookmarks.RootFolder, bookmarks);

                _autoCompleteEntries.AddRange(bookmarks);
                _autoCompleteEntries.AddRange(
                    ParentTabs.History.Connections.OrderByDescending(c => c.LastConnection).Distinct(
                        new EqualityComparer<HistoryWindow.HistoricalConnection>(
                            (x, y) => x.Connection.Host == y.Connection.Host)).Where(
                                c => _autoCompleteEntries.FindIndex(a => a.Host == c.Connection.Host) == -1).Select
                        (c => c.Connection));
            }

            _validAutoCompleteEntries = _autoCompleteEntries.Where(c => c.DisplayName.IndexOf(urlTextBox.Text, StringComparison.InvariantCultureIgnoreCase) != -1 || c.Host.IndexOf(urlTextBox.Text, StringComparison.InvariantCultureIgnoreCase) != -1).OrderBy(c => c.DisplayName).Take(6).ToList();

            if (_validAutoCompleteEntries.Count > 0)
            {
                _omniBarPanel.SuspendLayout();
                _omniBarBorder.SuspendLayout();

                for (int i = 0; i < _validAutoCompleteEntries.Count; i++)
                {
                    IConnection connection = _validAutoCompleteEntries[i];
                    HtmlPanel autoCompletePanel = _omniBarPanel.Controls[i] as HtmlPanel;

                    autoCompletePanel.Text =
                        String.Format(
                            @"<div style=""background-color: #FFFFFF; padding: 5px; font-family: {3}; font-size: {4}pt; height: 30px; color: #9999BF;""><font color=""green"">{0}://{1}</font>{2}</div>",
                            ConnectionFactory.GetProtocol(connection).ProtocolPrefix,
                            Regex.Replace(connection.Host, urlTextBox.Text, "<b>$0</b>", RegexOptions.IgnoreCase), connection.DisplayName == connection.Host
                                                                                                                       ? ""
                                                                                                                       : " - " +
                                                                                                                         Regex.Replace(
                                                                                                                             connection.DisplayName,
                                                                                                                             urlTextBox.Text, "<b>$0</b>",
                                                                                                                             RegexOptions.IgnoreCase),
                            urlTextBox.Font.FontFamily.GetName(0), urlTextBox.Font.SizeInPoints);

                    if (connection == currentlyFocusedItem)
                        FocusOmniBarItem(autoCompletePanel);
                }

                _omniBarPanel.Height = _validAutoCompleteEntries.Count * 30 - 1;
                _omniBarBorder.Height = _omniBarPanel.Height + 2;
                _omniBarPanel.Visible = true;
                _omniBarBorder.Visible = true;
                _omniBarPanel.ResumeLayout();
                _omniBarBorder.ResumeLayout();
                _omniBarPanel.PerformLayout();
                _omniBarBorder.PerformLayout();
            }

            else
            {
                _omniBarPanel.Visible = false;
                _omniBarBorder.Visible = false;
            }
        }

        void autoCompletePanel_MouseLeave(object sender, EventArgs e)
        {
            if (_omniBarFocusIndex != -1 && (sender as HtmlPanel) == (_omniBarPanel.Controls[_omniBarFocusIndex] as HtmlPanel))
                return;

            (sender as HtmlPanel).Text = _backgroundColorRegex.Replace((sender as HtmlPanel).Text, "background-color: #FFFFFF;");
        }

        void autoCompletePanel_MouseEnter(object sender, EventArgs e)
        {
            if (_omniBarFocusIndex != -1 && (sender as HtmlPanel) == (_omniBarPanel.Controls[_omniBarFocusIndex] as HtmlPanel))
                return;

            (sender as HtmlPanel).Text = _backgroundColorRegex.Replace((sender as HtmlPanel).Text, "background-color: #CDE5FE;");
        }

        protected void GetAllBookmarks(BookmarksFolder currentFolder, List<IConnection> bookmarks)
        {
            bookmarks.AddRange(currentFolder.Bookmarks);

            foreach (BookmarksFolder childFolder in currentFolder.ChildFolders)
                GetAllBookmarks(childFolder, bookmarks);
        }

        private void urlTextBox_Leave(object sender, EventArgs e)
        {
            _omniBarFocusIndex = -1;
            _omniBarBorder.Visible = false;
            _omniBarPanel.Visible = false;
        }

        protected class EqualityComparer<T> : IEqualityComparer<T>
        {
            public EqualityComparer(Func<T, T, bool> cmp)
            {
                this.cmp = cmp;
            }
            public bool Equals(T x, T y)
            {
                return cmp(x, y);
            }

            public int GetHashCode(T obj)
            {
                return obj.GetHashCode();
            }

            public Func<T, T, bool> cmp { get; set; }
        }
    }
}