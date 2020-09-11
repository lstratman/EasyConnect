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
            this._textLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.disconnectedLabel = new System.Windows.Forms.Label();
            this._errorMessageLinkLabel = new System.Windows.Forms.LinkLabel();
            this.reconnectButton = new System.Windows.Forms.Button();
            this.disconnectedPanel.SuspendLayout();
            this._textLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // connectingLabel
            // 
            this.connectingLabel.BackColor = System.Drawing.Color.White;
            this.connectingLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.connectingLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.disconnectedPanel.BackColor = System.Drawing.Color.White;
            this.disconnectedPanel.Controls.Add(this._textLayoutPanel);
            this.disconnectedPanel.Controls.Add(this.reconnectButton);
            this.disconnectedPanel.Location = new System.Drawing.Point(12, 12);
            this.disconnectedPanel.Name = "disconnectedPanel";
            this.disconnectedPanel.Size = new System.Drawing.Size(606, 507);
            this.disconnectedPanel.TabIndex = 1;
            this.disconnectedPanel.Visible = false;
            // 
            // _textLayoutPanel
            // 
            this._textLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this._textLayoutPanel.AutoSize = true;
            this._textLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._textLayoutPanel.Controls.Add(this.disconnectedLabel);
            this._textLayoutPanel.Controls.Add(this._errorMessageLinkLabel);
            this._textLayoutPanel.Location = new System.Drawing.Point(130, 220);
            this._textLayoutPanel.Name = "_textLayoutPanel";
            this._textLayoutPanel.Size = new System.Drawing.Size(337, 36);
            this._textLayoutPanel.TabIndex = 2;
            // 
            // disconnectedLabel
            // 
            this.disconnectedLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.disconnectedLabel.AutoSize = true;
            this.disconnectedLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.disconnectedLabel.ForeColor = System.Drawing.Color.Gray;
            this.disconnectedLabel.Location = new System.Drawing.Point(3, 5);
            this.disconnectedLabel.Name = "disconnectedLabel";
            this.disconnectedLabel.Size = new System.Drawing.Size(132, 25);
            this.disconnectedLabel.TabIndex = 1;
            this.disconnectedLabel.Text = "Disconnected";
            this.disconnectedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // _errorMessageLinkLabel
            // 
            this._errorMessageLinkLabel.AutoSize = true;
            this._errorMessageLinkLabel.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._errorMessageLinkLabel.ForeColor = System.Drawing.Color.Gray;
            this._errorMessageLinkLabel.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this._errorMessageLinkLabel.LinkArea = new System.Windows.Forms.LinkArea(1, 18);
            this._errorMessageLinkLabel.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(119)))), ((int)(((byte)(146)))), ((int)(((byte)(206)))));
            this._errorMessageLinkLabel.Location = new System.Drawing.Point(138, 5);
            this._errorMessageLinkLabel.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this._errorMessageLinkLabel.Name = "_errorMessageLinkLabel";
            this._errorMessageLinkLabel.Size = new System.Drawing.Size(199, 31);
            this._errorMessageLinkLabel.TabIndex = 3;
            this._errorMessageLinkLabel.TabStop = true;
            this._errorMessageLinkLabel.Text = "(view error message)";
            this._errorMessageLinkLabel.UseCompatibleTextRendering = true;
            this._errorMessageLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._errorMessageLinkLabel_LinkClicked);
            // 
            // reconnectButton
            // 
            this.reconnectButton.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reconnectButton.Location = new System.Drawing.Point(260, 258);
            this.reconnectButton.Name = "reconnectButton";
            this.reconnectButton.Size = new System.Drawing.Size(87, 28);
            this.reconnectButton.TabIndex = 0;
            this.reconnectButton.Text = "Reconnect";
            this.reconnectButton.UseVisualStyleBackColor = true;
            this.reconnectButton.Click += new System.EventHandler(this.reconnectButton_Click);
            // 
            // BaseConnectionForm
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(242)))));
            this.ClientSize = new System.Drawing.Size(630, 531);
            this.Controls.Add(this.disconnectedPanel);
            this.Controls.Add(this.connectingLabel);
            this.Name = "BaseConnectionForm";
            this.disconnectedPanel.ResumeLayout(false);
            this.disconnectedPanel.PerformLayout();
            this._textLayoutPanel.ResumeLayout(false);
            this._textLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

	}

        #endregion

		private Label connectingLabel;
		private Panel disconnectedPanel;
		private Button reconnectButton;
		private Label disconnectedLabel;
		private FlowLayoutPanel _textLayoutPanel;
		private LinkLabel _errorMessageLinkLabel;
    }
}