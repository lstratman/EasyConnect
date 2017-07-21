// Copyright (c) 2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.IO;
using Granados.Util;
using Granados.X11Forwarding;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

// USE_UNTRUSTED_ACCESS enables "Untrusted" access that is provided by the SECURITY extension of the X server.
// Unfortunately this option is useless to the X server like Xming because it claims trusted access to use resources.
//#define USE_UNTRUSTED_ACCESS

namespace Granados.X11 {

    /// <summary>
    /// Connection manager
    /// </summary>
    internal class X11ConnectionManager {

        private const string DEFAULT_AUTH_NAME = "MIT-MAGIC-COOKIE-1";
        private const int SEND_TIMEOUT = 2000;
        private const int RESPONSE_TIMEOUT = 2000;

        private readonly SSHProtocolEventManager _protocolEventManager;

        private bool _setupDone = false;
        private Func<IX11Socket> _socketFactory;
        private string _spoofedAuthProtocolName;    // spoofed (fake) name
        private byte[] _spoofedAuthCookie;  // spoofed (fake) cookie
        private string _xAuthProtocolName;  // actual name to be used for the authorization
        private byte[] _xAuthCookie;    // actual cookie to be used for the authorization
        private XauthorityEntry _authEntry;
        private uint? _authId;

        private X11ForwardingParams _param;

        /// <summary>
        /// X11 forwarding parameters
        /// </summary>
        public X11ForwardingParams Params {
            get {
                return _param;
            }
        }

        /// <summary>
        /// Indicates whether the setup is already done.
        /// </summary>
        public bool SetupDone {
            get {
                return _setupDone;
            }
        }

        /// <summary>
        /// Spoofed, fake authorization protocol name for the X11 forwarding.
        /// </summary>
        /// <remarks>
        /// This property will be set in the <see cref="Setup(X11ForwardingParams)"/>.
        /// </remarks>
        public string SpoofedAuthProtocolName {
            get {
                return _spoofedAuthProtocolName;
            }
        }

        /// <summary>
        /// Spoofed, fake authorization protocol data for the X11 forwarding.
        /// </summary>
        /// <remarks>
        /// This property will be set in the <see cref="Setup(X11ForwardingParams)"/>.
        /// </remarks>
        public byte[] SpoofedAuthProtocolData {
            get {
                return _spoofedAuthCookie;
            }
        }

