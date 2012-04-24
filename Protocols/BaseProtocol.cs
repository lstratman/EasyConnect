using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyConnect.Protocols
{
    public abstract class BaseProtocol<TConnection, TOptionsUi> 
        where TConnection : IConnection
        where TOptionsUi : BaseOptionsUi
    {
        public abstract string ProtocolPrefix
        {
            get;
        }
    }
}
