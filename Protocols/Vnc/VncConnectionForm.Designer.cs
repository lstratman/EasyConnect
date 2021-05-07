namespace EasyConnect.Protocols.Vnc
{
    partial class VncConnectionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VncConnectionForm));
            this._vncDesktop = new EasyConnect.Protocols.Vnc.VncDesktop();
            this.SuspendLayout();
            // 
            // _vncConnection
            // 
            this._vncDesktop.Dock = System.Windows.Forms.DockStyle.None;
            this._vncDesktop.Enabled = true;
            this._vncDesktop.Location = new System.Drawing.Point(0, 0);
            this._vncDesktop.Name = "_vncDesktop";
            this._vncDesktop.Size = new System.Drawing.Size(786, 551);
            this._vncDesktop.TabIndex = 0;
            // 
            // VncConnectionForm
            // 
            this.BackColor = System.Drawing.Color.White;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 551);
            this.Controls.Add(this._vncDesktop);
            this.Name = "VncConnectionForm";
            this.Text = "VncConnectionForm";
            this.ResumeLayout(false);

        }

        #endregion

        private EasyConnect.Protocols.Vnc.VncDesktop _vncDesktop;

    }
}