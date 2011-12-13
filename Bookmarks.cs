using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Security;
using Stratman.Windows.Forms.TitleBarTabs;

namespace EasyConnect
{
    public partial class BookmarksWindow : Form
    {
        protected string _bookmarksFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Bookmarks.xml";
        protected MainForm.ConnectionDelegate _connectionDelegate = null;
        protected SecureString _password = null;
        protected BookmarksFolder _rootFolder = new BookmarksFolder();
        protected Dictionary<TreeNode, BookmarksFolder> _folderTreeNodes = new Dictionary<TreeNode, BookmarksFolder>();
        protected Dictionary<ListViewItem, RDCConnection> _listViewConnections = new Dictionary<ListViewItem, RDCConnection>();
        protected Dictionary<ListViewItem, BookmarksFolder> _listViewFolders = new Dictionary<ListViewItem, BookmarksFolder>();

        public BookmarksFolder RootFolder
        {
            get
            {
                return _rootFolder;
            }
        }

        protected MainForm ParentTabs
        {
            get
            {
                return (MainForm)Parent;
            }
        }

        public BookmarksWindow(MainForm.ConnectionDelegate connectionDelegate, SecureString password)
        {
            InitializeComponent();

            _connectionDelegate = connectionDelegate;
            _password = password;

            if (File.Exists(_bookmarksFileName))
            {
                XmlDocument bookmarks = new XmlDocument();
                bookmarks.Load(_bookmarksFileName);

                _rootFolder.Bookmarks.Clear();
                _rootFolder.ChildFolders.Clear();
                _folderTreeNodes[_bookmarksTreeView.Nodes[0]] = RootFolder;

                InitializeTreeView(bookmarks.SelectSingleNode("/bookmarks"), _rootFolder, _bookmarksTreeView.Nodes[0]);

                _bookmarksTreeView.Nodes[0].Expand();
            }
        }

        protected void InitializeTreeView(XmlNode bookmarksFolder, BookmarksFolder currentFolder, TreeNode currentNode)
        {
            if (bookmarksFolder == null)
                return;

            foreach (XmlNode bookmark in bookmarksFolder.SelectNodes("bookmark"))
                currentFolder.Bookmarks.Add(new RDCConnection(bookmark, _password));

            foreach (XmlNode folder in bookmarksFolder.SelectNodes("folder"))
            {
                BookmarksFolder newFolder = new BookmarksFolder
                                                {
                                                    Name = folder.SelectSingleNode("@name").Value
                                                };

                TreeNode newFolderNode = new TreeNode(newFolder.Name);
                _folderTreeNodes[newFolderNode] = newFolder;

                currentNode.Nodes.Add(newFolderNode);

                currentFolder.ChildFolders.Add(newFolder);
                InitializeTreeView(folder, newFolder, newFolderNode);
            }
        }

        public SecureString Password
        {
            set
            {
                _password = value;
            }
        }

        public void Save()
        {
            XmlDocument bookmarksFile = new XmlDocument();
            XmlNode rootNode = bookmarksFile.CreateNode(XmlNodeType.Element, "bookmarks", null);

            bookmarksFile.AppendChild(rootNode);
            SaveTreeView(_rootFolder, rootNode);

            FileInfo destinationFile = new FileInfo(_bookmarksFileName);

            Directory.CreateDirectory(destinationFile.DirectoryName);
            bookmarksFile.Save(_bookmarksFileName);
        }

        protected void SaveTreeView(BookmarksFolder currentFolder, XmlNode parentNode)
        {
            foreach (RDCConnection bookmark in currentFolder.Bookmarks)
            {
                XmlNode connectionNode = parentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "bookmark", null);
                parentNode.AppendChild(connectionNode);

                bookmark.ToXmlNode(connectionNode);
            }

            foreach (BookmarksFolder folder in currentFolder.ChildFolders)
            {
                XmlNode folderNode = parentNode.OwnerDocument.CreateNode(XmlNodeType.Element, "folder", null);

                folderNode.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("name"));
                folderNode.Attributes["name"].Value = folder.Name;

