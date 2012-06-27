using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using EasyConnect.Protocols;
using EasyConnect.Protocols.Rdp;
using Stratman.Windows.Forms.TitleBarTabs;

namespace EasyConnect
{
    public partial class BookmarksWindow : Form
    {
        protected string _bookmarksFileName =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Bookmarks.xml";

        protected Dictionary<TreeNode, BookmarksFolder> _folderTreeNodes = new Dictionary<TreeNode, BookmarksFolder>();

        protected Dictionary<ListViewItem, IConnection> _listViewConnections =
            new Dictionary<ListViewItem, IConnection>();

        protected Dictionary<ListViewItem, BookmarksFolder> _listViewFolders =
            new Dictionary<ListViewItem, BookmarksFolder>();

        protected BookmarksFolder _rootFolder = new BookmarksFolder();

        protected List<object> _copiedItems = new List<object>();
        protected List<object> _cutItems = new List<object>();

        protected MainForm _applicationForm = null;

        protected bool _showOptionsAfterItemLabelEdit = false;
        protected bool _deferSort = false;

        public BookmarksWindow(MainForm applicationForm)
        {
            InitializeComponent();

            _applicationForm = applicationForm;
            _bookmarksFoldersTreeView.Sorted = true;
            _bookmarksListView.ListViewItemSorter = new BookmarksListViewComparer();

            if (File.Exists(_bookmarksFileName))
            {
                XmlSerializer bookmarksSerializer = new XmlSerializer(typeof(BookmarksFolder));

                using (XmlReader bookmarksReader = new XmlTextReader(_bookmarksFileName))
                {
                    _rootFolder = (BookmarksFolder)bookmarksSerializer.Deserialize(bookmarksReader);
                }
            }

            _rootFolder.Bookmarks.CollectionModified += Bookmarks_CollectionModified;
            _rootFolder.ChildFolders.CollectionModified += ChildFolders_CollectionModified;

            _folderTreeNodes[_bookmarksFoldersTreeView.Nodes[0]] = _rootFolder;

            InitializeTreeView(_rootFolder);

            _bookmarksFoldersTreeView.Nodes[0].Expand();

            foreach (ToolStripMenuItem protocolMenuItem in ConnectionFactory.GetProtocols().Select(protocol => new ToolStripMenuItem(protocol.ProtocolTitle, null, (sender, args) => _addBookmarkMenuItem_Click(protocol))))
                _addBookmarkMenuItem.DropDownItems.Add(protocolMenuItem);
        }

        protected void InitializeTreeView(BookmarksFolder currentFolder)
        {
            if (currentFolder.Bookmarks != null && currentFolder.Bookmarks.Count > 0)
            {
                currentFolder.Bookmarks.ForEach(b => b.ParentFolder = currentFolder);
                Bookmarks_CollectionModified(
                    currentFolder.Bookmarks, new ListModificationEventArgs(ListModification.RangeAdded, 0, currentFolder.Bookmarks.Count));
            }

            if (currentFolder.ChildFolders != null && currentFolder.ChildFolders.Count > 0)
            {
                currentFolder.ChildFolders.ForEach(f => f.ParentFolder = currentFolder);
                ChildFolders_CollectionModified(
                    currentFolder.ChildFolders, new ListModificationEventArgs(ListModification.RangeAdded, 0, currentFolder.ChildFolders.Count));

                foreach (BookmarksFolder childFolder in currentFolder.ChildFolders)
                    InitializeTreeView(childFolder);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C))
            {
                if (_bookmarksFoldersTreeView.Focused && _bookmarksFoldersTreeView.SelectedNode != null)
                {
                    _copyFolderMenuItem_Click(null, null);
                    return true;
                }

                else if (_bookmarksListView.Focused && _bookmarksListView.SelectedItems.Count > 0)
                {
                    copyToolStripMenuItem_Click(null, null);
                    return true;
                }
            }

            else if (keyData == (Keys.Control | Keys.V))
            {
                if (_bookmarksFoldersTreeView.SelectedNode != null)
                {
                    _pasteFolderMenuItem_Click(null, null);
                    return true;
                }
            }

            else if (keyData == (Keys.Control | Keys.X))
            {
                if (_bookmarksFoldersTreeView.Focused && _bookmarksFoldersTreeView.SelectedNode != null)
                {
                    _cutFolderMenuItem_Click(null, null);
                    return true;
                }

                else if (_bookmarksListView.Focused && _bookmarksListView.SelectedItems.Count > 0)
                {
                    _cutBookmarkMenuItem_Click(null, null);
                    return true;
                }
            }

