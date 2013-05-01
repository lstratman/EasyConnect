using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    partial class BaseConnectionForm
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
			this.connectingLabel = new System.Windows.Forms.Label();
			this.disconnectedPanel = new System.Windows.Forms.Panel();
			this.disconnectedLabel = new System.Windows.Forms.Label();
			this.reconnectButton = new System.Windows.Forms.Button();
			this.disconnectedPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// connectingLabel
			// 
			this.connectingLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.connectingLabel.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.connectingLabel.ForeColor = System.Drawing.Color.Gray;
			this.connectingLabel.Location = new System.Drawing.Point(0, 0);
			this.connectingLabel.Name = "connectingLabel";
			this.connectingLabel.Size = new System.Drawing.Size(630, 531);
			this.connectingLabel.TabIndex = 0;
			this.connectingLabel.Text = "Connecting...";
			this.connectingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// disconnectedPanel
			// 
			this.disconnectedPanel.BackColor = System.Drawing.Color.Transparent;
			this.disconnectedPanel.Controls.Add(this.disconnectedLabel);
			this.disconnectedPanel.Controls.Add(this.reconnectButton);
			this.disconnectedPanel.Location = new System.Drawing.Point(12, 12);
			this.disconnectedPanel.Name = "disconnectedPanel";
			this.disconnectedPanel.Size = new System.Drawing.Size(606, 507);
			this.disconnectedPanel.TabIndex = 1;
			this.disconnectedPanel.Visible = false;
			// 
			// disconnectedLabel
			// 
			this.disconnectedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.disconnectedLabel.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.disconnectedLabel.ForeColor = System.Drawing.Color.Gray;
			this.disconnectedLabel.Location = new System.Drawing.Point(3, 229);
			this.disconnectedLabel.Name = "disconnectedLabel";
			this.disconnectedLabel.Size = new System.Drawing.Size(600, 22);
			this.disconnectedLabel.TabIndex = 1;
			this.disconnectedLabel.Text = "Disconnected";
			this.disconnectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// reconnectButton
			// 
			this.reconnectButton.Location = new System.Drawing.Point(266, 254);
			this.reconnectButton.Name = "reconnectButton";
			this.reconnectButton.Size = new System.Drawing.Size(75, 23);
			this.reconnectButton.TabIndex = 0;
			this.reconnectButton.Text = "Reconnect";
			this.reconnectButton.UseVisualStyleBackColor = true;
			this.reconnectButton.Click += new System.EventHandler(this.reconnectButton_Click);
			// 
			// BaseConnectionForm
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this.ClientSize = new System.Drawing.Size(630, 531);
			this.Controls.Add(this.disconnectedPanel);
			this.Controls.Add(this.connectingLabel);
			this.Name = "BaseConnectionForm";
			this.disconnectedPanel.ResumeLayout(false);
			this.ResumeLayout(false);

	}

        #endregion

		private Label connectingLabel;
		private Panel disconnectedPanel;
		private Button reconnectButton;
		private Label disconnectedLabel;
    }
}