using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace EasyConnect.Protocols.Ssh
{
    public class SshProtocol : BaseProtocol<SshConnection, SshOptionsForm, SshConnectionForm>
    {
        public override string ProtocolPrefix
        {
            get
            {
                return "ssh";
            }
        }

        public override string ProtocolTitle
        {
            get
            {
                return "Secure Shell";
            }
        }

        public override Icon ProtocolIcon
        {
            get
            {
                return Resources.SshProtocol;
            }
        }
    }
}
