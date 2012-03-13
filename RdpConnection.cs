using System;
using System.Runtime.Serialization;
using System.Security;
using System.Xml;
using System.Xml.Serialization;

namespace EasyConnect
{
    [Serializable]
    public class RdpConnection : ICloneable, ISerializable 
    {
        [NonSerialized]
        protected SecureString _encryptionPassword = null;
        [NonSerialized]
        protected SecureString _password = null;
        [NonSerialized]
        protected BookmarksFolder _parentFolder = null;
        [NonSerialized]
        protected byte[] _encryptedPasswordBytes = null;

        public RdpConnection()
        {
            AudioMode = AudioMode.Locally;
            KeyboardMode = KeyboardMode.Remotely;
            VisualStyles = true;
            PersistentBitmapCaching = true;
            ConnectClipboard = true;
            ConnectPrinters = true;
            Guid = Guid.NewGuid();
            RecordingMode = RecordingMode.RecordFromThisComputer;
        }

        public RdpConnection(SecureString encryptionPassword)
        {
            AudioMode = AudioMode.Locally;
            KeyboardMode = KeyboardMode.Remotely;
            EncryptionPassword = encryptionPassword;
            VisualStyles = true;
            PersistentBitmapCaching = true;
            ConnectClipboard = true;
            ConnectPrinters = true;
            Guid = Guid.NewGuid();
            RecordingMode = RecordingMode.RecordFromThisComputer;
        }

        protected RdpConnection(SerializationInfo info, StreamingContext context)
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
            Guid = new Guid(info.GetString("Guid"));
            Host = info.GetString("Host");
            IsBookmark = info.GetBoolean("IsBookmark");
            KeyboardMode = info.GetValue<KeyboardMode>("KeyboardMode");
            Name = info.GetString("Name");
            PersistentBitmapCaching = info.GetBoolean("PersistentBitmapCaching");
            RecordingMode = info.GetValue<RecordingMode>("RecordingMode");
            Username = info.GetString("Username");
            VisualStyles = info.GetBoolean("VisualStyles");
            WindowContentsWhileDragging = info.GetBoolean("WindowContentsWhileDragging");

            string encryptedPassword = info.GetString("Password");

            if (!String.IsNullOrEmpty(encryptedPassword))
                _encryptedPasswordBytes = Convert.FromBase64String(encryptedPassword);
        }

        public RdpConnection(XmlNode node, SecureString encryptionPassword)
        {
            EncryptionPassword = encryptionPassword;
            Host = node.SelectSingleNode("@host").Value;
            Username = node.SelectSingleNode("@username").Value;
            DesktopWidth = Convert.ToInt32(node.SelectSingleNode("@desktopWidth").Value);
            DesktopHeight = Convert.ToInt32(node.SelectSingleNode("@desktopHeight").Value);
            ColorDepth = Convert.ToInt32(node.SelectSingleNode("@colorDepth").Value);
            Name = node.SelectSingleNode("@name").Value;
            Guid = (node.SelectSingleNode("@guid") != null
                        ? new Guid(node.SelectSingleNode("@guid").Value)
                        : Guid.NewGuid());
            IsBookmark = (node.SelectSingleNode("@isBookmark") != null &&
                          Boolean.Parse(node.SelectSingleNode("@isBookmark").Value));
            AudioMode = (node.SelectSingleNode("@audioMode") != null
                             ? (AudioMode) Enum.Parse(typeof (AudioMode), node.SelectSingleNode("@audioMode").Value)
                             : AudioMode.Locally);
            KeyboardMode = (node.SelectSingleNode("@keyboardMode") != null
                                ? (KeyboardMode)
                                  Enum.Parse(typeof (KeyboardMode), node.SelectSingleNode("@keyboardMode").Value)
                                : KeyboardMode.Remotely);
            ConnectPrinters = (node.SelectSingleNode("@connectPrinters") == null ||
                               Boolean.Parse(node.SelectSingleNode("@connectPrinters").Value));
            ConnectClipboard = (node.SelectSingleNode("@connectClipboard") == null ||
                                Boolean.Parse(node.SelectSingleNode("@connectClipboard").Value));
            ConnectDrives = (node.SelectSingleNode("@connectDrives") != null &&
                             Boolean.Parse(node.SelectSingleNode("@connectDrives").Value));
            DesktopBackground = (node.SelectSingleNode("@desktopBackground") != null &&
                                 Boolean.Parse(node.SelectSingleNode("@desktopBackground").Value));
            FontSmoothing = (node.SelectSingleNode("@fontSmoothing") != null &&
                             Boolean.Parse(node.SelectSingleNode("@fontSmoothing").Value));
            DesktopComposition = (node.SelectSingleNode("@desktopComposition") != null &&
                                  Boolean.Parse(node.SelectSingleNode("@desktopComposition").Value));
            WindowContentsWhileDragging = (node.SelectSingleNode("@windowContentsWhileDragging") != null &&
                                           Boolean.Parse(node.SelectSingleNode("@windowContentsWhileDragging").Value));
            Animations = (node.SelectSingleNode("@animations") != null &&
                          Boolean.Parse(node.SelectSingleNode("@animations").Value));
            VisualStyles = (node.SelectSingleNode("@visualStyles") == null ||
                            Boolean.Parse(node.SelectSingleNode("@visualStyles").Value));
            PersistentBitmapCaching = (node.SelectSingleNode("@persistentBitmapCaching") == null ||
                                       Boolean.Parse(node.SelectSingleNode("@persistentBitmapCaching").Value));

            if (node.SelectSingleNode("@password") != null)
            {
                byte[] decryptedPassword = CryptoUtilities.Decrypt(_encryptionPassword,
                                                                   Convert.FromBase64String(
                                                                       node.SelectSingleNode("@password").Value));
                SecureString password = new SecureString();

                for (int i = 0; i < decryptedPassword.Length; i++)
                {
                    if (decryptedPassword[i] == 0)
                        break;

                    password.AppendChar((char) decryptedPassword[i]);
                    decryptedPassword[i] = 0;
                }

                Password = password;
            }
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

        [XmlElement("Password")]
        public string Base64Password
        {
            get
            {
                if (Password == null)
                    return null;

                return Convert.ToBase64String(CryptoUtilities.Encrypt(_encryptionPassword, _password));
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    Password = null;
                    return;
                }

                if (_encryptionPassword == null)
                {
                    _encryptedPasswordBytes = Convert.FromBase64String(value);
                    return;
                }

                byte[] decryptedPassword = CryptoUtilities.Decrypt(_encryptionPassword, Convert.FromBase64String(value));
                SecureString password = new SecureString();

                for (int i = 0; i < decryptedPassword.Length; i++)
                {
                    if (decryptedPassword[i] == 0)
                        break;

                    password.AppendChar((char)decryptedPassword[i]);
                    decryptedPassword[i] = 0;
                }

                Password = password;
            }
        }

