using System;
using System.Resources;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;

using Poderosa;
using Poderosa.Toolkit;
using Poderosa.Connection;
using Poderosa.ConnectionParam;
using Poderosa.Terminal;
using Poderosa.Forms;
using Poderosa.Communication;
using Poderosa.Config;
using Poderosa.MacroEnv;
using Poderosa.Text;
using Poderosa.UI;
using Granados.SSHC;

namespace WalburySoftware
{
    public class TerminalControl : Control
    {
        #region fields
        private string _username = "";
        private string _password = "";
        private string _hostname = "";
        private string _identifyFile = "";
        private AuthType _authType = AuthType.Password;
        private ConnectionMethod _connectionMethod;
        private TerminalPane _terminalPane;
        #endregion
        #region Constructors
        public TerminalControl(string UserName, string Password, string Hostname, ConnectionMethod Method)
        {
            this._connectionMethod = Method;
            this._hostname = Hostname;
            this._password = Password;
            this._username = UserName;

            this.InitializeTerminalPane();
        }
        public TerminalControl()
        {
            this.InitializeTerminalPane();
        }
        private void InitializeTerminalPane()
        {
            if (GApp._frame == null)
            {
                string[] args = new string[0];
                GApp.Run(args);
                GApp._frame._multiPaneControl.InitUI(null, GApp.Options);
                GEnv.InterThreadUIService.MainFrameHandle = GApp._frame.Handle;
            }
            this._terminalPane = new TerminalPane();
            this.TerminalPane.Dock = DockStyle.Fill;
            this.Controls.Add(this.TerminalPane);
        }
        #endregion
        #region methods
        public void ApplyNewDisplayDialog()
        { 
        
        }
        public void Connect()
        {
            #region old stuff
            /*
            Poderosa.ConnectionParam.LogType logType = Poderosa.ConnectionParam.LogType.Default;
            string file = null;
            if (this.TerminalPane.Connection != null)
            {
                logType = this.TerminalPane.Connection.LogType;
                file = this.TerminalPane.Connection.LogPath;
                //GApp.GetConnectionCommandTarget().Close();
                this.TerminalPane.Connection.Close();
                this.TerminalPane.Detach();
            }


            SSHTerminalParam p = new SSHTerminalParam((Poderosa.ConnectionParam.ConnectionMethod)this.Method, this.Host, this.UserName, this.Password);
            
            GApp.GlobalCommandTarget.SilentNewConnection(p);
            

            if (file != null)
                this.SetLog((LogType) logType, file, true);
            */
            #endregion

            // Save old log info in case this is a reconnect
            Poderosa.ConnectionParam.LogType logType = Poderosa.ConnectionParam.LogType.Default;
            string file = null;
            if (this.TerminalPane.Connection != null)
            {
                logType = this.TerminalPane.Connection.LogType;
                file = this.TerminalPane.Connection.LogPath;
                //GApp.GetConnectionCommandTarget().Close();
                this.TerminalPane.Connection.Close();
                this.TerminalPane.Detach();
            }

            try
            {
                //------------------------------------------------------------------------
                SSHTerminalParam sshp = new SSHTerminalParam((Poderosa.ConnectionParam.ConnectionMethod)this.Method, this.Host, this.UserName, this.Password);
                sshp.AuthType = this.AuthType;
                sshp.IdentityFile = this.IdentifyFile;
                sshp.Encoding = EncodingType.ISO8859_1;
                sshp.Port = 22;
                sshp.RenderProfile = new RenderProfile();
                sshp.TerminalType = TerminalType.XTerm;

                CommunicationUtil.SilentClient s = new CommunicationUtil.SilentClient();
                Size sz = this.Size;

                SocketWithTimeout swt;
                swt = new SSHConnector((Poderosa.ConnectionParam.SSHTerminalParam)sshp, sz, sshp.Passphrase, (HostKeyCheckCallback)null);
                swt.AsyncConnect(s, sshp.Host, sshp.Port);
                ConnectionTag ct = s.Wait(swt);

                this.TerminalPane.FakeVisible = true;

                this.TerminalPane.Attach(ct);
                ct.Receiver.Listen();
                //-------------------------------------------------------------
                if (file != null)
                    this.SetLog((LogType)logType, file, true);
                this.TerminalPane.ConnectionTag.RenderProfile = new RenderProfile();
                this.SetPaneColors(Color.LightBlue, Color.Black);
            }
            catch
            {
                //MessageBox.Show(e.Message, "Connection Error");
                return;
            }
        }
        public void Close()
        {
            if (this.TerminalPane.Connection != null)
            {
                this.TerminalPane.Connection.Close();
                this.TerminalPane.Detach();
            }
        }
        public void SendText(string command)
        {
            //GApp.GetConnectionCommandTarget().Connection.WriteChars(command.ToCharArray());
            this.TerminalPane.Connection.WriteChars(command.ToCharArray());
        }
        public string GetLastLine()
        {
            return new string(this.TerminalPane.Document.LastLine.Text);
        }
        /// <summary>
        /// Wait until some data is recieved
        /// </summary>
        public void WaitConnected()
        {
            while (this.TerminalPane.Connection.ReceivedDataSize == 0)
            { }
        }
        /// <summary>
        /// Create New Log
        /// </summary>
        /// <param name="logType">I guess just use Default all the time here</param>
        /// <param name="File">This should be a full path. Example: @"C:\Temp\logfilename.txt"</param>
        /// <param name="append">Set this to true</param>
        public void SetLog(LogType logType, string File, bool append)
        {
            // make sure directory exists
            string dir = File.Substring(0, File.LastIndexOf(@"\"));
            if (!System.IO.Directory.Exists(dir))
              System.IO.Directory.CreateDirectory(dir);

            this.TerminalPane.Connection.ResetLog((Poderosa.ConnectionParam.LogType)logType, File, append);
            //this.TerminalPane.Connection.ResetLog(Poderosa.ConnectionParam.LogType.Default, File, append);
        }
        public void CommentLog(string comment)
        {
            DateTime dt = new DateTime();
            string s = "\r\n----- Comment added " + dt.Date + " -----\r\n";

            this.TerminalPane.Connection.TextLogger.Comment(s);
            this.TerminalPane.Connection.BinaryLogger.Comment(s);


            this.TerminalPane.Connection.TextLogger.Comment(comment);
            this.TerminalPane.Connection.BinaryLogger.Comment(comment);

            s = "\r\n----------------------------------------------\r\n";
            this.TerminalPane.Connection.TextLogger.Comment(s);
            this.TerminalPane.Connection.BinaryLogger.Comment(s);

        }
        public void SetPaneColors(Color TextColor, Color BackColor)
        {
            RenderProfile prof = this.TerminalPane.ConnectionTag.RenderProfile;
            prof.BackColor = BackColor;
            prof.ForeColor = TextColor;
            
            this.TerminalPane.ApplyRenderProfile(prof);


        }
        public void CopySelectedTextToClipboard()
        {
            //GApp.GlobalCommandTarget.Copy();

            if (GEnv.TextSelection.IsEmpty) return;

            string t = GEnv.TextSelection.GetSelectedText();
            if (t.Length > 0)
                Clipboard.SetDataObject(t, false);
            
        }
        public void PasteTextFromClipboard()
        {
            //GApp.GetConnectionCommandTarget().Paste();
            string value = (string)Clipboard.GetDataObject().GetData("Text");
            if (value == null || value.Length == 0 || this.TerminalPane == null || this.TerminalPane.ConnectionTag == null) return ;

            PasteProcessor p = new PasteProcessor(this.TerminalPane.ConnectionTag, value);
            p.Perform();
            
        }
        #endregion
        #region Properties
        public AuthType AuthType
        {
            get
            {
                return this._authType;
            }
            set
            {
                this._authType = value; ;
            }
        }
        public string IdentifyFile
        {
            get
            {
                return this._identifyFile;
            }
            set
            {
                this._identifyFile = value;
            }
        }
        public TerminalPane TerminalPane
        {
            get
            {
                return this._terminalPane;
            }
        }
        public string UserName
        {
            get
            {
                return this._username;
            }
            set
            {
                this._username = value;
            }
        }
        public string Password
        {
            get
            {
                return this._password;
            }
            set
            {
                this._password = value;
            }
        }
        public string Host
        {
            get
            {
                return this._hostname;
            }
            set
            {
                this._hostname = value;
            }
        }
        public ConnectionMethod Method
        {
            get
            {
                return this._connectionMethod;
            }
            set
            {
                this._connectionMethod = value;
            }
        }
        public static int ScrollBackBuffer
        {
            get
            {
                return GApp.Options.TerminalBufferSize;
            }
            set
            {
                GApp.Options.TerminalBufferSize = value;
             }
        }
        #endregion
        #region overrides
        protected override void OnKeyDown(KeyEventArgs e)
        {
            // this don't work :(
            //Console.WriteLine(e.KeyCode);
            base.OnKeyDown(e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            //Console.WriteLine(e.KeyChar);
            // this don't work :(
            base.OnKeyPress(e);
        }
        protected override void OnGotFocus(EventArgs e)
        {

            base.OnGotFocus(e);

            this.TerminalPane.Focus();
        }
        #endregion
    }
    #region enums
    public enum ConnectionMethod
    {
        /// <summary>
        /// Telnet
        /// </summary>
        Telnet,
        /// <summary>
        /// SSH1
        /// </summary>
        SSH1,
        /// <summary>
        /// SSH2
        /// </summary>
        SSH2
    }
    public enum LogType
    {
        /// <summary>
        /// <ja>ログはとりません。</ja>
        /// <en>The log is not recorded.</en>
        /// </summary>
        [EnumValue(Description = "Enum.LogType.None")]
        None,
        /// <summary>
        /// <ja>テキストモードのログです。これが標準です。</ja>
        /// <en>The log is a plain text file. This is standard.</en>
        /// </summary>
        [EnumValue(Description = "Enum.LogType.Default")]
        Default,
        /// <summary>
        /// <ja>バイナリモードのログです。</ja>
        /// <en>The log is a binary file.</en>
        /// </summary>
        [EnumValue(Description = "Enum.LogType.Binary")]
        Binary,
        /// <summary>
        /// <ja>XMLで保存します。また内部的なバグ追跡においてこのモードでのログ採取をお願いすることがあります。</ja>
        /// <en>The log is an XML file. We may ask you to record the log in this type for debugging.</en>
        /// </summary>
        [EnumValue(Description = "Enum.LogType.Xml")]
        Xml
    }    
    #endregion
}
