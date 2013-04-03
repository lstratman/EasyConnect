namespace EasyConnect.Protocols
{
	/// <summary>
	/// Version of <see cref="BaseConnectionForm"/> that contains a strongly typed <see cref="IConnection"/> property.
	/// </summary>
	/// <typeparam name="T">Type of connection that will be established by this window.</typeparam>
	public abstract class BaseConnectionForm<T> : BaseConnectionForm, IConnectionForm<T>
		where T : IConnection
	{
		/// <summary>
		/// Connection that will be established.
		/// </summary>
		public T Connection
		{
			get;
			set;
		}
	}
}
