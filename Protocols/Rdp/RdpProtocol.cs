using System.Drawing;

namespace EasyConnect.Protocols.Rdp
{
	/// <summary>
	/// Protocol class for the Microsoft Remote Desktop (RDP) protocol.
	/// </summary>
	public class RdpProtocol : BaseProtocol<RdpConnection, RdpOptionsForm, RdpConnectionForm>
	{
		/// <summary>
		/// Prefix used to identify this protocol in URIs.
		/// </summary>
		public override string ProtocolPrefix
		{
			get
			{
				return "rdp";
			}
		}

		/// <summary>
		/// Descriptive text used to identify this protocol.
		/// </summary>
		public override string ProtocolTitle
		{
			get
			{
				return "Remote Desktop";
			}
		}

		/// <summary>
		/// Icon used to identify connections for this protocol in the bookmarks manager.
		/// </summary>
		public override Icon ProtocolIcon
		{
			get
			{
				return Resources.RdpIcon;
			}
		}
	}
}