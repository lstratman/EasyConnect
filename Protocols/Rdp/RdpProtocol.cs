using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Windows.Forms;
using AxMSTSCLib;

namespace EasyConnect.Protocols.Rdp
{
    public class RdpProtocol : BaseProtocol<RdpConnection, RdpOptionsForm, RdpConnectionForm>
    {
        public override string ProtocolPrefix
        {
            get
            {
                return "rdp";
            }
        }

        public override string ProtocolTitle
        {
            get
            {
                return "Remote Desktop";
            }
        }

        public override void Connect(Panel containerPanel, RdpConnection connection)
        {
            AxMsRdpClient2 rdpWindow = new AxMsRdpClient2();

            rdpWindow.Width = containerPanel.Width;
            rdpWindow.DesktopWidth = (connection.DesktopWidth == 0
                                ? _rdcWindow.Width - 2
                                : connection.DesktopWidth);
            rdpWindow.DesktopHeight = (connection.DesktopHeight == 0
                                 ? _rdcWindow.Height - 1
                                 : connection.DesktopHeight);
            AudioMode = connection.AudioMode;
            KeyboardMode = connection.KeyboardMode;
            ConnectPrinters = connection.ConnectPrinters;
            ConnectClipboard = connection.ConnectClipboard;
            ConnectDrives = connection.ConnectDrives;
            DesktopBackground = connection.DesktopBackground;
            FontSmoothing = connection.FontSmoothing;
            DesktopComposition = connection.DesktopComposition;
            WindowContentsWhileDragging = connection.WindowContentsWhileDragging;
            Animations = connection.Animations;
            VisualStyles = connection.VisualStyles;
            PersistentBitmapCaching = connection.PersistentBitmapCaching;

            if (!String.IsNullOrEmpty(connection.Username))
                Username = connection.Username;

            if (connection.Password != null && connection.Password.Length > 0)
                Password = connection.Password;

            Host = connection.Host;
            MediaTypeNames.Text = connection.DisplayName;
            urlTextBox.Text = connection.Host;

            _connection = connection;
            ParentTabs.History.AddToHistory(_connection);

            containerPanel.Controls.Add(rdpWindow);

        }
    }
}
