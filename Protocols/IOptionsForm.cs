using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyConnect.Protocols
{
    public interface IOptionsForm
    {
        IConnection Connection
        {
            get;
            set;
        }

        bool DefaultsMode
        {
            get;
            set;
        }
    }

    public interface IOptionsForm<TConnection> : IOptionsForm
        where TConnection : IConnection
    {
        TConnection Connection
        {
            get;
            set;
        }
    }
}