            if (keyData == Keys.Delete)
            {
                if (_bookmarksFoldersTreeView.Focused && _bookmarksFoldersTreeView.SelectedNode != null)
                {
                    _deleteFolderMenuItem_Click(null, null);
                    return true;
                }

                else if (_bookmarksListView.Focused && _bookmarksListView.SelectedItems.Count > 0)
                {
                    deleteToolStripMenuItem1_Click(null, null);
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
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

                        _folderTreeNodes[folderTreeNode] = currentFolder;

                        currentFolder.ChildFolders.CollectionModified += ChildFolders_CollectionModified;
                        currentFolder.Bookmarks.CollectionModified += Bookmarks_CollectionModified;

                        ChildFolders_CollectionModified(
                            currentFolder.ChildFolders, new ListModificationEventArgs(ListModification.RangeAdded, 0, currentFolder.ChildFolders.Count));
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
                            KeyValuePair<ListViewItem, BookmarksFolder> listViewItem =
                                _listViewFolders.SingleOrDefault(kvp => kvp.Value == _folderTreeNodes[treeNode]);

                            if (listViewItem.Key != null)
                            {
                                _bookmarksListView.Items.Remove(listViewItem.Key);
                                _listViewFolders.Remove(listViewItem.Key);

                                sortListView = true;
                            }

                            RemoveAllFolders(treeNode);
                            containerFolder.Key.Nodes.Remove(treeNode);

                            i--;
                        }
                    }
                }
            }

            if (IsHandleCreated && !_deferSort)
            {
                _bookmarksFoldersTreeView.BeginInvoke(new Action(SortTreeView));

                if (sortListView)
                    _bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));
            }
        }

        void Bookmarks_CollectionModified(object sender, ListModificationEventArgs e)
        {
            ListWithEvents<IConnection> bookmarks = sender as ListWithEvents<IConnection>;
            bool sortListView = false;

            if (e.Modification == ListModification.ItemModified || e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded)
            {
                for (int i = e.StartIndex; i < e.StartIndex + e.Count; i++)
                {
                    IConnection currentBookmark = bookmarks[i];
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

            if (IsHandleCreated && sortListView && !_deferSort)
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

        public void Save()
        {
            FileInfo destinationFile = new FileInfo(_bookmarksFileName);
            XmlSerializer bookmarksSerializer = new XmlSerializer(typeof(BookmarksFolder));

            Directory.CreateDirectory(destinationFile.DirectoryName);
            
            using (XmlWriter bookmarksWriter = new XmlTextWriter(_bookmarksFileName, new UnicodeEncoding()))
            {
                bookmarksSerializer.Serialize(bookmarksWriter, _rootFolder);
                bookmarksWriter.Flush();
            }
        }

        public void Export(string path)
        {
            FileInfo destinationFile = new FileInfo(path);
            XmlSerializer bookmarksSerializer = new XmlSerializer(typeof(BookmarksFolder));

            Directory.CreateDirectory(destinationFile.DirectoryName);

            object clonedFolder = _rootFolder.CloneAnon();

            using (XmlWriter bookmarksWriter = new XmlTextWriter(path, new UnicodeEncoding()))
            {
                bookmarksSerializer.Serialize(bookmarksWriter, clonedFolder);
                bookmarksWriter.Flush();
            }
        }

        public void Import(string path)
        {
            
            //ISSUE: Display shows old and new Bookmark items
            //ISSUE: Dialog shows truncated suggested file name

            if (MessageBox.Show("This will erase any currently saved bookmarks and import the contents of the selected file. Do you wish to continue?", "Continue with import?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
            {

                if (File.Exists(path))
                {
                    XmlSerializer bookmarksSerializer = new XmlSerializer(typeof(BookmarksFolder));

                    using (XmlReader bookmarksReader = new XmlTextReader(path))
                    {
                        //_rootFolder
                        BookmarksFolder importedRootFolder = (BookmarksFolder)bookmarksSerializer.Deserialize(bookmarksReader);

                        importedRootFolder.Bookmarks.CollectionModified += Bookmarks_CollectionModified;
                        importedRootFolder.ChildFolders.CollectionModified += ChildFolders_CollectionModified;

                        _rootFolder = importedRootFolder;

                        _folderTreeNodes[_bookmarksFoldersTreeView.Nodes[0]] = _rootFolder;

                        InitializeTreeView(_rootFolder);
                    }

                    _bookmarksFoldersTreeView.Nodes[0].Expand();
                    Save();
                }
            }
        }

        private void _bookmarksTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _bookmarksListView.Items.Clear();
            _listViewConnections.Clear();
            _listViewFolders.Clear();

            if (!_folderTreeNodes.ContainsKey(e.Node))
                return;

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

            foreach (IConnection bookmark in folder.Bookmarks)
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
            {
                pasteToolStripMenuItem.Enabled = (_copiedItems.Count > 0 || _cutItems.Count > 0) && _listViewFolders.ContainsKey(_bookmarksListView.SelectedItems[0]) &&
                                                 _bookmarksListView.SelectedItems.Count == 1;
                _bookmarkContextMenu.Show(Cursor.Position);
            }
        }

        private void _openBookmarkNewTabMenuItem_Click(object sender, EventArgs e)
        {
            //ParentTabs.SelectedTab = _applicationForm.Connect(_listViewConnections[_bookmarksListView.SelectedItems[0]]);

            OpenSelectedBookmarks();
        }

        private void _editBookmarkMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem selectedItem = _bookmarksListView.SelectedItems[0];
            TitleBarTab optionsTab = new TitleBarTab(ParentTabs)
                {
                    Content = new OptionsWindow(ParentTabs)
                        {
                            OptionsForms = new List<Form>
                                {
                                    ConnectionFactory.CreateOptionsForm(
                                        _listViewConnections[selectedItem])
                                }
                        }
                };

            optionsTab.Content.FormClosed +=
                (o, args) => selectedItem.SubItems[1].Text = _listViewConnections[selectedItem].Host;

            ParentTabs.Tabs.Add(optionsTab);
            ParentTabs.ResizeTabContents(optionsTab);
            ParentTabs.SelectedTab = optionsTab;
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            _deferSort = true;

            foreach (ListViewItem selectedItem in _bookmarksListView.SelectedItems)
            {
                if (_listViewConnections.ContainsKey(selectedItem))
                {
                    _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode].Bookmarks.Remove(
                        _listViewConnections[selectedItem]);
                    _listViewConnections.Remove(selectedItem);
                }

                else
                {
                    _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode].ChildFolders.Remove(
                        _listViewFolders[selectedItem]);
                    _listViewFolders.Remove(selectedItem);
                }

                _bookmarksListView.Items.Remove(selectedItem);
            }

            _deferSort = false;

            _bookmarksFoldersTreeView.BeginInvoke(new Action(SortTreeView));
            _bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));

            Save();
        }

        private void _folderOpenAllMenuItem_Click(object sender, EventArgs e)
        {
            OpenAllBookmarks(_folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode]);
        }

        private void OpenAllBookmarks(BookmarksFolder folder)
        {
            foreach (IConnection connection in folder.Bookmarks)
                _applicationForm.Connect(connection);

            foreach (BookmarksFolder childFolder in folder.ChildFolders)
                OpenAllBookmarks(childFolder);
        }

        private void OpenSelectedBookmarks()
        {

            foreach (ListViewItem item in _bookmarksListView.SelectedItems)
            {
                _applicationForm.Connect(_listViewConnections[item]);
            }
        }

        private void _bookmarksTreeView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeViewHitTestInfo hitTestInfo = _bookmarksFoldersTreeView.HitTest(e.Location);

                if (hitTestInfo.Node != null)
                {
                    _bookmarksFoldersTreeView.SelectedNode = hitTestInfo.Node;
                    _pasteFolderMenuItem.Enabled = _copiedItems.Count > 0 || _cutItems.Count > 0;

                    _folderContextMenu.Show(Cursor.Position);
                }
            }
        }

        private void _addBookmarkMenuItem_Click(IProtocol type)
        {
            IConnection connection = (IConnection)ConnectionFactory.GetDefaults(type.GetType()).Clone();

            connection.Name = "New Connection";

            _deferSort = true;
            _folderTreeNodes[FoldersTreeView.SelectedNode].Bookmarks.Add(connection);
            _deferSort = false;

            _showOptionsAfterItemLabelEdit = true;

            ListViewItem newListItem = _listViewConnections.First(pair => pair.Value == connection).Key;
            _bookmarksListView.SelectedIndices.Clear();

            SortListView();
            Save();

            newListItem.Selected = true;
            newListItem.BeginEdit();
        }

        private void _addFolderMenuItem_Click(object sender, EventArgs e)
        {
            BookmarksFolder newFolder = new BookmarksFolder
                                            {
                                                Name = "New folder"
                                            };

        	_deferSort = true;
            _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode].ChildFolders.Add(newFolder);
        	_deferSort = false;

            TreeNode newNode = _folderTreeNodes.SingleOrDefault(kvp => kvp.Value == newFolder).Key;

            _bookmarksFoldersTreeView.SelectedNode.Expand();
            _bookmarksFoldersTreeView.SelectedNode = newNode;

			SortTreeView();
            Save();
            newNode.BeginEdit();
        }

        private void _bookmarksTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.CancelEdit || String.IsNullOrEmpty(e.Label))
                return;

            _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode].Name = e.Label;
            Save();

            _bookmarksFoldersTreeView.BeginInvoke(new Action(SortTreeView));
        }

        private void _renameFolderMenuItem_Click(object sender, EventArgs e)
        {
            _bookmarksFoldersTreeView.SelectedNode.BeginEdit();
        }

        private void _deleteFolderMenuItem_Click(object sender, EventArgs e)
        {
            _deferSort = true;

            _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode.Parent].ChildFolders.Remove(
                _folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode]);

            _bookmarksListView.Items.Clear();
            _bookmarksFoldersTreeView.BeginInvoke(new Action(_bookmarksFoldersTreeView.Sort));

            _deferSort = false;

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

            if (_showOptionsAfterItemLabelEdit)
            {
                ListViewItem selectedItem = _bookmarksListView.SelectedItems[0];
                TitleBarTab optionsTab = new TitleBarTab(ParentTabs)
                    {
                        Content = new OptionsWindow(ParentTabs)
                            {
                                OptionsForms = new List<Form>
                                    {
                                        ConnectionFactory.CreateOptionsForm(
                                            _listViewConnections[selectedItem])
                                    }
                            }
                    };

                optionsTab.Content.FormClosed +=
                    (o, args) => selectedItem.SubItems[1].Text = _listViewConnections[selectedItem].Host;

                ParentTabs.Tabs.Add(optionsTab);
                ParentTabs.ResizeTabContents(optionsTab);
                ParentTabs.SelectedTab = optionsTab;

                _showOptionsAfterItemLabelEdit = false;
            }

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

        private void _openBookmarkNewWindowMenuItem_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm(new Guid[]
                                                 {
                                                     _listViewConnections[
                                                         _bookmarksListView.SelectedItems[0]].Guid
                                                 });
            mainForm.Show();
        }

        public IConnection FindBookmark(Guid bookmarkGuid)
        {
            return FindBookmark(bookmarkGuid, _rootFolder);
        }

        protected IConnection FindBookmark(Guid bookmarkGuid, BookmarksFolder searchFolder)
        {
            IConnection bookmark = searchFolder.Bookmarks.FirstOrDefault(b => b.Guid == bookmarkGuid);

            if (bookmark != null)
                return bookmark;

            foreach (BookmarksFolder childFolder in searchFolder.ChildFolders)
            {
                bookmark = FindBookmark(bookmarkGuid, childFolder);

                if (bookmark != null)
                    return bookmark;
            }

            return null;
        }

        private void _folderOpenAllNewWindowMenuItem_Click(object sender, EventArgs e)
        {
            List<Guid> bookmarkGuids = new List<Guid>();
            FindAllBookmarks(_folderTreeNodes[_bookmarksFoldersTreeView.SelectedNode], bookmarkGuids);
            
            if (bookmarkGuids.Count > 0)
            {
                MainForm mainForm = new MainForm(bookmarkGuids.ToArray());
                mainForm.Show();
            }
        }

        private void FindAllBookmarks(BookmarksFolder bookmarksFolder, List<Guid> bookmarkGuids)
        {
            bookmarkGuids.AddRange(bookmarksFolder.Bookmarks.Select(bookmark => bookmark.Guid));

            foreach (BookmarksFolder childFolder in bookmarksFolder.ChildFolders)
                FindAllBookmarks(childFolder, bookmarkGuids);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _copiedItems.Clear();
            _cutItems.Clear();

            foreach (ListViewItem item in _bookmarksListView.SelectedItems)
                _copiedItems.Add(
                    _listViewConnections.ContainsKey(item)
                        ? _listViewConnections[item]
                        : (object) _listViewFolders[item]);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteItems(_listViewFolders[_bookmarksListView.SelectedItems[0]]);
        }

        private void _pasteFolderMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode currentFolderNode = FoldersTreeView.SelectedNode;
            
            PasteItems(_folderTreeNodes[currentFolderNode]);
            FoldersTreeView.SelectedNode = currentFolderNode;
        }

        private void PasteItems(BookmarksFolder targetFolder)
        {
            _deferSort = true;

            List<object> source = _cutItems.Union(_copiedItems).ToList();

            if ((source[0] is BookmarksFolder && ((BookmarksFolder)source[0]).ParentFolder == targetFolder) || (source[0] is IConnection && ((IConnection)source[0]).ParentFolder == targetFolder))
            {
                MessageBox.Show(this, "You cannot paste items into their existing parent folders.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<object> clonedItems = (from item in source
                                        select ((ICloneable)item).Clone()).ToList();

            foreach (object clonedItem in clonedItems)
            {
                if (clonedItem is IConnection)
                    targetFolder.Bookmarks.Add((IConnection)clonedItem);

                else
                    targetFolder.MergeFolder((BookmarksFolder)clonedItem);
            }

            if (_cutItems.Count > 0)
            {
                foreach (object cutItem in _cutItems)
                {
                    IConnection connection = cutItem as IConnection;

                    if (connection != null)
                    {
                        connection.ParentFolder.Bookmarks.Remove(connection);

                        if (_listViewConnections.ContainsValue(connection))
                            _bookmarksListView.Items.Remove(_listViewConnections.First(kvp => kvp.Value == connection).Key);
                    }

                    else
                    {
                        BookmarksFolder folder = cutItem as BookmarksFolder;

                        folder.ParentFolder.ChildFolders.Remove(folder);

                        if (_listViewFolders.ContainsValue(folder))
                            _bookmarksListView.Items.Remove(_listViewFolders.First(kvp => kvp.Value == folder).Key);
                    }
                }

                _cutItems.Clear();
            }

            _bookmarksFoldersTreeView.BeginInvoke(new Action(SortTreeView));
            _bookmarksListView.BeginInvoke(new Action(_bookmarksListView.Sort));

            _deferSort = false;

            Save();
        }

        private void _copyFolderMenuItem_Click(object sender, EventArgs e)
        {
            _copiedItems.Clear();
            _cutItems.Clear();
            _copiedItems.Add(_folderTreeNodes[FoldersTreeView.SelectedNode]);
        }

        private void SortTreeView()
        {
            TreeNode currentlySelectedNode = _bookmarksFoldersTreeView.SelectedNode;
            
			_bookmarksFoldersTreeView.BeginUpdate();
            _bookmarksFoldersTreeView.Sort();
            _bookmarksFoldersTreeView.SelectedNode = currentlySelectedNode;
			_bookmarksFoldersTreeView.EndUpdate();
        }

        private void SortListView()
        {
            _bookmarksListView.BeginUpdate();
            _bookmarksListView.Sort();
            _bookmarksListView.EndUpdate();
        }

        private void _cutBookmarkMenuItem_Click(object sender, EventArgs e)
        {
            _copiedItems.Clear();
            _cutItems.Clear();

            foreach (ListViewItem item in _bookmarksListView.SelectedItems)
            {
                _cutItems.Add(
                    _listViewConnections.ContainsKey(item)
                        ? _listViewConnections[item]
                        : (object) _listViewFolders[item]);

                item.ForeColor = Color.Gray;
            }
        }

        private void _cutFolderMenuItem_Click(object sender, EventArgs e)
        {
            _copiedItems.Clear();
            _cutItems.Clear();

            _cutItems.Add(_folderTreeNodes[FoldersTreeView.SelectedNode]);
            FoldersTreeView.SelectedNode.ForeColor = Color.Gray;
        }

        private void _exportBookMarkMenuitem_Click(object sender, EventArgs e)
        {
            _bookmarkExportDialog.ShowDialog();
            Export(_bookmarkExportDialog.FileName);
        }

        private void _importBookmarkMenuItem_Click(object sender, EventArgs e)
        {
            _bookmarkImportDialog.ShowDialog();
            Import(_bookmarkImportDialog.FileName);
        }

        private void _bookmarkExportDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void _bookmarkImportDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

    }
}