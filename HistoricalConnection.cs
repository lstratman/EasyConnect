using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using EasyConnect.Protocols;

namespace EasyConnect
{
    /// <summary>
    /// Wraps a connection that the user made and the time that they made it.
    /// </summary>
    public class HistoricalConnection : ISerializable, IXmlSerializable
	{
		/// <summary>
		/// Default constructor.
		/// </summary>
		public HistoricalConnection()
		{
		}

		/// <summary>
		/// Constructor for <see cref="ISerializable"/>.
		/// </summary>
		/// <param name="info">Container for serialized data.</param>
		/// <param name="context">Serialization context.</param>
		// ReSharper disable UnusedParameter.Local
		public HistoricalConnection(SerializationInfo info, StreamingContext context)
			// ReSharper restore UnusedParameter.Local
		{
			LastConnection = info.GetDateTime("LastConnection");
			Connection = (IConnection) info.GetValue("Connection", typeof (IConnection));
		}

		/// <summary>
		/// Constructor that initializes <see cref="Connection"/>.
		/// </summary>
		/// <param name="connection">Connection that the user made.</param>
		public HistoricalConnection(IConnection connection)
		{
			Connection = connection;
		}

		/// <summary>
		/// Connection that the user made.
		/// </summary>
		public IConnection Connection
		{
			get;
			set;
		}

		/// <summary>
		/// Date and time that the user made the connection.
		/// </summary>
		public DateTime LastConnection
		{
			get;
			set;
		}

		/// <summary>
		/// Serialization method for <see cref="ISerializable"/>.
		/// </summary>
		/// <param name="info">Container for serialized data.</param>
		/// <param name="context">Serialization context.</param>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("LastConnection", LastConnection);
			info.AddValue("Connection", Connection);
		}

		/// <summary>
		/// Schema method for <see cref="IXmlSerializable"/>.
		/// </summary>
		/// <returns>Null, always.</returns>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Deserializes the historical connection data from an XML source.
		/// </summary>
		/// <param name="reader">XML source that we are deserializing from.</param>
		public void ReadXml(XmlReader reader)
		{
			// If we are dealing with a legacy historical connection, deserialize it into LegacyHistoricalConnection
			if (reader.GetAttribute("isLegacy") != "false")
			{
				XmlSerializer serializer = new XmlSerializer(typeof (LegacyHistoricalConnection));
				LegacyHistoricalConnection legacyHistoricalConnection = (LegacyHistoricalConnection) serializer.Deserialize(reader);

				LastConnection = legacyHistoricalConnection.LastConnection;
				Connection = legacyHistoricalConnection;
			}

				// Otherwise, get the type of connection from the node name under Connection and deserialize it using that
			else
			{
				reader.Read();

				while (reader.MoveToContent() == XmlNodeType.Element)
				{
					switch (reader.LocalName)
					{
						case "LastConnection":
							LastConnection = DateTime.Parse(reader.ReadElementContentAsString());
							break;

						case "Connection":
							reader.Read();

							XmlSerializer serializer =
								new XmlSerializer(
									reader.LocalName == "HistoricalConnection"
										? typeof (LegacyHistoricalConnection)
										: ConnectionFactory.GetProtocols().First(p => p.ConnectionType.Name == reader.LocalName).ConnectionType);
							Connection = (IConnection) serializer.Deserialize(reader);

							reader.Read();

							break;
					}
				}

				reader.Read();
			}
		}

		/// <summary>
		/// Serializes the connection data to XML.
		/// </summary>
		/// <param name="writer">XML destination that we're serializing to.</param>
		public void WriteXml(XmlWriter writer)
		{
			// Write an explicit isLegacy="false" attribute so that we don't try to deserialize this using LegacyHistoricalConnection.
			writer.WriteAttributeString("isLegacy", "false");
			writer.WriteElementString("LastConnection", LastConnection.ToString());

			writer.WriteStartElement("Connection");

			XmlSerializer serializer = new XmlSerializer(Connection.GetType());

			serializer.Serialize(writer, Connection);
			writer.WriteEndElement();
		}
	}
}