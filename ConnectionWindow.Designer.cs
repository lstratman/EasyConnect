namespace EasyConnect
{
    partial class ConnectionWindow
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionWindow));
			this._toolsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this._newTabMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._newWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this._historyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._toolsMenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this._optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this._updatesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._toolsMenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this._aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._bookmarksMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this._bookmarksManagerMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
			this.bookmarkThisSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolbarBackground = new System.Windows.Forms.Panel();
			this._urlPanelContainer = new System.Windows.Forms.Panel();
			this._toolsButton = new System.Windows.Forms.PictureBox();
			this._bookmarksButton = new System.Windows.Forms.PictureBox();
			this.urlTextBox = new System.Windows.Forms.TextBox();
			this.urlBorder = new System.Windows.Forms.Panel();
			this.urlBackground = new System.Windows.Forms.Panel();
			this._iconPictureBox = new System.Windows.Forms.PictureBox();
			this._connectionContainerPanel = new System.Windows.Forms.Panel();
			this._omniBarPanel = new System.Windows.Forms.Panel();
			this._omniBarBorder = new System.Windows.Forms.Panel();
			this._toolsMenu.SuspendLayout();
			this._bookmarksMenu.SuspendLayout();
			this.toolbarBackground.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._toolsButton)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._bookmarksButton)).BeginInit();
			this.urlBorder.SuspendLayout();
			this.urlBackground.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// _toolsMenu
			// 
			this._toolsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newTabMenuItem,
            this._newWindowMenuItem,
            this.toolStripSeparator1,
            this._historyToolStripMenuItem,
            this._toolsMenuSeparator1,
            this._optionsMenuItem,
            this.toolStripSeparator2,
            this._updatesMenuItem,
            this._toolsMenuSeparator2,
            this._aboutMenuItem,
            this._exitMenuItem});
			this._toolsMenu.Name = "_toolsMenu";
			this._toolsMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this._toolsMenu.Size = new System.Drawing.Size(187, 182);
			this._toolsMenu.VisibleChanged += new System.EventHandler(this._toolsMenu_VisibleChanged);
			// 
			// _newTabMenuItem
			// 
			this._newTabMenuItem.Name = "_newTabMenuItem";
			this._newTabMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
			this._newTabMenuItem.Size = new System.Drawing.Size(186, 22);
			this._newTabMenuItem.Text = "New tab";
			this._newTabMenuItem.Click += new System.EventHandler(this._newTabMenuItem_Click);
			// 
			// _newWindowMenuItem
			// 
			this._newWindowMenuItem.Name = "_newWindowMenuItem";
			this._newWindowMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this._newWindowMenuItem.Size = new System.Drawing.Size(186, 22);
			this._newWindowMenuItem.Text = "New window";
			this._newWindowMenuItem.Click += new System.EventHandler(this._newWindowMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(183, 6);
			// 
			// _historyToolStripMenuItem
			// 
			this._historyToolStripMenuItem.Name = "_historyToolStripMenuItem";
			this._historyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.H)));
			this._historyToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this._historyToolStripMenuItem.Text = "History";
			this._historyToolStripMenuItem.Click += new System.EventHandler(this._historyToolStripMenuItem_Click);
			// 
			// _toolsMenuSeparator1
			// 
			this._toolsMenuSeparator1.Name = "_toolsMenuSeparator1";
			this._toolsMenuSeparator1.Size = new System.Drawing.Size(183, 6);
			// 
			// _optionsMenuItem
			// 
			this._optionsMenuItem.Name = "_optionsMenuItem";
			this._optionsMenuItem.Size = new System.Drawing.Size(186, 22);
			this._optionsMenuItem.Text = "Options";
			this._optionsMenuItem.Click += new System.EventHandler(this._optionsMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(183, 6);
			// 
			// _updatesMenuItem
			// 
			this._updatesMenuItem.Name = "_updatesMenuItem";
			this._updatesMenuItem.Size = new System.Drawing.Size(186, 22);
			this._updatesMenuItem.Text = "Check for update";
	        this._updatesMenuItem.Visible = false;
			this._updatesMenuItem.Click += new System.EventHandler(this._updatesMenuItem_Click);
			// 
			// _toolsMenuSeparator2
			// 
			this._toolsMenuSeparator2.Name = "_toolsMenuSeparator2";
			this._toolsMenuSeparator2.Size = new System.Drawing.Size(183, 6);
			this._toolsMenuSeparator2.Visible = false;
			// 
			// _aboutMenuItem
			// 
			this._aboutMenuItem.Name = "_aboutMenuItem";
			this._aboutMenuItem.Size = new System.Drawing.Size(186, 22);
			this._aboutMenuItem.Text = "About...";
			this._aboutMenuItem.Click += new System.EventHandler(this._aboutMenuItem_Click);
			// 
			// _exitMenuItem
			// 
			this._exitMenuItem.Name = "_exitMenuItem";
			this._exitMenuItem.Size = new System.Drawing.Size(186, 22);
			this._exitMenuItem.Text = "Exit";
			this._exitMenuItem.Click += new System.EventHandler(this._exitMenuItem_Click);
			// 
			// _bookmarksMenu
			// 
			this._bookmarksMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._bookmarksManagerMenuItem2,
            this.bookmarkThisSiteToolStripMenuItem});
			this._bookmarksMenu.Name = "_bookmarksMenu";
			this._bookmarksMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this._bookmarksMenu.Size = new System.Drawing.Size(259, 48);
			this._bookmarksMenu.VisibleChanged += new System.EventHandler(this._bookmarksMenu_VisibleChanged);
			// 
			// _bookmarksManagerMenuItem2
			// 
			this._bookmarksManagerMenuItem2.Name = "_bookmarksManagerMenuItem2";
			this._bookmarksManagerMenuItem2.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
			this._bookmarksManagerMenuItem2.Size = new System.Drawing.Size(258, 22);
			this._bookmarksManagerMenuItem2.Text = "Bookmarks manager";
			this._bookmarksManagerMenuItem2.Click += new System.EventHandler(this._bookmarksManagerMenuItem2_Click);
			// 
			// bookmarkThisSiteToolStripMenuItem
			// 
			this.bookmarkThisSiteToolStripMenuItem.Name = "bookmarkThisSiteToolStripMenuItem";
			this.bookmarkThisSiteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
			this.bookmarkThisSiteToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
			this.bookmarkThisSiteToolStripMenuItem.Text = "Bookmark this server";
			this.bookmarkThisSiteToolStripMenuItem.Click += new System.EventHandler(this._bookmarkMenuItem_Click);
			// 
			// toolbarBackground
			// 
			this.toolbarBackground.BackgroundImage = global::EasyConnect.Properties.Resources.ToolbarBackground;
			this.toolbarBackground.Controls.Add(this._urlPanelContainer);
			this.toolbarBackground.Controls.Add(this._toolsButton);
			this.toolbarBackground.Controls.Add(this._bookmarksButton);
			this.toolbarBackground.Controls.Add(this.urlTextBox);
			this.toolbarBackground.Controls.Add(this.urlBorder);
			this.toolbarBackground.Dock = System.Windows.Forms.DockStyle.Top;
			this.toolbarBackground.Location = new System.Drawing.Point(0, 0);
			this.toolbarBackground.Name = "toolbarBackground";
			this.toolbarBackground.Size = new System.Drawing.Size(622, 36);
			this.toolbarBackground.TabIndex = 5;
			this.toolbarBackground.Click += new System.EventHandler(this.toolbarBackground_Click);
			// 
			// _urlPanelContainer
			// 
			this._urlPanelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._urlPanelContainer.BackColor = System.Drawing.Color.White;
			this._urlPanelContainer.Location = new System.Drawing.Point(34, 8);
			this._urlPanelContainer.Name = "_urlPanelContainer";
			this._urlPanelContainer.Size = new System.Drawing.Size(509, 19);
			this._urlPanelContainer.TabIndex = 0;
			// 
			// _toolsButton
			// 
			this._toolsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._toolsButton.BackColor = System.Drawing.Color.Transparent;
			this._toolsButton.Image = global::EasyConnect.Properties.Resources.ToolsActive;
			this._toolsButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._toolsButton.Location = new System.Drawing.Point(589, 5);
			this._toolsButton.Margin = new System.Windows.Forms.Padding(4, 4, 3, 3);
			this._toolsButton.Name = "_toolsButton";
			this._toolsButton.Size = new System.Drawing.Size(27, 27);
			this._toolsButton.TabIndex = 5;
			this._toolsButton.TabStop = false;
			this._toolsButton.Click += new System.EventHandler(this._toolsButton_Click);
			this._toolsButton.MouseEnter += new System.EventHandler(this._toolsButton_MouseEnter);
			this._toolsButton.MouseLeave += new System.EventHandler(this._toolsButton_MouseLeave);
			// 
			// _bookmarksButton
			// 
			this._bookmarksButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._bookmarksButton.BackColor = System.Drawing.Color.Transparent;
			this._bookmarksButton.Image = global::EasyConnect.Properties.Resources.BookmarksActive;
			this._bookmarksButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this._bookmarksButton.Location = new System.Drawing.Point(558, 5);
			this._bookmarksButton.Margin = new System.Windows.Forms.Padding(4, 4, 3, 3);
			this._bookmarksButton.Name = "_bookmarksButton";
			this._bookmarksButton.Size = new System.Drawing.Size(27, 27);
			this._bookmarksButton.TabIndex = 4;
			this._bookmarksButton.TabStop = false;
			this._bookmarksButton.Click += new System.EventHandler(this._bookmarksButton_Click);
			this._bookmarksButton.MouseEnter += new System.EventHandler(this._bookmarksButton_MouseEnter);
			this._bookmarksButton.MouseLeave += new System.EventHandler(this._bookmarksButton_MouseLeave);
			// 
			// urlTextBox
			// 
			this.urlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.urlTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.urlTextBox.Font = new System.Drawing.Font("Tahoma", 11.25F);
			this.urlTextBox.Location = new System.Drawing.Point(34, 8);
			this.urlTextBox.Margin = new System.Windows.Forms.Padding(9);
			this.urlTextBox.Name = "urlTextBox";
			this.urlTextBox.Size = new System.Drawing.Size(509, 19);
			this.urlTextBox.TabIndex = 0;
			this.urlTextBox.WordWrap = false;
			this.urlTextBox.TextChanged += new System.EventHandler(this.urlTextBox_TextChanged);
			this.urlTextBox.Enter += new System.EventHandler(this.urlTextBox_Enter);
			this.urlTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.urlTextBox_KeyDown);
			this.urlTextBox.Leave += new System.EventHandler(this.urlTextBox_Leave);
			// 
			// urlBorder
			// 
			this.urlBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.urlBorder.BackColor = System.Drawing.Color.Silver;
			this.urlBorder.Controls.Add(this.urlBackground);
			this.urlBorder.ForeColor = System.Drawing.Color.Silver;
			this.urlBorder.Location = new System.Drawing.Point(5, 5);
			this.urlBorder.Name = "urlBorder";
			this.urlBorder.Size = new System.Drawing.Size(548, 26);
			this.urlBorder.TabIndex = 1;
			// 
			// urlBackground
			// 
			this.urlBackground.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.urlBackground.BackColor = System.Drawing.Color.White;
			this.urlBackground.Controls.Add(this._iconPictureBox);
			this.urlBackground.ForeColor = System.Drawing.Color.Silver;
			this.urlBackground.Location = new System.Drawing.Point(1, 1);
			this.urlBackground.Name = "urlBackground";
			this.urlBackground.Size = new System.Drawing.Size(546, 24);
			this.urlBackground.TabIndex = 2;
			// 
			// _iconPictureBox
			// 
			this._iconPictureBox.Location = new System.Drawing.Point(4, 4);
			this._iconPictureBox.Name = "_iconPictureBox";
			this._iconPictureBox.Size = new System.Drawing.Size(16, 16);
			this._iconPictureBox.TabIndex = 0;
			this._iconPictureBox.TabStop = false;
			// 
			// _connectionContainerPanel
			// 
			this._connectionContainerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._connectionContainerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this._connectionContainerPanel.Location = new System.Drawing.Point(0, 36);
			this._connectionContainerPanel.Name = "_connectionContainerPanel";
			this._connectionContainerPanel.Size = new System.Drawing.Size(622, 399);
			this._connectionContainerPanel.TabIndex = 6;
			// 
			// _omniBarPanel
			// 
			this._omniBarPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._omniBarPanel.BackColor = System.Drawing.Color.White;
			this._omniBarPanel.ForeColor = System.Drawing.Color.Silver;
			this._omniBarPanel.Location = new System.Drawing.Point(6, 30);
			this._omniBarPanel.Name = "_omniBarPanel";
			this._omniBarPanel.Size = new System.Drawing.Size(546, 72);
			this._omniBarPanel.TabIndex = 7;
			this._omniBarPanel.Visible = false;
			// 
			// _omniBarBorder
			// 
			this._omniBarBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._omniBarBorder.BackColor = System.Drawing.Color.Silver;
			this._omniBarBorder.ForeColor = System.Drawing.Color.Silver;
			this._omniBarBorder.Location = new System.Drawing.Point(5, 29);
			this._omniBarBorder.Name = "_omniBarBorder";
			this._omniBarBorder.Size = new System.Drawing.Size(548, 74);
			this._omniBarBorder.TabIndex = 8;
			this._omniBarBorder.Visible = false;
			// 
			// ConnectionWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(622, 435);
			this.Controls.Add(this._omniBarPanel);
			this.Controls.Add(this._omniBarBorder);
			this.Controls.Add(this.toolbarBackground);
			this.Controls.Add(this._connectionContainerPanel);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ConnectionWindow";
			this.Text = "New Tab";
			this.Shown += new System.EventHandler(this.ConnectionWindow_Shown);
			this._toolsMenu.ResumeLayout(false);
			this._bookmarksMenu.ResumeLayout(false);
			this.toolbarBackground.ResumeLayout(false);
			this.toolbarBackground.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._toolsButton)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._bookmarksButton)).EndInit();
			this.urlBorder.ResumeLayout(false);
			this.urlBackground.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel toolbarBackground;
        private System.Windows.Forms.TextBox urlTextBox;
        private System.Windows.Forms.Panel urlBorder;
        private System.Windows.Forms.Panel urlBackground;
        private System.Windows.Forms.PictureBox _toolsButton;
        private System.Windows.Forms.PictureBox _bookmarksButton;
        private System.Windows.Forms.ContextMenuStrip _toolsMenu;
        private System.Windows.Forms.ToolStripMenuItem _newTabMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _newWindowMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolsMenuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _optionsMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolsMenuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _exitMenuItem;
        private System.Windows.Forms.ContextMenuStrip _bookmarksMenu;
        private System.Windows.Forms.ToolStripMenuItem _bookmarksManagerMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem bookmarkThisSiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _historyToolStripMenuItem;
        private System.Windows.Forms.Panel _connectionContainerPanel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _updatesMenuItem;
        private System.Windows.Forms.Panel _omniBarPanel;
        private System.Windows.Forms.Panel _omniBarBorder;
		private System.Windows.Forms.ToolStripMenuItem _aboutMenuItem;
		private System.Windows.Forms.PictureBox _iconPictureBox;
		private System.Windows.Forms.Panel _urlPanelContainer;
    }
}