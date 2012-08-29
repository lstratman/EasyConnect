using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    public abstract class BaseConnectionForm : Form, IConnectionForm
    {
        protected BaseConnectionForm()
        {
            CloseParentFormOnDisconnect = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            ConnectionWindow.GotFocus += OnConnectionWindowGotFocus;
            base.OnLoad(e);
        }

        protected virtual void OnConnectionWindowGotFocus(object sender, EventArgs e)
        {
            if (ConnectionFormFocused != null)
                ConnectionFormFocused(ConnectionWindow, e);
        }

        protected virtual void OnConnected(object sender, EventArgs e)
        {
            IsConnected = true;

            if (Connected != null)
                Connected(sender, e);
        }

        public event EventHandler Connected;
        public event EventHandler ConnectionFormFocused;

        protected abstract Control ConnectionWindow
        {
            get;
        }

        public bool IsConnected
        {
            get;
            protected set;
        }

        public bool CloseParentFormOnDisconnect
        {
            get;
            set;
        }

        public abstract void Connect();
    }

    public abstract class BaseConnectionForm<T> : BaseConnectionForm, IConnectionForm<T>
        where T : IConnection
    {
        public T Connection
        {
            get;
            set;
        }
    }
}
