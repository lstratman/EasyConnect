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
			this.SuspendLayout();
			// 
			// _terminal
			// 
			this._terminal.AuthType = Poderosa.ConnectionParam.AuthType.Password;
			this._terminal.Dock = System.Windows.Forms.DockStyle.Fill;
			this._terminal.Host = "";
			this._terminal.IdentifyFile = "";
			this._terminal.Location = new System.Drawing.Point(0, 0);
			this._terminal.Method = WalburySoftware.ConnectionMethod.Telnet;
			this._terminal.Name = "_terminal";
			this._terminal.Password = "";
			this._terminal.Size = new System.Drawing.Size(713, 518);
			this._terminal.TabIndex = 0;
			this._terminal.UserName = "";
			// 
			// PowerShellConnectionForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(713, 518);
			this.Controls.Add(this._terminal);
			this.Name = "PowerShellConnectionForm";
			this.Text = "PowerShellConnectionForm";
			this.ResumeLayout(false);

		}

		#endregion

		private WalburySoftware.TerminalControl _terminal;

	}
}