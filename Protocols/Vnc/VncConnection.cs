using System;
using System.Runtime.Serialization;
using System.Security;
using System.Xml.Serialization;
using EasyConnect.Common;

namespace EasyConnect.Protocols.Vnc
{
	/// <summary>
	/// Connection class for connecting to VNC servers.
	/// </summary>
	[Serializable]
	public class VncConnection : BaseConnection
	{
		/// <summary>
		/// Default constructor; initializes properties to default values.
		/// </summary>
		public VncConnection()
		{
			Port = 5900;
			PictureQuality = 7;
			ShareClipboard = true;
			PreferredEncoding = "ZRLE";
			ShowLocalCursor = true;
			AmtUseTls = true;
		}

		/// <summary>
		/// Serialization constructor required for <see cref="ISerializable"/>; reads connection data from <paramref name="info"/>.
		/// </summary>
		/// <param name="info">Serialization store that we are going to read our data from.</param>
		/// <param name="context">Streaming context to use during the deserialization process.</param>
		protected VncConnection(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			foreach (SerializationEntry entry in info)
			{
				switch (entry.Name)
				{
					case "Port":
						Port = (int) entry.Value;
						break;

					case "Display":
						Display = (int) entry.Value;
						break;

					case "ViewOnly":
						ViewOnly = (bool) entry.Value;
						break;

				    case "ShareClipboard":
				        ShareClipboard = (bool) entry.Value;
				        break;

					case "ShowLocalCursor":
						ShowLocalCursor = (bool)entry.Value;
						break;

					case "PictureQuality":
						PictureQuality = (int) entry.Value;
						break;

					case "PreferredEncoding":
						PreferredEncoding = (string) entry.Value;
						break;
					case "UseAmtKvm": UseAmtKvm = (bool)entry.Value; break;
					case "AmtUseTls": AmtUseTls = (bool)entry.Value; break;
					case "AmtUsername": AmtUsername = (string)entry.Value; break;
					case "AmtDomain": AmtDomain = (string)entry.Value; break;
					case "AmtPassword": EncryptedAmtPassword = (string)entry.Value; break;
				}
			}
		}

		/// <summary>
		/// Base port that the VNC server is listening on.
		/// </summary>
		public int Port
		{
			get;
			set;
		}

		/// <summary>
		/// Number of the display on the host server that should be connected to.
		/// </summary>
		public int Display
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether or not the connection should be established in view-only mode.
		/// </summary>
		public bool ViewOnly
		{
			get;
			set;
		}

        /// <summary>
        /// Flag indicating whether or not to share the clipboard with the remote server.
        /// </summary>
	    public bool ShareClipboard
	    {
	        get;
	        set;
	    }

		public bool ShowLocalCursor
		{
			get;
			set;
		}

		public int PictureQuality
        {
			get;
			set;
        }

		public string PreferredEncoding
		{
			get;
			set;
		}

		public bool UseAmtKvm { get; set; }
		public bool AmtUseTls { get; set; }
		public string AmtUsername { get; set; }
		public string AmtDomain { get; set; }

		[XmlIgnore]
		public SecureString AmtPassword { get; set; }

		[XmlElement("AmtPassword")]
		public string EncryptedAmtPassword
		{
			get
			{
				return AmtPassword == null || AmtPassword.Length == 0
					? null
					: Convert.ToBase64String(ConnectionFactory.Encrypt(AmtPassword));
			}
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					AmtPassword = null;
					return;
				}

				byte[] decrypted = ConnectionFactory.Decrypt(Convert.FromBase64String(value));
				try
				{
					SecureString password = new SecureString();
					for (int i = 0; i < decrypted.Length && decrypted[i] != 0; i++)
						password.AppendChar((char)decrypted[i]);
					AmtPassword = password;
				}
				finally
				{
					Array.Clear(decrypted, 0, decrypted.Length);
				}
			}
		}

		public override object CloneAnon()
		{
			VncConnection clone = (VncConnection)base.CloneAnon();
			clone.AmtUsername = null;
			clone.AmtDomain = null;
			clone.AmtPassword = null;
			return clone;
		}

		/// <summary>
		/// Method required for <see cref="ISerializable"/>; serializes the connection data to <paramref name="info"/>.
		/// </summary>
		/// <param name="info">Serialization store that the connection's data will be written to.</param>
		/// <param name="context">Streaming context to use during the serialization process.</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("Port", Port);
			info.AddValue("Display", Display);
			info.AddValue("ViewOnly", ViewOnly);
            info.AddValue("ShareClipboard", ShareClipboard);
			info.AddValue("ShowLocalCursor", ShowLocalCursor);
			info.AddValue("PictureQuality", PictureQuality);
			info.AddValue("PreferredEncoding", PreferredEncoding);
			info.AddValue("UseAmtKvm", UseAmtKvm);
			info.AddValue("AmtUseTls", AmtUseTls);
			info.AddValue("AmtUsername", AmtUsername);
			info.AddValue("AmtDomain", AmtDomain);
			info.AddValue("AmtPassword", EncryptedAmtPassword);
		}
	}
}