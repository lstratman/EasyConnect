// Copyright (c) 2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.IO;
using Granados.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Granados.X11 {

    /// <summary>
    /// Exception that is thrown in the <see cref="IX11Socket"/>.
    /// </summary>
    internal class X11SocketException : X11UtilException {
        public X11SocketException(string message)
            : base(message) {
        }
        public X11SocketException(string message, Exception exception)
            : base(message, exception) {
        }
    }

    /// <summary>
    /// Socket to connect to the X server.
    /// </summary>
    internal interface IX11Socket : IDisposable {

        /// <summary>
        /// Connects to the X server with a specified display number.
        /// </summary>
        /// <param name="display">display number</param>
        /// <exception cref="X11SocketException">error during opening the socket</exception>
        void Connect(int display);

        /// <summary>
        /// Closes the socket.
        /// </summary>
        void Close();

        /// <summary>
        /// Sends a datagram synchronously.
        /// </summary>
        /// <param name="data">datagram to send</param>
        /// <param name="timeoutMillisec">timeout to send the datagram</param>
        /// <returns>true if the data has been sent successfully.</returns>
        bool Send(DataFragment data, int timeoutMillisec);

        /// <summary>
        /// Receives bytes of the specified length synchronously.
        /// </summary>
        /// <param name="buffer">buffer to store bytes</param>
        /// <param name="offset">start position to store bytes</param>
        /// <param name="length">length of bytes to receive</param>
        /// <param name="timeoutMillisec">timeout to receive the next datagram</param>
        /// <returns>true if the bytes of the specified length was received successfully.</returns>
        bool ReceiveBytes(byte[] buffer, int offset, int length, int timeoutMillisec);

        /// <summary>
        /// Starts a receiving thread.
        /// </summary>
        /// <param name="onDataCallback">a callback that handles the received data.</param>
        /// <param name="onClosedCallback">a callback which will be called when the socket has been closed by peer.</param>
        /// <remarks>
        /// After the receiving thread was started, <see cref="IX11Socket.ReceiveBytes(byte[], int, int, int)"/> will always fail.
        /// </remarks>
        void StartReceivingThread(Action<DataFragment> onDataCallback, Action onClosedCallback);
    }

    /// <summary>
    /// Abstract implementation of the <see cref="IX11Socket"/>.
    /// </summary>
    internal abstract class AbstractX11Socket : IX11Socket {

        private Socket _socket = null;
        private Thread _receivingThread = null;
        private volatile bool _shutdown = false;

        protected AbstractX11Socket() {
        }

        // Connection process
        protected abstract Socket DoConnect(int display);

        public void Connect(int display) {
            if (_socket != null) {
                throw new X11SocketException("already connected.");
            }
            _socket = DoConnect(display);
        }

        public void Close() {
            if (_socket != null && !_shutdown) {
                _shutdown = true;
                Thread.MemoryBarrier();
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }

        public bool Send(DataFragment data, int timeoutMillisec) {
            if (_socket == null) {
                return false;
            }
            return SendBytes(_socket, data.Data, data.Offset, data.Length, timeoutMillisec);
        }

        public bool ReceiveBytes(byte[] buffer, int offset, int length, int timeoutMillisec) {
            if (_socket == null || _receivingThread != null) {
                return false;
            }
            return ReceiveBytes(_socket, buffer, offset, length, timeoutMillisec);
        }

        protected bool SendBytes(Socket sock, byte[] buffer, int offset, int length, int timeoutMillisec) {
            int oldTimeout = sock.SendTimeout;
            try {
                sock.SendTimeout = timeoutMillisec;
                int len = sock.Send(buffer, offset, length, SocketFlags.None);
                return len == length;
            }
            catch (SocketException e) {
                Debug.WriteLine(e.Message);
                return false;
            }
            finally {
                sock.SendTimeout = oldTimeout;
            }
        }

        protected bool ReceiveBytes(Socket sock, byte[] buffer, int offset, int length, int timeoutMillisec) {
            int oldTimeout = sock.ReceiveTimeout;
            try {
                sock.ReceiveTimeout = timeoutMillisec;
                int ofs = offset;
                int len = length;
                while (len > 0) {
                    int read = sock.Receive(buffer, offset, length, SocketFlags.None);
                    if (read <= 0) {
                        return false;
                    }
                    ofs += read;
                    len -= read;
                }
                return true;
            }
            catch (SocketException e) {
                Debug.WriteLine(e.Message);
                return false;
            }
            finally {
                sock.ReceiveTimeout = oldTimeout;
            }
        }

        public void StartReceivingThread(Action<DataFragment> onDataCallback, Action onClosedCallback) {
            if (_socket == null || _receivingThread != null) {
                return;
            }

            _receivingThread = new Thread(() => {
                byte[] buffer = new byte[0x20000];
                DataFragment dataFrag = new DataFragment(buffer, 0, 0);

                try {
                    while (true) {
                        int received = _socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                        if (received > 0) {
                            dataFrag.SetLength(0, received);
                            onDataCallback(dataFrag);
                        }
                    }
                }
                catch (Exception e) {
                    if (!_shutdown) {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                        System.Diagnostics.Debug.WriteLine(e.StackTrace);
                    }

                    if (!_socket.Connected && !_shutdown) {
                        // closed by the peer
                        onClosedCallback();
                    }
                }
            });
            _receivingThread.Start();
        }

        public virtual void Dispose() {
            if (_socket != null) {
                _socket.Dispose();
            }
        }
    }

    /// <summary>
    /// A <see cref="IX11Socket"/> implementation that connects to the X server with a TCP port.
    /// </summary>
    internal class X11TcpSocket : AbstractX11Socket {

        private const int X_PORT_BASE = 6000;
        private const int X_PORT_MAX = 65535;

        public X11TcpSocket() {
        }

        protected override Socket DoConnect(int display) {
            if (display < 0 || display > X_PORT_MAX - X_PORT_BASE) {
                throw new X11SocketException(Strings.GetString("InvalidDisplayNumber"));
            }

            return ConnectSocket(X_PORT_BASE + display);
        }

        protected Socket ConnectSocket(int port) {
            var addrs = new List<IPAddress>();
            if (Socket.OSSupportsIPv4) {
                addrs.Add(IPAddress.Loopback);
            }
            if (Socket.OSSupportsIPv6) {
                addrs.Add(IPAddress.IPv6Loopback);
            }

            Socket xsocket = null;
            foreach (IPAddress addr in addrs) {
                Socket sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
                try {
                    sock.Connect(new IPEndPoint(addr, port));
                    sock.NoDelay = true;
                    xsocket = sock;
                    break;
                }
                catch (Exception) {
                    sock.Dispose();
                }
            }

            if (xsocket == null) {
                throw new X11SocketException(Strings.GetString("FailedConnectXServer"));
            }

            return xsocket;
        }
    }

    /// <summary>
    /// A <see cref="IX11Socket"/> implementation that connects to the X server
    /// by using the Cygwin's unix domain socket emulation.
    /// </summary>
    internal class X11CygwinDomainSocket : X11TcpSocket {

        private const int SEND_TIMEOUT = 1000;
        private const int RESPONSE_TIMEOUT = 2000;

        private readonly string _x11UnixPath;

        public X11CygwinDomainSocket(string x11UnixPath) {
            _x11UnixPath = x11UnixPath;
        }

        protected override Socket DoConnect(int display) {
            string sockPath = FindDomainSocketFile(display);
            var info = ParseDomainSocketFile(sockPath);
            int port = info.Item1;
            uint[] guid = info.Item2;

            Socket sock = null;
            try {
                sock = ConnectSocket(port);
                Negotiate(sock, guid);
            }
            catch (Exception e) {
                if (sock != null) {
                    sock.Dispose();
                }
                if (e is X11SocketException) {
                    throw;
                }
                throw new X11SocketException(Strings.GetString("FailedConnectXServerWithCygwinDomainSocket"), e);
            }

            return sock;
        }

        private string FindDomainSocketFile(int display) {
            if (_x11UnixPath == null || _x11UnixPath.Length == 0) {
                throw new X11SocketException(Strings.GetString("X11UnixDirectoryPathRequired"));
            }

            string path = Path.Combine(_x11UnixPath, "X" + display.ToString(NumberFormatInfo.InvariantInfo));
            if (!File.Exists(path)) {
                throw new X11SocketException(Strings.GetString("DomainSocketFileNotFound"));
            }

            return path;
        }

        private Tuple<int, uint[]> ParseDomainSocketFile(string path) {
            string line;
            try {
                line = File.ReadAllText(path, Encoding.ASCII);
            }
            catch (Exception e) {
                throw new X11SocketException(Strings.GetString("FailedToReadDomainSocketFile"), e);
            }

            var match = Regex.Match(line, @"^!<socket\s*>(\d+)\s+s\s+([a-fA-F0-9]{8})-([a-fA-F0-9]{8})-([a-fA-F0-9]{8})-([a-fA-F0-9]{8})");
            if (!match.Success) {
                throw new X11SocketException(Strings.GetString("FailedToReadDomainSocketFile"));
            }

            try {
                int port = Int32.Parse(match.Groups[1].Value, NumberFormatInfo.InvariantInfo);
                uint[] guid = {
                    UInt32.Parse(match.Groups[2].Value, NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo),
                    UInt32.Parse(match.Groups[3].Value, NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo),
                    UInt32.Parse(match.Groups[4].Value, NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo),
                    UInt32.Parse(match.Groups[5].Value, NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo)
                };

                return Tuple.Create(port, guid);
            }
            catch (Exception e) {
                throw new X11SocketException(Strings.GetString("FailedToReadDomainSocketFile"), e);
            }
        }

        private void Negotiate(Socket sock, uint[] guid) {
            byte[] secret = new byte[16];
            putUInt32LE(secret, 0, guid[0]);
            putUInt32LE(secret, 4, guid[1]);
            putUInt32LE(secret, 8, guid[2]);
            putUInt32LE(secret, 12, guid[3]);

            bool sent = SendBytes(sock, secret, 0, secret.Length, SEND_TIMEOUT);
            if (!sent) {
                throw new X11SocketException(Strings.GetString("FailedToOpenCygwinDomainSocket"));
            }

            byte[] secretRsp = new byte[16];
            bool received = ReceiveBytes(sock, secretRsp, 0, secretRsp.Length, RESPONSE_TIMEOUT);
            if (!received || !secretRsp.SequenceEqual(secret)) {
                throw new X11SocketException(Strings.GetString("FailedToOpenCygwinDomainSocket"));
            }

            uint pid = (uint)Process.GetCurrentProcess().Id;
            uint uid = 0;    // use a constant value because we cannot get valid euid in the Cygwin environment
            uint gid = 0;    // use a constant value because we cannot get valid egid in the Cygwin environment

            byte[] credentials = new byte[12];
            putUInt32LE(credentials, 0, pid);
            putUInt32LE(credentials, 4, uid);
            putUInt32LE(credentials, 8, gid);

            sent = SendBytes(sock, credentials, 0, credentials.Length, SEND_TIMEOUT);
            if (!sent) {
                throw new X11SocketException(Strings.GetString("FailedToOpenCygwinDomainSocket"));
            }

            byte[] credentialsRsp = new byte[12];
            received = ReceiveBytes(sock, credentialsRsp, 0, credentialsRsp.Length, RESPONSE_TIMEOUT);
            if (!received) {
                throw new X11SocketException(Strings.GetString("FailedToOpenCygwinDomainSocket"));
            }

            // ok, the pseudo domain socket has opened
        }

        private void putUInt32LE(byte[] buff, int offset, uint value) {
            buff[offset] = (byte)value;
            value >>= 8;
            buff[++offset] = (byte)value;
            value >>= 8;
            buff[++offset] = (byte)value;
            value >>= 8;
            buff[++offset] = (byte)value;
        }
    }

}
