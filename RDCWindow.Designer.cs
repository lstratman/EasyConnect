namespace EasyConnect
{
    partial class RDCWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RDCWindow));
            this._rdcWindow = new AxMSTSCLib.AxMsRdpClient2();
            this.borderLeft = new System.Windows.Forms.Panel();
            this.toolbarBackground = new System.Windows.Forms.Panel();
            this.urlTextBox = new System.Windows.Forms.TextBox();
            this.urlBorder = new System.Windows.Forms.Panel();
            this.urlBackground = new System.Windows.Forms.Panel();
            this.borderRight = new System.Windows.Forms.Panel();
            this.forwardButton = new System.Windows.Forms.PictureBox();
            this.backButton = new System.Windows.Forms.PictureBox();
            this.borderBottom = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this._rdcWindow)).BeginInit();
            this.toolbarBackground.SuspendLayout();
            this.urlBorder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.forwardButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.backButton)).BeginInit();
            this.SuspendLayout();
            // 
            // _rdcWindow
            // 
            this._rdcWindow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._rdcWindow.Enabled = true;
            this._rdcWindow.Location = new System.Drawing.Point(1, 35);
            this._rdcWindow.Name = "_rdcWindow";
            this._rdcWindow.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("_rdcWindow.OcxState")));
            this._rdcWindow.Size = new System.Drawing.Size(620, 399);
            this._rdcWindow.TabIndex = 0;
            // 
            // borderLeft
            // 
            this.borderLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.borderLeft.BackColor = System.Drawing.Color.Transparent;
            this.borderLeft.BackgroundImage = global::EasyConnect.Properties.Resources.Border;
            this.borderLeft.Location = new System.Drawing.Point(0, 0);
            this.borderLeft.Name = "borderLeft";
            this.borderLeft.Size = new System.Drawing.Size(2, 435);
            this.borderLeft.TabIndex = 6;
            // 
            // toolbarBackground
            // 
            this.toolbarBackground.BackgroundImage = global::EasyConnect.Properties.Resources.ToolbarBackground;
            this.toolbarBackground.Controls.Add(this.forwardButton);
            this.toolbarBackground.Controls.Add(this.backButton);
            this.toolbarBackground.Controls.Add(this.urlTextBox);
            this.toolbarBackground.Controls.Add(this.urlBorder);
            this.toolbarBackground.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarBackground.Location = new System.Drawing.Point(0, 0);
            this.toolbarBackground.Name = "toolbarBackground";
            this.toolbarBackground.Size = new System.Drawing.Size(622, 36);
            this.toolbarBackground.TabIndex = 5;
            // 
            // urlTextBox
            // 
            this.urlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.urlTextBox.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.urlTextBox.Location = new System.Drawing.Point(79, 8);
            this.urlTextBox.Margin = new System.Windows.Forms.Padding(9);
            this.urlTextBox.Name = "urlTextBox";
            this.urlTextBox.Size = new System.Drawing.Size(527, 19);
            this.urlTextBox.TabIndex = 0;
            this.urlTextBox.Text = "about:blank";
            this.urlTextBox.WordWrap = false;
            this.urlTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.urlTextBox_KeyDown);
            // 
            // urlBorder
            // 
            this.urlBorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlBorder.BackColor = System.Drawing.Color.Silver;
            this.urlBorder.Controls.Add(this.urlBackground);
            this.urlBorder.ForeColor = System.Drawing.Color.Silver;
            this.urlBorder.Location = new System.Drawing.Point(69, 5);
            this.urlBorder.Name = "urlBorder";
            this.urlBorder.Size = new System.Drawing.Size(547, 26);
            this.urlBorder.TabIndex = 1;
            // 
            // urlBackground
            // 
            this.urlBackground.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.urlBackground.BackColor = System.Drawing.Color.White;
            this.urlBackground.ForeColor = System.Drawing.Color.Silver;
            this.urlBackground.Location = new System.Drawing.Point(1, 1);
            this.urlBackground.Name = "urlBackground";
            this.urlBackground.Size = new System.Drawing.Size(545, 24);
            this.urlBackground.TabIndex = 2;
            // 
            // borderRight
            // 
            this.borderRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.borderRight.BackColor = System.Drawing.Color.Transparent;
            this.borderRight.BackgroundImage = global::EasyConnect.Properties.Resources.Border;
            this.borderRight.Location = new System.Drawing.Point(620, 0);
            this.borderRight.Name = "borderRight";
            this.borderRight.Size = new System.Drawing.Size(2, 435);
            this.borderRight.TabIndex = 7;
            // 
            // forwardButton
            // 
            this.forwardButton.BackColor = System.Drawing.Color.Transparent;
            this.forwardButton.Image = global::EasyConnect.Properties.Resources.ForwardActive;
            this.forwardButton.Location = new System.Drawing.Point(37, 5);
            this.forwardButton.Margin = new System.Windows.Forms.Padding(4, 4, 3, 3);
            this.forwardButton.Name = "forwardButton";
            this.forwardButton.Size = new System.Drawing.Size(27, 27);
            this.forwardButton.TabIndex = 3;
            this.forwardButton.TabStop = false;
            // 
            // backButton
            // 
            this.backButton.BackColor = System.Drawing.Color.Transparent;
            this.backButton.Image = global::EasyConnect.Properties.Resources.BackActive;
            this.backButton.Location = new System.Drawing.Point(6, 5);
            this.backButton.Margin = new System.Windows.Forms.Padding(4, 4, 3, 3);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(27, 27);
            this.backButton.TabIndex = 2;
            this.backButton.TabStop = false;
            // 
            // borderBottom
            // 
            this.borderBottom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.borderBottom.BackColor = System.Drawing.Color.Transparent;
            this.borderBottom.Location = new System.Drawing.Point(0, 433);
            this.borderBottom.Name = "borderBottom";
            this.borderBottom.Size = new System.Drawing.Size(622, 2);
            this.borderBottom.TabIndex = 8;
            // 
            // RDCWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 435);
            this.Controls.Add(this.borderBottom);
            this.Controls.Add(this.borderRight);
            this.Controls.Add(this.borderLeft);
            this.Controls.Add(this.toolbarBackground);
            this.Controls.Add(this._rdcWindow);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RDCWindow";
            this.Text = "New Tab";
            ((System.ComponentModel.ISupportInitialize)(this._rdcWindow)).EndInit();
            this.toolbarBackground.ResumeLayout(false);
            this.toolbarBackground.PerformLayout();
            this.urlBorder.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.forwardButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.backButton)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxMSTSCLib.AxMsRdpClient2 _rdcWindow;
        private System.Windows.Forms.Panel borderLeft;
        private System.Windows.Forms.Panel toolbarBackground;
        private System.Windows.Forms.PictureBox forwardButton;
        private System.Windows.Forms.PictureBox backButton;
        private System.Windows.Forms.TextBox urlTextBox;
        private System.Windows.Forms.Panel urlBorder;
        private System.Windows.Forms.Panel urlBackground;
        private System.Windows.Forms.Panel borderRight;
        private System.Windows.Forms.Panel borderBottom;
    }
}