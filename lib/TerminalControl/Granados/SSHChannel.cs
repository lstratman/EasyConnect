// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

//#define DEBUG_REPORT_SSHCHANNELS

namespace Granados {
    using Granados.IO;
    using System;

    /// <summary>
    /// An interface of the class that can send data through the specific channel.
    /// </summary>
    /// <remarks>
    /// The concrete class is provided by the Granados.
    /// </remarks>
    public interface ISSHChannel {

        /// <summary>
        /// Local channel number.
        /// </summary>
        uint LocalChannel {
            get;
        }

        /// <summary>
        /// Remote channel number.
        /// </summary>
        uint RemoteChannel {
            get;
        }

        /// <summary>
        /// Channel type. (predefined type) 
        /// </summary>
        ChannelType ChannelType {
            get;
        }

        /// <summary>
        /// Channel type string. (actual channel type name)
        /// </summary>
        string ChannelTypeString {
            get;
        }

        /// <summary>
        /// true if this channel is open.
        /// </summary>
        bool IsOpen {
            get;
        }

        /// <summary>
        /// true if this channel is ready for use.
        /// </summary>
        bool IsReady {
            get;
        }

        /// <summary>
        /// Maximum size of the channel data which can be sent with a single SSH packet.
        /// </summary>
        /// <remarks>
        /// In SSH2, this size will be determined from the maximum packet size specified by the server.
        /// In SSH1, this size will be determined from the maximum packet size specified in the protocol specification.
        /// </remarks>
        int MaxChannelDatagramSize {
            get;
        }

        /// <summary>
        /// Send window dimension change message.
        /// </summary>
        /// <remarks>
        /// In SSH1's interactive-session, this method will send SSH_CMSG_WINDOW_SIZE packet.
        /// In SSH1's other channel, this method will be ignored.
        /// In SSH2, this method will send SSH_MSG_CHANNEL_REQUEST "window-change" packet.
        /// </remarks>
        /// <param name="width">terminal width, columns</param>
        /// <param name="height">terminal height, rows</param>
        /// <param name="pixelWidth">terminal width, pixels</param>
        /// <param name="pixelHeight">terminal height, pixels</param>
        /// <exception cref="SSHChannelInvalidOperationException">the operation is not allowed.</exception>
        void ResizeTerminal(uint width, uint height, uint pixelWidth, uint pixelHeight);

        /// <summary>
        /// Block execution of the current thread until the channel is ready for the communication.
        /// </summary>
        /// <returns>
        /// true if this channel is ready for the communication.<br/>
        /// false if this channel failed to open.<br/>
        /// false if this channel is going to close or is already closed.
        /// </returns>
        bool WaitReady();

        /// <summary>
        /// Send data.
        /// </summary>
        /// <remarks>
        /// In SSH1's interactive-session, this method will send SSH_CMSG_STDIN_DATA packet.
        /// Otherwise, this method will send SSH_MSG_CHANNEL_DATA packet.
        /// </remarks>
        /// <param name="data">data to send</param>
        /// <exception cref="SSHChannelInvalidOperationException">the operation is not allowed.</exception>
        void Send(DataFragment data);

        /// <summary>
        /// Send EOF.
        /// </summary>
        /// <remarks>
        /// In SSH1's interactive-session, this method will send SSH_CMSG_EOF packet.
        /// In SSH1's other channel, this method will be ignored.
        /// In SSH2, this method will send SSH_MSG_CHANNEL_EOF packet.
        /// </remarks>
        /// <exception cref="SSHChannelInvalidOperationException">the operation is not allowed.</exception>
        void SendEOF();

        /// <summary>
        /// Send Break. (SSH2, session channel only)
        /// </summary>
        /// <param name="breakLength">break-length in milliseconds</param>
        /// <returns>true if succeeded. false if the request failed.</returns>
        /// <exception cref="SSHChannelInvalidOperationException">the operation is not allowed.</exception>
        bool SendBreak(int breakLength);

        /// <summary>
        /// Close this channel.
        /// </summary>
        /// <remarks>
        /// After calling this method, all mothods of the <see cref="ISSHChannel"/> will throw <see cref="SSHChannelInvalidOperationException"/>.
        /// </remarks>
        /// <remarks>
        /// If this method was called under the inappropriate channel state, the method call will be ignored silently.
        /// </remarks>
        void Close();
    }

    /// <summary>
    /// An interface of the channel object that can handle events about the specific channel.
    /// </summary>
    /// <remarks>
    /// The user of Granados needs to implement the concrete class of this interface.<br/>
    /// <see cref="SimpleSSHChannelEventHandler"/> is a good start.
    /// </remarks>
    public interface ISSHChannelEventHandler : IDisposable {

