using System;
using System.Runtime.Serialization;
using System.Security;
using System.Xml;
using System.Xml.Serialization;
using EasyConnect.Common;

namespace EasyConnect.Protocols.Rdp
{
    [Serializable]
    public class RdpConnection : BaseConnection
    {
        public RdpConnection()
        {
            AudioMode = AudioMode.Locally;
            KeyboardMode = KeyboardMode.Remotely;
            VisualStyles = true;
            PersistentBitmapCaching = true;
            ConnectClipboard = true;
            ConnectPrinters = true;
            RecordingMode = RecordingMode.RecordFromThisComputer;
        }

        public RdpConnection(SecureString encryptionPassword)
            : this()
        {
            EncryptionPassword = encryptionPassword;
        }

        protected RdpConnection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Animations = info.GetBoolean("Animations");
            AudioMode = info.GetValue<AudioMode>("AudioMode");
            ColorDepth = info.GetInt32("ColorDepth");
            ConnectClipboard = info.GetBoolean("ConnectClipboard");
            ConnectDrives = info.GetBoolean("ConnectDrives");
            ConnectPrinters = info.GetBoolean("ConnectPrinters");
            DesktopBackground = info.GetBoolean("DesktopBackground");
            DesktopComposition = info.GetBoolean("DesktopComposition");
            DesktopHeight = info.GetInt32("DesktopHeight");
            DesktopWidth = info.GetInt32("DesktopWidth");
            FontSmoothing = info.GetBoolean("FontSmoothing");
            Host = info.GetString("Host");
            KeyboardMode = info.GetValue<KeyboardMode>("KeyboardMode");
            Name = info.GetString("Name");
            PersistentBitmapCaching = info.GetBoolean("PersistentBitmapCaching");
            RecordingMode = info.GetValue<RecordingMode>("RecordingMode");
            Username = info.GetString("Username");
            VisualStyles = info.GetBoolean("VisualStyles");
            WindowContentsWhileDragging = info.GetBoolean("WindowContentsWhileDragging");
        }

        public int DesktopWidth
        {
            get;
            set;
        }

        public int DesktopHeight
        {
            get;
            set;
        }

        public string Host
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string DisplayName
        {
            get
            {
                return String.IsNullOrEmpty(Name)
                           ? Host
                           : Name;
            }
        }

        public int ColorDepth
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public AudioMode AudioMode
        {
            get;
            set;
        }

        public RecordingMode RecordingMode
        {
            get;
            set;
        }

        public KeyboardMode KeyboardMode
        {
            get;
            set;
        }

        public bool ConnectPrinters
        {
            get;
            set;
        }

        public bool ConnectClipboard
        {
            get;
            set;
        }

        public bool ConnectDrives
        {
            get;
            set;
        }

        public bool DesktopBackground
        {
            get;
            set;
        }

        public bool FontSmoothing
        {
            get;
            set;
        }

        public bool DesktopComposition
        {
            get;
            set;
        }

        public bool WindowContentsWhileDragging
        {
            get;
            set;
        }

        public bool Animations
        {
            get;
            set;
        }

        public bool VisualStyles
        {
            get;
            set;
        }

        public bool PersistentBitmapCaching
        {
            get;
            set;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Animations", Animations);
            info.AddValue("AudioMode", AudioMode);
            info.AddValue("ColorDepth", ColorDepth);
            info.AddValue("ConnectClipboard", ConnectClipboard);
            info.AddValue("ConnectDrives", ConnectDrives);
            info.AddValue("ConnectPrinters", ConnectPrinters);
            info.AddValue("DesktopBackground", DesktopBackground);
            info.AddValue("DesktopComposition", DesktopComposition);
            info.AddValue("DesktopHeight", DesktopHeight);
            info.AddValue("DesktopWidth", DesktopWidth);
            info.AddValue("FontSmoothing", FontSmoothing);
            info.AddValue("Guid", Guid.ToString());
            info.AddValue("Host", Host);
            info.AddValue("KeyboardMode", KeyboardMode);
            info.AddValue("PersistentBitmapCaching", PersistentBitmapCaching);
            info.AddValue("RecordingMode", RecordingMode);
            info.AddValue("Username", Username);
            info.AddValue("VisualStyles", VisualStyles);
            info.AddValue("WindowContentsWhileDragging", WindowContentsWhileDragging);
        }

        public override object CloneAnon()
        {
            RdpConnection clonedConnection =  (RdpConnection)base.CloneAnon();
            clonedConnection.Username = null;

            return clonedConnection;
        }

        public override string Uri
        {
            get
            {
                return Host;
            }
        }
    }
}