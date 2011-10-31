namespace EasyConnect
{
    partial class HistoryWindow
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Today");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Yesterday");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("This Week");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("This Month");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("This Year");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("History", 1, 1, new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5});
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HistoryWindow));
            this.historyTreeView = new System.Windows.Forms.TreeView();
            this.historyImageList = new System.Windows.Forms.ImageList(this.components);
            this.historyContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.propertiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.historyContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // historyTreeView
            // 
            this.historyTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyTreeView.ImageIndex = 0;
            this.historyTreeView.ImageList = this.historyImageList;
            this.historyTreeView.Location = new System.Drawing.Point(0, 0);
            this.historyTreeView.Name = "historyTreeView";
            treeNode1.Name = "Today";
            treeNode1.Text = "Today";
            treeNode2.Name = "Yesterday";
            treeNode2.Text = "Yesterday";
            treeNode3.Name = "ThisWeek";
            treeNode3.Text = "This Week";
            treeNode4.Name = "ThisMonth";
            treeNode4.Text = "This Month";
            treeNode5.Name = "ThisYear";
            treeNode5.Text = "This Year";
            treeNode6.ImageIndex = 1;
            treeNode6.Name = "History";
            treeNode6.SelectedImageIndex = 1;
            treeNode6.Text = "History";
            this.historyTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode6});
            this.historyTreeView.SelectedImageIndex = 0;
            this.historyTreeView.Size = new System.Drawing.Size(284, 262);
            this.historyTreeView.TabIndex = 0;
            this.historyTreeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.historyTreeView_NodeMouseDoubleClick);
            this.historyTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.historyTreeView_NodeMouseClick);
            // 
            // historyImageList
            // 
            this.historyImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("historyImageList.ImageStream")));
            this.historyImageList.TransparentColor = System.Drawing.Color.Transparent;
            this.historyImageList.Images.SetKeyName(0, "Calendar.png");
            this.historyImageList.Images.SetKeyName(1, "History.ico");
            this.historyImageList.Images.SetKeyName(2, "RDCSmall.ico");
            // 
            // historyContextMenu
            // 
            this.historyContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.propertiesMenuItem,
            this.connectMenuItem});
            this.historyContextMenu.Name = "historyContextMenu";
            this.historyContextMenu.Size = new System.Drawing.Size(137, 48);
            // 
            // propertiesMenuItem
            // 
            this.propertiesMenuItem.Image = global::EasyConnect.Properties.Resources.Properties;
            this.propertiesMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
            this.propertiesMenuItem.Name = "propertiesMenuItem";
            this.propertiesMenuItem.Size = new System.Drawing.Size(136, 22);
            this.propertiesMenuItem.Text = "Properties...";
            this.propertiesMenuItem.Click += new System.EventHandler(this.propertiesMenuItem_Click);
            // 
            // connectMenuItem
            // 
            this.connectMenuItem.Image = global::EasyConnect.Properties.Resources.RDCSmall;
            this.connectMenuItem.Name = "connectMenuItem";
            this.connectMenuItem.Size = new System.Drawing.Size(136, 22);
            this.connectMenuItem.Text = "Connect";
            this.connectMenuItem.Click += new System.EventHandler(this.connectMenuItem_Click);
            // 
            // HistoryWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.historyTreeView);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HistoryWindow";
            this.Text = "History";
            this.historyContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView historyTreeView;
        private System.Windows.Forms.ImageList historyImageList;
        private System.Windows.Forms.ContextMenuStrip historyContextMenu;
        private System.Windows.Forms.ToolStripMenuItem propertiesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectMenuItem;
    }
}