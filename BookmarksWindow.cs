using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using System.Xml;

namespace EasyConnect
{
    public partial class BookmarksWindow : Form
    {
        protected string _bookmarksFileName =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Bookmarks.xml";

        protected MainForm.ConnectionDelegate _connectionDelegate = null;
        protected Dictionary<TreeNode, BookmarksFolder> _folderTreeNodes = new Dictionary<TreeNode, BookmarksFolder>();

        protected Dictionary<ListViewItem, RDCConnection> _listViewConnections =
            new Dictionary<ListViewItem, RDCConnection>();

        protected Dictionary<ListViewItem, BookmarksFolder> _listViewFolders =
            new Dictionary<ListViewItem, BookmarksFolder>();

        protected SecureString _password = null;
        protected BookmarksFolder _rootFolder = new BookmarksFolder();

        public BookmarksWindow(MainForm.ConnectionDelegate connectionDelegate, SecureString password)
        {
            InitializeComponent();

            _connectionDelegate = connectionDelegate;
            _password = password;
            _bookmarksTreeView.Sorted = true;

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
                return (MainForm) Parent;
            }
        }

        public SecureString Password
        {
            set
            {
                _password = value;
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

            foreach (BookmarksFolder childFolder in folder.ChildFolders.OrderBy(f => f.Name))
            {
                ListViewItem item = new ListViewItem(new string[]
                                                         {
                                                             childFolder.Name
                                                         }, 1);

                _listViewFolders[item] = childFolder;
                _bookmarksListView.Items.Add(item);
            }

            foreach (RDCConnection bookmark in folder.Bookmarks.OrderBy(b => String.IsNullOrEmpty(b.Name)
                                                                                 ? b.Host
                                                                                 : b.Name))
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
                    ParentTabs.SelectedTab =
                        _connectionDelegate(_listViewConnections[_bookmarksListView.SelectedItems[0]]);
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

        private void _openBookmarkNewTabMenuItem_Click(object sender, EventArgs e)
        {
            ParentTabs.SelectedTab = _connectionDelegate(_listViewConnections[_bookmarksListView.SelectedItems[0]]);
        }

        private void _editBookmarkMenuItem_Click(object sender, EventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(this,
                                                                     _listViewConnections[
                                                                         _bookmarksListView.SelectedItems[0]],
                                                                     _connectionDelegate, _password);
            connectionWindow.ShowDialog();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _folderTreeNodes[_bookmarksTreeView.SelectedNode].Bookmarks.Remove(
                _listViewConnections[_bookmarksListView.SelectedItems[0]]);

            _listViewConnections.Remove(_bookmarksListView.SelectedItems[0]);
            _bookmarksListView.Items.Remove(_bookmarksListView.SelectedItems[0]);

            Save();
        }

        private void _folderOpenAllMenuItem_Click(object sender, EventArgs e)
        {
            OpenAllBookmarks(_folderTreeNodes[_bookmarksTreeView.SelectedNode]);
        }

        private void OpenAllBookmarks(BookmarksFolder folder)
        {
            foreach (RDCConnection connection in folder.Bookmarks)
                _connectionDelegate(connection);

            foreach (BookmarksFolder childFolder in folder.ChildFolders)
                OpenAllBookmarks(childFolder);
        }

        private void _bookmarksTreeView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeViewHitTestInfo hitTestInfo = _bookmarksTreeView.HitTest(e.Location);

                if (hitTestInfo.Node != null)
                {
                    _bookmarksTreeView.SelectedNode = hitTestInfo.Node;
                    _folderContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void _addBookmarkMenuItem_Click(object sender, EventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(this, _connectionDelegate, _password);
            connectionWindow.ShowDialog();
        }

        private void _addFolderMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode newNode = _bookmarksTreeView.SelectedNode.Nodes.Add("New folder");
            _folderTreeNodes[newNode] = new BookmarksFolder
                                            {
                                                Name = newNode.Text
                                            };
            _folderTreeNodes[_bookmarksTreeView.SelectedNode].ChildFolders.Add(_folderTreeNodes[newNode]);

            _bookmarksTreeView.SelectedNode.Expand();
            _bookmarksTreeView.SelectedNode = newNode;

            Save();
            newNode.BeginEdit();
        }

        private void _bookmarksTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.CancelEdit || String.IsNullOrEmpty(e.Label))
                return;

            _folderTreeNodes[_bookmarksTreeView.SelectedNode].Name = e.Label;
            Save();

            _bookmarksTreeView.BeginInvoke(new Action(_bookmarksTreeView.Sort));
        }

        private void _renameFolderMenuItem_Click(object sender, EventArgs e)
        {
            _bookmarksTreeView.SelectedNode.BeginEdit();
        }

        private void _deleteFolderMenuItem_Click(object sender, EventArgs e)
        {
            _folderTreeNodes[_bookmarksTreeView.SelectedNode.Parent].ChildFolders.Remove(
                _folderTreeNodes[_bookmarksTreeView.SelectedNode]);

            RemoveAllFolders(_bookmarksTreeView.SelectedNode);

            _bookmarksListView.Items.Clear();
            _bookmarksTreeView.SelectedNode.Parent.Nodes.Remove(_bookmarksTreeView.SelectedNode);

            Save();
        }

        private void RemoveAllFolders(TreeNode currentNode)
        {
            foreach (TreeNode childNode in currentNode.Nodes)
                RemoveAllFolders(childNode);

            _folderTreeNodes.Remove(currentNode);
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

        private void createFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newFolderButton_Click(null, null);
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