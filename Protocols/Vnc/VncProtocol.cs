using System.Drawing;

namespace EasyConnect.Protocols.Vnc
{
    public class VncProtocol : BaseProtocol<VncConnection, VncOptionsForm, VncConnectionForm>
    {
        public override string ProtocolPrefix
        {
            get
            {
                return "vnc";
            }
        }

        public override string ProtocolTitle
        {
            get
            {
                return "VNC";
            }
        }

        public override Icon ProtocolIcon
        {
            get
            {
                return Resources.VncIcon;
            }
        }
    }
}
