using System.Windows.Forms;

namespace EasyConnect
{
    partial class BookmarksWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private SplitContainer _splitContainer;
        private TreeView _bookmarksFoldersTreeView;
        private ImageList _treeViewImageList;
        private ListView _bookmarksListView;
        private ColumnHeader _bookmarkNameColumnHeader;
        private ColumnHeader _bookmarkUriColumnHeader;
        private ImageList _listViewImageList;
        private ContextMenuStrip _folderContextMenu;
        private ToolStripMenuItem _folderOpenAllMenuItem;
        private ToolStripMenuItem _folderOpenAllNewWindowMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem _renameFolderMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem _cutFolderMenuItem;
        private ToolStripMenuItem _copyFolderMenuItem;
        private ToolStripMenuItem _pasteFolderMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem _deleteFolderMenuItem;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem _addBookmarkMenuItem;
        private ToolStripMenuItem _addFolderMenuItem;
        private ContextMenuStrip _bookmarkContextMenu;
        private ToolStripMenuItem _openBookmarkNewTabMenuItem;
        private ToolStripMenuItem _openBookmarkNewWindowMenuItem;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem _editBookmarkMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem _cutBookmarkMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem deleteToolStripMenuItem1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Bookmarks");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BookmarksWindow));
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._bookmarksFoldersTreeView = new System.Windows.Forms.TreeView();
            this._treeViewImageList = new System.Windows.Forms.ImageList(this.components);
            this._bookmarksListView = new System.Windows.Forms.ListView();
            this._bookmarkNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._bookmarkUriColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._bookmarkNotesColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._folderContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._folderOpenAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._folderOpenAllNewWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this._setUsernamePasswordMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._clearUsernamePasswordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._renameFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._cutFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._copyFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._pasteFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this._deleteFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this._addBookmarkMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._addFolderMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this._exportBookMarkMenuitem = new System.Windows.Forms.ToolStripMenuItem();
            this._importBookmarkMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._listViewImageList = new System.Windows.Forms.ImageList(this.components);
            this._bookmarksCard = new EasyConnect.Common.MaterialCard();
            this.panelBackground = new System.Windows.Forms.Panel();
            this._bookmarkContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._openBookmarkNewTabMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openBookmarkNewWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this._editBookmarkMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this._cutBookmarkMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this._bookmarkExportDialog = new System.Windows.Forms.SaveFileDialog();
            this._bookmarkImportDialog = new System.Windows.Forms.OpenFileDialog();
            this._dragImageList = new System.Windows.Forms.ImageList(this.components);
            this.toolbarBackground = new System.Windows.Forms.Panel();
            this._urlPanelContainer = new System.Windows.Forms.Panel();
            this.urlTextBox = new System.Windows.Forms.TextBox();
            this.urlBackground = new System.Windows.Forms.Panel();
            this._iconPictureBox = new System.Windows.Forms.PictureBox();
            this.urlBackgroundRight = new System.Windows.Forms.PictureBox();
            this.urlBackgroundLeft = new System.Windows.Forms.PictureBox();
            this._toolsButton = new System.Windows.Forms.PictureBox();
            this._toolsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._newTabMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._newWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this._historyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this._settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsMenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._updatesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsMenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.headerText = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).BeginInit();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._folderContextMenu.SuspendLayout();
            this._bookmarkContextMenu.SuspendLayout();
            this.toolbarBackground.SuspendLayout();
            this.urlBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._toolsButton)).BeginInit();
            this._toolsMenu.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _splitContainer
            // 
            this._splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._splitContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._splitContainer.Location = new System.Drawing.Point(0, 96);
            this._splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._splitContainer.Panel1.Controls.Add(this._bookmarksFoldersTreeView);
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.AutoScroll = true;
            this._splitContainer.Panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._splitContainer.Panel2.Controls.Add(this._bookmarksListView);
            this._splitContainer.Panel2.Controls.Add(this._bookmarksCard);
            this._splitContainer.Panel2.Controls.Add(this.panelBackground);
            this._splitContainer.Panel2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 32);
            this._splitContainer.Size = new System.Drawing.Size(975, 381);
            this._splitContainer.SplitterDistance = 257;
            this._splitContainer.TabIndex = 0;
            // 
            // _bookmarksFoldersTreeView
            // 
            this._bookmarksFoldersTreeView.AllowDrop = true;
            this._bookmarksFoldersTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._bookmarksFoldersTreeView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this._bookmarksFoldersTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._bookmarksFoldersTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this._bookmarksFoldersTreeView.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._bookmarksFoldersTreeView.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            this._bookmarksFoldersTreeView.HideSelection = false;
            this._bookmarksFoldersTreeView.ImageIndex = 0;
            this._bookmarksFoldersTreeView.ImageList = this._treeViewImageList;
            this._bookmarksFoldersTreeView.LabelEdit = true;
            this._bookmarksFoldersTreeView.Location = new System.Drawing.Point(12, 12);
            this._bookmarksFoldersTreeView.Name = "_bookmarksFoldersTreeView";
            treeNode1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(99)))), ((int)(((byte)(104)))));
            treeNode1.Name = "root";
            treeNode1.Text = "Bookmarks";
            this._bookmarksFoldersTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this._bookmarksFoldersTreeView.SelectedImageIndex = 0;
            this._bookmarksFoldersTreeView.ShowLines = false;
            this._bookmarksFoldersTreeView.ShowRootLines = false;
            this._bookmarksFoldersTreeView.Size = new System.Drawing.Size(233, 358);
            this._bookmarksFoldersTreeView.TabIndex = 0;
            this._bookmarksFoldersTreeView.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this._bookmarksFoldersTreeView_BeforeLabelEdit);
            this._bookmarksFoldersTreeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this._bookmarksTreeView_AfterLabelEdit);
            this._bookmarksFoldersTreeView.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this._bookmarksFoldersTreeView_DrawNode);
            this._bookmarksFoldersTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this._bookmarksFoldersTreeView_ItemDrag);
            this._bookmarksFoldersTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this._bookmarksTreeView_AfterSelect);
            this._bookmarksFoldersTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this._bookmarks_DragDrop);
            this._bookmarksFoldersTreeView.DragEnter += new System.Windows.Forms.DragEventHandler(this._bookmarks_DragEnter);
            this._bookmarksFoldersTreeView.DragOver += new System.Windows.Forms.DragEventHandler(this._bookmarksFoldersTreeView_DragOver);
            this._bookmarksFoldersTreeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this._bookmarksTreeView_MouseClick);
            // 
            // _treeViewImageList
            // 
            this._treeViewImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this._treeViewImageList.ImageSize = new System.Drawing.Size(24, 24);
            this._treeViewImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _bookmarksListView
            // 
            this._bookmarksListView.AllowDrop = true;
            this._bookmarksListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._bookmarksListView.BackColor = System.Drawing.Color.White;
            this._bookmarksListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._bookmarksListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._bookmarkNameColumnHeader,
            this._bookmarkUriColumnHeader,
            this._bookmarkNotesColumnHeader});
            this._bookmarksListView.ContextMenuStrip = this._folderContextMenu;
            this._bookmarksListView.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._bookmarksListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._bookmarksListView.HideSelection = false;
            this._bookmarksListView.LabelEdit = true;
            this._bookmarksListView.Location = new System.Drawing.Point(36, 35);
            this._bookmarksListView.Name = "_bookmarksListView";
            this._bookmarksListView.OwnerDraw = true;
            this._bookmarksListView.Size = new System.Drawing.Size(641, 307);
            this._bookmarksListView.SmallImageList = this._listViewImageList;
            this._bookmarksListView.TabIndex = 0;
            this._bookmarksListView.UseCompatibleStateImageBehavior = false;
            this._bookmarksListView.View = System.Windows.Forms.View.Details;
            this._bookmarksListView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this._bookmarksListView_AfterLabelEdit);
            this._bookmarksListView.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this._bookmarksListView_BeforeLabelEdit);
            this._bookmarksListView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this._bookmarksListView_DrawColumnHeader);
            this._bookmarksListView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this._bookmarksListView_DrawItem);
            this._bookmarksListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this._bookmarksListView_DrawSubItem);
            this._bookmarksListView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this._bookmarksListView_ItemDrag);
            this._bookmarksListView.DragDrop += new System.Windows.Forms.DragEventHandler(this._bookmarks_DragDrop);
            this._bookmarksListView.DragEnter += new System.Windows.Forms.DragEventHandler(this._bookmarks_DragEnter);
            this._bookmarksListView.DragOver += new System.Windows.Forms.DragEventHandler(this._bookmarksListView_DragOver);
            this._bookmarksListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this._bookmarksListView_MouseClick);
            this._bookmarksListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this._bookmarksListView_MouseDoubleClick);
            this._bookmarksListView.MouseDown += new System.Windows.Forms.MouseEventHandler(this._bookmarksListView_MouseDown);
            // 
            // _bookmarkNameColumnHeader
            // 
            this._bookmarkNameColumnHeader.Text = "Name";
            this._bookmarkNameColumnHeader.Width = 200;
            // 
            // _bookmarkUriColumnHeader
            // 
            this._bookmarkUriColumnHeader.Text = "URI";
            this._bookmarkUriColumnHeader.Width = 155;
            // 
            // _bookmarkNotesColumnHeader
            // 
            this._bookmarkNotesColumnHeader.Text = "Notes (double-click to edit)";
            this._bookmarkNotesColumnHeader.Width = 342;
            // 
            // _folderContextMenu
            // 
            this._folderContextMenu.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._folderContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._folderOpenAllMenuItem,
            this._folderOpenAllNewWindowMenuItem,
            this.toolStripSeparator9,
            this._setUsernamePasswordMenuItem,
            this._clearUsernamePasswordToolStripMenuItem,
            this.toolStripSeparator1,
            this._renameFolderMenuItem,
            this.toolStripSeparator2,
            this._cutFolderMenuItem,
            this._copyFolderMenuItem,
            this._pasteFolderMenuItem,
            this.toolStripSeparator3,
            this._deleteFolderMenuItem,
            this.toolStripSeparator4,
            this._addBookmarkMenuItem,
            this._addFolderMenuItem,
            this.toolStripSeparator8,
            this._exportBookMarkMenuitem,
            this._importBookmarkMenuItem});
            this._folderContextMenu.Name = "_folderContextMenu";
            this._folderContextMenu.ShowImageMargin = false;
            this._folderContextMenu.Size = new System.Drawing.Size(270, 326);
            this._folderContextMenu.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this._folderContextMenu_Closed);
            this._folderContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this._folderContextMenu_Opening);
            this._folderContextMenu.Opened += new System.EventHandler(this._folderContextMenu_Opened);
            // 
            // _folderOpenAllMenuItem
            // 
            this._folderOpenAllMenuItem.Name = "_folderOpenAllMenuItem";
            this._folderOpenAllMenuItem.Size = new System.Drawing.Size(269, 22);
            this._folderOpenAllMenuItem.Text = "Open all bookmarks";
            this._folderOpenAllMenuItem.Click += new System.EventHandler(this._folderOpenAllMenuItem_Click);
            // 
            // _folderOpenAllNewWindowMenuItem
            // 
            this._folderOpenAllNewWindowMenuItem.Name = "_folderOpenAllNewWindowMenuItem";
            this._folderOpenAllNewWindowMenuItem.Size = new System.Drawing.Size(269, 22);
            this._folderOpenAllNewWindowMenuItem.Text = "Open all bookmarks in a new window";
            this._folderOpenAllNewWindowMenuItem.Click += new System.EventHandler(this._folderOpenAllNewWindowMenuItem_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(266, 6);
            // 
            // _setUsernamePasswordMenuItem
            // 
            this._setUsernamePasswordMenuItem.Name = "_setUsernamePasswordMenuItem";
            this._setUsernamePasswordMenuItem.Size = new System.Drawing.Size(269, 22);
            this._setUsernamePasswordMenuItem.Text = "Set username and password...";
            this._setUsernamePasswordMenuItem.Click += new System.EventHandler(this._setUsernamePasswordMenuItem_Click);
            // 
            // _clearUsernamePasswordToolStripMenuItem
            // 
            this._clearUsernamePasswordToolStripMenuItem.Name = "_clearUsernamePasswordToolStripMenuItem";
            this._clearUsernamePasswordToolStripMenuItem.Size = new System.Drawing.Size(269, 22);
            this._clearUsernamePasswordToolStripMenuItem.Text = "Clear username and password";
            this._clearUsernamePasswordToolStripMenuItem.Click += new System.EventHandler(this._clearUsernamePasswordToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(266, 6);
            // 
            // _renameFolderMenuItem
            // 
            this._renameFolderMenuItem.Name = "_renameFolderMenuItem";
            this._renameFolderMenuItem.Size = new System.Drawing.Size(269, 22);
            this._renameFolderMenuItem.Text = "Rename...";
            this._renameFolderMenuItem.Click += new System.EventHandler(this._renameFolderMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(266, 6);
            // 
            // _cutFolderMenuItem
            // 
            this._cutFolderMenuItem.Name = "_cutFolderMenuItem";
            this._cutFolderMenuItem.Size = new System.Drawing.Size(269, 22);
            this._cutFolderMenuItem.Text = "Cut";
            this._cutFolderMenuItem.Click += new System.EventHandler(this._cutFolderMenuItem_Click);
            // 
            // _copyFolderMenuItem
            // 
            this._copyFolderMenuItem.Name = "_copyFolderMenuItem";
            this._copyFolderMenuItem.Size = new System.Drawing.Size(269, 22);
            this._copyFolderMenuItem.Text = "Copy";
            this._copyFolderMenuItem.Click += new System.EventHandler(this._copyFolderMenuItem_Click);
            // 
            // _pasteFolderMenuItem
            // 
            this._pasteFolderMenuItem.Enabled = false;
            this._pasteFolderMenuItem.Name = "_pasteFolderMenuItem";
            this._pasteFolderMenuItem.Size = new System.Drawing.Size(269, 22);
            this._pasteFolderMenuItem.Text = "Paste";
            this._pasteFolderMenuItem.Click += new System.EventHandler(this._pasteFolderMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(266, 6);
            // 
            // _deleteFolderMenuItem
            // 
            this._deleteFolderMenuItem.Name = "_deleteFolderMenuItem";
            this._deleteFolderMenuItem.Size = new System.Drawing.Size(269, 22);
            this._deleteFolderMenuItem.Text = "Delete";
            this._deleteFolderMenuItem.Click += new System.EventHandler(this._deleteFolderMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(266, 6);
            // 
            // _addBookmarkMenuItem
            // 
            this._addBookmarkMenuItem.Name = "_addBookmarkMenuItem";
            this._addBookmarkMenuItem.Size = new System.Drawing.Size(269, 22);
            this._addBookmarkMenuItem.Text = "Add bookmark...";
            // 
            // _addFolderMenuItem
            // 
            this._addFolderMenuItem.Name = "_addFolderMenuItem";
            this._addFolderMenuItem.Size = new System.Drawing.Size(269, 22);
            this._addFolderMenuItem.Text = "Add folder...";
            this._addFolderMenuItem.Click += new System.EventHandler(this._addFolderMenuItem_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(266, 6);
            // 
            // _exportBookMarkMenuitem
            // 
            this._exportBookMarkMenuitem.Name = "_exportBookMarkMenuitem";
            this._exportBookMarkMenuitem.Size = new System.Drawing.Size(269, 22);
            this._exportBookMarkMenuitem.Text = "Export";
            this._exportBookMarkMenuitem.Click += new System.EventHandler(this._exportBookMarkMenuitem_Click);
            // 
            // _importBookmarkMenuItem
            // 
            this._importBookmarkMenuItem.Name = "_importBookmarkMenuItem";
            this._importBookmarkMenuItem.Size = new System.Drawing.Size(269, 22);
            this._importBookmarkMenuItem.Text = "Import";
            this._importBookmarkMenuItem.Click += new System.EventHandler(this._importBookmarkMenuItem_Click);
            // 
            // _listViewImageList
            // 
            this._listViewImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this._listViewImageList.ImageSize = new System.Drawing.Size(32, 32);
            this._listViewImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _bookmarksCard
            // 
            this._bookmarksCard.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._bookmarksCard.BackColor = System.Drawing.Color.Transparent;
            this._bookmarksCard.Location = new System.Drawing.Point(24, 24);
            this._bookmarksCard.Name = "_bookmarksCard";
            this._bookmarksCard.Size = new System.Drawing.Size(665, 330);
            this._bookmarksCard.TabIndex = 10;
            // 
            // panelBackground
            // 
            this.panelBackground.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBackground.BackColor = System.Drawing.Color.White;
            this.panelBackground.Location = new System.Drawing.Point(68, 27);
            this.panelBackground.Name = "panelBackground";
            this.panelBackground.Size = new System.Drawing.Size(592, 325);
            this.panelBackground.TabIndex = 9;
            // 
            // _bookmarkContextMenu
            // 
            this._bookmarkContextMenu.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._bookmarkContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._openBookmarkNewTabMenuItem,
            this._openBookmarkNewWindowMenuItem,
            this.toolStripSeparator5,
            this._editBookmarkMenuItem,
            this.renameToolStripMenuItem,
            this.toolStripSeparator6,
            this._cutBookmarkMenuItem,
            this.copyToolStripMenuItem,
            this.toolStripSeparator7,
            this.deleteToolStripMenuItem1});
            this._bookmarkContextMenu.Name = "_bookmarkContextMenu";
            this._bookmarkContextMenu.ShowImageMargin = false;
            this._bookmarkContextMenu.Size = new System.Drawing.Size(184, 176);
            this._bookmarkContextMenu.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this._bookmarkContextMenu_Closed);
            // 
            // _openBookmarkNewTabMenuItem
            // 
            this._openBookmarkNewTabMenuItem.Name = "_openBookmarkNewTabMenuItem";
            this._openBookmarkNewTabMenuItem.Size = new System.Drawing.Size(183, 22);
            this._openBookmarkNewTabMenuItem.Text = "Open in a new tab";
            this._openBookmarkNewTabMenuItem.Click += new System.EventHandler(this._openBookmarkNewTabMenuItem_Click);
            // 
            // _openBookmarkNewWindowMenuItem
            // 
            this._openBookmarkNewWindowMenuItem.Name = "_openBookmarkNewWindowMenuItem";
            this._openBookmarkNewWindowMenuItem.Size = new System.Drawing.Size(183, 22);
            this._openBookmarkNewWindowMenuItem.Text = "Open in a new window";
            this._openBookmarkNewWindowMenuItem.Click += new System.EventHandler(this._openBookmarkNewWindowMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(180, 6);
            // 
            // _editBookmarkMenuItem
            // 
            this._editBookmarkMenuItem.Name = "_editBookmarkMenuItem";
            this._editBookmarkMenuItem.Size = new System.Drawing.Size(183, 22);
            this._editBookmarkMenuItem.Text = "Edit...";
            this._editBookmarkMenuItem.Click += new System.EventHandler(this._editBookmarkMenuItem_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.renameToolStripMenuItem.Text = "Rename...";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(180, 6);
            // 
            // _cutBookmarkMenuItem
            // 
            this._cutBookmarkMenuItem.Name = "_cutBookmarkMenuItem";
            this._cutBookmarkMenuItem.Size = new System.Drawing.Size(183, 22);
            this._cutBookmarkMenuItem.Text = "Cut";
            this._cutBookmarkMenuItem.Click += new System.EventHandler(this._cutBookmarkMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(180, 6);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(183, 22);
            this.deleteToolStripMenuItem1.Text = "Delete";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.deleteToolStripMenuItem1_Click);
            // 
            // _bookmarkExportDialog
            // 
            this._bookmarkExportDialog.DefaultExt = "ecx";
            this._bookmarkExportDialog.FileName = "EasyConnectBookmarks";
            this._bookmarkExportDialog.Filter = "EasyConnect Bookmark files|*.ecx|All files|*.*";
            // 
            // _bookmarkImportDialog
            // 
            this._bookmarkImportDialog.DefaultExt = "ecx";
            this._bookmarkImportDialog.FileName = "EasyConnectBookmarks";
            this._bookmarkImportDialog.Filter = "EasyConnect Bookmark files|*.ecx|All files|*.*";
            // 
            // _dragImageList
            // 
            this._dragImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this._dragImageList.ImageSize = new System.Drawing.Size(16, 16);
            this._dragImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // toolbarBackground
            // 
            this.toolbarBackground.BackColor = System.Drawing.Color.White;
            this.toolbarBackground.Controls.Add(this._urlPanelContainer);
            this.toolbarBackground.Controls.Add(this.urlTextBox);
            this.toolbarBackground.Controls.Add(this.urlBackground);
            this.toolbarBackground.Controls.Add(this._toolsButton);
            this.toolbarBackground.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarBackground.Location = new System.Drawing.Point(0, 0);
            this.toolbarBackground.Name = "toolbarBackground";
            this.toolbarBackground.Size = new System.Drawing.Size(975, 40);
            this.toolbarBackground.TabIndex = 6;
            // 
            // _urlPanelContainer
            // 
            this._urlPanelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._urlPanelContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this._urlPanelContainer.Location = new System.Drawing.Point(41, 9);
            this._urlPanelContainer.Name = "_urlPanelContainer";
            this._urlPanelContainer.Size = new System.Drawing.Size(871, 20);
            this._urlPanelContainer.TabIndex = 0;
            // 
            // urlTextBox
            // 
            this.urlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.urlTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.urlTextBox.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.urlTextBox.Location = new System.Drawing.Point(41, 10);
            this.urlTextBox.Margin = new System.Windows.Forms.Padding(9);
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.Size = new System.Drawing.Size(872, 20);
            this.urlTextBox.TabIndex = 0;
            this.urlTextBox.WordWrap = false;
            // 
            // urlBackground
            // 
            this.urlBackground.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this.urlBackground.Controls.Add(this._iconPictureBox);
            this.urlBackground.Controls.Add(this.urlBackgroundRight);
            this.urlBackground.Controls.Add(this.urlBackgroundLeft);
            this.urlBackground.ForeColor = System.Drawing.Color.Silver;
            this.urlBackground.Location = new System.Drawing.Point(5, 6);
            this.urlBackground.Name = "urlBackground";
            this.urlBackground.Size = new System.Drawing.Size(927, 28);
            this.urlBackground.TabIndex = 2;
            this.urlBackground.Resize += new System.EventHandler(this.urlBackground_Resize);
            // 
            // _iconPictureBox
            // 
            this._iconPictureBox.InitialImage = null;
            this._iconPictureBox.Location = new System.Drawing.Point(11, 6);
            this._iconPictureBox.Name = "_iconPictureBox";
            this._iconPictureBox.Size = new System.Drawing.Size(16, 16);
            this._iconPictureBox.TabIndex = 0;
            this._iconPictureBox.TabStop = false;
            // 
            // urlBackgroundRight
            // 
            this.urlBackgroundRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.urlBackgroundRight.Image = global::EasyConnect.Properties.Resources.UrlBoxRight;
            this.urlBackgroundRight.Location = new System.Drawing.Point(915, 0);
            this.urlBackgroundRight.Name = "urlBackgroundRight";
            this.urlBackgroundRight.Size = new System.Drawing.Size(12, 28);
            this.urlBackgroundRight.TabIndex = 2;
            this.urlBackgroundRight.TabStop = false;
            // 
            // urlBackgroundLeft
            // 
            this.urlBackgroundLeft.Image = global::EasyConnect.Properties.Resources.UrlBoxLeft;
            this.urlBackgroundLeft.Location = new System.Drawing.Point(0, 0);
            this.urlBackgroundLeft.Name = "urlBackgroundLeft";
            this.urlBackgroundLeft.Size = new System.Drawing.Size(12, 28);
            this.urlBackgroundLeft.TabIndex = 1;
            this.urlBackgroundLeft.TabStop = false;
            // 
            // _toolsButton
            // 
            this._toolsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._toolsButton.BackColor = System.Drawing.Color.Transparent;
            this._toolsButton.Image = global::EasyConnect.Properties.Resources.ToolsActive;
            this._toolsButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this._toolsButton.Location = new System.Drawing.Point(941, 6);
            this._toolsButton.Margin = new System.Windows.Forms.Padding(4, 4, 3, 3);
            this._toolsButton.Name = "_toolsButton";
            this._toolsButton.Size = new System.Drawing.Size(28, 28);
            this._toolsButton.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._toolsButton.TabIndex = 5;
            this._toolsButton.TabStop = false;
            this._toolsButton.Click += new System.EventHandler(this._toolsButton_Click);
            this._toolsButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this._toolsButton_MouseDown);
            this._toolsButton.MouseEnter += new System.EventHandler(this._toolsButton_MouseEnter);
            this._toolsButton.MouseLeave += new System.EventHandler(this._toolsButton_MouseLeave);
            // 
            // _toolsMenu
            // 
            this._toolsMenu.BackColor = System.Drawing.Color.White;
            this._toolsMenu.DropShadowEnabled = false;
            this._toolsMenu.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._toolsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newTabMenuItem,
            this._newWindowMenuItem,
            this.toolStripSeparator10,
            this._historyToolStripMenuItem,
            this.toolStripSeparator11,
            this._settingsMenuItem,
            this._toolsMenuSeparator1,
            this._updatesMenuItem,
            this._toolsMenuSeparator2,
            this._aboutMenuItem,
            this._exitMenuItem});
            this._toolsMenu.Name = "_toolsMenu";
            this._toolsMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this._toolsMenu.ShowImageMargin = false;
            this._toolsMenu.Size = new System.Drawing.Size(173, 182);
            this._toolsMenu.VisibleChanged += new System.EventHandler(this._toolsMenu_VisibleChanged);
            // 
            // _newTabMenuItem
            // 
            this._newTabMenuItem.Name = "_newTabMenuItem";
            this._newTabMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this._newTabMenuItem.Size = new System.Drawing.Size(172, 22);
            this._newTabMenuItem.Text = "New tab";
            this._newTabMenuItem.Click += new System.EventHandler(this._newTabMenuItem_Click);
            // 
            // _newWindowMenuItem
            // 
            this._newWindowMenuItem.Name = "_newWindowMenuItem";
            this._newWindowMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this._newWindowMenuItem.Size = new System.Drawing.Size(172, 22);
            this._newWindowMenuItem.Text = "New window";
            this._newWindowMenuItem.Click += new System.EventHandler(this._newWindowMenuItem_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(169, 6);
            // 
            // _historyToolStripMenuItem
            // 
            this._historyToolStripMenuItem.Name = "_historyToolStripMenuItem";
            this._historyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
            this._historyToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this._historyToolStripMenuItem.Text = "History";
            this._historyToolStripMenuItem.Click += new System.EventHandler(this._historyToolStripMenuItem_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(169, 6);
            // 
            // _settingsMenuItem
            // 
            this._settingsMenuItem.Name = "_settingsMenuItem";
            this._settingsMenuItem.Size = new System.Drawing.Size(172, 22);
            this._settingsMenuItem.Text = "Settings";
            this._settingsMenuItem.Click += new System.EventHandler(this._settingsMenuItem_Click);
            // 
            // _toolsMenuSeparator1
            // 
            this._toolsMenuSeparator1.Name = "_toolsMenuSeparator1";
            this._toolsMenuSeparator1.Size = new System.Drawing.Size(169, 6);
            // 
            // _updatesMenuItem
            // 
            this._updatesMenuItem.Name = "_updatesMenuItem";
            this._updatesMenuItem.Size = new System.Drawing.Size(172, 22);
            this._updatesMenuItem.Text = "Check for update";
            this._updatesMenuItem.Visible = false;
            this._updatesMenuItem.Click += new System.EventHandler(this._updatesMenuItem_Click);
            // 
            // _toolsMenuSeparator2
            // 
            this._toolsMenuSeparator2.Name = "_toolsMenuSeparator2";
            this._toolsMenuSeparator2.Size = new System.Drawing.Size(169, 6);
            // 
            // _aboutMenuItem
            // 
            this._aboutMenuItem.Name = "_aboutMenuItem";
            this._aboutMenuItem.Size = new System.Drawing.Size(172, 22);
            this._aboutMenuItem.Text = "About...";
            this._aboutMenuItem.Click += new System.EventHandler(this._aboutMenuItem_Click);
            // 
            // _exitMenuItem
            // 
            this._exitMenuItem.Name = "_exitMenuItem";
            this._exitMenuItem.Size = new System.Drawing.Size(172, 22);
            this._exitMenuItem.Text = "Exit";
            this._exitMenuItem.Click += new System.EventHandler(this._exitMenuItem_Click);
            // 
            // headerPanel
            // 
            this.headerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.headerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(103)))), ((int)(((byte)(214)))));
            this.headerPanel.Controls.Add(this.headerText);
            this.headerPanel.Location = new System.Drawing.Point(0, 40);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new System.Drawing.Size(975, 56);
            this.headerPanel.TabIndex = 7;
            // 
            // headerText
            // 
            this.headerText.AutoSize = true;
            this.headerText.BackColor = System.Drawing.Color.Transparent;
            this.headerText.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerText.ForeColor = System.Drawing.Color.White;
            this.headerText.Location = new System.Drawing.Point(12, 17);
            this.headerText.Name = "headerText";
            this.headerText.Size = new System.Drawing.Size(91, 21);
            this.headerText.TabIndex = 0;
            this.headerText.Text = "Bookmarks";
            // 
            // BookmarksWindow
            // 
            this.BackColor = System.Drawing.Color.Silver;
            this.ClientSize = new System.Drawing.Size(975, 473);
            this.Controls.Add(this.headerPanel);
            this.Controls.Add(this.toolbarBackground);
            this.Controls.Add(this._splitContainer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BookmarksWindow";
            this.Text = "Bookmarks";
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer)).EndInit();
            this._splitContainer.ResumeLayout(false);
            this._folderContextMenu.ResumeLayout(false);
            this._bookmarkContextMenu.ResumeLayout(false);
            this.toolbarBackground.ResumeLayout(false);
            this.toolbarBackground.PerformLayout();
            this.urlBackground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._toolsButton)).EndInit();
            this._toolsMenu.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem renameToolStripMenuItem;
        private SaveFileDialog _bookmarkExportDialog;
        private ToolStripMenuItem _exportBookMarkMenuitem;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripMenuItem _importBookmarkMenuItem;
        private OpenFileDialog _bookmarkImportDialog;
        private ToolStripSeparator toolStripSeparator9;
        private ImageList _dragImageList;
        private ToolStripMenuItem _setUsernamePasswordMenuItem;
        private ToolStripMenuItem _clearUsernamePasswordToolStripMenuItem;
        private ColumnHeader _bookmarkNotesColumnHeader;
        private Panel toolbarBackground;
        private Panel _urlPanelContainer;
        private PictureBox _toolsButton;
        private TextBox urlTextBox;
        private ContextMenuStrip _toolsMenu;
        private ToolStripMenuItem _newTabMenuItem;
        private ToolStripMenuItem _newWindowMenuItem;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripMenuItem _historyToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripMenuItem _updatesMenuItem;
        private ToolStripSeparator _toolsMenuSeparator2;
        private ToolStripMenuItem _aboutMenuItem;
        private ToolStripMenuItem _exitMenuItem;
        private ToolStripMenuItem _settingsMenuItem;
        private ToolStripSeparator _toolsMenuSeparator1;
        private Panel panelBackground;
        private Panel headerPanel;
        private Label headerText;
        private Common.MaterialCard _bookmarksCard;
        private Panel urlBackground;
        private PictureBox _iconPictureBox;
        private PictureBox urlBackgroundLeft;
        private PictureBox urlBackgroundRight;
    }
}
