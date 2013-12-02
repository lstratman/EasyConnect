using System;
using System.Security;
using System.Windows.Forms;
using Poderosa.TerminalControl;

namespace EasyConnect.Protocols.Ssh
{
	/// <summary>
	/// UI that displays a Secure Shell (SSH) connection via the <see cref="TerminalControl"/> class.
	/// </summary>
	public partial class SshConnectionForm : BaseConnectionForm<SshConnection>
	{
		/// <summary>
		/// Default constructor.  Turns off the warning that the terminal displays when it encounters an unknown terminal control code.
		/// </summary>
		public SshConnectionForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Control instance that hosts the actual SSH prompt UI.
		/// </summary>
		protected override Control ConnectionWindow
		{
			get
			{
				return _terminal;
			}
		}

		/// <summary>
		/// Establishes the connection to the remote server; initializes this <see cref="_terminal"/>'s properties from 
		/// <see cref="BaseConnectionForm{T}.Connection"/> and then calls <see cref="TerminalControl.AsyncConnect"/> on <see cref="_terminal"/>.
		/// </summary>
		public override void Connect()
		{
			_terminal.Font = Connection.Font;
			_terminal.ForeColor = Connection.TextColor;
			_terminal.BackColor = Connection.BackgroundColor;

			_terminal.Username = Connection.InheritedUsername;
			_terminal.HostName = Connection.Host;

			SecureString password = Connection.InheritedPassword;

			// Set the auth file and the auth method to PublicKey if an identity file was specified
			if (!String.IsNullOrEmpty(Connection.IdentityFile))
				_terminal.IdentityFile = Connection.IdentityFile;

			// Otherwise, set the auth type to Password
			else if (password != null && password.Length > 0)
				_terminal.Password = password;

			_terminal.Connected += OnConnected;
			_terminal.LoggedOff += OnLoggedOff;
			_terminal.Disconnected += OnConnectionLost;

			_terminal.AsyncConnect();
		}
	}
}