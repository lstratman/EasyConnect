using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using EasyConnect.Protocols;

namespace EasyConnect
{
	/// <summary>
	/// Global options for the application, currently just includes the flag indicating whether the toolbar should be automatically hidden in 
	/// <see cref="ConnectionWindow"/> instances.
	/// </summary>
	[Serializable]
	public class Options
	{
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

		/// <summary>
		/// Deserializes an instance of this class from an XML file on disk.
		/// </summary>
		/// <returns></returns>
		public static Options Load()
		{
			// If the options file doesn't exist yet (first time the application is being run), just create a new instance of the class
			if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml"))
				return new Options();

			using (
				XmlReader reader =
					new XmlTextReader(
						new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml", FileMode.Open))
				)
			{
				XmlSerializer serializer = new XmlSerializer(typeof (Options));
				return (Options) serializer.Deserialize(reader);
			}
		}

		/// <summary>
		/// Serializes the option data to disk in an XML file.
		/// </summary>
		public void Save()
		{
			XmlSerializer serializer = new XmlSerializer(GetType());

			using (
				FileStream fileStream = new FileStream(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml", FileMode.Create))
				serializer.Serialize(fileStream, this);
		}
	}
}