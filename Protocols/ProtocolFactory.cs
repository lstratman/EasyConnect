using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace EasyConnect.Protocols
{
    public class ProtocolFactory
    {
        protected static Dictionary<Type, IProtocol> _protocols = new Dictionary<Type, IProtocol>();
        protected static Dictionary<Type, IConnection> _defaults = new Dictionary<Type, IConnection>();

        private ProtocolFactory()
        {
        }

        static ProtocolFactory()
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
            }

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Defaults.xml"))
            {
                using (XmlReader reader = new XmlTextReader(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Defaults.xml", FileMode.Open)))
                {
                    reader.MoveToContent();
                    reader.Read();

                    while (reader.Read())
                    {
                        IConnection defaults = Deserialize(reader);
                        IProtocol protocol = _protocols.First(pair => pair.Key == defaults.GetType()).Value;

                        _defaults[protocol.GetType()] = defaults;
                    }
                }
            }
        }

        public static void SetDefaults(IConnection defaults)
        {
            IProtocol protocol = _protocols.First(pair => pair.Value.ConnectionType == defaults.GetType()).Value;

            _defaults[protocol.GetType()] = defaults;

            using (XmlWriter writer = new XmlTextWriter(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Defaults.xml", FileMode.Create), Encoding.Unicode))
            {
                writer.WriteStartElement("Defaults");

                foreach (IConnection connection in _defaults.Values)
                {
                    XmlSerializer serializer = new XmlSerializer(connection.GetType());
                    serializer.Serialize(writer, connection);
                }

                writer.WriteEndElement();
            }
        }

        public static IConnection GetDefaults(IProtocol protocol)
        {
            return _defaults.ContainsKey(protocol.GetType())
                       ? _defaults[protocol.GetType()]
                       : (IConnection) Activator.CreateInstance(_protocols[protocol.GetType()].ConnectionType);
        }

        public static List<IProtocol> GetProtocols()
        {
            return _protocols.Values.ToList();
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
            string protocolPrefix = match.Groups["uri"].Success
                                        ? match.Groups["uri"].Value
                                        : "rdp";
            IProtocol protocol =
                _protocols.First(
                    pair => pair.Value.ProtocolPrefix.ToLower() == protocolPrefix.ToLower()).Value;
            IConnection connection = (IConnection)GetDefaults(protocol).Clone();

            connection.Host = match.Groups["host"].Value;

            return connection;
        }

        public static BaseConnectionPanel CreateConnectionForm(IConnection connection, Panel containerPanel, Form parentWindow)
        {
            return _protocols.First(pair => pair.Value.ConnectionType.Name == connection.GetType().Name).Value.CreateConnectionPanel(connection, containerPanel, parentWindow);
        }

        public static Form CreateOptionsForm(IConnection connection)
        {
            return _protocols.First(pair => pair.Value.ConnectionType.Name == connection.GetType().Name).Value.GetOptionsForm(connection);
        }
    }
}
