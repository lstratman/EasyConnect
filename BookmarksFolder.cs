using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyConnect
{
    public class BookmarksFolder
    {
        protected List<BookmarksFolder> _childFolders = new List<BookmarksFolder>();
        protected List<RDCConnection> _bookmarks = new List<RDCConnection>();

        public string Name
        {
            get;
            set;
        }

        public List<BookmarksFolder> ChildFolders
        {
            get
            {
                return _childFolders;
            }
        }

        public List<RDCConnection> Bookmarks
        {
            get
            {
                return _bookmarks;
            }
        }
    }
}
