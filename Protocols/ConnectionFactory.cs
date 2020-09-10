using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
#if APPX
using Windows.Storage;
#endif

namespace EasyConnect.Protocols
{
	/// <summary>
	/// Helper class for getting a list of all connection protocol, getting and setting connection protocol defaults, and getting connections for URIs
	/// input by the user (i.e. rdp://somehost or ssh://anotherhost).
	/// </summary>
	public class ConnectionFactory
	{
#if !APPX
        /// <summary>
        /// Filename where history data is loaded from/saved to.
        /// </summary>
        private static string DefaultsFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyConnect", "Defaults.xml");
#endif

        /// <summary>
        /// A lookup associating each protocol type with an instantiated copy of that protocol class.
        /// </summary>
        protected static Dictionary<Type, IProtocol> _protocols = new Dictionary<Type, IProtocol>();

		/// <summary>
		/// A lookup associating each protocol type with a default <see cref="IConnection"/> instance for that protocol.
		/// </summary>
		protected static Dictionary<Type, IConnection> _defaults = null;

		/// <summary>
		/// Password to use when encrypting and decrypting sensitive data in <see cref="IConnection"/> instances.
		/// </summary>
		protected static SecureString _encryptionPassword = null;

		/// <summary>
		/// Default URI prefix to assume when the user enters a URI manually without specifying a protocol.
		/// </summary>
		protected static string _defaultProtocolPrefix = null;

		/// <summary>
		/// Crypto object used in the <see cref="Encrypt(byte[])"/> and <see cref="Decrypt"/> methods.
		/// </summary>
		protected static object _crypto;

		/// <summary>
		/// Type of encryption that should be used to protect passwords and other sensitive data in settings files.
		/// </summary>
		protected static EncryptionType _encryptionType;

		/// <summary>
		/// Static constructor.  Reads all .dll files in the application's directory and looks for types implementing <see cref="IProtocol"/>; instances of
		/// these types are created and added to <see cref="_protocols"/>.
		/// </summary>
		static ConnectionFactory()
		{
			foreach (string assemblyFile in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories))
			{
				try
				{
					Assembly loadedAssembly = Assembly.LoadFile(assemblyFile);

					// Get each type in the assembly that implements IProtocol
					foreach (Type type in from type in loadedAssembly.GetTypes()
					                      where type.IsClass && !type.IsNotPublic && !type.IsAbstract
					                      let interfaces = type.GetInterfaces()
					                      where interfaces.Contains(typeof (IProtocol))
					                      select type)
						_protocols[type] = (IProtocol) Activator.CreateInstance(type);
				}

					// Ignore any loading, reflection, or image format exceptions that we run into
				catch (FileLoadException)
				{
				}

				catch (BadImageFormatException)
				{
				}

				catch (ReflectionTypeLoadException)
				{
				}
			}
		}

		/// <summary>
		/// Privatized constructor to keep instances of this class from being created.
		/// </summary>
		private ConnectionFactory()
		{
		}

		/// <summary>
		/// Type of encryption that should be used to protect passwords and other sensitive data in settings files.
		/// </summary>
		public static EncryptionType EncryptionType
		{
			get
			{
				return _encryptionType;
			}
		}

		/// <summary>
		/// Flag indicating whether the <see cref="_crypto"/> object has been initialized.
		/// </summary>
		public static bool ReadyForCrypto
		{
			get
			{
				return _crypto != null;
			}
		}

		/// <summary>
		/// Decrypts <paramref name="data"/> by using the <see cref="_crypto"/> currently set.
		/// </summary>
		/// <param name="data">Data that we are to decrypt.</param>
		/// <returns>Decrypted data.</returns>
		public static byte[] Decrypt(byte[] data)
		{
			// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
			if (_crypto is SymmetricAlgorithm)
			{
				byte[] decryptedData = new byte[data.Length];
				MemoryStream memoryStream = new MemoryStream(data, 0, data.Length);
				CryptoStream cryptoStream = new CryptoStream(memoryStream, ((SymmetricAlgorithm) _crypto).CreateDecryptor(), CryptoStreamMode.Read);

				cryptoStream.Read(decryptedData, 0, decryptedData.Length);
				cryptoStream.Close();
				memoryStream.Close();

				return decryptedData;
			}

			else if (_crypto is RSACryptoServiceProvider)
				return ((RSACryptoServiceProvider) _crypto).Decrypt(data, true);

			else
				throw new NotSupportedException("The crypto object " + _crypto.GetType().Name + " is not supported.");
			// ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
		}

