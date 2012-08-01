namespace EasyConnect.Protocols.Vnc
{
    partial class VncOptionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VncOptionsForm));
            this._displayLabel = new System.Windows.Forms.Label();
            this._displayNumberLabel = new System.Windows.Forms.Label();
            this._flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._hostPanel = new System.Windows.Forms.Panel();
            this._portLabel = new System.Windows.Forms.Label();
            this._hostNameLabel = new System.Windows.Forms.Label();
            this._hostNameTextBox = new System.Windows.Forms.TextBox();
            this._hostLabel = new System.Windows.Forms.Label();
            this._hostDividerPanel = new System.Windows.Forms.Panel();
            this._displayPanel = new System.Windows.Forms.Panel();
            this._scaleLabel = new System.Windows.Forms.Label();
            this._portUpDown = new System.Windows.Forms.NumericUpDown();
            this._displayUpDown = new System.Windows.Forms.NumericUpDown();
            this._scaleCheckbox = new System.Windows.Forms.CheckBox();
            this._viewOnlyCheckbox = new System.Windows.Forms.CheckBox();
            this._viewOnlyLabel = new System.Windows.Forms.Label();
            this._flowLayoutPanel.SuspendLayout();
            this._hostPanel.SuspendLayout();
            this._displayPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._portUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._displayUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // _displayLabel
            // 
            this._displayLabel.AutoSize = true;
            this._displayLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._displayLabel.Location = new System.Drawing.Point(8, 11);
            this._displayLabel.Name = "_displayLabel";
            this._displayLabel.Size = new System.Drawing.Size(61, 16);
            this._displayLabel.TabIndex = 55;
            this._displayLabel.Text = "Display";
            // 
            // _displayNumberLabel
            // 
            this._displayNumberLabel.Location = new System.Drawing.Point(91, 9);
            this._displayNumberLabel.Name = "_displayNumberLabel";
            this._displayNumberLabel.Size = new System.Drawing.Size(150, 20);
            this._displayNumberLabel.TabIndex = 53;
            this._displayNumberLabel.Text = "Display:";
            this._displayNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _flowLayoutPanel
            // 
            this._flowLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._flowLayoutPanel.Controls.Add(this._hostPanel);
            this._flowLayoutPanel.Controls.Add(this._hostDividerPanel);
            this._flowLayoutPanel.Controls.Add(this._displayPanel);
            this._flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this._flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this._flowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this._flowLayoutPanel.Name = "_flowLayoutPanel";
            this._flowLayoutPanel.Padding = new System.Windows.Forms.Padding(15);
            this._flowLayoutPanel.Size = new System.Drawing.Size(720, 683);
            this._flowLayoutPanel.TabIndex = 83;
            this._flowLayoutPanel.WrapContents = false;
            // 
            // _hostPanel
            // 
            this._hostPanel.Controls.Add(this._portUpDown);
            this._hostPanel.Controls.Add(this._portLabel);
            this._hostPanel.Controls.Add(this._hostNameLabel);
            this._hostPanel.Controls.Add(this._hostNameTextBox);
            this._hostPanel.Controls.Add(this._hostLabel);
            this._hostPanel.Location = new System.Drawing.Point(18, 18);
            this._hostPanel.Name = "_hostPanel";
            this._hostPanel.Size = new System.Drawing.Size(684, 67);
            this._hostPanel.TabIndex = 84;
            // 
            // _portLabel
            // 
            this._portLabel.Location = new System.Drawing.Point(91, 34);
            this._portLabel.Name = "_portLabel";
            this._portLabel.Size = new System.Drawing.Size(150, 20);
            this._portLabel.TabIndex = 55;
            this._portLabel.Text = "Port:";
            this._portLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _hostNameLabel
            // 
            this._hostNameLabel.Location = new System.Drawing.Point(91, 9);
            this._hostNameLabel.Name = "_hostNameLabel";
            this._hostNameLabel.Size = new System.Drawing.Size(150, 20);
            this._hostNameLabel.TabIndex = 53;
            this._hostNameLabel.Text = "Host name:";
            this._hostNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _hostNameTextBox
            // 
            this._hostNameTextBox.Location = new System.Drawing.Point(247, 10);
            this._hostNameTextBox.Name = "_hostNameTextBox";
            this._hostNameTextBox.Size = new System.Drawing.Size(154, 20);
            this._hostNameTextBox.TabIndex = 52;
            // 
            // _hostLabel
            // 
            this._hostLabel.AutoSize = true;
            this._hostLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._hostLabel.Location = new System.Drawing.Point(8, 11);
            this._hostLabel.Name = "_hostLabel";
            this._hostLabel.Size = new System.Drawing.Size(40, 16);
            this._hostLabel.TabIndex = 54;
            this._hostLabel.Text = "Host";
            // 
            // _hostDividerPanel
            // 
            this._hostDividerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._hostDividerPanel.BackColor = System.Drawing.Color.Silver;
            this._hostDividerPanel.Location = new System.Drawing.Point(18, 91);
            this._hostDividerPanel.Name = "_hostDividerPanel";
            this._hostDividerPanel.Size = new System.Drawing.Size(684, 1);
            this._hostDividerPanel.TabIndex = 63;
            // 
            // _displayPanel
            // 
            this._displayPanel.Controls.Add(this._viewOnlyCheckbox);
            this._displayPanel.Controls.Add(this._viewOnlyLabel);
            this._displayPanel.Controls.Add(this._scaleCheckbox);
            this._displayPanel.Controls.Add(this._displayUpDown);
            this._displayPanel.Controls.Add(this._displayLabel);
            this._displayPanel.Controls.Add(this._displayNumberLabel);
            this._displayPanel.Controls.Add(this._scaleLabel);
            this._displayPanel.Location = new System.Drawing.Point(18, 98);
            this._displayPanel.Name = "_displayPanel";
            this._displayPanel.Size = new System.Drawing.Size(684, 87);
            this._displayPanel.TabIndex = 83;
            // 
            // _scaleLabel
            // 
            this._scaleLabel.Location = new System.Drawing.Point(91, 34);
            this._scaleLabel.Name = "_scaleLabel";
            this._scaleLabel.Size = new System.Drawing.Size(150, 20);
            this._scaleLabel.TabIndex = 54;
            this._scaleLabel.Text = "Scale?:";
            this._scaleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _portUpDown
            // 
            this._portUpDown.Location = new System.Drawing.Point(247, 36);
            this._portUpDown.Name = "_portUpDown";
            this._portUpDown.Size = new System.Drawing.Size(61, 20);
            this._portUpDown.TabIndex = 56;
            // 
            // _displayUpDown
            // 
            this._displayUpDown.Location = new System.Drawing.Point(247, 11);
            this._displayUpDown.Name = "_displayUpDown";
            this._displayUpDown.Size = new System.Drawing.Size(61, 20);
            this._displayUpDown.TabIndex = 81;
            // 
            // _scaleCheckbox
            // 
            this._scaleCheckbox.AutoSize = true;
            this._scaleCheckbox.Location = new System.Drawing.Point(247, 39);
            this._scaleCheckbox.Name = "_scaleCheckbox";
            this._scaleCheckbox.Size = new System.Drawing.Size(15, 14);
            this._scaleCheckbox.TabIndex = 82;
            this._scaleCheckbox.UseVisualStyleBackColor = true;
            // 
            // _viewOnlyCheckbox
            // 
            this._viewOnlyCheckbox.AutoSize = true;
            this._viewOnlyCheckbox.Location = new System.Drawing.Point(247, 60);
            this._viewOnlyCheckbox.Name = "_viewOnlyCheckbox";
            this._viewOnlyCheckbox.Size = new System.Drawing.Size(15, 14);
            this._viewOnlyCheckbox.TabIndex = 84;
            this._viewOnlyCheckbox.UseVisualStyleBackColor = true;
            // 
            // _viewOnlyLabel
            // 
            this._viewOnlyLabel.Location = new System.Drawing.Point(91, 55);
            this._viewOnlyLabel.Name = "_viewOnlyLabel";
            this._viewOnlyLabel.Size = new System.Drawing.Size(150, 20);
            this._viewOnlyLabel.TabIndex = 83;
            this._viewOnlyLabel.Text = "View only?:";
            this._viewOnlyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // VncOptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 683);
            this.Controls.Add(this._flowLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VncOptionsForm";
            this.Text = "VNC Options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VncOptionsForm_FormClosing);
            this.Load += new System.EventHandler(this.VncOptionsForm_Load);
            this._flowLayoutPanel.ResumeLayout(false);
            this._hostPanel.ResumeLayout(false);
            this._hostPanel.PerformLayout();
            this._displayPanel.ResumeLayout(false);
            this._displayPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._portUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._displayUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label _displayLabel;
        private System.Windows.Forms.Label _displayNumberLabel;
        private System.Windows.Forms.FlowLayoutPanel _flowLayoutPanel;
        private System.Windows.Forms.Panel _hostPanel;
        private System.Windows.Forms.Label _portLabel;
        private System.Windows.Forms.Label _hostNameLabel;
        private System.Windows.Forms.TextBox _hostNameTextBox;
        private System.Windows.Forms.Label _hostLabel;
        private System.Windows.Forms.Panel _hostDividerPanel;
        private System.Windows.Forms.Panel _displayPanel;
        private System.Windows.Forms.Label _scaleLabel;
        private System.Windows.Forms.NumericUpDown _portUpDown;
        private System.Windows.Forms.NumericUpDown _displayUpDown;
        private System.Windows.Forms.CheckBox _scaleCheckbox;
        private System.Windows.Forms.CheckBox _viewOnlyCheckbox;
        private System.Windows.Forms.Label _viewOnlyLabel;
    }
}