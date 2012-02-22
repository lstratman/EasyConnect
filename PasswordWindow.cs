using System;
using System.Security;
using System.Windows.Forms;

namespace EasyConnect
{
    public partial class PasswordWindow : Form
    {
        public PasswordWindow()
        {
            InitializeComponent();
        }

        public SecureString Password
        {
            get
            {
                return passwordTextBox.SecureText;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            passwordTextBox.Focus();
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