        // <>---+---> OnEstablished --+---> OnReady --->| OnData            | ----+
        //      |                     |                 | OnExtendedData    |     |
        //      |                     |                 | OnEOF             |     |
        //      |                     |                 | OnUnhandledPacket |     |
        //      |                     |                                           |
        //      +---------------------+---> OnRequestFailed ----------------------+---> OnClosing --> OnClosed

        /// <summary>
        /// Notifies that the channel has been established.
        /// </summary>
        /// <param name="data">channel type specific data</param>
        void OnEstablished(DataFragment data);

        /// <summary>
        /// Notifies that the channel is ready for use.
        /// </summary>
        void OnReady();

        /// <summary>
        /// Notifies received channel data. (SSH_MSG_CHANNEL_DATA)
        /// </summary>
        /// <param name="data">data fragment</param>
        void OnData(DataFragment data);

        /// <summary>
        /// Notifies received extended channel data. (SSH_MSG_CHANNEL_EXTENDED_DATA)
        /// </summary>
        /// <param name="type">data type code. (e.g. SSH_EXTENDED_DATA_STDERR)</param>
        /// <param name="data">data fragment</param>
        void OnExtendedData(uint type, DataFragment data);

        /// <summary>
        /// Notifies that the channel is going to close.
        /// </summary>
        /// <remarks>
        /// Note that this method may be called before <see cref="OnEstablished(DataFragment)"/> or <see cref="OnReady()"/> is called.
        /// </remarks>
        /// <param name="byServer">true if the server requested closing the channel.</param>
        void OnClosing(bool byServer);

        /// <summary>
        /// Notifies that the channel has been closed.
        /// </summary>
        /// <remarks>
        /// Note that this method may be called before <see cref="OnEstablished(DataFragment)"/> or <see cref="OnReady()"/> is called.
        /// </remarks>
        /// <param name="byServer">true if the server requested closing the channel.</param>
        void OnClosed(bool byServer);

        /// <summary>
        /// Notifies SSH_MSG_CHANNEL_EOF. (SSH2)
        /// </summary>
        void OnEOF();

        /// <summary>
        /// Notifies that the setup request has been failed.
        /// </summary>
        void OnRequestFailed();

        /// <summary>
        /// Notifies that an exception has occurred.
        /// </summary>
        /// <param name="error">exception object</param>
        void OnError(Exception error);

        /// <summary>
        /// Notifies unhandled packet.
        /// </summary>
        /// <param name="packetType">a message number</param>
        /// <param name="data">packet image excluding message number field and channel number field.</param>
        void OnUnhandledPacket(byte packetType, DataFragment data);

        /// <summary>
        /// Notifies that the SSH connection has been closed or disposed.
        /// </summary>
        /// <remarks>
        /// This method may be called multiple times.
        /// </remarks>
        void OnConnectionLost();
    }

    /// <summary>
    /// A simple channel event handler class that do nothing.
    /// </summary>
    public class SimpleSSHChannelEventHandler : ISSHChannelEventHandler {

        public virtual void OnEstablished(DataFragment data) {
        }

        public virtual void OnReady() {
        }

        public virtual void OnData(DataFragment data) {
        }

        public virtual void OnExtendedData(uint type, DataFragment data) {
        }

        public virtual void OnClosing(bool byServer) {
        }

        public virtual void OnClosed(bool byServer) {
        }

        public virtual void OnEOF() {
        }

        public virtual void OnRequestFailed() {
        }

        public virtual void OnError(Exception error) {
        }

        public virtual void OnUnhandledPacket(byte packetType, DataFragment data) {
        }

        public virtual void OnConnectionLost() {
        }

        public virtual void Dispose() {
        }

    }

    /// <summary>
    /// A function type that creates a new channel handler object.
    /// </summary>
    /// <param name="channel">channel object</param>
    public delegate THandler SSHChannelEventHandlerCreator<THandler>(ISSHChannel channel)
                where THandler : ISSHChannelEventHandler;

    /// <summary>
    /// An exception that indicates the operation is not allowed under the current state of the channel.
    /// </summary>
    public class SSHChannelInvalidOperationException : SSHException {
        public SSHChannelInvalidOperationException(string message)
            : base(message) {
        }
    }

}

namespace Granados.SSH {
    using Granados.IO;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;
#if DEBUG_REPORT_SSHCHANNELS
    using System.Linq;
#endif

    /// <summary>
    /// A wrapper class of <see cref="ISSHChannelEventHandler"/> for internal use.
    /// </summary>
    internal class SSHChannelEventHandlerIgnoreErrorWrapper : ISSHChannelEventHandler {

        private readonly ISSHChannelEventHandler _coreHandler;

