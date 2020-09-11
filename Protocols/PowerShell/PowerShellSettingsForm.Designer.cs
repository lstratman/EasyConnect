namespace EasyConnect.Protocols.PowerShell
{
	partial class PowerShellSettingsForm
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
            System.Security.SecureString secureString1 = new System.Security.SecureString();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PowerShellSettingsForm));
            this._colorDialog = new System.Windows.Forms.ColorDialog();
            this._fontDialog = new System.Windows.Forms.FontDialog();
            this._shortcutsLabel = new System.Windows.Forms.Label();
            this._rootLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._settingsCard = new EasyConnect.Common.MaterialCard();
            this._settingsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._hostNameLabel = new System.Windows.Forms.Label();
            this._hostNameTextBox = new System.Windows.Forms.TextBox();
            this._divider1 = new System.Windows.Forms.Panel();
            this._userNameLabel = new System.Windows.Forms.Label();
            this._userNameTextBox = new System.Windows.Forms.TextBox();
            this._divider2 = new System.Windows.Forms.Panel();
            this._passwordLabel = new System.Windows.Forms.Label();
            this._passwordTextBox = new SecurePasswordTextBox.SecureTextBox();
            this._inheritedPasswordTextBox = new System.Windows.Forms.TextBox();
            this._divider3 = new System.Windows.Forms.Panel();
            this._textColorLabel = new System.Windows.Forms.Label();
            this._textColorPanel = new System.Windows.Forms.Panel();
            this._divider4 = new System.Windows.Forms.Panel();
            this._backgroundColorLabel = new System.Windows.Forms.Label();
            this._backgroundColorPanel = new System.Windows.Forms.Panel();
            this._divider5 = new System.Windows.Forms.Panel();
            this._fontLabel = new System.Windows.Forms.Label();
            this._fontTextBox = new System.Windows.Forms.TextBox();
            this._fontBrowseButton = new System.Windows.Forms.Button();
            this._shortcutsCard = new EasyConnect.Common.MaterialCard();
            this._shortcutsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this._rootLayoutPanel.SuspendLayout();
            this._settingsCard.SuspendLayout();
            this._settingsLayoutPanel.SuspendLayout();
            this._shortcutsCard.SuspendLayout();
            this._shortcutsLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _fontDialog
            // 
            this._fontDialog.Color = System.Drawing.SystemColors.ControlText;
            this._fontDialog.FixedPitchOnly = true;
            // 
            // _shortcutsLabel
            // 
            this._shortcutsLabel.AutoSize = true;
            this._shortcutsLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._shortcutsLabel.Location = new System.Drawing.Point(17, 417);
            this._shortcutsLabel.Margin = new System.Windows.Forms.Padding(17, 22, 3, 6);
            this._shortcutsLabel.Name = "_shortcutsLabel";
            this._shortcutsLabel.Size = new System.Drawing.Size(62, 17);
            this._shortcutsLabel.TabIndex = 117;
            this._shortcutsLabel.Text = "Shortcuts";
            // 
            // _rootLayoutPanel
            // 
            this._rootLayoutPanel.Controls.Add(this._settingsCard);
            this._rootLayoutPanel.Controls.Add(this._shortcutsLabel);
            this._rootLayoutPanel.Controls.Add(this._shortcutsCard);
            this._rootLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rootLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this._rootLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._rootLayoutPanel.Name = "_rootLayoutPanel";
            this._rootLayoutPanel.Size = new System.Drawing.Size(718, 601);
            this._rootLayoutPanel.TabIndex = 120;
            // 
            // _settingsCard
            // 
            this._settingsCard.BackColor = System.Drawing.Color.White;
            this._settingsCard.Controls.Add(this._settingsLayoutPanel);
            this._settingsCard.Location = new System.Drawing.Point(15, 19);
            this._settingsCard.Margin = new System.Windows.Forms.Padding(15, 19, 3, 3);
            this._settingsCard.Name = "_settingsCard";
            this._settingsCard.Size = new System.Drawing.Size(692, 373);
            this._settingsCard.TabIndex = 87;
            // 
            // _settingsLayoutPanel
            // 
            this._settingsLayoutPanel.BackColor = System.Drawing.Color.White;
            this._settingsLayoutPanel.Controls.Add(this._hostNameLabel);
            this._settingsLayoutPanel.Controls.Add(this._hostNameTextBox);
            this._settingsLayoutPanel.Controls.Add(this._divider1);
            this._settingsLayoutPanel.Controls.Add(this._userNameLabel);
            this._settingsLayoutPanel.Controls.Add(this._userNameTextBox);
            this._settingsLayoutPanel.Controls.Add(this._divider2);
            this._settingsLayoutPanel.Controls.Add(this._passwordLabel);
            this._settingsLayoutPanel.Controls.Add(this._passwordTextBox);
            this._settingsLayoutPanel.Controls.Add(this._inheritedPasswordTextBox);
            this._settingsLayoutPanel.Controls.Add(this._divider3);
            this._settingsLayoutPanel.Controls.Add(this._textColorLabel);
            this._settingsLayoutPanel.Controls.Add(this._textColorPanel);
            this._settingsLayoutPanel.Controls.Add(this._divider4);
            this._settingsLayoutPanel.Controls.Add(this._backgroundColorLabel);
            this._settingsLayoutPanel.Controls.Add(this._backgroundColorPanel);
            this._settingsLayoutPanel.Controls.Add(this._divider5);
            this._settingsLayoutPanel.Controls.Add(this._fontLabel);
            this._settingsLayoutPanel.Controls.Add(this._fontTextBox);
            this._settingsLayoutPanel.Controls.Add(this._fontBrowseButton);
            this._settingsLayoutPanel.Location = new System.Drawing.Point(5, 9);
            this._settingsLayoutPanel.Name = "_settingsLayoutPanel";
            this._settingsLayoutPanel.Size = new System.Drawing.Size(682, 351);
            this._settingsLayoutPanel.TabIndex = 112;
            // 
            // _hostNameLabel
            // 
            this._hostNameLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._hostNameLabel.Location = new System.Drawing.Point(19, 15);
            this._hostNameLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._hostNameLabel.Name = "_hostNameLabel";
            this._hostNameLabel.Size = new System.Drawing.Size(486, 20);
            this._hostNameLabel.TabIndex = 99;
            this._hostNameLabel.Text = "Host name";
            this._hostNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _hostNameTextBox
            // 
            this._hostNameTextBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._hostNameTextBox.Location = new System.Drawing.Point(511, 12);
            this._hostNameTextBox.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
            this._hostNameTextBox.Name = "_hostNameTextBox";
            this._hostNameTextBox.Size = new System.Drawing.Size(154, 25);
            this._hostNameTextBox.TabIndex = 98;
            // 
            // _divider1
            // 
            this._divider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._divider1.Location = new System.Drawing.Point(0, 56);
            this._divider1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this._divider1.Name = "_divider1";
            this._divider1.Size = new System.Drawing.Size(682, 1);
            this._divider1.TabIndex = 100;
            // 
            // _userNameLabel
            // 
            this._userNameLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._userNameLabel.Location = new System.Drawing.Point(19, 75);
            this._userNameLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._userNameLabel.Name = "_userNameLabel";
            this._userNameLabel.Size = new System.Drawing.Size(486, 20);
            this._userNameLabel.TabIndex = 96;
            this._userNameLabel.Text = "Username";
            this._userNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _userNameTextBox
            // 
            this._userNameTextBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._userNameTextBox.Location = new System.Drawing.Point(511, 75);
            this._userNameTextBox.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
            this._userNameTextBox.Name = "_userNameTextBox";
            this._userNameTextBox.Size = new System.Drawing.Size(154, 25);
            this._userNameTextBox.TabIndex = 95;
            this._userNameTextBox.Enter += new System.EventHandler(this._userNameTextBox_Enter);
            this._userNameTextBox.Leave += new System.EventHandler(this._userNameTextBox_Leave);
            // 
            // _divider2
            // 
            this._divider2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._divider2.Location = new System.Drawing.Point(0, 116);
            this._divider2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this._divider2.Name = "_divider2";
            this._divider2.Size = new System.Drawing.Size(682, 1);
            this._divider2.TabIndex = 101;
            // 
            // _passwordLabel
            // 
            this._passwordLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._passwordLabel.Location = new System.Drawing.Point(19, 135);
            this._passwordLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._passwordLabel.Name = "_passwordLabel";
            this._passwordLabel.Size = new System.Drawing.Size(486, 20);
            this._passwordLabel.TabIndex = 97;
            this._passwordLabel.Text = "Password";
            this._passwordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _passwordTextBox
            // 
            this._passwordTextBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._passwordTextBox.ForeColor = System.Drawing.SystemColors.ControlText;
            this._passwordTextBox.Location = new System.Drawing.Point(511, 135);
            this._passwordTextBox.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
            this._passwordTextBox.Name = "_passwordTextBox";
            this._passwordTextBox.PasswordChar = '*';
            this._passwordTextBox.SecureText = secureString1;
            this._passwordTextBox.Size = new System.Drawing.Size(154, 25);
            this._passwordTextBox.TabIndex = 109;
            this._passwordTextBox.Leave += new System.EventHandler(this._passwordTextBox_Leave);
            // 
            // _inheritedPasswordTextBox
            // 
            this._inheritedPasswordTextBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._inheritedPasswordTextBox.ForeColor = System.Drawing.Color.LightGray;
            this._inheritedPasswordTextBox.Location = new System.Drawing.Point(3, 188);
            this._inheritedPasswordTextBox.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
            this._inheritedPasswordTextBox.Name = "_inheritedPasswordTextBox";
            this._inheritedPasswordTextBox.Size = new System.Drawing.Size(154, 25);
            this._inheritedPasswordTextBox.TabIndex = 113;
            this._inheritedPasswordTextBox.Text = "Inheriting password";
            this._inheritedPasswordTextBox.Visible = false;
            this._inheritedPasswordTextBox.Enter += new System.EventHandler(this._inheritedPasswordTextBox_Enter);
            // 
            // _divider3
            // 
            this._divider3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._divider3.Location = new System.Drawing.Point(0, 216);
            this._divider3.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this._divider3.Name = "_divider3";
            this._divider3.Size = new System.Drawing.Size(682, 1);
            this._divider3.TabIndex = 110;
            // 
            // _textColorLabel
            // 
            this._textColorLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._textColorLabel.Location = new System.Drawing.Point(19, 235);
            this._textColorLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._textColorLabel.Name = "_textColorLabel";
            this._textColorLabel.Size = new System.Drawing.Size(604, 20);
            this._textColorLabel.TabIndex = 102;
            this._textColorLabel.Text = "Text color";
            this._textColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _textColorPanel
            // 
            this._textColorPanel.BackColor = System.Drawing.Color.White;
            this._textColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._textColorPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._textColorPanel.Location = new System.Drawing.Point(629, 235);
            this._textColorPanel.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
            this._textColorPanel.Name = "_textColorPanel";
            this._textColorPanel.Size = new System.Drawing.Size(35, 25);
            this._textColorPanel.TabIndex = 103;
            this._textColorPanel.Click += new System.EventHandler(this._textColorPanel_Click);
            // 
            // _divider4
            // 
            this._divider4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._divider4.Location = new System.Drawing.Point(0, 276);
            this._divider4.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this._divider4.Name = "_divider4";
            this._divider4.Size = new System.Drawing.Size(682, 1);
            this._divider4.TabIndex = 111;
            // 
            // _backgroundColorLabel
            // 
            this._backgroundColorLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._backgroundColorLabel.Location = new System.Drawing.Point(19, 295);
            this._backgroundColorLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._backgroundColorLabel.Name = "_backgroundColorLabel";
            this._backgroundColorLabel.Size = new System.Drawing.Size(604, 20);
            this._backgroundColorLabel.TabIndex = 100;
            this._backgroundColorLabel.Text = "Background color";
            this._backgroundColorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _backgroundColorPanel
            // 
            this._backgroundColorPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this._backgroundColorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._backgroundColorPanel.Cursor = System.Windows.Forms.Cursors.Hand;
            this._backgroundColorPanel.Location = new System.Drawing.Point(629, 295);
            this._backgroundColorPanel.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
            this._backgroundColorPanel.Name = "_backgroundColorPanel";
            this._backgroundColorPanel.Size = new System.Drawing.Size(35, 25);
            this._backgroundColorPanel.TabIndex = 101;
            this._backgroundColorPanel.Click += new System.EventHandler(this._backgroundColorPanel_Click);
            // 
            // _divider5
            // 
            this._divider5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._divider5.Location = new System.Drawing.Point(0, 336);
            this._divider5.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this._divider5.Name = "_divider5";
            this._divider5.Size = new System.Drawing.Size(682, 1);
            this._divider5.TabIndex = 112;
            // 
            // _fontLabel
            // 
            this._fontLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._fontLabel.Location = new System.Drawing.Point(19, 355);
            this._fontLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._fontLabel.Name = "_fontLabel";
            this._fontLabel.Size = new System.Drawing.Size(486, 20);
            this._fontLabel.TabIndex = 104;
            this._fontLabel.Text = "Font";
            this._fontLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _fontTextBox
            // 
            this._fontTextBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._fontTextBox.Location = new System.Drawing.Point(511, 355);
            this._fontTextBox.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
            this._fontTextBox.Name = "_fontTextBox";
            this._fontTextBox.ReadOnly = true;
            this._fontTextBox.Size = new System.Drawing.Size(125, 25);
            this._fontTextBox.TabIndex = 105;
            // 
            // _fontBrowseButton
            // 
            this._fontBrowseButton.Location = new System.Drawing.Point(640, 354);
            this._fontBrowseButton.Margin = new System.Windows.Forms.Padding(1, 14, 3, 3);
            this._fontBrowseButton.Name = "_fontBrowseButton";
            this._fontBrowseButton.Size = new System.Drawing.Size(26, 27);
            this._fontBrowseButton.TabIndex = 106;
            this._fontBrowseButton.Text = "...";
            this._fontBrowseButton.UseVisualStyleBackColor = true;
            this._fontBrowseButton.Click += new System.EventHandler(this._fontBrowseButton_Click);
            // 
            // _shortcutsCard
            // 
            this._shortcutsCard.BackColor = System.Drawing.Color.White;
            this._shortcutsCard.Controls.Add(this._shortcutsLayoutPanel);
            this._shortcutsCard.Location = new System.Drawing.Point(15, 443);
            this._shortcutsCard.Margin = new System.Windows.Forms.Padding(15, 3, 3, 3);
            this._shortcutsCard.Name = "_shortcutsCard";
            this._shortcutsCard.Size = new System.Drawing.Size(692, 135);
            this._shortcutsCard.TabIndex = 119;
            // 
            // _shortcutsLayoutPanel
            // 
            this._shortcutsLayoutPanel.BackColor = System.Drawing.Color.White;
            this._shortcutsLayoutPanel.Controls.Add(this.label5);
            this._shortcutsLayoutPanel.Controls.Add(this.label6);
            this._shortcutsLayoutPanel.Controls.Add(this.panel1);
            this._shortcutsLayoutPanel.Controls.Add(this.label3);
            this._shortcutsLayoutPanel.Controls.Add(this.label4);
            this._shortcutsLayoutPanel.Location = new System.Drawing.Point(5, 9);
            this._shortcutsLayoutPanel.Name = "_shortcutsLayoutPanel";
            this._shortcutsLayoutPanel.Size = new System.Drawing.Size(682, 116);
            this._shortcutsLayoutPanel.TabIndex = 118;
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.White;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(19, 15);
            this.label5.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(486, 20);
            this.label5.TabIndex = 115;
            this.label5.Text = "Paste";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.Color.White;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(511, 19);
            this.label6.Margin = new System.Windows.Forms.Padding(3, 19, 3, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(155, 19);
            this.label6.TabIndex = 116;
            this.label6.Text = "Mouse right-click";
            this.label6.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel1.Location = new System.Drawing.Point(0, 56);
            this.panel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(682, 1);
            this.panel1.TabIndex = 119;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.White;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(19, 75);
            this.label3.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(486, 20);
            this.label3.TabIndex = 107;
            this.label3.Text = "Copy";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.White;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(511, 79);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 19, 3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(155, 19);
            this.label4.TabIndex = 110;
            this.label4.Text = "Mouse select";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // PowerShellSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(718, 601);
            this.Controls.Add(this._rootLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PowerShellSettingsForm";
            this.Text = "PowerShell";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PowerShellSettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.PowerShellSettingsForm_Load);
            this._rootLayoutPanel.ResumeLayout(false);
            this._rootLayoutPanel.PerformLayout();
            this._settingsCard.ResumeLayout(false);
            this._settingsLayoutPanel.ResumeLayout(false);
            this._settingsLayoutPanel.PerformLayout();
            this._shortcutsCard.ResumeLayout(false);
            this._shortcutsLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.ColorDialog _colorDialog;
		private System.Windows.Forms.FontDialog _fontDialog;
        private Common.MaterialCard _settingsCard;
        private System.Windows.Forms.Button _fontBrowseButton;
        private System.Windows.Forms.TextBox _fontTextBox;
        private System.Windows.Forms.Label _fontLabel;
        private System.Windows.Forms.Panel _textColorPanel;
        private System.Windows.Forms.Panel _backgroundColorPanel;
        private System.Windows.Forms.Label _backgroundColorLabel;
        private SecurePasswordTextBox.SecureTextBox _passwordTextBox;
        private System.Windows.Forms.Label _userNameLabel;
        private System.Windows.Forms.TextBox _userNameTextBox;
        private System.Windows.Forms.Label _passwordLabel;
        private System.Windows.Forms.TextBox _hostNameTextBox;
        private System.Windows.Forms.Label _textColorLabel;
        private System.Windows.Forms.Label _hostNameLabel;
        private System.Windows.Forms.FlowLayoutPanel _settingsLayoutPanel;
        private System.Windows.Forms.Panel _divider1;
        private System.Windows.Forms.Panel _divider2;
        private System.Windows.Forms.Panel _divider3;
        private System.Windows.Forms.Panel _divider4;
        private System.Windows.Forms.Panel _divider5;
        private System.Windows.Forms.Label _shortcutsLabel;
        private System.Windows.Forms.FlowLayoutPanel _shortcutsLayoutPanel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private Common.MaterialCard _shortcutsCard;
        private System.Windows.Forms.TextBox _inheritedPasswordTextBox;
        private System.Windows.Forms.FlowLayoutPanel _rootLayoutPanel;
    }
}