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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HistoryWindow));
			this.historyImageList = new System.Windows.Forms.ImageList(this.components);
			this.historyContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.propertiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.connectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._historyListView = new System.Windows.Forms.ListView();
			this._timeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this._nameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this._hostHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.historyContextMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// historyImageList
			// 
			this.historyImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("historyImageList.ImageStream")));
			this.historyImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.historyImageList.Images.SetKeyName(0, "RDCSmall.ico");
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
			// _historyListView
			// 
			this._historyListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._historyListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._historyListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._timeHeader,
            this._nameHeader,
            this._hostHeader});
			this._historyListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this._historyListView.Location = new System.Drawing.Point(0, 0);
			this._historyListView.Name = "_historyListView";
			this._historyListView.Size = new System.Drawing.Size(764, 483);
			this._historyListView.SmallImageList = this.historyImageList;
			this._historyListView.TabIndex = 1;
			this._historyListView.UseCompatibleStateImageBehavior = false;
			this._historyListView.View = System.Windows.Forms.View.Details;
			this._historyListView.DoubleClick += new System.EventHandler(this._historyListView_DoubleClick);
			this._historyListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this._historyListView_MouseClick);
			// 
			// _timeHeader
			// 
			this._timeHeader.Text = "Time";
			// 
			// _nameHeader
			// 
			this._nameHeader.Text = "Name";
			// 
			// _hostHeader
			// 
			this._hostHeader.Text = "Host";
			// 
			// HistoryWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(760, 480);
			this.Controls.Add(this._historyListView);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "HistoryWindow";
			this.Text = "History";
			this.historyContextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList historyImageList;
        private System.Windows.Forms.ContextMenuStrip historyContextMenu;
        private System.Windows.Forms.ToolStripMenuItem propertiesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectMenuItem;
        private System.Windows.Forms.ListView _historyListView;
        private System.Windows.Forms.ColumnHeader _timeHeader;
        private System.Windows.Forms.ColumnHeader _nameHeader;
        private System.Windows.Forms.ColumnHeader _hostHeader;
    }
}