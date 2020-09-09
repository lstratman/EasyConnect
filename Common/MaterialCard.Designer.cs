namespace EasyConnect.Common
{
    partial class MaterialCard
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MaterialCard));
            this._topLeftPanel = new System.Windows.Forms.Panel();
            this._topRightPanel = new System.Windows.Forms.Panel();
            this._panelTop = new System.Windows.Forms.Panel();
            this._panelBottom = new System.Windows.Forms.Panel();
            this._panelBottomRight = new System.Windows.Forms.Panel();
            this._panelBottomLeft = new System.Windows.Forms.Panel();
            this._panelLeft = new System.Windows.Forms.Panel();
            this._panelRight = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // _topLeftPanel
            // 
            this._topLeftPanel.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelTopLeft;
            this._topLeftPanel.Location = new System.Drawing.Point(0, 0);
            this._topLeftPanel.Name = "_topLeftPanel";
            this._topLeftPanel.Size = new System.Drawing.Size(11, 11);
            this._topLeftPanel.TabIndex = 0;
            // 
            // _topRightPanel
            // 
            this._topRightPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._topRightPanel.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelTopRight;
            this._topRightPanel.Location = new System.Drawing.Point(289, 0);
            this._topRightPanel.Name = "_topRightPanel";
            this._topRightPanel.Size = new System.Drawing.Size(11, 11);
            this._topRightPanel.TabIndex = 1;
            // 
            // _panelTop
            // 
            this._panelTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._panelTop.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelTop;
            this._panelTop.Location = new System.Drawing.Point(11, 0);
            this._panelTop.Name = "_panelTop";
            this._panelTop.Size = new System.Drawing.Size(278, 11);
            this._panelTop.TabIndex = 2;
            // 
            // _panelBottom
            // 
            this._panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._panelBottom.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelBottom;
            this._panelBottom.Location = new System.Drawing.Point(11, 289);
            this._panelBottom.Name = "_panelBottom";
            this._panelBottom.Size = new System.Drawing.Size(278, 11);
            this._panelBottom.TabIndex = 5;
            // 
            // _panelBottomRight
            // 
            this._panelBottomRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._panelBottomRight.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelBottomRight;
            this._panelBottomRight.Location = new System.Drawing.Point(289, 289);
            this._panelBottomRight.Name = "_panelBottomRight";
            this._panelBottomRight.Size = new System.Drawing.Size(11, 11);
            this._panelBottomRight.TabIndex = 4;
            // 
            // _panelBottomLeft
            // 
            this._panelBottomLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._panelBottomLeft.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelBottomLeft;
            this._panelBottomLeft.Location = new System.Drawing.Point(0, 289);
            this._panelBottomLeft.Name = "_panelBottomLeft";
            this._panelBottomLeft.Size = new System.Drawing.Size(11, 11);
            this._panelBottomLeft.TabIndex = 3;
            // 
            // _panelLeft
            // 
            this._panelLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this._panelLeft.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_panelLeft.BackgroundImage")));
            this._panelLeft.Location = new System.Drawing.Point(0, 11);
            this._panelLeft.Name = "_panelLeft";
            this._panelLeft.Size = new System.Drawing.Size(11, 278);
            this._panelLeft.TabIndex = 6;
            // 
            // _panelRight
            // 
            this._panelRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._panelRight.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("_panelRight.BackgroundImage")));
            this._panelRight.Location = new System.Drawing.Point(289, 11);
            this._panelRight.Name = "_panelRight";
            this._panelRight.Size = new System.Drawing.Size(11, 278);
            this._panelRight.TabIndex = 7;
            // 
            // MaterialCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this._panelRight);
            this.Controls.Add(this._panelLeft);
            this.Controls.Add(this._panelBottom);
            this.Controls.Add(this._panelBottomRight);
            this.Controls.Add(this._panelTop);
            this.Controls.Add(this._panelBottomLeft);
            this.Controls.Add(this._topRightPanel);
            this.Controls.Add(this._topLeftPanel);
            this.Name = "MaterialCard";
            this.Size = new System.Drawing.Size(300, 300);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _topLeftPanel;
        private System.Windows.Forms.Panel _topRightPanel;
        private System.Windows.Forms.Panel _panelTop;
        private System.Windows.Forms.Panel _panelBottom;
        private System.Windows.Forms.Panel _panelBottomRight;
        private System.Windows.Forms.Panel _panelBottomLeft;
        private System.Windows.Forms.Panel _panelLeft;
        private System.Windows.Forms.Panel _panelRight;
    }
}
