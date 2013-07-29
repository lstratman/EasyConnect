using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyConnect.Protocols
{
	/// <summary>
	/// Interface implemented by all classes that are specifying a connection protocol.
	/// </summary>
	public interface IProtocol
	{
		/// <summary>
		/// Prefix used to identify this protocol in URIs; i.e. rdp or ssh in [protocol]://[hostname].
		/// </summary>
		string ProtocolPrefix
		{
			get;
		}

		/// <summary>
		/// Display text used to name this protocol.
		/// </summary>
		string ProtocolTitle
		{
			get;
		}

		/// <summary>
		/// Type of connections established for this protocol.
		/// </summary>
		Type ConnectionType
		{
			get;
		}

		/// <summary>
		/// Icon used to identify this protocol in the UI.
		/// </summary>
		Icon ProtocolIcon
		{
			get;
		}

		/// <summary>
		/// Gets an options form used to capture configuration data for a connection for this protocol.
		/// </summary>
		/// <returns>Form used to capture configuration data for a connection for this protocol.</returns>
		Form GetOptionsForm();

		/// <summary>
		/// Gets an options form used to capture configuration data for <paramref name="connection"/>.
		/// </summary>
		/// <returns>Form used to capture configuration data for <paramref name="connection"/>.</returns>
		Form GetOptionsForm(IConnection connection);

		/// <summary>
		/// Gets an options form used to capture defaults to be used in connections for this protocol.  Typically returns a UI identical to 
		/// <see cref="GetOptionsForm()"/> with the exception of the hostname to use for the connection.
		/// </summary>
		/// <returns>Options form used to capture defaults to be used in connections for this protocol</returns>
		Form GetOptionsFormInDefaultsMode();

		/// <summary>
		/// Creates the form that will house the UI for establishing and displaying <paramref name="connection"/>.
		/// </summary>
		/// <param name="connection">Connection that we need to create the UI for.</param>
		/// <param name="containerPanel">Panel that this UI will be contained within.</param>
		/// <returns>Form that will house the UI for establishing and displaying <paramref name="connection"/>.</returns>
		BaseConnectionForm CreateConnectionForm(IConnection connection, Panel containerPanel);
	}
}