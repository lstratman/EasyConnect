// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

namespace Granados {

    using Granados.IO;
    using Granados.PortForwarding;
    using Granados.SSH;
    using Granados.SSH1;
    using Granados.SSH2;
    using Granados.Util;
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// Channel type
    /// </summary>
    public enum ChannelType {
        Session,
        Shell,
        ForwardedLocalToRemote,
        ForwardedRemoteToLocal,
        ExecCommand,
        Subsystem,
        AgentForwarding,
        X11Forwarding,
        Other,
    }

    /// <summary>
    /// SSH protocol event logger
    /// </summary>
    public interface ISSHProtocolEventLogger {

        /// <summary>
        /// Notifies sending a packet related to the negotiation or status changes.
        /// </summary>
        /// <param name="messageType">message type (message name defined in the SSH protocol specification)</param>
        /// <param name="details">text that describes details</param>
        void OnSend(string messageType, string details);

        /// <summary>
        /// Notifies a packet related to the negotiation or status changes has been received.
        /// </summary>
        /// <param name="messageType">message type (message name defined in the SSH protocol specification)</param>
        /// <param name="details">text that describes details</param>
        void OnReceived(string messageType, string details);

        /// <summary>
        /// Notifies additional informations related to the negotiation or status changes.
        /// </summary>
        /// <param name="details">text that describes details</param>
        void OnTrace(string details);
    }

    /// <summary>
    /// A proxy class for reading status of the underlying <see cref="IGranadosSocket"/> object.
    /// </summary>
    public class SocketStatusReader {

        private readonly IGranadosSocket _socket;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="socket">socket object</param>
        internal SocketStatusReader(IGranadosSocket socket) {
            _socket = socket;
        }

        /// <summary>
        /// Gets status of the socket object.
        /// </summary>
        public SocketStatus SocketStatus {
            get {
                return _socket.SocketStatus;
            }
        }

        /// <summary>
        /// Gets whether any received data are available on the socket
        /// </summary>
        public bool DataAvailable {
            get {
                return _socket.DataAvailable;
            }
        }

    }

    /// <summary>
    /// SSH connection
    /// </summary>
    public interface ISSHConnection : IDisposable {

        /// <summary>
        /// SSH protocol (SSH1 or SSH2)
        /// </summary>
        SSHProtocol SSHProtocol {
            get;
        }

        /// <summary>
        /// Connection parameter
        /// </summary>
        SSHConnectionParameter ConnectionParameter {
            get;
        }

        /// <summary>
        /// A property that indicates whether this connection is open.
        /// </summary>
        bool IsOpen {
            get;
        }

        /// <summary>
        /// Authenticatrion status
        /// </summary>
        AuthenticationStatus AuthenticationStatus {
            get;
        }

        /// <summary>
        /// A proxy object for reading status of the underlying <see cref="IGranadosSocket"/> object.
        /// </summary>
        SocketStatusReader SocketStatusReader {
            get;
        }

        /// <summary>
        /// Sends a disconnect message to the server, then closes this connection.
        /// </summary>
        /// <param name="reasonCode">reason code (this value is ignored on the SSH1 connection)</param>
        /// <param name="message">a message to be notified to the server</param>
        void Disconnect(DisconnectionReasonCode reasonCode, string message);

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <remarks>
        /// This method closes the underlying socket object.
        /// </remarks>
        void Close();

        /// <summary>
        /// Opens shell channel (SSH2) or interactive session (SSH1)
        /// </summary>
        /// <typeparam name="THandler">type of the channel event handler</typeparam>
        /// <param name="handlerCreator">a function that creates a channel event handler</param>
        /// <returns>a new channel event handler which was created by <paramref name="handlerCreator"/></returns>
        THandler OpenShell<THandler>(SSHChannelEventHandlerCreator<THandler> handlerCreator)
                where THandler : ISSHChannelEventHandler;

