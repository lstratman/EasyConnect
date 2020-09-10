using System.Drawing;

namespace EasyConnect.Protocols.Vnc
{
	/// <summary>
	/// Protocol class for the VNC protocol.
	/// </summary>
	public class VncProtocol : BaseProtocol<VncConnection, VncSettingsForm, VncConnectionForm>
	{
		/// <summary>
		/// Prefix used to identify this protocol in URIs.
		/// </summary>
		public override string ProtocolPrefix
		{
			get
			{
				return "vnc";
			}
		}

		/// <summary>
		/// Descriptive text used to identify this protocol.
		/// </summary>
		public override string ProtocolTitle
		{
			get
			{
				return "VNC";
			}
		}

		/// <summary>
		/// Icon used to identify connections for this protocol in the bookmarks manager.
		/// </summary>
		public override Icon ProtocolIcon
		{
			get
			{
				return Resources.VncIcon;
			}
		}
	}
}