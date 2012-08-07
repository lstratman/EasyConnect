using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace EasyConnect.Protocols.Ssh
{
    [Serializable]
    public class SshConnection : BaseConnection
    {
        public SshConnection()
        {
            BackgroundColor = Color.Black;
            TextColor = Color.LightGray;
            Font = new Font("Courier New", 10);
        }

        public SshConnection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Username = info.GetString("Username");
            IdentityFile = info.GetString("IdentityFile");
            BackgroundColor = Color.FromArgb(info.GetInt32("BackgroundColor"));
            TextColor = Color.FromArgb(info.GetInt32("TextColor"));
            FontFamily = info.GetString("FontFamily");
            FontSize = info.GetSingle("FontSize");
        }

        public string Username
        {
            get;
            set;
        }

        public string IdentityFile
        {
            get;
            set;
        }

        [XmlElement(ElementName = "BackgroundColor")]
        public int BackgroundColorArgb
        {
            get
            {
                return BackgroundColor.ToArgb();
            }

            set
            {
                BackgroundColor = Color.FromArgb(value);
            }
        }

        [XmlElement(ElementName = "TextColor")]
        public int TextColorArgb
        {
            get
            {
                return TextColor.ToArgb();
            }

            set
            {
                TextColor = Color.FromArgb(value);
            }
        }

        [XmlIgnore]
        public Color BackgroundColor
        {
            get;
            set;
        }

        [XmlIgnore]
        public Color TextColor
        {
            get;
            set;
        }

        [XmlIgnore]
        public Font Font
        {
            get
            {
                return new Font(FontFamily, FontSize);
            }

            set
            {
                FontFamily = value.FontFamily.GetName(0);
                FontSize = value.SizeInPoints;
            }
        }

        public string FontFamily
        {
            get;
            set;
        }

        public float FontSize
        {
            get;
            set;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Username", Username);
            info.AddValue("IdentityFile", IdentityFile);
            info.AddValue("TextColor", TextColor.ToArgb());
            info.AddValue("BackgroundColor", BackgroundColor.ToArgb());
            info.AddValue("FontFamily", FontFamily);
            info.AddValue("FontSize", FontSize);
        }
    }
}
