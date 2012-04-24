using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Xml.Serialization;
using EasyConnect.Common;

namespace EasyConnect.Protocols
{
    [Serializable]
    public abstract class BaseConnection : IConnection
    {
        protected BaseConnection()
        {
        }

        protected BaseConnection(SerializationInfo info, StreamingContext context)
        {
            IsBookmark = info.GetBoolean("IsBookmark");
            string encryptedPassword = info.GetString("Password");

            if (!String.IsNullOrEmpty(encryptedPassword))
                _encryptedPasswordBytes = Convert.FromBase64String(encryptedPassword);
        }
        
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Password", _password == null ? null : Convert.ToBase64String(CryptoUtilities.Encrypt(_encryptionPassword, _password)));
            info.AddValue("IsBookmark", IsBookmark);
        }

        [NonSerialized]
        protected BookmarksFolder _parentFolder = null;
        [NonSerialized]
        protected SecureString _encryptionPassword = null;
        [NonSerialized]
        protected SecureString _password = null;
        [NonSerialized]
        protected byte[] _encryptedPasswordBytes = null;

        public abstract string Uri
        {
            get;
        }

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

        public virtual Guid Guid
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

                        password.AppendChar((char)decryptedPassword[i]);
                        decryptedPassword[i] = 0;
                    }

                    _password = password;

                    for (int i = 0; i < _encryptedPasswordBytes.Length; i++)
                        _encryptedPasswordBytes[i] = 0;

                    _encryptedPasswordBytes = null;
                }
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
            ((BaseConnection)clonedConnection).EncryptionPassword = _encryptionPassword;

            return clonedConnection;
        }

        public virtual object CloneAnon()
        {
            object clonedConnection = SerializationUtilities.Clone(this);

            ((BaseConnection)clonedConnection).ParentFolder = null;
            ((BaseConnection)clonedConnection).Guid = new Guid();
            ((BaseConnection)clonedConnection).EncryptionPassword = null;

            return clonedConnection;
        }
    }
}
