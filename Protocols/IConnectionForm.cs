using System;

namespace EasyConnect.Protocols
{
	/// <summary>
	/// Interface implemented by all forms containing a UI for a remote connection.  Contains a strongly typed connection property.
	/// </summary>
	/// <typeparam name="TConnection">Type of connection that this form establishes.</typeparam>
	public interface IConnectionForm<TConnection> : IConnectionForm
		where TConnection : IConnection
	{
		/// <summary>
		/// Connection that will be established.
		/// </summary>
		TConnection Connection
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Interface implemented by all forms containing a UI for a remote connection.
	/// </summary>
	public interface IConnectionForm
	{
		/// <summary>
		/// Establish the connection to the remote system.
		/// </summary>
		void Connect();

		/// <summary>
		/// Event that is fired when the connection for this window is established.
		/// </summary>
		event EventHandler Connected;

		/// <summary>
		/// Event that is fired when the connection window gains focus.
		/// </summary>
		event EventHandler ConnectionFormFocused;
	}
}