using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;
using AxMSTSCLib;
using EasyConnect.Common;
using EasyConnect.Properties;
using EasyConnect.Protocols;
using EasyConnect.Protocols.Rdp;
using MSTSCLib;
using Stratman.Windows.Forms.TitleBarTabs;

namespace EasyConnect
{
    public partial class ConnectionWindow : Form
    {
        protected bool _connectClipboard = true;
        protected IConnection _connection = null;
        protected BaseConnectionForm _connectionForm = null;

        protected Dictionary<ToolStripMenuItem, IConnection> _menuItemConnections =
            new Dictionary<ToolStripMenuItem, IConnection>();

        public ConnectionWindow()
        {
            InitializeComponent();
        }

        public ConnectionWindow(IConnection connection)
            : this()
        {
            _connection = connection;
            Text = connection.DisplayName;
            urlTextBox.Text = connection.Host;
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
                IConnection newConnection = ConnectionFactory.GetConnection(urlTextBox.Text);
                
                newConnection.Host = urlTextBox.Text;
                newConnection.Guid = Guid.NewGuid();

                _connection = newConnection;
                Connect();
            }
        }

        public event EventHandler Connected;

        public void Connect()
        {
            _connectionForm = ConnectionFactory.CreateConnectionForm(_connection, _connectionContainerPanel);
            Text = _connection.DisplayName;
            
            _connectionForm.Connected += Connected;
            _connectionForm.Connect();
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

        private void PopulateBookmarks(BookmarksFolder currentFolder, ToolStripItemCollection menuItems, bool root)
        {
            ToolStripItemCollection addLocation = menuItems;

            if (!root)
            {
                ToolStripMenuItem folderMenuItem = new ToolStripMenuItem(currentFolder.Name, Resources.Folder)
                                                       {
                                                           DropDownDirection = ToolStripDropDownDirection.Left
                                                       };

                menuItems.Add(folderMenuItem);
                addLocation = folderMenuItem.DropDownItems;
            }

            foreach (BookmarksFolder childFolder in currentFolder.ChildFolders.OrderBy(f => f.Name))
                PopulateBookmarks(childFolder, addLocation, false);

            foreach (IConnection bookmark in currentFolder.Bookmarks.OrderBy(b => b.DisplayName))
            {
                ToolStripMenuItem bookmarkMenuItem = new ToolStripMenuItem(
                    bookmark.DisplayName, Resources.RDCSmall,
                    (object sender, EventArgs e) =>
                        {
                            _connection = bookmark;
                            Connect();
                        });

                _menuItemConnections[bookmarkMenuItem] = bookmark;
                addLocation.Add(bookmarkMenuItem);
            }
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

            if (_connectionForm == null || !_connectionForm.IsConnected)
                Close();
        }

        private void _optionsMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.OpenOptions();

            if (_connectionForm == null || !_connectionForm.IsConnected)
                Close();
        }

        private void _historyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.OpenHistory();

            if (_connectionForm == null || !_connectionForm.IsConnected)
                Close();
        }

        private void ConnectionWindow_Shown(object sender, EventArgs e)
        {
            if (_connectionForm != null && _connectionForm.IsConnected)
                _connectionForm.Focus();
        }

        private void ConnectionWindow_MouseDown(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("Captured mouse down");
        }
    }
}