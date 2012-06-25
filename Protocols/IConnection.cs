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
        string Host
        {
            get;
            set;
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

        bool IsBookmark
        {
            get;
            set;
        }

        object CloneAnon();
    }
}
