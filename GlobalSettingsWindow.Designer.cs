namespace EasyConnect
{
    partial class GlobalSettingsWindow
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
            this._defaultProtocolLabel = new System.Windows.Forms.Label();
            this._defaultProtocolDropdown = new System.Windows.Forms.ComboBox();
            this._card = new EasyConnect.Common.MaterialCard();
            this._divider1 = new System.Windows.Forms.Panel();
            this._divider2 = new System.Windows.Forms.Panel();
            this._divider3 = new System.Windows.Forms.Panel();
            this._autoHideLabel = new System.Windows.Forms.Label();
            this._autoHideCheckbox = new System.Windows.Forms.CheckBox();
            this._encryptionTypeLabel = new System.Windows.Forms.Label();
            this._encryptionTypeDropdown = new System.Windows.Forms.ComboBox();
            this._enableAeroPeekLabel = new System.Windows.Forms.Label();
            this._enableAeroPeekCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // _defaultProtocolLabel
            // 
            this._defaultProtocolLabel.BackColor = System.Drawing.Color.White;
            this._defaultProtocolLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._defaultProtocolLabel.Location = new System.Drawing.Point(36, 45);
            this._defaultProtocolLabel.Name = "_defaultProtocolLabel";
            this._defaultProtocolLabel.Size = new System.Drawing.Size(174, 20);
            this._defaultProtocolLabel.TabIndex = 65;
            this._defaultProtocolLabel.Text = "Default protocol";
            this._defaultProtocolLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _defaultProtocolDropdown
            // 
            this._defaultProtocolDropdown.BackColor = System.Drawing.Color.White;
            this._defaultProtocolDropdown.DisplayMember = "ProtocolTitle";
            this._defaultProtocolDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._defaultProtocolDropdown.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._defaultProtocolDropdown.ForeColor = System.Drawing.SystemColors.ControlText;
            this._defaultProtocolDropdown.FormattingEnabled = true;
            this._defaultProtocolDropdown.Location = new System.Drawing.Point(499, 44);
            this._defaultProtocolDropdown.Name = "_defaultProtocolDropdown";
            this._defaultProtocolDropdown.Size = new System.Drawing.Size(182, 25);
            this._defaultProtocolDropdown.TabIndex = 68;
            // 
            // _card
            // 
            this._card.BackColor = System.Drawing.Color.White;
            this._card.Location = new System.Drawing.Point(12, 22);
            this._card.Name = "_card";
            this._card.Size = new System.Drawing.Size(692, 249);
            this._card.TabIndex = 77;
            // 
            // _divider1
            // 
            this._divider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._divider1.Location = new System.Drawing.Point(17, 86);
            this._divider1.Name = "_divider1";
            this._divider1.Size = new System.Drawing.Size(682, 1);
            this._divider1.TabIndex = 78;
            // 
            // _divider2
            // 
            this._divider2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._divider2.Location = new System.Drawing.Point(17, 146);
            this._divider2.Name = "_divider2";
            this._divider2.Size = new System.Drawing.Size(682, 1);
            this._divider2.TabIndex = 79;
            // 
            // _divider3
            // 
            this._divider3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this._divider3.Location = new System.Drawing.Point(17, 206);
            this._divider3.Name = "_divider3";
            this._divider3.Size = new System.Drawing.Size(682, 1);
            this._divider3.TabIndex = 80;
            // 
            // _autoHideLabel
            // 
            this._autoHideLabel.BackColor = System.Drawing.Color.White;
            this._autoHideLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._autoHideLabel.Location = new System.Drawing.Point(36, 105);
            this._autoHideLabel.Name = "_autoHideLabel";
            this._autoHideLabel.Size = new System.Drawing.Size(198, 20);
            this._autoHideLabel.TabIndex = 82;
            this._autoHideLabel.Text = "Auto-hide connection toolbar?";
            this._autoHideLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _autoHideCheckbox
            // 
            this._autoHideCheckbox.AutoSize = true;
            this._autoHideCheckbox.ForeColor = System.Drawing.SystemColors.ControlText;
            this._autoHideCheckbox.Location = new System.Drawing.Point(666, 111);
            this._autoHideCheckbox.Name = "_autoHideCheckbox";
            this._autoHideCheckbox.Size = new System.Drawing.Size(15, 14);
            this._autoHideCheckbox.TabIndex = 81;
            this._autoHideCheckbox.UseVisualStyleBackColor = true;
            // 
            // _encryptionTypeLabel
            // 
            this._encryptionTypeLabel.BackColor = System.Drawing.Color.White;
            this._encryptionTypeLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._encryptionTypeLabel.Location = new System.Drawing.Point(36, 166);
            this._encryptionTypeLabel.Name = "_encryptionTypeLabel";
            this._encryptionTypeLabel.Size = new System.Drawing.Size(173, 20);
            this._encryptionTypeLabel.TabIndex = 84;
            this._encryptionTypeLabel.Text = "Encryption type";
            this._encryptionTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _encryptionTypeDropdown
            // 
            this._encryptionTypeDropdown.BackColor = System.Drawing.SystemColors.Control;
            this._encryptionTypeDropdown.DisplayMember = "Text";
            this._encryptionTypeDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._encryptionTypeDropdown.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._encryptionTypeDropdown.ForeColor = System.Drawing.SystemColors.ControlText;
            this._encryptionTypeDropdown.FormattingEnabled = true;
            this._encryptionTypeDropdown.Location = new System.Drawing.Point(499, 165);
            this._encryptionTypeDropdown.Name = "_encryptionTypeDropdown";
            this._encryptionTypeDropdown.Size = new System.Drawing.Size(182, 25);
            this._encryptionTypeDropdown.TabIndex = 83;
            this._encryptionTypeDropdown.ValueMember = "Value";
            // 
            // _enableAeroPeekLabel
            // 
            this._enableAeroPeekLabel.BackColor = System.Drawing.Color.White;
            this._enableAeroPeekLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._enableAeroPeekLabel.Location = new System.Drawing.Point(36, 226);
            this._enableAeroPeekLabel.Name = "_enableAeroPeekLabel";
            this._enableAeroPeekLabel.Size = new System.Drawing.Size(174, 20);
            this._enableAeroPeekLabel.TabIndex = 86;
            this._enableAeroPeekLabel.Text = "Enable AeroPeek?";
            this._enableAeroPeekLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _enableAeroPeekCheckbox
            // 
            this._enableAeroPeekCheckbox.AutoSize = true;
            this._enableAeroPeekCheckbox.ForeColor = System.Drawing.SystemColors.ControlText;
            this._enableAeroPeekCheckbox.Location = new System.Drawing.Point(666, 230);
            this._enableAeroPeekCheckbox.Name = "_enableAeroPeekCheckbox";
            this._enableAeroPeekCheckbox.Size = new System.Drawing.Size(15, 14);
            this._enableAeroPeekCheckbox.TabIndex = 85;
            this._enableAeroPeekCheckbox.UseVisualStyleBackColor = true;
            // 
            // GlobalSettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(717, 311);
            this.Controls.Add(this._enableAeroPeekLabel);
            this.Controls.Add(this._enableAeroPeekCheckbox);
            this.Controls.Add(this._encryptionTypeLabel);
            this.Controls.Add(this._encryptionTypeDropdown);
            this.Controls.Add(this._autoHideLabel);
            this.Controls.Add(this._autoHideCheckbox);
            this.Controls.Add(this._divider3);
            this.Controls.Add(this._divider2);
            this.Controls.Add(this._divider1);
            this.Controls.Add(this._defaultProtocolLabel);
            this.Controls.Add(this._defaultProtocolDropdown);
            this.Controls.Add(this._card);
            this.Name = "GlobalSettingsWindow";
            this.Text = "Global Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GlobalSettingsWindow_FormClosing);
            this.Load += new System.EventHandler(this.GlobalSettingsWindow_Load);
            this.Shown += new System.EventHandler(this.GlobalSettingsWindow_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _defaultProtocolLabel;
        private System.Windows.Forms.ComboBox _defaultProtocolDropdown;
        private Common.MaterialCard _card;
        private System.Windows.Forms.Panel _divider1;
        private System.Windows.Forms.Panel _divider2;
        private System.Windows.Forms.Panel _divider3;
        private System.Windows.Forms.Label _autoHideLabel;
        private System.Windows.Forms.CheckBox _autoHideCheckbox;
        private System.Windows.Forms.Label _encryptionTypeLabel;
        private System.Windows.Forms.ComboBox _encryptionTypeDropdown;
        private System.Windows.Forms.Label _enableAeroPeekLabel;
        private System.Windows.Forms.CheckBox _enableAeroPeekCheckbox;
    }
}