using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace EasyConnect.Protocols
{
    /// <summary>
    /// Helper class for getting a list of all connection protocol, getting and setting connection protocol defaults, and getting connections for URIs
    /// input by the user (i.e. rdp://somehost or ssh://anotherhost).
    /// </summary>
    public class ConnectionFactory
    {
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
        /// Password to use when encrypting and decrypting sensitive data in <see cref="IConnection"/> instances.
        /// </summary>
        public static SecureString EncryptionPassword
        {
            get
            {
                return _encryptionPassword;
            }

            set
            {
                _encryptionPassword = value;

                if (_encryptionPassword != null)
                    _encryptionPassword.MakeReadOnly();
            }
        }

        /// <summary>
        /// Sets the default protocol to assume when the user enters a URI manually without specifying a prefix.
        /// </summary>
        /// <param name="protocol">Connection protocol that we want to use by default.</param>
        public static void SetDefaultProtocol(IProtocol protocol)
        {
            if (_defaults == null)
                InitializeDefaults();

            _defaultProtocolPrefix = protocol.ProtocolPrefix;
            SaveDefaults();
        }

        /// <summary>
        /// Gets the default protocol to assume when the user enters a URI manually without specifying a prefix.
        /// </summary>
        /// <returns>Connection protocol that we want to use by default.</returns>
        public static IProtocol GetDefaultProtocol()
        {
            if (_defaults == null)
                InitializeDefaults();

            return _protocols.First(
                pair => pair.Value.ProtocolPrefix == (String.IsNullOrEmpty(_defaultProtocolPrefix)
                                                          ? "rdp"
                                                          : _defaultProtocolPrefix)).Value;
        }

        /// <summary>
        /// Deserializes the defaults for each protocol type as well as the default protocol to assume when the user enters a URI manually without specifying
        /// a prefix.
        /// </summary>
        protected static void InitializeDefaults()
        {
            _defaults = new Dictionary<Type, IConnection>();

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Defaults.xml"))
            {
                using (
                    XmlReader reader =
                        new XmlTextReader(
                            new FileStream(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Defaults.xml", FileMode.Open)))
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
        }

        /// <summary>
        /// Serializes the defaults for each protocol type as well as the default protocol to assume when the user enters a URI manually without specifying
        /// a prefix.
        /// </summary>
        protected static void SaveDefaults()
        {
            using (
                XmlWriter writer =
                    new XmlTextWriter(
                        new FileStream(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Defaults.xml", FileMode.Create),
                        Encoding.Unicode))
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
        }

        /// <summary>
        /// Sets the defaults to use when creating a new connection for a particular protocol.
        /// </summary>
        /// <param name="defaults">Default connection data to use when creating a new connection for the protocol.</param>
        public static void SetDefaults(IConnection defaults)
        {
            if (_defaults == null)
                InitializeDefaults();

            // Get the protocol for this connection type
            IProtocol protocol = _protocols.First(pair => pair.Value.ConnectionType == defaults.GetType()).Value;

            _defaults[protocol.GetType()] = defaults;
            SaveDefaults();
        }

        /// <summary>
        /// Gets the defaults to use when creating a new connection for a particular protocol.
        /// </summary>
        /// <param name="protocolType">Protocol type that we are to retrieve defaults for.</param>
        /// <returns>Default connection data to use when creating a new connection for <paramref name="protocolType"/>.</returns>
        public static IConnection GetDefaults(Type protocolType)
        {
            if (_defaults == null)
                InitializeDefaults();

            return GetDefaults(_protocols[protocolType]);
        }

        /// <summary>
        /// Gets the defaults to use when creating a new connection for a particular protocol.
        /// </summary>
        /// <param name="protocol">Protocol that we are to retrieve defaults for.</param>
        /// <returns>Default connection data to use when creating a new connection for <paramref name="protocol"/>.</returns>
        public static IConnection GetDefaults(IProtocol protocol)
        {
            if (_defaults == null)
                InitializeDefaults();

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
        public static IConnection GetConnection(string uri)
        {
            // Get the protocol for the URI
            Regex uriCompontents = new Regex("^(?<prefix>(?<protocol>.+)://){0,1}(?<host>.+)$");
            Match match = uriCompontents.Match(uri);
            string protocolPrefix = match.Groups["protocol"].Success
                                        ? match.Groups["protocol"].Value
                                        : GetDefaultProtocol().ProtocolPrefix;

            // Get the default connection data for the protocol
            IProtocol protocol = _protocols.First(pair => pair.Value.ProtocolPrefix.ToLower() == protocolPrefix.ToLower()).Value;
            IConnection connection = (IConnection) GetDefaults(protocol).Clone();

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
        /// <param name="connection">Connection that we want the options UI for.</param>
        /// <returns>The UI to enter data for establishing <paramref name="connection"/> (i.e. host, username, password, etc.).</returns>
        public static Form CreateOptionsForm(IConnection connection)
        {
            return _protocols.First(pair => pair.Value.ConnectionType.Name == connection.GetType().Name).Value.GetOptionsForm(connection);
        }
    }
}
