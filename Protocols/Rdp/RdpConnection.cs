using System;
using System.Runtime.Serialization;
using System.Security;
using System.Xml.Serialization;
using EasyConnect.Common;

namespace EasyConnect.Protocols.Rdp
{
	/// <summary>
	/// Connection class for connecting to Microsoft Remote Desktop (RDP) servers.
	/// </summary>
	[Serializable]
	public class RdpConnection : BaseConnection
	{
		/// <summary>
		/// Default constructor; initializes various connection parameters to default values.
		/// </summary>
		public RdpConnection()
		{
			AudioMode = AudioMode.Locally;
			KeyboardMode = KeyboardMode.Remotely;
			VisualStyles = true;
			PersistentBitmapCaching = true;
			ConnectClipboard = true;
			ConnectPrinters = true;
			RecordingMode = RecordingMode.RecordFromThisComputer;
		}

		/// <summary>
		/// Serialization constructor required for <see cref="ISerializable"/>; reads connection data from <paramref name="info"/>.
		/// </summary>
		/// <param name="info">Serialization store that we are going to read our data from.</param>
		/// <param name="context">Streaming context to use during the deserialization process.</param>
		protected RdpConnection(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Animations = info.GetBoolean("Animations");
			AudioMode = info.GetValue<AudioMode>("AudioMode");
			ColorDepth = info.GetInt32("ColorDepth");
			ConnectClipboard = info.GetBoolean("ConnectClipboard");
			ConnectDrives = info.GetBoolean("ConnectDrives");
			ConnectPrinters = info.GetBoolean("ConnectPrinters");
			DesktopBackground = info.GetBoolean("DesktopBackground");
			DesktopComposition = info.GetBoolean("DesktopComposition");
			DesktopHeight = info.GetInt32("DesktopHeight");
			DesktopWidth = info.GetInt32("DesktopWidth");
			FontSmoothing = info.GetBoolean("FontSmoothing");
			KeyboardMode = info.GetValue<KeyboardMode>("KeyboardMode");
			PersistentBitmapCaching = info.GetBoolean("PersistentBitmapCaching");
			RecordingMode = info.GetValue<RecordingMode>("RecordingMode");
			VisualStyles = info.GetBoolean("VisualStyles");
			WindowContentsWhileDragging = info.GetBoolean("WindowContentsWhileDragging");
			ConnectToAdminChannel = info.GetBoolean("ConnectToAdminChannel");

            UseTSProxy = info.GetBoolean("UseTSProxy");
            ProxyName = info.GetString("ProxyName");
            ProxyUserName = info.GetString("ProxyUserName");

            string encryptedProxyPassword = info.GetString("ProxyPassword");
            if (!String.IsNullOrEmpty(encryptedProxyPassword))
            {
                EncryptedPassword = encryptedProxyPassword;
            }
		}

		/// <summary>
		/// Width of the resulting desktop.  0 means that it should fill the available window space.
		/// </summary>
		public int DesktopWidth
		{
			get;
			set;
		}

		/// <summary>
		/// Height of the resulting desktop.  0 means that it should fill the available window space.
		/// </summary>
		public int DesktopHeight
		{
			get;
			set;
		}

		/// <summary>
		/// Color depth (16, 24, or 32) of the resulting desktop.
		/// </summary>
		public int ColorDepth
		{
			get;
			set;
		}

		/// <summary>
		/// Where sounds originating from the remote server should be played (locally or remotely).
		/// </summary>
		public AudioMode AudioMode
		{
			get;
			set;
		}

