using System.Collections.Generic;
using Stratman.Windows.Forms.TitleBarTabs;

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
        protected ListWithEvents<RDCConnection> _bookmarks = new ListWithEvents<RDCConnection>();

        /// <summary>
        /// Folders beneath this one in the hierarchy.
        /// </summary>
        protected ListWithEvents<BookmarksFolder> _childFolders = new ListWithEvents<BookmarksFolder>();

        public BookmarksFolder()
        {
            _bookmarks.CollectionModified += _bookmarks_CollectionModified;
            _childFolders.CollectionModified += _childFolders_CollectionModified;
        }

        void _childFolders_CollectionModified(object sender, ListModificationEventArgs e)
        {
            if (e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded || e.Modification == ListModification.ItemModified)
            {
                for (int i = e.StartIndex; i < e.StartIndex + e.Count; i++)
                    _childFolders[i].ParentFolder = this;
            }
        }

        void _bookmarks_CollectionModified(object sender, ListModificationEventArgs e)
        {
            if (e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded || e.Modification == ListModification.ItemModified)
            {
                for (int i = e.StartIndex; i < e.StartIndex + e.Count; i++)
                    _bookmarks[i].ParentFolder = this;
            }
        }

        public BookmarksFolder ParentFolder
        {
            get;
            set;
        }

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
        public ListWithEvents<BookmarksFolder> ChildFolders
        {
            get
            {
                return _childFolders;
            }
        }

        /// <summary>
        /// Bookmarked connections contained in this folder.
        /// </summary>
        public ListWithEvents<RDCConnection> Bookmarks
        {
            get
            {
                return _bookmarks;
            }
        }
    }
}