using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Text;

namespace EasyConnect.Protocols
{
    public interface IConnection : ICloneable, ISerializable
    {
        string Uri
        {
            get;
        }

        string Host
        {
            get;
        }

        string DisplayName
        {
            get;
        }

        string Name
        {
            get;
            set;
        }

        BookmarksFolder ParentFolder
        {
            get;
            set;
        }

        Guid Guid
        {
            get;
            set;
        }

        SecureString EncryptionPassword
        {
            set;
        }

        bool IsBookmark
        {
            get;
            set;
        }

        object CloneAnon();
    }
}
