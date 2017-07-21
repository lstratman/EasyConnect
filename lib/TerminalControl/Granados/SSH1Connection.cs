// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.AgentForwarding;
using Granados.Crypto;
using Granados.IO;
using Granados.IO.SSH1;
using Granados.Mono.Math;
using Granados.PKI;
using Granados.PortForwarding;
using Granados.SSH;
using Granados.Util;
using Granados.X11;
using Granados.X11Forwarding;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Granados.SSH1 {

    /// <summary>
    /// SSH1 connection
    /// </summary>
    internal class SSH1Connection : ISSHConnection, IDisposable {

        private readonly IGranadosSocket _socket;
        private readonly ISSHConnectionEventHandler _eventHandler;
        private readonly SocketStatusReader _socketStatusReader;

        private readonly SSHConnectionParameter _param;
        private readonly SSHProtocolEventManager _protocolEventManager;

        private readonly SSHChannelCollection _channelCollection;
        private SSH1InteractiveSession _interactiveSession;

        private readonly SSH1Packetizer _packetizer;
        private readonly SSH1SynchronousPacketHandler _syncHandler;
        private readonly SSHPacketInterceptorCollection _packetInterceptors;
        private readonly SSH1KeyExchanger _keyExchanger;

        private readonly Lazy<SSH1RemotePortForwarding> _remotePortForwarding;
        private readonly Lazy<SSH1AgentForwarding> _agentForwarding;
        private readonly Lazy<SSH1X11Forwarding> _x11Forwarding;

        private readonly SSH1ConnectionInfo _connectionInfo;

        private byte[] _sessionID = null;
        private AuthenticationStatus _authenticationStatus = AuthenticationStatus.NotStarted;
        private int _remotePortForwardCount = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">socket wrapper</param>
        /// <param name="param">connection parameter</param>
        /// <param name="serverVersion">server version</param>
        /// <param name="clientVersion">client version</param>
        /// <param name="connectionEventHandlerCreator">a factory function to create a connection event handler (can be null)</param>
        /// <param name="protocolEventLoggerCreator">a factory function to create a protocol log event handler (can be null)</param>
        internal SSH1Connection(
                    PlainSocket socket,
                    SSHConnectionParameter param,
                    string serverVersion,
                    string clientVersion,
                    Func<ISSHConnection, ISSHConnectionEventHandler> connectionEventHandlerCreator,
                    Func<ISSHConnection, ISSHProtocolEventLogger> protocolEventLoggerCreator) {

            _socket = socket;

            var connEventHandler = connectionEventHandlerCreator != null ? connectionEventHandlerCreator(this) : null;
            if (connEventHandler != null) {
                _eventHandler = new SSHConnectionEventHandlerIgnoreErrorWrapper(connEventHandler);
            }
            else {
                _eventHandler = new SimpleSSHConnectionEventHandler();
            }

            _protocolEventManager = new SSHProtocolEventManager(
                                        protocolEventLoggerCreator != null ?
                                            protocolEventLoggerCreator(this) : null);

            _socketStatusReader = new SocketStatusReader(socket);
            _param = param.Clone();
            _channelCollection = new SSHChannelCollection();
            _interactiveSession = null;

            _connectionInfo = new SSH1ConnectionInfo(param.HostName, param.PortNumber, serverVersion, clientVersion);

            IDataHandler adapter =
                new DataHandlerAdapter(
                    onData:
                        ProcessPacket,
                    onClosed:
                        OnConnectionClosed,
                    onError:
                        OnError
                );
            _syncHandler = new SSH1SynchronousPacketHandler(socket, adapter, _protocolEventManager);
            _packetizer = new SSH1Packetizer(_syncHandler);

            _packetInterceptors = new SSHPacketInterceptorCollection();
            _keyExchanger = new SSH1KeyExchanger(this, _syncHandler, _param, _connectionInfo, UpdateClientKey, UpdateServerKey);
            _packetInterceptors.Add(_keyExchanger);

            _remotePortForwarding = new Lazy<SSH1RemotePortForwarding>(CreateRemotePortForwarding);
            _agentForwarding = new Lazy<SSH1AgentForwarding>(CreateAgentForwarding);
            _x11Forwarding = new Lazy<SSH1X11Forwarding>(CreateX11Forwarding);

            // set packetizer as a socket data handler
            socket.SetHandler(_packetizer);
        }

        /// <summary>
        /// Lazy initialization to setup the instance of <see cref="SSH1RemotePortForwarding"/>.
        /// </summary>
        /// <returns>a new instance of <see cref="SSH1RemotePortForwarding"/></returns>
        private SSH1RemotePortForwarding CreateRemotePortForwarding() {
            var instance = new SSH1RemotePortForwarding(_syncHandler, StartIdleInteractiveSession);
            _packetInterceptors.Add(instance);
            return instance;
        }

        /// <summary>
        /// Lazy initialization to setup the instance of <see cref="SSH1AgentForwarding"/>.
        /// </summary>
        /// <returns>a new instance of <see cref="SSH1AgentForwarding"/></returns>
        private SSH1AgentForwarding CreateAgentForwarding() {
            var instance = new SSH1AgentForwarding(
                            syncHandler:
                                _syncHandler,
                            authKeyProvider:
                                _param.AgentForwardingAuthKeyProvider,
                            createChannel:
                                remoteChannel => {
                                    uint localChannel = _channelCollection.GetNewChannelNumber();
                                    return new SSH1AgentForwardingChannel(
                                                    _syncHandler, _protocolEventManager, localChannel, remoteChannel);
                                },
                            registerChannel:
                                RegisterChannel
                        );
            _packetInterceptors.Add(instance);
            return instance;
        }

        /// <summary>
        /// Lazy initialization to setup the instance of <see cref="SSH1X11Forwarding"/>.
        /// </summary>
        /// <returns>a new instance of <see cref="SSH1X11Forwarding"/></returns>
        private SSH1X11Forwarding CreateX11Forwarding() {
            var instance = new SSH1X11Forwarding(
                            syncHandler:
                                _syncHandler,
                            connectionInfo:
                                _connectionInfo,
                            protocolEventManager:
                                _protocolEventManager,
                            createChannel:
                                remoteChannel => {
                                    uint localChannel = _channelCollection.GetNewChannelNumber();
                                    return new SSH1X11ForwardingChannel(
                                                    _syncHandler, _protocolEventManager, localChannel, remoteChannel);
                                },
                            registerChannel:
                                RegisterChannel
                        );
            _packetInterceptors.Add(instance);
            return instance;
        }

        /// <summary>
        /// SSH protocol (SSH1 or SSH2)
        /// </summary>
        public SSHProtocol SSHProtocol {
            get {
                return SSHProtocol.SSH1;
            }
        }

        /// <summary>
        /// Connection parameter
        /// </summary>
        public SSHConnectionParameter ConnectionParameter {
            get {
                return _param;
            }
        }

        /// <summary>
        /// A property that indicates whether this connection is open.
        /// </summary>
        public bool IsOpen {
            get {
                return _socket.SocketStatus == SocketStatus.Ready && _authenticationStatus == AuthenticationStatus.Success;
            }
        }

        /// <summary>
        /// Authenticatrion status
        /// </summary>
        public AuthenticationStatus AuthenticationStatus {
            get {
                return _authenticationStatus;
            }
        }

        /// <summary>
        /// A proxy object for reading status of the underlying <see cref="IGranadosSocket"/> object.
        /// </summary>
        public SocketStatusReader SocketStatusReader {
            get {
                return _socketStatusReader;
            }
        }

        /// <summary>
        /// Implements <see cref="IDisposable"/>.
        /// </summary>
        public void Dispose() {
            if (_socket is IDisposable) {
                ((IDisposable)_socket).Dispose();
            }
        }

        /// <summary>
        /// Establishes a SSH connection to the server.
        /// </summary>
        /// <exception cref="SSHException">error</exception>
        internal void Connect() {
            if (_authenticationStatus != AuthenticationStatus.NotStarted) {
                throw new SSHException("Connect() was called twice.");
            }

            try {
                //key exchange
                _keyExchanger.ExecKeyExchange();

                //user authentication
                SSH1UserAuthentication userAuth = new SSH1UserAuthentication(this, _param, _syncHandler, _sessionID);
                _packetInterceptors.Add(userAuth);
                userAuth.ExecAuthentication();

                _authenticationStatus = AuthenticationStatus.Success;
            }
            catch (Exception) {
                _authenticationStatus = AuthenticationStatus.Failure;
                Close();
                throw;
            }
        }

        /// <summary>
        /// Sends a disconnect message to the server, then closes this connection.
        /// </summary>
        /// <param name="reasonCode">reason code (this value is ignored on the SSH1 connection)</param>
        /// <param name="message">a message to be notified to the server</param>
        public void Disconnect(DisconnectionReasonCode reasonCode, string message) {
            if (!this.IsOpen) {
                return;
            }
            _syncHandler.SendDisconnect(
                new SSH1Packet(SSH1PacketType.SSH_MSG_DISCONNECT)
                    .WriteString(message)
            );
            Close();
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <remarks>
        /// This method closes the underlying socket object.
        /// </remarks>
        public void Close() {
            if (_socket.SocketStatus == SocketStatus.Closed || _socket.SocketStatus == SocketStatus.RequestingClose) {
                return;
            }
            _socket.Close();
        }

        /// <summary>
        /// Sends client version to the server.
        /// </summary>
        internal void SendMyVersion() {
            string cv = _connectionInfo.ClientVersionString;
            string cv2 = cv + _param.VersionEOL;
            byte[] data = Encoding.ASCII.GetBytes(cv2);
            _socket.Write(data, 0, data.Length);
            _protocolEventManager.Trace("client version-string : {0}", cv);
        }

        /// <summary>
        /// Opens shell on an interactive session
        /// </summary>
        /// <typeparam name="THandler">type of the channel event handler</typeparam>
        /// <param name="handlerCreator">a function that creates a channel event handler</param>
        /// <returns>a new channel event handler which was created by <paramref name="handlerCreator"/></returns>
        public THandler OpenShell<THandler>(SSHChannelEventHandlerCreator<THandler> handlerCreator)
                where THandler : ISSHChannelEventHandler {

            if (_interactiveSession != null) {
                throw new SSHException(Strings.GetString("OnlySingleInteractiveSessionCanBeStarted"));
            }

            if (_param.AgentForwardingAuthKeyProvider != null) {
                bool started = _agentForwarding.Value.StartAgentForwarding();
                if (!started) {
                    _protocolEventManager.Trace("the request of the agent forwarding has been rejected.");
                    // FIXME: OpenShell() should be aborted in this case if the user can disable the agent forwarding.
                }
                else {
                    _protocolEventManager.Trace("the request of the agent forwarding has been accepted.");
                }
            }

            if (_param.X11ForwardingParams != null) {
                bool started = _x11Forwarding.Value.ListenX11Forwarding(_param.X11ForwardingParams);
                if (!started) {
                    _protocolEventManager.Trace("the request of the X11 forwarding has been rejected.");
                    throw new SSHException(Strings.GetString("X11ForwardingRejectedFromServer"));
                }
                else {
                    _protocolEventManager.Trace("the request of the X11 forwarding has been accepted.");
                }
            }

            return CreateChannelByClient(
                        handlerCreator:
                            handlerCreator,
                        channelCreator:
                            localChannel =>
                                new SSH1InteractiveSession(
                                    _syncHandler, _protocolEventManager, localChannel, ChannelType.Shell, "Shell"),
                        initiate:
                            channel => {
                                _interactiveSession = channel;
                                channel.ExecShell(_param);
                            }
                    );
        }

        /// <summary>
        /// Opens execute-command channel
        /// </summary>
        /// <typeparam name="THandler">type of the channel event handler</typeparam>
        /// <param name="handlerCreator">a function that creates a channel event handler</param>
        /// <param name="command">command to execute</param>
        /// <returns>a new channel event handler which was created by <paramref name="handlerCreator"/></returns>
        public THandler ExecCommand<THandler>(SSHChannelEventHandlerCreator<THandler> handlerCreator, string command)
                where THandler : ISSHChannelEventHandler {
            if (_interactiveSession != null) {
                throw new SSHException(Strings.GetString("OnlySingleInteractiveSessionCanBeStarted"));
            }

            return CreateChannelByClient(
                        handlerCreator:
                            handlerCreator,
                        channelCreator:
                            localChannel =>
                                new SSH1InteractiveSession(
                                    _syncHandler, _protocolEventManager, localChannel, ChannelType.ExecCommand, "ExecCommand"),
                        initiate:
                            channel => {
                                _interactiveSession = channel;
                                channel.ExecCommand(_param, command);
                            }
                    );
        }

        /// <summary>
        /// Opens subsystem channel (SSH2 only)
        /// </summary>
        /// <typeparam name="THandler">type of the channel event handler</typeparam>
        /// <param name="handlerCreator">a function that creates a channel event handler</param>
        /// <param name="subsystemName">subsystem name</param>
        /// <returns>a new channel event handler which was created by <paramref name="handlerCreator"/>.</returns>
        public THandler OpenSubsystem<THandler>(SSHChannelEventHandlerCreator<THandler> handlerCreator, string subsystemName)
                where THandler : ISSHChannelEventHandler {

            throw new NotSupportedException("OpenSubsystem is not supported on the SSH1 connection.");
        }

        /// <summary>
        /// Opens local port forwarding channel
        /// </summary>
        /// <typeparam name="THandler">type of the channel event handler</typeparam>
        /// <param name="handlerCreator">a function that creates a channel event handler</param>
        /// <param name="remoteHost">the host to connect to</param>
        /// <param name="remotePort">the port number to connect to</param>
        /// <param name="originatorIp">originator's IP address</param>
        /// <param name="originatorPort">originator's port number</param>
        /// <returns>a new channel event handler which was created by <paramref name="handlerCreator"/>.</returns>
        public THandler ForwardPort<THandler>(SSHChannelEventHandlerCreator<THandler> handlerCreator, string remoteHost, uint remotePort, string originatorIp, uint originatorPort)
                where THandler : ISSHChannelEventHandler {

            StartIdleInteractiveSession();

            return CreateChannelByClient(
                        handlerCreator:
                            handlerCreator,
                        channelCreator:
                            localChannel => new SSH1LocalPortForwardingChannel(
                                                _syncHandler, _protocolEventManager, localChannel,
                                                remoteHost, remotePort, originatorIp, originatorPort),
                        initiate:
                            channel => {
                                channel.SendOpen();
                            }
                    );
        }

        /// <summary>
        /// Requests the remote port forwarding.
        /// </summary>
        /// <param name="requestHandler">a handler that handles the port forwarding requests from the server</param>
        /// <param name="addressToBind">address to bind on the server</param>
        /// <param name="portNumberToBind">port number to bind on the server</param>
        /// <returns>true if the request has been accepted, otherwise false.</returns>
        public bool ListenForwardedPort(IRemotePortForwardingHandler requestHandler, string addressToBind, uint portNumberToBind) {

            SSH1RemotePortForwarding.CreateChannelFunc createChannel =
                (requestInfo, remoteChannel) => {
                    uint localChannel = _channelCollection.GetNewChannelNumber();
                    return new SSH1RemotePortForwardingChannel(
                                    _syncHandler,
                                    _protocolEventManager,
                                    localChannel,
                                    remoteChannel
                                );
                };

            SSH1RemotePortForwarding.RegisterChannelFunc registerChannel = RegisterChannel;

            // Note:
            //  According to the SSH 1.5 protocol specification, the client has to specify host and port
            //  the connection should be forwarded to.
            //  For keeping the interface compatible with SSH2, we use generated host-port pair that indicates
            //  which port on the server is listening.
            string hostToConnect = "granados" + Interlocked.Increment(ref _remotePortForwardCount).ToString(NumberFormatInfo.InvariantInfo);
            uint portToConnect = portNumberToBind;

            return _remotePortForwarding.Value.ListenForwardedPort(
                    requestHandler, createChannel, registerChannel, portNumberToBind, hostToConnect, portToConnect);
        }

        /// <summary>
        /// Cancel the remote port forwarding. (SSH2 only)
        /// </summary>
        /// <param name="addressToBind">address to bind on the server</param>
        /// <param name="portNumberToBind">port number to bind on the server</param>
        /// <returns>true if the remote port forwarding has been cancelled, otherwise false.</returns>
        public bool CancelForwardedPort(string addressToBind, uint portNumberToBind) {

            throw new NotSupportedException("cancellation of the port forwarding is not supported");
        }

        /// <summary>
        /// Sends ignorable data
        /// </summary>
        /// <param name="message">a message to be sent. the server may record this message into the log.</param>
        public void SendIgnorableData(string message) {
            Transmit(
                new SSH1Packet(SSH1PacketType.SSH_MSG_IGNORE)
                    .WriteString(message)
            );
        }

        /// <summary>
        /// Creates a new channel (initialted by the client)
        /// </summary>
        /// <typeparam name="TChannel">type of the channel object</typeparam>
        /// <typeparam name="THandler">type of the event handler</typeparam>
        /// <param name="handlerCreator">a function to create an event handler</param>
        /// <param name="channelCreator">a function to create a channel object</param>
        /// <param name="initiate">a function to be called after a channel has been created</param>
        /// <returns></returns>
        private THandler CreateChannelByClient<TChannel, THandler>(SSHChannelEventHandlerCreator<THandler> handlerCreator, Func<uint, TChannel> channelCreator, Action<TChannel> initiate)
            where TChannel : SSH1ChannelBase
            where THandler : ISSHChannelEventHandler {

            uint localChannel = _channelCollection.GetNewChannelNumber();
            var channel = channelCreator(localChannel);
            var eventHandler = handlerCreator(channel);

            RegisterChannel(channel, eventHandler);

            try {
                initiate(channel);
            }
            catch (Exception) {
                DetachChannel(channel);
                throw;
            }

            return eventHandler;
        }

        /// <summary>
        /// Registers a new channel to this connection.
        /// </summary>
        /// <param name="channel">a channel object</param>
        /// <param name="eventHandler">an event handler</param>
        private void RegisterChannel(SSH1ChannelBase channel, ISSHChannelEventHandler eventHandler) {
            channel.SetHandler(eventHandler);
            channel.Died += DetachChannel;
            _channelCollection.Add(channel, eventHandler);
        }

        /// <summary>
        /// Detach channel object.
        /// </summary>
        /// <param name="channel">a channel operator</param>
        private void DetachChannel(ISSHChannel channel) {
            if (Object.ReferenceEquals(channel, _interactiveSession)) {
                _interactiveSession = null;
            }
            var handler = _channelCollection.FindHandler(channel.LocalChannel);
            _channelCollection.Remove(channel);
            if (handler != null) {
                handler.Dispose();
            }
        }

        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="packet">packet to send</param>
        private void Transmit(SSH1Packet packet) {
            _syncHandler.Send(packet);
        }

        /// <summary>
        /// Processes a received packet.
        /// </summary>
        /// <param name="packet">a received packet</param>
        private void ProcessPacket(DataFragment packet) {
            try {
                DoProcessPacket(packet);
            }
            catch (Exception ex) {
                _eventHandler.OnError(ex);
            }
        }

        /// <summary>
        /// Processes a received packet.
        /// </summary>
        /// <param name="packet">a received packet</param>
        private void DoProcessPacket(DataFragment packet) {
            if (_packetInterceptors.InterceptPacket(packet)) {
                return;
            }

            if (packet.Length < 1) {
                return; // invalid packet
            }

            SSH1DataReader reader = new SSH1DataReader(packet);
            SSH1PacketType pt = (SSH1PacketType)reader.ReadByte();
            switch (pt) {
                case SSH1PacketType.SSH_SMSG_STDOUT_DATA:
                case SSH1PacketType.SSH_SMSG_STDERR_DATA:
                case SSH1PacketType.SSH_SMSG_SUCCESS:
                case SSH1PacketType.SSH_SMSG_FAILURE:
                case SSH1PacketType.SSH_SMSG_EXITSTATUS: {
                        SSH1InteractiveSession interactiveSession = _interactiveSession;
                        if (interactiveSession != null) {
                            interactiveSession.ProcessPacket(pt, reader.GetRemainingDataView());
                        }
                    }
                    break;
                case SSH1PacketType.SSH_MSG_CHANNEL_OPEN_CONFIRMATION:
                case SSH1PacketType.SSH_MSG_CHANNEL_OPEN_FAILURE:
                case SSH1PacketType.SSH_MSG_CHANNEL_DATA:
                case SSH1PacketType.SSH_MSG_CHANNEL_CLOSE:
                case SSH1PacketType.SSH_MSG_CHANNEL_CLOSE_CONFIRMATION: {
                        uint localChannel = reader.ReadUInt32();
                        var channel = _channelCollection.FindOperator(localChannel) as SSH1ChannelBase;
                        if (channel != null) {
                            channel.ProcessPacket(pt, reader.GetRemainingDataView());
                        }
                    }
                    break;
                case SSH1PacketType.SSH_MSG_IGNORE: {
                        byte[] data = reader.ReadByteString();
                        _eventHandler.OnIgnoreMessage(data);
                    }
                    break;
                case SSH1PacketType.SSH_MSG_DEBUG: {
                        string message = reader.ReadString();
                        _eventHandler.OnDebugMessage(false, message);
                    }
                    break;
                case SSH1PacketType.SSH_MSG_PORT_OPEN:
                case SSH1PacketType.SSH_SMSG_AGENT_OPEN:
                case SSH1PacketType.SSH_SMSG_X11_OPEN: {    // unhandled channel-open requests
                        var failurePacket = new SSH1Packet(SSH1PacketType.SSH_MSG_CHANNEL_OPEN_FAILURE);
                        _syncHandler.Send(failurePacket);
                    }
                    break;
                default:
                    _eventHandler.OnUnhandledMessage((byte)pt, packet.GetBytes());
                    break;
            }
        }

        /// <summary>
        /// Start idle interactive session with opening shell.
        /// </summary>
        private void StartIdleInteractiveSession() {
            if (_interactiveSession != null) {
                return;
            }
            OpenShell(channel => new SimpleSSHChannelEventHandler());
        }

        /// <summary>
        /// Tasks to do when the underlying socket has been closed. 
        /// </summary>
        private void OnConnectionClosed() {
            _channelCollection.ForEach((channel, handler) => handler.OnConnectionLost());
            _packetInterceptors.ForEach(interceptor => interceptor.OnConnectionClosed());
            _eventHandler.OnConnectionClosed();
        }

        /// <summary>
        /// Tasks to do when the underlying socket raised an exception.
        /// </summary>
        private void OnError(Exception error) {
            _eventHandler.OnError(error);
        }

        /// <summary>
        /// Updates cipher settings for the client.
        /// </summary>
        /// <param name="cipherClient"></param>
        private void UpdateClientKey(Cipher cipherClient) {
            _packetizer.SetCipher(cipherClient, _param.CheckMACError);
        }

        /// <summary>
        /// Updates cipher settings for the server.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="cipherServer"></param>
        private void UpdateServerKey(byte[] sessionId, Cipher cipherServer) {
            _sessionID = sessionId;
            _syncHandler.SetCipher(cipherServer);
        }
    }

    /// <summary>
    /// Synchronization of sending/receiving packets.
    /// </summary>
    internal class SSH1SynchronousPacketHandler : AbstractSynchronousPacketHandler<SSH1Packet> {
        #region SSH1SynchronousPacketHandler

        private readonly object _cipherSync = new object();
        private Cipher _cipher = null;

        private readonly SSHProtocolEventManager _protocolEventManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">socket object for sending packets.</param>
        /// <param name="handler">the next handler received packets are redirected to.</param>
        /// <param name="protocolEventManager">protocol event manager</param>
        public SSH1SynchronousPacketHandler(IGranadosSocket socket, IDataHandler handler, SSHProtocolEventManager protocolEventManager)
            : base(socket, handler) {

            _protocolEventManager = protocolEventManager;
        }

        /// <summary>
        /// Set cipher settings.
        /// </summary>
        /// <param name="cipher">cipher to encrypt a packet to be sent.</param>
        public void SetCipher(Cipher cipher) {
            lock (_cipherSync) {
                _cipher = cipher;
            }
        }

        /// <summary>
        /// Do additional work for a packet to be sent.
        /// </summary>
        /// <param name="packet">a packet object</param>
        protected override void BeforeSend(SSH1Packet packet) {
            SSH1PacketType packetType = packet.GetPacketType();
            switch (packetType) {
                case SSH1PacketType.SSH_CMSG_STDIN_DATA:
                case SSH1PacketType.SSH_SMSG_STDOUT_DATA:
                case SSH1PacketType.SSH_SMSG_STDERR_DATA:
                case SSH1PacketType.SSH_MSG_CHANNEL_DATA:
                case SSH1PacketType.SSH_MSG_IGNORE:
                case SSH1PacketType.SSH_MSG_DEBUG:
                    return;
            }

            _protocolEventManager.NotifySend(packetType, String.Empty);
        }

        /// <summary>
        /// Do additional work for a received packet.
        /// </summary>
        /// <param name="packet">a packet image</param>
        protected override void AfterReceived(DataFragment packet) {
            if (packet.Length == 0) {
                return;
            }

            SSH1PacketType packetType = (SSH1PacketType)packet.Data[packet.Offset];
            switch (packetType) {
                case SSH1PacketType.SSH_CMSG_STDIN_DATA:
                case SSH1PacketType.SSH_SMSG_STDOUT_DATA:
                case SSH1PacketType.SSH_SMSG_STDERR_DATA:
                case SSH1PacketType.SSH_MSG_CHANNEL_DATA:
                case SSH1PacketType.SSH_MSG_IGNORE:
                case SSH1PacketType.SSH_MSG_DEBUG:
                    return;
            }

            _protocolEventManager.NotifyReceive(packetType, String.Empty);
        }

        /// <summary>
        /// Gets the binary image of the packet to be sent.
        /// </summary>
        /// <remarks>
        /// The packet object will be allowed to be reused.
        /// </remarks>
        /// <param name="packet">a packet object</param>
        /// <returns>binary image of the packet</returns>
        protected override DataFragment GetPacketImage(SSH1Packet packet) {
            lock (_cipherSync) {
                return packet.GetImage(_cipher);
            }
        }

        /// <summary>
        /// Allows to reuse a packet object.
        /// </summary>
        /// <param name="packet">a packet object</param>
        protected override void Recycle(SSH1Packet packet) {
            packet.Recycle();
        }

        /// <summary>
        /// Gets the packet type name of the packet to be sent. (for debugging)
        /// </summary>
        /// <param name="packet">a packet object</param>
        /// <returns>packet name.</returns>
        protected override string GetMessageName(SSH1Packet packet) {
            return packet.GetPacketType().ToString();
        }

        /// <summary>
        /// Gets the packet type name of the received packet. (for debugging)
        /// </summary>
        /// <param name="packet">a packet image</param>
        /// <returns>packet name.</returns>
        protected override string GetMessageName(DataFragment packet) {
            if (packet.Length > 0) {
                return ((SSH1PacketType)packet.Data[packet.Offset]).ToString();
            }
            else {
                return "?";
            }
        }

        #endregion
    }

    /// <summary>
    /// Class for supporting key exchange sequence.
    /// </summary>
    internal class SSH1KeyExchanger : ISSHPacketInterceptor {
        #region

        private const int PASSING_TIMEOUT = 1000;
        private const int RESPONSE_TIMEOUT = 5000;

        private enum SequenceStatus {
            /// <summary>next key exchange can be started</summary>
            Idle,
            /// <summary>key exchange has been succeeded</summary>
            Succeeded,
            /// <summary>key exchange has been failed</summary>
            Failed,
            /// <summary>the connection has been closed</summary>
            ConnectionClosed,
            /// <summary>waiting for SSH_SMSG_PUBLIC_KEY</summary>
            WaitPublicKey,
            /// <summary>SSH_SMSG_PUBLIC_KEY has been received.</summary>
            PublicKeyReceived,
            /// <summary>SSH_CMSG_SESSION_KEY has been sent. waiting for SSH_SMSG_SUCCESS|SSH_SMSG_FAILURE</summary>
            WaitSessionKeyResult,
        }

        public delegate void UpdateClientKeyDelegate(Cipher cipherClient);
        public delegate void UpdateServerKeyDelegate(byte[] sessionID, Cipher cipherServer);

        private readonly UpdateClientKeyDelegate _updateClientKey;
        private readonly UpdateServerKeyDelegate _updateServerKey;

        private readonly SSH1Connection _connection;
        private readonly SSH1SynchronousPacketHandler _syncHandler;
        private readonly SSHConnectionParameter _param;
        private readonly SSH1ConnectionInfo _cInfo;

        private readonly object _sequenceLock = new object();
        private volatile SequenceStatus _sequenceStatus = SequenceStatus.Idle;

        private readonly AtomicBox<DataFragment> _receivedPacket = new AtomicBox<DataFragment>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="syncHandler"></param>
        /// <param name="param"></param>
        /// <param name="info"></param>
        /// <param name="updateClientKey"></param>
        /// <param name="updateServerKey"></param>
        public SSH1KeyExchanger(
                    SSH1Connection connection,
                    SSH1SynchronousPacketHandler syncHandler,
                    SSHConnectionParameter param,
                    SSH1ConnectionInfo info,
                    UpdateClientKeyDelegate updateClientKey,
                    UpdateServerKeyDelegate updateServerKey) {
            _connection = connection;
            _syncHandler = syncHandler;
            _param = param;
            _cInfo = info;
            _updateClientKey = updateClientKey;
            _updateServerKey = updateServerKey;
        }

        /// <summary>
        /// Intercept a received packet.
        /// </summary>
        /// <param name="packet">a packet image</param>
        /// <returns>result</returns>
        public SSHPacketInterceptorResult InterceptPacket(DataFragment packet) {
            if (_sequenceStatus == SequenceStatus.Succeeded || _sequenceStatus == SequenceStatus.Failed) {
                return SSHPacketInterceptorResult.Finished;
            }

            SSH1PacketType packetType = (SSH1PacketType)packet[0];
            lock (_sequenceLock) {
                switch (_sequenceStatus) {
                    case SequenceStatus.WaitPublicKey:
                        if (packetType == SSH1PacketType.SSH_SMSG_PUBLIC_KEY) {
                            _sequenceStatus = SequenceStatus.PublicKeyReceived;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        break;
                    case SequenceStatus.WaitSessionKeyResult:
                        if (packetType == SSH1PacketType.SSH_SMSG_SUCCESS) {
                            _sequenceStatus = SequenceStatus.Succeeded;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        else if (packetType == SSH1PacketType.SSH_SMSG_FAILURE) {
                            _sequenceStatus = SequenceStatus.Failed;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        break;
                    default:
                        break;
                }
                return SSHPacketInterceptorResult.PassThrough;
            }
        }

        /// <summary>
        /// Handles connection close.
        /// </summary>
        public void OnConnectionClosed() {
            lock (_sequenceLock) {
                if (_sequenceStatus != SequenceStatus.ConnectionClosed) {
                    _sequenceStatus = SequenceStatus.ConnectionClosed;
                    DataFragment dummyPacket = new DataFragment(new byte[1] { 0xff }, 0, 1);
                    _receivedPacket.TrySet(dummyPacket, PASSING_TIMEOUT);
                }
            }
        }

        /// <summary>
        /// Executes key exchange.
        /// </summary>
        /// <remarks>
        /// if an error has been occurred during the key-exchange, an exception will be thrown.
        /// </remarks>
        public void ExecKeyExchange() {
            lock (_sequenceLock) {
                if (_sequenceStatus != SequenceStatus.Idle) {
                    throw new InvalidOperationException(Strings.GetString("RequestedTaskIsAlreadyRunning"));
                }
                _sequenceStatus = SequenceStatus.WaitPublicKey;
            }
            DoKeyExchange();
        }

        /// <summary>
        /// Key exchange sequence.
        /// </summary>
        /// <exception cref="SSHException">no response</exception>
        private void DoKeyExchange() {
            try {
                ReceiveServerKey();

                if (_param.VerifySSHHostKey != null) {
                    bool accepted = _param.VerifySSHHostKey(_cInfo.GetSSHHostKeyInformationProvider());
                    if (!accepted) {
                        throw new SSHException(Strings.GetString("HostKeyDenied"));
                    }
                }

                byte[] sessionId = ComputeSessionId();
                byte[] sessionKey = new byte[32];
                RngManager.GetSecureRng().GetBytes(sessionKey);

                SendSessionKey(sessionId, sessionKey);
            }
            catch (Exception) {
                lock (_sequenceLock) {
                    _sequenceStatus = SequenceStatus.Failed;
                    Monitor.PulseAll(_sequenceLock);
                }
                throw;
            }
            finally {
                _receivedPacket.Clear();
            }
        }

        /// <summary>
        /// Waits SSH_SMSG_PUBLIC_KEY from server, then parse it.
        /// </summary>
        private void ReceiveServerKey() {
            lock (_sequenceLock) {
                CheckConnectionClosed();
                Debug.Assert(_sequenceStatus == SequenceStatus.WaitPublicKey || _sequenceStatus == SequenceStatus.PublicKeyReceived);
            }

            DataFragment packet = null;
            if (!_receivedPacket.TryGet(ref packet, RESPONSE_TIMEOUT)) {
                throw new SSHException(Strings.GetString("ServerDoesntRespond"));
            }

            lock (_sequenceLock) {
                CheckConnectionClosed();
                Debug.Assert(_sequenceStatus == SequenceStatus.PublicKeyReceived);
            }

            SSH1DataReader reader = new SSH1DataReader(packet);
            SSH1PacketType packetType = (SSH1PacketType)reader.ReadByte();
            Debug.Assert(packetType == SSH1PacketType.SSH_SMSG_PUBLIC_KEY);
            _cInfo.AntiSpoofingCookie = reader.Read(8);
            _cInfo.ServerKeyBits = reader.ReadInt32();
            BigInteger serverKeyExponent = reader.ReadMPInt();
            BigInteger serverKeyModulus = reader.ReadMPInt();
            _cInfo.ServerKey = new RSAPublicKey(serverKeyExponent, serverKeyModulus);
            _cInfo.HostKeyBits = reader.ReadInt32();
            BigInteger hostKeyExponent = reader.ReadMPInt();
            BigInteger hostKeyModulus = reader.ReadMPInt();
            _cInfo.HostKey = new RSAPublicKey(hostKeyExponent, hostKeyModulus);
            _cInfo.ServerProtocolFlags = (SSH1ProtocolFlags)reader.ReadUInt32();
            int supportedCiphersMask = reader.ReadInt32();
            _cInfo.SupportedEncryptionAlgorithmsMask = supportedCiphersMask;
            int supportedAuthenticationsMask = reader.ReadInt32();

            bool foundCipher = false;
            foreach (CipherAlgorithm algorithm in _param.PreferableCipherAlgorithms) {
                if ((algorithm == CipherAlgorithm.Blowfish || algorithm == CipherAlgorithm.TripleDES)
                    && ((supportedCiphersMask & (1 << (int)algorithm)) != 0)) {

                    _cInfo.IncomingPacketCipher = _cInfo.OutgoingPacketCipher = algorithm;
                    foundCipher = true;
                    break;
                }
            }
            if (!foundCipher) {
                throw new SSHException(String.Format(Strings.GetString("ServerNotSupportedX"), "Blowfish/TripleDES"));
            }

            switch (_param.AuthenticationType) {
                case AuthenticationType.Password:
                    if ((supportedAuthenticationsMask & (1 << (int)AuthenticationType.Password)) == 0) {
                        throw new SSHException(String.Format(Strings.GetString("ServerNotSupportedPassword")));
                    }
                    break;
                case AuthenticationType.PublicKey:
                    if ((supportedAuthenticationsMask & (1 << (int)AuthenticationType.PublicKey)) == 0) {
                        throw new SSHException(String.Format(Strings.GetString("ServerNotSupportedRSA")));
                    }
                    break;
                default:
                    throw new SSHException(Strings.GetString("InvalidAuthenticationType"));
            }
        }

        /// <summary>
        /// Computes session id.
        /// </summary>
        /// <returns>session id</returns>
        private byte[] ComputeSessionId() {
            byte[] hostKeyMod = _cInfo.HostKey.Modulus.GetBytes();
            byte[] serverKeyMod = _cInfo.ServerKey.Modulus.GetBytes();

            using (var md5 = new MD5CryptoServiceProvider()) {
                md5.TransformBlock(hostKeyMod, 0, hostKeyMod.Length, hostKeyMod, 0);
                md5.TransformBlock(serverKeyMod, 0, serverKeyMod.Length, serverKeyMod, 0);
                md5.TransformFinalBlock(_cInfo.AntiSpoofingCookie, 0, _cInfo.AntiSpoofingCookie.Length);
                return md5.Hash;
            }
        }

        /// <summary>
        /// Builds SSH_CMSG_SESSION_KEY packet.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="sessionKey"></param>
        /// <returns>a packet object</returns>
        private SSH1Packet BuildSessionKeyPacket(byte[] sessionId, byte[] sessionKey) {
            byte[] sessionKeyXor = (byte[])sessionKey.Clone();
            // xor first 16 bytes
            for (int i = 0; i < sessionId.Length; i++) {
                sessionKeyXor[i] ^= sessionId[i];
            }

            RSAPublicKey firstEncryption;
            RSAPublicKey secondEncryption;
            int firstKeyByteLen;
            int secondKeyByteLen;
            RSAPublicKey serverKey = _cInfo.ServerKey;
            RSAPublicKey hostKey = _cInfo.HostKey;
            if (serverKey.Modulus < hostKey.Modulus) {
                firstEncryption = serverKey;
                secondEncryption = hostKey;
                firstKeyByteLen = (_cInfo.ServerKeyBits + 7) / 8;
                secondKeyByteLen = (_cInfo.HostKeyBits + 7) / 8;
            }
            else {
                firstEncryption = hostKey;
                secondEncryption = serverKey;
                firstKeyByteLen = (_cInfo.HostKeyBits + 7) / 8;
                secondKeyByteLen = (_cInfo.ServerKeyBits + 7) / 8;
            }

            Rng rng = RngManager.GetSecureRng();
            BigInteger firstResult = RSAUtil.PKCS1PadType2(sessionKeyXor, firstKeyByteLen, rng).ModPow(firstEncryption.Exponent, firstEncryption.Modulus);
            BigInteger secondResult = RSAUtil.PKCS1PadType2(firstResult.GetBytes(), secondKeyByteLen, rng).ModPow(secondEncryption.Exponent, secondEncryption.Modulus);

            return new SSH1Packet(SSH1PacketType.SSH_CMSG_SESSION_KEY)
                    .WriteByte((byte)_cInfo.OutgoingPacketCipher.Value)
                    .Write(_cInfo.AntiSpoofingCookie)
                    .WriteBigInteger(secondResult)
                    .WriteUInt32((uint)_cInfo.ClientProtocolFlags);
        }

        /// <summary>
        /// Sends SSH_CMSG_SESSION_KEY packet and waits SSH_SMSG_SUCCESS.
        /// </summary>
        /// <param name="sessionId">session id</param>
        /// <param name="sessionKey">session key</param>
        public void SendSessionKey(byte[] sessionId, byte[] sessionKey) {
            lock (_sequenceLock) {
                CheckConnectionClosed();
                Debug.Assert(_sequenceStatus == SequenceStatus.PublicKeyReceived);
            }

            Cipher cipherServer = CipherFactory.CreateCipher(SSHProtocol.SSH1, _cInfo.OutgoingPacketCipher.Value, sessionKey);
            Cipher cipherClient = CipherFactory.CreateCipher(SSHProtocol.SSH1, _cInfo.IncomingPacketCipher.Value, sessionKey);

            _updateClientKey(cipherClient); // prepare decryption of the response

            lock (_sequenceLock) {
                CheckConnectionClosed();
                _sequenceStatus = SequenceStatus.WaitSessionKeyResult;
            }

            var packet = BuildSessionKeyPacket(sessionId, sessionKey);
            _syncHandler.Send(packet);

            _updateServerKey(sessionId, cipherServer);  // prepare encryption for the trailing packets

            DataFragment response = null;
            if (!_receivedPacket.TryGet(ref response, RESPONSE_TIMEOUT)) {
                throw new SSHException(Strings.GetString("ServerDoesntRespond"));
            }

            lock (_sequenceLock) {
                CheckConnectionClosed();
                if (_sequenceStatus != SequenceStatus.Succeeded) {
                    throw new SSHException(Strings.GetString("EncryptionAlgorithmNegotiationFailed"));
                }
            }
        }

        /// <summary>
        /// Check ConnectionClosed.
        /// </summary>
        private void CheckConnectionClosed() {
            lock (_sequenceLock) {
                if (_sequenceStatus == SequenceStatus.ConnectionClosed) {
                    throw new SSHException(Strings.GetString("ConnectionClosed"));
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Class for supporting user authentication
    /// </summary>
    internal class SSH1UserAuthentication : ISSHPacketInterceptor {
        #region

        private const int PASSING_TIMEOUT = 1000;
        private const int RESPONSE_TIMEOUT = 5000;

        private readonly SSHConnectionParameter _param;
        private readonly SSH1Connection _connection;
        private readonly SSH1SynchronousPacketHandler _syncHandler;
        private readonly byte[] _sessionID;

        private readonly object _sequenceLock = new object();
        private volatile SequenceStatus _sequenceStatus = SequenceStatus.Idle;

        private readonly AtomicBox<DataFragment> _receivedPacket = new AtomicBox<DataFragment>();

        private enum SequenceStatus {
            /// <summary>authentication can be started</summary>
            Idle,
            /// <summary>authentication has been finished.</summary>
            Done,
            /// <summary>the connection has been closed</summary>
            ConnectionClosed,
            /// <summary>authentication has been started</summary>
            StartAuthentication,
            /// <summary>SSH_CMSG_USER has been sent. waiting for SSH_SMSG_SUCCESS|SSH_SMSG_FAILURE.</summary>
            User_WaitResult,
            /// <summary>SSH_SMSG_SUCCESS has been received</summary>
            User_SuccessReceived,
            /// <summary>SSH_SMSG_FAILURE has been received</summary>
            User_FailureReceived,

            //--- RSA challenge-response

            /// <summary>SSH_CMSG_AUTH_RSA has been sent. waiting for SSH_SMSG_AUTH_RSA_CHALLENGE|SSH_SMSG_FAILURE</summary>
            RSA_WaitChallenge,
            /// <summary>SSH_SMSG_AUTH_RSA_CHALLENGE has been received</summary>
            RSA_ChallengeReceived,
            /// <summary>SSH_CMSG_AUTH_RSA_RESPONSE has been sent. waiting for SSH_SMSG_SUCCESS|SSH_SMSG_FAILURE</summary>
            RSA_WaitResponseResult,
            /// <summary>SSH_SMSG_SUCCESS has been received</summary>
            RSA_SuccessReceived,
            /// <summary>SSH_SMSG_FAILURE has been received</summary>
            RSA_FailureReceived,

            //--- password authentication

            /// <summary>SSH_CMSG_AUTH_PASSWORD has been sent. waiting for SSH_SMSG_SUCCESS|SSH_SMSG_FAILURE</summary>
            PA_WaitResult,
            /// <summary>SSH_MSG_USERAUTH_SUCCESS has been received</summary>
            PA_SuccessReceived,
            /// <summary>SSH_MSG_USERAUTH_FAILURE has been received</summary>
            PA_FailureReceived,
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="param"></param>
        /// <param name="syncHandler"></param>
        /// <param name="sessionID"></param>
        public SSH1UserAuthentication(
                    SSH1Connection connection,
                    SSHConnectionParameter param,
                    SSH1SynchronousPacketHandler syncHandler,
                    byte[] sessionID) {
            _connection = connection;
            _param = param;
            _syncHandler = syncHandler;
            _sessionID = sessionID;
        }

        /// <summary>
        /// Intercept a received packet.
        /// </summary>
        /// <param name="packet">a packet image</param>
        /// <returns>result</returns>
        public SSHPacketInterceptorResult InterceptPacket(DataFragment packet) {
            if (_sequenceStatus == SequenceStatus.Done) {   // fast check
                return SSHPacketInterceptorResult.Finished;
            }

            SSH1PacketType packetType = (SSH1PacketType)packet[0];
            lock (_sequenceLock) {
                switch (_sequenceStatus) {
                    case SequenceStatus.User_WaitResult:
                        if (packetType == SSH1PacketType.SSH_SMSG_SUCCESS) {
                            _sequenceStatus = SequenceStatus.User_SuccessReceived;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        if (packetType == SSH1PacketType.SSH_SMSG_FAILURE) {
                            _sequenceStatus = SequenceStatus.User_FailureReceived;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        break;

                    // RSA challenge-response

                    case SequenceStatus.RSA_WaitChallenge:
                        if (packetType == SSH1PacketType.SSH_SMSG_AUTH_RSA_CHALLENGE) {
                            _sequenceStatus = SequenceStatus.RSA_ChallengeReceived;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        if (packetType == SSH1PacketType.SSH_SMSG_FAILURE) {
                            _sequenceStatus = SequenceStatus.RSA_FailureReceived;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        break;
                    case SequenceStatus.RSA_WaitResponseResult:
                        if (packetType == SSH1PacketType.SSH_SMSG_SUCCESS) {
                            _sequenceStatus = SequenceStatus.RSA_SuccessReceived;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        if (packetType == SSH1PacketType.SSH_SMSG_FAILURE) {
                            _sequenceStatus = SequenceStatus.RSA_FailureReceived;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        break;

                    // Password authentication

                    case SequenceStatus.PA_WaitResult:
                        if (packetType == SSH1PacketType.SSH_SMSG_SUCCESS) {
                            _sequenceStatus = SequenceStatus.PA_SuccessReceived;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        if (packetType == SSH1PacketType.SSH_SMSG_FAILURE) {
                            _sequenceStatus = SequenceStatus.PA_FailureReceived;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        break;

                    default:
                        break;
                }
                return SSHPacketInterceptorResult.PassThrough;
            }
        }

        /// <summary>
        /// Handles connection close.
        /// </summary>
        public void OnConnectionClosed() {
            lock (_sequenceLock) {
                if (_sequenceStatus != SequenceStatus.ConnectionClosed) {
                    _sequenceStatus = SequenceStatus.ConnectionClosed;
                    DataFragment dummyPacket = new DataFragment(new byte[1] { 0xff }, 0, 1);
                    _receivedPacket.TrySet(dummyPacket, PASSING_TIMEOUT);
                }
            }
        }

        /// <summary>
        /// Executes authentication.
        /// </summary>
        /// <remarks>
        /// if an error has been occurred during the authentication, an exception will be thrown.
        /// </remarks>
        public void ExecAuthentication() {
            lock (_sequenceLock) {
                if (_sequenceStatus != SequenceStatus.Idle) {
                    throw new InvalidOperationException(Strings.GetString("RequestedTaskIsAlreadyRunning"));
                }
                _sequenceStatus = SequenceStatus.StartAuthentication;
            }
            DoAuthentication();
        }

        /// <summary>
        /// Authentication sequence.
        /// </summary>
        /// <returns>true if the sequence was succeeded</returns>
        /// <exception cref="SSHException">no response</exception>
        private void DoAuthentication() {
            try {
                bool success = DeclareUser();

                if (success) {
                    return;
                }

                switch (_param.AuthenticationType) {
                    case AuthenticationType.Password:
                        PasswordAuthentication();
                        break;
                    case AuthenticationType.PublicKey:
                        PublickeyAuthentication();
                        break;
                    default:
                        throw new SSHException(Strings.GetString("InvalidAuthenticationType"));
                }
            }
            finally {
                _receivedPacket.Clear();
                lock (_sequenceLock) {
                    _sequenceStatus = SequenceStatus.Done;
                }
            }
        }

        /// <summary>
        /// Builds SSH_CMSG_USER packet.
        /// </summary>
        /// <returns>a packet object</returns>
        private SSH1Packet BuildUserPacket() {
            return new SSH1Packet(SSH1PacketType.SSH_CMSG_USER)
                    .WriteString(_param.UserName);
        }

        /// <summary>
        /// Declaring-User sequence.
        /// </summary>
        /// <returns>result of declaring user.</returns>
        private bool DeclareUser() {
            lock (_sequenceLock) {
                CheckConnectionClosed();
                Debug.Assert(_sequenceStatus == SequenceStatus.StartAuthentication);
                _sequenceStatus = SequenceStatus.User_WaitResult;
            }

            var packet = BuildUserPacket();
            _syncHandler.Send(packet);

            DataFragment response = null;
            if (!_receivedPacket.TryGet(ref response, RESPONSE_TIMEOUT)) {
                throw new SSHException(Strings.GetString("ServerDoesntRespond"));
            }

            lock (_sequenceLock) {
                CheckConnectionClosed();

                if (_sequenceStatus == SequenceStatus.User_SuccessReceived) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Builds SSH_CMSG_AUTH_PASSWORD packet.
        /// </summary>
        /// <returns>a packet object</returns>
        private SSH1Packet BuildAuthPasswordPacket() {
            return new SSH1Packet(SSH1PacketType.SSH_CMSG_AUTH_PASSWORD)
                        .WriteString(_param.Password);
        }

        /// <summary>
        /// Password authentication sequence.
        /// </summary>
        private void PasswordAuthentication() {
            lock (_sequenceLock) {
                CheckConnectionClosed();
                Debug.Assert(_sequenceStatus == SequenceStatus.User_FailureReceived);
                _sequenceStatus = SequenceStatus.PA_WaitResult;
            }

            var packet = BuildAuthPasswordPacket();
            _syncHandler.Send(packet);

            DataFragment response = null;
            if (!_receivedPacket.TryGet(ref response, RESPONSE_TIMEOUT)) {
                throw new SSHException(Strings.GetString("ServerDoesntRespond"));
            }

            lock (_sequenceLock) {
                CheckConnectionClosed();
                if (_sequenceStatus != SequenceStatus.PA_SuccessReceived) {
                    throw new SSHException(Strings.GetString("AuthenticationFailed"));
                }
            }
        }

        /// <summary>
        /// Builds SSH_CMSG_AUTH_RSA packet.
        /// </summary>
        /// <param name="key">private key data</param>
        /// <returns>a packet object</returns>
        private SSH1Packet BuildAuthRSAPacket(SSH1UserAuthKey key) {
            return new SSH1Packet(SSH1PacketType.SSH_CMSG_AUTH_RSA)
                    .WriteBigInteger(key.PublicModulus);
        }

        /// <summary>
        /// Builds SSH_CMSG_AUTH_RSA_RESPONSE packet.
        /// </summary>
        /// <param name="hash">hash data</param>
        /// <returns>a packet object</returns>
        private SSH1Packet BuildRSAResponsePacket(byte[] hash) {
            return new SSH1Packet(SSH1PacketType.SSH_CMSG_AUTH_RSA_RESPONSE)
                    .Write(hash);
        }

        /// <summary>
        /// RSA Publickey authentication sequence.
        /// </summary>
        private void PublickeyAuthentication() {
            SSH1UserAuthKey key = new SSH1UserAuthKey(_param.IdentityFile, _param.Password);

            lock (_sequenceLock) {
                CheckConnectionClosed();
                Debug.Assert(_sequenceStatus == SequenceStatus.User_FailureReceived);
                _sequenceStatus = SequenceStatus.RSA_WaitChallenge;
            }

            var packetRsa = BuildAuthRSAPacket(key);
            _syncHandler.Send(packetRsa);

            DataFragment response = null;
            if (!_receivedPacket.TryGet(ref response, RESPONSE_TIMEOUT)) {
                throw new SSHException(Strings.GetString("ServerDoesntRespond"));
            }

            lock (_sequenceLock) {
                CheckConnectionClosed();
                if (_sequenceStatus == SequenceStatus.RSA_FailureReceived) {
                    throw new SSHException(Strings.GetString("AuthenticationFailed"));
                }
                Debug.Assert(_sequenceStatus == SequenceStatus.RSA_ChallengeReceived);
            }

            SSH1DataReader challengeReader = new SSH1DataReader(response);
            challengeReader.ReadByte(); // skip message number
            BigInteger encryptedChallenge = challengeReader.ReadMPInt();
            BigInteger challenge = key.decryptChallenge(encryptedChallenge);
            byte[] rawchallenge = RSAUtil.StripPKCS1Pad(challenge, 2).GetBytes();

            byte[] hash;
            using (var md5 = new MD5CryptoServiceProvider()) {
                md5.TransformBlock(rawchallenge, 0, rawchallenge.Length, rawchallenge, 0);
                md5.TransformFinalBlock(_sessionID, 0, _sessionID.Length);
                hash = md5.Hash;
            }

            lock (_sequenceLock) {
                CheckConnectionClosed();
                _sequenceStatus = SequenceStatus.RSA_WaitResponseResult;
            }

            var packetRes = BuildRSAResponsePacket(hash);
            _syncHandler.Send(packetRes);

            response = null;
            if (!_receivedPacket.TryGet(ref response, RESPONSE_TIMEOUT)) {
                throw new SSHException(Strings.GetString("ServerDoesntRespond"));
            }

            lock (_sequenceLock) {
                CheckConnectionClosed();
                if (_sequenceStatus != SequenceStatus.RSA_SuccessReceived) {
                    throw new SSHException(Strings.GetString("AuthenticationFailed"));
                }
                Debug.Assert(_sequenceStatus == SequenceStatus.RSA_SuccessReceived);
            }
        }

        /// <summary>
        /// Check ConnectionClosed.
        /// </summary>
        private void CheckConnectionClosed() {
            lock (_sequenceLock) {
                if (_sequenceStatus == SequenceStatus.ConnectionClosed) {
                    throw new SSHException(Strings.GetString("ConnectionClosed"));
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Class for supporting remote port-forwarding
    /// </summary>
    internal class SSH1RemotePortForwarding : ISSHPacketInterceptor {
        #region

        private const int PASSING_TIMEOUT = 1000;
        private const int RESPONSE_TIMEOUT = 5000;

        public delegate SSH1RemotePortForwardingChannel CreateChannelFunc(RemotePortForwardingRequest requestInfo, uint remoteChannel);
        public delegate void RegisterChannelFunc(SSH1RemotePortForwardingChannel channel, ISSHChannelEventHandler eventHandler);

        private readonly SSH1SynchronousPacketHandler _syncHandler;

        private readonly Action _startInteractiveSession;

        private class PortDictKey {
            private readonly string _host;
            private readonly uint _port;

            public PortDictKey(string host, uint port) {
                _host = host;
                _port = port;
            }

            public override bool Equals(object obj) {
                var other = obj as PortDictKey;
                if (other == null) {
                    return false;
                }
                return this._host == other._host && this._port == other._port;
            }

            public override int GetHashCode() {
                return _host.GetHashCode() + _port.GetHashCode();
            }
        }

        private class PortInfo {
            public readonly IRemotePortForwardingHandler RequestHandler;
            public readonly CreateChannelFunc CreateChannel;
            public readonly RegisterChannelFunc RegisterChannel;

            public PortInfo(IRemotePortForwardingHandler requestHandler, CreateChannelFunc createChannel, RegisterChannelFunc registerChannel) {
                this.RequestHandler = requestHandler;
                this.CreateChannel = createChannel;
                this.RegisterChannel = registerChannel;
            }
        }
        private readonly ConcurrentDictionary<PortDictKey, PortInfo> _portDict = new ConcurrentDictionary<PortDictKey, PortInfo>();

        private readonly object _sequenceLock = new object();
        private volatile SequenceStatus _sequenceStatus = SequenceStatus.Idle;

        private readonly AtomicBox<DataFragment> _receivedPacket = new AtomicBox<DataFragment>();

        private enum SequenceStatus {
            /// <summary>Idle</summary>
            Idle,
            /// <summary>the connection has been closed</summary>
            ConnectionClosed,
            /// <summary>SSH_CMSG_PORT_FORWARD_REQUEST has been sent. waiting for SSH_SMSG_SUCCESS | SSH_SMSG_FAILURE.</summary>
            WaitPortForwardResponse,
            /// <summary>SSH_SMSG_SUCCESS has been received.</summary>
            PortForwardSuccess,
            /// <summary>SSH_SMSG_FAILURE has been received.</summary>
            PortForwardFailure,
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SSH1RemotePortForwarding(SSH1SynchronousPacketHandler syncHandler, Action startInteractiveSession) {
            _syncHandler = syncHandler;
            _startInteractiveSession = startInteractiveSession;
        }

        /// <summary>
        /// Intercept a received packet.
        /// </summary>
        /// <param name="packet">a packet image</param>
        /// <returns>result</returns>
        public SSHPacketInterceptorResult InterceptPacket(DataFragment packet) {
            SSH1PacketType packetType = (SSH1PacketType)packet[0];
            SSHPacketInterceptorResult result = CheckPortOpenRequestPacket(packetType, packet);
            if (result != SSHPacketInterceptorResult.PassThrough) {
                return result;
            }

            lock (_sequenceLock) {
                switch (_sequenceStatus) {
                    case SequenceStatus.WaitPortForwardResponse:
                        if (packetType == SSH1PacketType.SSH_SMSG_SUCCESS) {
                            _sequenceStatus = SequenceStatus.PortForwardSuccess;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        if (packetType == SSH1PacketType.SSH_SMSG_FAILURE) {
                            _sequenceStatus = SequenceStatus.PortForwardFailure;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        break;
                }

                return SSHPacketInterceptorResult.PassThrough;
            }
        }

        /// <summary>
        /// Handles new request.
        /// </summary>
        /// <param name="packetType">packet type</param>
        /// <param name="packet">packet data</param>
        /// <returns>result</returns>
        private SSHPacketInterceptorResult CheckPortOpenRequestPacket(SSH1PacketType packetType, DataFragment packet) {
            if (packetType != SSH1PacketType.SSH_MSG_PORT_OPEN) {
                return SSHPacketInterceptorResult.PassThrough;
            }

            SSH1DataReader reader = new SSH1DataReader(packet);
            reader.ReadByte();    // skip message number
            uint remoteChannel = reader.ReadUInt32();
            string host = reader.ReadString();
            uint port = reader.ReadUInt32();

            // reject the request if we don't know the host / port pair.
            PortDictKey key = new PortDictKey(host, port);
            PortInfo portInfo;
            if (!_portDict.TryGetValue(key, out portInfo)) {
                RejectPortForward(remoteChannel);
                return SSHPacketInterceptorResult.Consumed;
            }

            RemotePortForwardingRequest requestInfo = new RemotePortForwardingRequest("", 0, "", 0);

            // create a temporary channel
            var channel = portInfo.CreateChannel(requestInfo, remoteChannel);

            // check the request by the request handler
            RemotePortForwardingReply reply;
            try {
                reply = portInfo.RequestHandler.OnRemotePortForwardingRequest(requestInfo, channel);
            }
            catch (Exception) {
                RejectPortForward(remoteChannel);
                return SSHPacketInterceptorResult.Consumed;
            }

            if (!reply.Accepted) {
                RejectPortForward(remoteChannel);
                return SSHPacketInterceptorResult.Consumed;
            }

            // register a channel to the connection object
            portInfo.RegisterChannel(channel, reply.EventHandler);

            // send SSH_MSG_CHANNEL_OPEN_CONFIRMATION
            channel.SendOpenConfirmation();

            return SSHPacketInterceptorResult.Consumed;
        }

        /// <summary>
        /// Handles connection close.
        /// </summary>
        public void OnConnectionClosed() {
            lock (_sequenceLock) {
                if (_sequenceStatus != SequenceStatus.ConnectionClosed) {
                    _sequenceStatus = SequenceStatus.ConnectionClosed;
                    DataFragment dummyPacket = new DataFragment(new byte[1] { 0xff }, 0, 1);
                    _receivedPacket.TrySet(dummyPacket, PASSING_TIMEOUT);
                    Monitor.PulseAll(_sequenceLock);
                }
            }
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_OPEN_FAILURE for rejecting the request.
        /// </summary>
        /// <param name="remoteChannel">remote channel number</param>
        private void RejectPortForward(uint remoteChannel) {
            var packet = new SSH1Packet(SSH1PacketType.SSH_MSG_CHANNEL_OPEN_FAILURE);
            _syncHandler.Send(packet);
        }

        /// <summary>
        /// Builds SSH_CMSG_PORT_FORWARD_REQUEST packet.
        /// </summary>
        /// <param name="portNumberToBind">port number to bind on the server</param>
        /// <param name="hostToConnect">host the connection should be be forwarded to</param>
        /// <param name="portNumberToConnect">port the connection should be be forwarded to</param>
        /// <returns></returns>
        private SSH1Packet BuildPortForwardPacket(uint portNumberToBind, string hostToConnect, uint portNumberToConnect) {
            return new SSH1Packet(SSH1PacketType.SSH_CMSG_PORT_FORWARD_REQUEST)
                    .WriteUInt32(portNumberToBind)
                    .WriteString(hostToConnect)
                    .WriteUInt32(portNumberToConnect);
        }

        /// <summary>
        /// Starts remote port forwarding.
        /// </summary>
        /// <param name="requestHandler">request handler</param>
        /// <param name="createChannel">a function for creating a new channel object</param>
        /// <param name="registerChannel">a function for registering a new channel object</param>
        /// <param name="portNumberToBind">port number to bind on the server</param>
        /// <param name="hostToConnect">host the connection should be be forwarded to</param>
        /// <param name="portNumberToConnect">port the connection should be be forwarded to</param>
        /// <returns>true if the remote port forwarding has been started.</returns>
        public bool ListenForwardedPort(
                IRemotePortForwardingHandler requestHandler,
                CreateChannelFunc createChannel,
                RegisterChannelFunc registerChannel,
                uint portNumberToBind,
                string hostToConnect,
                uint portNumberToConnect) {

            IRemotePortForwardingHandler requestHandlerWrapper =
                new RemotePortForwardingHandlerIgnoreErrorWrapper(requestHandler);

            bool success = ListenForwardedPortCore(
                                requestHandlerWrapper,
                                createChannel,
                                registerChannel,
                                portNumberToBind,
                                hostToConnect,
                                portNumberToConnect);

            if (success) {
                _startInteractiveSession();
                requestHandlerWrapper.OnRemotePortForwardingStarted(portNumberToBind);
            }
            else {
                requestHandlerWrapper.OnRemotePortForwardingFailed();
            }

            return success;
        }

        private bool ListenForwardedPortCore(
                IRemotePortForwardingHandler requestHandler,
                CreateChannelFunc createChannel,
                RegisterChannelFunc registerChannel,
                uint portNumberToBind,
                string hostToConnect,
                uint portNumberToConnect) {

            lock (_sequenceLock) {
                while (_sequenceStatus != SequenceStatus.Idle) {
                    if (_sequenceStatus == SequenceStatus.ConnectionClosed) {
                        return false;
                    }
                    Monitor.Wait(_sequenceLock);
                }

                _receivedPacket.Clear();
                _sequenceStatus = SequenceStatus.WaitPortForwardResponse;
            }

            var packet = BuildPortForwardPacket(portNumberToBind, hostToConnect, portNumberToConnect);
            _syncHandler.Send(packet);

            DataFragment response = null;
            bool accepted = false;
            if (_receivedPacket.TryGet(ref response, RESPONSE_TIMEOUT)) {
                lock (_sequenceLock) {
                    if (_sequenceStatus == SequenceStatus.PortForwardSuccess) {
                        accepted = true;
                        PortDictKey key = new PortDictKey(hostToConnect, portNumberToConnect);
                        _portDict.TryAdd(
                            key,
                            new PortInfo(requestHandler, createChannel, registerChannel));
                    }
                }
            }

            lock (_sequenceLock) {
                // reset status
                _sequenceStatus = SequenceStatus.Idle;
                Monitor.PulseAll(_sequenceLock);
            }

            return accepted;
        }

        #endregion
    }

    /// <summary>
    /// Class for supporting authentication agent forwarding
    /// </summary>
    internal class SSH1AgentForwarding : ISSHPacketInterceptor {
        #region

        private const int PASSING_TIMEOUT = 1000;
        private const int RESPONSE_TIMEOUT = 5000;

        public delegate SSH1AgentForwardingChannel CreateChannelFunc(uint remoteChannel);
        public delegate void RegisterChannelFunc(SSH1AgentForwardingChannel channel, ISSHChannelEventHandler eventHandler);

        private readonly SSH1SynchronousPacketHandler _syncHandler;
        private readonly CreateChannelFunc _createChannel;
        private readonly RegisterChannelFunc _registerChannel;
        private readonly IAgentForwardingAuthKeyProvider _authKeyProvider;

        private readonly object _sequenceLock = new object();
        private volatile SequenceStatus _sequenceStatus = SequenceStatus.Idle;

        private readonly AtomicBox<DataFragment> _receivedPacket = new AtomicBox<DataFragment>();

        private enum SequenceStatus {
            /// <summary>Idle</summary>
            Idle,
            /// <summary>the connection has been closed</summary>
            ConnectionClosed,
            /// <summary>SSH_CMSG_AGENT_REQUEST_FORWARDING has been sent. waiting for SSH_SMSG_SUCCESS | SSH_SMSG_FAILURE.</summary>
            WaitAgentForwardingResponse,
            /// <summary>SSH_SMSG_SUCCESS has been received.</summary>
            AgentForwardingSuccess,
            /// <summary>SSH_SMSG_FAILURE has been received.</summary>
            AgentForwardingFailure,
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SSH1AgentForwarding(
                SSH1SynchronousPacketHandler syncHandler,
                IAgentForwardingAuthKeyProvider authKeyProvider,
                CreateChannelFunc createChannel,
                RegisterChannelFunc registerChannel) {
            _syncHandler = syncHandler;
            _createChannel = createChannel;
            _registerChannel = registerChannel;
            _authKeyProvider = authKeyProvider;
        }

        /// <summary>
        /// Intercept a received packet.
        /// </summary>
        /// <param name="packet">a packet image</param>
        /// <returns>result</returns>
        public SSHPacketInterceptorResult InterceptPacket(DataFragment packet) {
            SSH1PacketType packetType = (SSH1PacketType)packet[0];
            SSHPacketInterceptorResult result = CheckAgentOpenRequestPacket(packetType, packet);
            if (result != SSHPacketInterceptorResult.PassThrough) {
                return result;
            }

            lock (_sequenceLock) {
                switch (_sequenceStatus) {
                    case SequenceStatus.WaitAgentForwardingResponse:
                        if (packetType == SSH1PacketType.SSH_SMSG_SUCCESS) {
                            _sequenceStatus = SequenceStatus.AgentForwardingSuccess;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        if (packetType == SSH1PacketType.SSH_SMSG_FAILURE) {
                            _sequenceStatus = SequenceStatus.AgentForwardingFailure;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        break;
                }

                return SSHPacketInterceptorResult.PassThrough;
            }
        }

        /// <summary>
        /// Handles new request.
        /// </summary>
        /// <param name="packetType">packet type</param>
        /// <param name="packet">packet data</param>
        /// <returns>result</returns>
        private SSHPacketInterceptorResult CheckAgentOpenRequestPacket(SSH1PacketType packetType, DataFragment packet) {
            if (packetType != SSH1PacketType.SSH_SMSG_AGENT_OPEN) {
                return SSHPacketInterceptorResult.PassThrough;
            }

            SSH1DataReader reader = new SSH1DataReader(packet);
            reader.ReadByte();    // skip message number
            uint remoteChannel = reader.ReadUInt32();

            if (_authKeyProvider == null || !_authKeyProvider.IsAuthKeyProviderEnabled) {
                RejectAgentForwarding(remoteChannel);
                return SSHPacketInterceptorResult.Consumed;
            }

            // create a channel
            var channel = _createChannel(remoteChannel);

            // create a handler
            var handler = new OpenSSHAgentForwardingMessageHandler(channel, _authKeyProvider);

            // register a channel to the connection object
            _registerChannel(channel, handler);

            // send SSH_MSG_CHANNEL_OPEN_CONFIRMATION
            channel.SendOpenConfirmation();

            return SSHPacketInterceptorResult.Consumed;
        }

        /// <summary>
        /// Handles connection close.
        /// </summary>
        public void OnConnectionClosed() {
            lock (_sequenceLock) {
                if (_sequenceStatus != SequenceStatus.ConnectionClosed) {
                    _sequenceStatus = SequenceStatus.ConnectionClosed;
                    DataFragment dummyPacket = new DataFragment(new byte[1] { 0xff }, 0, 1);
                    _receivedPacket.TrySet(dummyPacket, PASSING_TIMEOUT);
                    Monitor.PulseAll(_sequenceLock);
                }
            }
        }

        /// <summary>
        /// Builds SSH_CMSG_AGENT_REQUEST_FORWARDING packet.
        /// </summary>
        /// <returns></returns>
        private SSH1Packet BuildAgentRequestForwardingPacket() {
            return new SSH1Packet(SSH1PacketType.SSH_CMSG_AGENT_REQUEST_FORWARDING);
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_OPEN_FAILURE for rejecting the request.
        /// </summary>
        /// <param name="remoteChannel">remote channel number</param>
        private void RejectAgentForwarding(uint remoteChannel) {
            var packet = new SSH1Packet(SSH1PacketType.SSH_MSG_CHANNEL_OPEN_FAILURE);
            _syncHandler.Send(packet);
        }

        /// <summary>
        /// Starts agent forwarding.
        /// </summary>
        /// <returns>true if the agent forwarding has been started.</returns>
        public bool StartAgentForwarding() {
            lock (_sequenceLock) {
                while (_sequenceStatus != SequenceStatus.Idle) {
                    if (_sequenceStatus == SequenceStatus.ConnectionClosed) {
                        return false;
                    }
                    Monitor.Wait(_sequenceLock);
                }

                _receivedPacket.Clear();
                _sequenceStatus = SequenceStatus.WaitAgentForwardingResponse;
            }

            var packet = BuildAgentRequestForwardingPacket();
            _syncHandler.Send(packet);

            DataFragment response = null;
            bool accepted = false;
            if (_receivedPacket.TryGet(ref response, RESPONSE_TIMEOUT)) {
                lock (_sequenceLock) {
                    if (_sequenceStatus == SequenceStatus.AgentForwardingSuccess) {
                        accepted = true;
                    }
                }
            }

            lock (_sequenceLock) {
                // reset status
                _sequenceStatus = SequenceStatus.Idle;
                Monitor.PulseAll(_sequenceLock);
            }

            return accepted;
        }

        #endregion
    }

    /// <summary>
    /// Class for supporting X11 forwarding
    /// </summary>
    internal class SSH1X11Forwarding : ISSHPacketInterceptor {
        #region

        private const int PASSING_TIMEOUT = 1000;
        private const int RESPONSE_TIMEOUT = 5000;

        public delegate SSH1X11ForwardingChannel CreateChannelFunc(uint remoteChannel);
        public delegate void RegisterChannelFunc(SSH1X11ForwardingChannel channel, ISSHChannelEventHandler eventHandler);

        private readonly CreateChannelFunc _createChannel;
        private readonly RegisterChannelFunc _registerChannel;
        private readonly X11ConnectionManager _x11ConnectionManager;
        private readonly SSH1SynchronousPacketHandler _syncHandler;
        private readonly SSH1ConnectionInfo _connectionInfo;
        private readonly SSHProtocolEventManager _protocolEventManager;

        private readonly object _sequenceLock = new object();
        private volatile SequenceStatus _sequenceStatus = SequenceStatus.Idle;

        private readonly AtomicBox<DataFragment> _receivedPacket = new AtomicBox<DataFragment>();

        private enum SequenceStatus {
            /// <summary>Idle</summary>
            Idle,
            /// <summary>the connection has been closed</summary>
            ConnectionClosed,
            /// <summary>SSH_CMSG_X11_REQUEST_FORWARDING has been sent. waiting for SSH_SMSG_SUCCESS | SSH_SMSG_FAILURE.</summary>
            WaitX11ForwardResponse,
            /// <summary>SSH_SMSG_SUCCESS has been received.</summary>
            X11ForwardSuccess,
            /// <summary>SSH_SMSG_FAILURE has been received.</summary>
            X11ForwardFailure,
            /// <summary>SSH_SMSG_X11_OPEN can be accepted</summary>
            CanOpenX11Forward,
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SSH1X11Forwarding(
                    SSH1SynchronousPacketHandler syncHandler,
                    SSH1ConnectionInfo connectionInfo,
                    SSHProtocolEventManager protocolEventManager,
                    CreateChannelFunc createChannel,
                    RegisterChannelFunc registerChannel) {
            _syncHandler = syncHandler;
            _connectionInfo = connectionInfo;
            _x11ConnectionManager = new X11ConnectionManager(protocolEventManager);
            _createChannel = createChannel;
            _registerChannel = registerChannel;
            _protocolEventManager = protocolEventManager;
        }

        /// <summary>
        /// Intercept a received packet.
        /// </summary>
        /// <param name="packet">a packet image</param>
        /// <returns>result</returns>
        public SSHPacketInterceptorResult InterceptPacket(DataFragment packet) {
            SSH1PacketType packetType = (SSH1PacketType)packet[0];
            SSHPacketInterceptorResult result = CheckX11OpenRequestPacket(packetType, packet);
            if (result != SSHPacketInterceptorResult.PassThrough) {
                return result;
            }

            lock (_sequenceLock) {
                switch (_sequenceStatus) {
                    case SequenceStatus.WaitX11ForwardResponse:
                        if (packetType == SSH1PacketType.SSH_SMSG_SUCCESS) {
                            _sequenceStatus = SequenceStatus.X11ForwardSuccess;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        if (packetType == SSH1PacketType.SSH_SMSG_FAILURE) {
                            _sequenceStatus = SequenceStatus.X11ForwardFailure;
                            _receivedPacket.TrySet(packet, PASSING_TIMEOUT);
                            return SSHPacketInterceptorResult.Consumed;
                        }
                        break;
                }

                return SSHPacketInterceptorResult.PassThrough;
            }
        }

        /// <summary>
        /// Handles new request.
        /// </summary>
        /// <param name="packetType">packet type</param>
        /// <param name="packet">packet data</param>
        /// <returns>result</returns>
        private SSHPacketInterceptorResult CheckX11OpenRequestPacket(SSH1PacketType packetType, DataFragment packet) {
            lock (_sequenceLock) {
                if (_sequenceStatus != SequenceStatus.CanOpenX11Forward) {
                    return SSHPacketInterceptorResult.PassThrough;
                }
            }

            if (packetType != SSH1PacketType.SSH_SMSG_X11_OPEN) {
                return SSHPacketInterceptorResult.PassThrough;
            }

            SSH1DataReader reader = new SSH1DataReader(packet);
            reader.ReadByte();    // skip message number
            uint remoteChannel = reader.ReadUInt32();
            string originator;
            if (_connectionInfo.ServerProtocolFlags.HasFlag(SSH1ProtocolFlags.SSH_PROTOFLAG_HOST_IN_FWD_OPEN)
                && _connectionInfo.ClientProtocolFlags.HasFlag(SSH1ProtocolFlags.SSH_PROTOFLAG_HOST_IN_FWD_OPEN)
                && reader.RemainingDataLength >= 4) {

                originator = reader.ReadString();
            }
            else {
                originator = "";
            }

            // OpenSSH sends originator in the form of "X11 connection from {address} port {port}".
            // Only in the case that "{address} port {port}" part was found in the originator,
            // the address will be checked to determine whether the request can be accepted.

            var match = Regex.Match(originator, @"(\S+)\s+port\s+(\d+)");
            if (match.Success) {
                IPAddress addr;
                if (IPAddress.TryParse(match.Groups[1].Value, out addr)) {
                    if (!IPAddress.IsLoopback(addr)) {
                        _protocolEventManager.Trace("SSH_SMSG_X11_OPEN originator is \"{0}\" (rejected)", originator);
                        RejectX11Forward(remoteChannel);
                        return SSHPacketInterceptorResult.Consumed;
                    }
                }
            }

            _protocolEventManager.Trace("SSH_SMSG_X11_OPEN originator is \"{0}\" (accepted)", originator);

            // create a temporary channel
            var channel = _createChannel(remoteChannel);

            // get a channel handler
            var handler = _x11ConnectionManager.CreateChannelHandler(channel);
            if (handler == null) {
                // failed to connect to the X server
                _protocolEventManager.Trace("SSH_SMSG_X11_OPEN failed");
                RejectX11Forward(remoteChannel);
                return SSHPacketInterceptorResult.Consumed;
            }

            // register a channel to the connection object
            _registerChannel(channel, handler);

            // send SSH_MSG_CHANNEL_OPEN_CONFIRMATION
            channel.SendOpenConfirmation();

            return SSHPacketInterceptorResult.Consumed;
        }

        /// <summary>
        /// Handles connection close.
        /// </summary>
        public void OnConnectionClosed() {
            lock (_sequenceLock) {
                if (_sequenceStatus != SequenceStatus.ConnectionClosed) {
                    _sequenceStatus = SequenceStatus.ConnectionClosed;
                    DataFragment dummyPacket = new DataFragment(new byte[1] { 0xff }, 0, 1);
                    _receivedPacket.TrySet(dummyPacket, PASSING_TIMEOUT);
                    Monitor.PulseAll(_sequenceLock);
                }
            }
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_OPEN_FAILURE for rejecting the request.
        /// </summary>
        /// <param name="remoteChannel">remote channel number</param>
        private void RejectX11Forward(uint remoteChannel) {
            var packet = new SSH1Packet(SSH1PacketType.SSH_MSG_CHANNEL_OPEN_FAILURE);
            _syncHandler.Send(packet);
        }

        /// <summary>
        /// Builds SSH_CMSG_X11_REQUEST_FORWARDING packet.
        /// </summary>
        /// <param name="authProtocolName">authorization protocol name</param>
        /// <param name="authCookieHex">authorization cookie (hex string)</param>
        /// <param name="screen">screen number</param>
        /// <returns>packet object</returns>
        private SSH1Packet BuildX11ForwardPacket(string authProtocolName, string authCookieHex, int screen) {
            var packet = new SSH1Packet(SSH1PacketType.SSH_CMSG_X11_REQUEST_FORWARDING)
                    .WriteString(authProtocolName)
                    .WriteString(authCookieHex);
            if (_connectionInfo.ClientProtocolFlags.HasFlag(SSH1ProtocolFlags.SSH_PROTOFLAG_SCREEN_NUMBER)) {
                packet.WriteUInt32((uint)screen);
            }
            return packet;
        }

        /// <summary>
        /// Starts X11 forwarding.
        /// </summary>
        /// <param name="param">X11 forwarding parameters</param>
        /// <returns>true if the X11 forwarding has been started.</returns>
        /// <exception cref="X11UtilException"></exception>
        /// <exception cref="X11SocketException"></exception>
        public bool ListenX11Forwarding(X11ForwardingParams param) {
            lock (_sequenceLock) {
                while (_sequenceStatus != SequenceStatus.Idle) {
                    if (_sequenceStatus == SequenceStatus.ConnectionClosed) {
                        return false;
                    }
                    Monitor.Wait(_sequenceLock);
                }

                _receivedPacket.Clear();
                _sequenceStatus = SequenceStatus.WaitX11ForwardResponse;
            }

            if (!_x11ConnectionManager.SetupDone) {
                _x11ConnectionManager.Setup(param);
            }

            var packet = BuildX11ForwardPacket(
                            _x11ConnectionManager.SpoofedAuthProtocolName,
                            _x11ConnectionManager.SpoofedAuthProtocolDataHex,
                            _x11ConnectionManager.Params.Screen);
            _syncHandler.Send(packet);

            DataFragment response = null;
            bool accepted = false;
            if (_receivedPacket.TryGet(ref response, RESPONSE_TIMEOUT)) {
                lock (_sequenceLock) {
                    if (_sequenceStatus == SequenceStatus.X11ForwardSuccess) {
                        accepted = true;
                    }
                }
            }

            lock (_sequenceLock) {
                _sequenceStatus = accepted ? SequenceStatus.CanOpenX11Forward : SequenceStatus.Idle;
                Monitor.PulseAll(_sequenceLock);
            }

            return accepted;
        }

        #endregion
    }
}
