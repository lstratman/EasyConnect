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
            this._passwordTextBox = new SecurePasswordTextBox.SecureTextBox();
            this._flowLayoutPanel.SuspendLayout();
            this._hostPanel.SuspendLayout();
            this._generalPanel.SuspendLayout();
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
            this._generalPanel.Controls.Add(this._passwordTextBox);
            this._generalPanel.Controls.Add(this._userNameLabel);
            this._generalPanel.Controls.Add(this._userNameTextBox);
            this._generalPanel.Controls.Add(this._passwordLabel);
            this._generalPanel.Controls.Add(this._generalLabel);
            this._generalPanel.Location = new System.Drawing.Point(18, 72);
            this._generalPanel.Name = "_generalPanel";
            this._generalPanel.Size = new System.Drawing.Size(684, 74);
            this._generalPanel.TabIndex = 83;
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
    }
}