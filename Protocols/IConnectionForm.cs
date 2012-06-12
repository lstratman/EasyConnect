using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    public interface IConnectionPanel<TConnection> : IConnectionPanel
        where TConnection : IConnection
    {
        TConnection Connection
        {
            get;
            set;
        }
    }

    public interface IConnectionPanel
    {
        void Connect();

        event EventHandler Connected;
    }
}
