using System;
using System.Linq;
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

		/// <summary>
		/// Pointer to the folder that contains this folder.  Not serialized because of the fact that this data is implicit in the hierarchical folder 
		/// structure for bookmarks.
		/// </summary>
		[NonSerialized]
		protected BookmarksFolder _parentFolder = null;

		/// <summary>
		/// Password that should be used for all descendant <see cref="IConnection"/>s unless they have their own value explicitly defined.
		/// </summary>
		[NonSerialized]
		protected SecureString _password = null;

		/// <summary>
		/// Default constructor, wires up the collection modification events in <see cref="_bookmarks"/> and <see cref="_childFolders"/>.
		/// </summary>
		public BookmarksFolder()
		{
			_bookmarks.CollectionModified += _bookmarks_CollectionModified;
			_childFolders.CollectionModified += _childFolders_CollectionModified;
		}

		/// <summary>
		/// Pointer to the folder that contains this folder.  Not serialized because of the fact that this data is implicit in the hierarchical folder 
		/// structure for bookmarks.
		/// </summary>
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
		/// Username that should be used for all descendant <see cref="IConnection"/>s unless they have their own value explicitly defined.
		/// </summary>
		public string Username
		{
			get;
			set;
		}

		/// <summary>
		/// Password that should be used for all descendant <see cref="IConnection"/>s unless they have their own value explicitly defined.
		/// </summary>
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
			}
		}

		/// <summary>
		/// Encrypted and Base64-encoded data in <see cref="_password"/>
		/// </summary>
		[XmlElement("Password")]
		public string EncryptedPassword
		{
			get
			{
				if (Password == null || Password.Length == 0)
					return null;

				return Convert.ToBase64String(ConnectionFactory.Encrypt(_password));
			}

			set
			{
				if (String.IsNullOrEmpty(value))
				{
					Password = null;
					return;
				}

				// Decrypt the password and put it into a secure string
				SecureString password = new SecureString();
				byte[] decryptedPassword = ConnectionFactory.Decrypt(Convert.FromBase64String(value));

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

		/// <summary>
		/// Handler method that's called when the contents of <see cref="_childFolders"/> are modified.  Sets the <see cref="ParentFolder"/> property of each
		/// added/modified child folder to this folder.
		/// </summary>
		/// <param name="sender">Object from which this event originated; <see cref="_childFolders"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _childFolders_CollectionModified(object sender, ListModificationEventArgs e)
		{
			if (e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded || e.Modification == ListModification.ItemModified)
			{
				for (int i = e.StartIndex; i < e.StartIndex + e.Count; i++)
					_childFolders[i].ParentFolder = this;
			}
		}

		/// <summary>
		/// Handler method that's called when the contents of <see cref="_bookmarks"/> are modified.  Sets the <see cref="IConnection.ParentFolder"/> property 
		/// of each added/modified bookmark to this folder.
		/// </summary>
		/// <param name="sender">Object from which this event originated; <see cref="_bookmarks"/> in this case.</param>
		/// <param name="e">Arguments associated with this event.</param>
		private void _bookmarks_CollectionModified(object sender, ListModificationEventArgs e)
		{
			if (e.Modification == ListModification.ItemAdded || e.Modification == ListModification.RangeAdded || e.Modification == ListModification.ItemModified)
			{
				for (int i = e.StartIndex; i < e.StartIndex + e.Count; i++)
					_bookmarks[i].ParentFolder = this;
			}
		}

		/// <summary>
		/// Creates a cloned copy of this folder as well as all descendant folders and bookmarks.
		/// </summary>
		/// <returns>A cloned copy of this folder as well as all descendant folders and bookmarks.</returns>
		public object Clone()
		{
			BookmarksFolder clonedFolder = new BookmarksFolder
				                               {
					                               Name = Name
				                               };

			foreach (IConnection bookmark in Bookmarks)
				clonedFolder.Bookmarks.Add((IConnection) bookmark.Clone());

			foreach (BookmarksFolder childFolder in ChildFolders)
				clonedFolder.ChildFolders.Add((BookmarksFolder) childFolder.Clone());

			return clonedFolder;
		}

		/// <summary>
		/// Creates a cloned copy of this folder as well as all descendant folders and bookmarks.  For the bookmarks, all sensitive data, such as usernames
		/// and passwords, are scrubbed.
		/// </summary>
		/// <returns>A cloned copy of this folder as well as all descendant folders and bookmarks.</returns>
		public object CloneAnon()
		{
			BookmarksFolder clonedFolder = new BookmarksFolder
				                               {
					                               Name = Name
				                               };

			foreach (IConnection bookmark in Bookmarks)
				clonedFolder.Bookmarks.Add((IConnection) bookmark.CloneAnon());

			foreach (BookmarksFolder childFolder in ChildFolders)
				clonedFolder.ChildFolders.Add((BookmarksFolder) childFolder.CloneAnon());

			return clonedFolder;
		}

		/// <summary>
		/// When pasting a child folder into this folder, if a child folder by the same name already exists, we merge the contents of 
		/// <paramref name="childFolder"/> with that folder.  Bookmarks in <paramref name="childFolder"/> are copied to the destination folder; we don't
		/// overwrite any bookmarks that have the same name.  This merge process is then carried out recursively on all descendant folders of 
		/// <paramref name="childFolder"/>.
		/// </summary>
		/// <param name="childFolder">Child folder that we are copying or merging into this folder.</param>
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

		/// <summary>
		/// Required method for <see cref="IXmlSerializable"/> that returns the schema to use during serialization.
		/// </summary>
		/// <returns>Null in all cases.</returns>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Deserializes the data for a bookmarks folder from <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">XML source that we're deserializing this folder from.</param>
		public void ReadXml(XmlReader reader)
		{
			// Move to the child nodes
			reader.MoveToContent();
			reader.Read();

			while (reader.MoveToContent() == XmlNodeType.Element)
			{
				switch (reader.LocalName)
				{
					case "Name":
						Name = reader.ReadElementContentAsString();
						break;

					case "Username":
						Username = reader.ReadElementContentAsString();
						break;

					case "Password":
						EncryptedPassword = reader.ReadElementContentAsString();
						break;

					case "ChildFolders":
						if (!reader.IsEmptyElement)
						{
							reader.Read();

							// Call this method recursively to read each child folder
							while (reader.MoveToContent() == XmlNodeType.Element)
							{
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
								Bookmarks.Add(ConnectionFactory.Deserialize(reader));
						}

						reader.Read();

						break;
				}
			}

			reader.Read();
		}

		/// <summary>
		/// Serializes the data for this bookmarks folder to <paramref name="writer"/>.
		/// </summary>
		/// <param name="writer">XML destination that we are to serialize this bookmarks folder to.</param>
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteElementString("Name", Name);

			if (Password != null)
				writer.WriteElementString("Password", EncryptedPassword);

			if (!String.IsNullOrEmpty(Username))
				writer.WriteElementString("Username", Username);

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