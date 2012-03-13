using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Xml.Serialization;
using Stratman.Windows.Forms.TitleBarTabs;

namespace EasyConnect
{
    /// <summary>
    /// Folder that contains <see cref="RdpConnection"/>s and child <see cref="BookmarksFolder"/> instances.
    /// </summary>
    [Serializable]
    public class BookmarksFolder : ICloneable
    {
        /// <summary>
        /// Bookmarked connections contained in this folder.
        /// </summary>
        protected ListWithEvents<RdpConnection> _bookmarks = new ListWithEvents<RdpConnection>();

        /// <summary>
        /// Folders beneath this one in the hierarchy.
        /// </summary>
        protected ListWithEvents<BookmarksFolder> _childFolders = new ListWithEvents<BookmarksFolder>();

        [NonSerialized]
        protected BookmarksFolder _parentFolder = null;

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
        public ListWithEvents<RdpConnection> Bookmarks
        {
            get
            {
                return _bookmarks;
            }
        }

        public SecureString EncryptionPassword
        {
            set
            {
                foreach (RdpConnection bookmark in Bookmarks)
                    bookmark.EncryptionPassword = value;

                foreach (BookmarksFolder childFolder in ChildFolders)
                    childFolder.EncryptionPassword = value;
            }
        }

        public object Clone()
        {
            BookmarksFolder clonedFolder = new BookmarksFolder
                                               {
                                                   Name = Name
                                               };

            foreach (RdpConnection bookmark in Bookmarks)
                clonedFolder.Bookmarks.Add((RdpConnection)bookmark.Clone());

            foreach (BookmarksFolder childFolder in ChildFolders)
                clonedFolder.ChildFolders.Add((BookmarksFolder)childFolder.Clone());

            return clonedFolder;
        }

        public void MergeFolder(BookmarksFolder childFolder)
        {
            if (ChildFolders.Any(f => f.Name == childFolder.Name))
            {
                BookmarksFolder mergeTarget = ChildFolders.First(f => f.Name == childFolder.Name);

                foreach (RdpConnection bookmark in childFolder.Bookmarks)
                    mergeTarget.Bookmarks.Add(bookmark);

                foreach (BookmarksFolder folder in childFolder.ChildFolders)
                    mergeTarget.MergeFolder(folder);
            }

            else
                ChildFolders.Add(childFolder);
        }
    }
}