		/// <summary>
		/// Encrypts <paramref name="data"/> by using the <see cref="_crypto"/> currently set.
		/// </summary>
		/// <param name="data">Data that we are to encrypt.</param>
		/// <returns>Encrypted data.</returns>
		public static byte[] Encrypt(byte[] data)
		{
			// ReSharper disable CanBeReplacedWithTryCastAndCheckForNull
			if (_crypto is SymmetricAlgorithm)
			{
				MemoryStream memoryStream = new MemoryStream();
				CryptoStream cryptoStream = new CryptoStream(memoryStream, ((SymmetricAlgorithm) _crypto).CreateEncryptor(), CryptoStreamMode.Write);

				cryptoStream.Write(data, 0, data.Length);
				cryptoStream.Close();
				memoryStream.Close();

				return memoryStream.ToArray();
			}

			else if (_crypto is RSACryptoServiceProvider)
				return ((RSACryptoServiceProvider) _crypto).Encrypt(data, true);

			else
				throw new NotSupportedException("The crypto object " + _crypto.GetType().Name + " is not supported.");
			// ReSharper restore CanBeReplacedWithTryCastAndCheckForNull
		}

		/// <summary>
		/// Encrypts a password specified in <paramref name="data"/> by using the <see cref="_crypto"/> currently set.
		/// </summary>
		/// <param name="data">Data that we are to encrypt.</param>
		/// <returns>Encrypted data.</returns>
		public static byte[] Encrypt(SecureString data)
		{
			IntPtr marshalledDataBytes = Marshal.SecureStringToGlobalAllocAnsi(data);
			byte[] dataBytes = new byte[data.Length];

			Marshal.Copy(marshalledDataBytes, dataBytes, 0, dataBytes.Length);

			byte[] encryptedData = Encrypt(dataBytes);

			// Clear the data bytes from memory
			for (int i = 0; i < dataBytes.Length; i++)
				dataBytes[i] = 0;

			Marshal.ZeroFreeGlobalAllocAnsi(marshalledDataBytes);

			return encryptedData;
		}

		/// <summary>
		/// Set the <see cref="EncryptionType"/> and <see cref="_crypto"/> objects.
		/// </summary>
		/// <param name="encryptionType">Encryption type to be used.</param>
		public static void SetEncryptionType(EncryptionType encryptionType)
		{
			SetEncryptionType(encryptionType, null);
		}

		/// <summary>
		/// Set the <see cref="EncryptionType"/> and <see cref="_crypto"/> objects.
		/// </summary>
		/// <param name="encryptionType">Encryption type to be used.</param>
		/// <param name="encryptionKey">If <paramref name="encryptionType"/> is a symmetric algorithm, this represents the encryption key to use.</param>
		public static void SetEncryptionType(EncryptionType encryptionType, SecureString encryptionKey)
		{
			switch (encryptionType)
			{
					// Simply open or create an RSA key container called EasyConnect
				case EncryptionType.Rsa:
					CspParameters parameters = new CspParameters
						                           {
							                           KeyContainerName = "EasyConnect"
						                           };

					_crypto = new RSACryptoServiceProvider(parameters);

					break;

					// Initialize a Rijndael instance with the key in encryptionKey
				case EncryptionType.Rijndael:
					if (encryptionKey == null)
						throw new ArgumentException("When Rijndael is used as the encryption type, the encryption password cannot be null.", "encryptionKey");

					Rijndael rijndael = Rijndael.Create();
					rijndael.KeySize = 256;

					// Get the bytes for the password
					IntPtr marshalledKeyBytes = Marshal.SecureStringToGlobalAllocAnsi(encryptionKey);
					byte[] keyBytes = new byte[rijndael.KeySize / 8];

					Marshal.Copy(marshalledKeyBytes, keyBytes, 0, Math.Min(keyBytes.Length, encryptionKey.Length));

					// Set the encryption key to the key bytes and the IV to a predetermined string
					rijndael.Key = keyBytes;
					rijndael.IV = Convert.FromBase64String("QGWyKbe+W9H0mL2igm73jw==");

					Marshal.ZeroFreeGlobalAllocAnsi(marshalledKeyBytes);

					_crypto = rijndael;

					break;

				default:
					throw new ArgumentException("The encryption type " + encryptionType.ToString("G") + " is not supported.", "encryptionType");
			}

			_encryptionType = encryptionType;
		}

		/// <summary>
		/// Sets the default protocol to assume when the user enters a URI manually without specifying a prefix.
		/// </summary>
		/// <param name="protocol">Connection protocol that we want to use by default.</param>
		public static async Task SetDefaultProtocol(IProtocol protocol)
		{
			if (_defaults == null)
				await InitializeDefaults();

			_defaultProtocolPrefix = protocol.ProtocolPrefix;
			await SaveDefaults();
		}

