// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.IO;
using Granados.IO.SSH1;
using Granados.SSH;
using Granados.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Granados.SSH1 {

    /// <summary>
    /// SSH1 abstract channel class. (base of the interactive-session and channels)
    /// </summary>
    internal abstract class SSH1ChannelBase : ISSHChannel {
        #region

        private readonly IPacketSender<SSH1Packet> _packetSender;
        private readonly SSHProtocolEventManager _protocolEventManager;

        private ISSHChannelEventHandler _handler = new SimpleSSHChannelEventHandler();

        /// <summary>
        /// Event that notifies that this channel is already dead.
        /// </summary>
        internal event Action<ISSHChannel> Died;

        /// <summary>
        /// Constructor
        /// </summary>
        public SSH1ChannelBase(
                IPacketSender<SSH1Packet> packetSender,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                uint remoteChannel,
                ChannelType channelType,
                string channelTypeString) {

            _packetSender = packetSender;
            _protocolEventManager = protocolEventManager;
            LocalChannel = localChannel;
            RemoteChannel = remoteChannel;
            ChannelType = channelType;
            ChannelTypeString = channelTypeString;
        }

        #region ISSHChannel properties

        /// <summary>
        /// Local channel number.
        /// </summary>
        public uint LocalChannel {
            get;
            private set;
        }

        /// <summary>
        /// Remote channel number.
        /// </summary>
        public uint RemoteChannel {
            get;
            private set;
        }

        /// <summary>
        /// Channel type. (predefined type)
        /// </summary>
        public ChannelType ChannelType {
            get;
            private set;
        }

        /// <summary>
        /// Channel type string. (actual channel type name)
        /// </summary>
        public string ChannelTypeString {
            get;
            private set;
        }

        /// <summary>
        /// true if this channel is open.
        /// </summary>
        public abstract bool IsOpen {
            get;
        }

        /// <summary>
        /// true if this channel is ready for use.
        /// </summary>
        public abstract bool IsReady {
            get;
        }

        /// <summary>
        /// Maximum size of the channel data which can be sent with a single SSH packet.
        /// </summary>
        /// <remarks>
        /// In SSH1, this size will be determined from the maximum packet size specified in the protocol specification.
        /// </remarks>
        public abstract int MaxChannelDatagramSize {
            get;
        }

        #endregion

        #region ISSHChannel methods

        /// <summary>
        /// Send window dimension change message.
        /// </summary>
        /// <param name="width">terminal width, columns</param>
        /// <param name="height">terminal height, rows</param>
        /// <param name="pixelWidth">terminal width, pixels</param>
        /// <param name="pixelHeight">terminal height, pixels</param>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public abstract void ResizeTerminal(uint width, uint height, uint pixelWidth, uint pixelHeight);

        /// <summary>
        /// Block execution of the current thread until the channel is ready for the communication.
        /// </summary>
        /// <returns>
        /// true if this channel is ready for the communication.<br/>
        /// false if this channel failed to open.<br/>
        /// false if this channel is going to close or is already closed.
        /// </returns>
        public abstract bool WaitReady();

        /// <summary>
        /// Send data.
        /// </summary>
        /// <param name="data">data to send</param>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public abstract void Send(DataFragment data);

        /// <summary>
        /// Send EOF.
        /// </summary>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public abstract void SendEOF();

        /// <summary>
        /// Send Break. (SSH2, session channel only)
        /// </summary>
        /// <param name="breakLength">break-length in milliseconds</param>
        /// <returns>true if succeeded. false if the request failed.</returns>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public bool SendBreak(int breakLength) {
            return false;
        }

        /// <summary>
        /// Close this channel.
        /// </summary>
        /// <remarks>
        /// After calling this method, all mothods of the <see cref="ISSHChannel"/> will throw <see cref="SSHChannelInvalidOperationException"/>.
        /// </remarks>
        /// <remarks>
        /// If this method was called under the inappropriate channel state, the method call will be ignored silently.
        /// </remarks>
        public abstract void Close();

        #endregion

        /// <summary>
        /// Sets handler
        /// </summary>
        /// <param name="handler"></param>
        public void SetHandler(ISSHChannelEventHandler handler) {
            _handler = new SSHChannelEventHandlerIgnoreErrorWrapper(handler);
        }

        /// <summary>
        /// Process packet about this channel.
        /// </summary>
        /// <param name="packetType">a packet type (message number)</param>
        /// <param name="packetFragment">a packet image except message number and recipient channel.</param>
        public abstract void ProcessPacket(SSH1PacketType packetType, DataFragment packetFragment);

        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="packet">packet object</param>
        protected void Transmit(SSH1Packet packet) {
            _packetSender.Send(packet);
        }

        /// <summary>
        /// Event handler object
        /// </summary>
        protected ISSHChannelEventHandler Handler {
            get {
                return _handler;
            }
        }

        /// <summary>
        /// Sets remote channel.
        /// </summary>
        /// <param name="remoteChannel">remote channel</param>
        protected void SetRemoteChannel(uint remoteChannel) {
            RemoteChannel = remoteChannel;
        }

        /// <summary>
        /// Notifies terminating this channel.
        /// </summary>
        protected void Die() {
            if (Died != null) {
                Died(this);
            }
        }

        /// <summary>
        /// Outputs Trace message.
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args">format arguments</param>
        protected void Trace(string format, params object[] args) {
            _protocolEventManager.Trace(format, args);
        }

        #endregion
    }

    /// <summary>
    /// SSH1 pseudo channel class for the interactive session.
    /// </summary>
    internal class SSH1InteractiveSession : SSH1ChannelBase {
        #region

        private const int PASSING_TIMEOUT = 1000;
        private const int RESPONSE_TIMEOUT = 5000;

        protected enum State {
            /// <summary>initial state</summary>
            Initial,
            /// <summary>SSH_CMSG_REQUEST_PTY has been sent. waiting SSH_SMSG_SUCCESS | SSH_SMSG_FAILURE.</summary>
            WaitStartPTYResponse,
            /// <summary>SSH_SMSG_SUCCESS has been received</summary>
            StartPTYSuccess,
            /// <summary>SSH_SMSG_FAILURE has been received</summary>
            StartPTYFailure,
            /// <summary>the interactive session has been established. more request may be requested.</summary>
            Established,
            /// <summary>the interactive session is ready for use</summary>
            Ready,
            /// <summary>closing has been requested</summary>
            Closing,
            /// <summary>the interactive session has been closed</summary>
            Closed,
        }

        private volatile State _state;
        private readonly object _stateSync = new object();

        private bool _eof = false;

        private readonly AtomicBox<DataFragment> _receivedPacket = new AtomicBox<DataFragment>();

        // Min/Max size of the SSH_CMSG_STDIN_DATA datagram
        // Maximum packet size is 262144 bytes. (not including the length field and padding)
        // SSH_CMSG_STDIN_DATA:
        //   Packet type (SSH_CMSG_STDIN_DATA) : 1 byte
        //   Data length : 4 bytes
        //   Data : n bytes
        //   Check bytes : 4 bytes
        private const int MAX_DATAGRAM_SIZE = 262144 - (1 + 4 + 4);

        /// <summary>
        /// Constructor
        /// </summary>
        public SSH1InteractiveSession(
                IPacketSender<SSH1Packet> packetSender,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                ChannelType channelType,
                string channelTypeString)
            : base(packetSender, protocolEventManager, localChannel, 0, channelType, channelTypeString) {

            _state = State.Initial;
        }

        #region ISSHChannel properties

        /// <summary>
        /// true if this channel is open.
        /// </summary>
        public override bool IsOpen {
            get {
                return _state != State.Closed;
            }
        }

        /// <summary>
        /// true if this channel is ready for use.
        /// </summary>
        public override bool IsReady {
            get {
                return _state == State.Ready;
            }
        }

        /// <summary>
        /// Maximum size of the channel data which can be sent with a single SSH packet.
        /// </summary>
        /// <remarks>
        /// In SSH1, this size will be determined from the maximum packet size specified in the protocol specification.
        /// </remarks>
        public override int MaxChannelDatagramSize {
            get {
                return MAX_DATAGRAM_SIZE;
            }
        }

        #endregion

        #region ISSHChannel methods

        /// <summary>
        /// Send window dimension change message.
        /// </summary>
        /// <param name="width">terminal width, columns</param>
        /// <param name="height">terminal height, rows</param>
        /// <param name="pixelWidth">terminal width, pixels</param>
        /// <param name="pixelHeight">terminal height, pixels</param>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public override void ResizeTerminal(uint width, uint height, uint pixelWidth, uint pixelHeight) {
            lock (_stateSync) {
                if (_state == State.Closing || _state == State.Closed) {
                    throw new SSHChannelInvalidOperationException("Channel already closed");
                }

                Transmit(
                    new SSH1Packet(SSH1PacketType.SSH_CMSG_WINDOW_SIZE)
                        .WriteUInt32(height)
                        .WriteUInt32(width)
                        .WriteUInt32(pixelWidth)
                        .WriteUInt32(pixelHeight)
                );
            }

            Trace("CH[{0}] resize window : width={1} height={2} pixelWidth={3} pixelHeight={4}",
                LocalChannel, width, height, pixelWidth, pixelHeight);
        }

        /// <summary>
        /// Block execution of the current thread until the channel is ready for the communication.
        /// </summary>
        /// <returns>
        /// true if this channel is ready for the communication.<br/>
        /// false if this channel failed to open.<br/>
        /// false if this channel is going to close or is already closed.
        /// </returns>
        public override bool WaitReady() {
            // quick check before the lock block
            if (_state == State.Ready) {
                return true;
            }
            lock (_stateSync) {
                while (true) {
                    if (_state == State.Ready) {
                        return true;
                    }
                    if (_state == State.Closing || _state == State.Closed) {
                        return false;
                    }
                    Monitor.Wait(_stateSync);
                }
            }
        }

        /// <summary>
        /// Send data.
        /// </summary>
        /// <param name="data">data to send</param>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public override void Send(DataFragment data) {
            lock (_stateSync) {
                if (_state == State.Closing || _state == State.Closed) {
                    throw new SSHChannelInvalidOperationException("Channel already closed");
                }

                int offset = data.Offset;
                int length = data.Length;
                int maxSize = MaxChannelDatagramSize;

                do {
                    int datagramSize = (length > maxSize) ? maxSize : length;
                    Transmit(
                        new SSH1Packet(SSH1PacketType.SSH_CMSG_STDIN_DATA)
                            .WriteAsString(data.Data, offset, datagramSize)
                    );
                    offset += datagramSize;
                    length -= datagramSize;
                } while (length > 0);
            }
        }

        /// <summary>
        /// Send EOF.
        /// </summary>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public override void SendEOF() {
            lock (_stateSync) {
                if (_state == State.Closing || _state == State.Closed) {
                    throw new SSHChannelInvalidOperationException("Channel already closed");
                }

                Transmit(
                    new SSH1Packet(SSH1PacketType.SSH_CMSG_EOF)
                );

                _eof = true;
            }

            Trace("CH[{0}] send EOF", LocalChannel);
        }

        /// <summary>
        /// Close this channel.
        /// </summary>
        /// <remarks>
        /// After calling this method, all mothods of the <see cref="ISSHChannel"/> will throw <see cref="SSHChannelInvalidOperationException"/>.
        /// </remarks>
        /// <remarks>
        /// If this method was called under the inappropriate channel state, the method call will be ignored silently.
        /// </remarks>
        public override void Close() {
            // quick check for avoiding deadlock
            if (_state != State.Established && _state != State.Ready) {
                return;
            }

            Trace("CH[{0}] close by client", LocalChannel);

            lock (_stateSync) {
                if (_state != State.Established && _state != State.Ready) {
                    return;
                }

                if (!_eof) {
                    SendEOF();
                }
            }

            SetStateClosed(false);
        }

        #endregion

        /// <summary>
        /// Starts a shell
        /// </summary>
        /// <param name="param">connection parameters</param>
        /// <returns>true if shell has been started successfully</returns>
        public bool ExecShell(SSHConnectionParameter param) {
            if (!OpenPTY(param)) {
                return false;
            }

            lock (_stateSync) {
                Transmit(
                    new SSH1Packet(SSH1PacketType.SSH_CMSG_EXEC_SHELL)
                );
                _state = State.Established;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

            Trace("CH[{0}] execute shell", LocalChannel);

            DataFragment empty = new DataFragment(0);
            Handler.OnEstablished(empty);

            lock (_stateSync) {
                _state = State.Ready;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

            Handler.OnReady();

            return true;
        }

        /// <summary>
        /// Starts a command
        /// </summary>
        /// <param name="param">connection parameters</param>
        /// <param name="command">command line to execute</param>
        public void ExecCommand(SSHConnectionParameter param, string command) {
            Task.Run(() => DoExecCommand(param, command));
        }

        /// <summary>
        /// Starts a command
        /// </summary>
        /// <param name="param">connection parameters</param>
        /// <param name="command">command line to execute</param>
        private void DoExecCommand(SSHConnectionParameter param, string command) {
            if (!OpenPTY(param)) {
                return;
            }

            lock (_stateSync) {
                Transmit(
                    new SSH1Packet(SSH1PacketType.SSH_CMSG_EXEC_CMD)
                        .WriteString(command)
                );
                _state = State.Established;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

            Trace("CH[{0}] execute command : command={1}", LocalChannel, command);

            DataFragment empty = new DataFragment(0);
            Handler.OnEstablished(empty);

            lock (_stateSync) {
                _state = State.Ready;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

            Handler.OnReady();
        }

        /// <summary>
        /// Open PTY
        /// </summary>
        /// <param name="param">connection parameters</param>
        /// <returns>true if pty has been opened successfully</returns>
        private bool OpenPTY(SSHConnectionParameter param) {
            lock (_stateSync) {
                if (_state != State.Initial) {
                    return false;
                }

                _state = State.WaitStartPTYResponse;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

            Transmit(
                new SSH1Packet(SSH1PacketType.SSH_CMSG_REQUEST_PTY)
                    .WriteString(param.TerminalName)
                    .WriteInt32(param.TerminalHeight)
                    .WriteInt32(param.TerminalWidth)
                    .WriteInt32(param.TerminalPixelWidth)
                    .WriteInt32(param.TerminalPixelHeight)
                    .Write(new byte[1]) //TTY_OP_END
            );

            Trace("CH[{0}] request PTY : term={1} width={2} height={3} pixelWidth={4} pixelHeight={5}",
                LocalChannel, param.TerminalName,
                param.TerminalWidth, param.TerminalHeight,
                param.TerminalPixelWidth, param.TerminalPixelHeight);

            DataFragment packet = null;
            if (!_receivedPacket.TryGet(ref packet, RESPONSE_TIMEOUT)) {
                RequestFailed();
                return false;
            }

            lock (_stateSync) {
                if (_state == State.StartPTYSuccess) {
                    return true;
                }
            }

            RequestFailed();
            return false;
        }

        /// <summary>
        /// Changes state when the request was failed.
        /// </summary>
        private void RequestFailed() {
            Handler.OnRequestFailed();
            SetStateClosed(false);
        }

        /// <summary>
        /// Set state to "Closed".
        /// </summary>
        /// <param name="byServer"></param>
        private void SetStateClosed(bool byServer) {
            lock (_stateSync) {
                if (_state == State.Closed) {
                    return;
                }
                if (_state == State.Closing) {
                    _state = State.Closed;
                    Monitor.PulseAll(_stateSync);   // notifies state change
                    goto OnClosed;
                }
                _state = State.Closing;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

            Handler.OnClosing(byServer);

            lock (_stateSync) {
                if (_state == State.Closed) {   // state transition has occurred in OnClosing()
                    return;
                }
                _state = State.Closed;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

        OnClosed:
            Handler.OnClosed(byServer);

            Die();
        }

        /// <summary>
        /// Process packet about this channel.
        /// </summary>
        /// <param name="packetType">a packet type (message number)</param>
        /// <param name="packetFragment">a packet image except message number and recipient channel.</param>
        public override void ProcessPacket(SSH1PacketType packetType, DataFragment packetFragment) {
            if (_state == State.Closed) {
                return; // ignore
            }

            DataFragment dataFragmentArg;

            lock (_stateSync) {
                switch (_state) {
                    case State.Initial:
                        break;
                    case State.WaitStartPTYResponse:
                        if (packetType == SSH1PacketType.SSH_SMSG_SUCCESS) {
                            _state = State.StartPTYSuccess;
                            Monitor.PulseAll(_stateSync);   // notifies state change
                            _receivedPacket.TrySet(packetFragment, PASSING_TIMEOUT);
                        }
                        else if (packetType == SSH1PacketType.SSH_SMSG_FAILURE) {
                            _state = State.StartPTYFailure;
                            Monitor.PulseAll(_stateSync);   // notifies state change
                            _receivedPacket.TrySet(packetFragment, PASSING_TIMEOUT);
                        }
                        break;
                    case State.Established:
                        break;
                    case State.Ready:
                        switch (packetType) {
                            case SSH1PacketType.SSH_SMSG_STDOUT_DATA: {
                                    SSH1DataReader reader = new SSH1DataReader(packetFragment);
                                    int len = reader.ReadInt32();
                                    dataFragmentArg = reader.GetRemainingDataView(len);
                                }
                                goto OnData;    // do it out of the lock block
                            case SSH1PacketType.SSH_SMSG_STDERR_DATA: {
                                    SSH1DataReader reader = new SSH1DataReader(packetFragment);
                                    int len = reader.ReadInt32();
                                    dataFragmentArg = reader.GetRemainingDataView(len);
                                }
                                goto OnData;    // do it out of the lock block
                            case SSH1PacketType.SSH_SMSG_EXITSTATUS:
                                Transmit(
                                    new SSH1Packet(SSH1PacketType.SSH_CMSG_EXIT_CONFIRMATION)
                                );
                                goto SetStateClosedByServer;    // do it out of the lock block
                        }
                        goto OnUnhandledPacket; // do it out of the lock block
                }
            }

            return;

        OnData:
            Handler.OnData(dataFragmentArg);
            return;

        SetStateClosedByServer:
            Trace("CH[{0}] closed by server", LocalChannel);
            SetStateClosed(true);
            return;

        OnUnhandledPacket:
            Handler.OnUnhandledPacket((byte)packetType, packetFragment);
            return;
        }

        #endregion
    }

    /// <summary>
    /// SSH1 channel base class.
    /// </summary>
    internal abstract class SSH1SubChannelBase : SSH1ChannelBase {
        #region

        protected enum State {
            /// <summary>channel has been requested by the server</summary>
            InitiatedByServer,
            /// <summary>channel has been requested by the client</summary>
            InitiatedByClient,
            /// <summary>channel has been established. more request may be requested.</summary>
            Established,
            /// <summary>channel is ready for use</summary>
            Ready,
            /// <summary>closing has been requested</summary>
            Closing,
            /// <summary>channel has been closed</summary>
            Closed,
        }

        protected enum SubPacketProcessResult {
            /// <summary>the packet was not consumed</summary>
            NotConsumed,
            /// <summary>the packet was consumed</summary>
            Consumed,
        }

        private volatile State _state;
        private readonly object _stateSync = new object();

        // Min/Max size of the SSH_MSG_CHANNEL_DATA datagram
        // Maximum packet size is 262144 bytes. (not including the length field and padding)
        // SSH_MSG_CHANNEL_DATA:
        //   Packet type (SSH_MSG_CHANNEL_DATA) : 1 byte
        //   Remote channel : 4 bytes
        //   Data length : 4 bytes
        //   Data : n bytes
        //   Check bytes : 4 bytes
        private const int MAX_DATAGRAM_SIZE = 262144 - (1 + 4 + 4 + 4);

        /// <summary>
        /// Constructor (initiated by server)
        /// </summary>
        public SSH1SubChannelBase(
                IPacketSender<SSH1Packet> packetSender,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                uint remoteChannel,
                ChannelType channelType,
                string channelTypeString)
            : base(packetSender, protocolEventManager, localChannel, remoteChannel, channelType, channelTypeString) {

            _state = State.InitiatedByServer; // SendOpenConfirmation() will change state to "Opened"
        }

        /// <summary>
        /// Constructor (initiated by client)
        /// </summary>
        public SSH1SubChannelBase(
                IPacketSender<SSH1Packet> packetSender,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                ChannelType channelType,
                string channelTypeString)
            : base(packetSender, protocolEventManager, localChannel, 0, channelType, channelTypeString) {

            _state = State.InitiatedByClient; // receiving SSH_MSG_CHANNEL_OPEN_CONFIRMATION will change state to "Opened"
        }

        /// <summary>
        /// Major state
        /// </summary>
        protected State MajorState {
            get {
                return _state;
            }
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_OPEN
        /// </summary>
        public void SendOpen() {
            var packet = BuildOpenPacket();
            Transmit(packet.Item1);
            Trace(packet.Item2, packet.Item3);
        }

        /// <summary>
        /// Builds SSH_MSG_CHANNEL_OPEN packet
        /// </summary>
        /// <returns>Tuple{ packet, traceTextFormat, traceTextArgs }</returns>
        protected abstract Tuple<SSH1Packet, string, object[]> BuildOpenPacket();

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_OPEN_CONFIRMATION
        /// </summary>
        /// <exception cref="InvalidOperationException">inappropriate channel state</exception>
        public void SendOpenConfirmation() {
            lock (_stateSync) {
                if (_state != State.InitiatedByServer) {
                    throw new InvalidOperationException();
                }

                Transmit(
                    new SSH1Packet(SSH1PacketType.SSH_MSG_CHANNEL_OPEN_CONFIRMATION)
                        .WriteUInt32(RemoteChannel)
                        .WriteUInt32(LocalChannel)
                );

                _state = State.Established;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

            Trace("CH[{0}] opened", LocalChannel);

            Handler.OnEstablished(new DataFragment(0));
            OnChannelEstablished();
        }

        #region ISSHChannel properties

        /// <summary>
        /// true if this channel is open.
        /// </summary>
        public override bool IsOpen {
            get {
                return _state != State.Closed;
            }
        }

        /// <summary>
        /// true if this channel is ready for use.
        /// </summary>
        public override bool IsReady {
            get {
                return _state == State.Ready;
            }
        }

        /// <summary>
        /// Maximum size of the channel data which can be sent with a single SSH packet.
        /// </summary>
        /// <remarks>
        /// In SSH1, this size will be determined from the maximum packet size specified in the protocol specification.
        /// </remarks>
        public override int MaxChannelDatagramSize {
            get {
                return MAX_DATAGRAM_SIZE;
            }
        }

        #endregion

        #region ISSHChannel methods

        /// <summary>
        /// Send window dimension change message.
        /// </summary>
        /// <param name="width">terminal width, columns</param>
        /// <param name="height">terminal height, rows</param>
        /// <param name="pixelWidth">terminal width, pixels</param>
        /// <param name="pixelHeight">terminal height, pixels</param>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public override void ResizeTerminal(uint width, uint height, uint pixelWidth, uint pixelHeight) {
            // do nothing
        }

        /// <summary>
        /// Block execution of the current thread until the channel is ready for the communication.
        /// </summary>
        /// <returns>
        /// true if this channel is ready for the communication.<br/>
        /// false if this channel failed to open.<br/>
        /// false if this channel is going to close or is already closed.
        /// </returns>
        public override bool WaitReady() {
            // quick check before the lock block
            if (_state == State.Ready) {
                return true;
            }
            lock (_stateSync) {
                while (true) {
                    if (_state == State.Ready) {
                        return true;
                    }
                    if (_state == State.Closing || _state == State.Closed) {
                        return false;
                    }
                    Monitor.Wait(_stateSync);
                }
            }
        }

        /// <summary>
        /// Send data.
        /// </summary>
        /// <param name="data">data to send</param>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public override void Send(DataFragment data) {
            lock (_stateSync) {
                if (_state == State.Closing || _state == State.Closed) {
                    throw new SSHChannelInvalidOperationException("Channel already closed");
                }

                int offset = data.Offset;
                int length = data.Length;
                int maxSize = MaxChannelDatagramSize;

                do {
                    int datagramSize = (length > maxSize) ? maxSize : length;
                    Transmit(
                        new SSH1Packet(SSH1PacketType.SSH_MSG_CHANNEL_DATA)
                            .WriteUInt32(RemoteChannel)
                            .WriteAsString(data.Data, offset, datagramSize)
                    );
                    offset += datagramSize;
                    length -= datagramSize;
                } while (length > 0);
            }
        }

        /// <summary>
        /// Send EOF.
        /// </summary>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public override void SendEOF() {
            // do nothing
        }

        /// <summary>
        /// Close this channel.
        /// </summary>
        /// <remarks>
        /// After calling this method, all mothods of the <see cref="ISSHChannel"/> will throw <see cref="SSHChannelInvalidOperationException"/>.
        /// </remarks>
        /// <remarks>
        /// If this method was called under the inappropriate channel state, the method call will be ignored silently.
        /// </remarks>
        public override void Close() {
            // quick check for avoiding deadlock
            if (_state != State.Established && _state != State.Ready) {
                return;
            }

            lock (_stateSync) {
                if (_state != State.Established && _state != State.Ready) {
                    return;
                }

                _state = State.Closing;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

            Handler.OnClosing(false);

            Transmit(
                new SSH1Packet(SSH1PacketType.SSH_MSG_CHANNEL_CLOSE)
                    .WriteUInt32(RemoteChannel)
            );
            Trace("CH[{0}] close", LocalChannel);
        }

        #endregion

        /// <summary>
        /// Do additional work when <see cref="State"/> was changed to <see cref="State.Established"/>.
        /// </summary>
        /// <returns>true if the channel is ready for use.</returns>
        protected virtual void OnChannelEstablished() {
            // derived class can override this.
            SetStateReady();
        }

        /// <summary>
        /// The derived class can change state from "Established" to "Ready" by calling this method.
        /// </summary>
        protected void SetStateReady() {
            lock (_stateSync) {
                if (_state != State.Established) {
                    return;
                }

                _state = State.Ready;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }
            Handler.OnReady();
        }

        /// <summary>
        /// The derived class can change state from "Established" to "Closing" by calling this method.
        /// </summary>
        protected void RequestFailed() {
            lock (_stateSync) {
                if (_state == State.Established) {
                    goto Close;
                }
                else if (_state == State.InitiatedByClient) {
                    goto SetStateClosedByClient;
                }
            }

            return;

        Close:
            Handler.OnRequestFailed();
            Close();
            return;

        SetStateClosedByClient:
            Handler.OnRequestFailed();
            SetStateClosed(false);
            return;
        }

        /// <summary>
        /// Set state to "Closed".
        /// </summary>
        /// <param name="byServer"></param>
        private void SetStateClosed(bool byServer) {
            lock (_stateSync) {
                if (_state == State.Closed) {
                    return;
                }
                if (_state == State.Closing) {
                    _state = State.Closed;
                    Monitor.PulseAll(_stateSync);   // notifies state change
                    goto OnClosed;
                }
                _state = State.Closing;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

            Handler.OnClosing(byServer);

            lock (_stateSync) {
                if (_state == State.Closed) {   // state transition has occurred in OnClosing()
                    return;
                }
                _state = State.Closed;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

        OnClosed:
            Handler.OnClosed(byServer);

            Die();
        }

        /// <summary>
        /// Process packet additionally.
        /// </summary>
        /// <remarks>
        /// This method will be called repeatedly while <see cref="State"/> is <see cref="State.Established"/> or <see cref="State.Ready"/>.
        /// </remarks>
        /// <param name="packetType">a packet type (message number)</param>
        /// <param name="packetFragment">a packet image except message number and recipient channel.</param>
        /// <returns>result</returns>
        protected virtual SubPacketProcessResult ProcessPacketSub(SSH1PacketType packetType, DataFragment packetFragment) {
            // derived class can override this.
            return SubPacketProcessResult.NotConsumed;
        }

        /// <summary>
        /// Process packet about this channel.
        /// </summary>
        /// <param name="packetType">a packet type (message number)</param>
        /// <param name="packetFragment">a packet image except message number and recipient channel.</param>
        public override void ProcessPacket(SSH1PacketType packetType, DataFragment packetFragment) {
            if (_state == State.Closed) {
                return; // ignore
            }

            DataFragment dataFragmentArg;

            lock (_stateSync) {
                switch (_state) {
                    case State.InitiatedByServer:
                        break;
                    case State.InitiatedByClient:
                        if (packetType == SSH1PacketType.SSH_MSG_CHANNEL_OPEN_CONFIRMATION) {
                            SSH1DataReader reader = new SSH1DataReader(packetFragment);
                            SetRemoteChannel(reader.ReadUInt32());
                            _state = State.Established;
                            Monitor.PulseAll(_stateSync);   // notifies state change
                            dataFragmentArg = new DataFragment(0);
                            goto OnEstablished; // do it out of the lock block
                        }
                        if (packetType == SSH1PacketType.SSH_MSG_CHANNEL_OPEN_FAILURE) {
                            goto RequestFailed; // do it out of the lock block
                        }
                        break;
                    case State.Closing:
                        if (packetType == SSH1PacketType.SSH_MSG_CHANNEL_CLOSE_CONFIRMATION) {
                            goto SetStateClosedByClient;    // do it out of the lock block
                        }
                        break;
                    case State.Established:
                    case State.Ready:
                        if (ProcessPacketSub(packetType, packetFragment) == SubPacketProcessResult.Consumed) {
                            return;
                        }
                        switch (packetType) {
                            case SSH1PacketType.SSH_MSG_CHANNEL_DATA: {
                                    SSH1DataReader reader = new SSH1DataReader(packetFragment);
                                    int len = reader.ReadInt32();
                                    dataFragmentArg = reader.GetRemainingDataView(len);
                                }
                                goto OnData;    // do it out of the lock block
                            case SSH1PacketType.SSH_MSG_CHANNEL_CLOSE:
                                Transmit(
                                    new SSH1Packet(SSH1PacketType.SSH_MSG_CHANNEL_CLOSE_CONFIRMATION)
                                        .WriteUInt32(RemoteChannel)
                                );
                                goto SetStateClosedByServer;    // do it out of the lock block
                        }
                        goto OnUnhandledPacket; // do it out of the lock block
                }
            }

            return;

        OnEstablished:
            Trace("CH[{0}] channel opend : remoteChannel={1}", LocalChannel, RemoteChannel);
            Handler.OnEstablished(dataFragmentArg);
            OnChannelEstablished();
            return;

        RequestFailed:
            Trace("CH[{0}] request failed", LocalChannel);
            RequestFailed();
            return;

        SetStateClosedByClient:
            Trace("CH[{0}] closed by client", LocalChannel);
            SetStateClosed(false);
            return;

        SetStateClosedByServer:
            Trace("CH[{0}] closed by server", LocalChannel);
            SetStateClosed(true);
            return;

        OnData:
            Handler.OnData(dataFragmentArg);
            return;

        OnUnhandledPacket:
            Handler.OnUnhandledPacket((byte)packetType, packetFragment);
            return;
        }

        #endregion
    }

    /// <summary>
    /// SSH1 channel operator for the local port forwarding.
    /// </summary>
    internal class SSH1LocalPortForwardingChannel : SSH1SubChannelBase {
        #region

        private const ChannelType CHANNEL_TYPE = ChannelType.ForwardedLocalToRemote;
        private const string CHANNEL_TYPE_STRING = "ForwardedLocalToRemote";

        private readonly string _remoteHost;
        private readonly uint _remotePort;
        private readonly string _originatorIp;
        private readonly uint _originatorPort;

        /// <summary>
        /// Constructor (initiated by client)
        /// </summary>
        public SSH1LocalPortForwardingChannel(
                IPacketSender<SSH1Packet> packetSender,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                string remoteHost,
                uint remotePort,
                string originatorIp,
                uint originatorPort)
            : base(packetSender, protocolEventManager, localChannel, CHANNEL_TYPE, CHANNEL_TYPE_STRING) {

            _remoteHost = remoteHost;
            _remotePort = remotePort;
            _originatorIp = originatorIp;
            _originatorPort = originatorPort;
        }

        /// <summary>
        /// Builds SSH_MSG_PORT_OPEN packet
        /// </summary>
        protected override Tuple<SSH1Packet, string, object[]> BuildOpenPacket() {
            var packet =
                new SSH1Packet(SSH1PacketType.SSH_MSG_PORT_OPEN)
                    .WriteUInt32(LocalChannel)
                    .WriteString(_remoteHost)
                    .WriteUInt32(_remotePort);
            // Note:
            //  "originator" is specified only if both sides specified
            //  SSH_PROTOFLAG_HOST_IN_FWD_OPEN in the protocol flags.
            //  currently 0 is used as the protocol flags.
            //  
            //  .WriteString(_originatorIp + ":" + _originatorPort.ToString(NumberFormatInfo.InvariantInfo))

            return Tuple.Create(
                packet,
                "CH[0] port open : remoteHost={1} remotePort={2}",
                new object[] { LocalChannel, _remoteHost, _remotePort }
            );

        }
        #endregion
    }

    /// <summary>
    /// SSH1 channel operator for the remote port forwarding.
    /// </summary>
    internal class SSH1RemotePortForwardingChannel : SSH1SubChannelBase {
        #region

        private const ChannelType CHANNEL_TYPE = ChannelType.ForwardedRemoteToLocal;
        private const string CHANNEL_TYPE_STRING = "ForwardedRemoteToLocal";

        /// <summary>
        /// Constructor (initiated by server)
        /// </summary>
        public SSH1RemotePortForwardingChannel(
                IPacketSender<SSH1Packet> packetSender,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                uint remoteChannel)
            : base(packetSender, protocolEventManager, localChannel, remoteChannel, CHANNEL_TYPE, CHANNEL_TYPE_STRING) {
        }

        /// <summary>
        /// Builds an open-channel packet.
        /// </summary>
        protected override Tuple<SSH1Packet, string, object[]> BuildOpenPacket() {
            // this method should not be called.
            throw new InvalidOperationException();
        }

        #endregion
    }

    /// <summary>
    /// SSH1 channel operator for the agent forwarding.
    /// </summary>
    internal class SSH1AgentForwardingChannel : SSH1SubChannelBase {
        #region

        private const ChannelType CHANNEL_TYPE = ChannelType.AgentForwarding;
        private const string CHANNEL_TYPE_STRING = "AgentForwarding";

        /// <summary>
        /// Constructor (initiated by server)
        /// </summary>
        public SSH1AgentForwardingChannel(
                IPacketSender<SSH1Packet> packetSender,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                uint remoteChannel)
            : base(packetSender, protocolEventManager, localChannel, remoteChannel, CHANNEL_TYPE, CHANNEL_TYPE_STRING) {
        }

        /// <summary>
        /// Builds an open-channel packet.
        /// </summary>
        protected override Tuple<SSH1Packet, string, object[]> BuildOpenPacket() {
            // this method should not be called.
            throw new InvalidOperationException();
        }

        #endregion
    }

    /// <summary>
    /// SSH1 channel operator for the X11 forwarding.
    /// </summary>
    internal class SSH1X11ForwardingChannel : SSH1SubChannelBase {
        #region

        private const ChannelType CHANNEL_TYPE = ChannelType.X11Forwarding;
        private const string CHANNEL_TYPE_STRING = "X11Forwarding";

        /// <summary>
        /// Constructor (initiated by server)
        /// </summary>
        public SSH1X11ForwardingChannel(
                IPacketSender<SSH1Packet> packetSender,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                uint remoteChannel)
            : base(packetSender, protocolEventManager, localChannel, remoteChannel, CHANNEL_TYPE, CHANNEL_TYPE_STRING) {
        }

        /// <summary>
        /// Builds an open-channel packet.
        /// </summary>
        protected override Tuple<SSH1Packet, string, object[]> BuildOpenPacket() {
            // this method should not be called.
            throw new InvalidOperationException();
        }

        #endregion
    }
}
