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
            this._passwordLabel = new System.Windows.Forms.Label();
            this._generalLabel = new System.Windows.Forms.Label();
            this._userNameLabel = new System.Windows.Forms.Label();
            this._userNameTextBox = new System.Windows.Forms.TextBox();
            this._flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._hostPanel = new System.Windows.Forms.Panel();
            this._hostNameLabel = new System.Windows.Forms.Label();
            this._hostNameTextBox = new System.Windows.Forms.TextBox();
            this._hostLabel = new System.Windows.Forms.Label();
            this._hostDividerPanel = new System.Windows.Forms.Panel();
            this._generalPanel = new System.Windows.Forms.Panel();
            this._identityInfoLink = new System.Windows.Forms.LinkLabel();
            this._identityFileTextbox = new System.Windows.Forms.TextBox();
            this._identityFileBrowseButton = new System.Windows.Forms.Button();
            this._identityFileLabel = new System.Windows.Forms.Label();
            this._passwordTextBox = new SecurePasswordTextBox.SecureTextBox();
            this._generalDividerPanel = new System.Windows.Forms.Panel();
            this._displayPanel = new System.Windows.Forms.Panel();
            this._fontBrowseButton = new System.Windows.Forms.Button();
            this._fontTextBox = new System.Windows.Forms.TextBox();
            this._fontLabel = new System.Windows.Forms.Label();
            this._textColorPanel = new System.Windows.Forms.Panel();
            this._textColorLabel = new System.Windows.Forms.Label();
            this._backgroundColorPanel = new System.Windows.Forms.Panel();
            this._displayLabel = new System.Windows.Forms.Label();
            this._backgroundColorLabel = new System.Windows.Forms.Label();
            this._displayDividerPanel = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this._pasteKeysLabel = new System.Windows.Forms.Label();
            this._copyKeysLabel = new System.Windows.Forms.Label();
            this._pasteLabel = new System.Windows.Forms.Label();
            this._shortcutsLabel = new System.Windows.Forms.Label();
            this._copyLabel = new System.Windows.Forms.Label();
            this._colorDialog = new System.Windows.Forms.ColorDialog();
            this._fontDialog = new System.Windows.Forms.FontDialog();
            this._openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this._identityInfoTooltip = new System.Windows.Forms.ToolTip(this.components);
            this._clearScreenKeysLabel = new System.Windows.Forms.Label();
            this._clearScreenLabel = new System.Windows.Forms.Label();
            this._flowLayoutPanel.SuspendLayout();
            this._hostPanel.SuspendLayout();
            this._generalPanel.SuspendLayout();
            this._displayPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // _passwordLabel
            // 
            this._passwordLabel.Location = new System.Drawing.Point(91, 35);
            this._passwordLabel.Name = "_passwordLabel";
            this._passwordLabel.Size = new System.Drawing.Size(150, 20);
            this._passwordLabel.TabIndex = 50;
            this._passwordLabel.Text = "Password:";
            this._passwordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _generalLabel
            // 
            this._generalLabel.AutoSize = true;
            this._generalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._generalLabel.Location = new System.Drawing.Point(8, 11);
            this._generalLabel.Name = "_generalLabel";
            this._generalLabel.Size = new System.Drawing.Size(63, 16);
            this._generalLabel.TabIndex = 51;
            this._generalLabel.Text = "General";
            // 
            // _userNameLabel
            // 
            this._userNameLabel.Location = new System.Drawing.Point(91, 9);
            this._userNameLabel.Name = "_userNameLabel";
            this._userNameLabel.Size = new System.Drawing.Size(150, 20);
            this._userNameLabel.TabIndex = 49;
            this._userNameLabel.Text = "User name:";
            this._userNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _userNameTextBox
            // 
            this._userNameTextBox.Location = new System.Drawing.Point(247, 10);
            this._userNameTextBox.Name = "_userNameTextBox";
            this._userNameTextBox.Size = new System.Drawing.Size(154, 20);
            this._userNameTextBox.TabIndex = 48;
            // 
            // _flowLayoutPanel
            // 
            this._flowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._flowLayoutPanel.Controls.Add(this._hostPanel);
            this._flowLayoutPanel.Controls.Add(this._hostDividerPanel);
            this._flowLayoutPanel.Controls.Add(this._generalPanel);
            this._flowLayoutPanel.Controls.Add(this._generalDividerPanel);
            this._flowLayoutPanel.Controls.Add(this._displayPanel);
            this._flowLayoutPanel.Controls.Add(this._displayDividerPanel);
            this._flowLayoutPanel.Controls.Add(this.panel2);
            this._flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this._flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._flowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this._flowLayoutPanel.Name = "_flowLayoutPanel";
            this._flowLayoutPanel.Padding = new System.Windows.Forms.Padding(15);
            this._flowLayoutPanel.Size = new System.Drawing.Size(726, 687);
            this._flowLayoutPanel.TabIndex = 83;
            this._flowLayoutPanel.WrapContents = false;
            this._flowLayoutPanel.Resize += new System.EventHandler(this._flowLayoutPanel_Resize);
            // 
            // _hostPanel
            // 
            this._hostPanel.Controls.Add(this._hostNameLabel);
            this._hostPanel.Controls.Add(this._hostNameTextBox);
            this._hostPanel.Controls.Add(this._hostLabel);
            this._hostPanel.Location = new System.Drawing.Point(18, 18);
            this._hostPanel.Name = "_hostPanel";
            this._hostPanel.Size = new System.Drawing.Size(684, 41);
            this._hostPanel.TabIndex = 84;
            // 
            // _hostNameLabel
            // 
            this._hostNameLabel.Location = new System.Drawing.Point(91, 9);
            this._hostNameLabel.Name = "_hostNameLabel";
            this._hostNameLabel.Size = new System.Drawing.Size(150, 20);
            this._hostNameLabel.TabIndex = 53;
            this._hostNameLabel.Text = "Host name:";
            this._hostNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _hostNameTextBox
            // 
            this._hostNameTextBox.Location = new System.Drawing.Point(247, 10);
            this._hostNameTextBox.Name = "_hostNameTextBox";
            this._hostNameTextBox.Size = new System.Drawing.Size(154, 20);
            this._hostNameTextBox.TabIndex = 52;
            // 
            // _hostLabel
            // 
            this._hostLabel.AutoSize = true;
            this._hostLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._hostLabel.Location = new System.Drawing.Point(8, 11);
            this._hostLabel.Name = "_hostLabel";
            this._hostLabel.Size = new System.Drawing.Size(40, 16);
            this._hostLabel.TabIndex = 54;
            this._hostLabel.Text = "Host";
            // 
            // _hostDividerPanel
            // 
            this._hostDividerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._hostDividerPanel.BackColor = System.Drawing.Color.Silver;
            this._hostDividerPanel.Location = new System.Drawing.Point(18, 65);
            this._hostDividerPanel.Name = "_hostDividerPanel";
            this._hostDividerPanel.Size = new System.Drawing.Size(684, 1);
            this._hostDividerPanel.TabIndex = 63;
            // 
            // _generalPanel
            // 
            this._generalPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._generalPanel.Controls.Add(this._identityInfoLink);
            this._generalPanel.Controls.Add(this._identityFileTextbox);
            this._generalPanel.Controls.Add(this._identityFileBrowseButton);
            this._generalPanel.Controls.Add(this._identityFileLabel);
            this._generalPanel.Controls.Add(this._passwordTextBox);
            this._generalPanel.Controls.Add(this._userNameLabel);
            this._generalPanel.Controls.Add(this._userNameTextBox);
            this._generalPanel.Controls.Add(this._passwordLabel);
            this._generalPanel.Controls.Add(this._generalLabel);
            this._generalPanel.Location = new System.Drawing.Point(18, 72);
            this._generalPanel.Name = "_generalPanel";
            this._generalPanel.Size = new System.Drawing.Size(684, 93);
            this._generalPanel.TabIndex = 83;
            // 
            // _identityInfoLink
            // 
            this._identityInfoLink.AutoSize = true;
            this._identityInfoLink.Location = new System.Drawing.Point(437, 65);
            this._identityInfoLink.Name = "_identityInfoLink";
            this._identityInfoLink.Size = new System.Drawing.Size(13, 13);
            this._identityInfoLink.TabIndex = 90;
            this._identityInfoLink.TabStop = true;
            this._identityInfoLink.Text = "?";
            this._identityInfoTooltip.SetToolTip(this._identityInfoLink, "Must be an ssh.com-formatted \r\n(---- BEGIN SSH2 ENCRYPTED \r\nPRIVATE KEY ----) pri" +
        "vate key.");
            // 
            // _identityFileTextbox
            // 
            this._identityFileTextbox.Location = new System.Drawing.Point(246, 62);
            this._identityFileTextbox.Name = "_identityFileTextbox";
            this._identityFileTextbox.Size = new System.Drawing.Size(154, 20);
            this._identityFileTextbox.TabIndex = 88;
            // 
            // _identityFileBrowseButton
            // 
            this._identityFileBrowseButton.Location = new System.Drawing.Point(406, 60);
            this._identityFileBrowseButton.Name = "_identityFileBrowseButton";
            this._identityFileBrowseButton.Size = new System.Drawing.Size(25, 23);
            this._identityFileBrowseButton.TabIndex = 87;
            this._identityFileBrowseButton.Text = "...";
            this._identityFileBrowseButton.UseVisualStyleBackColor = true;
            this._identityFileBrowseButton.Click += new System.EventHandler(this._identityFileBrowseButton_Click);
            // 
            // _identityFileLabel
            // 
            this._identityFileLabel.Location = new System.Drawing.Point(90, 61);
            this._identityFileLabel.Name = "_identityFileLabel";
            this._identityFileLabel.Size = new System.Drawing.Size(150, 20);
            this._identityFileLabel.TabIndex = 85;
            this._identityFileLabel.Text = "Identity:";
            this._identityFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _passwordTextBox
            // 
            this._passwordTextBox.Location = new System.Drawing.Point(247, 36);
            this._passwordTextBox.Name = "_passwordTextBox";
            this._passwordTextBox.PasswordChar = '*';
            this._passwordTextBox.SecureText = secureString1;
            this._passwordTextBox.Size = new System.Drawing.Size(154, 20);
            this._passwordTextBox.TabIndex = 84;
            // 
            // _generalDividerPanel
            // 
            this._generalDividerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._generalDividerPanel.BackColor = System.Drawing.Color.Silver;
            this._generalDividerPanel.Location = new System.Drawing.Point(18, 171);
            this._generalDividerPanel.Name = "_generalDividerPanel";
            this._generalDividerPanel.Size = new System.Drawing.Size(684, 1);
            this._generalDividerPanel.TabIndex = 85;
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
            this._displayPanel.Location = new System.Drawing.Point(18, 178);
            this._displayPanel.Name = "_displayPanel";
            this._displayPanel.Size = new System.Drawing.Size(684, 96);
            this._displayPanel.TabIndex = 86;
            // 
            // _fontBrowseButton
            // 
            this._fontBrowseButton.Location = new System.Drawing.Point(406, 62);
            this._fontBrowseButton.Name = "_fontBrowseButton";
            this._fontBrowseButton.Size = new System.Drawing.Size(25, 23);
            this._fontBrowseButton.TabIndex = 88;
            this._fontBrowseButton.Text = "...";
            this._fontBrowseButton.UseVisualStyleBackColor = true;
            this._fontBrowseButton.Click += new System.EventHandler(this._fontBrowseButton_Click);
            // 
            // _fontTextBox
            // 
            this._fontTextBox.Location = new System.Drawing.Point(246, 64);
            this._fontTextBox.Name = "_fontTextBox";
            this._fontTextBox.ReadOnly = true;
            this._fontTextBox.Size = new System.Drawing.Size(154, 20);
            this._fontTextBox.TabIndex = 85;
            // 
            // _fontLabel
            // 
            this._fontLabel.Location = new System.Drawing.Point(91, 62);
            this._fontLabel.Name = "_fontLabel";
            this._fontLabel.Size = new System.Drawing.Size(150, 20);
            this._fontLabel.TabIndex = 84;
            this._fontLabel.Text = "Font:";
            this._fontLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _textColorPanel
            // 
            this._textColorPanel.BackColor = System.Drawing.Color.LightGray;
            this._textColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._textColorPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._textColorPanel.Location = new System.Drawing.Point(247, 37);
            this._textColorPanel.Name = "_textColorPanel";
            this._textColorPanel.Size = new System.Drawing.Size(35, 21);
            this._textColorPanel.TabIndex = 83;
            this._textColorPanel.Click += new System.EventHandler(this._textColorPanel_Click);
            // 
            // _textColorLabel
            // 
            this._textColorLabel.Location = new System.Drawing.Point(91, 36);
            this._textColorLabel.Name = "_textColorLabel";
            this._textColorLabel.Size = new System.Drawing.Size(150, 20);
            this._textColorLabel.TabIndex = 82;
            this._textColorLabel.Text = "Text color:";
            this._textColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _backgroundColorPanel
            // 
            this._backgroundColorPanel.BackColor = System.Drawing.Color.Black;
            this._backgroundColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this._backgroundColorPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._backgroundColorPanel.Location = new System.Drawing.Point(247, 12);
            this._backgroundColorPanel.Name = "_backgroundColorPanel";
            this._backgroundColorPanel.Size = new System.Drawing.Size(35, 21);
            this._backgroundColorPanel.TabIndex = 81;
            this._backgroundColorPanel.Click += new System.EventHandler(this._backgroundColorPanel_Click);
            // 
            // _displayLabel
            // 
            this._displayLabel.AutoSize = true;
            this._displayLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._displayLabel.Location = new System.Drawing.Point(8, 11);
            this._displayLabel.Name = "_displayLabel";
            this._displayLabel.Size = new System.Drawing.Size(61, 16);
            this._displayLabel.TabIndex = 55;
            this._displayLabel.Text = "Display";
            // 
            // _backgroundColorLabel
            // 
            this._backgroundColorLabel.Location = new System.Drawing.Point(91, 11);
            this._backgroundColorLabel.Name = "_backgroundColorLabel";
            this._backgroundColorLabel.Size = new System.Drawing.Size(150, 20);
            this._backgroundColorLabel.TabIndex = 54;
            this._backgroundColorLabel.Text = "Background color:";
            this._backgroundColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _displayDividerPanel
            // 
            this._displayDividerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._displayDividerPanel.BackColor = System.Drawing.Color.Silver;
            this._displayDividerPanel.Location = new System.Drawing.Point(18, 280);
            this._displayDividerPanel.Name = "_displayDividerPanel";
            this._displayDividerPanel.Size = new System.Drawing.Size(684, 1);
            this._displayDividerPanel.TabIndex = 87;
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
            this.panel2.Location = new System.Drawing.Point(18, 287);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(684, 93);
            this.panel2.TabIndex = 88;
            // 
            // _pasteKeysLabel
            // 
            this._pasteKeysLabel.AutoSize = true;
            this._pasteKeysLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._pasteKeysLabel.Location = new System.Drawing.Point(243, 40);
            this._pasteKeysLabel.Name = "_pasteKeysLabel";
            this._pasteKeysLabel.Size = new System.Drawing.Size(37, 13);
            this._pasteKeysLabel.TabIndex = 90;
            this._pasteKeysLabel.Text = "Alt+V";
            // 
            // _copyKeysLabel
            // 
            this._copyKeysLabel.AutoSize = true;
            this._copyKeysLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._copyKeysLabel.Location = new System.Drawing.Point(243, 15);
            this._copyKeysLabel.Name = "_copyKeysLabel";
            this._copyKeysLabel.Size = new System.Drawing.Size(37, 13);
            this._copyKeysLabel.TabIndex = 89;
            this._copyKeysLabel.Text = "Alt+C";
            // 
            // _pasteLabel
            // 
            this._pasteLabel.Location = new System.Drawing.Point(91, 36);
            this._pasteLabel.Name = "_pasteLabel";
            this._pasteLabel.Size = new System.Drawing.Size(150, 20);
            this._pasteLabel.TabIndex = 82;
            this._pasteLabel.Text = "Paste:";
            this._pasteLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _shortcutsLabel
            // 
            this._shortcutsLabel.AutoSize = true;
            this._shortcutsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._shortcutsLabel.Location = new System.Drawing.Point(8, 11);
            this._shortcutsLabel.Name = "_shortcutsLabel";
            this._shortcutsLabel.Size = new System.Drawing.Size(102, 16);
            this._shortcutsLabel.TabIndex = 55;
            this._shortcutsLabel.Text = "Shortcut Keys";
            // 
            // _copyLabel
            // 
            this._copyLabel.Location = new System.Drawing.Point(91, 11);
            this._copyLabel.Name = "_copyLabel";
            this._copyLabel.Size = new System.Drawing.Size(150, 20);
            this._copyLabel.TabIndex = 54;
            this._copyLabel.Text = "Copy:";
            this._copyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _fontDialog
            // 
            this._fontDialog.Color = System.Drawing.SystemColors.ControlText;
            this._fontDialog.FixedPitchOnly = true;
            // 
            // _openFileDialog
            // 
            this._openFileDialog.Filter = "Identity files (id_rsa, id_dsa)|id_rsa;id_dsa|All files (*.*)|*.*";
            // 
            // _clearScreenKeysLabel
            // 
            this._clearScreenKeysLabel.AutoSize = true;
            this._clearScreenKeysLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._clearScreenKeysLabel.Location = new System.Drawing.Point(243, 65);
            this._clearScreenKeysLabel.Name = "_clearScreenKeysLabel";
            this._clearScreenKeysLabel.Size = new System.Drawing.Size(36, 13);
            this._clearScreenKeysLabel.TabIndex = 92;
            this._clearScreenKeysLabel.Text = "Alt+L";
            // 
            // _clearScreenLabel
            // 
            this._clearScreenLabel.Location = new System.Drawing.Point(91, 61);
            this._clearScreenLabel.Name = "_clearScreenLabel";
            this._clearScreenLabel.Size = new System.Drawing.Size(150, 20);
            this._clearScreenLabel.TabIndex = 91;
            this._clearScreenLabel.Text = "Clear screen:";
            this._clearScreenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SshOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(724, 686);
            this.Controls.Add(this._flowLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SshOptionsForm";
            this.Text = "Secure Shell Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SshOptionsForm_FormClosing);
            this.Load += new System.EventHandler(this.SshOptionsForm_Load);
            this._flowLayoutPanel.ResumeLayout(false);
            this._hostPanel.ResumeLayout(false);
            this._hostPanel.PerformLayout();
            this._generalPanel.ResumeLayout(false);
            this._generalPanel.PerformLayout();
            this._displayPanel.ResumeLayout(false);
            this._displayPanel.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label _passwordLabel;
        private System.Windows.Forms.Label _generalLabel;
        private System.Windows.Forms.Label _userNameLabel;
        private System.Windows.Forms.TextBox _userNameTextBox;
        private System.Windows.Forms.FlowLayoutPanel _flowLayoutPanel;
        private System.Windows.Forms.Panel _hostPanel;
        private System.Windows.Forms.Label _hostNameLabel;
        private System.Windows.Forms.TextBox _hostNameTextBox;
        private System.Windows.Forms.Label _hostLabel;
        private System.Windows.Forms.Panel _hostDividerPanel;
        private System.Windows.Forms.Panel _generalPanel;
        private SecurePasswordTextBox.SecureTextBox _passwordTextBox;
        private System.Windows.Forms.Button _identityFileBrowseButton;
        private System.Windows.Forms.Label _identityFileLabel;
        private System.Windows.Forms.Panel _generalDividerPanel;
        private System.Windows.Forms.Panel _displayPanel;
        private System.Windows.Forms.Label _displayLabel;
        private System.Windows.Forms.Label _backgroundColorLabel;
        private System.Windows.Forms.ColorDialog _colorDialog;
        private System.Windows.Forms.Panel _textColorPanel;
        private System.Windows.Forms.Label _textColorLabel;
        private System.Windows.Forms.Panel _backgroundColorPanel;
        private System.Windows.Forms.Button _fontBrowseButton;
        private System.Windows.Forms.TextBox _fontTextBox;
        private System.Windows.Forms.Label _fontLabel;
        private System.Windows.Forms.FontDialog _fontDialog;
        private System.Windows.Forms.OpenFileDialog _openFileDialog;
        private System.Windows.Forms.TextBox _identityFileTextbox;
        private System.Windows.Forms.Panel _displayDividerPanel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label _pasteKeysLabel;
        private System.Windows.Forms.Label _copyKeysLabel;
        private System.Windows.Forms.Label _pasteLabel;
        private System.Windows.Forms.Label _shortcutsLabel;
        private System.Windows.Forms.Label _copyLabel;
        private System.Windows.Forms.LinkLabel _identityInfoLink;
        private System.Windows.Forms.ToolTip _identityInfoTooltip;
        private System.Windows.Forms.Label _clearScreenKeysLabel;
        private System.Windows.Forms.Label _clearScreenLabel;
    }
}