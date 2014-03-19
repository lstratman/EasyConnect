using System.Drawing;
using SecurePasswordTextBox;

namespace EasyConnect
{
    partial class OptionsWindow
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsWindow));
			this._sidebarContainer = new System.Windows.Forms.Panel();
			this._sidebarFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
			this._optionsLabel = new System.Windows.Forms.Label();
			this._topBorderPanel = new System.Windows.Forms.Panel();
			this._containerPanel = new System.Windows.Forms.Panel();
			this._sidebarContainer.SuspendLayout();
			this._sidebarFlowLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// _sidebarContainer
			// 
			this._sidebarContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this._sidebarContainer.BackColor = System.Drawing.Color.White;
			this._sidebarContainer.Controls.Add(this._sidebarFlowLayoutPanel);
			this._sidebarContainer.Location = new System.Drawing.Point(0, 0);
			this._sidebarContainer.Margin = new System.Windows.Forms.Padding(0);
			this._sidebarContainer.Name = "_sidebarContainer";
			this._sidebarContainer.Size = new System.Drawing.Size(217, 682);
			this._sidebarContainer.TabIndex = 0;
			// 
			// _sidebarFlowLayoutPanel
			// 
			this._sidebarFlowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this._sidebarFlowLayoutPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this._sidebarFlowLayoutPanel.Controls.Add(this._optionsLabel);
			this._sidebarFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this._sidebarFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this._sidebarFlowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
			this._sidebarFlowLayoutPanel.Name = "_sidebarFlowLayoutPanel";
			this._sidebarFlowLayoutPanel.Size = new System.Drawing.Size(217, 682);
			this._sidebarFlowLayoutPanel.TabIndex = 2;
			// 
			// _optionsLabel
			// 
			this._optionsLabel.BackColor = System.Drawing.Color.Transparent;
			this._optionsLabel.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._optionsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(97)))), ((int)(((byte)(102)))));
			this._optionsLabel.Location = new System.Drawing.Point(0, 0);
			this._optionsLabel.Margin = new System.Windows.Forms.Padding(0);
			this._optionsLabel.Name = "_optionsLabel";
			this._optionsLabel.Padding = new System.Windows.Forms.Padding(20, 21, 12, 0);
			this._optionsLabel.Size = new System.Drawing.Size(214, 75);
			this._optionsLabel.TabIndex = 0;
			this._optionsLabel.Text = "EasyConnect";
			// 
			// _topBorderPanel
			// 
			this._topBorderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._topBorderPanel.BackColor = System.Drawing.Color.Silver;
			this._topBorderPanel.Location = new System.Drawing.Point(0, 0);
			this._topBorderPanel.Name = "_topBorderPanel";
			this._topBorderPanel.Size = new System.Drawing.Size(740, 1);
			this._topBorderPanel.TabIndex = 1;
			// 
			// _containerPanel
			// 
			this._containerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._containerPanel.AutoScroll = true;
			this._containerPanel.Location = new System.Drawing.Point(217, 0);
			this._containerPanel.Name = "_containerPanel";
			this._containerPanel.Size = new System.Drawing.Size(524, 682);
			this._containerPanel.TabIndex = 2;
			// 
			// OptionsWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(249)))), ((int)(((byte)(249)))), ((int)(((byte)(249)))));
			this.ClientSize = new System.Drawing.Size(741, 682);
			this.Controls.Add(this._containerPanel);
			this.Controls.Add(this._topBorderPanel);
			this.Controls.Add(this._sidebarContainer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "OptionsWindow";
			this.Text = "Options";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptionsWindow_FormClosing);
			this.Load += new System.EventHandler(this.OptionsWindow_Load);
			this._sidebarContainer.ResumeLayout(false);
			this._sidebarFlowLayoutPanel.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _sidebarContainer;
        private System.Windows.Forms.Label _optionsLabel;
        private System.Windows.Forms.Panel _topBorderPanel;
        private System.Windows.Forms.Panel _containerPanel;
        private System.Windows.Forms.FlowLayoutPanel _sidebarFlowLayoutPanel;
    }
}