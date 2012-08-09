using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Win32;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.LocalShell;
using Poderosa.Debugging;
using Poderosa.Terminal;
using Poderosa.Communication;
using Poderosa.Text;
using Poderosa.Config;
using Poderosa.Forms;
using Poderosa.Toolkit;

namespace Poderosa
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void connectSSHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Poderosa.Forms.MultiPaneControl mc = new Poderosa.Forms.MultiPaneControl();
            //mc.Dock = DockStyle.Fill;
            //mc.BackColor = System.Drawing.Color.AliceBlue;
            //tabPage1.Controls.Add(mc);

            InitialAction a = new InitialAction();
            //Poderosa.Forms.GFrame frame = new Poderosa.Forms.GFrame(a);
            ConnectionHistory hst = GApp.ConnectionHistory;
            Poderosa.Forms.LoginDialog dlg = new Poderosa.Forms.LoginDialog();

            TCPTerminalParam param = hst.TopTCPParam;

            dlg.ApplyParam(param);
            dlg.StartPosition = FormStartPosition.CenterParent;

            //if (GCUtil.ShowModalDialog(_frame, dlg) == DialogResult.OK)
            //frame.Show();
            //GCUtil.ShowModalDialog(frame, dlg);
            //dlg.ShowDialog();
            dlg._hostBox.Text = "palm";
            dlg._methodBox.SelectedIndex = 2;
            dlg._portBox.Text = "22";
            dlg._userNameBox.Text = "bwilliam";
            dlg._passphraseBox.Text = "lkmj9u";
            dlg.OnOK(null, null);

            Connection.ConnectionTag ct = dlg.Result;
        }
    }
}