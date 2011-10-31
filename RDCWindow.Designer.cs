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
            this.rdcWindow = new AxMSTSCLib.AxMsRdpClient2();
            ((System.ComponentModel.ISupportInitialize)(this.rdcWindow)).BeginInit();
            this.SuspendLayout();
            // 
            // rdcWindow
            // 
            this.rdcWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rdcWindow.Enabled = true;
            this.rdcWindow.Location = new System.Drawing.Point(0, 0);
            this.rdcWindow.Name = "rdcWindow";
            this.rdcWindow.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("rdcWindow.OcxState")));
            this.rdcWindow.Size = new System.Drawing.Size(284, 262);
            this.rdcWindow.TabIndex = 0;
            // 
            // TerminalServicesWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this.rdcWindow);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TerminalServicesWindow";
            this.Text = "TerminalServicesWindow";
            ((System.ComponentModel.ISupportInitialize)(this.rdcWindow)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxMSTSCLib.AxMsRdpClient2 rdcWindow;
    }
}