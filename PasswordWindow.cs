using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Security;
using System.Runtime.InteropServices;

namespace UltraRDC
{
    public partial class PasswordWindow : Form
    {
        public PasswordWindow()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            passwordTextBox.Focus();
        }

        public SecureString Password
        {
            get
            {
                return passwordTextBox.SecureText;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void passwordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                okButton_Click(null, null);
        }
    }
}
