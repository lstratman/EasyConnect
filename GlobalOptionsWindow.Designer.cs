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
            // GlobalOptionsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(597, 384);
            this.Controls.Add(this._defaultProtocolLabel);
            this.Controls.Add(this._generalLabel);
            this.Controls.Add(this._defaultProtocolDropdown);
            this.Name = "GlobalOptionsWindow";
            this.Text = "Global Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GlobalOptionsWindow_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _defaultProtocolLabel;
        private System.Windows.Forms.Label _generalLabel;
        private System.Windows.Forms.ComboBox _defaultProtocolDropdown;
    }
}