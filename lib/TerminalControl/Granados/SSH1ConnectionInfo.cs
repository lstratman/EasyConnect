// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.KnownHosts;
using Granados.PKI;
using Granados.Util;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Granados.SSH1 {

    /// <summary>
    /// SSH1 protocol flags
    /// </summary>
    [Flags]
    internal enum SSH1ProtocolFlags : uint {
        SSH_PROTOFLAG_SCREEN_NUMBER = 1,
        SSH_PROTOFLAG_HOST_IN_FWD_OPEN = 2,
    }

    /// <summary>
    /// A class retains miscellaneous informations about a SSH1 connection.
    /// </summary>
    internal class SSH1ConnectionInfo {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostName">host name</param>
        /// <param name="portNumber">port number</param>
        /// <param name="serverVersionString">a version string received from the server</param>
        /// <param name="clientVersionString">a version string of myself</param>
        public SSH1ConnectionInfo(string hostName, int portNumber, string serverVersionString, string clientVersionString) {
            HostName = hostName;
            PortNumber = portNumber;
            ServerVersionString = serverVersionString;
            ClientVersionString = clientVersionString;
            ClientProtocolFlags = SSH1ProtocolFlags.SSH_PROTOFLAG_SCREEN_NUMBER | SSH1ProtocolFlags.SSH_PROTOFLAG_HOST_IN_FWD_OPEN;
        }

        /// <summary>
        /// Host name
        /// </summary>
        public string HostName {
            get;
            private set;
        }

        /// <summary>
        /// Port number.
        /// </summary>
        public int PortNumber {
            get;
            private set;
        }

        /// <summary>
        /// A version string received from the server
        /// </summary>
        public string ServerVersionString {
            get;
            private set;
        }

        /// <summary>
        /// A version string of myself
        /// </summary>
        public string ClientVersionString {
            get;
            private set;
        }

        /// <summary>
        /// anti_spoofing_cookie sent by the server.
        /// </summary>
        /// <remarks>This property is null until the information is obtained from the server.</remarks>
        public byte[] AntiSpoofingCookie {
            get;
            set;
        }

        /// <summary>
        /// Bit lengths of the server key.
        /// </summary>
        /// <remarks>This property is zero until the information is obtained from the server.</remarks>
        public int ServerKeyBits {
            get;
            set;
        }

        /// <summary>
        /// Server key of the server.
        /// </summary>
        /// <remarks>This property is null until the information is obtained from the server.</remarks>
        public RSAPublicKey ServerKey {
            get;
            set;
        }

        /// <summary>
        /// Bit lengths of the host key.
        /// </summary>
        /// <remarks>This property is zero until the information is obtained from the server.</remarks>
        public int HostKeyBits {
            get;
            set;
        }

        /// <summary>
        /// Host key of the server.
        /// </summary>
        /// <remarks>This property is null until the information is obtained from the server.</remarks>
        public RSAPublicKey HostKey {
            get;
            set;
        }

        /// <summary>
        /// Server's supported encryption algorithms.
        /// </summary>
        /// <remarks>This property is zero until the information is obtained from the server.</remarks>
        public int SupportedEncryptionAlgorithmsMask {
            get;
            set;
        }

        /// <summary>
        /// Server's supported encryption algorithms.
        /// </summary>
        /// <remarks>This property is empty until the information is obtained from the server.</remarks>
        public string SupportedEncryptionAlgorithms {
            get {
                var algs = new List<string>();
                var mask = SupportedEncryptionAlgorithmsMask;
                if ((mask & 2) != 0) {
                    algs.Add("Idea");
                }
                if ((mask & 4) != 0) {
                    algs.Add("DES");
                }
                if ((mask & 8) != 0) {
                    algs.Add("TripleDES");
                }
                if ((mask & 16) != 0) {
                    algs.Add("TSS");
                }
                if ((mask & 32) != 0) {
                    algs.Add("RC4");
                }
                if ((mask & 64) != 0) {
                    algs.Add("Blowfish");
                }
                return String.Join(",", algs);
            }
        }

        /// <summary>
        /// Cipher algorithm determined to use for the outgoing packet.
        /// </summary>
        /// <remarks>This property is null until the algorithm is detemined.</remarks>
        public CipherAlgorithm? OutgoingPacketCipher {
            get;
            set;
        }

        /// <summary>
        /// Cipher algorithm determined to use for the incoming packet.
        /// </summary>
        /// <remarks>This property is null until the algorithm is detemined.</remarks>
        public CipherAlgorithm? IncomingPacketCipher {
            get;
            set;
        }

        /// <summary>
        /// Protocol flags of the client.
        /// </summary>
        public SSH1ProtocolFlags ClientProtocolFlags {
            get;
            private set;
        }

        /// <summary>
        /// Protocol flags that received from the server.
        /// </summary>
        public SSH1ProtocolFlags ServerProtocolFlags {
            get;
            set;
        }

        /// <summary>
        /// Get an object that provides informations about a host key.
        /// </summary>
        /// <returns>An object that provides informations about a host key.</returns>
        public ISSHHostKeyInformationProvider GetSSHHostKeyInformationProvider() {
            if (HostName == null) {
                throw new InvalidOperationException("HostName is null.");
            }
            if (HostKey == null) {
                throw new InvalidOperationException("HostKey is null.");
            }
            return new SSH1HostKeyInformationProvider(HostName, PortNumber, HostKey);
        }

        /// <summary>
        /// <see cref="ISSHHostKeyInformationProvider"/> imlpementation for SSH1
        /// </summary>
        private class SSH1HostKeyInformationProvider : ISSHHostKeyInformationProvider {

            private readonly RSAPublicKey _hostKey;
            private readonly Lazy<string> _knownHostsString;
            private readonly Lazy<byte[]> _encodedHostKey;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="hostName">host name</param>
            /// <param name="portNumber">port number</param>
            /// <param name="hostKey">host key</param>
            public SSH1HostKeyInformationProvider(string hostName, int portNumber, RSAPublicKey hostKey) {
                HostName = hostName;
                PortNumber = portNumber;

                _hostKey = hostKey;

                _knownHostsString =
                    new Lazy<string>(
                        () => {
                            // Poderosa known_hosts format
                            return new StringBuilder()
                                .Append("ssh1 ")
                                .Append(Encoding.ASCII.GetString(Base64.Encode(_encodedHostKey.Value)))
                                .ToString();
                        },
                        false
                    );

                _encodedHostKey =
                    new Lazy<byte[]>(
                        () => {
                            return new SSH1PayloadImageBuilder(0x10000)
                                    .WriteBigInteger(_hostKey.Exponent)
                                    .WriteBigInteger(_hostKey.Modulus)
                                    .GetBytes();
                        },
                        false
                    );
            }

            /// <summary>
            /// SSH protocol version.
            /// </summary>
            public SSHProtocol Protocol {
                get {
                    return SSHProtocol.SSH1;
                }
            }

            /// <summary>
            /// Host name
            /// </summary>
            public string HostName {
                get;
                private set;
            }

            /// <summary>
            /// Port number.
            /// </summary>
            public int PortNumber {
                get;
                private set;
            }

            /// <summary>
            /// A string in ssh_known_hosts format of the Poderosa.
            /// </summary>
            public string KnownHostsString {
                get {
                    return _knownHostsString.Value;
                }
            }

            /// <summary>
            /// Finger print of the host key.
            /// </summary>
            public byte[] HostKeyFingerPrint {
                get {
                    return new MD5CryptoServiceProvider().ComputeHash(_encodedHostKey.Value);
                }
            }
        }
    }

}