		/// <summary>
		/// When recording in the remote system, this indicates where (locally or remotely) the recording source should be.
		/// </summary>
		public RecordingMode RecordingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Which system (locally or remotely) Windows shortcut keys like Alt+Tab should be directed to.
		/// </summary>
		public KeyboardMode KeyboardMode
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether the remote system should connect to local printers.
		/// </summary>
		public bool ConnectPrinters
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether the remote session should use the local clipboard.
		/// </summary>
		public bool ConnectClipboard
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether the remote system should map network drives to the user's local hard drive instances.
		/// </summary>
		public bool ConnectDrives
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether we should display the remote desktop background.
		/// </summary>
		public bool DesktopBackground
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether we should use font smoothing when rendering text from the remote system.
		/// </summary>
		public bool FontSmoothing
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether advanced visual effects like Aero Glass should be enabled.
		/// </summary>
		public bool DesktopComposition
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether a window's contents should be displayed while it is being dragged around the screen.
		/// </summary>
		public bool WindowContentsWhileDragging
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether we should animate the showing and hiding of menus.
		/// </summary>
		public bool Animations
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether the Windows Basic theme should be used when displaying the user's desktop.
		/// </summary>
		public bool VisualStyles
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether we should use bitmap caching during the rendering process.
		/// </summary>
		public bool PersistentBitmapCaching
		{
			get;
			set;
		}

		/// <summary>
		/// Flag indicating whether we should connect to the admin channel (the local, physical desktop display) when establishing a connection.
		/// </summary>
		public bool ConnectToAdminChannel
		{
			get;
			set;
		}

        /// <summary>
        /// Flag indicating whether we should connect via a TS proxy.
        /// </summary>
        public bool UseTSProxy
        {
            get;
            set;
        }

        /// <summary>
        /// The name for the proxy server.
        /// </summary>
        public string ProxyName
        {
            get;
            set;
        }

        /// <summary>
        /// The user name for the proxy server.
        /// </summary>
        public string ProxyUserName
        {
            get;
            set;
        }

        /// <summary>
        /// Encrypted and Base64-encoded password for the proxy server.
        /// </summary>
        [XmlElement("ProxyPassword")]
        public string EncryptedProxyPassword
        {
            get
            {
                if (ProxyPassword == null || ProxyPassword.Length == 0)
                    return null;

                return Convert.ToBase64String(ConnectionFactory.Encrypt(ProxyPassword));
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    ProxyPassword = null;
                    return;
                }

                // Decrypt the password and put it into a secure string
                SecureString password = new SecureString();
                byte[] decryptedPassword = ConnectionFactory.Decrypt(Convert.FromBase64String(value));

                for (int i = 0; i < decryptedPassword.Length; i++)
                {
                    if (decryptedPassword[i] == 0)
                        break;

                    password.AppendChar((char)decryptedPassword[i]);
                    decryptedPassword[i] = 0;
                }

                ProxyPassword = password;
            }
        }

        /// <summary>
        /// The raw text of the password for the proxy server.
        /// </summary>
        [XmlIgnore]
        public SecureString ProxyPassword
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

			info.AddValue("Animations", Animations);
			info.AddValue("AudioMode", AudioMode);
			info.AddValue("ColorDepth", ColorDepth);
			info.AddValue("ConnectClipboard", ConnectClipboard);
			info.AddValue("ConnectDrives", ConnectDrives);
			info.AddValue("ConnectPrinters", ConnectPrinters);
			info.AddValue("DesktopBackground", DesktopBackground);
			info.AddValue("DesktopComposition", DesktopComposition);
			info.AddValue("DesktopHeight", DesktopHeight);
			info.AddValue("DesktopWidth", DesktopWidth);
			info.AddValue("FontSmoothing", FontSmoothing);
			info.AddValue("KeyboardMode", KeyboardMode);
			info.AddValue("PersistentBitmapCaching", PersistentBitmapCaching);
			info.AddValue("RecordingMode", RecordingMode);
			info.AddValue("VisualStyles", VisualStyles);
			info.AddValue("WindowContentsWhileDragging", WindowContentsWhileDragging);
			info.AddValue("ConnectToAdminChannel", ConnectToAdminChannel);

            info.AddValue("UseTSProxy", UseTSProxy);
            info.AddValue("ProxyName", ProxyName);
            info.AddValue("ProxyUserName", ProxyUserName);
            info.AddValue(
                "ProxyPassword", ProxyPassword == null
                                     ? null
                                     : EncryptedProxyPassword);
		}
	}
}