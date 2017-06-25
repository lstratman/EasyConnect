// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.IO;
using Granados.IO.SSH2;
using Granados.SSH;
using Granados.Util;
using Granados.X11;
using Granados.X11Forwarding;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Granados.SSH2 {

    /// <summary>
    /// SSH2 channel base class.
    /// </summary>
    internal class SSH2ChannelBase : ISSHChannel {
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

        protected enum ChannelRequestResult {
            /// <summary>server replied SSH_MSG_REQUEST_SUCCESS</summary>
            Success,
            /// <summary>server replied SSH_MSG_REQUEST_FAILURE</summary>
            Failure,
            /// <summary>no response</summary>
            Timeout,
        }

        // Min/Max size of the SSH_MSG_CHANNEL_DATA datagram
        // Maximum payload size is 32768 bytes. (described in RFC4253)
        // SSH_MSG_CHANNEL_DATA:
        //   Packet type (SSH_MSG_CHANNEL_DATA) : 1 byte
        //   Recipient channel : 4 bytes
        //   Data length : 4 bytes
        //   Data : n bytes
        private const int CHANNEL_DATA_INFO_SIZE = 1 + 4 + 4;
        private const int MIN_DATAGRAM_SIZE = 512;
        private const int MAX_DATAGRAM_SIZE = 32768 - CHANNEL_DATA_INFO_SIZE;

        private readonly IPacketSender<SSH2Packet> _packetSender;
        private readonly SSHProtocolEventManager _protocolEventManager;
        private readonly int _localMaxPacketSize;
        private readonly int _localWindowSize;
        private int _localWindowSizeLeft;
        private uint _serverMaxPacketSize;
        private uint _serverWindowSizeLeft;

        private volatile State _state;
        private readonly object _stateSync = new object();

        private readonly object _channelRequestStatusSync = new object();
        private volatile bool _channelRequestStatus = false;
        private readonly AtomicBox<bool> _channelRequestResult = new AtomicBox<bool>();

        private ISSHChannelEventHandler _handler = new SimpleSSHChannelEventHandler();

        private int? _maxDatagramSize = null;

        /// <summary>
        /// Event that notifies that this channel is already dead.
        /// </summary>
        internal event Action<ISSHChannel> Died;

        /// <summary>
        /// Constructor (initiated by server)
        /// </summary>
        public SSH2ChannelBase(
                IPacketSender<SSH2Packet> packetSender,
                SSHConnectionParameter param,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                uint remoteChannel,
                ChannelType channelType,
                string channelTypeString,
                uint serverWindowSize,
                uint serverMaxPacketSize) {

            _packetSender = packetSender;
            _protocolEventManager = protocolEventManager;
            LocalChannel = localChannel;
            RemoteChannel = remoteChannel;
            ChannelType = channelType;
            ChannelTypeString = channelTypeString;

            _localMaxPacketSize = param.MaxPacketSize;
            _localWindowSize = _localWindowSizeLeft = param.WindowSize;
            _serverMaxPacketSize = serverMaxPacketSize;
            _serverWindowSizeLeft = serverWindowSize;

            _state = State.InitiatedByServer; // SendOpenConfirmation() will change state to "Opened"
        }

        /// <summary>
        /// Constructor (initiated by client)
        /// </summary>
        public SSH2ChannelBase(
                IPacketSender<SSH2Packet> packetSender,
                SSHConnectionParameter param,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                ChannelType channelType,
                string channelTypeString) {

            _packetSender = packetSender;
            _protocolEventManager = protocolEventManager;
            LocalChannel = localChannel;
            RemoteChannel = 0;
            ChannelType = channelType;
            ChannelTypeString = channelTypeString;

            _localMaxPacketSize = param.MaxPacketSize;
            _localWindowSize = _localWindowSizeLeft = param.WindowSize;
            _serverMaxPacketSize = 0;
            _serverWindowSizeLeft = 0;

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
        /// Sends SSH_MSG_CHANNEL_REQUEST packet (no response)
        /// </summary>
        /// <param name="requestPacket">SSH_MSG_CHANNEL_REQUEST packet</param>
        protected void SendRequest(SSH2Packet requestPacket) {
            lock (_channelRequestStatusSync) {
                SpinWait.SpinUntil(() => _channelRequestStatus == false);
                _channelRequestStatus = true;
            }
            try {
                Transmit(0, requestPacket);
            }
            finally {
                lock (_channelRequestStatusSync) {
                    _channelRequestStatus = false;
                }
            }
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_REQUEST packet and wait response asynchronously.
        /// </summary>
        /// <param name="requestPacket">SSH_MSG_CHANNEL_REQUEST packet</param>
        /// <returns>asynchronous task that wait for the response</returns>
        protected Task<ChannelRequestResult> SendRequestAndWaitResponseAsync(SSH2Packet requestPacket) {
            lock (_channelRequestStatusSync) {
                SpinWait.SpinUntil(() => _channelRequestStatus == false);
                _channelRequestStatus = true;
            }

            try {
                _channelRequestResult.Clear();
                Transmit(0, requestPacket);
                return WaitResponseAsync();
            }
            finally {
                lock (_channelRequestStatusSync) {
                    _channelRequestStatus = false;
                }
            }
        }

        private Task<ChannelRequestResult> WaitResponseAsync() {
            const int RESPONSE_TIMEOUT = 10000;
            return Task<ChannelRequestResult>.Run(() => {
                bool result = false;
                if (_channelRequestResult.TryGet(ref result, RESPONSE_TIMEOUT)) {
                    return result ? ChannelRequestResult.Success : ChannelRequestResult.Failure;
                }
                else {
                    return ChannelRequestResult.Timeout;
                }
            });
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
        public bool IsOpen {
            get {
                return _state != State.Closed;
            }
        }

        /// <summary>
        /// true if this channel is ready for use.
        /// </summary>
        public bool IsReady {
            get {
                return _state == State.Ready;
            }
        }

        /// <summary>
        /// Maximum size of the channel data which can be sent with a single SSH packet.
        /// </summary>
        /// <remarks>
        /// In SSH2, this size will be determined from the maximum packet size specified by the server.
        /// </remarks>
        public int MaxChannelDatagramSize {
            get {
                if (_maxDatagramSize == null) {
                    if (_serverMaxPacketSize == 0) {
                        return MIN_DATAGRAM_SIZE;
                    }
                    _maxDatagramSize =
                        (_serverMaxPacketSize < CHANNEL_DATA_INFO_SIZE) ?
                            MIN_DATAGRAM_SIZE :
                            (int)Math.Max(Math.Min(_serverMaxPacketSize - CHANNEL_DATA_INFO_SIZE, MAX_DATAGRAM_SIZE), MIN_DATAGRAM_SIZE);
                }
                return _maxDatagramSize.Value;
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
        public void ResizeTerminal(uint width, uint height, uint pixelWidth, uint pixelHeight) {
            lock (_stateSync) {
                if (_state == State.Closing || _state == State.Closed) {
                    throw new SSHChannelInvalidOperationException("Channel already closed");
                }

                SendRequest(
                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_REQUEST)
                        .WriteUInt32(RemoteChannel)
                        .WriteString("window-change")
                        .WriteBool(false)
                        .WriteUInt32(width)
                        .WriteUInt32(height)
                        .WriteUInt32(pixelWidth)
                        .WriteUInt32(pixelHeight)
                );
            }

            _protocolEventManager.Trace(
                "CH[{0}] window-change : width={1} height={2} pixelWidth={3} pixelHeight={4}",
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
        public bool WaitReady() {
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
        public void Send(DataFragment data) {
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
                        datagramSize,
                        new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_DATA)
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
        public void SendEOF() {
            lock (_stateSync) {
                if (_state == State.Closing || _state == State.Closed) {
                    throw new SSHChannelInvalidOperationException("Channel already closed");
                }

                Transmit(
                    0,
                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_EOF)
                        .WriteUInt32(RemoteChannel)
                );
            }

            _protocolEventManager.Trace("CH[{0}] send EOF", LocalChannel);
        }

        /// <summary>
        /// Send Break. (SSH2, session channel only)
        /// </summary>
        /// <param name="breakLength">break-length in milliseconds</param>
        /// <returns>true if succeeded. false if the request failed.</returns>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public virtual bool SendBreak(int breakLength) {
            // derived class can override this.
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
        public void Close() {
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

            _handler.OnClosing(false);

            Transmit(
                0,
                new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_CLOSE)
                    .WriteUInt32(RemoteChannel)
            );

            _protocolEventManager.Trace("CH[{0}] SSH_MSG_CHANNEL_CLOSE", LocalChannel);
        }

        #endregion

        /// <summary>
        /// Sets handler
        /// </summary>
        /// <param name="handler"></param>
        public void SetHandler(ISSHChannelEventHandler handler) {
            _handler = new SSHChannelEventHandlerIgnoreErrorWrapper(handler);
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_OPEN
        /// </summary>
        public void SendOpen() {
            Transmit(0, BuildOpenPacket());
            _protocolEventManager.Trace(
                "CH[{0}] SSH_MSG_CHANNEL_OPEN channelType={1} windowSize={2} maxPacketSize={3}",
                LocalChannel, ChannelTypeString, _localWindowSize, _localMaxPacketSize);
        }

        /// <summary>
        /// Builds SSH_MSG_CHANNEL_OPEN packet
        /// </summary>
        protected virtual SSH2Packet BuildOpenPacket() {
            return new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_OPEN)
                        .WriteString(ChannelTypeString)
                        .WriteUInt32(LocalChannel)
                        .WriteInt32(_localWindowSize)
                        .WriteInt32(_localMaxPacketSize);
        }

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
                    0,
                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_OPEN_CONFIRMATION)
                        .WriteUInt32(RemoteChannel)
                        .WriteUInt32(LocalChannel)
                        .WriteInt32(_localWindowSize)
                        .WriteInt32(_localMaxPacketSize)
                );

                _state = State.Established;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

            _protocolEventManager.Trace(
                "CH[{0}] SSH_MSG_CHANNEL_OPEN_CONFIRMATION windowSize={1} maxPacketSize={2}",
                LocalChannel, _localWindowSize, _localMaxPacketSize);

            _handler.OnEstablished(new DataFragment(0));
            OnChannelEstablished();
        }

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
                if (_state == State.Established) {
                    _state = State.Ready;
                    Monitor.PulseAll(_stateSync);   // notifies state change
                }
            }
            _handler.OnReady();
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
            _handler.OnRequestFailed();
            Close();
            return;

        SetStateClosedByClient:
            _handler.OnRequestFailed();
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

            _handler.OnClosing(byServer);

            lock (_stateSync) {
                if (_state == State.Closed) {   // state transition has occurred in OnClosing()
                    return;
                }
                _state = State.Closed;
                Monitor.PulseAll(_stateSync);   // notifies state change
            }

        OnClosed:
            _handler.OnClosed(byServer);

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
        protected virtual SubPacketProcessResult ProcessPacketSub(SSH2PacketType packetType, DataFragment packetFragment) {
            // derived class can override this.
            return SubPacketProcessResult.NotConsumed;
        }

        /// <summary>
        /// Process packet about this channel.
        /// </summary>
        /// <param name="packetType">a packet type (message number)</param>
        /// <param name="packetFragment">a packet image except message number and recipient channel.</param>
        public void ProcessPacket(SSH2PacketType packetType, DataFragment packetFragment) {
            if (_state == State.Closed) {
                return; // ignore
            }

            DataFragment dataFragmentArg;
            uint dataTypeCodeArg;

            lock (_stateSync) {
                switch (_state) {
                    case State.InitiatedByServer:
                        break;
                    case State.InitiatedByClient:
                        if (packetType == SSH2PacketType.SSH_MSG_CHANNEL_OPEN_CONFIRMATION) {
                            SSH2DataReader reader = new SSH2DataReader(packetFragment);
                            RemoteChannel = reader.ReadUInt32();
                            _serverWindowSizeLeft = reader.ReadUInt32();
                            _serverMaxPacketSize = reader.ReadUInt32();

                            _state = State.Established;
                            Monitor.PulseAll(_stateSync);   // notifies state change
                            dataFragmentArg = reader.GetRemainingDataView();
                            goto OnEstablished; // do it out of the lock block
                        }
                        if (packetType == SSH2PacketType.SSH_MSG_CHANNEL_OPEN_FAILURE) {
                            SSH2DataReader reader = new SSH2DataReader(packetFragment);
                            uint reasonCode = reader.ReadUInt32();
                            string description = reader.ReadUTF8String();
                            string lang = reader.ReadString();
                            goto RequestFailed; // do it out of the lock block
                        }
                        break;
                    case State.Closing:
                        if (packetType == SSH2PacketType.SSH_MSG_CHANNEL_CLOSE) {
                            goto SetStateClosedByClient;    // do it out of the lock block
                        }
                        break;
                    case State.Established:
                    case State.Ready:
                        if (ProcessPacketSub(packetType, packetFragment) == SubPacketProcessResult.Consumed) {
                            return;
                        }
                        switch (packetType) {
                            case SSH2PacketType.SSH_MSG_CHANNEL_DATA: {
                                    SSH2DataReader reader = new SSH2DataReader(packetFragment);
                                    int len = reader.ReadInt32();
                                    dataFragmentArg = reader.GetRemainingDataView(len);
                                    AdjustWindowSize(len);
                                }
                                goto OnData;    // do it out of the lock block
                            case SSH2PacketType.SSH_MSG_CHANNEL_EXTENDED_DATA: {
                                    SSH2DataReader reader = new SSH2DataReader(packetFragment);
                                    dataTypeCodeArg = reader.ReadUInt32();
                                    int len = reader.ReadInt32();
                                    dataFragmentArg = reader.GetRemainingDataView(len);
                                    AdjustWindowSize(len);
                                }
                                goto OnExtendedData;    // do it out of the lock block
                            case SSH2PacketType.SSH_MSG_CHANNEL_REQUEST: {
                                    SSH2DataReader reader = new SSH2DataReader(packetFragment);
                                    string request = reader.ReadString();
                                    bool wantReply = reader.ReadBool();
                                    if (wantReply) { //we reject unknown requests including keep-alive check
                                        Transmit(
                                            0,
                                            new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_FAILURE)
                                                .WriteUInt32(RemoteChannel)
                                        );
                                    }
                                }
                                break;
                            case SSH2PacketType.SSH_MSG_CHANNEL_EOF:
                                goto OnEOF; // do it out of the lock block
                            case SSH2PacketType.SSH_MSG_CHANNEL_CLOSE:
                                Transmit(
                                    0,
                                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_CLOSE)
                                        .WriteUInt32(RemoteChannel)
                                );
                                goto SetStateClosedByServer;    // do it out of the lock block
                            case SSH2PacketType.SSH_MSG_CHANNEL_WINDOW_ADJUST: {
                                    SSH2DataReader reader = new SSH2DataReader(packetFragment);
                                    uint bytesToAdd = reader.ReadUInt32();
                                    // some servers may not send SSH_MSG_CHANNEL_WINDOW_ADJUST.
                                    // it is dangerous to wait this message in send procedure
                                    _serverWindowSizeLeft += bytesToAdd;
                                }
                                goto OnWindowAdjust;
                            case SSH2PacketType.SSH_MSG_CHANNEL_SUCCESS:
                            case SSH2PacketType.SSH_MSG_CHANNEL_FAILURE: {
                                    _channelRequestResult.TrySet(packetType == SSH2PacketType.SSH_MSG_CHANNEL_SUCCESS, 1000);
                                }
                                break;
                            default:
                                goto OnUnhandledPacket;
                        }
                        break;  // case State.Ready
                }
            }

            return;

        OnEstablished:
            _protocolEventManager.Trace(
                "CH[{0}] remoteCH={1} remoteWindowSize={2} remoteMaxPacketSize={3}",
                LocalChannel, RemoteChannel, _serverWindowSizeLeft, _serverMaxPacketSize);
            _handler.OnEstablished(dataFragmentArg);
            OnChannelEstablished();
            return;

        RequestFailed:
            _protocolEventManager.Trace("CH[{0}] request failed", LocalChannel);
            RequestFailed();
            return;

        SetStateClosedByClient:
            _protocolEventManager.Trace("CH[{0}] closed completely", LocalChannel);
            SetStateClosed(false);
            return;

        SetStateClosedByServer:
            _protocolEventManager.Trace("CH[{0}] closed by server", LocalChannel);
            SetStateClosed(true);
            return;

        OnData:
            _handler.OnData(dataFragmentArg);
            return;

        OnExtendedData:
            _handler.OnExtendedData(dataTypeCodeArg, dataFragmentArg);
            return;

        OnEOF:
            _protocolEventManager.Trace("CH[{0}] caught EOF", LocalChannel);
            _handler.OnEOF();
            return;

        OnWindowAdjust:
            _protocolEventManager.Trace(
                "CH[{0}] adjusted remote window size to {1}",
                LocalChannel, _serverWindowSizeLeft);
            return;

        OnUnhandledPacket:
            _handler.OnUnhandledPacket((byte)packetType, packetFragment);
            return;

        }

        /// <summary>
        /// Sends a packet.
        /// </summary>
        /// <param name="consumedSize">consumed window size.</param>
        /// <param name="packet">packet object</param>
        protected void Transmit(int consumedSize, SSH2Packet packet) {
            if (_serverWindowSizeLeft < (uint)consumedSize) {
                // FIXME: currently, window size on the remote side is totally ignored...
                _serverWindowSizeLeft = 0;
            }
            else {
                _serverWindowSizeLeft -= (uint)consumedSize;
            }

            _packetSender.Send(packet);
        }

        /// <summary>
        /// Outputs Trace message.
        /// </summary>
        /// <param name="format">format string</param>
        /// <param name="args">format arguments</param>
        protected void Trace(string format, params object[] args) {
            _protocolEventManager.Trace(format, args);
        }

        /// <summary>
        /// Adjust window size.
        /// </summary>
        /// <param name="dataLength">consumed length.</param>
        private void AdjustWindowSize(int dataLength) {
            _localWindowSizeLeft = Math.Max(_localWindowSizeLeft - dataLength, 0);

            if (_localWindowSizeLeft < _localWindowSize / 2) {
                Transmit(
                    0,
                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_WINDOW_ADJUST)
                        .WriteUInt32(RemoteChannel)
                        .WriteInt32(_localWindowSize - _localWindowSizeLeft)
                );

                _protocolEventManager.Trace(
                    "CH[{0}] adjusted local window size : {1} --> {2}",
                    LocalChannel, _localWindowSizeLeft, _localWindowSize);

                _localWindowSizeLeft = _localWindowSize;
            }
        }

        /// <summary>
        /// Notifies terminating this channel.
        /// </summary>
        protected void Die() {
            if (Died != null) {
                Died(this);
            }
        }

        #endregion
    }

    /// <summary>
    /// SSH2 channel operator for the session.
    /// </summary>
    internal class SSH2SessionChannel : SSH2ChannelBase {
        #region

        private const ChannelType CHANNEL_TYPE = ChannelType.Session;
        private const string CHANNEL_TYPE_STRING = "session";

        /// <summary>
        /// Constructor (initiated by client)
        /// </summary>
        public SSH2SessionChannel(
                IPacketSender<SSH2Packet> packetSender,
                SSHConnectionParameter param,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel)
            : base(packetSender, param, protocolEventManager, localChannel, CHANNEL_TYPE, CHANNEL_TYPE_STRING) {
        }

        /// <summary>
        /// Send Break. (SSH2, session channel only)
        /// </summary>
        /// <param name="breakLength">break-length in milliseconds</param>
        /// <returns>true if succeeded. false if the request failed.</returns>
        /// <exception cref="SSHChannelInvalidOperationException">the channel is already closed.</exception>
        public override bool SendBreak(int breakLength) {
            if (MajorState == State.Closing || MajorState == State.Closed) {
                throw new SSHChannelInvalidOperationException("Channel already closed");
            }

            var reqTask =
                SendRequestAndWaitResponseAsync(
                            new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_REQUEST)
                                .WriteUInt32(RemoteChannel)
                                .WriteString("break")
                                .WriteBool(true)
                                .WriteInt32(breakLength)
                );

            Trace("CH[{0}] break : breakLength={1}", LocalChannel, breakLength);

            var result = reqTask.Result;

            return result == ChannelRequestResult.Success;
        }

        #endregion
    }

    /// <summary>
    /// SSH2 channel operator for the shell.
    /// </summary>
    internal class SSH2ShellChannel : SSH2SessionChannel {
        #region

        private enum MinorState {
            /// <summary>initial state</summary>
            NotReady,
            /// <summary>waiting SSH_MSG_CHANNEL_SUCCESS | SSH_MSG_CHANNEL_FAILURE for "pty-req" request</summary>
            WaitPtyReqConfirmation,
            /// <summary>waiting SSH_MSG_CHANNEL_SUCCESS | SSH_MSG_CHANNEL_FAILURE for "auth-agent-req@openssh.com" request</summary>
            WaitAuthAgentReqConfirmation,
            /// <summary>waiting SSH_MSG_CHANNEL_SUCCESS | SSH_MSG_CHANNEL_FAILURE for "x11-req" request</summary>
            WaitX11ReqConfirmation,
            /// <summary>waiting SSH_MSG_CHANNEL_SUCCESS | SSH_MSG_CHANNEL_FAILURE for "shell" request</summary>
            WaitShellConfirmation,
            /// <summary></summary>
            Ready,
        }

        private readonly SSHConnectionParameter _param;
        // X11 connection manager (null if X11 forwarding is inactive)
        private readonly X11ConnectionManager _x11ConnectionManager;

        private volatile MinorState _state;
        private readonly object _stateSync = new object();

        /// <summary>
        /// Constructor (initiated by client)
        /// </summary>
        public SSH2ShellChannel(
                IPacketSender<SSH2Packet> packetSender,
                SSHConnectionParameter param,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                X11ConnectionManager x11ConnectionManager)
            : base(packetSender, param, protocolEventManager, localChannel) {
            _param = param;
            _x11ConnectionManager = x11ConnectionManager;
        }

        /// <summary>
        /// Do additional work when <see cref="SSH2ChannelBase.State"/> was changed to <see cref="SSH2ChannelBase.State.Established"/>.
        /// </summary>
        /// <returns>true if the channel is ready for use.</returns>
        protected override void OnChannelEstablished() {
            Task.Run(() => {
                var reqPtyResult = SendPtyRequest();
                if (!reqPtyResult) {
                    return;
                }

                var reqAuthAgentResult = SendAuthAgentRequest();
                if (!reqAuthAgentResult) {
                    return;
                }

                var reqX11Result = SendX11Request();
                if (!reqX11Result) {
                    return;
                }

                var reqShellResult = SendShellRequest();
                if (!reqShellResult) {
                    return;
                }
            });
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_REQUEST "pty-req"
        /// </summary>
        private bool SendPtyRequest() {
            lock (_stateSync) {
                if (_state != MinorState.NotReady) {
                    return false;
                }
                _state = MinorState.WaitPtyReqConfirmation;
            }

            var reqTask =
                SendRequestAndWaitResponseAsync(
                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_REQUEST)
                        .WriteUInt32(RemoteChannel)
                        .WriteString("pty-req")
                        .WriteBool(true)
                        .WriteString(_param.TerminalName)
                        .WriteInt32(_param.TerminalWidth)
                        .WriteInt32(_param.TerminalHeight)
                        .WriteInt32(_param.TerminalPixelWidth)
                        .WriteInt32(_param.TerminalPixelHeight)
                        .WriteAsString(new byte[0])
                );

            Trace("CH[{0}] pty-req : term={1} width={2} height={3} pixelWidth={4} pixelHeight={5}",
                LocalChannel, _param.TerminalName,
                _param.TerminalWidth, _param.TerminalHeight,
                _param.TerminalPixelWidth, _param.TerminalPixelHeight);

            var result = reqTask.Result;

            lock (_stateSync) {
                if (_state != MinorState.WaitPtyReqConfirmation) {
                    return false;
                }
                if (result == ChannelRequestResult.Success) {
                    return true;    // do the next task
                }
                else {
                    _state = MinorState.NotReady;
                    goto RequestFailed;
                }
            }

        RequestFailed:
            RequestFailed();
            return false;
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_REQUEST "auth-agent-req@openssh.com"
        /// </summary>
        private bool SendAuthAgentRequest() {
            if (_param.AgentForwardingAuthKeyProvider == null) {
                return true;    // do the next task
            }

            lock (_stateSync) {
                _state = MinorState.WaitAuthAgentReqConfirmation;
            }

            var reqTask =
                SendRequestAndWaitResponseAsync(
                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_REQUEST)
                        .WriteUInt32(RemoteChannel)
                        .WriteString("auth-agent-req@openssh.com")
                        .WriteBool(true)
                );

            Trace("CH[{0}] auth-agent-req@openssh.com", LocalChannel);

            var result = reqTask.Result;

            lock (_stateSync) {
                if (_state != MinorState.WaitAuthAgentReqConfirmation) {
                    return false;
                }
                if (result == ChannelRequestResult.Success) {
                    Trace("CH[{0}] the request of the agent forwarding has been accepted.", LocalChannel);
                    return true;    // do the next task
                }
                else {
                    Trace("CH[{0}] the request of the agent forwarding has been rejected.", LocalChannel);
                    _state = MinorState.NotReady;
                    goto RequestFailed;
                }
            }

        RequestFailed:
            RequestFailed();
            return false;
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_REQUEST "x11-req"
        /// </summary>
        private bool SendX11Request() {
            if (_param.X11ForwardingParams == null || _x11ConnectionManager == null) {
                return true;    // do the next task
            }

            lock (_stateSync) {
                _state = MinorState.WaitX11ReqConfirmation;
            }

            var reqTask =
                SendRequestAndWaitResponseAsync(
                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_REQUEST)
                        .WriteUInt32(RemoteChannel)
                        .WriteString("x11-req")
                        .WriteBool(true)
                        .WriteBool(false)   // not single-connection
                        .WriteString(_x11ConnectionManager.SpoofedAuthProtocolName)
                        .WriteString(_x11ConnectionManager.SpoofedAuthProtocolDataHex)
                        .WriteUInt32((uint)_x11ConnectionManager.Params.Screen) // screen number
                );

            Trace("CH[{0}] x11-req", LocalChannel);

            var result = reqTask.Result;

            lock (_stateSync) {
                if (_state != MinorState.WaitX11ReqConfirmation) {
                    return false;
                }
                if (result == ChannelRequestResult.Success) {
                    Trace("CH[{0}] the request of the X11 forwarding has been accepted.", LocalChannel);
                    return true;    // do the next task
                }
                else {
                    Trace("CH[{0}] the request of the X11 forwarding has been rejected.", LocalChannel);
                    _state = MinorState.NotReady;
                    goto RequestFailed;
                }
            }

        RequestFailed:
            RequestFailed();
            return false;
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_REQUEST "shell"
        /// </summary>
        private bool SendShellRequest() {
            lock (_stateSync) {
                _state = MinorState.WaitShellConfirmation;
            }

            var reqTask =
                SendRequestAndWaitResponseAsync(
                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_REQUEST)
                        .WriteUInt32(RemoteChannel)
                        .WriteString("shell")
                        .WriteBool(true)
                );

            Trace("CH[{0}] shell", LocalChannel);

            var result = reqTask.Result;

            lock (_stateSync) {
                if (_state != MinorState.WaitShellConfirmation) {
                    return false;
                }
                if (result == ChannelRequestResult.Success) {
                    _state = MinorState.Ready;
                    goto SetStateReady;
                }
                else {
                    _state = MinorState.NotReady;
                    goto RequestFailed;
                }
            }

        SetStateReady:
            SetStateReady();
            return true;

        RequestFailed:
            RequestFailed();
            return false;
        }

        #endregion
    }

    /// <summary>
    /// SSH2 channel operator for the command execution.
    /// </summary>
    internal class SSH2ExecChannel : SSH2SessionChannel {
        #region

        private enum MinorState {
            /// <summary>initial state</summary>
            NotReady,
            /// <summary>waiting SSH_MSG_CHANNEL_SUCCESS | SSH_MSG_CHANNEL_FAILURE for "exec" request</summary>
            WaitExecConfirmation,
            /// <summary></summary>
            Ready,
        }

        private readonly string _command;

        private volatile MinorState _state;
        private readonly object _stateSync = new object();

        /// <summary>
        /// Constructor (initiated by client)
        /// </summary>
        public SSH2ExecChannel(
                IPacketSender<SSH2Packet> packetSender,
                SSHConnectionParameter param,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                string command)
            : base(packetSender, param, protocolEventManager, localChannel) {

            _command = command;
        }

        /// <summary>
        /// Do additional work when <see cref="SSH2ChannelBase.State"/> was changed to <see cref="SSH2ChannelBase.State.Established"/>.
        /// </summary>
        /// <returns>true if the channel is ready for use.</returns>
        protected override void OnChannelEstablished() {
            Task.Run(() => SendExecRequest());
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_REQUEST "exec"
        /// </summary>
        private void SendExecRequest() {
            lock (_stateSync) {
                if (_state != MinorState.NotReady) {
                    return;
                }
                _state = MinorState.WaitExecConfirmation;
            }

            var reqTask =
                SendRequestAndWaitResponseAsync(
                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_REQUEST)
                        .WriteUInt32(RemoteChannel)
                        .WriteString("exec")
                        .WriteBool(true)
                        .WriteString(_command)
                );

            Trace("CH[{0}] exec : command={1}", LocalChannel, _command);

            var result = reqTask.Result;

            lock (_stateSync) {
                if (_state != MinorState.WaitExecConfirmation) {
                    return;
                }
                if (result == ChannelRequestResult.Success) {
                    _state = MinorState.Ready;
                    goto SetStateReady;
                }
                else {
                    _state = MinorState.NotReady;
                    goto RequestFailed;
                }
            }

        SetStateReady:
            SetStateReady();
            return;

        RequestFailed:
            RequestFailed();
            return;
        }

        #endregion
    }

    /// <summary>
    /// SSH2 channel operator for the subsystem.
    /// </summary>
    internal class SSH2SubsystemChannel : SSH2SessionChannel {
        #region

        private enum MinorState {
            /// <summary>initial state</summary>
            NotReady,
            /// <summary>waiting SSH_MSG_CHANNEL_SUCCESS | SSH_MSG_CHANNEL_FAILURE for "subsystem" request</summary>
            WaitSubsystemConfirmation,
            /// <summary></summary>
            Ready,
        }

        private readonly string _subsystemName;

        private volatile MinorState _state;
        private readonly object _stateSync = new object();

        /// <summary>
        /// Constructor (initiated by client)
        /// </summary>
        public SSH2SubsystemChannel(
                IPacketSender<SSH2Packet> packetSender,
                SSHConnectionParameter param,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                string subsystemName)
            : base(packetSender, param, protocolEventManager, localChannel) {

            _subsystemName = subsystemName;
        }

        /// <summary>
        /// Do additional work when <see cref="SSH2ChannelBase.State"/> was changed to <see cref="SSH2ChannelBase.State.Established"/>.
        /// </summary>
        /// <returns>true if the channel is ready for use.</returns>
        protected override void OnChannelEstablished() {
            Task.Run(() => SendSubsystemRequest());
        }

        /// <summary>
        /// Sends SSH_MSG_CHANNEL_REQUEST "subsystem"
        /// </summary>
        private void SendSubsystemRequest() {
            lock (_stateSync) {
                if (_state != MinorState.NotReady) {
                    return;
                }
                _state = MinorState.WaitSubsystemConfirmation;
            }

            var reqTask =
                SendRequestAndWaitResponseAsync(
                    new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_REQUEST)
                        .WriteUInt32(RemoteChannel)
                        .WriteString("subsystem")
                        .WriteBool(true)
                        .WriteString(_subsystemName)
                );

            Trace("CH[{0}] subsystem : subsystem={1}", LocalChannel, _subsystemName);

            var result = reqTask.Result;

            lock (_stateSync) {
                if (_state != MinorState.WaitSubsystemConfirmation) {
                    return;
                }
                if (result == ChannelRequestResult.Success) {
                    _state = MinorState.Ready;
                    goto SetStateReady;
                }
                else {
                    _state = MinorState.NotReady;
                    goto RequestFailed;
                }
            }

        SetStateReady:
            SetStateReady();
            return;

        RequestFailed:
            RequestFailed();
            return;
        }

        #endregion
    }

    /// <summary>
    /// SSH2 channel operator for the local port forwarding.
    /// </summary>
    internal class SSH2LocalPortForwardingChannel : SSH2ChannelBase {
        #region

        private const ChannelType CHANNEL_TYPE = ChannelType.ForwardedLocalToRemote;
        private const string CHANNEL_TYPE_STRING = "direct-tcpip";

        private readonly int _localWindowSize;
        private readonly int _localMaxPacketSize;
        private readonly string _remoteHost;
        private readonly uint _remotePort;
        private readonly string _originatorIp;
        private readonly uint _originatorPort;

        /// <summary>
        /// Constructor (initiated by client)
        /// </summary>
        public SSH2LocalPortForwardingChannel(
                IPacketSender<SSH2Packet> packetSender,
                SSHConnectionParameter param,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                string remoteHost,
                uint remotePort,
                string originatorIp,
                uint originatorPort)
            : base(packetSender, param, protocolEventManager, localChannel, CHANNEL_TYPE, CHANNEL_TYPE_STRING) {

            _localWindowSize = param.WindowSize;
            _localMaxPacketSize = param.WindowSize;
            _remoteHost = remoteHost;
            _remotePort = remotePort;
            _originatorIp = originatorIp;
            _originatorPort = originatorPort;
        }

        /// <summary>
        /// Builds SSH_MSG_CHANNEL_OPEN packet
        /// </summary>
        protected override SSH2Packet BuildOpenPacket() {
            return new SSH2Packet(SSH2PacketType.SSH_MSG_CHANNEL_OPEN)
                    .WriteString("direct-tcpip")
                    .WriteUInt32(LocalChannel)
                    .WriteInt32(_localWindowSize)
                    .WriteInt32(_localMaxPacketSize)
                    .WriteString(_remoteHost)
                    .WriteUInt32(_remotePort)
                    .WriteString(_originatorIp)
                    .WriteUInt32(_originatorPort);
        }

        #endregion
    }

    /// <summary>
    /// SSH2 channel operator for the remote port forwarding.
    /// </summary>
    internal class SSH2RemotePortForwardingChannel : SSH2ChannelBase {
        #region

        private const ChannelType CHANNEL_TYPE = ChannelType.ForwardedRemoteToLocal;
        private const string CHANNEL_TYPE_STRING = "forwarded-tcpip";

        /// <summary>
        /// Constructor (initiated by server)
        /// </summary>
        public SSH2RemotePortForwardingChannel(
                IPacketSender<SSH2Packet> packetSender,
                SSHConnectionParameter param,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                uint remoteChannel,
                uint serverWindowSize,
                uint serverMaxPacketSize)
            : base(packetSender, param, protocolEventManager, localChannel, remoteChannel, CHANNEL_TYPE, CHANNEL_TYPE_STRING, serverWindowSize, serverMaxPacketSize) {
        }

        #endregion
    }

    /// <summary>
    /// SSH2 channel operator for OpenSSH's agent forwarding.
    /// </summary>
    internal class SSH2OpenSSHAgentForwardingChannel : SSH2ChannelBase {
        #region

        private const ChannelType CHANNEL_TYPE = ChannelType.AgentForwarding;
        private const string CHANNEL_TYPE_STRING = "auth-agent@openssh.com";

        /// <summary>
        /// Constructor (initiated by server)
        /// </summary>
        public SSH2OpenSSHAgentForwardingChannel(
                IPacketSender<SSH2Packet> packetSender,
                SSHConnectionParameter param,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                uint remoteChannel,
                uint serverWindowSize,
                uint serverMaxPacketSize)
            : base(packetSender, param, protocolEventManager, localChannel, remoteChannel, CHANNEL_TYPE, CHANNEL_TYPE_STRING, serverWindowSize, serverMaxPacketSize) {
        }

        #endregion
    }

    /// <summary>
    /// SSH2 channel operator for the X11 forwarding.
    /// </summary>
    internal class SSH2X11ForwardingChannel : SSH2ChannelBase {
        #region

        private const ChannelType CHANNEL_TYPE = ChannelType.X11Forwarding;
        private const string CHANNEL_TYPE_STRING = "x11";

        /// <summary>
        /// Constructor (initiated by server)
        /// </summary>
        public SSH2X11ForwardingChannel(
                IPacketSender<SSH2Packet> packetSender,
                SSHConnectionParameter param,
                SSHProtocolEventManager protocolEventManager,
                uint localChannel,
                uint remoteChannel,
                uint serverWindowSize,
                uint serverMaxPacketSize)
            : base(packetSender, param, protocolEventManager, localChannel, remoteChannel, CHANNEL_TYPE, CHANNEL_TYPE_STRING, serverWindowSize, serverMaxPacketSize) {
        }

        #endregion
    }

}
