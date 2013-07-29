using System.Drawing;

namespace EasyConnect.Protocols.Ssh
{
	/// <summary>
	/// Protocol class for the Secure Shell (SSH) protocol.
	/// </summary>
	public class SshProtocol : BaseProtocol<SshConnection, SshOptionsForm, SshConnectionForm>
	{
		/// <summary>
		/// Prefix used to identify this protocol in URIs.
		/// </summary>
		public override string ProtocolPrefix
		{
			get
			{
				return "ssh";
			}
		}

		/// <summary>
		/// Descriptive text used to identify this protocol.
		/// </summary>
		public override string ProtocolTitle
		{
			get
			{
				return "Secure Shell";
			}
		}

		/// <summary>
		/// Icon used to identify connections for this protocol in the bookmarks manager.
		/// </summary>
		public override Icon ProtocolIcon
		{
			get
			{
				return Resources.SshProtocol;
			}
		}
	}
}