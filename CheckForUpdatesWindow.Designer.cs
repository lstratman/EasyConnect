namespace EasyConnect
{
    partial class CheckForUpdatesWindow
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
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this._statusLabel = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _progressBar
            // 
            this._progressBar.Location = new System.Drawing.Point(15, 34);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(313, 16);
            this._progressBar.TabIndex = 0;
            // 
            // _statusLabel
            // 
            this._statusLabel.AutoSize = true;
            this._statusLabel.Location = new System.Drawing.Point(12, 9);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(117, 13);
            this._statusLabel.TabIndex = 1;
            this._statusLabel.Text = "Checking for updates...";
            // 
            // _cancelButton
            // 
            this._cancelButton.Location = new System.Drawing.Point(252, 57);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new System.Drawing.Size(75, 23);
            this._cancelButton.TabIndex = 2;
            this._cancelButton.Text = "Cancel";
            this._cancelButton.UseVisualStyleBackColor = true;
            this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
            // 
            // _okButton
            // 
            this._okButton.Enabled = false;
            this._okButton.Location = new System.Drawing.Point(171, 57);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new System.Drawing.Size(75, 23);
            this._okButton.TabIndex = 3;
            this._okButton.Text = "OK";
            this._okButton.UseVisualStyleBackColor = true;
            this._okButton.Click += new System.EventHandler(this._okButton_Click);
            // 
            // CheckForUpdatesWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(338, 91);
            this.Controls.Add(this._okButton);
            this.Controls.Add(this._cancelButton);
            this.Controls.Add(this._statusLabel);
            this.Controls.Add(this._progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CheckForUpdatesWindow";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Checking for updates...";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CheckForUpdatesWindow_FormClosing);
            this.Shown += new System.EventHandler(this.CheckForUpdatesWindow_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Label _statusLabel;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _okButton;
    }
}