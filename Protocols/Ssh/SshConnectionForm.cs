using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Poderosa;
using Poderosa.Config;

namespace EasyConnect.Protocols.Ssh
{
    public partial class SshConnectionForm : BaseConnectionForm<SshConnection>
    {
        public SshConnectionForm()
        {
            InitializeComponent();

            GEnv.Options.WarningOption = WarningOption.Ignore;
            GEnv.Options.Font = new Font("Consolas", 14);

            Connected += SshConnectionForm_Connected;
            _terminal.GotFocus += _terminal_GotFocus;
        }

        void _terminal_GotFocus(object sender, EventArgs e)
        {
            if (ConnectionFormFocused != null)
                ConnectionFormFocused(_terminal, e);
        }

        void SshConnectionForm_Connected(object sender, EventArgs e)
        {
            IsConnected = true;
        }

        public override void Connect()
        {
            _terminal.UserName = Connection.Username;
            _terminal.Host = Connection.Host;

            if (Connection.Password != null && Connection.Password.Length > 0)
            {
                IntPtr password = Marshal.SecureStringToGlobalAllocAnsi(Connection.Password);
                _terminal.Password = Marshal.PtrToStringAnsi(password);
            }

            _terminal.Connect();
        }

        public override event EventHandler Connected;
        public override event EventHandler ConnectionFormFocused;
    }
}
