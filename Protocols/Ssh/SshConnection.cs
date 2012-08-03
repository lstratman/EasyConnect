using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace EasyConnect.Protocols.Ssh
{
    [Serializable]
    public class SshConnection : BaseConnection
    {
        public SshConnection()
        {
        }

        public SshConnection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Username = info.GetString("Username");
        }

        public string Username
        {
            get;
            set;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Username", Username);
        }
    }
}