                parentNode.AppendChild(folderNode);
                SaveTreeView(folder, folderNode);
            }
        }

        private void _bookmarksTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _bookmarksListView.Items.Clear();
            _listViewConnections.Clear();
            _listViewFolders.Clear();

            BookmarksFolder folder = _folderTreeNodes[e.Node];

            foreach (BookmarksFolder childFolder in folder.ChildFolders)
            {
                ListViewItem item = new ListViewItem(new string[]
                                                         {
                                                             childFolder.Name
                                                         }, 1);

                _listViewFolders[item] = childFolder;
                _bookmarksListView.Items.Add(item);
            }

            foreach (RDCConnection bookmark in folder.Bookmarks)
            {
                ListViewItem item = new ListViewItem(new string[]
                                                         {
                                                             String.IsNullOrEmpty(bookmark.Name)
                                                                 ? bookmark.Host
                                                                 : bookmark.Name, bookmark.Host
                                                         }, 0);

                _listViewConnections[item] = bookmark;
                _bookmarksListView.Items.Add(item);
            }
        }

        private void _bookmarksListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_bookmarksListView.SelectedItems.Count > 0)
            {
                if (_listViewConnections.ContainsKey(_bookmarksListView.SelectedItems[0]))
                {
                    TitleBarTab newTab = _connectionDelegate(_listViewConnections[_bookmarksListView.SelectedItems[0]]);
                    ParentTabs.SelectedTabIndex = ParentTabs.Tabs.IndexOf(newTab);
                }

                else
                {
                    BookmarksFolder folder = _listViewFolders[_bookmarksListView.SelectedItems[0]];
                    TreeNode node = _folderTreeNodes.First(p => p.Value == folder).Key;

                    _bookmarksTreeView.SelectedNode = node;
                }
            }
        }

        private void _bookmarksListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && _bookmarksListView.SelectedItems.Count > 0)
                _bookmarkContextMenu.Show(Cursor.Position);
        }

        /*private void newFolderButton_Click(object sender, EventArgs e)
        {
            RenameWindow folderNameWindow = new RenameWindow();

            folderNameWindow.ValueText = "New Folder";
            folderNameWindow.Title = "Folder Name";

            DialogResult result = folderNameWindow.ShowDialog(this);

            if (result != DialogResult.OK)
                return;

            TreeNode newNode = new TreeNode(folderNameWindow.ValueText, 0, 0);

            favoritesTreeView.SelectedNode.Expand();
            AddTreeNode(favoritesTreeView.SelectedNode.Nodes, newNode);
            favoritesTreeView.SelectedNode = newNode;

            Save();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            favoritesTreeView.SelectedNode.Remove();
            Save();
        }

        private void propertiesButton_Click(object sender, EventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(this, _connections[favoritesTreeView.SelectedNode], _connectionDelegate, _password);
            connectionWindow.ShowDialog();
        }

        private void favoritesTreeView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TreeNode currentNode = favoritesTreeView.GetNodeAt(new Point(e.X, e.Y));

            if (currentNode == null || currentNode.ImageIndex != 1)
                return;

            _connectionDelegate(_connections[currentNode]);
        }

        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            propertiesButton_Click(null, null);
        }

        private void createFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newFolderButton_Click(null, null);
        }

        private void newConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(this, _connectionDelegate, _password);
            connectionWindow.ShowDialog();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameWindow folderNameWindow = new RenameWindow();
            TreeNode node = favoritesTreeView.SelectedNode;
            TreeNode parent = node.Parent;

            folderNameWindow.ValueText = node.Text;
            folderNameWindow.Title = "Folder Name";

            DialogResult result = folderNameWindow.ShowDialog(this);

            if (result != DialogResult.OK)
                return;

            node.Remove();
            node.Text = folderNameWindow.ValueText;

            AddTreeNode(parent.Nodes, node);

            Save();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteButton_Click(null, null);
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _connectionDelegate(_connections[favoritesTreeView.SelectedNode]);
        }*/
    }
}
