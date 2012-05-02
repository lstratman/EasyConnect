using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyConnect.Protocols
{
    public interface IOptionsForm<TConnection>
        where TConnection : IConnection
    {
        TConnection Connection
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
}
