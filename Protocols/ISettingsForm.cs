namespace EasyConnect.Protocols
{
	/// <summary>
	/// Interface implemented by all forms containing a UI for capturing option data for a connection.
	/// </summary>
	public interface ISettingsForm
	{
		/// <summary>
		/// Connection for which the settings are being captured.
		/// </summary>
		IConnection Connection
		{
			get;
			set;
		}

		/// <summary>
		/// Whether the form should be displayed in defaults mode (i.e. capturing default settings for a protocol instead of settings for a specific connection).
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
	/// <typeparam name="TConnection">Type of connection for which settings are being captured.</typeparam>
	public interface ISettingsForm<TConnection> : ISettingsForm
		where TConnection : IConnection
	{
		/// <summary>
		/// Connection for which settings data is being captured.
		/// </summary>
		new TConnection Connection
		{
			get;
			set;
		}
	}
}