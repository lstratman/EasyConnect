// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.IO;
using Granados.KnownHosts;
using Granados.PKI;
using Granados.Util;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Granados.SSH2 {

    /// <summary>
    /// A class retains miscellaneous informations about a SSH2 connection.
    /// </summary>
    internal class SSH2ConnectionInfo {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostName">host name</param>
        /// <param name="portNumber">port number</param>
        /// <param name="serverVersionString">a version string received from the server</param>
        /// <param name="clientVersionString">a version string of myself</param>
        public SSH2ConnectionInfo(string hostName, int portNumber, string serverVersionString, string clientVersionString) {
            HostName = hostName;
            PortNumber = portNumber;
            ServerVersionString = serverVersionString;
            ClientVersionString = clientVersionString;
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
        /// Server's supported KEX algorithms
        /// </summary>
        /// <remarks>This property is null until the information is obtained from the server.</remarks>
        public string SupportedKEXAlgorithms {
            get;
            set;
        }

        /// <summary>
        /// Server's supported host key algorithms
        /// </summary>
        /// <remarks>This property is null until the information is obtained from the server.</remarks>
        public string SupportedHostKeyAlgorithms {
            get;
            set;
        }

        /// <summary>
        /// Server's supported encryption algorithms (client to server).
        /// </summary>
        /// <remarks>This property is null until the information is obtained from the server.</remarks>
        public string SupportedEncryptionAlgorithmsClientToServer {
            get;
            set;
        }

        /// <summary>
        /// Server's supported encryption algorithms (server to client).
        /// </summary>
        /// <remarks>This property is null until the information is obtained from the server.</remarks>
        public string SupportedEncryptionAlgorithmsServerToClient {
            get;
            set;
        }

        /// <summary>
        /// Host key algorithm determined to use for the verification of the server's host key.
        /// </summary>
        /// <remarks>This property is null until the algorithm is detemined.</remarks>
        public PublicKeyAlgorithm? HostKeyAlgorithm {
            get;
            set;
        }

        /// <summary>
        /// KEX algorithm determined to use.
        /// </summary>
        /// <remarks>This property is null until the algorithm is detemined.</remarks>
        public KexAlgorithm? KEXAlgorithm {
            get;
            set;
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
        /// Public key of the server.
        /// </summary>
        /// <remarks>This property is null until the information is obtained from the server.</remarks>
        public PublicKey HostKey {
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
            return new SSH2HostKeyInformationProvider(HostName, PortNumber, HostKey);
        }

        /// <summary>
        /// <see cref="ISSHHostKeyInformationProvider"/> imlpementation for SSH2
        /// </summary>
        private class SSH2HostKeyInformationProvider : ISSHHostKeyInformationProvider {

            private readonly PublicKey _hostKey;
            private readonly Lazy<string> _knownHostsString;
            private readonly Lazy<byte[]> _encodedHostKey;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="hostName">host name</param>
            /// <param name="portNumber">port number</param>
            /// <param name="hostKey">host key</param>
            public SSH2HostKeyInformationProvider(string hostName, int portNumber, PublicKey hostKey) {
                HostName = hostName;
                PortNumber = portNumber;

                _hostKey = hostKey;

                _knownHostsString =
                    new Lazy<string>(
                        () => {
                            // Poderosa known_hosts format
                            return new StringBuilder()
                                .Append(_hostKey.Algorithm.GetAlgorithmName())
                                .Append(' ')
                                .Append(Encoding.ASCII.GetString(Base64.Encode(_encodedHostKey.Value)))
                                .ToString();
                        },
                        false
                    );

                _encodedHostKey =
                    new Lazy<byte[]>(
                        () => {
                            SSH2PayloadImageBuilder image = new SSH2PayloadImageBuilder(0x10000);
                            image.WriteString(_hostKey.Algorithm.GetAlgorithmName());
                            if (_hostKey is RSAPublicKey) {
                                RSAPublicKey rsa = (RSAPublicKey)_hostKey;
                                image.WriteBigInteger(rsa.Exponent);
                                image.WriteBigInteger(rsa.Modulus);
                            }
                            else if (_hostKey is DSAPublicKey) {
                                DSAPublicKey dsa = (DSAPublicKey)_hostKey;
                                image.WriteBigInteger(dsa.P);
                                image.WriteBigInteger(dsa.Q);
                                image.WriteBigInteger(dsa.G);
                                image.WriteBigInteger(dsa.Y);
                            }
                            else if (_hostKey is ECDSAPublicKey) {
                                ECDSAPublicKey ec = (ECDSAPublicKey)_hostKey;
                                image.WriteString(ec.CurveName);
                                image.WriteAsString(ec.ToOctetString());
                            }
                            else if (_hostKey is EDDSAPublicKey) {
                                EDDSAPublicKey ed = (EDDSAPublicKey)_hostKey;
                                image.WriteAsString(ed.Bytes);
                            }
                            else {
                                throw new SSHException("Host key algorithm is unsupported");
                            }
                            return image.GetBytes();
                        },
                        false
                    );
            }

            /// <summary>
            /// SSH protocol version.
            /// </summary>
            public SSHProtocol Protocol {
                get {
                    return SSHProtocol.SSH2;
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
                    using (var md5 = new MD5CryptoServiceProvider()) {
                        return md5.ComputeHash(_encodedHostKey.Value);
                    }
                }
            }
        }
    }

}
