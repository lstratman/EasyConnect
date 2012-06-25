using System;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Stratman.Windows.Forms.TitleBarTabs;

namespace EasyConnect.Protocols
{
    /// <summary>
    /// Folder that contains <see cref="IConnection"/>s and child <see cref="BookmarksFolder"/> instances.
    /// </summary>
    [Serializable]
    public class BookmarksFolder : ICloneable, IXmlSerializable
    {
        /// <summary>
        /// Bookmarked connections contained in this folder.
        /// </summary>
        protected ListWithEvents<IConnection> _bookmarks = new ListWithEvents<IConnection>();

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
        public ListWithEvents<IConnection> Bookmarks
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
                foreach (IConnection bookmark in Bookmarks)
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

            foreach (IConnection bookmark in Bookmarks)
                clonedFolder.Bookmarks.Add((IConnection)bookmark.Clone());

            foreach (BookmarksFolder childFolder in ChildFolders)
                clonedFolder.ChildFolders.Add((BookmarksFolder)childFolder.Clone());

            return clonedFolder;
        }

        public object CloneAnon()
        {
            BookmarksFolder clonedFolder = new BookmarksFolder
            {
                Name = Name
            };

            foreach (IConnection bookmark in Bookmarks)
                clonedFolder.Bookmarks.Add((IConnection)bookmark.CloneAnon());

            foreach (BookmarksFolder childFolder in ChildFolders)
                clonedFolder.ChildFolders.Add((BookmarksFolder)childFolder.CloneAnon());

            return clonedFolder;
        }

        public void MergeFolder(BookmarksFolder childFolder)
        {
            if (ChildFolders.Any(f => f.Name == childFolder.Name))
            {
                BookmarksFolder mergeTarget = ChildFolders.First(f => f.Name == childFolder.Name);

                foreach (IConnection bookmark in childFolder.Bookmarks)
                    mergeTarget.Bookmarks.Add(bookmark);

                foreach (BookmarksFolder folder in childFolder.ChildFolders)
                    mergeTarget.MergeFolder(folder);
            }

            else
                ChildFolders.Add(childFolder);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            reader.Read();

            while (reader.MoveToContent() == XmlNodeType.Element)
            {
                switch (reader.LocalName)
                {
                    case "Name":
                        Name = reader.ReadElementContentAsString();
                        break;

                    case "ChildFolders":
                        if (!reader.IsEmptyElement)
                        {
                            while (reader.MoveToContent() == XmlNodeType.Element)
                            {
                                reader.Read();
                                BookmarksFolder childFolder = new BookmarksFolder();
                                childFolder.ReadXml(reader);

                                ChildFolders.Add(childFolder);
                            }
                        }

                        reader.Read();

                        break;

                    case "Bookmarks":
                        if (!reader.IsEmptyElement)
                        {
                            reader.Read();

                            while (reader.MoveToContent() == XmlNodeType.Element)
                                Bookmarks.Add(ProtocolFactory.Deserialize(reader));
                        }

                        reader.Read();

                        break;
                }
            }

            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("Name", Name);
            writer.WriteStartElement("ChildFolders");

            foreach (BookmarksFolder childFolder in ChildFolders)
            {
                writer.WriteStartElement("BookmarksFolder");
                childFolder.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();

            writer.WriteStartElement("Bookmarks");

            foreach (IConnection bookmark in Bookmarks)
            {
                XmlSerializer bookmarkSerializer = new XmlSerializer(bookmark.GetType());
                bookmarkSerializer.Serialize(writer, bookmark);
            }

            writer.WriteEndElement();
        }
    }
}