        /// <summary>
        /// Opens execute-command channel
        /// </summary>
        /// <typeparam name="THandler">type of the channel event handler</typeparam>
        /// <param name="handlerCreator">a function that creates a channel event handler</param>
        /// <param name="command">command to execute</param>
        /// <returns>a new channel event handler which was created by <paramref name="handlerCreator"/></returns>
        THandler ExecCommand<THandler>(SSHChannelEventHandlerCreator<THandler> handlerCreator, string command)
                where THandler : ISSHChannelEventHandler;

        /// <summary>
        /// Opens subsystem channel (SSH2 only)
        /// </summary>
        /// <typeparam name="THandler">type of the channel event handler</typeparam>
        /// <param name="handlerCreator">a function that creates a channel event handler</param>
        /// <param name="subsystemName">subsystem name</param>
        /// <returns>a new channel event handler which was created by <paramref name="handlerCreator"/>.</returns>
        THandler OpenSubsystem<THandler>(SSHChannelEventHandlerCreator<THandler> handlerCreator, string subsystemName)
                where THandler : ISSHChannelEventHandler;

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
        THandler ForwardPort<THandler>(SSHChannelEventHandlerCreator<THandler> handlerCreator, string remoteHost, uint remotePort, string originatorIp, uint originatorPort)
                where THandler : ISSHChannelEventHandler;

        /// <summary>
        /// Requests the remote port forwarding.
        /// </summary>
        /// <param name="requestHandler">a handler that handles the port forwarding requests from the server</param>
        /// <param name="addressToBind">address to bind on the server</param>
        /// <param name="portNumberToBind">port number to bind on the server</param>
        /// <returns>true if the request has been accepted, otherwise false.</returns>
        bool ListenForwardedPort(IRemotePortForwardingHandler requestHandler, string addressToBind, uint portNumberToBind);

        /// <summary>
        /// Cancel the remote port forwarding. (SSH2 only)
        /// </summary>
        /// <param name="addressToBind">address to bind on the server</param>
        /// <param name="portNumberToBind">port number to bind on the server</param>
        /// <returns>true if the remote port forwarding has been cancelled, otherwise false.</returns>
        bool CancelForwardedPort(string addressToBind, uint portNumberToBind);

        /// <summary>
        /// Sends ignorable data
        /// </summary>
        /// <param name="message">a message to be sent. the server may record this message into the log.</param>
        void SendIgnorableData(string message);

    }

    /// <summary>
    /// Connection event handler
    /// </summary>
    public interface ISSHConnectionEventHandler {
        /// <summary>
        /// Notifies SSH_MSG_DEBUG.
        /// </summary>
        /// <param name="alwaysDisplay">
        /// If true, the message should be displayed.
        /// Otherwise, it should not be displayed unless debugging information has been explicitly requested by the user.
        /// </param>
        /// <param name="message">a message text</param>
        void OnDebugMessage(bool alwaysDisplay, string message);

        /// <summary>
        /// Notifies SSH_MSG_IGNORE.
        /// </summary>
        /// <param name="data">data</param>
        void OnIgnoreMessage(byte[] data);

        /// <summary>
        /// Notifies unhandled message.
        /// </summary>
        /// <param name="type">value of the message number field</param>
        /// <param name="data">packet image</param>
        void OnUnhandledMessage(byte type, byte[] data);

        /// <summary>
        /// Notifies that an exception has occurred. 
        /// </summary>
        /// <param name="error">exception object</param>
        void OnError(Exception error);

        /// <summary>
        /// Notifies that the connection has been closed.
        /// </summary>
        void OnConnectionClosed();

        event EventHandler NormalTermination;
        event EventHandler<AbnormalTerminationEventArgs> AbnormalTermination;
    }

    /// <summary>
    /// A simple connection event handler class that do nothing.
    /// </summary>
    public class SimpleSSHConnectionEventHandler : ISSHConnectionEventHandler {
        public event EventHandler NormalTermination;
        public event EventHandler<AbnormalTerminationEventArgs> AbnormalTermination;

        public virtual void OnDebugMessage(bool alwaysDisplay, string message) {
        }

