using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ViewerX;

namespace EasyConnect.Protocols.Vnc
{
    [Serializable]
    public class VncConnection : BaseConnection
    {
        public VncConnection()
        {
            Port = 5900;
			AuthenticationType = ViewerLoginType.VLT_VNC;
			EncryptionType = EncryptionPluginType.EPT_NONE;
			EncodingType = VNCEncoding.RFB_ZRLE;
			ColorDepth = ColorDepth.COLOR_FULL;
        }

        protected VncConnection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
			foreach (SerializationEntry entry in info)
			{
				switch (entry.Name)
				{
					case "Port":
						Port = (int) entry.Value;
						break;

					case "Display":
						Display = (int) entry.Value;
						break;

					case "ViewOnly":
						ViewOnly = (bool) entry.Value;
						break;

					case "Scale":
						Scale = (bool) entry.Value;
						break;

					case "AuthenticationType":
						AuthenticationType = (ViewerLoginType) Enum.Parse(typeof (ViewerLoginType), (string) entry.Value);
						break;

					case "EncryptionType":
						EncryptionType = (EncryptionPluginType) Enum.Parse(typeof (EncryptionPluginType), (string) entry.Value);
						break;

					case "KeyFile":
						KeyFile = (string) entry.Value;
						break;

					case "EncodingType":
						EncodingType = (VNCEncoding) Enum.Parse(typeof (VNCEncoding), (string) entry.Value);
						break;

					case "ColorDepth":
						ColorDepth = (ColorDepth) Enum.Parse(typeof (ColorDepth), (string) entry.Value);
						break;
				}
			}
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

		public ViewerLoginType AuthenticationType
		{
			get;
			set;
		}

		public EncryptionPluginType EncryptionType
		{
			get;
			set;
		}

		public string KeyFile
		{
			get;
			set;
		}

		public VNCEncoding EncodingType
		{
			get;
			set;
		}

		public ColorDepth ColorDepth
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
			info.AddValue("AuthenticationType", AuthenticationType.ToString("G"));
			info.AddValue("EncryptionType", EncryptionType.ToString("G"));
			info.AddValue("KeyFile", KeyFile);
			info.AddValue("EncodingType", EncodingType.ToString("G"));
			info.AddValue("ColorDepth", ColorDepth.ToString("G"));
        }
    }
}
