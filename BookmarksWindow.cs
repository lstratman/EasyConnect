using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Forms;
using System.Xml;
using Stratman.Windows.Forms.TitleBarTabs;

namespace EasyConnect
{
    public partial class BookmarksWindow : Form
    {
        protected string _bookmarksFileName =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Bookmarks.xml";

        protected Dictionary<TreeNode, BookmarksFolder> _folderTreeNodes = new Dictionary<TreeNode, BookmarksFolder>();

        protected Dictionary<ListViewItem, RDCConnection> _listViewConnections =
            new Dictionary<ListViewItem, RDCConnection>();

        protected Dictionary<ListViewItem, BookmarksFolder> _listViewFolders =
            new Dictionary<ListViewItem, BookmarksFolder>();

        protected BookmarksFolder _rootFolder = new BookmarksFolder();

        protected MainForm _applicationForm = null;

        public BookmarksWindow(MainForm applicationForm)
        {
            InitializeComponent();

            _applicationForm = applicationForm;
            _bookmarksFoldersTreeView.Sorted = true;
            _bookmarksListView.ListViewItemSorter = new BookmarksListViewComparer();

            if (File.Exists(_bookmarksFileName))
            {
                _rootFolder.Bookmarks.Clear();
                _rootFolder.ChildFolders.Clear();

                _rootFolder.Bookmarks.CollectionModified += Bookmarks_CollectionModified;
                _rootFolder.ChildFolders.CollectionModified += ChildFolders_CollectionModified;

                _folderTreeNodes[_bookmarksFoldersTreeView.Nodes[0]] = RootFolder;

                XmlDocument bookmarks = new XmlDocument();
                bookmarks.Load(_bookmarksFileName);

                InitializeTreeView(bookmarks.SelectSingleNode("/bookmarks"), _rootFolder, _bookmarksFoldersTreeView.Nodes[0]);

                _bookmarksFoldersTreeView.Nodes[0].Expand();
            }
        }

