namespace EasyConnect.Protocols.Vnc
{
    partial class VncSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VncSettingsForm));
            this._viewOnlyCheckbox = new System.Windows.Forms.CheckBox();
            this._displayUpDown = new System.Windows.Forms.NumericUpDown();
            this._rootLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._settingsCard = new EasyConnect.Common.MaterialCard();
            this._settingsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._hostNameLabel = new System.Windows.Forms.Label();
            this._hostNameTextBox = new System.Windows.Forms.TextBox();
            this._divider1 = new System.Windows.Forms.Panel();
            this._passwordLabel = new System.Windows.Forms.Label();
            this._passwordTextBox = new SecurePasswordTextBox.SecureTextBox();
            this._divider3 = new System.Windows.Forms.Panel();
            this._portLabel = new System.Windows.Forms.Label();
            this._portTextBox = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this._displayNumberLabel = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this._viewOnlyLabel = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this._shareClipboardLabel = new System.Windows.Forms.Label();
            this._clipboardCheckbox = new System.Windows.Forms.CheckBox();
            this._pictureQualityLabel = new System.Windows.Forms.Label();
            this._pictureQualityContainer = new System.Windows.Forms.Panel();
            this._pictureQualityLowLabel = new System.Windows.Forms.Label();
            this._pictureQualitySlider = new System.Windows.Forms.TrackBar();
            this._pictureQualityHighLabel = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this._inheritedPasswordTextBox = new System.Windows.Forms.TextBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this._preferredEncodingLabel = new System.Windows.Forms.Label();
            this._preferredEncodingDropdown = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this._displayUpDown)).BeginInit();
            this._rootLayoutPanel.SuspendLayout();
            this._settingsCard.SuspendLayout();
            this._settingsLayoutPanel.SuspendLayout();
            this._pictureQualityContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pictureQualitySlider)).BeginInit();
            this.SuspendLayout();
            // 
            // _viewOnlyCheckbox
            // 
            this._viewOnlyCheckbox.AutoSize = true;
            this._viewOnlyCheckbox.Location = new System.Drawing.Point(652, 299);
            this._viewOnlyCheckbox.Margin = new System.Windows.Forms.Padding(3, 19, 3, 3);
            this._viewOnlyCheckbox.Name = "_viewOnlyCheckbox";
            this._viewOnlyCheckbox.Size = new System.Drawing.Size(15, 14);
            this._viewOnlyCheckbox.TabIndex = 84;
            this._viewOnlyCheckbox.UseVisualStyleBackColor = true;
            // 
            // _displayUpDown
            // 
            this._displayUpDown.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._displayUpDown.Location = new System.Drawing.Point(511, 232);
            this._displayUpDown.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
            this._displayUpDown.Name = "_displayUpDown";
            this._displayUpDown.Size = new System.Drawing.Size(154, 25);
            this._displayUpDown.TabIndex = 81;
            // 
            // _rootLayoutPanel
            // 
            this._rootLayoutPanel.Controls.Add(this._settingsCard);
            this._rootLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rootLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this._rootLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._rootLayoutPanel.Name = "_rootLayoutPanel";
            this._rootLayoutPanel.Size = new System.Drawing.Size(723, 551);
            this._rootLayoutPanel.TabIndex = 121;
            // 
            // _settingsCard
            // 
            this._settingsCard.BackColor = System.Drawing.Color.White;
            this._settingsCard.Controls.Add(this._settingsLayoutPanel);
            this._settingsCard.Location = new System.Drawing.Point(15, 19);
            this._settingsCard.Margin = new System.Windows.Forms.Padding(15, 19, 3, 3);
            this._settingsCard.Name = "_settingsCard";
            this._settingsCard.Size = new System.Drawing.Size(692, 508);
            this._settingsCard.TabIndex = 87;
            // 
            // _settingsLayoutPanel
            // 
            this._settingsLayoutPanel.BackColor = System.Drawing.Color.White;
            this._settingsLayoutPanel.Controls.Add(this._hostNameLabel);
            this._settingsLayoutPanel.Controls.Add(this._hostNameTextBox);
            this._settingsLayoutPanel.Controls.Add(this._divider1);
            this._settingsLayoutPanel.Controls.Add(this._passwordLabel);
            this._settingsLayoutPanel.Controls.Add(this._passwordTextBox);
            this._settingsLayoutPanel.Controls.Add(this._inheritedPasswordTextBox);
            this._settingsLayoutPanel.Controls.Add(this._divider3);
            this._settingsLayoutPanel.Controls.Add(this._portLabel);
            this._settingsLayoutPanel.Controls.Add(this._portTextBox);
            this._settingsLayoutPanel.Controls.Add(this.panel2);
            this._settingsLayoutPanel.Controls.Add(this._displayNumberLabel);
            this._settingsLayoutPanel.Controls.Add(this._displayUpDown);
            this._settingsLayoutPanel.Controls.Add(this.panel3);
            this._settingsLayoutPanel.Controls.Add(this._viewOnlyLabel);
            this._settingsLayoutPanel.Controls.Add(this._viewOnlyCheckbox);
            this._settingsLayoutPanel.Controls.Add(this.panel4);
            this._settingsLayoutPanel.Controls.Add(this._shareClipboardLabel);
            this._settingsLayoutPanel.Controls.Add(this._clipboardCheckbox);
            this._settingsLayoutPanel.Controls.Add(this.panel5);
            this._settingsLayoutPanel.Controls.Add(this._preferredEncodingLabel);
            this._settingsLayoutPanel.Controls.Add(this._preferredEncodingDropdown);
            this._settingsLayoutPanel.Controls.Add(this.panel1);
            this._settingsLayoutPanel.Controls.Add(this._pictureQualityLabel);
            this._settingsLayoutPanel.Controls.Add(this._pictureQualityContainer);
            this._settingsLayoutPanel.Location = new System.Drawing.Point(5, 9);
            this._settingsLayoutPanel.Name = "_settingsLayoutPanel";
            this._settingsLayoutPanel.Size = new System.Drawing.Size(682, 489);
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
            this._hostNameTextBox.TabIndex = 0;
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
            // _passwordLabel
            // 
            this._passwordLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._passwordLabel.Location = new System.Drawing.Point(19, 75);
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
            this._passwordTextBox.Location = new System.Drawing.Point(511, 75);
            this._passwordTextBox.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
            this._passwordTextBox.Name = "_passwordTextBox";
            this._passwordTextBox.PasswordChar = '*';
            this._passwordTextBox.SecureText = secureString1;
            this._passwordTextBox.Size = new System.Drawing.Size(154, 25);
            this._passwordTextBox.TabIndex = 2;
            this._passwordTextBox.Leave += new System.EventHandler(this._passwordTextBox_Leave);
            // 
            // _divider3
            // 
            this._divider3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._divider3.Location = new System.Drawing.Point(0, 156);
            this._divider3.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this._divider3.Name = "_divider3";
            this._divider3.Size = new System.Drawing.Size(682, 1);
            this._divider3.TabIndex = 110;
            // 
            // _portLabel
            // 
            this._portLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._portLabel.Location = new System.Drawing.Point(19, 175);
            this._portLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._portLabel.Name = "_portLabel";
            this._portLabel.Size = new System.Drawing.Size(486, 20);
            this._portLabel.TabIndex = 112;
            this._portLabel.Text = "Port";
            this._portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _portTextBox
            // 
            this._portTextBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._portTextBox.Location = new System.Drawing.Point(511, 172);
            this._portTextBox.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
            this._portTextBox.Name = "_portTextBox";
            this._portTextBox.Size = new System.Drawing.Size(154, 25);
            this._portTextBox.TabIndex = 111;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel2.Location = new System.Drawing.Point(0, 216);
            this.panel2.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(682, 1);
            this.panel2.TabIndex = 113;
            // 
            // _displayNumberLabel
            // 
            this._displayNumberLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._displayNumberLabel.Location = new System.Drawing.Point(19, 235);
            this._displayNumberLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._displayNumberLabel.Name = "_displayNumberLabel";
            this._displayNumberLabel.Size = new System.Drawing.Size(486, 20);
            this._displayNumberLabel.TabIndex = 115;
            this._displayNumberLabel.Text = "Display";
            this._displayNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel3.Location = new System.Drawing.Point(0, 276);
            this.panel3.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(682, 1);
            this.panel3.TabIndex = 116;
            // 
            // _viewOnlyLabel
            // 
            this._viewOnlyLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._viewOnlyLabel.Location = new System.Drawing.Point(19, 295);
            this._viewOnlyLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._viewOnlyLabel.Name = "_viewOnlyLabel";
            this._viewOnlyLabel.Size = new System.Drawing.Size(627, 20);
            this._viewOnlyLabel.TabIndex = 124;
            this._viewOnlyLabel.Text = "View only";
            this._viewOnlyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel4.Location = new System.Drawing.Point(0, 336);
            this.panel4.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(682, 1);
            this.panel4.TabIndex = 125;
            // 
            // _shareClipboardLabel
            // 
            this._shareClipboardLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._shareClipboardLabel.Location = new System.Drawing.Point(19, 355);
            this._shareClipboardLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._shareClipboardLabel.Name = "_shareClipboardLabel";
            this._shareClipboardLabel.Size = new System.Drawing.Size(627, 20);
            this._shareClipboardLabel.TabIndex = 126;
            this._shareClipboardLabel.Text = "Share clipboard";
            this._shareClipboardLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _clipboardCheckbox
            // 
            this._clipboardCheckbox.AutoSize = true;
            this._clipboardCheckbox.Location = new System.Drawing.Point(652, 359);
            this._clipboardCheckbox.Margin = new System.Windows.Forms.Padding(3, 19, 3, 3);
            this._clipboardCheckbox.Name = "_clipboardCheckbox";
            this._clipboardCheckbox.Size = new System.Drawing.Size(15, 14);
            this._clipboardCheckbox.TabIndex = 127;
            this._clipboardCheckbox.UseVisualStyleBackColor = true;
            // 
            // _pictureQualityLabel
            // 
            this._pictureQualityLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._pictureQualityLabel.Location = new System.Drawing.Point(19, 485);
            this._pictureQualityLabel.Margin = new System.Windows.Forms.Padding(19, 25, 3, 18);
            this._pictureQualityLabel.Name = "_pictureQualityLabel";
            this._pictureQualityLabel.Size = new System.Drawing.Size(405, 20);
            this._pictureQualityLabel.TabIndex = 128;
            this._pictureQualityLabel.Text = "Picture quality";
            this._pictureQualityLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _pictureQualityContainer
            // 
            this._pictureQualityContainer.Controls.Add(this._pictureQualityHighLabel);
            this._pictureQualityContainer.Controls.Add(this._pictureQualityLowLabel);
            this._pictureQualityContainer.Controls.Add(this._pictureQualitySlider);
            this._pictureQualityContainer.Location = new System.Drawing.Point(430, 463);
            this._pictureQualityContainer.Name = "_pictureQualityContainer";
            this._pictureQualityContainer.Size = new System.Drawing.Size(245, 61);
            this._pictureQualityContainer.TabIndex = 129;
            // 
            // _pictureQualityLowLabel
            // 
            this._pictureQualityLowLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this._pictureQualityLowLabel.Location = new System.Drawing.Point(9, 40);
            this._pictureQualityLowLabel.Name = "_pictureQualityLowLabel";
            this._pictureQualityLowLabel.Size = new System.Drawing.Size(102, 19);
            this._pictureQualityLowLabel.TabIndex = 80;
            this._pictureQualityLowLabel.Text = "Low";
            // 
            // _pictureQualitySlider
            // 
            this._pictureQualitySlider.Location = new System.Drawing.Point(3, 3);
            this._pictureQualitySlider.Minimum = 1;
            this._pictureQualitySlider.Name = "_pictureQualitySlider";
            this._pictureQualitySlider.Size = new System.Drawing.Size(240, 45);
            this._pictureQualitySlider.TabIndex = 0;
            this._pictureQualitySlider.Value = 1;
            // 
            // _pictureQualityHighLabel
            // 
            this._pictureQualityHighLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this._pictureQualityHighLabel.Location = new System.Drawing.Point(137, 40);
            this._pictureQualityHighLabel.Name = "_pictureQualityHighLabel";
            this._pictureQualityHighLabel.Size = new System.Drawing.Size(102, 19);
            this._pictureQualityHighLabel.TabIndex = 81;
            this._pictureQualityHighLabel.Text = "High";
            this._pictureQualityHighLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel1.Location = new System.Drawing.Point(0, 456);
            this.panel1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(682, 1);
            this.panel1.TabIndex = 130;
            // 
            // _inheritedPasswordTextBox
            // 
            this._inheritedPasswordTextBox.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._inheritedPasswordTextBox.ForeColor = System.Drawing.Color.LightGray;
            this._inheritedPasswordTextBox.Location = new System.Drawing.Point(3, 128);
            this._inheritedPasswordTextBox.Margin = new System.Windows.Forms.Padding(3, 15, 3, 0);
            this._inheritedPasswordTextBox.Name = "_inheritedPasswordTextBox";
            this._inheritedPasswordTextBox.Size = new System.Drawing.Size(154, 25);
            this._inheritedPasswordTextBox.TabIndex = 3;
            this._inheritedPasswordTextBox.Text = "Inheriting password";
            this._inheritedPasswordTextBox.Visible = false;
            this._inheritedPasswordTextBox.Enter += new System.EventHandler(this._inheritedPasswordTextBox_Enter);
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.panel5.Location = new System.Drawing.Point(0, 396);
            this.panel5.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(682, 1);
            this.panel5.TabIndex = 133;
            // 
            // _preferredEncodingLabel
            // 
            this._preferredEncodingLabel.BackColor = System.Drawing.Color.White;
            this._preferredEncodingLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._preferredEncodingLabel.Location = new System.Drawing.Point(19, 415);
            this._preferredEncodingLabel.Margin = new System.Windows.Forms.Padding(19, 15, 3, 18);
            this._preferredEncodingLabel.Name = "_preferredEncodingLabel";
            this._preferredEncodingLabel.Size = new System.Drawing.Size(456, 20);
            this._preferredEncodingLabel.TabIndex = 132;
            this._preferredEncodingLabel.Text = "Preferred encoding";
            this._preferredEncodingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _preferredEncodingDropdown
            // 
            this._preferredEncodingDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._preferredEncodingDropdown.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._preferredEncodingDropdown.FormattingEnabled = true;
            this._preferredEncodingDropdown.Items.AddRange(new object[] {
            "",
            "Raw",
            "CopyRect",
            "ZLib",
            "ZRLE",
            "Tight"});
            this._preferredEncodingDropdown.Location = new System.Drawing.Point(481, 415);
            this._preferredEncodingDropdown.Margin = new System.Windows.Forms.Padding(3, 15, 3, 3);
            this._preferredEncodingDropdown.Name = "_preferredEncodingDropdown";
            this._preferredEncodingDropdown.Size = new System.Drawing.Size(184, 25);
            this._preferredEncodingDropdown.TabIndex = 131;
            // 
            // VncSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(723, 551);
            this.Controls.Add(this._rootLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VncSettingsForm";
            this.Text = "VNC";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VncSettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.VncSettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this._displayUpDown)).EndInit();
            this._rootLayoutPanel.ResumeLayout(false);
            this._settingsCard.ResumeLayout(false);
            this._settingsLayoutPanel.ResumeLayout(false);
            this._settingsLayoutPanel.PerformLayout();
            this._pictureQualityContainer.ResumeLayout(false);
            this._pictureQualityContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pictureQualitySlider)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.NumericUpDown _displayUpDown;
        private System.Windows.Forms.CheckBox _viewOnlyCheckbox;
        private System.Windows.Forms.FlowLayoutPanel _rootLayoutPanel;
        private Common.MaterialCard _settingsCard;
        private System.Windows.Forms.FlowLayoutPanel _settingsLayoutPanel;
        private System.Windows.Forms.Label _hostNameLabel;
        private System.Windows.Forms.TextBox _hostNameTextBox;
        private System.Windows.Forms.Panel _divider1;
        private System.Windows.Forms.Label _passwordLabel;
        private SecurePasswordTextBox.SecureTextBox _passwordTextBox;
        private System.Windows.Forms.Panel _divider3;
        private System.Windows.Forms.Label _portLabel;
        private System.Windows.Forms.TextBox _portTextBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label _displayNumberLabel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label _viewOnlyLabel;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label _shareClipboardLabel;
        private System.Windows.Forms.CheckBox _clipboardCheckbox;
        private System.Windows.Forms.Label _pictureQualityLabel;
        private System.Windows.Forms.Panel _pictureQualityContainer;
        private System.Windows.Forms.Label _pictureQualityHighLabel;
        private System.Windows.Forms.Label _pictureQualityLowLabel;
        private System.Windows.Forms.TrackBar _pictureQualitySlider;
        private System.Windows.Forms.TextBox _inheritedPasswordTextBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label _preferredEncodingLabel;
        private System.Windows.Forms.ComboBox _preferredEncodingDropdown;
    }
}