namespace EasyConnect
{
    partial class RDCWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RDCWindow));
            this._toolsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._newTabMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._newWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsMenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsMenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._bookmarksMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._bookmarksManagerMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.bookmarkThisSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolbarBackground = new System.Windows.Forms.Panel();
            this._toolsButton = new System.Windows.Forms.PictureBox();
            this._bookmarksButton = new System.Windows.Forms.PictureBox();
            this._closeButton = new System.Windows.Forms.PictureBox();
            this.urlTextBox = new System.Windows.Forms.TextBox();
            this.urlBorder = new System.Windows.Forms.Panel();
            this.urlBackground = new System.Windows.Forms.Panel();
            this._rdcWindow = new AxMSTSCLib.AxMsRdpClient2();
            this._toolsMenu.SuspendLayout();
            this._bookmarksMenu.SuspendLayout();
            this.toolbarBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._toolsButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._bookmarksButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._closeButton)).BeginInit();
            this.urlBorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._rdcWindow)).BeginInit();
            this.SuspendLayout();
            // 
            // _toolsMenu
            // 
            this._toolsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newTabMenuItem,
            this._newWindowMenuItem,
            this._toolsMenuSeparator1,
            this._optionsMenuItem,
            this._toolsMenuSeparator2,
            this._exitMenuItem});
            this._toolsMenu.Name = "_toolsMenu";
            this._toolsMenu.Size = new System.Drawing.Size(187, 104);
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
            // 
            // _toolsMenuSeparator2
            // 
            this._toolsMenuSeparator2.Name = "_toolsMenuSeparator2";
            this._toolsMenuSeparator2.Size = new System.Drawing.Size(183, 6);
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
            this._bookmarksMenu.Size = new System.Drawing.Size(259, 48);
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
            this.bookmarkThisSiteToolStripMenuItem.Text = "Bookmark this site";
            this.bookmarkThisSiteToolStripMenuItem.Click += new System.EventHandler(this._bookmarkMenuItem_Click);
            // 
            // toolbarBackground
            // 
            this.toolbarBackground.BackgroundImage = global::EasyConnect.Properties.Resources.ToolbarBackground;
            this.toolbarBackground.Controls.Add(this._toolsButton);
            this.toolbarBackground.Controls.Add(this._bookmarksButton);
            this.toolbarBackground.Controls.Add(this._closeButton);
            this.toolbarBackground.Controls.Add(this.urlTextBox);
            this.toolbarBackground.Controls.Add(this.urlBorder);
            this.toolbarBackground.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarBackground.Location = new System.Drawing.Point(0, 0);
            this.toolbarBackground.Name = "toolbarBackground";
            this.toolbarBackground.Size = new System.Drawing.Size(622, 36);
            this.toolbarBackground.TabIndex = 5;
            // 
            // _toolsButton
            // 
            this._toolsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._toolsButton.BackColor = System.Drawing.Color.Transparent;
            this._toolsButton.Image = global::EasyConnect.Properties.Resources.ToolsActive;
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
            // _closeButton
            // 
            this._closeButton.BackColor = System.Drawing.Color.Transparent;
            this._closeButton.Image = global::EasyConnect.Properties.Resources.CloseActive;
            this._closeButton.Location = new System.Drawing.Point(6, 5);
            this._closeButton.Margin = new System.Windows.Forms.Padding(4, 4, 3, 3);
            this._closeButton.Name = "_closeButton";
            this._closeButton.Size = new System.Drawing.Size(27, 27);
            this._closeButton.TabIndex = 2;
            this._closeButton.TabStop = false;
            this._closeButton.Click += new System.EventHandler(this._closeButton_Click);
            this._closeButton.MouseEnter += new System.EventHandler(this._closeButton_MouseEnter);
            this._closeButton.MouseLeave += new System.EventHandler(this._closeButton_MouseLeave);
            // 
            // urlTextBox
            // 
            this.urlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.urlTextBox.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.urlTextBox.Location = new System.Drawing.Point(48, 8);
            this.urlTextBox.Margin = new System.Windows.Forms.Padding(9);
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.Size = new System.Drawing.Size(495, 19);
            this.urlTextBox.TabIndex = 0;
            this.urlTextBox.WordWrap = false;
            this.urlTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.urlTextBox_KeyDown);
            // 
            // urlBorder
            // 
            this.urlBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlBorder.BackColor = System.Drawing.Color.Silver;
            this.urlBorder.Controls.Add(this.urlBackground);
            this.urlBorder.ForeColor = System.Drawing.Color.Silver;
            this.urlBorder.Location = new System.Drawing.Point(38, 5);
            this.urlBorder.Name = "urlBorder";
            this.urlBorder.Size = new System.Drawing.Size(515, 26);
            this.urlBorder.TabIndex = 1;
            // 
            // urlBackground
            // 
            this.urlBackground.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlBackground.BackColor = System.Drawing.Color.White;
            this.urlBackground.ForeColor = System.Drawing.Color.Silver;
            this.urlBackground.Location = new System.Drawing.Point(1, 1);
            this.urlBackground.Name = "urlBackground";
            this.urlBackground.Size = new System.Drawing.Size(513, 24);
            this.urlBackground.TabIndex = 2;
            // 
            // _rdcWindow
            // 
            this._rdcWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._rdcWindow.Enabled = true;
            this._rdcWindow.Location = new System.Drawing.Point(-1, 35);
            this._rdcWindow.Name = "_rdcWindow";
            this._rdcWindow.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("_rdcWindow.OcxState")));
            this._rdcWindow.Size = new System.Drawing.Size(624, 401);
            this._rdcWindow.TabIndex = 0;
            // 
            // RDCWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 435);
            this.Controls.Add(this.toolbarBackground);
            this.Controls.Add(this._rdcWindow);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RDCWindow";
            this.Text = "New Tab";
            this._toolsMenu.ResumeLayout(false);
            this._bookmarksMenu.ResumeLayout(false);
            this.toolbarBackground.ResumeLayout(false);
            this.toolbarBackground.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._toolsButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._bookmarksButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._closeButton)).EndInit();
            this.urlBorder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._rdcWindow)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxMSTSCLib.AxMsRdpClient2 _rdcWindow;
        private System.Windows.Forms.Panel toolbarBackground;
        private System.Windows.Forms.PictureBox _closeButton;
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
    }
}