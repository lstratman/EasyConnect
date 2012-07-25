using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    public interface IConnectionForm<TConnection> : IConnectionForm
        where TConnection : IConnection
    {
        TConnection Connection
        {
            get;
            set;
        }
    }

    public interface IConnectionForm
    {
        void Connect();

        event EventHandler Connected;
        event EventHandler ConnectionFormFocused;
    }
}
