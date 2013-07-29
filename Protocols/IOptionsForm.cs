namespace EasyConnect.Protocols
{
	/// <summary>
	/// Interface implemented by all forms containing a UI for capturing option data for a connection.
	/// </summary>
	public interface IOptionsForm
	{
		/// <summary>
		/// Connection for which the options are being captured.
		/// </summary>
		IConnection Connection
		{
			get;
			set;
		}

		/// <summary>
		/// Whether the form should be displayed in defaults mode (i.e. capturing default options for a protocol instead of options for a specific connection).
		/// </summary>
		bool DefaultsMode
		{
			get;
			set;
		}
	}

	/// <summary>
	/// Interface implemented by all forms containing a UI for capturing option data for a connection.  Contains a strongly typed connection property.
	/// </summary>
	/// <typeparam name="TConnection">Type of connection for which options are being captured.</typeparam>
	public interface IOptionsForm<TConnection> : IOptionsForm
		where TConnection : IConnection
	{
		/// <summary>
		/// Connection for which options data is being captured.
		/// </summary>
		new TConnection Connection
		{
			get;
			set;
		}
	}
}