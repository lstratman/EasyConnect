using System;
using System.Xml.Serialization;
using EasyConnect.Protocols.Rdp;

namespace EasyConnect
{
    /// <summary>
    /// Legacy class designed to accommodate when derived the history connections from <see cref="RdpConnection"/>.
    /// </summary>
    [Serializable]
	[XmlRoot("HistoricalConnection")]
	public class LegacyHistoricalConnection : RdpConnection
	{
		/// <summary>
		/// Time the user made the connection.
		/// </summary>
		public DateTime LastConnection
		{
			get;
			set;
		}
	}
}