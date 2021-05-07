
namespace EasyConnect.Protocols.Vnc
{
    partial class VncDesktop
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
            this.SuspendLayout();
            // 
            // VncDesktop
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.DoubleBuffered = true;
            this.Name = "VncDesktop";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.VncDesktop_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VncDesktop_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VncDesktop_KeyUp);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.VncDesktop_MouseDown);
            this.MouseEnter += new System.EventHandler(this.VncDesktop_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.VncDesktop_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.VncDesktop_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.VncDesktop_MouseUp);
            this.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.VncDesktop_PreviewKeyDown);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
