using System;
using System.Runtime.Serialization;
using System.Security;

namespace EasyConnect.Protocols
{
	/// <summary>
	/// Interface implemented by each class serving as data for a remote connection.
	/// </summary>
	public interface IConnection : ICloneable, ISerializable
	{
		/// <summary>
		/// Host name of the server that we are to connect to.
		/// </summary>
		string Host
		{
			get;
			set;
		}

		/// <summary>
		/// Display text to use when identifying this connection.
		/// </summary>
		string DisplayName
		{
			get;
		}

		/// <summary>
		/// Name used to identify this connection for bookmarking purposes.
		/// </summary>
		string Name
		{
			get;
			set;
		}

		/// <summary>
		/// Pointer to the folder that contains the bookmark of this connection.
		/// </summary>
		BookmarksFolder ParentFolder
		{
			get;
			set;
		}

		/// <summary>
		/// Unique identifier for this connection.
		/// </summary>
		Guid Guid
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether or not this connection is part of a bookmark.
		/// </summary>
		bool IsBookmark
		{
			get;
			set;
		}

		/// <summary>
		/// Password, if any, used when establishing this connection.
		/// </summary>
		SecureString Password
		{
			get;
			set;
		}

		/// <summary>
		/// Creates an anonymized copy of this connection, with sensitive information, like <see cref="Password"/>, removed.
		/// </summary>
		/// <returns>An anonymized copy of this connection, with sensitive information, like <see cref="Password"/>, removed.</returns>
		object CloneAnon();
	}
}