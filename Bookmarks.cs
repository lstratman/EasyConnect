using EasyConnect.Protocols;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
#if APPX
using Windows.Storage;
#endif
using System.Windows.Forms;

namespace EasyConnect
{
    public class Bookmarks
    {
        /// <summary>
		/// Full path to the file where bookmarks data is serialized.
		/// </summary>
		private static string BookmarksFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyConnect", "Bookmarks.xml");

        /// <summary>
		/// Root folder in the bookmarks folder structure.
		/// </summary>
		public BookmarksFolder RootFolder
        {
            get;
            private set;
        }

        private Bookmarks()
        {
            RootFolder = new BookmarksFolder();
        }

        public static Bookmarks Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Serializes the bookmarks to disk after first removing any password-sensitive information.  This is accomplished by first calling
        /// <see cref="BookmarksFolder.CloneAnon"/> on <see cref="RootFolder"/> and then serializing it.
        /// </summary>
        /// <param name="path">Path of the file that we are to serialize to.</param>
        public void Export(string path)
        {
            FileInfo destinationFile = new FileInfo(path);
            XmlSerializer bookmarksSerializer = new XmlSerializer(typeof(BookmarksFolder));

            // ReSharper disable AssignNullToNotNullAttribute
            Directory.CreateDirectory(destinationFile.DirectoryName);
            // ReSharper restore AssignNullToNotNullAttribute

            object clonedFolder = Bookmarks.Instance.RootFolder.CloneAnon();

            using (XmlWriter bookmarksWriter = new XmlTextWriter(path, new UnicodeEncoding()))
            {
                bookmarksSerializer.Serialize(bookmarksWriter, clonedFolder);
                bookmarksWriter.Flush();
            }
        }

        /// <summary>
		/// Imports bookmarks previously saved via a call to <see cref="Export"/> and overwrites any existing bookmarks data.
		/// </summary>
		/// <param name="path">Path of the file that we're loading from.</param>
		public async Task<bool> Import(string path)
        {
            if (MessageBox.Show("This will erase any currently saved bookmarks and import the contents of the selected file. Do you wish to continue?", "Continue with import?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
            {
                if (File.Exists(path))
                {
                    XmlSerializer bookmarksSerializer = new XmlSerializer(typeof(BookmarksFolder));

                    using (XmlReader bookmarksReader = new XmlTextReader(path))
                    {
                        RootFolder = (BookmarksFolder)bookmarksSerializer.Deserialize(bookmarksReader);
                    }

                    await Save();

                    return true;
                }
            }

            return false;
        }

        public static async Task Init()
        {
            Instance = new Bookmarks();

#if APPX
            IStorageFile bookmarksFile = (IStorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync("Bookmarks.xml");
            string bookmarksFileText = null;

            // If there isn't a file in the Windows Store app data directory, try the desktop app directory
            if (bookmarksFile == null)
            {
                try
                {
                    if (File.Exists(BookmarksFileName))
                    {
                        bookmarksFileText = File.ReadAllText(BookmarksFileName);
                    }
                }

                catch (Exception)
                {
                }
            }

            else
            {
                bookmarksFileText = await FileIO.ReadTextAsync(bookmarksFile);
            }

            if (String.IsNullOrEmpty(bookmarksFileText))
            {
                return;
            }

            using (StringReader bookmarksFileTextReader = new StringReader(bookmarksFileText))
            using (XmlReader bookmarksXmlReader = new XmlTextReader(bookmarksFileTextReader))
            {
                // Deserialize the bookmarks folder structure from BookmarksFileName; BookmarksFolder.ReadXml() will call itself recursively to deserialize
                // child folders, so all we have to do is start the deserialization process from the root folder
                XmlSerializer bookmarksSerializer = new XmlSerializer(typeof(BookmarksFolder));
                Instance.RootFolder = (BookmarksFolder)bookmarksSerializer.Deserialize(bookmarksXmlReader);
            }
#else
            if (File.Exists(BookmarksFileName))
			{
				// Deserialize the bookmarks folder structure from BookmarksFileName; BookmarksFolder.ReadXml() will call itself recursively to deserialize
				// child folders, so all we have to do is start the deserialization process from the root folder
				XmlSerializer bookmarksSerializer = new XmlSerializer(typeof (BookmarksFolder));

				using (XmlReader bookmarksReader = new XmlTextReader(BookmarksFileName))
					Instance.RootFolder = (BookmarksFolder) bookmarksSerializer.Deserialize(bookmarksReader);
			}
#endif
        }

        public async Task Save()
        {
            XmlSerializer bookmarksSerializer = new XmlSerializer(typeof(BookmarksFolder));

#if APPX
            IStorageFile bookmarksFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Bookmarks.xml", CreationCollisionOption.ReplaceExisting);

            StringWriter bookmarksFileText = new StringWriter();
            bookmarksSerializer.Serialize(bookmarksFileText, RootFolder);

            await FileIO.WriteTextAsync(bookmarksFile, bookmarksFileText.ToString());
#else
            FileInfo destinationFile = new FileInfo(BookmarksFileName);

            // ReSharper disable AssignNullToNotNullAttribute
            Directory.CreateDirectory(destinationFile.DirectoryName);
            // ReSharper restore AssignNullToNotNullAttribute

            using (XmlWriter bookmarksWriter = new XmlTextWriter(BookmarksFileName, new UnicodeEncoding()))
            {
                bookmarksSerializer.Serialize(bookmarksWriter, RootFolder);
                bookmarksWriter.Flush();
            }
#endif
        }

        /// <summary>
		/// Locates the <see cref="IConnection"/> instance for the given <paramref name="bookmarkGuid"/> in the bookmarks folder structure.  Calls 
		/// <see cref="FindBookmark(Guid, BookmarksFolder)"/> to do the actual work.
		/// </summary>
		/// <param name="bookmarkGuid"><see cref="IConnection.Guid"/> value of the <see cref="IConnection"/> that we're searching for.</param>
		/// <returns>The <see cref="IConnection"/> bookmark corresponding to <paramref name="bookmarkGuid"/> if it exists, null otherwise.</returns>
		public IConnection FindBookmark(Guid bookmarkGuid)
        {
            return FindBookmark(bookmarkGuid, RootFolder);
        }

        /// <summary>
        /// Recursive method that searches <paramref name="searchFolder"/> and its descendants for an <see cref="IConnection"/> instance whose 
        /// <see cref="IConnection.Guid"/> property corresponds to <paramref name="bookmarkGuid"/>.  Called from <see cref="FindBookmark(Guid)"/>.
        /// </summary>
        /// <param name="bookmarkGuid"><see cref="IConnection.Guid"/> value of the <see cref="IConnection"/> that we're searching for.</param>
        /// <param name="searchFolder">Current folder that we're searching.</param>
        /// <returns>The <see cref="IConnection"/> bookmark corresponding to <paramref name="bookmarkGuid"/> if it exists, null otherwise.</returns>
        protected IConnection FindBookmark(Guid bookmarkGuid, BookmarksFolder searchFolder)
        {
            IConnection bookmark = searchFolder.Bookmarks.FirstOrDefault(b => b.Guid == bookmarkGuid);

            if (bookmark != null)
                return bookmark;

            foreach (BookmarksFolder childFolder in searchFolder.ChildFolders)
            {
                bookmark = FindBookmark(bookmarkGuid, childFolder);

                if (bookmark != null)
                    return bookmark;
            }

            return null;
        }
    }
}
