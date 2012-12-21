using System;
using System.Runtime.Serialization;
using ViewerX;

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
			AuthenticationType = ViewerLoginType.VLT_VNC;
			EncryptionType = EncryptionPluginType.EPT_NONE;
			EncodingType = VNCEncoding.RFB_ZRLE;
			ColorDepth = ColorDepth.COLOR_FULL;
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

					case "Scale":
						Scale = (bool) entry.Value;
						break;

					case "AuthenticationType":
						AuthenticationType = (ViewerLoginType) Enum.Parse(typeof (ViewerLoginType), (string) entry.Value);
						break;

					case "EncryptionType":
						EncryptionType = (EncryptionPluginType) Enum.Parse(typeof (EncryptionPluginType), (string) entry.Value);
						break;

					case "KeyFile":
						KeyFile = (string) entry.Value;
						break;

					case "EncodingType":
						EncodingType = (VNCEncoding) Enum.Parse(typeof (VNCEncoding), (string) entry.Value);
						break;

					case "ColorDepth":
						ColorDepth = (ColorDepth) Enum.Parse(typeof (ColorDepth), (string) entry.Value);
						break;
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
		/// Flag indicating whether or not scaling should be performed to fit the connection's display into the viewing window.
		/// </summary>
		public bool Scale
		{
			get;
			set;
		}

		/// <summary>
		/// Authentication type to use when establishing the connection.
		/// </summary>
		public ViewerLoginType AuthenticationType
		{
			get;
			set;
		}

		/// <summary>
		/// Encryption type to use when establishing the connection.
		/// </summary>
		public EncryptionPluginType EncryptionType
		{
			get;
			set;
		}

		/// <summary>
		/// If <see cref="EncryptionType"/> is specified, this is the file path to the key that should be used during the encryption/decryption process.
		/// </summary>
		public string KeyFile
		{
			get;
			set;
		}

		/// <summary>
		/// How the data should be encoded when sending display data from the server to the client.
		/// </summary>
		public VNCEncoding EncodingType
		{
			get;
			set;
		}

		/// <summary>
		/// Color depth to use when sending display data from the server to the client.
		/// </summary>
		public ColorDepth ColorDepth
		{
			get;
			set;
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
			info.AddValue("Scale", Scale);
			info.AddValue("AuthenticationType", AuthenticationType.ToString("G"));
			info.AddValue("EncryptionType", EncryptionType.ToString("G"));
			info.AddValue("KeyFile", KeyFile);
			info.AddValue("EncodingType", EncodingType.ToString("G"));
			info.AddValue("ColorDepth", ColorDepth.ToString("G"));
		}
	}
}