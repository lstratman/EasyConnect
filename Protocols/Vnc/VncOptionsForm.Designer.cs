namespace EasyConnect.Protocols.Vnc
{
    partial class VncOptionsForm
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
			System.Security.SecureString secureString2 = new System.Security.SecureString();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VncOptionsForm));
			this._displayLabel = new System.Windows.Forms.Label();
			this._displayNumberLabel = new System.Windows.Forms.Label();
			this._flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
			this._generalPanel = new System.Windows.Forms.Panel();
			this._inheritedPasswordLabel = new System.Windows.Forms.Label();
			this._passwordTextBox = new SecurePasswordTextBox.SecureTextBox();
			this._passwordLabel = new System.Windows.Forms.Label();
			this._generalLabel = new System.Windows.Forms.Label();
			this._hostPanel = new System.Windows.Forms.Panel();
			this._portUpDown = new System.Windows.Forms.NumericUpDown();
			this._portLabel = new System.Windows.Forms.Label();
			this._hostNameLabel = new System.Windows.Forms.Label();
			this._hostNameTextBox = new System.Windows.Forms.TextBox();
			this._hostLabel = new System.Windows.Forms.Label();
			this._displayPanel = new System.Windows.Forms.Panel();
			this._viewOnlyCheckbox = new System.Windows.Forms.CheckBox();
			this._viewOnlyLabel = new System.Windows.Forms.Label();
			this._scaleCheckbox = new System.Windows.Forms.CheckBox();
			this._displayUpDown = new System.Windows.Forms.NumericUpDown();
			this._scaleLabel = new System.Windows.Forms.Label();
			this._titleLabel = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this._flowLayoutPanel.SuspendLayout();
			this._generalPanel.SuspendLayout();
			this._hostPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._portUpDown)).BeginInit();
			this._displayPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._displayUpDown)).BeginInit();
			this.SuspendLayout();
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
			// _displayNumberLabel
			// 
			this._displayNumberLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._displayNumberLabel.Location = new System.Drawing.Point(31, 38);
			this._displayNumberLabel.Name = "_displayNumberLabel";
			this._displayNumberLabel.Size = new System.Drawing.Size(122, 20);
			this._displayNumberLabel.TabIndex = 53;
			this._displayNumberLabel.Text = "Display:";
			this._displayNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _flowLayoutPanel
			// 
			this._flowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._flowLayoutPanel.Controls.Add(this._generalPanel);
			this._flowLayoutPanel.Controls.Add(this._hostPanel);
			this._flowLayoutPanel.Controls.Add(this._displayPanel);
			this._flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this._flowLayoutPanel.Location = new System.Drawing.Point(0, 61);
			this._flowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this._flowLayoutPanel.Name = "_flowLayoutPanel";
			this._flowLayoutPanel.Padding = new System.Windows.Forms.Padding(15, 0, 15, 15);
			this._flowLayoutPanel.Size = new System.Drawing.Size(719, 623);
			this._flowLayoutPanel.TabIndex = 83;
			this._flowLayoutPanel.WrapContents = false;
			// 
			// _generalPanel
			// 
			this._generalPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._generalPanel.Controls.Add(this._inheritedPasswordLabel);
			this._generalPanel.Controls.Add(this._passwordTextBox);
			this._generalPanel.Controls.Add(this._passwordLabel);
			this._generalPanel.Controls.Add(this._generalLabel);
			this._generalPanel.Location = new System.Drawing.Point(18, 3);
			this._generalPanel.Name = "_generalPanel";
			this._generalPanel.Size = new System.Drawing.Size(684, 76);
			this._generalPanel.TabIndex = 86;
			// 
			// _inheritedPasswordLabel
			// 
			this._inheritedPasswordLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._inheritedPasswordLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._inheritedPasswordLabel.Location = new System.Drawing.Point(319, 42);
			this._inheritedPasswordLabel.Name = "_inheritedPasswordLabel";
			this._inheritedPasswordLabel.Size = new System.Drawing.Size(263, 16);
			this._inheritedPasswordLabel.TabIndex = 94;
			// 
			// _passwordTextBox
			// 
			this._passwordTextBox.Location = new System.Drawing.Point(159, 39);
			this._passwordTextBox.Name = "_passwordTextBox";
			this._passwordTextBox.PasswordChar = '*';
			this._passwordTextBox.SecureText = secureString2;
			this._passwordTextBox.Size = new System.Drawing.Size(154, 20);
			this._passwordTextBox.TabIndex = 82;
			this._passwordTextBox.TextChanged += new System.EventHandler(this._passwordTextBox_TextChanged);
			// 
			// _passwordLabel
			// 
			this._passwordLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._passwordLabel.Location = new System.Drawing.Point(31, 39);
			this._passwordLabel.Name = "_passwordLabel";
			this._passwordLabel.Size = new System.Drawing.Size(125, 20);
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
			// _hostPanel
			// 
			this._hostPanel.Controls.Add(this._portUpDown);
			this._hostPanel.Controls.Add(this._portLabel);
			this._hostPanel.Controls.Add(this._hostNameLabel);
			this._hostPanel.Controls.Add(this._hostNameTextBox);
			this._hostPanel.Controls.Add(this._hostLabel);
			this._hostPanel.Location = new System.Drawing.Point(18, 85);
			this._hostPanel.Name = "_hostPanel";
			this._hostPanel.Size = new System.Drawing.Size(684, 99);
			this._hostPanel.TabIndex = 84;
			// 
			// _portUpDown
			// 
			this._portUpDown.Location = new System.Drawing.Point(159, 63);
			this._portUpDown.Maximum = new decimal(new int[] {
            32167,
            0,
            0,
            0});
			this._portUpDown.Name = "_portUpDown";
			this._portUpDown.Size = new System.Drawing.Size(61, 20);
			this._portUpDown.TabIndex = 56;
			this._portUpDown.Value = new decimal(new int[] {
            5900,
            0,
            0,
            0});
			// 
			// _portLabel
			// 
			this._portLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._portLabel.Location = new System.Drawing.Point(31, 61);
			this._portLabel.Name = "_portLabel";
			this._portLabel.Size = new System.Drawing.Size(122, 20);
			this._portLabel.TabIndex = 55;
			this._portLabel.Text = "Port:";
			this._portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _hostNameLabel
			// 
			this._hostNameLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._hostNameLabel.Location = new System.Drawing.Point(31, 38);
			this._hostNameLabel.Name = "_hostNameLabel";
			this._hostNameLabel.Size = new System.Drawing.Size(122, 20);
			this._hostNameLabel.TabIndex = 53;
			this._hostNameLabel.Text = "Host name:";
			this._hostNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _hostNameTextBox
			// 
			this._hostNameTextBox.Location = new System.Drawing.Point(159, 39);
			this._hostNameTextBox.Name = "_hostNameTextBox";
			this._hostNameTextBox.Size = new System.Drawing.Size(154, 20);
			this._hostNameTextBox.TabIndex = 52;
			// 
			// _hostLabel
			// 
			this._hostLabel.AutoSize = true;
			this._hostLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F);
			this._hostLabel.Location = new System.Drawing.Point(8, 11);
			this._hostLabel.Name = "_hostLabel";
			this._hostLabel.Size = new System.Drawing.Size(35, 17);
			this._hostLabel.TabIndex = 54;
			this._hostLabel.Text = "Host";
			// 
			// _displayPanel
			// 
			this._displayPanel.Controls.Add(this._viewOnlyCheckbox);
			this._displayPanel.Controls.Add(this._viewOnlyLabel);
			this._displayPanel.Controls.Add(this._scaleCheckbox);
			this._displayPanel.Controls.Add(this._displayUpDown);
			this._displayPanel.Controls.Add(this._displayLabel);
			this._displayPanel.Controls.Add(this._displayNumberLabel);
			this._displayPanel.Controls.Add(this._scaleLabel);
			this._displayPanel.Location = new System.Drawing.Point(18, 190);
			this._displayPanel.Name = "_displayPanel";
			this._displayPanel.Size = new System.Drawing.Size(684, 118);
			this._displayPanel.TabIndex = 83;
			// 
			// _viewOnlyCheckbox
			// 
			this._viewOnlyCheckbox.AutoSize = true;
			this._viewOnlyCheckbox.Location = new System.Drawing.Point(159, 83);
			this._viewOnlyCheckbox.Name = "_viewOnlyCheckbox";
			this._viewOnlyCheckbox.Size = new System.Drawing.Size(15, 14);
			this._viewOnlyCheckbox.TabIndex = 84;
			this._viewOnlyCheckbox.UseVisualStyleBackColor = true;
			// 
			// _viewOnlyLabel
			// 
			this._viewOnlyLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._viewOnlyLabel.Location = new System.Drawing.Point(31, 83);
			this._viewOnlyLabel.Name = "_viewOnlyLabel";
			this._viewOnlyLabel.Size = new System.Drawing.Size(125, 20);
			this._viewOnlyLabel.TabIndex = 83;
			this._viewOnlyLabel.Text = "View only?:";
			this._viewOnlyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _scaleCheckbox
			// 
			this._scaleCheckbox.AutoSize = true;
			this._scaleCheckbox.Location = new System.Drawing.Point(159, 64);
			this._scaleCheckbox.Name = "_scaleCheckbox";
			this._scaleCheckbox.Size = new System.Drawing.Size(15, 14);
			this._scaleCheckbox.TabIndex = 82;
			this._scaleCheckbox.UseVisualStyleBackColor = true;
			// 
			// _displayUpDown
			// 
			this._displayUpDown.Location = new System.Drawing.Point(159, 37);
			this._displayUpDown.Name = "_displayUpDown";
			this._displayUpDown.Size = new System.Drawing.Size(61, 20);
			this._displayUpDown.TabIndex = 81;
			// 
			// _scaleLabel
			// 
			this._scaleLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
			this._scaleLabel.Location = new System.Drawing.Point(31, 60);
			this._scaleLabel.Name = "_scaleLabel";
			this._scaleLabel.Size = new System.Drawing.Size(125, 20);
			this._scaleLabel.TabIndex = 54;
			this._scaleLabel.Text = "Scale?:";
			this._scaleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// _titleLabel
			// 
			this._titleLabel.AutoSize = true;
			this._titleLabel.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(97)))), ((int)(((byte)(102)))));
			this._titleLabel.Location = new System.Drawing.Point(24, 19);
			this._titleLabel.Name = "_titleLabel";
			this._titleLabel.Size = new System.Drawing.Size(134, 30);
			this._titleLabel.TabIndex = 89;
			this._titleLabel.Text = "VNC Options";
			// 
			// panel1
			// 
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BackColor = System.Drawing.Color.Silver;
			this.panel1.Location = new System.Drawing.Point(29, 62);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(667, 1);
			this.panel1.TabIndex = 90;
			// 
			// VncOptionsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this.ClientSize = new System.Drawing.Size(719, 683);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this._titleLabel);
			this.Controls.Add(this._flowLayoutPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "VncOptionsForm";
			this.Text = "VNC Options";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VncOptionsForm_FormClosing);
			this.Load += new System.EventHandler(this.VncOptionsForm_Load);
			this._flowLayoutPanel.ResumeLayout(false);
			this._generalPanel.ResumeLayout(false);
			this._generalPanel.PerformLayout();
			this._hostPanel.ResumeLayout(false);
			this._hostPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._portUpDown)).EndInit();
			this._displayPanel.ResumeLayout(false);
			this._displayPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._displayUpDown)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _displayLabel;
        private System.Windows.Forms.Label _displayNumberLabel;
        private System.Windows.Forms.FlowLayoutPanel _flowLayoutPanel;
        private System.Windows.Forms.Panel _hostPanel;
        private System.Windows.Forms.Label _portLabel;
        private System.Windows.Forms.Label _hostNameLabel;
        private System.Windows.Forms.TextBox _hostNameTextBox;
		private System.Windows.Forms.Label _hostLabel;
        private System.Windows.Forms.Panel _displayPanel;
        private System.Windows.Forms.Label _scaleLabel;
        private System.Windows.Forms.NumericUpDown _portUpDown;
        private System.Windows.Forms.NumericUpDown _displayUpDown;
        private System.Windows.Forms.CheckBox _scaleCheckbox;
        private System.Windows.Forms.CheckBox _viewOnlyCheckbox;
        private System.Windows.Forms.Label _viewOnlyLabel;
		private System.Windows.Forms.Panel _generalPanel;
        private System.Windows.Forms.Label _passwordLabel;
		private System.Windows.Forms.Label _generalLabel;
        private SecurePasswordTextBox.SecureTextBox _passwordTextBox;
		private System.Windows.Forms.Label _inheritedPasswordLabel;
		private System.Windows.Forms.Label _titleLabel;
		private System.Windows.Forms.Panel panel1;
    }
}