        public SSHChannelEventHandlerIgnoreErrorWrapper(ISSHChannelEventHandler coreHandler) {
            _coreHandler = coreHandler;
        }

        public void OnEstablished(DataFragment data) {
            try {
                _coreHandler.OnEstablished(data);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnReady() {
            try {
                _coreHandler.OnReady();
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnData(DataFragment data) {
            try {
                _coreHandler.OnData(data);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnExtendedData(uint type, DataFragment data) {
            try {
                _coreHandler.OnExtendedData(type, data);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnClosing(bool byServer) {
            try {
                _coreHandler.OnClosing(byServer);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnClosed(bool byServer) {
            try {
                _coreHandler.OnClosed(byServer);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnEOF() {
            try {
                _coreHandler.OnEOF();
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnRequestFailed() {
            try {
                _coreHandler.OnRequestFailed();
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

        public void OnUnhandledPacket(byte packetType, DataFragment data) {
            try {
                _coreHandler.OnUnhandledPacket(packetType, data);
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void OnConnectionLost() {
            try {
                _coreHandler.OnConnectionLost();
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine(e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void Dispose() {
        }
    }

    /// <summary>
    /// An internal class to manage the pair of the <see cref="ISSHChannel"/> and <see cref="ISSHChannelEventHandler"/>.
    /// </summary>
    internal class SSHChannelCollection {

        private int _channelNumber = -1;

        private class ChannelEntry {
            public readonly ISSHChannel Channel;
            public readonly ISSHChannelEventHandler EventHandler;

            public ChannelEntry(ISSHChannel channel, ISSHChannelEventHandler handler) {
                this.Channel = channel;
                this.EventHandler = handler;
            }
        }

        private readonly ConcurrentDictionary<uint, ChannelEntry> _dic = new ConcurrentDictionary<uint, ChannelEntry>();

        /// <summary>
        /// Constructor
        /// </summary>
        public SSHChannelCollection() {
        }

        /// <summary>
        /// Get the new channel number.
        /// </summary>
        /// <returns>channel number</returns>
        public uint GetNewChannelNumber() {
            return (uint)Interlocked.Increment(ref _channelNumber);
        }

        /// <summary>
        /// Add new channel.
        /// </summary>
        /// <param name="channel">channel</param>
        /// <param name="eventHandler">channel handler</param>
        public void Add(ISSHChannel channel, ISSHChannelEventHandler eventHandler) {
            uint channelNumber = channel.LocalChannel;
            var entry = new ChannelEntry(channel, eventHandler);
            _dic.TryAdd(channelNumber, entry);
#if DEBUG_REPORT_SSHCHANNELS
            Debug.WriteLine("** CHANNEL ADD " + channelNumber.ToString()
                + " { " + String.Join(", ", _dic.Keys.OrderBy(n => n).Select(n => n.ToString())) + " }");
#endif
        }

        /// <summary>
        /// Remove channel.
        /// </summary>
        /// <param name="channel">channel</param>
        public void Remove(ISSHChannel channel) {
            uint channelNumber = channel.LocalChannel;
            ChannelEntry entry;
            _dic.TryRemove(channelNumber, out entry);
#if DEBUG_REPORT_SSHCHANNELS
            Debug.WriteLine("** CHANNEL REMOVE " + channelNumber.ToString()
                + " { " + String.Join(", ", _dic.Keys.OrderBy(n => n).Select(n => n.ToString())) + " }");
#endif
        }

        /// <summary>
        /// Find channel by a local channel number.
        /// </summary>
        /// <param name="channelNumber">a local channel number</param>
        /// <returns>channel operator object, or null if no channel number didn't match.</returns>
        public ISSHChannel FindOperator(uint channelNumber) {
            ChannelEntry entry;
            if (_dic.TryGetValue(channelNumber, out entry)) {
                return entry.Channel;
            }
            return null;
        }

        /// <summary>
        /// Find channel event handler by a local channel number.
        /// </summary>
        /// <param name="channelNumber">a local channel number</param>
        /// <returns>channel handler object, or null if no channel number didn't match.</returns>
        public ISSHChannelEventHandler FindHandler(uint channelNumber) {
            ChannelEntry entry;
            if (_dic.TryGetValue(channelNumber, out entry)) {
                return entry.EventHandler;
            }
            return null;
        }

        /// <summary>
        /// Call the specified action on each channel of the collection.
        /// </summary>
        /// <param name="action"></param>
        public void ForEach(Action<ISSHChannel, ISSHChannelEventHandler> action) {
            foreach (var entry in _dic.Values) {
                try {
                    action(entry.Channel, entry.EventHandler);
                }
                catch (Exception e) {
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                }
            }
        }
    }

}
