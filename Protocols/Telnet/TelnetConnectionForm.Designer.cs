﻿using System.Windows.Forms;

namespace EasyConnect.Protocols.Telnet
{
    partial class TelnetConnectionForm
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
            this._terminal = new Poderosa.Terminal.TerminalControl();
            this.SuspendLayout();
            // 
            // _terminal
            // 
            this._terminal.Dock = DockStyle.Fill;
            this._terminal.BackColor = System.Drawing.Color.Black;
            this._terminal.ForeColor = System.Drawing.Color.Silver;
            this._terminal.Location = new System.Drawing.Point(0, 0);
            this._terminal.Name = "_terminal";
            this._terminal.Size = new System.Drawing.Size(286, 266);
            this._terminal.TabIndex = 0;
            this._terminal.Text = "terminalControl1";
            // 
            // SshConnectionForm
            // 
            this.BackColor = System.Drawing.Color.White;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Controls.Add(this._terminal);
            this.Name = "SshConnectionForm";
            this.Text = "SshConnectionForm";
            this.ResumeLayout(false);

        }

        #endregion

        private Poderosa.Terminal.TerminalControl _terminal;
    }
}