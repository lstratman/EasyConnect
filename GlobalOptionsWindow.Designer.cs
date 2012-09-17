namespace EasyConnect
{
    partial class GlobalOptionsWindow
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
            this._generalLabel = new System.Windows.Forms.Label();
            this._defaultProtocolDropdown = new System.Windows.Forms.ComboBox();
            this._autoHideCheckbox = new System.Windows.Forms.CheckBox();
            this._autoHideLabel = new System.Windows.Forms.Label();
            this._encryptionTypeDropdown = new System.Windows.Forms.ComboBox();
            this._encryptionTypeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // _defaultProtocolLabel
            // 
            this._defaultProtocolLabel.Location = new System.Drawing.Point(109, 27);
            this._defaultProtocolLabel.Name = "_defaultProtocolLabel";
            this._defaultProtocolLabel.Size = new System.Drawing.Size(150, 20);
            this._defaultProtocolLabel.TabIndex = 65;
            this._defaultProtocolLabel.Text = "Default protocol:";
            this._defaultProtocolLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _generalLabel
            // 
            this._generalLabel.AutoSize = true;
            this._generalLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._generalLabel.Location = new System.Drawing.Point(26, 29);
            this._generalLabel.Name = "_generalLabel";
            this._generalLabel.Size = new System.Drawing.Size(63, 16);
            this._generalLabel.TabIndex = 67;
            this._generalLabel.Text = "General";
            // 
            // _defaultProtocolDropdown
            // 
            this._defaultProtocolDropdown.DisplayMember = "ProtocolTitle";
            this._defaultProtocolDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._defaultProtocolDropdown.FormattingEnabled = true;
            this._defaultProtocolDropdown.Location = new System.Drawing.Point(267, 29);
            this._defaultProtocolDropdown.Name = "_defaultProtocolDropdown";
            this._defaultProtocolDropdown.Size = new System.Drawing.Size(182, 21);
            this._defaultProtocolDropdown.TabIndex = 68;
            // 
            // _autoHideCheckbox
            // 
            this._autoHideCheckbox.AutoSize = true;
            this._autoHideCheckbox.Location = new System.Drawing.Point(267, 57);
            this._autoHideCheckbox.Name = "_autoHideCheckbox";
            this._autoHideCheckbox.Size = new System.Drawing.Size(15, 14);
            this._autoHideCheckbox.TabIndex = 69;
            this._autoHideCheckbox.UseVisualStyleBackColor = true;
            // 
            // _autoHideLabel
            // 
            this._autoHideLabel.AutoSize = true;
            this._autoHideLabel.Location = new System.Drawing.Point(107, 57);
            this._autoHideLabel.Name = "_autoHideLabel";
            this._autoHideLabel.Size = new System.Drawing.Size(152, 13);
            this._autoHideLabel.TabIndex = 70;
            this._autoHideLabel.Text = "Auto-hide connection toolbar?:";
            // 
            // _encryptionTypeDropdown
            // 
            this._encryptionTypeDropdown.DisplayMember = "Text";
            this._encryptionTypeDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._encryptionTypeDropdown.FormattingEnabled = true;
            this._encryptionTypeDropdown.Location = new System.Drawing.Point(267, 78);
            this._encryptionTypeDropdown.Name = "_encryptionTypeDropdown";
            this._encryptionTypeDropdown.Size = new System.Drawing.Size(182, 21);
            this._encryptionTypeDropdown.TabIndex = 71;
            this._encryptionTypeDropdown.ValueMember = "Value";
            this._encryptionTypeDropdown.SelectedIndexChanged += new System.EventHandler(this._encryptionTypeDropdown_SelectedIndexChanged);
            // 
            // _encryptionTypeLabel
            // 
            this._encryptionTypeLabel.AutoSize = true;
            this._encryptionTypeLabel.Location = new System.Drawing.Point(176, 81);
            this._encryptionTypeLabel.Name = "_encryptionTypeLabel";
            this._encryptionTypeLabel.Size = new System.Drawing.Size(83, 13);
            this._encryptionTypeLabel.TabIndex = 72;
            this._encryptionTypeLabel.Text = "Encryption type:";
            // 
            // GlobalOptionsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(597, 384);
            this.Controls.Add(this._encryptionTypeLabel);
            this.Controls.Add(this._encryptionTypeDropdown);
            this.Controls.Add(this._autoHideLabel);
            this.Controls.Add(this._autoHideCheckbox);
            this.Controls.Add(this._defaultProtocolLabel);
            this.Controls.Add(this._generalLabel);
            this.Controls.Add(this._defaultProtocolDropdown);
            this.Name = "GlobalOptionsWindow";
            this.Text = "Global Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GlobalOptionsWindow_FormClosing);
            this.Shown += new System.EventHandler(this.GlobalOptionsWindow_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _defaultProtocolLabel;
        private System.Windows.Forms.Label _generalLabel;
        private System.Windows.Forms.ComboBox _defaultProtocolDropdown;
        private System.Windows.Forms.CheckBox _autoHideCheckbox;
        private System.Windows.Forms.Label _autoHideLabel;
        private System.Windows.Forms.ComboBox _encryptionTypeDropdown;
        private System.Windows.Forms.Label _encryptionTypeLabel;
    }
}