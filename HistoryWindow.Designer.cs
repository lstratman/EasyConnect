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
            this._toolsMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._newTabMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._newWindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this._historyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this._optionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsMenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._updatesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._toolsMenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this._aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.headerPanel = new System.Windows.Forms.Panel();
            this.headerText = new System.Windows.Forms.Label();
            this.materialCard1 = new EasyConnect.Common.MaterialCard();
            this.toolbarBackground = new System.Windows.Forms.Panel();
            this._urlPanelContainer = new System.Windows.Forms.Panel();
            this.urlTextBox = new System.Windows.Forms.TextBox();
            this.urlBackground = new System.Windows.Forms.Panel();
            this._iconPictureBox = new System.Windows.Forms.PictureBox();
            this.urlBackgroundRight = new System.Windows.Forms.PictureBox();
            this.urlBackgroundLeft = new System.Windows.Forms.PictureBox();
            this._toolsButton = new System.Windows.Forms.PictureBox();
            this.historyContextMenu.SuspendLayout();
            this._toolsMenu.SuspendLayout();
            this.headerPanel.SuspendLayout();
            this.toolbarBackground.SuspendLayout();
            this.urlBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._toolsButton)).BeginInit();
            this.SuspendLayout();
            // 
            // historyImageList
            // 
            this.historyImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.historyImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.historyImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // historyContextMenu
            // 
            this.historyContextMenu.BackColor = System.Drawing.Color.White;
            this.historyContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.propertiesMenuItem,
            this.connectMenuItem});
            this.historyContextMenu.Name = "historyContextMenu";
            this.historyContextMenu.ShowImageMargin = false;
            this.historyContextMenu.Size = new System.Drawing.Size(156, 70);
            // 
            // propertiesMenuItem
            // 
            this.propertiesMenuItem.Name = "propertiesMenuItem";
            this.propertiesMenuItem.Size = new System.Drawing.Size(180, 22);
            this.propertiesMenuItem.Text = "Properties...";
            this.propertiesMenuItem.Click += new System.EventHandler(this.propertiesMenuItem_Click);
            // 
            // connectMenuItem
            // 
            this.connectMenuItem.Name = "connectMenuItem";
            this.connectMenuItem.Size = new System.Drawing.Size(180, 22);
            this.connectMenuItem.Text = "Connect";
            this.connectMenuItem.Click += new System.EventHandler(this.connectMenuItem_Click);
            // 
            // _historyListView
            // 
            this._historyListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._historyListView.BackColor = System.Drawing.Color.White;
            this._historyListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this._historyListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this._timeHeader,
            this._nameHeader,
            this._hostHeader});
            this._historyListView.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._historyListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this._historyListView.HideSelection = false;
            this._historyListView.Location = new System.Drawing.Point(28, 124);
            this._historyListView.Name = "_historyListView";
            this._historyListView.Size = new System.Drawing.Size(706, 330);
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
            this._optionsMenuItem,
            this._toolsMenuSeparator1,
            this._updatesMenuItem,
            this._toolsMenuSeparator2,
            this._aboutMenuItem,
            this._exitMenuItem});
            this._toolsMenu.Name = "_toolsMenu";
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
            // _optionsMenuItem
            // 
            this._optionsMenuItem.Name = "_optionsMenuItem";
            this._optionsMenuItem.Size = new System.Drawing.Size(172, 22);
            this._optionsMenuItem.Text = "Options";
            this._optionsMenuItem.Click += new System.EventHandler(this._optionsMenuItem_Click);
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
            this.headerPanel.Size = new System.Drawing.Size(760, 56);
            this.headerPanel.TabIndex = 8;
            // 
            // headerText
            // 
            this.headerText.AutoSize = true;
            this.headerText.BackColor = System.Drawing.Color.Transparent;
            this.headerText.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.headerText.ForeColor = System.Drawing.Color.White;
            this.headerText.Location = new System.Drawing.Point(12, 17);
            this.headerText.Name = "headerText";
            this.headerText.Size = new System.Drawing.Size(64, 21);
            this.headerText.TabIndex = 0;
            this.headerText.Text = "History";
            // 
            // materialCard1
            // 
            this.materialCard1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialCard1.BackColor = System.Drawing.Color.White;
            this.materialCard1.Location = new System.Drawing.Point(18, 114);
            this.materialCard1.Name = "materialCard1";
            this.materialCard1.Size = new System.Drawing.Size(726, 350);
            this.materialCard1.TabIndex = 9;
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
            this.toolbarBackground.Size = new System.Drawing.Size(760, 40);
            this.toolbarBackground.TabIndex = 10;
            // 
            // _urlPanelContainer
            // 
            this._urlPanelContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._urlPanelContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(243)))), ((int)(((byte)(244)))));
            this._urlPanelContainer.Location = new System.Drawing.Point(41, 9);
            this._urlPanelContainer.Name = "_urlPanelContainer";
            this._urlPanelContainer.Size = new System.Drawing.Size(656, 20);
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
            this.urlTextBox.Size = new System.Drawing.Size(657, 20);
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
            this.urlBackground.Size = new System.Drawing.Size(712, 28);
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
            this.urlBackgroundRight.Location = new System.Drawing.Point(700, 0);
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
            this._toolsButton.Location = new System.Drawing.Point(726, 6);
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
            // HistoryWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(760, 480);
            this.Controls.Add(this.toolbarBackground);
            this.Controls.Add(this._historyListView);
            this.Controls.Add(this.materialCard1);
            this.Controls.Add(this.headerPanel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "HistoryWindow";
            this.Text = "History";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HistoryWindow_FormClosing);
            this.historyContextMenu.ResumeLayout(false);
            this._toolsMenu.ResumeLayout(false);
            this.headerPanel.ResumeLayout(false);
            this.headerPanel.PerformLayout();
            this.toolbarBackground.ResumeLayout(false);
            this.toolbarBackground.PerformLayout();
            this.urlBackground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._iconPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.urlBackgroundLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._toolsButton)).EndInit();
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
        private System.Windows.Forms.ContextMenuStrip _toolsMenu;
        private System.Windows.Forms.ToolStripMenuItem _newTabMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _newWindowMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem _historyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem _optionsMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolsMenuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem _updatesMenuItem;
        private System.Windows.Forms.ToolStripSeparator _toolsMenuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem _aboutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exitMenuItem;
        private System.Windows.Forms.Panel headerPanel;
        private System.Windows.Forms.Label headerText;
        private Common.MaterialCard materialCard1;
        private System.Windows.Forms.Panel toolbarBackground;
        private System.Windows.Forms.Panel _urlPanelContainer;
        private System.Windows.Forms.TextBox urlTextBox;
        private System.Windows.Forms.Panel urlBackground;
        private System.Windows.Forms.PictureBox _iconPictureBox;
        private System.Windows.Forms.PictureBox urlBackgroundRight;
        private System.Windows.Forms.PictureBox urlBackgroundLeft;
        private System.Windows.Forms.PictureBox _toolsButton;
    }
}