using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Windows.Forms;
using AxMSTSCLib;

namespace EasyConnect.Protocols.Rdp
{
    public class RdpProtocol : BaseProtocol<RdpConnection, RdpOptionsForm, RdpConnectionPanel>
    {
        public override string ProtocolPrefix
        {
            get
            {
                return "rdp";
            }
        }

        public override string ProtocolTitle
        {
            get
            {
                return "Remote Desktop";
            }
        }
    }
}
