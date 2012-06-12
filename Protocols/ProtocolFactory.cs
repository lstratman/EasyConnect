using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace EasyConnect.Protocols
{
    public class ProtocolFactory
    {
        protected static Dictionary<Type, IProtocol> _protocols = new Dictionary<Type, IProtocol>();

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

                catch (FileLoadException loadEx)
                {
                }

                catch (BadImageFormatException imgEx)
                {
                }
            }


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
