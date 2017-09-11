using EasyConnect.Protocols;
using EasyTabs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
#if APPX
using Windows.Storage;
#endif
using static EasyConnect.HistoryWindow;

namespace EasyConnect
{
    public class History
    {
#if !APPX
        /// <summary>
		/// Filename where history data is loaded from/saved to.
		/// </summary>
		private static string HistoryFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyConnect", "History.xml");
#endif

        private ListWithEvents<HistoricalConnection> _connections = new ListWithEvents<HistoricalConnection>();

        private History()
        {
        }

        public static History Instance
        {
            get;
            private set;
        }

        public ListWithEvents<HistoricalConnection> Connections
        {
            get
            {
                return _connections;
            }
        }

        /// <summary>
		/// Returns the connection, if any, in <see cref="_connections"/> whose <see cref="IConnection.Guid"/> matches <paramref name="historyGuid"/>.
		/// </summary>
		/// <param name="historyGuid">GUID we should use when matching against <see cref="IConnection.Guid"/> for each history entry.</param>
		/// <returns>The connection, if any, in <see cref="_connections"/> whose <see cref="IConnection.Guid"/> matches 
		/// <paramref name="historyGuid"/>.</returns>
		public IConnection FindInHistory(Guid historyGuid)
        {
            return _connections.FirstOrDefault((HistoricalConnection c) => c.Connection.Guid == historyGuid) == null
                       ? null
                       : _connections.First((HistoricalConnection c) => c.Connection.Guid == historyGuid).Connection;
        }

        /// <summary>
        /// Adds a connection that the user has made to the list of historical connections.
        /// </summary>
        /// <param name="connection">Connection that was made that should be added to the history.</param>
        public async Task AddToHistory(IConnection connection)
        {
            HistoricalConnection historyEntry = new HistoricalConnection(connection)
            {
                LastConnection = DateTime.Now
            };

            AddToHistory(historyEntry);
            await Save();
        }

        /// <summary>
		/// Adds <paramref name="historyEntry"/> to <see cref="_connections"/>.
		/// </summary>
		/// <param name="historyEntry">Connection that we're adding to the history.</param>
		protected void AddToHistory(HistoricalConnection historyEntry)
        {
            _connections.Add(historyEntry);
        }

        public static async Task Init()
        {
            Instance = new History();

#if APPX
            IStorageFile historyFile = (IStorageFile)await ApplicationData.Current.LocalFolder.TryGetItemAsync("History.xml");

            if (historyFile == null)
            {
                return;
            }

            string historyFileText = await FileIO.ReadTextAsync(historyFile);

            using (StringReader historyFileTextReader = new StringReader(historyFileText))
            using (XmlReader historyXmlReader = new XmlTextReader(historyFileTextReader))
            {
                XmlSerializer historySerializer = new XmlSerializer(typeof(List<HistoricalConnection>));
                List<HistoricalConnection> historicalConnections = (List<HistoricalConnection>)historySerializer.Deserialize(historyXmlReader);

                foreach (HistoricalConnection historyEntry in historicalConnections)
                    Instance.AddToHistory(historyEntry);
            }
#else
            // Load the history data
            if (File.Exists(HistoryFileName))
            {
                XmlSerializer historySerializer = new XmlSerializer(typeof(List<HistoricalConnection>));
                List<HistoricalConnection> historicalConnections = null;

                using (XmlReader historyReader = new XmlTextReader(HistoryFileName))
                    historicalConnections = (List<HistoricalConnection>)historySerializer.Deserialize(historyReader);

                foreach (HistoricalConnection historyEntry in historicalConnections)
                    Instance.AddToHistory(historyEntry);
            }
#endif
        }

        public async Task Save()
        {
            XmlSerializer historySerializer = new XmlSerializer(typeof(List<HistoricalConnection>));

#if APPX
            // Remove all connections older than two weeks
            foreach (HistoricalConnection connection in _connections.Where(c => c.LastConnection < DateTime.Now.AddDays(-14)).ToList())
            {
                _connections.Remove(connection);
            }

            IStorageFile historyFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("History.xml", CreationCollisionOption.ReplaceExisting);

            StringWriter historyFileText = new StringWriter();
            historySerializer.Serialize(historyFileText, _connections.ToList());

            await FileIO.WriteTextAsync(historyFile, historyFileText.ToString());
#else
            FileInfo destinationFile = new FileInfo(HistoryFileName);

			// ReSharper disable AssignNullToNotNullAttribute
			Directory.CreateDirectory(destinationFile.DirectoryName);
			// ReSharper restore AssignNullToNotNullAttribute

			// Remove all connections older than two weeks
			foreach (HistoricalConnection connection in _connections.Where(c => c.LastConnection < DateTime.Now.AddDays(-14)).ToList())
			{
				_connections.Remove(connection);
			}

			using (XmlWriter historyWriter = new XmlTextWriter(HistoryFileName, new UnicodeEncoding()))
			{
				historySerializer.Serialize(historyWriter, _connections.ToList());
				historyWriter.Flush();
			}
#endif
        }
    }
}
