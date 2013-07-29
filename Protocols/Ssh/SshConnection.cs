using System;
using System.Drawing;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace EasyConnect.Protocols.Ssh
{
	/// <summary>
	/// Connection class for connecting to Secure Shell (SSH) servers.
	/// </summary>
	[Serializable]
	public class SshConnection : BaseConnection
	{
		/// <summary>
		/// Default constructor; initializes various connection parameters to default values.
		/// </summary>
		public SshConnection()
		{
			BackgroundColor = Color.Black;
			TextColor = Color.LightGray;
			Font = new Font("Courier New", 10);
		}

		/// <summary>
		/// Serialization constructor required for <see cref="ISerializable"/>; reads connection data from <paramref name="info"/>.
		/// </summary>
		/// <param name="info">Serialization store that we are going to read our data from.</param>
		/// <param name="context">Streaming context to use during the deserialization process.</param>
		public SshConnection(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			IdentityFile = info.GetString("IdentityFile");
			BackgroundColor = Color.FromArgb(info.GetInt32("BackgroundColor"));
			TextColor = Color.FromArgb(info.GetInt32("TextColor"));
			FontFamily = info.GetString("FontFamily");
			FontSize = info.GetSingle("FontSize");
		}

		/// <summary>
		/// RSA key file that should be presented when establishing the connection.
		/// </summary>
		public string IdentityFile
		{
			get;
			set;
		}

		/// <summary>
		/// Background color to use in the connection prompt window.  Separate property from <see cref="BackgroundColor"/> for <see cref="IXmlSerializable"/>
		/// purposes, as <see cref="Color"/> can't be automatically serialized.
		/// </summary>
		[XmlElement(ElementName = "BackgroundColor")]
		public int BackgroundColorArgb
		{
			get
			{
				return BackgroundColor.ToArgb();
			}

			set
			{
				BackgroundColor = Color.FromArgb(value);
			}
		}

		/// <summary>
		/// Text color to use in the connection prompt window.  Separate property from <see cref="BackgroundColor"/> for <see cref="IXmlSerializable"/>
		/// purposes, as <see cref="Color"/> can't be automatically serialized.
		/// </summary>
		[XmlElement(ElementName = "TextColor")]
		public int TextColorArgb
		{
			get
			{
				return TextColor.ToArgb();
			}

			set
			{
				TextColor = Color.FromArgb(value);
			}
		}

		/// <summary>
		/// Background color to use in the connection prompt window.
		/// </summary>
		[XmlIgnore]
		public Color BackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// Text color to use in the connection prompt window.
		/// </summary>
		[XmlIgnore]
		public Color TextColor
		{
			get;
			set;
		}

		/// <summary>
		/// Font to use in the connection prompt window.
		/// </summary>
		[XmlIgnore]
		public Font Font
		{
			get
			{
				return new Font(FontFamily, FontSize);
			}

			set
			{
				FontFamily = value.FontFamily.GetName(0);
				FontSize = value.SizeInPoints;
			}
		}

		/// <summary>
		/// Name of the font family (Courier New, Arial, etc.) to use in the connection prompt window.
		/// </summary>
		public string FontFamily
		{
			get;
			set;
		}

		/// <summary>
		/// Font size in points to use in the connection prompt window.
		/// </summary>
		public float FontSize
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

			info.AddValue("IdentityFile", IdentityFile);
			info.AddValue("TextColor", TextColor.ToArgb());
			info.AddValue("BackgroundColor", BackgroundColor.ToArgb());
			info.AddValue("FontFamily", FontFamily);
			info.AddValue("FontSize", FontSize);
		}
	}
}