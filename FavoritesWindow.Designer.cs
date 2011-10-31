namespace UltraRDC
{
    partial class FavoritesWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Favorites");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FavoritesWindow));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.newFolderButton = new System.Windows.Forms.ToolStripButton();
            this.deleteButton = new System.Windows.Forms.ToolStripButton();
            this.separator = new System.Windows.Forms.ToolStripSeparator();
            this.propertiesButton = new System.Windows.Forms.ToolStripButton();
            this.favoritesTreeView = new System.Windows.Forms.TreeView();
            this.favoritesImageList = new System.Windows.Forms.ImageList(this.components);
            this.dragImageList = new System.Windows.Forms.ImageList(this.components);
            this.favoritesContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.propertiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip.SuspendLayout();
            this.favoritesContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newFolderButton,
            this.deleteButton,
            this.separator,
            this.propertiesButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip.Size = new System.Drawing.Size(284, 25);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip1";
            // 
            // newFolderButton
            // 
            this.newFolderButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newFolderButton.Enabled = false;
            this.newFolderButton.Image = global::UltraRDC.Properties.Resources.Folder;
            this.newFolderButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newFolderButton.Name = "newFolderButton";
            this.newFolderButton.Size = new System.Drawing.Size(23, 22);
            this.newFolderButton.Text = "New Folder";
            this.newFolderButton.Click += new System.EventHandler(this.newFolderButton_Click);
            // 
            // deleteButton
            // 
            this.deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteButton.Enabled = false;
            this.deleteButton.Image = global::UltraRDC.Properties.Resources.Delete;
            this.deleteButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(23, 22);
            this.deleteButton.Text = "Delete";
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // separator
            // 
            this.separator.Name = "separator";
            this.separator.Size = new System.Drawing.Size(6, 25);
            // 
            // propertiesButton
            // 
            this.propertiesButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.propertiesButton.Enabled = false;
            this.propertiesButton.Image = global::UltraRDC.Properties.Resources.Properties;
            this.propertiesButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.propertiesButton.Name = "propertiesButton";
            this.propertiesButton.Size = new System.Drawing.Size(23, 22);
            this.propertiesButton.Text = "Properties";
            this.propertiesButton.Click += new System.EventHandler(this.propertiesButton_Click);
            // 
            // favoritesTreeView
            // 
            this.favoritesTreeView.AllowDrop = true;
            this.favoritesTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.favoritesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.favoritesTreeView.ImageIndex = 0;
            this.favoritesTreeView.ImageList = this.favoritesImageList;
            this.favoritesTreeView.Location = new System.Drawing.Point(0, 25);
            this.favoritesTreeView.Name = "favoritesTreeView";
            treeNode1.Name = "Favorites";
            treeNode1.Text = "Favorites";
            this.favoritesTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.favoritesTreeView.SelectedImageIndex = 0;
            this.favoritesTreeView.Size = new System.Drawing.Size(284, 237);
            this.favoritesTreeView.TabIndex = 1;
            this.favoritesTreeView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.favoritesTreeView_MouseDoubleClick);
            this.favoritesTreeView.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(this.favoritesTreeView_GiveFeedback);
            this.favoritesTreeView.DragLeave += new System.EventHandler(this.favoritesTreeView_DragLeave);
            this.favoritesTreeView.DragDrop += new System.Windows.Forms.DragEventHandler(this.favoritesTreeView_DragDrop);
            this.favoritesTreeView.DragEnter += new System.Windows.Forms.DragEventHandler(this.favoritesTreeView_DragEnter);
            this.favoritesTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.favoritesTreeView_NodeMouseClick);
            this.favoritesTreeView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.favoritesTreeView_ItemDrag);
            this.favoritesTreeView.DragOver += new System.Windows.Forms.DragEventHandler(this.favoritesTreeView_DragOver);
            // 
            // favoritesImageList
            // 
            this.favoritesImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("favoritesImageList.ImageStream")));
            this.favoritesImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.favoritesImageList.Images.SetKeyName(0, "Folder.png");
            this.favoritesImageList.Images.SetKeyName(1, "RDCSmall.ico");
            // 
            // dragImageList
            // 
            this.dragImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.dragImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.dragImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // favoritesContextMenu
            // 
            this.favoritesContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.propertiesToolStripMenuItem,
            this.createFolderToolStripMenuItem,
            this.newConnectionToolStripMenuItem,
            this.renameToolStripMenuItem,
            this.deleteToolStripMenuItem,
            this.connectToolStripMenuItem});
            this.favoritesContextMenu.Name = "favoritesContextMenu";
            this.favoritesContextMenu.Size = new System.Drawing.Size(173, 136);
            // 
            // propertiesToolStripMenuItem
            // 
            this.propertiesToolStripMenuItem.Image = global::UltraRDC.Properties.Resources.Properties;
            this.propertiesToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.propertiesToolStripMenuItem.Name = "propertiesToolStripMenuItem";
            this.propertiesToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.propertiesToolStripMenuItem.Text = "Properties...";
            this.propertiesToolStripMenuItem.Click += new System.EventHandler(this.propertiesToolStripMenuItem_Click);
            // 
            // createFolderToolStripMenuItem
            // 
            this.createFolderToolStripMenuItem.Image = global::UltraRDC.Properties.Resources.Folder;
            this.createFolderToolStripMenuItem.Name = "createFolderToolStripMenuItem";
            this.createFolderToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.createFolderToolStripMenuItem.Text = "New Folder...";
            this.createFolderToolStripMenuItem.Click += new System.EventHandler(this.createFolderToolStripMenuItem_Click);
            // 
            // newConnectionToolStripMenuItem
            // 
            this.newConnectionToolStripMenuItem.Image = global::UltraRDC.Properties.Resources.RDCSmall;
            this.newConnectionToolStripMenuItem.Name = "newConnectionToolStripMenuItem";
            this.newConnectionToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.newConnectionToolStripMenuItem.Text = "New Connection...";
            this.newConnectionToolStripMenuItem.Click += new System.EventHandler(this.newConnectionToolStripMenuItem_Click);
            // 
            // renameToolStripMenuItem
            // 
            this.renameToolStripMenuItem.Name = "renameToolStripMenuItem";
            this.renameToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.renameToolStripMenuItem.Text = "Rename...";
            this.renameToolStripMenuItem.Click += new System.EventHandler(this.renameToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::UltraRDC.Properties.Resources.Delete;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Image = global::UltraRDC.Properties.Resources.RDCSmall;
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.connectToolStripMenuItem.Text = "Connect";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem_Click);
            // 
            // FavoritesWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.favoritesTreeView);
            this.Controls.Add(this.toolStrip);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FavoritesWindow";
            this.Text = "Favorites";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.favoritesContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton deleteButton;
        private System.Windows.Forms.ToolStripButton newFolderButton;
        private System.Windows.Forms.TreeView favoritesTreeView;
        private System.Windows.Forms.ImageList favoritesImageList;
        private System.Windows.Forms.ToolStripSeparator separator;
        private System.Windows.Forms.ToolStripButton propertiesButton;
        private System.Windows.Forms.ImageList dragImageList;
        private System.Windows.Forms.ContextMenuStrip favoritesContextMenu;
        private System.Windows.Forms.ToolStripMenuItem propertiesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newConnectionToolStripMenuItem;
    }
}