        void ChildFolders_CollectionModified(object sender, ListModificationEventArgs e)
        {
            ListWithEvents<BookmarksFolder> childFolders = sender as ListWithEvents<BookmarksFolder>;
            bool sortListView = false;

            if (e.Modification == ListModification.ItemModified || e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded)
            {
                for (int i = e.StartIndex; i < e.StartIndex + e.Count; i++)
                {
                    BookmarksFolder currentFolder = childFolders[i];
                    TreeNode parentTreeNode =
                        _folderTreeNodes.Single(kvp => kvp.Value == currentFolder.ParentFolder).Key;
                    TreeNode folderTreeNode = _folderTreeNodes.SingleOrDefault(kvp => kvp.Value == currentFolder).Key;

                    if (folderTreeNode == null)
                    {
                        folderTreeNode = new TreeNode(currentFolder.Name);
                        parentTreeNode.Nodes.Add(folderTreeNode);

                        currentFolder.ChildFolders.CollectionModified += ChildFolders_CollectionModified;
                        currentFolder.Bookmarks.CollectionModified += Bookmarks_CollectionModified;
                    }

                    else
                    {
                        folderTreeNode.Parent.Nodes.Remove(folderTreeNode);
                        parentTreeNode.Nodes.Add(folderTreeNode);
                    }

                    if (_bookmarksFoldersTreeView.SelectedNode == parentTreeNode)
                    {
                        ListViewItem newItem = new ListViewItem(currentFolder.Name, 1);

                        _bookmarksListView.Items.Add(newItem);
                        _listViewFolders[newItem] = currentFolder;

                        sortListView = true;
                    }

                    _folderTreeNodes[folderTreeNode] = currentFolder;
                }
            }

            else
            {
                KeyValuePair<TreeNode, BookmarksFolder> containerFolder =
                    _folderTreeNodes.SingleOrDefault(kvp => kvp.Value.ChildFolders == childFolders);

                if (containerFolder.Key != null)
                {
                    for (int i = 0; i < containerFolder.Key.Nodes.Count; i++)
                    {
                        TreeNode treeNode = containerFolder.Key.Nodes[i];

                        if (!containerFolder.Value.ChildFolders.Contains(_folderTreeNodes[treeNode]))
                        {
                            RemoveAllFolders(treeNode);
                            containerFolder.Key.Nodes.Remove(treeNode);

                            KeyValuePair<ListViewItem, BookmarksFolder> listViewItem =
                                _listViewFolders.SingleOrDefault(kvp => kvp.Value == _folderTreeNodes[treeNode]);

                            if (listViewItem.Key != null)
                            {
                                _bookmarksListView.Items.Remove(listViewItem.Key);
                                _listViewFolders.Remove(listViewItem.Key);

                                sortListView = true;
                            }

                            i--;
                        }
                    }
                }
            }

            if (IsHandleCreated)
            {
                _bookmarksFoldersTreeView.BeginInvoke(new Action(_bookmarksFoldersTreeView.Sort));

                if (sortListView)
                    _bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));
            }
        }

        void Bookmarks_CollectionModified(object sender, ListModificationEventArgs e)
        {
            ListWithEvents<RDCConnection> bookmarks = sender as ListWithEvents<RDCConnection>;
            bool sortListView = false;

            if (e.Modification == ListModification.ItemModified || e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded)
            {
                for (int i = e.StartIndex; i < e.StartIndex + e.Count; i++)
                {
                    RDCConnection currentBookmark = bookmarks[i];
                    TreeNode parentTreeNode =
                        _folderTreeNodes.Single(kvp => kvp.Value == currentBookmark.ParentFolder).Key;

                    if (_bookmarksFoldersTreeView.SelectedNode == parentTreeNode)
                    {
                        ListViewItem newItem = new ListViewItem(currentBookmark.DisplayName, 0);
                        newItem.SubItems.Add(currentBookmark.Host);

                        _listViewConnections[newItem] = currentBookmark;
                        _bookmarksListView.Items.Add(newItem);

                        sortListView = true;
                    }
                }
            }

            else
            {
                KeyValuePair<TreeNode, BookmarksFolder> containerFolder =
                    _folderTreeNodes.SingleOrDefault(kvp => kvp.Value.Bookmarks == bookmarks);

                if (containerFolder.Key != null && containerFolder.Key == _bookmarksFoldersTreeView.SelectedNode)
                {
                    for (int i = 0; i < _bookmarksListView.Items.Count; i++)
                    {
                        ListViewItem bookmark = _bookmarksListView.Items[i];

                        if (bookmark.ImageIndex != 0)
                            continue;

                        if (!containerFolder.Value.Bookmarks.Contains(_listViewConnections[bookmark]))
                        {
                            _listViewConnections.Remove(bookmark);
                            _bookmarksListView.Items.Remove(bookmark);
                            sortListView = true;

                            i--;
                        }
                    }
                }
            }

            if (IsHandleCreated && sortListView)
                _bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));
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

        public Dictionary<TreeNode, BookmarksFolder> TreeNodeFolders
        {
            get
            {
                return _folderTreeNodes;
            }
        }

        public TreeView FoldersTreeView
        {
            get
            {
                return _bookmarksFoldersTreeView;
            }
        }

        protected void InitializeTreeView(XmlNode bookmarksFolder, BookmarksFolder currentFolder, TreeNode currentNode)
        {
            if (bookmarksFolder == null)
                return;

            foreach (XmlNode bookmark in bookmarksFolder.SelectNodes("bookmark"))
                currentFolder.Bookmarks.Add(new RDCConnection(bookmark, _applicationForm.Password));

            foreach (XmlNode folder in bookmarksFolder.SelectNodes("folder"))
            {
                BookmarksFolder newFolder = new BookmarksFolder
                                                {
                                                    Name = folder.SelectSingleNode("@name").Value
                                                };

                currentFolder.ChildFolders.Add(newFolder);
                InitializeTreeView(folder, newFolder, _folderTreeNodes.SingleOrDefault(kvp => kvp.Value == newFolder).Key);
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
                ListViewItem item = new ListViewItem(bookmark.DisplayName, 0);
                item.SubItems.Add(bookmark.Host);

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
                        _applicationForm.Connect(_listViewConnections[_bookmarksListView.SelectedItems[0]]);
                }

                else
                {
                    BookmarksFolder folder = _listViewFolders[_bookmarksListView.SelectedItems[0]];
                    TreeNode node = _folderTreeNodes.First(p => p.Value == folder).Key;

                    _bookmarksFoldersTreeView.SelectedNode = node;
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
            ParentTabs.SelectedTab = _applicationForm.Connect(_listViewConnections[_bookmarksListView.SelectedItems[0]]);
        }

        private void _editBookmarkMenuItem_Click(object sender, EventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(_applicationForm,
                                                                     _listViewConnections[
                                                                         _bookmarksListView.SelectedItems[0]],
                                                                     _folderTreeNodes[
                                                                         _bookmarksFoldersTreeView.SelectedNode]);
            connectionWindow.ShowDialog();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode].Bookmarks.Remove(
                _listViewConnections[_bookmarksListView.SelectedItems[0]]);

            _listViewConnections.Remove(_bookmarksListView.SelectedItems[0]);
            _bookmarksListView.Items.Remove(_bookmarksListView.SelectedItems[0]);

            Save();
        }

        private void _folderOpenAllMenuItem_Click(object sender, EventArgs e)
        {
            OpenAllBookmarks(_folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode]);
        }

        private void OpenAllBookmarks(BookmarksFolder folder)
        {
            foreach (RDCConnection connection in folder.Bookmarks)
                _applicationForm.Connect(connection);

            foreach (BookmarksFolder childFolder in folder.ChildFolders)
                OpenAllBookmarks(childFolder);
        }

        private void _bookmarksTreeView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeViewHitTestInfo hitTestInfo = _bookmarksFoldersTreeView.HitTest(e.Location);

                if (hitTestInfo.Node != null)
                {
                    _bookmarksFoldersTreeView.SelectedNode = hitTestInfo.Node;
                    _folderContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void _addBookmarkMenuItem_Click(object sender, EventArgs e)
        {
            ConnectionWindow connectionWindow = new ConnectionWindow(_applicationForm,
                                                                     _folderTreeNodes[
                                                                         _bookmarksFoldersTreeView.SelectedNode]);
            connectionWindow.ShowDialog();
        }

        private void _addFolderMenuItem_Click(object sender, EventArgs e)
        {
            BookmarksFolder newFolder = new BookmarksFolder
                                            {
                                                Name = "New folder"
                                            };
            _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode].ChildFolders.Add(newFolder);

            TreeNode newNode = _folderTreeNodes.SingleOrDefault(kvp => kvp.Value == newFolder).Key;

            _bookmarksFoldersTreeView.SelectedNode.Expand();
            _bookmarksFoldersTreeView.SelectedNode = newNode;

            Save();
            newNode.BeginEdit();
        }

        private void _bookmarksTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.CancelEdit || String.IsNullOrEmpty(e.Label))
                return;

            _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode].Name = e.Label;
            Save();

            _bookmarksFoldersTreeView.BeginInvoke(new Action(_bookmarksFoldersTreeView.Sort));
        }

        private void _renameFolderMenuItem_Click(object sender, EventArgs e)
        {
            _bookmarksFoldersTreeView.SelectedNode.BeginEdit();
        }

        private void _deleteFolderMenuItem_Click(object sender, EventArgs e)
        {
            _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode.Parent].ChildFolders.Remove(
                _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode]);

            _bookmarksListView.Items.Clear();

            Save();
        }

        private void RemoveAllFolders(TreeNode currentNode)
        {
            foreach (TreeNode childNode in currentNode.Nodes)
                RemoveAllFolders(childNode);

            _folderTreeNodes.Remove(currentNode);
        }

        private void _bookmarksListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.CancelEdit || String.IsNullOrEmpty(e.Label))
                return;

            if (_listViewConnections.ContainsKey(_bookmarksListView.Items[e.Item]))
                _listViewConnections[_bookmarksListView.Items[e.Item]].Name = e.Label;

            else
                _listViewFolders[_bookmarksListView.Items[e.Item]].Name = e.Label;

            Save();

            _bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));
        }

        protected class BookmarksListViewComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                ListViewItem item1 = x as ListViewItem;
                ListViewItem item2 = y as ListViewItem;

                if (item1.ImageIndex != item2.ImageIndex)
                {
                    if (item2.ImageIndex > item1.ImageIndex)
                        return 1;

                    else
                        return -1;
                }

                return String.Compare(item1.Text, item2.Text);
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _bookmarksListView.SelectedItems[0].BeginEdit();
        }
    }
}