// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.AgentForwarding;
using Granados.KeyboardInteractive;
using Granados.KnownHosts;
using Granados.PKI;
using Granados.X11Forwarding;
using System;

namespace Granados {

    /// <summary>
    /// SSH connection parameter.
    /// </summary>
    /// <remarks>
    /// Fill the properties of ConnectionParameter object before you start the connection.
    /// </remarks>
    public class SSHConnectionParameter {

        /// <summary>
        /// Host name.
        /// </summary>
        public string HostName {
            get;
            set;
        }

        /// <summary>
        /// Port number.
        /// </summary>
        public int PortNumber {
            get;
            set;
        }

        /// <summary>
        /// SSH Protocol version.
        /// </summary>
        public SSHProtocol Protocol {
            get;
            set;
        }

        /// <summary>
        /// Preferable cipher algorithms.
        /// </summary>
        public CipherAlgorithm[] PreferableCipherAlgorithms {
            get;
            set;
        }

        /// <summary>
        /// Preferable host key algorithms.
        /// </summary>
        public PublicKeyAlgorithm[] PreferableHostKeyAlgorithms {
            get;
            set;
        }

        /// <summary>
        /// Authentication type.
        /// </summary>
        public AuthenticationType AuthenticationType {
            get;
            set;
        }

        /// <summary>
        /// User name for login.
        /// </summary>
        public string UserName {
            get;
            set;
        }

        /// <summary>
        /// Password for login.
        /// </summary>
        public string Password {
            get;
            set;
        }

        /// <summary>
        /// Identity file path.
        /// </summary>
        public string IdentityFile {
            get;
            set;
        }

        /// <summary>
        /// Callback to verify a host key.
        /// </summary>
        public VerifySSHHostKeyDelegate VerifySSHHostKey {
            get;
            set;
        }

        /// <summary>
        /// A factory function to create a handler for the keyboard-interactive authentication.
        /// </summary>
        /// <remarks>
        /// This property can be null if the keyboard-interactive authentication is not used.
        /// </remarks>
        public Func<ISSHConnection, IKeyboardInteractiveAuthenticationHandler> KeyboardInteractiveAuthenticationHandlerCreator {
            get;
            set;
        }

        /// <summary>
        /// Terminal name. (vt100, xterm, etc.)
        /// </summary>
        public string TerminalName {
            get;
            set;
        }

        /// <summary>
        /// Terminal columns.
        /// </summary>
        public int TerminalWidth {
            get;
            set;
        }

        /// <summary>
        /// Terminal raws.
        /// </summary>
        public int TerminalHeight {
            get;
            set;
        }

        /// <summary>
        /// Terminal width in pixels.
        /// </summary>
        public int TerminalPixelWidth {
            get;
            set;
        }

        /// <summary>
        /// Terminal height in pixels.
        /// </summary>
        public int TerminalPixelHeight {
            get;
            set;
        }

        /// <summary>
        /// Whether integrity of the incoming packet is checked using MAC.
        /// </summary>
        public bool CheckMACError {
            get;
            set;
        }

        /// <summary>
        /// Window size of the SSH2 channel.
        /// </summary>
        /// <remarks>This property is used only in SSH2.</remarks>
        public int WindowSize {
            get;
            set;
        }

        /// <summary>
        /// Maximum packet size of the SSH2 connection.
        /// </summary>
        /// <remarks>This property is used only in SSH2.</remarks>
        public int MaxPacketSize {
            get;
            set;
        }

        /// <summary>
        /// End of line characters for terminating a version string.
        /// </summary>
        /// <remarks>
        /// Some server may expect irregular end-of-line character(s).
        /// Initial value is '\n' for SSH1 and '/r/n' for SSH2.
        /// </remarks>
        public string VersionEOL {
            get {
                return _versionEOL ?? ((Protocol == SSHProtocol.SSH1) ? "\n" : "\r\n");
            }
            set {
                _versionEOL = value;
            }
        }
        private string _versionEOL;

        /// <summary>
        /// Key provider for the agent forwarding.
        /// </summary>
        /// <remarks>
        /// This property can be null.<br/>
        /// If this property was not null, the agent forwarding will be requested to the server before a new shell is opened.
        /// </remarks>
        public IAgentForwardingAuthKeyProvider AgentForwardingAuthKeyProvider {
            get;
            set;
        }

        /// <summary>
        /// X11 forwarding parameters.
        /// </summary>
        /// <remarks>
        /// This property can be null.<br/>
        /// If this property was not null, the X11 forwarding will be requested to the server before a new shell is opened.
        /// </remarks>
        public X11ForwardingParams X11ForwardingParams {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostName">Host name</param>
        /// <param name="portNumber">port number</param>
        /// <param name="protocol">SSH protocol version</param>
        /// <param name="authType">authentication type</param>
        /// <param name="userName">user name for login</param>
        /// <param name="password">password for login. pass empty string for the keyboard interactive mode.</param>
        public SSHConnectionParameter(string hostName, int portNumber, SSHProtocol protocol, AuthenticationType authType, string userName, string password) {
            HostName = hostName;
            PortNumber = portNumber;
            Protocol = protocol;
            PreferableCipherAlgorithms = new CipherAlgorithm[] { CipherAlgorithm.AES256CTR, CipherAlgorithm.AES256, CipherAlgorithm.AES192CTR, CipherAlgorithm.AES192, CipherAlgorithm.AES128CTR, CipherAlgorithm.AES128, CipherAlgorithm.Blowfish, CipherAlgorithm.TripleDES };
            PreferableHostKeyAlgorithms = new PublicKeyAlgorithm[] { PublicKeyAlgorithm.DSA, PublicKeyAlgorithm.RSA };
            AuthenticationType = authType;
            UserName = userName;
            Password = password;
            TerminalName = "vt100";
            WindowSize = 0x1000;
            MaxPacketSize = 0x10000;
            CheckMACError = true;
            VerifySSHHostKey = p => true;
        }

        /// <summary>
        /// Clone this object.
        /// </summary>
        /// <returns>a new object.</returns>
        public SSHConnectionParameter Clone() {
            SSHConnectionParameter p = (SSHConnectionParameter)MemberwiseClone();
            p.PreferableCipherAlgorithms = (CipherAlgorithm[])p.PreferableCipherAlgorithms.Clone();
            p.PreferableHostKeyAlgorithms = (PublicKeyAlgorithm[])p.PreferableHostKeyAlgorithms.Clone();
            if (p.X11ForwardingParams != null) {
                p.X11ForwardingParams = p.X11ForwardingParams.Clone();
            }
            return p;
        }
    }
}
