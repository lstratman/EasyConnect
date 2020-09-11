using System.Windows.Forms;

namespace EasyConnect.Protocols.Rdp
{
    partial class RdpConnectionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RdpConnectionForm));
            this._rdpWindow = new AxMSTSCLib.AxMsRdpClient6NotSafeForScripting();
            ((System.ComponentModel.ISupportInitialize)(this._rdpWindow)).BeginInit();
            this.SuspendLayout();
            // 
            // _rdpWindow
            // 
            this._rdpWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this._rdpWindow.Enabled = true;
            this._rdpWindow.Location = new System.Drawing.Point(0, 0);
            this._rdpWindow.Name = "_rdpWindow";
            this._rdpWindow.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("_rdpWindow.OcxState")));
            this._rdpWindow.Size = new System.Drawing.Size(284, 262);
            this._rdpWindow.TabIndex = 0;
            this._rdpWindow.OnDisconnected += new AxMSTSCLib.IMsTscAxEvents_OnDisconnectedEventHandler(this._rdpWindow_OnDisconnected);
            // 
            // RdpConnectionForm
            // 
            this.BackColor = System.Drawing.Color.White;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this._rdpWindow);
            this.Name = "RdpConnectionForm";
            this.Text = "RdpConnectionForm";
            this.GotFocus += RdpConnectionForm_GotFocus;
            ((System.ComponentModel.ISupportInitialize)(this._rdpWindow)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxMSTSCLib.AxMsRdpClient6NotSafeForScripting _rdpWindow;

    }
}