        [XmlIgnore]
        public SecureString Password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;

                if (_password != null)
                    _password.MakeReadOnly();
            }
        }

        public bool IsBookmark
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

        public SecureString EncryptionPassword
        {
            set
            {
                _encryptionPassword = value;

                if (_encryptionPassword != null && _encryptedPasswordBytes != null)
                {
                    byte[] decryptedPassword = CryptoUtilities.Decrypt(_encryptionPassword, _encryptedPasswordBytes);
                    SecureString password = new SecureString();

                    for (int i = 0; i < decryptedPassword.Length; i++)
                    {
                        if (decryptedPassword[i] == 0)
                            break;

                        password.AppendChar((char) decryptedPassword[i]);
                        decryptedPassword[i] = 0;
                    }

                    _password = password;

                    for (int i = 0; i < _encryptedPasswordBytes.Length; i++)
                        _encryptedPasswordBytes[i] = 0;

                    _encryptedPasswordBytes = null;
                }
            }
        }

        public Guid Guid
        {
            get;
            set;
        }

        [XmlIgnore]
        public BookmarksFolder ParentFolder
        {
            get
            {
                return _parentFolder;
            }

            set
            {
                _parentFolder = value;
            }
        }

        public virtual void ToXmlNode(XmlNode node)
        {
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("host"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("username"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("desktopWidth"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("desktopHeight"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("colorDepth"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("name"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("isBookmark"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("audioMode"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("keyboardMode"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("connectPrinters"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("connectClipboard"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("connectDrives"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("desktopBackground"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("fontSmoothing"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("desktopComposition"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("windowContentsWhileDragging"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("animations"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("visualStyles"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("persistentBitmapCaching"));
            node.Attributes.Append(node.OwnerDocument.CreateAttribute("guid"));

            node.Attributes["host"].Value = Host;
            node.Attributes["username"].Value = Username;
            node.Attributes["desktopWidth"].Value = DesktopWidth.ToString();
            node.Attributes["desktopHeight"].Value = DesktopHeight.ToString();
            node.Attributes["colorDepth"].Value = ColorDepth.ToString();
            node.Attributes["name"].Value = Name;
            node.Attributes["isBookmark"].Value = IsBookmark.ToString();
            node.Attributes["audioMode"].Value = AudioMode.ToString();
            node.Attributes["keyboardMode"].Value = KeyboardMode.ToString();
            node.Attributes["connectPrinters"].Value = ConnectPrinters.ToString();
            node.Attributes["connectClipboard"].Value = ConnectClipboard.ToString();
            node.Attributes["connectDrives"].Value = ConnectDrives.ToString();
            node.Attributes["desktopBackground"].Value = DesktopBackground.ToString();
            node.Attributes["fontSmoothing"].Value = FontSmoothing.ToString();
            node.Attributes["desktopComposition"].Value = DesktopComposition.ToString();
            node.Attributes["windowContentsWhileDragging"].Value = WindowContentsWhileDragging.ToString();
            node.Attributes["animations"].Value = Animations.ToString();
            node.Attributes["visualStyles"].Value = VisualStyles.ToString();
            node.Attributes["persistentBitmapCaching"].Value = PersistentBitmapCaching.ToString();
            node.Attributes["guid"].Value = Guid.ToString();

            if (_password != null && _password.Length > 0)
            {
                node.Attributes.Append(node.OwnerDocument.CreateAttribute("password"));
                node.Attributes["password"].Value =
                    Convert.ToBase64String(CryptoUtilities.Encrypt(_encryptionPassword, _password));
            }
        }

        public object Clone()
        {
            RdpConnection clonedConnection = SerializationHelper.Clone(this);

            clonedConnection.ParentFolder = null;
            clonedConnection.Guid = new Guid();
            clonedConnection.EncryptionPassword = _encryptionPassword;

            return clonedConnection;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
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
            info.AddValue("IsBookmark", IsBookmark);
            info.AddValue("KeyboardMode", KeyboardMode);
            info.AddValue("Name", Name);
            info.AddValue("PersistentBitmapCaching", PersistentBitmapCaching);
            info.AddValue("RecordingMode", RecordingMode);
            info.AddValue("Username", Username);
            info.AddValue("VisualStyles", VisualStyles);
            info.AddValue("WindowContentsWhileDragging", WindowContentsWhileDragging);
            info.AddValue("Password", _password == null ? null : Convert.ToBase64String(CryptoUtilities.Encrypt(_encryptionPassword, _password)));
        }
    }
}