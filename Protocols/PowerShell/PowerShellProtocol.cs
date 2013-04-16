using System.Drawing;

namespace EasyConnect.Protocols.PowerShell
{
	/// <summary>
	/// Protocol class for the Secure Shell (SSH) protocol.
	/// </summary>
	public class SshProtocol : BaseProtocol<PowerShellConnection, PowerShellOptionsForm, PowerShellConnectionForm>
	{
		/// <summary>
		/// Prefix used to identify this protocol in URIs.
		/// </summary>
		public override string ProtocolPrefix
		{
			get
			{
				return "ps";
			}
		}

		/// <summary>
		/// Descriptive text used to identify this protocol.
		/// </summary>
		public override string ProtocolTitle
		{
			get
			{
				return "PowerShell";
			}
		}

		/// <summary>
		/// Icon used to identify connections for this protocol in the bookmarks manager.
		/// </summary>
		public override Icon ProtocolIcon
		{
			get
			{
				return Resources.PowerShell;
			}
		}
	}
}