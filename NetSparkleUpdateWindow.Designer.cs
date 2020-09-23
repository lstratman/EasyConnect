namespace EasyConnect
{
    partial class NetSparkleUpdateWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetSparkleUpdateWindow));
            this._updateAvailableLabel = new System.Windows.Forms.Label();
            this._changeLogHeaderLabel = new System.Windows.Forms.Label();
            this._changeLogText = new System.Windows.Forms.WebBrowser();
            this._skipButton = new System.Windows.Forms.Button();
            this._remindButton = new System.Windows.Forms.Button();
            this._updateButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _updateAvailableLabel
            // 
            this._updateAvailableLabel.AutoSize = true;
            this._updateAvailableLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._updateAvailableLabel.Location = new System.Drawing.Point(13, 13);
            this._updateAvailableLabel.Name = "_updateAvailableLabel";
            this._updateAvailableLabel.Size = new System.Drawing.Size(518, 17);
            this._updateAvailableLabel.TabIndex = 0;
            this._updateAvailableLabel.Text = "Version x.x of EasyConnect is available, you have version x.x.  Would you like to" +
    " update?";
            // 
            // _changeLogHeaderLabel
            // 
            this._changeLogHeaderLabel.AutoSize = true;
            this._changeLogHeaderLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._changeLogHeaderLabel.Location = new System.Drawing.Point(13, 53);
            this._changeLogHeaderLabel.Name = "_changeLogHeaderLabel";
            this._changeLogHeaderLabel.Size = new System.Drawing.Size(85, 17);
            this._changeLogHeaderLabel.TabIndex = 1;
            this._changeLogHeaderLabel.Text = "Change Log:";
            // 
            // _changeLogText
            // 
            this._changeLogText.AllowNavigation = false;
            this._changeLogText.AllowWebBrowserDrop = false;
            this._changeLogText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._changeLogText.Location = new System.Drawing.Point(16, 73);
            this._changeLogText.MinimumSize = new System.Drawing.Size(20, 20);
            this._changeLogText.Name = "_changeLogText";
            this._changeLogText.Size = new System.Drawing.Size(601, 309);
            this._changeLogText.TabIndex = 2;
            // 
            // _skipButton
            // 
            this._skipButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._skipButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._skipButton.Location = new System.Drawing.Point(15, 405);
            this._skipButton.Name = "_skipButton";
            this._skipButton.Size = new System.Drawing.Size(128, 30);
            this._skipButton.TabIndex = 3;
            this._skipButton.Text = "Skip This Update";
            this._skipButton.UseVisualStyleBackColor = true;
            this._skipButton.Click += new System.EventHandler(this._skipButton_Click);
            // 
            // _remindButton
            // 
            this._remindButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._remindButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._remindButton.Location = new System.Drawing.Point(149, 405);
            this._remindButton.Name = "_remindButton";
            this._remindButton.Size = new System.Drawing.Size(128, 30);
            this._remindButton.TabIndex = 4;
            this._remindButton.Text = "Remind Me Later";
            this._remindButton.UseVisualStyleBackColor = true;
            this._remindButton.Click += new System.EventHandler(this._remindButton_Click);
            // 
            // _updateButton
            // 
            this._updateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._updateButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._updateButton.Location = new System.Drawing.Point(490, 405);
            this._updateButton.Name = "_updateButton";
            this._updateButton.Size = new System.Drawing.Size(128, 30);
            this._updateButton.TabIndex = 5;
            this._updateButton.Text = "Update";
            this._updateButton.UseVisualStyleBackColor = true;
            this._updateButton.Click += new System.EventHandler(this._updateButton_Click);
            // 
            // NetSparkleUpdateWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 450);
            this.Controls.Add(this._updateButton);
            this.Controls.Add(this._remindButton);
            this.Controls.Add(this._skipButton);
            this.Controls.Add(this._changeLogText);
            this.Controls.Add(this._changeLogHeaderLabel);
            this.Controls.Add(this._updateAvailableLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NetSparkleUpdateWindow";
            this.Text = "EasyConnect Updater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NetSparkleUpdateWindow_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label _updateAvailableLabel;
        private System.Windows.Forms.Label _changeLogHeaderLabel;
        private System.Windows.Forms.WebBrowser _changeLogText;
        private System.Windows.Forms.Button _skipButton;
        private System.Windows.Forms.Button _remindButton;
        private System.Windows.Forms.Button _updateButton;
    }
}