using System;
using System.Security;
using System.Xml;

namespace EasyConnect
{
    public class RDCConnection
    {
        protected SecureString _encryptionPassword = null;
        protected SecureString _password = null;

        public RDCConnection()
        {
            AudioMode = AudioMode.Locally;
            KeyboardMode = KeyboardMode.Remotely;
            VisualStyles = true;
            PersistentBitmapCaching = true;
            ConnectClipboard = true;
            ConnectPrinters = true;
            Guid = Guid.NewGuid();
        }

        public RDCConnection(SecureString encryptionPassword)
        {
            AudioMode = AudioMode.Locally;
            KeyboardMode = KeyboardMode.Remotely;
            EncryptionPassword = encryptionPassword;
            VisualStyles = true;
            PersistentBitmapCaching = true;
            ConnectClipboard = true;
            ConnectPrinters = true;
            Guid = Guid.NewGuid();
        }

        public RDCConnection(XmlNode node, SecureString encryptionPassword)
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
            }
        }

        public Guid Guid
        {
            get;
            set;
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
    }
}