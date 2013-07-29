namespace EasyConnect.Protocols.Rdp
{
	/// <summary>
	/// Specifies where shortcut keys (like Alt+Tab) are sent:  to the local system or the remote connection.
	/// </summary>
	public enum KeyboardMode
	{
		/// <summary>
		/// Shortcuts should be sent locally.
		/// </summary>
		Locally = 0,

		/// <summary>
		/// Shortcuts should be sent to the remote connection.
		/// </summary>
		Remotely = 1
	}
}