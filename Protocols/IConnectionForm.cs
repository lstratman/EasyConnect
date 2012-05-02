using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
    public interface IConnectionForm<TConnection>
        where TConnection : IConnection
    {
        void Connect(TConnection connection);

        event EventHandler Connected;
    }
}
