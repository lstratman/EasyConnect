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
            this._vncFramebuffer = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // _vncConnection
            // 
            this._vncFramebuffer.Dock = System.Windows.Forms.DockStyle.None;
            this._vncFramebuffer.Enabled = true;
            this._vncFramebuffer.Location = new System.Drawing.Point(0, 0);
            this._vncFramebuffer.Name = "_vncFramebuffer";
            this._vncFramebuffer.Size = new System.Drawing.Size(786, 551);
            this._vncFramebuffer.TabIndex = 0;
            // 
            // VncConnectionForm
            // 
            this.BackColor = System.Drawing.Color.White;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(786, 551);
            this.Controls.Add(this._vncFramebuffer);
            this.Name = "VncConnectionForm";
            this.Text = "VncConnectionForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel _vncFramebuffer;

    }
}