        public virtual void OnIgnoreMessage(byte[] data) {
        }

        public virtual void OnUnhandledMessage(byte type, byte[] data) {
        }

        public virtual void OnError(Exception error) {
        }

        public virtual void OnConnectionClosed() {
        }
    }

    /// <summary>
    /// A static class for creating a new SSH connection
    /// </summary>
    public static class SSHConnection {

        /// <summary>
        /// Establish a SSH connection
        /// </summary>
        /// <param name="socket">TCP socket which is already connected to the server.</param>
        /// <param name="param">SSH connection parameter</param>
        /// <param name="connectionEventHandlerCreator">a factory function to create a connection event handler (can be null)</param>
        /// <param name="protocolEventLoggerCreator">a factory function to create a protocol log event handler (can be null)</param>
        /// <returns>new connection object</returns>
        public static ISSHConnection Connect(
                    Socket socket,
                    SSHConnectionParameter param,
                    Func<ISSHConnection, ISSHConnectionEventHandler> connectionEventHandlerCreator = null,
                    Func<ISSHConnection, ISSHProtocolEventLogger> protocolEventLoggerCreator = null) {

            if (socket == null) {
                throw new ArgumentNullException("socket");
            }
            if (param == null) {
                throw new ArgumentNullException("param");
            }
            if (!socket.Connected) {
                throw new ArgumentException("socket is not connected to the remote host", "socket");
            }
            if (param.UserName == null) {
                throw new ArgumentException("UserName property is not set", "param");
            }
            if (param.AuthenticationType != AuthenticationType.KeyboardInteractive && param.Password == null) {
                throw new ArgumentException("Password property is not set", "param");
            }

            string clientVersion = SSHUtil.ClientVersionString(param.Protocol);

            PlainSocket psocket = new PlainSocket(socket, null);
            try {
                // receive protocol version string
                SSHProtocolVersionReceiver protoVerReceiver = new SSHProtocolVersionReceiver();
                protoVerReceiver.Receive(psocket, 5000);
                // verify the version string
                protoVerReceiver.Verify(param.Protocol);

                ISSHConnection sshConnection;

                if (param.Protocol == SSHProtocol.SSH1) {
                    // create a connection object
                    var con = new SSH1Connection(
                                psocket,
                                param,
                                protoVerReceiver.ServerVersion,
                                clientVersion,
                                connectionEventHandlerCreator,
                                protocolEventLoggerCreator);
                    // start receiving loop
                    psocket.RepeatAsyncRead();
                    // send client version
                    con.SendMyVersion();
                    // establish a SSH connection
                    con.Connect();
                    sshConnection = con;
                }
                else {
                    // create a connection object
                    var con = new SSH2Connection(
                                psocket,
                                param,
                                protoVerReceiver.ServerVersion,
                                clientVersion,
                                connectionEventHandlerCreator,
                                protocolEventLoggerCreator);
                    // start receiving loop
                    psocket.RepeatAsyncRead();
                    // send client version
                    con.SendMyVersion();
                    // establish a SSH connection
                    con.Connect();
                    sshConnection = con;
                }

                return sshConnection;
            }
            catch (Exception) {
                psocket.Close();
                throw;
            }
        }

    }

}


namespace Granados.SSH {

    using Granados.IO;
    using Granados.Util;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// A wrapper class of <see cref="ISSHConnectionEventHandler"/> for internal use.
    /// </summary>
    internal class SSHConnectionEventHandlerIgnoreErrorWrapper : ISSHConnectionEventHandler {

        private readonly ISSHConnectionEventHandler _coreHandler;

        public event EventHandler NormalTermination
        {
            add
            {
                _coreHandler.NormalTermination += value;
            }

            remove
            {
                _coreHandler.NormalTermination -= value;
            }
        }

        public event EventHandler<AbnormalTerminationEventArgs> AbnormalTermination
        {
            add
            {
                _coreHandler.AbnormalTermination += value;
            }

            remove
            {
                _coreHandler.AbnormalTermination -= value;
            }
        }

