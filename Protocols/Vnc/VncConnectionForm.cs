using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VncSharp;

namespace EasyConnect.Protocols.Vnc
{
    public partial class VncConnectionForm : BaseConnectionForm<VncConnection>
    {
        public VncConnectionForm()
        {
            InitializeComponent();

            Connected += VncConnectionForm_Connected;
            _vncConnection.GotFocus += _vncConnection_GotFocus;
        }

        void _vncConnection_GotFocus(object sender, EventArgs e)
        {
            if (ConnectionFormFocused != null)
                ConnectionFormFocused(_vncConnection, e);
        }

        void VncConnectionForm_Connected(object sender, EventArgs e)
        {
            IsConnected = true;
        }

        public override void Connect()
        {
            _vncConnection.VncPort = Connection.Port;
            _vncConnection.ConnectComplete += _vncConnection_ConnectComplete;
            _vncConnection.GetPassword = GetPassword;
            _vncConnection.Connect(Connection.Host, Connection.Display, Connection.ViewOnly, Connection.Scale);
        }

        private string GetPassword()
        {
            return "133t$p3@k";
        }

        void _vncConnection_ConnectComplete(object sender, ConnectEventArgs e)
        {
            if (Connected != null)
                Connected(this, null);
        }

        public override event EventHandler Connected;
        public override event EventHandler ConnectionFormFocused;
    }
}
