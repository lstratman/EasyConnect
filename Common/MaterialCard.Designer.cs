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
            this._panelTopLeft = new System.Windows.Forms.Panel();
            this._panelTopRight = new System.Windows.Forms.Panel();
            this._panelTop = new System.Windows.Forms.Panel();
            this._panelBottom = new System.Windows.Forms.Panel();
            this._panelBottomRight = new System.Windows.Forms.Panel();
            this._panelBottomLeft = new System.Windows.Forms.Panel();
            this._panelLeft = new System.Windows.Forms.Panel();
            this._panelRight = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // _panelTopLeft
            // 
            this._panelTopLeft.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelTopLeft;
            this._panelTopLeft.Location = new System.Drawing.Point(0, 0);
            this._panelTopLeft.Name = "_panelTopLeft";
            this._panelTopLeft.Size = new System.Drawing.Size(11, 11);
            this._panelTopLeft.TabIndex = 0;
            // 
            // _panelTopRight
            // 
            this._panelTopRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._panelTopRight.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelTopRight;
            this._panelTopRight.Location = new System.Drawing.Point(289, 0);
            this._panelTopRight.Name = "_panelTopRight";
            this._panelTopRight.Size = new System.Drawing.Size(11, 11);
            this._panelTopRight.TabIndex = 1;
            // 
            // _panelTop
            // 
            this._panelTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._panelTop.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelTop;
            this._panelTop.Location = new System.Drawing.Point(11, 0);
            this._panelTop.Name = "_panelTop";
            this._panelTop.Size = new System.Drawing.Size(278, 5);
            this._panelTop.TabIndex = 2;
            // 
            // _panelBottom
            // 
            this._panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._panelBottom.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelBottom;
            this._panelBottom.Location = new System.Drawing.Point(11, 295);
            this._panelBottom.Name = "_panelBottom";
            this._panelBottom.Size = new System.Drawing.Size(278, 5);
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
            this._panelLeft.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelLeft;
            this._panelLeft.Location = new System.Drawing.Point(0, 11);
            this._panelLeft.Name = "_panelLeft";
            this._panelLeft.Size = new System.Drawing.Size(5, 278);
            this._panelLeft.TabIndex = 6;
            // 
            // _panelRight
            // 
            this._panelRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._panelRight.BackgroundImage = global::EasyConnect.Common.Properties.Resources.PanelRight;
            this._panelRight.Location = new System.Drawing.Point(296, 11);
            this._panelRight.Name = "_panelRight";
            this._panelRight.Size = new System.Drawing.Size(5, 278);
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
            this.Controls.Add(this._panelTopRight);
            this.Controls.Add(this._panelTopLeft);
            this.Name = "MaterialCard";
            this.Size = new System.Drawing.Size(300, 300);
            this.Resize += new System.EventHandler(this.MaterialCard_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _panelTopLeft;
        private System.Windows.Forms.Panel _panelTopRight;
        private System.Windows.Forms.Panel _panelTop;
        private System.Windows.Forms.Panel _panelBottom;
        private System.Windows.Forms.Panel _panelBottomRight;
        private System.Windows.Forms.Panel _panelBottomLeft;
        private System.Windows.Forms.Panel _panelLeft;
        private System.Windows.Forms.Panel _panelRight;
    }
}
