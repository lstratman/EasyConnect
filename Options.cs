using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using EasyConnect.Protocols;
using System.Threading.Tasks;
#if APPX
using Windows.Storage;
using Windows.Storage.Streams;
#endif

namespace EasyConnect
{
	/// <summary>
	/// Global options for the application, currently just includes the flag indicating whether the toolbar should be automatically hidden in 
	/// <see cref="ConnectionWindow"/> instances.
	/// </summary>
	[Serializable]
	public class Options
	{
        private Options()
        {
        }

		/// <summary>
		/// Flag indicating whether the toolbar in <see cref="ConnectionWindow"/> instances should be hidden when the user is focused on the connection content
		/// area.
		/// </summary>
		public bool AutoHideToolbar
		{
			get;
			set;
		}

		/// <summary>
		/// Type of encryption that should be used to protect passwords and other sensitive data in settings files.
		/// </summary>
		public EncryptionType? EncryptionType
		{
			get;
			set;
		}

		public bool EnableAeroPeek
		{
			get;
			set;
		}

        public static Options Instance
        {
            get;
            private set;
        }

        [XmlIgnore]
        public bool FirstLaunch
        {
            get;
            private set;
        }

		/// <summary>
		/// Deserializes an instance of this class from an XML file on disk.
		/// </summary>
		/// <returns></returns>
		public static async Task Init()
		{
#if APPX
            IStorageFile optionsFile = (IStorageFile) await ApplicationData.Current.LocalFolder.TryGetItemAsync("Options.xml");

            if (optionsFile == null)
            {
                Instance = new Options();
                Instance.FirstLaunch = true;

                return;
            }

            string optionsFileText = await FileIO.ReadTextAsync(optionsFile);

            using (StringReader optionsFileTextReader = new StringReader(optionsFileText))
            using (XmlReader optionsXmlReader = new XmlTextReader(optionsFileTextReader))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Options));
                Instance = (Options)serializer.Deserialize(optionsXmlReader);
            }
#else
            // If the options file doesn't exist yet (first time the application is being run), just create a new instance of the class
			if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml"))
            {
				Instance = new Options();
                Instance.FirstLaunch = true;

                return;
            }

			using (
				XmlReader reader =
					new XmlTextReader(
						new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml", FileMode.Open))
				)
			{
				XmlSerializer serializer = new XmlSerializer(typeof (Options));
				Instance = (Options) serializer.Deserialize(reader);
			}
#endif
        }

        /// <summary>
        /// Serializes the option data to disk in an XML file.
        /// </summary>
        public async Task Save()
		{
			XmlSerializer serializer = new XmlSerializer(GetType());

#if APPX
            IStorageFile optionsFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Options.xml", CreationCollisionOption.ReplaceExisting);

            StringWriter optionsFileText = new StringWriter();
            serializer.Serialize(optionsFileText, this);

            await FileIO.WriteTextAsync(optionsFile, optionsFileText.ToString());
#else
            using (
				FileStream fileStream = new FileStream(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml", FileMode.Create))
				serializer.Serialize(fileStream, this);
#endif
        }
	}
}