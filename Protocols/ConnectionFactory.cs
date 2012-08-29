using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace EasyConnect.Protocols
{
    public class ConnectionFactory
    {
        protected static Dictionary<Type, IProtocol> _protocols = new Dictionary<Type, IProtocol>();
        protected static Dictionary<Type, IConnection> _defaults = null;
        protected static SecureString _encryptionPassword = null;
        protected static string _defaultProtocolPrefix = null;

        private ConnectionFactory()
        {
        }

        public static SecureString EncryptionPassword
        {
            get
            {
                return _encryptionPassword;
            }

            set
            {
                _encryptionPassword = value;
            }
        }

        static ConnectionFactory()
        {
            foreach (string assemblyFile in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    Assembly loadedAssembly = Assembly.LoadFile(assemblyFile);

                    foreach (Type type in from type in loadedAssembly.GetTypes()
                                          where type.IsClass && !type.IsNotPublic && !type.IsAbstract
                                          let interfaces = type.GetInterfaces()
                                          where interfaces.Contains(typeof(IProtocol))
                                          select type)
                        _protocols[type] = (IProtocol)Activator.CreateInstance(type);
                }

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

        public static void SetDefaultProtocol(IProtocol protocol)
        {
            if (_defaults == null)
                InitializeDefaults();

            _defaultProtocolPrefix = protocol.ProtocolPrefix;
            SaveDefaults();
        }

        public static IProtocol GetDefaultProtocol()
        {
            if (_defaults == null)
                InitializeDefaults();

            return _protocols.First(pair => pair.Value.ProtocolPrefix == (String.IsNullOrEmpty(_defaultProtocolPrefix) ? "rdp" : _defaultProtocolPrefix)).Value;
        }

        protected static void InitializeDefaults()
        {
            _defaults = new Dictionary<Type, IConnection>();

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Defaults.xml"))
            {
                using (XmlReader reader = new XmlTextReader(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Defaults.xml", FileMode.Open)))
                {
                    reader.MoveToContent();

                    _defaultProtocolPrefix = reader.GetAttribute("defaultProtocolPrefix");

                    reader.Read();

                    while (reader.MoveToContent() == XmlNodeType.Element)
                    {
                        IConnection defaults = Deserialize(reader);
                        IProtocol protocol = _protocols.First(pair => pair.Value.ConnectionType == defaults.GetType()).Value;

                        _defaults[protocol.GetType()] = defaults;
                    }
                }
            }
        }

        protected static void SaveDefaults()
        {
            using (XmlWriter writer = new XmlTextWriter(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Defaults.xml", FileMode.Create), Encoding.Unicode))
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

        public static void SetDefaults(IConnection defaults)
        {
            if (_defaults == null)
                InitializeDefaults();

            IProtocol protocol = _protocols.First(pair => pair.Value.ConnectionType == defaults.GetType()).Value;

            _defaults[protocol.GetType()] = defaults;
            SaveDefaults();
        }

        public static IConnection GetDefaults(Type protocolType)
        {
            if (_defaults == null)
                InitializeDefaults();

            return GetDefaults(_protocols[protocolType]);
        }

        public static IConnection GetDefaults(IProtocol protocol)
        {
            if (_defaults == null)
                InitializeDefaults();

            return _defaults.ContainsKey(protocol.GetType())
                       ? _defaults[protocol.GetType()]
                       : (IConnection) Activator.CreateInstance(_protocols[protocol.GetType()].ConnectionType);
        }

        public static List<IProtocol> GetProtocols()
        {
            return _protocols.Values.ToList();
        }

        public static IProtocol GetProtocol(IConnection connection)
        {
            return _protocols.FirstOrDefault(p => p.Value.ConnectionType == connection.GetType()).Value;
        }

        public static IConnection Deserialize(XmlReader reader)
        {
            string typeName = reader.LocalName;
            IProtocol protocol = _protocols.First(pair => pair.Value.ConnectionType.Name == typeName).Value;
            XmlSerializer bookmarkSerializer = new XmlSerializer(protocol.ConnectionType);

            return (IConnection) bookmarkSerializer.Deserialize(reader);
        }

        public static IConnection GetConnection(string uri)
        {
            Regex uriCompontents = new Regex("^(?<prefix>(?<protocol>.+)://){0,1}(?<host>.+)$");
            Match match = uriCompontents.Match(uri);
            string protocolPrefix = match.Groups["protocol"].Success
                                        ? match.Groups["protocol"].Value
                                        : GetDefaultProtocol().ProtocolPrefix;
            IProtocol protocol =
                _protocols.First(
                    pair => pair.Value.ProtocolPrefix.ToLower() == protocolPrefix.ToLower()).Value;
            IConnection connection = (IConnection)GetDefaults(protocol).Clone();

            connection.Host = match.Groups["host"].Value;

            return connection;
        }

        public static BaseConnectionForm CreateConnectionForm(IConnection connection, Panel containerPanel)
        {
            return _protocols.First(pair => pair.Value.ConnectionType.Name == connection.GetType().Name).Value.CreateConnectionForm(connection, containerPanel);
        }

        public static Form CreateOptionsForm(IConnection connection)
        {
            return _protocols.First(pair => pair.Value.ConnectionType.Name == connection.GetType().Name).Value.GetOptionsForm(connection);
        }
    }
}