        public SSHConnectionEventHandlerIgnoreErrorWrapper(ISSHConnectionEventHandler handler) {
            _coreHandler = handler;
        }

        public void OnDebugMessage(bool alwaysDisplay, string message) {
            try {
                _coreHandler.OnDebugMessage(alwaysDisplay, message);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnIgnoreMessage(byte[] data) {
            try {
                _coreHandler.OnIgnoreMessage(data);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnUnhandledMessage(byte type, byte[] data) {
            try {
                _coreHandler.OnUnhandledMessage(type, data);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnError(Exception error) {
            try {
                _coreHandler.OnError(error);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnConnectionClosed() {
            try {
                _coreHandler.OnConnectionClosed();
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }
    }

    /// <summary>
    /// A class reads SSH protocol version
    /// </summary>
    internal class SSHProtocolVersionReceiver {

        private string _serverVersion = null;
        private readonly List<string> _lines = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        public SSHProtocolVersionReceiver() {
        }

        /// <summary>
        /// All lines recevied from the server including the version string.
        /// </summary>
        /// <remarks>Each string value doesn't contain the new-line characters.</remarks>
        public string[] Lines {
            get {
                return _lines.ToArray();
            }
        }

        /// <summary>
        /// Version string recevied from the server.
        /// </summary>
        /// <remarks>The string value doesn't contain the new-line characters.</remarks>
        public string ServerVersion {
            get {
                return _serverVersion;
            }
        }

        /// <summary>
        /// Receive version string.
        /// </summary>
        /// <param name="sock">socket object</param>
        /// <param name="timeout">timeout in msec</param>
        /// <returns>true if version string was received.</returns>
        public bool Receive(PlainSocket sock, long timeout) {
            byte[] buf = new byte[1];
            DateTime tm = DateTime.UtcNow.AddMilliseconds(timeout);
            using (MemoryStream mem = new MemoryStream()) {
                while (DateTime.UtcNow < tm && sock.SocketStatus == SocketStatus.Ready) {
                    int n = sock.ReadIfAvailable(buf);
                    if (n != 1) {
                        Thread.Sleep(10);
                        continue;
                    }
                    byte b = buf[0];
                    mem.WriteByte(b);
                    if (b == 0xa) { // LF
                        byte[] bytestr = mem.ToArray();
                        mem.SetLength(0);
                        string line = Encoding.UTF8.GetString(bytestr).TrimEnd('\xd', '\xa');
                        _lines.Add(line);
                        if (line.StartsWith("SSH-")) {
                            _serverVersion = line;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Verify server version
        /// </summary>
        /// <param name="protocol">expected protocol version</param>
        /// <exception cref="SSHException">server version doesn't match</exception>
        public void Verify(SSHProtocol protocol) {
            if (_serverVersion == null) {
                throw new SSHException(Strings.GetString("NotSSHServer"));
            }

            string[] sv = _serverVersion.Split('-');
            if (sv.Length >= 3 && sv[0] == "SSH") {
                string protocolVersion = sv[1];
                string[] pv = protocolVersion.Split('.');
                if (pv.Length >= 2) {
                    if (protocol == SSHProtocol.SSH1) {
                        if (pv[0] == "1") {
                            return; // OK
                        }
                    }
                    else if (protocol == SSHProtocol.SSH2) {
                        if (pv[0] == "2" || (pv[0] == "1" && pv[1] == "99")) {
                            return; // OK
                        }
                    }
                    throw new SSHException(
                        String.Format(Strings.GetString("IncompatibleProtocolVersion"), _serverVersion, protocol.ToString()));
                }
            }

            throw new SSHException(
                String.Format(Strings.GetString("InvalidServerVersionFormat"), _serverVersion));
        }
    }

    public class AbnormalTerminationEventArgs : EventArgs
    {
        public string Message;

        public AbnormalTerminationEventArgs(string message)
        {
            Message = message;
        }
    }

}
