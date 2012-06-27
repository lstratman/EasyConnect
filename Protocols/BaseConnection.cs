using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using EasyConnect.Common;

namespace EasyConnect.Protocols
{
    [Serializable]
    public abstract class BaseConnection : IConnection
    {
        protected BaseConnection()
        {
            Guid = Guid.NewGuid();
        }

        protected BaseConnection(SerializationInfo info, StreamingContext context)
        {
            IsBookmark = info.GetBoolean("IsBookmark");
            Name = info.GetString("Name");
            Host = info.GetString("Host");
            Guid = new Guid(info.GetString("Guid"));
            string encryptedPassword = info.GetString("Password");

            if (!String.IsNullOrEmpty(encryptedPassword))
            {
                byte[] encryptedPasswordBytes = Convert.FromBase64String(encryptedPassword);
                byte[] decryptedPassword = CryptoUtilities.Decrypt(ConnectionFactory.EncryptionPassword, encryptedPasswordBytes);
                SecureString password = new SecureString();

                for (int i = 0; i < decryptedPassword.Length; i++)
                {
                    if (decryptedPassword[i] == 0)
                        break;

                    password.AppendChar((char)decryptedPassword[i]);
                    decryptedPassword[i] = 0;
                }

                _password = password;

                for (int i = 0; i < encryptedPasswordBytes.Length; i++)
                    encryptedPasswordBytes[i] = 0;
            }

            if (Guid == Guid.Empty)
                Guid = Guid.NewGuid();
        }
        
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Password", _password == null ? null : Convert.ToBase64String(CryptoUtilities.Encrypt(ConnectionFactory.EncryptionPassword, _password)));
            info.AddValue("IsBookmark", IsBookmark);
            info.AddValue("Name", Name);
            info.AddValue("Host", Host);
            info.AddValue("Guid", Guid.ToString());
        }

        [XmlIgnore]
        [NonSerialized]
        protected BookmarksFolder _parentFolder = null;
        [NonSerialized]
        protected SecureString _password = null;

        public string Host
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

        public string Name
        {
            get;
            set;
        }

        [XmlIgnore]
        public virtual BookmarksFolder ParentFolder
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

        public Guid Guid
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

                return Convert.ToBase64String(CryptoUtilities.Encrypt(ConnectionFactory.EncryptionPassword, _password));
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    Password = null;
                    return;
                }

                byte[] decryptedPassword = CryptoUtilities.Decrypt(ConnectionFactory.EncryptionPassword, Convert.FromBase64String(value));
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

        public virtual object Clone()
        {
            object clonedConnection = SerializationUtilities.Clone(this);

            ((BaseConnection)clonedConnection).ParentFolder = null;
            ((BaseConnection)clonedConnection).Guid = new Guid();

            return clonedConnection;
        }

        public virtual object CloneAnon()
        {
            object clonedConnection = SerializationUtilities.Clone(this);

            ((BaseConnection)clonedConnection).ParentFolder = null;
            ((BaseConnection)clonedConnection).Guid = new Guid();

            return clonedConnection;
        }
    }
}
