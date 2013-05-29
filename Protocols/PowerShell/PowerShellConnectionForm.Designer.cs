namespace EasyConnect.Protocols.PowerShell
{
	partial class PowerShellConnectionForm
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
			this._terminal = new WalburySoftware.TerminalControl();
			this._statusStrip = new System.Windows.Forms.StatusStrip();
			this._statusStripSpacerLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this._progressLabel = new System.Windows.Forms.ToolStripStatusLabel();
			this._progressBar = new System.Windows.Forms.ToolStripProgressBar();
			this._statusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// _terminal
			// 
			this._terminal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._terminal.AuthType = Poderosa.ConnectionParam.AuthType.Password;
			this._terminal.BackColor = System.Drawing.Color.Navy;
			this._terminal.ForeColor = System.Drawing.Color.White;
			this._terminal.Host = "";
			this._terminal.IdentifyFile = "";
			this._terminal.Location = new System.Drawing.Point(-2, -2);
			this._terminal.Method = WalburySoftware.ConnectionMethod.Telnet;
			this._terminal.Name = "_terminal";
			this._terminal.Password = "";
			this._terminal.Size = new System.Drawing.Size(715, 500);
			this._terminal.TabIndex = 0;
			this._terminal.UserName = "";
			// 
			// _statusStrip
			// 
			this._statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._statusStripSpacerLabel,
            this._progressLabel,
            this._progressBar});
			this._statusStrip.Location = new System.Drawing.Point(0, 496);
			this._statusStrip.Name = "_statusStrip";
			this._statusStrip.Size = new System.Drawing.Size(713, 22);
			this._statusStrip.SizingGrip = false;
			this._statusStrip.TabIndex = 1;
			// 
			// _statusStripSpacerLabel
			// 
			this._statusStripSpacerLabel.Name = "_statusStripSpacerLabel";
			this._statusStripSpacerLabel.Size = new System.Drawing.Size(513, 17);
			this._statusStripSpacerLabel.Spring = true;
			// 
			// _progressLabel
			// 
			this._progressLabel.Name = "_progressLabel";
			this._progressLabel.Size = new System.Drawing.Size(52, 17);
			this._progressLabel.Text = "               ";
			// 
			// _progressBar
			// 
			this._progressBar.Name = "_progressBar";
			this._progressBar.Size = new System.Drawing.Size(100, 16);
			// 
			// PowerShellConnectionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(713, 518);
			this.Controls.Add(this._statusStrip);
			this.Controls.Add(this._terminal);
			this.Name = "PowerShellConnectionForm";
			this.Text = "PowerShellConnectionForm";
			this._statusStrip.ResumeLayout(false);
			this._statusStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private WalburySoftware.TerminalControl _terminal;
		private System.Windows.Forms.StatusStrip _statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel _statusStripSpacerLabel;
		private System.Windows.Forms.ToolStripStatusLabel _progressLabel;
		private System.Windows.Forms.ToolStripProgressBar _progressBar;

	}
}