		/// <summary>
		/// Gets the default protocol to assume when the user enters a URI manually without specifying a prefix.
		/// </summary>
		/// <returns>Connection protocol that we want to use by default.</returns>
		public static async Task<IProtocol> GetDefaultProtocol()
		{
			if (_defaults == null)
				await InitializeDefaults();

			return _protocols.First(
				pair => pair.Value.ProtocolPrefix == (String.IsNullOrEmpty(_defaultProtocolPrefix)
					                                      ? "rdp"
					                                      : _defaultProtocolPrefix)).Value;
		}

		/// <summary>
		/// Deserializes the defaults for each protocol type as well as the default protocol to assume when the user enters a URI manually without specifying
		/// a prefix.
		/// </summary>
		protected static async Task InitializeDefaults()
		{
            _defaults = new Dictionary<Type, IConnection>();

#if APPX
            IStorageFile defaultsFile = (IStorageFile) await ApplicationData.Current.LocalFolder.TryGetItemAsync("Defaults.xml");

            if (defaultsFile == null)
            {
                return;
            }

            string defaultsFileText = await FileIO.ReadTextAsync(defaultsFile);

            using (StringReader defaultsFileTextReader = new StringReader(defaultsFileText))
            using (XmlReader defaultsXmlReader = new XmlTextReader(defaultsFileTextReader))
            {
                defaultsXmlReader.MoveToContent();

                _defaultProtocolPrefix = defaultsXmlReader.GetAttribute("defaultProtocolPrefix");

                defaultsXmlReader.Read();

                // The node name for each node specifies the protocol type
                while (defaultsXmlReader.MoveToContent() == XmlNodeType.Element)
                {
                    IConnection defaults = Deserialize(defaultsXmlReader);
                    IProtocol protocol = _protocols.First(pair => pair.Value.ConnectionType == defaults.GetType()).Value;

                    _defaults[protocol.GetType()] = defaults;
                }
            }
#else
            if (File.Exists(DefaultsFileName))
			{
				using (XmlReader reader = new XmlTextReader(new FileStream(DefaultsFileName, FileMode.Open, FileAccess.Read)))
				{
					reader.MoveToContent();

					_defaultProtocolPrefix = reader.GetAttribute("defaultProtocolPrefix");

					reader.Read();

					// The node name for each node specifies the protocol type
					while (reader.MoveToContent() == XmlNodeType.Element)
					{
						IConnection defaults = Deserialize(reader);
						IProtocol protocol = _protocols.First(pair => pair.Value.ConnectionType == defaults.GetType()).Value;

						_defaults[protocol.GetType()] = defaults;
					}
				}
			}
#endif
        }

		/// <summary>
		/// Serializes the defaults for each protocol type as well as the default protocol to assume when the user enters a URI manually without specifying
		/// a prefix.
		/// </summary>
		protected static async Task SaveDefaults()
		{
#if APPX
		    IStorageFile defaultsFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("Defaults.xml", CreationCollisionOption.ReplaceExisting);
		    StringWriter defaultsFileText = new StringWriter();

		    using (XmlWriter writer = new XmlTextWriter(defaultsFileText))
		    {
		        writer.WriteStartElement("Defaults");
		        writer.WriteAttributeString("defaultProtocolPrefix", _defaultProtocolPrefix);

		        foreach (IConnection connection in _defaults.Values)
		        {
		            XmlSerializer serializer = new XmlSerializer(connection.GetType());
		            serializer.Serialize(writer, connection);
		        }

		        writer.WriteEndElement();
		    }

            await FileIO.WriteTextAsync(defaultsFile, defaultsFileText.ToString());
#else
            using (XmlWriter writer = new XmlTextWriter(new FileStream(DefaultsFileName, FileMode.Create), Encoding.Unicode))
			{
				writer.WriteStartElement("Defaults");
				writer.WriteAttributeString("defaultProtocolPrefix", _defaultProtocolPrefix);

				foreach (IConnection connection in _defaults.Values)
				{
					XmlSerializer serializer = new XmlSerializer(connection.GetType());
					serializer.Serialize(writer, connection);
				}

				writer.WriteEndElement();
			}
#endif
        }

		/// <summary>
		/// Sets the defaults to use when creating a new connection for a particular protocol.
		/// </summary>
		/// <param name="defaults">Default connection data to use when creating a new connection for the protocol.</param>
		public static async Task SetDefaults(IConnection defaults)
		{
			if (_defaults == null)
				await InitializeDefaults();

			// Get the protocol for this connection type
			IProtocol protocol = _protocols.First(pair => pair.Value.ConnectionType == defaults.GetType()).Value;

			_defaults[protocol.GetType()] = defaults;
			await SaveDefaults();
		}

