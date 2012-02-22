using System.Collections.Generic;

namespace EasyConnect
{
    /// <summary>
    /// Folder that contains <see cref="RDCConnection"/>s and child <see cref="BookmarksFolder"/> instances.
    /// </summary>
    public class BookmarksFolder
    {
        /// <summary>
        /// Bookmarked connections contained in this folder.
        /// </summary>
        protected List<RDCConnection> _bookmarks = new List<RDCConnection>();

        /// <summary>
        /// Folders beneath this one in the hierarchy.
        /// </summary>
        protected List<BookmarksFolder> _childFolders = new List<BookmarksFolder>();

        /// <summary>
        /// Name used to identify this folder.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Folders beneath this one in the hierarchy.
        /// </summary>
        public List<BookmarksFolder> ChildFolders
        {
            get
            {
                return _childFolders;
            }
        }

        /// <summary>
        /// Bookmarked connections contained in this folder.
        /// </summary>
        public List<RDCConnection> Bookmarks
        {
            get
            {
                return _bookmarks;
            }
        }
    }
}