using System.Drawing;

namespace EasyConnect.Protocols.Telnet
{
	/// <summary>
	/// Protocol class for the Telnet protocol.
	/// </summary>
	public class TelnetProtocol : BaseProtocol<TelnetConnection, TelnetSettingsForm, TelnetConnectionForm>
	{
		/// <summary>
		/// Prefix used to identify this protocol in URIs.
		/// </summary>
		public override string ProtocolPrefix
		{
			get
			{
				return "telnet";
			}
		}

		/// <summary>
		/// Descriptive text used to identify this protocol.
		/// </summary>
		public override string ProtocolTitle
		{
			get
			{
				return "Telnet";
			}
		}

		/// <summary>
		/// Icon used to identify connections for this protocol in the bookmarks manager.
		/// </summary>
		public override Icon ProtocolIcon
		{
			get
			{
				return Resources.TelnetProtocol;
			}
		}
	}
}