		/// <summary>
		/// Gets the defaults to use when creating a new connection for a particular protocol.
		/// </summary>
		/// <param name="protocolType">Protocol type that we are to retrieve defaults for.</param>
		/// <returns>Default connection data to use when creating a new connection for <paramref name="protocolType"/>.</returns>
		public static async Task<IConnection> GetDefaults(Type protocolType)
		{
			if (_defaults == null)
				await InitializeDefaults();

			return await GetDefaults(_protocols[protocolType]);
		}

		/// <summary>
		/// Gets the defaults to use when creating a new connection for a particular protocol.
		/// </summary>
		/// <param name="protocol">Protocol that we are to retrieve defaults for.</param>
		/// <returns>Default connection data to use when creating a new connection for <paramref name="protocol"/>.</returns>
		public static async Task<IConnection> GetDefaults(IProtocol protocol)
		{
			if (_defaults == null)
				await InitializeDefaults();

			return _defaults.ContainsKey(protocol.GetType())
				       ? _defaults[protocol.GetType()]
				       : (IConnection) Activator.CreateInstance(_protocols[protocol.GetType()].ConnectionType);
		}

		/// <summary>
		/// Gets the list of all connection protocols available for this application.
		/// </summary>
		/// <returns>The list of all connection protocols available for this application.</returns>
		public static List<IProtocol> GetProtocols()
		{
			return _protocols.Values.ToList();
		}

		/// <summary>
		/// Gets the protocol for a particular <paramref name="connection"/>.
		/// </summary>
		/// <param name="connection">Connection that we want the protocol for.</param>
		/// <returns>Protocol associated with <paramref name="connection"/>.</returns>
		public static IProtocol GetProtocol(IConnection connection)
		{
			return _protocols.FirstOrDefault(p => p.Value.ConnectionType == connection.GetType()).Value;
		}

		/// <summary>
		/// When reading protocol defaults in <see cref="InitializeDefaults"/>, this gets the type to use for each node by looking at its 
		/// <see cref="XmlNode.LocalName"/>, loading the appropriate type, and deserializing the node using that type.
		/// </summary>
		/// <param name="reader">Reader that we're getting default connection data from.</param>
		/// <returns>The deserialized default connection data for the node.</returns>
		public static IConnection Deserialize(XmlReader reader)
		{
			string typeName = reader.LocalName;
			IProtocol protocol = _protocols.First(pair => pair.Value.ConnectionType.Name == typeName).Value;
			XmlSerializer bookmarkSerializer = new XmlSerializer(protocol.ConnectionType);

			return (IConnection) bookmarkSerializer.Deserialize(reader);
		}

		/// <summary>
		/// When the user enters a URI manually, we get the protocol to use for that URI (either by matching the prefix or using the default one) and then
		/// return the default connection data to use for that protocol.
		/// </summary>
		/// <param name="uri">URI that the user has entered.</param>
		/// <returns>The default connection data to use for the protocol corresponding to <paramref name="uri"/>.</returns>
		public static async Task<IConnection> GetConnection(string uri)
		{
			// Get the protocol for the URI
			Regex uriCompontents = new Regex("^(?<prefix>(?<protocol>.+)://){0,1}(?<host>.+)$");
			Match match = uriCompontents.Match(uri);
			string protocolPrefix = match.Groups["protocol"].Success
				                        ? match.Groups["protocol"].Value
				                        : (await GetDefaultProtocol()).ProtocolPrefix;

			// Get the default connection data for the protocol
			IProtocol protocol = _protocols.First(pair => pair.Value.ProtocolPrefix.ToLower() == protocolPrefix.ToLower()).Value;
			IConnection connection = (IConnection) (await GetDefaults(protocol)).Clone();

			connection.Host = match.Groups["host"].Value;

			return connection;
		}

		/// <summary>
		/// Opens the UI to use to establish the <paramref name="connection"/>.
		/// </summary>
		/// <param name="connection">Connection that we want the UI for.</param>
		/// <param name="containerPanel">Container panel that will house the <paramref name="connection"/>'s UI.</param>
		/// <returns>The UI form to use to establish the <paramref name="connection"/>.</returns>
		public static BaseConnectionForm CreateConnectionForm(IConnection connection, Panel containerPanel)
		{
			return _protocols.First(pair => pair.Value.ConnectionType.Name == connection.GetType().Name).Value.CreateConnectionForm(connection, containerPanel);
		}

		/// <summary>
		/// Opens the UI to enter data for establishing <paramref name="connection"/> (i.e. host, username, password, etc.).
		/// </summary>
		/// <param name="connection">Connection that we want the settings UI for.</param>
		/// <returns>The UI to enter data for establishing <paramref name="connection"/> (i.e. host, username, password, etc.).</returns>
		public static Form CreateSettingsForm(IConnection connection)
		{
			return _protocols.First(pair => pair.Value.ConnectionType.Name == connection.GetType().Name).Value.GetSettingsForm(connection);
		}
	}
}