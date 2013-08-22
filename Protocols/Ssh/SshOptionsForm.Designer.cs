namespace EasyConnect.Protocols.Ssh
{
    partial class SshOptionsForm
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
			System.Security.SecureString secureString1 = new System.Security.SecureString();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SshOptionsForm));
			this._titleLabel = new System.Windows.Forms.Label();
			this._fontDialog = new System.Windows.Forms.FontDialog();
			this._colorDialog = new System.Windows.Forms.ColorDialog();
			this._clearScreenKeysLabel = new System.Windows.Forms.Label();
			this._clearScreenLabel = new System.Windows.Forms.Label();
			this._pasteKeysLabel = new System.Windows.Forms.Label();
			this._copyKeysLabel = new System.Windows.Forms.Label();
			this._pasteLabel = new System.Windows.Forms.Label();
			this._shortcutsLabel = new System.Windows.Forms.Label();
			this._copyLabel = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel1 = new System.Windows.Forms.Panel();
			this._fontBrowseButton = new System.Windows.Forms.Button();
			this._fontTextBox = new System.Windows.Forms.TextBox();
			this._fontLabel = new System.Windows.Forms.Label();
			this._flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
			this._hostPanel = new System.Windows.Forms.Panel();
			this._hostNameLabel = new System.Windows.Forms.Label();
			this._hostNameTextBox = new System.Windows.Forms.TextBox();
			this._hostLabel = new System.Windows.Forms.Label();
			this._generalPanel = new System.Windows.Forms.Panel();
			this._identityInfoLink = new System.Windows.Forms.LinkLabel();
			this._identityFileBrowseButton = new System.Windows.Forms.Button();
			this._identityFileTextbox = new System.Windows.Forms.TextBox();
			this._identityFileLabel = new System.Windows.Forms.Label();
			this._passwordTextBox = new SecurePasswordTextBox.SecureTextBox();
			this._inheritedPasswordLabel = new System.Windows.Forms.Label();
			this._inheritedUsernameLabel = new System.Windows.Forms.Label();
			this._userNameLabel = new System.Windows.Forms.Label();
			this._userNameTextBox = new System.Windows.Forms.TextBox();
			this._passwordLabel = new System.Windows.Forms.Label();
			this._generalLabel = new System.Windows.Forms.Label();
			this._displayPanel = new System.Windows.Forms.Panel();
			this._textColorPanel = new System.Windows.Forms.Panel();
			this._textColorLabel = new System.Windows.Forms.Label();
			this._backgroundColorPanel = new System.Windows.Forms.Panel();
			this._displayLabel = new System.Windows.Forms.Label();
			this._backgroundColorLabel = new System.Windows.Forms.Label();
			this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
			this._identityInfoTooltip = new System.Windows.Forms.ToolTip(this.components);
			this.panel2.SuspendLayout();
			this._flowLayoutPanel.SuspendLayout();
			this._hostPanel.SuspendLayout();
			this._generalPanel.SuspendLayout();
			this._displayPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// _titleLabel
			// 
			this._titleLabel.AutoSize = true;
			this._titleLabel.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(97)))), ((int)(((byte)(102)))));
			this._titleLabel.Location = new System.Drawing.Point(24, 19);
			this._titleLabel.Name = "_titleLabel";
			this._titleLabel.Size = new System.Drawing.Size(204, 30);
			this._titleLabel.TabIndex = 88;
			this._titleLabel.Text = "Secure Shell Options";
			// 
			// _fontDialog
			// 
			this._fontDialog.Color = System.Drawing.SystemColors.ControlText;
			this._fontDialog.FixedPitchOnly = true;
			// 
			// _clearScreenKeysLabel
			// 
			this._clearScreenKeysLabel.AutoSize = true;
			this._clearScreenKeysLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
			this._clearScreenKeysLabel.Location = new System.Drawing.Point(113, 92);
			this._clearScreenKeysLabel.Name = "_clearScreenKeysLabel";
			this._clearScreenKeysLabel.Size = new System.Drawing.Size(36, 13);
			this._clearScreenKeysLabel.TabIndex = 92;
			this._clearScreenKeysLabel.Text = "Alt+L";
			// 
			// _clearScreenLabel
			// 
			this._clearScreenLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._clearScreenLabel.Location = new System.Drawing.Point(31, 88);
			this._clearScreenLabel.Name = "_clearScreenLabel";
			this._clearScreenLabel.Size = new System.Drawing.Size(150, 20);
			this._clearScreenLabel.TabIndex = 91;
			this._clearScreenLabel.Text = "Clear screen:";
			this._clearScreenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _pasteKeysLabel
			// 
			this._pasteKeysLabel.AutoSize = true;
			this._pasteKeysLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
			this._pasteKeysLabel.Location = new System.Drawing.Point(112, 67);
			this._pasteKeysLabel.Name = "_pasteKeysLabel";
			this._pasteKeysLabel.Size = new System.Drawing.Size(37, 13);
			this._pasteKeysLabel.TabIndex = 90;
			this._pasteKeysLabel.Text = "Alt+V";
			// 
			// _copyKeysLabel
			// 
			this._copyKeysLabel.AutoSize = true;
			this._copyKeysLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold);
			this._copyKeysLabel.Location = new System.Drawing.Point(112, 42);
			this._copyKeysLabel.Name = "_copyKeysLabel";
			this._copyKeysLabel.Size = new System.Drawing.Size(37, 13);
			this._copyKeysLabel.TabIndex = 89;
			this._copyKeysLabel.Text = "Alt+C";
			// 
			// _pasteLabel
			// 
			this._pasteLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._pasteLabel.Location = new System.Drawing.Point(31, 63);
			this._pasteLabel.Name = "_pasteLabel";
			this._pasteLabel.Size = new System.Drawing.Size(150, 20);
			this._pasteLabel.TabIndex = 82;
			this._pasteLabel.Text = "Paste:";
			this._pasteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _shortcutsLabel
			// 
			this._shortcutsLabel.AutoSize = true;
			this._shortcutsLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this._shortcutsLabel.Location = new System.Drawing.Point(8, 11);
			this._shortcutsLabel.Name = "_shortcutsLabel";
			this._shortcutsLabel.Size = new System.Drawing.Size(87, 17);
			this._shortcutsLabel.TabIndex = 55;
			this._shortcutsLabel.Text = "Shortcut Keys";
			// 
			// _copyLabel
			// 
			this._copyLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._copyLabel.Location = new System.Drawing.Point(31, 38);
			this._copyLabel.Name = "_copyLabel";
			this._copyLabel.Size = new System.Drawing.Size(150, 20);
			this._copyLabel.TabIndex = 54;
			this._copyLabel.Text = "Copy:";
			this._copyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this._clearScreenKeysLabel);
			this.panel2.Controls.Add(this._clearScreenLabel);
			this.panel2.Controls.Add(this._pasteKeysLabel);
			this.panel2.Controls.Add(this._copyKeysLabel);
			this.panel2.Controls.Add(this._pasteLabel);
			this.panel2.Controls.Add(this._shortcutsLabel);
			this.panel2.Controls.Add(this._copyLabel);
			this.panel2.Location = new System.Drawing.Point(18, 325);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(684, 115);
			this.panel2.TabIndex = 88;
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BackColor = System.Drawing.Color.Silver;
			this.panel1.Location = new System.Drawing.Point(29, 62);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(667, 1);
			this.panel1.TabIndex = 89;
			// 
			// _fontBrowseButton
			// 
			this._fontBrowseButton.Location = new System.Drawing.Point(307, 85);
			this._fontBrowseButton.Name = "_fontBrowseButton";
			this._fontBrowseButton.Size = new System.Drawing.Size(26, 22);
			this._fontBrowseButton.TabIndex = 88;
			this._fontBrowseButton.Text = "...";
			this._fontBrowseButton.UseVisualStyleBackColor = true;
			this._fontBrowseButton.Click += new System.EventHandler(this._fontBrowseButton_Click);
			// 
			// _fontTextBox
			// 
			this._fontTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this._fontTextBox.Location = new System.Drawing.Point(148, 86);
			this._fontTextBox.Name = "_fontTextBox";
			this._fontTextBox.ReadOnly = true;
			this._fontTextBox.Size = new System.Drawing.Size(154, 20);
			this._fontTextBox.TabIndex = 85;
			// 
			// _fontLabel
			// 
			this._fontLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._fontLabel.Location = new System.Drawing.Point(31, 86);
			this._fontLabel.Name = "_fontLabel";
			this._fontLabel.Size = new System.Drawing.Size(92, 20);
			this._fontLabel.TabIndex = 84;
			this._fontLabel.Text = "Font:";
			this._fontLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _flowLayoutPanel
			// 
			this._flowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._flowLayoutPanel.Controls.Add(this._hostPanel);
			this._flowLayoutPanel.Controls.Add(this._generalPanel);
			this._flowLayoutPanel.Controls.Add(this._displayPanel);
			this._flowLayoutPanel.Controls.Add(this.panel2);
			this._flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this._flowLayoutPanel.Location = new System.Drawing.Point(0, 61);
			this._flowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this._flowLayoutPanel.Name = "_flowLayoutPanel";
			this._flowLayoutPanel.Padding = new System.Windows.Forms.Padding(15, 0, 15, 15);
			this._flowLayoutPanel.Size = new System.Drawing.Size(721, 464);
			this._flowLayoutPanel.TabIndex = 87;
			this._flowLayoutPanel.WrapContents = false;
			this._flowLayoutPanel.Resize += new System.EventHandler(this._flowLayoutPanel_Resize);
			// 
			// _hostPanel
			// 
			this._hostPanel.Controls.Add(this._hostNameLabel);
			this._hostPanel.Controls.Add(this._hostNameTextBox);
			this._hostPanel.Controls.Add(this._hostLabel);
			this._hostPanel.Location = new System.Drawing.Point(18, 3);
			this._hostPanel.Name = "_hostPanel";
			this._hostPanel.Size = new System.Drawing.Size(684, 68);
			this._hostPanel.TabIndex = 84;
			// 
			// _hostNameLabel
			// 
			this._hostNameLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._hostNameLabel.Location = new System.Drawing.Point(31, 38);
			this._hostNameLabel.Name = "_hostNameLabel";
			this._hostNameLabel.Size = new System.Drawing.Size(71, 20);
			this._hostNameLabel.TabIndex = 53;
			this._hostNameLabel.Text = "Host name:";
			this._hostNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _hostNameTextBox
			// 
			this._hostNameTextBox.Location = new System.Drawing.Point(108, 39);
			this._hostNameTextBox.Name = "_hostNameTextBox";
			this._hostNameTextBox.Size = new System.Drawing.Size(154, 20);
			this._hostNameTextBox.TabIndex = 52;
			// 
			// _hostLabel
			// 
			this._hostLabel.AutoSize = true;
			this._hostLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._hostLabel.Location = new System.Drawing.Point(8, 11);
			this._hostLabel.Name = "_hostLabel";
			this._hostLabel.Size = new System.Drawing.Size(35, 17);
			this._hostLabel.TabIndex = 54;
			this._hostLabel.Text = "Host";
			// 
			// _generalPanel
			// 
			this._generalPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._generalPanel.Controls.Add(this._identityInfoLink);
			this._generalPanel.Controls.Add(this._identityFileBrowseButton);
			this._generalPanel.Controls.Add(this._identityFileTextbox);
			this._generalPanel.Controls.Add(this._identityFileLabel);
			this._generalPanel.Controls.Add(this._passwordTextBox);
			this._generalPanel.Controls.Add(this._inheritedPasswordLabel);
			this._generalPanel.Controls.Add(this._inheritedUsernameLabel);
			this._generalPanel.Controls.Add(this._userNameLabel);
			this._generalPanel.Controls.Add(this._userNameTextBox);
			this._generalPanel.Controls.Add(this._passwordLabel);
			this._generalPanel.Controls.Add(this._generalLabel);
			this._generalPanel.Location = new System.Drawing.Point(18, 77);
			this._generalPanel.Name = "_generalPanel";
			this._generalPanel.Size = new System.Drawing.Size(684, 120);
			this._generalPanel.TabIndex = 83;
			// 
			// _identityInfoLink
			// 
			this._identityInfoLink.AutoSize = true;
			this._identityInfoLink.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._identityInfoLink.Location = new System.Drawing.Point(296, 92);
			this._identityInfoLink.Name = "_identityInfoLink";
			this._identityInfoLink.Size = new System.Drawing.Size(12, 13);
			this._identityInfoLink.TabIndex = 97;
			this._identityInfoLink.TabStop = true;
			this._identityInfoLink.Text = "?";
			this._identityInfoTooltip.SetToolTip(this._identityInfoLink, "Must be an ssh.com-formatted \r\n(---- BEGIN SSH2 ENCRYPTED \r\nPRIVATE KEY ----) pri" +
        "vate key");
			// 
			// _identityFileBrowseButton
			// 
			this._identityFileBrowseButton.Location = new System.Drawing.Point(267, 88);
			this._identityFileBrowseButton.Name = "_identityFileBrowseButton";
			this._identityFileBrowseButton.Size = new System.Drawing.Size(26, 22);
			this._identityFileBrowseButton.TabIndex = 96;
			this._identityFileBrowseButton.Text = "...";
			this._identityFileBrowseButton.UseVisualStyleBackColor = true;
			this._identityFileBrowseButton.Click += new System.EventHandler(this._identityFileBrowseButton_Click);
			// 
			// _identityFileTextbox
			// 
			this._identityFileTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this._identityFileTextbox.Location = new System.Drawing.Point(108, 89);
			this._identityFileTextbox.Name = "_identityFileTextbox";
			this._identityFileTextbox.ReadOnly = true;
			this._identityFileTextbox.Size = new System.Drawing.Size(154, 20);
			this._identityFileTextbox.TabIndex = 95;
			// 
			// _identityFileLabel
			// 
			this._identityFileLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._identityFileLabel.Location = new System.Drawing.Point(31, 88);
			this._identityFileLabel.Name = "_identityFileLabel";
			this._identityFileLabel.Size = new System.Drawing.Size(92, 20);
			this._identityFileLabel.TabIndex = 94;
			this._identityFileLabel.Text = "Identity file:";
			this._identityFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _passwordTextBox
			// 
			this._passwordTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
			this._passwordTextBox.Location = new System.Drawing.Point(108, 64);
			this._passwordTextBox.Name = "_passwordTextBox";
			this._passwordTextBox.PasswordChar = '*';
			this._passwordTextBox.SecureText = secureString1;
			this._passwordTextBox.Size = new System.Drawing.Size(154, 20);
			this._passwordTextBox.TabIndex = 93;
			this._passwordTextBox.TextChanged += new System.EventHandler(this._passwordTextBox_TextChanged);
			// 
			// _inheritedPasswordLabel
			// 
			this._inheritedPasswordLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._inheritedPasswordLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._inheritedPasswordLabel.Location = new System.Drawing.Point(268, 67);
			this._inheritedPasswordLabel.Name = "_inheritedPasswordLabel";
			this._inheritedPasswordLabel.Size = new System.Drawing.Size(263, 16);
			this._inheritedPasswordLabel.TabIndex = 92;
			// 
			// _inheritedUsernameLabel
			// 
			this._inheritedUsernameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._inheritedUsernameLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._inheritedUsernameLabel.Location = new System.Drawing.Point(268, 43);
			this._inheritedUsernameLabel.Name = "_inheritedUsernameLabel";
			this._inheritedUsernameLabel.Size = new System.Drawing.Size(263, 16);
			this._inheritedUsernameLabel.TabIndex = 91;
			// 
			// _userNameLabel
			// 
			this._userNameLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._userNameLabel.Location = new System.Drawing.Point(31, 38);
			this._userNameLabel.Name = "_userNameLabel";
			this._userNameLabel.Size = new System.Drawing.Size(64, 20);
			this._userNameLabel.TabIndex = 49;
			this._userNameLabel.Text = "User name:";
			this._userNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _userNameTextBox
			// 
			this._userNameTextBox.Location = new System.Drawing.Point(108, 40);
			this._userNameTextBox.Name = "_userNameTextBox";
			this._userNameTextBox.Size = new System.Drawing.Size(154, 20);
			this._userNameTextBox.TabIndex = 48;
			this._userNameTextBox.TextChanged += new System.EventHandler(this._userNameTextBox_TextChanged);
			// 
			// _passwordLabel
			// 
			this._passwordLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._passwordLabel.Location = new System.Drawing.Point(31, 64);
			this._passwordLabel.Name = "_passwordLabel";
			this._passwordLabel.Size = new System.Drawing.Size(63, 20);
			this._passwordLabel.TabIndex = 50;
			this._passwordLabel.Text = "Password:";
			this._passwordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _generalLabel
			// 
			this._generalLabel.AutoSize = true;
			this._generalLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this._generalLabel.Location = new System.Drawing.Point(8, 11);
			this._generalLabel.Name = "_generalLabel";
			this._generalLabel.Size = new System.Drawing.Size(53, 17);
			this._generalLabel.TabIndex = 51;
			this._generalLabel.Text = "General";
			// 
			// _displayPanel
			// 
			this._displayPanel.Controls.Add(this._fontBrowseButton);
			this._displayPanel.Controls.Add(this._fontTextBox);
			this._displayPanel.Controls.Add(this._fontLabel);
			this._displayPanel.Controls.Add(this._textColorPanel);
			this._displayPanel.Controls.Add(this._textColorLabel);
			this._displayPanel.Controls.Add(this._backgroundColorPanel);
			this._displayPanel.Controls.Add(this._displayLabel);
			this._displayPanel.Controls.Add(this._backgroundColorLabel);
			this._displayPanel.Location = new System.Drawing.Point(18, 203);
			this._displayPanel.Name = "_displayPanel";
			this._displayPanel.Size = new System.Drawing.Size(684, 116);
			this._displayPanel.TabIndex = 86;
			// 
			// _textColorPanel
			// 
			this._textColorPanel.BackColor = System.Drawing.Color.White;
			this._textColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this._textColorPanel.Cursor = System.Windows.Forms.Cursors.Hand;
			this._textColorPanel.Location = new System.Drawing.Point(148, 62);
			this._textColorPanel.Name = "_textColorPanel";
			this._textColorPanel.Size = new System.Drawing.Size(35, 21);
			this._textColorPanel.TabIndex = 83;
			this._textColorPanel.Click += new System.EventHandler(this._textColorPanel_Click);
			// 
			// _textColorLabel
			// 
			this._textColorLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._textColorLabel.Location = new System.Drawing.Point(31, 62);
			this._textColorLabel.Name = "_textColorLabel";
			this._textColorLabel.Size = new System.Drawing.Size(99, 20);
			this._textColorLabel.TabIndex = 82;
			this._textColorLabel.Text = "Text color:";
			this._textColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _backgroundColorPanel
			// 
			this._backgroundColorPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
			this._backgroundColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this._backgroundColorPanel.Cursor = System.Windows.Forms.Cursors.Hand;
			this._backgroundColorPanel.Location = new System.Drawing.Point(148, 39);
			this._backgroundColorPanel.Name = "_backgroundColorPanel";
			this._backgroundColorPanel.Size = new System.Drawing.Size(35, 21);
			this._backgroundColorPanel.TabIndex = 81;
			this._backgroundColorPanel.Click += new System.EventHandler(this._backgroundColorPanel_Click);
			// 
			// _displayLabel
			// 
			this._displayLabel.AutoSize = true;
			this._displayLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this._displayLabel.Location = new System.Drawing.Point(8, 11);
			this._displayLabel.Name = "_displayLabel";
			this._displayLabel.Size = new System.Drawing.Size(50, 17);
			this._displayLabel.TabIndex = 55;
			this._displayLabel.Text = "Display";
			// 
			// _backgroundColorLabel
			// 
			this._backgroundColorLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._backgroundColorLabel.Location = new System.Drawing.Point(31, 38);
			this._backgroundColorLabel.Name = "_backgroundColorLabel";
			this._backgroundColorLabel.Size = new System.Drawing.Size(108, 20);
			this._backgroundColorLabel.TabIndex = 54;
			this._backgroundColorLabel.Text = "Background color:";
			this._backgroundColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _openFileDialog
			// 
			this._openFileDialog.Filter = "Identity files (id_rsa, id_dsa)|id_rsa;id_dsa|All files (*.*)|*.*";
			// 
			// SshOptionsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this.ClientSize = new System.Drawing.Size(724, 525);
			this.Controls.Add(this._titleLabel);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this._flowLayoutPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "SshOptionsForm";
			this.Text = "Secure Shell Options";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SshOptionsForm_FormClosing);
			this.Load += new System.EventHandler(this.SshOptionsForm_Load);
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this._flowLayoutPanel.ResumeLayout(false);
			this._hostPanel.ResumeLayout(false);
			this._hostPanel.PerformLayout();
			this._generalPanel.ResumeLayout(false);
			this._generalPanel.PerformLayout();
			this._displayPanel.ResumeLayout(false);
			this._displayPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

		private System.Windows.Forms.Label _titleLabel;
		private System.Windows.Forms.FontDialog _fontDialog;
		private System.Windows.Forms.ColorDialog _colorDialog;
		private System.Windows.Forms.Label _clearScreenKeysLabel;
		private System.Windows.Forms.Label _clearScreenLabel;
		private System.Windows.Forms.Label _pasteKeysLabel;
		private System.Windows.Forms.Label _copyKeysLabel;
		private System.Windows.Forms.Label _pasteLabel;
		private System.Windows.Forms.Label _shortcutsLabel;
		private System.Windows.Forms.Label _copyLabel;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button _fontBrowseButton;
		private System.Windows.Forms.TextBox _fontTextBox;
		private System.Windows.Forms.Label _fontLabel;
		private System.Windows.Forms.FlowLayoutPanel _flowLayoutPanel;
		private System.Windows.Forms.Panel _hostPanel;
		private System.Windows.Forms.Label _hostNameLabel;
		private System.Windows.Forms.TextBox _hostNameTextBox;
		private System.Windows.Forms.Label _hostLabel;
		private System.Windows.Forms.Panel _generalPanel;
		private SecurePasswordTextBox.SecureTextBox _passwordTextBox;
		private System.Windows.Forms.Label _inheritedPasswordLabel;
		private System.Windows.Forms.Label _inheritedUsernameLabel;
		private System.Windows.Forms.Label _userNameLabel;
		private System.Windows.Forms.TextBox _userNameTextBox;
		private System.Windows.Forms.Label _passwordLabel;
		private System.Windows.Forms.Label _generalLabel;
		private System.Windows.Forms.Panel _displayPanel;
		private System.Windows.Forms.Panel _textColorPanel;
		private System.Windows.Forms.Label _textColorLabel;
		private System.Windows.Forms.Panel _backgroundColorPanel;
		private System.Windows.Forms.Label _displayLabel;
		private System.Windows.Forms.Label _backgroundColorLabel;
		private System.Windows.Forms.Button _identityFileBrowseButton;
		private System.Windows.Forms.TextBox _identityFileTextbox;
		private System.Windows.Forms.Label _identityFileLabel;
		private System.Windows.Forms.OpenFileDialog _openFileDialog;
		private System.Windows.Forms.LinkLabel _identityInfoLink;
		private System.Windows.Forms.ToolTip _identityInfoTooltip;

	}
}