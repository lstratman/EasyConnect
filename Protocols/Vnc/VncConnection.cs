using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EasyConnect.Protocols.Vnc
{
    [Serializable]
    public class VncConnection : BaseConnection
    {
        public VncConnection()
        {
            Port = 5900;
        }

        protected VncConnection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Port = info.GetInt32("Port");
            Display = info.GetInt32("Display");
            ViewOnly = info.GetBoolean("ViewOnly");
            Scale = info.GetBoolean("Scale");
            Username = info.GetString("Username");
        }

        public int Port
        {
            get;
            set;
        }

        public int Display
        {
            get;
            set;
        }

        public bool ViewOnly
        {
            get;
            set;
        }

        public bool Scale
        {
            get;
            set;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Port", Port);
            info.AddValue("Display", Display);
            info.AddValue("ViewOnly", ViewOnly);
            info.AddValue("Scale", Scale);
            info.AddValue("Username", Username);
        }
    }
}