        /// <summary>
        /// Hexadecimal string of the spoofed authorization protocol data.
        /// </summary>
        /// <remarks>
        /// This property will be set in the <see cref="Setup(X11ForwardingParams)"/>.
        /// </remarks>
        public string SpoofedAuthProtocolDataHex {
            get {
                return String.Concat(_spoofedAuthCookie.Select(b => b.ToString("x2", NumberFormatInfo.InvariantInfo)));
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="protocolEventManager">protocol event manager</param>
        public X11ConnectionManager(SSHProtocolEventManager protocolEventManager) {
            _protocolEventManager = protocolEventManager;
        }

        /// <summary>
        /// Setup the connection manager according to the parameters.
        /// </summary>
        /// <param name="param">parameters</param>
        /// <exception cref="X11UtilException"></exception>
        /// <exception cref="X11SocketException"></exception>
        public void Setup(X11ForwardingParams param) {
            if (_setupDone) {
                return;
            }

            _param = param.Clone();

            _spoofedAuthProtocolName = DEFAULT_AUTH_NAME;
            _spoofedAuthCookie = GenerateCookie();

            if (param.UseCygwinUnixDomainSocket) {
                _protocolEventManager.Trace("[X11] Use Cygwin's domain socket");
                _socketFactory = () => new X11CygwinDomainSocket(param.X11UnixFolder);
            }
            else {
                _protocolEventManager.Trace("[X11] Use TCP socket");
                _socketFactory = () => new X11TcpSocket();
            }

            XauthorityEntry xauthEntry;
            if (param.NeedAuth) {
                string xauthFile = FindXauthorityFile(param);
                if (xauthFile == null) {
                    throw new X11UtilException(Strings.GetString("XauthorityFileNotFound"));
                }
                var parser = new XauthorityParser();
                xauthEntry = parser.FindBest(xauthFile, param.Display);
                if (xauthEntry == null) {
                    throw new X11UtilException(Strings.GetString("SuitableAuthorizationInformationNotFound"));
                }
            }
            else {
                xauthEntry = new XauthorityEntry(0, "", param.Display, "", new byte[0]);
            }

            var cookieInfo = GetUntrustedAccessCookie(_socketFactory, param.Display, xauthEntry);
            if (cookieInfo == null) {
                // no SECURITY extension
                _protocolEventManager.Trace("[X11] \"Trusted\" access will be used.");
                _xAuthProtocolName = xauthEntry.Name;
                _xAuthCookie = xauthEntry.Data;
                _authEntry = xauthEntry;
                _authId = null;
                _setupDone = true;
                return;
            }

            _protocolEventManager.Trace("[X11] \"Untrusted\" access will be used.");
            _xAuthProtocolName = DEFAULT_AUTH_NAME;
            _xAuthCookie = cookieInfo.Item2;
            _authEntry = xauthEntry;
            _authId = cookieInfo.Item1;
            _setupDone = true;
            // TODO:
            // the authorization cookie should be deleted from the X server when
            // the forwarding channel is closed.
        }

        /// <summary>
        /// Gets a channel event handler for processing the X11 forwarding.
        /// </summary>
        /// <remarks>
        /// This method will be called when the new X11 forwarding channel is created.
        /// </remarks>
        /// <param name="channel">channel object</param>
        /// <returns>a handler object if the connection to the X server was established successfully. otherwise null.</returns>
        public ISSHChannelEventHandler CreateChannelHandler(ISSHChannel channel) {
            if (_socketFactory == null) {
                _protocolEventManager.Trace("[X11] No socket factory");
                return null;
            }
            var socket = _socketFactory();

            try {
                socket.Connect(_param.Display);
            }
            catch (Exception e) {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                _protocolEventManager.Trace("[X11] Failed to connect to the X server : {0}", e.Message);
                return null;
            }

            _protocolEventManager.Trace("[X11] Connected to the X server.");

            return new X11ChannelHandler(channel, socket, _spoofedAuthProtocolName, _spoofedAuthCookie, _xAuthProtocolName, _xAuthCookie);
        }

        /// <summary>
        /// Obtain a new cookie from the X server for the "Untrusted" access.
        /// </summary>
        /// <remarks>
        /// This method will be called in the setup procedure of this manager.
        /// </remarks>
        /// <param name="socketFactory"></param>
        /// <param name="display">display number</param>
        /// <param name="entry"></param>
        /// <returns>tuple of { authorization-id, authorization-data } if the untrusted access is available. otherwise null.</returns>
        private Tuple<uint, byte[]> GetUntrustedAccessCookie(Func<IX11Socket> socketFactory, int display, XauthorityEntry entry) {
            using (IX11Socket socket = socketFactory()) {
                socket.Connect(display);

                const bool BIGENDIAN = true;

                byte[] recv = new byte[100];
                var xmsg = new XProtocolMessage(BIGENDIAN);
                var reader = new XDataReader(BIGENDIAN);

                // Note: if USE_UNTRUSTED_ACCESS was disabled, this method only checks the connectability to the X server.

                // initiation
                {
                    byte[] authName = Encoding.ASCII.GetBytes(entry.Name);
                    xmsg.Clear()
                        .AppendByte(0x42)   // MSB first
                        .AppendByte(0)  // unused
                        .AppendUInt16(11)   // protocol-major-version
                        .AppendUInt16(0)    // protocol-minor-version
                        .AppendUInt16((ushort)authName.Length) // length of authorization-protocol-name
                        .AppendUInt16((ushort)entry.Data.Length)    // length of authorization-protocol-data
                        .AppendUInt16(0)    // unused
                        .AppendBytes(authName)  // authorization-protocol-name
                        .AppendPaddingBytesOf(authName)
                        .AppendBytes(entry.Data)    // authorization-protocol-data
                        .AppendPaddingBytesOf(entry.Data);
                    if (!socket.Send(xmsg.AsDataFragment(), SEND_TIMEOUT)) {
                        throw new X11UtilException(Strings.GetString("FailedToSendMessageToXServer"));
                    }

                    if (!socket.ReceiveBytes(recv, 0, 8, RESPONSE_TIMEOUT)) {
                        throw new X11UtilException(Strings.GetString("XServerDoesntRespond"));
                    }

                    if (recv[0] != 1 /*Success*/) {
                        throw new X11UtilException(Strings.GetString("X11AuthorizationFailed"));
                    }
                    int extraDataLen = reader.ReadUInt16(recv, 6) * 4;
                    byte[] extraData = new byte[extraDataLen];
                    if (!socket.ReceiveBytes(extraData, 0, extraDataLen, RESPONSE_TIMEOUT)) {
                        throw new X11UtilException(Strings.GetString("XServerDoesntRespond"));
                    }
                }

#if USE_UNTRUSTED_ACCESS

                // QueryExtension
                {
                    byte[] extName = Encoding.ASCII.GetBytes("SECURITY");
                    xmsg.Clear()
                        .AppendByte(98)   // opcode
                        .AppendByte(0)  // unused
                        .AppendUInt16((ushort)(2 + (extName.Length + 3) / 4))   // request-length
                        .AppendUInt16((ushort)extName.Length)    // length of name
                        .AppendUInt16(0)    // unused
                        .AppendBytes(extName)  // name
                        .AppendPaddingBytesOf(extName);
                    if (!socket.Send(xmsg.AsDataFragment(), SEND_TIMEOUT)) {
                        throw new X11UtilException(Strings.GetString("FailedToSendMessageToXServer"));
                    }

                    if (!socket.ReceiveBytes(recv, 0, 32, RESPONSE_TIMEOUT)) {
                        throw new X11UtilException(Strings.GetString("XServerDoesntRespond"));
                    }
                    if (recv[0] != 1 /*Reply*/ || recv[8] != 1 /*present*/) {
                        // no SECURITY extension
                        _protocolEventManager.Trace("[X11] X server doesn't have the SECURITY extension.");
                        return null;
                    }
                }
                byte secOpcode = recv[9];   // major-opcode of the SECURITY extension

                // SecurityQueryVersion
                {
                    xmsg.Clear()
                        .AppendByte(secOpcode)   // major-opcode
                        .AppendByte(0)  // minor-opcode
                        .AppendUInt16(2)   // request-length
                        .AppendUInt16(1)    // client-major-version
                        .AppendUInt16(0);   // client-minor-version
                    if (!socket.Send(xmsg.AsDataFragment(), SEND_TIMEOUT)) {
                        throw new X11UtilException(Strings.GetString("FailedToSendMessageToXServer"));
                    }

                    if (!socket.ReceiveBytes(recv, 0, 32, RESPONSE_TIMEOUT)) {
                        throw new X11UtilException(Strings.GetString("XServerDoesntRespond"));
                    }
                }

                // SecurityGenerateAuthorization
                {
                    byte[] authName = Encoding.ASCII.GetBytes(DEFAULT_AUTH_NAME);
                    byte[] authData = new byte[0];

                    uint[] valueList = new uint[] {
                        // timeout
                        0,  // no timeout
                        // trust-level
                        1,  // SecurityClientUntrusted
                    };

                    xmsg.Clear()
                        .AppendByte(secOpcode)   // major-opcode
                        .AppendByte(1)  // minor-opcode
                        .AppendUInt16((ushort)(3 + (authName.Length + 3) / 4 + (authData.Length + 3) / 4 + valueList.Length))
                        .AppendUInt16((ushort)authName.Length)
                        .AppendUInt16((ushort)authData.Length)
                        .AppendUInt32(3)    // value-mask : timeout + trust-level
                        .AppendBytes(authName)
                        .AppendPaddingBytesOf(authName)
                        .AppendBytes(authData)
                        .AppendPaddingBytesOf(authData)
                        .AppendUInt32(valueList[0])
                        .AppendUInt32(valueList[1]);
                    if (!socket.Send(xmsg.AsDataFragment(), SEND_TIMEOUT)) {
                        throw new X11UtilException(Strings.GetString("FailedToSendMessageToXServer"));
                    }

                    if (!socket.ReceiveBytes(recv, 0, 32, RESPONSE_TIMEOUT)) {
                        throw new X11UtilException(Strings.GetString("XServerDoesntRespond"));
                    }

                    if (recv[0] != 1 /*Reply*/) {
                        return null;
                    }

                    uint authId = reader.ReadUInt32(recv, 8);

                    int extraDataLength = (int)reader.ReadUInt32(recv, 4) * 4;
                    if (extraDataLength < 0 || extraDataLength > 1024) {
                        // something wrong...
                        return null;
                    }
                    int datLength = (int)reader.ReadUInt16(recv, 12);
                    if (datLength < 0 || datLength > extraDataLength) {
                        // something wrong...
                        return null;
                    }
                    byte[] extraData = new byte[extraDataLength];
                    if (!socket.ReceiveBytes(extraData, 0, extraDataLength, RESPONSE_TIMEOUT)) {
                        throw new X11UtilException(Strings.GetString("XServerDoesntRespond"));
                    }

                    byte[] generatedAuthData = reader.ReadBytes(extraData, 0, datLength);

                    return Tuple.Create(authId, generatedAuthData);
                }

#else   // USE_UNTRUSTED_ACCESS

                // no untrusted access
                return null;

#endif  // USE_UNTRUSTED_ACCESS
            }
        }

        private byte[] GenerateCookie() {
            byte[] cookie = new byte[16];
            RNGCryptoServiceProvider.Create().GetBytes(cookie);
            return cookie;
        }

        private string FindXauthorityFile(X11ForwardingParams param) {
            string xauthPath;
            if (!String.IsNullOrEmpty(param.XauthorityFile)) {
                xauthPath = param.XauthorityFile;
            }
            else {
                var home = Environment.GetEnvironmentVariable("HOME");
                if (home == null) {
                    return null;
                }
                xauthPath = Path.Combine(home, ".Xauthority");
            }

            return File.Exists(xauthPath) ? xauthPath : null;
        }
    }

    /// <summary>
    /// A channel event handler for the X11 forwarding.
    /// </summary>
    internal class X11ChannelHandler : SimpleSSHChannelEventHandler {

        private enum Status {
            // waiting for the connection-setup message of the X protocol.
            WaitSetupMessage,
            // the connection-setup message has been sent to the X server.
            Established,
            // EOF has been received.
            GotEOF,
            // error
            HasError,
        }

        private readonly ISSHChannel _channel;
        private readonly IX11Socket _x11Sock;
        private readonly byte[] _spoofedAuthProtocolName;   // spoofed (fake) name
        private readonly byte[] _spoofedAuthCookie; // spoofed (fake) cookie
        private readonly byte[] _authProtocolName;  // actual name to be used for the authorization
        private readonly byte[] _authCookie;    // actual cookie to be used for the authorization

        private volatile Status _state = Status.WaitSetupMessage;

        private readonly ByteBuffer _messageBuffer = new ByteBuffer(0x100, 0x10000);

        private const int SEND_TIMEOUT = 5000;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="channel">SSH channel</param>
        /// <param name="x11sock">a socket which is already established the connection with the X11 server</param>
        /// <param name="spoofedAuthProtocolName">spoofed authorization protocol name</param>
        /// <param name="spoofedAuthCookie">spoofed authorization cookie</param>
        /// <param name="authProtocolName">authorization protocol name actually used for the authorization</param>
        /// <param name="authCookie">authorization cookie actually used for the authorization</param>
        public X11ChannelHandler(
                    ISSHChannel channel,
                    IX11Socket x11sock,
                    string spoofedAuthProtocolName,
                    byte[] spoofedAuthCookie,
                    string authProtocolName,
                    byte[] authCookie) {

            _channel = channel;
            _x11Sock = x11sock;
            _spoofedAuthProtocolName = Encoding.ASCII.GetBytes(spoofedAuthProtocolName ?? "");
            _spoofedAuthCookie = spoofedAuthCookie ?? new byte[0];
            _authProtocolName = Encoding.ASCII.GetBytes(authProtocolName ?? "");
            _authCookie = authCookie ?? new byte[0];
        }

        /// <summary>
        /// Implements <see cref="ISSHChannelEventHandler.OnData(DataFragment)"/>.
        /// </summary>
        /// <param name="data"></param>
        public override void OnData(DataFragment data) {
            if (_state == Status.Established) {
                // pass data to the socket
                if (!_x11Sock.Send(data, SEND_TIMEOUT)) {
                    _state = Status.HasError;
                }
                return;
            }

            if (_state == Status.WaitSetupMessage) {
                _messageBuffer.Append(data);
                ProcessSetupMessage();
            }
        }

        private void ProcessSetupMessage() {
            if (_messageBuffer.Length < 12) {
                return;
            }

            var magic = _messageBuffer[0];
            bool byteOrder;
            if (magic == 0x42) {
                // MSB first
                byteOrder = true;   // big-endian
            }
            else if (magic == 0x6c) {
                // LSB first
                byteOrder = false;  // little-endian
            }
            else {
                _state = Status.HasError;
                return;
            }

            XDataReader reader = new XDataReader(byteOrder);

            ushort majorVersion = reader.ReadUInt16(_messageBuffer.RawBuffer, _messageBuffer.RawBufferOffset + 2);
            ushort minorVersion = reader.ReadUInt16(_messageBuffer.RawBuffer, _messageBuffer.RawBufferOffset + 4);

            int nameLen = reader.ReadUInt16(_messageBuffer.RawBuffer, _messageBuffer.RawBufferOffset + 6);
            int namePadLen = (4 - (nameLen % 4)) % 4;

            int dataLen = reader.ReadUInt16(_messageBuffer.RawBuffer, _messageBuffer.RawBufferOffset + 8);
            int dataPadLen = (4 - (dataLen % 4)) % 4;

            int messageLen = 12 + nameLen + namePadLen + dataLen + dataPadLen;

            if (_messageBuffer.Length < messageLen) {
                return;
            }

            // setup message has been received

            if (nameLen != _spoofedAuthProtocolName.Length
                || !CompareBytes(_messageBuffer, 12, _spoofedAuthProtocolName)) {
                // authorization protocol name doesn't match
                _state = Status.HasError;
                return;
            }

            if (dataLen != _spoofedAuthCookie.Length
                || !CompareBytes(_messageBuffer, 12 + nameLen + namePadLen, _spoofedAuthCookie)) {
                // authorization protocol data doesn't match
                _state = Status.HasError;
                return;
            }

            // start the receiving thread
            _x11Sock.StartReceivingThread(OnDataFromXServer, OnXSocketClosed);

            // send the modified setup message to the X server
            XProtocolMessage xmsg = new XProtocolMessage(byteOrder);
            xmsg.Clear()
                .AppendByte(byteOrder ? (byte)0x42 : (byte)0x6c)
                .AppendByte(0)
                .AppendUInt16(majorVersion)
                .AppendUInt16(minorVersion)
                .AppendUInt16((ushort)_authProtocolName.Length)
                .AppendUInt16((ushort)_authCookie.Length)
                .AppendUInt16(0)
                .AppendBytes(_authProtocolName)
                .AppendPaddingBytesOf(_authProtocolName)
                .AppendBytes(_authCookie)
                .AppendPaddingBytesOf(_authCookie);

            if (!_x11Sock.Send(xmsg.AsDataFragment(), SEND_TIMEOUT)) {
                _state = Status.HasError;
                return;
            }

            // delete original setup message
            _messageBuffer.RemoveHead(messageLen);
            // send remaining bytes
            if (_messageBuffer.Length > 0) {
                if (!_x11Sock.Send(_messageBuffer.AsDataFragment(), SEND_TIMEOUT)) {
                    _state = Status.HasError;
                    return;
                }
            }

            _state = Status.Established;
        }

        private bool CompareBytes(ByteBuffer buff, int offset, byte[] data) {
            if (buff.Length < offset + data.Length) {
                return false;
            }
            byte[] buffData = buff.RawBuffer;
            int buffOffset = buff.RawBufferOffset + offset;
            int count = data.Length;
            for (int i = 0; i < count; ++i) {
                if (buffData[buffOffset++] != data[i]) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Handles incoming data from the X server.
        /// </summary>
        /// <param name="data"></param>
        private void OnDataFromXServer(DataFragment data) {
            if (_state != Status.Established) {
                if (_state == Status.WaitSetupMessage) {
                    SpinWait.SpinUntil(() => _state == Status.Established, 2000);
                }
                Thread.MemoryBarrier();
                if (_state != Status.Established) {
                    return;
                }
            }

            try {
                if (_channel.IsOpen) {
                    _channel.Send(data);
                }
            }
            catch (Exception) {
            }
        }

        /// <summary>
        /// Called when the socket has been closed by peer
        /// </summary>
        private void OnXSocketClosed() {
            try {
                _channel.Close();
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public override void OnEOF() {
            if (_state == Status.GotEOF) {
                return;
            }
            _state = Status.GotEOF;

            try {
                _x11Sock.Close();
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }

            try {
                _channel.Close();
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public override void OnClosed(bool byServer) {
            if (byServer) {
                _x11Sock.Close();
            }
        }

        public override void OnConnectionLost() {
            _x11Sock.Close();
        }

        public override void Dispose() {
            _x11Sock.Dispose();
        }
    }

}
