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

        public abstract event EventHandler Connected;
        public abstract event EventHandler ConnectionFormFocused;

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
