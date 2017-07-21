/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SCPChannelStream.cs,v 1.2 2012/05/20 09:10:30 kzmi Exp $
 */

//#define DUMP_PACKET
//#define TRACE_RECEIVER

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

using Granados.SSH;
using Granados.SSH1;
using Granados.SSH2;
using Granados.IO;
using Granados.IO.SSH2;
using Granados.Util;
using System.Collections.Concurrent;

namespace Granados.Poderosa.SCP {

    /// <summary>
    /// Channel stream for SCPClient
    /// </summary>
    internal class SCPChannelStream : IDisposable {

        #region Private enum

        /// <summary>
        /// Stream status
        /// </summary>
        private enum StreamStatus {
            /// <summary>Channel is not opened yet</summary>
            NotOpened,
            /// <summary>Channel is opened, but not ready</summary>
            Opened,
            /// <summary>Channel is closing</summary>
            Closing,
            /// <summary>Channel is closed</summary>
            Closed,
            /// <summary>Channel has error</summary>
            Error,
        }

        #endregion

        #region Private constants

        private const int INITIAL_CAPACITY = 1024;

        #endregion

        #region Private fields

        private volatile StreamStatus _status;
        private readonly object _statusSync = new object();

        private ISSHChannel _channel = null;
        private SCPClientChannelEventHandler _eventHandler = null;

        private ByteBuffer _buffer = new ByteBuffer(INITIAL_CAPACITY, -1);
        private readonly object _bufferSync = new object();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public SCPChannelStream() {
        }

        #endregion

        #region Properties

#if UNITTEST
        internal byte[] DataBuffer {
            get {
                return _buffer;
            }
            set {
                _buffer = value;
            }
        }

        internal int BufferOffset {
            get {
                return _bufferOffset;
            }
            set {
                _bufferOffset = value;
            }
        }

        internal int BufferLength {
            get {
                return _bufferLength;
            }
            set {
                _bufferLength = value;
            }
        }

        internal SCPClientChannelEventReceiver ChannelReceiver {
            get {
                return _channelReceiver;
            }
        }

        internal string Status {
            get {
                return _status.ToString();
            }
        }
#endif

        #endregion

        #region Public methods

        /// <summary>
        /// Opens channel.
        /// </summary>
        /// <param name="connection">SSH connection object</param>
        /// <param name="command">Remote command</param>
        /// <param name="millisecondsTimeout">timeout in milliseconds</param>
        /// <exception cref="SCPClientInvalidStatusException">Channel has been already opened or already closed.</exception>
        /// <exception cref="SCPClientTimeoutException">Timeout has occurred while waiting for READY status.</exception>
        public void Open(ISSHConnection connection, string command, int millisecondsTimeout) {
            if (_status != StreamStatus.NotOpened)
                throw new SCPClientInvalidStatusException();

            ISSHChannel channel = null;
            SCPClientChannelEventHandler eventHandler =
             connection.ExecCommand(
                (ch) => {
                    channel = ch;
                    return new SCPClientChannelEventHandler(
                                new DataReceivedDelegate(OnDataReceived),
                                new ChannelStatusChangedDelegate(OnChannelStatusChanged)
                            );
                },
                command
            );

            _eventHandler = eventHandler;
            _channel = channel;

            if (!_eventHandler.WaitStatus(SCPChannelStatus.READY, millisecondsTimeout)) {
                throw new SCPClientTimeoutException();
            }

            lock(_statusSync) {
                if (_status == StreamStatus.NotOpened) {
                    _status = StreamStatus.Opened;
                }
            }
        }

#if UNITTEST
        internal void OpenForTest(SSHChannel dummyChannel) {
            if (_status != StreamStatus.NotOpened)
                throw new SCPClientInvalidStatusException();

            SCPClientChannelEventReceiver channelReceiver =
                new SCPClientChannelEventReceiver(
                    new DataReceivedDelegate(OnDataReceived),
                    new ChannelStatusChangedDelegate(OnChannelStatusChanged)
                );

            _channelReceiver = channelReceiver;
            _channel = dummyChannel;

            lock (_statusSync) {
                if (_status == StreamStatus.NotOpened) {
                    _status = StreamStatus.Opened;
                }
            }
        }
#endif

        /// <summary>
        /// Close channel.
        /// </summary>
        public void Close() {
            lock (_statusSync) {
                if (_status != StreamStatus.Opened && _status != StreamStatus.Error)
                    return;

                _status = StreamStatus.Closing;
            }

            _channel.SendEOF();

            if (_status != StreamStatus.Closing)
                return;

            _channel.Close();

            lock (_statusSync) {
                if (_status == StreamStatus.Closing) {
                    _status = StreamStatus.Closed;
                }
            }
        }

        /// <summary>
        /// Gets preferred datagram size.
        /// </summary>
        /// <returns>size in bytes</returns>
        public int GetPreferredDatagramSize() {
            return (_channel != null) ? Math.Max(1024, _channel.MaxChannelDatagramSize) : 1024;
        }

        /// <summary>
        /// Writes data.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        public void Write(byte[] buffer) {
            Write(buffer, buffer.Length);
        }

        /// <summary>
        /// Writes data.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <param name="length">Length</param>
        public void Write(byte[] buffer, int length) {
            if (_status != StreamStatus.Opened)
                throw new SCPClientInvalidStatusException();
            Debug.Assert(_channel != null);

            _channel.Send(new DataFragment(buffer, 0, length));
        }

        /// <summary>
        /// Reads data
        /// </summary>
        /// <param name="buffer">Byte array to store the data</param>
        /// <param name="millisecondsTimeout">Timeout in milliseconds</param>
        /// <returns>Length to be stored in the buffer</returns>
        /// <exception cref="SCPClientTimeoutException">Timeout has occurred</exception>
        public int Read(byte[] buffer, int millisecondsTimeout) {
            return Read(buffer, buffer.Length, millisecondsTimeout);
        }

        /// <summary>
        /// Reads data
        /// </summary>
        /// <param name="buffer">Byte array to store the data</param>
        /// <param name="maxLength">Maximum bytes to read</param>
        /// <param name="millisecondsTimeout">Timeout in milliseconds</param>
        /// <returns>Length to be stored in the buffer</returns>
        /// <exception cref="SCPClientTimeoutException">Timeout has occurred</exception>
        public int Read(byte[] buffer, int maxLength, int millisecondsTimeout) {
            if (_status != StreamStatus.Opened)
                throw new SCPClientInvalidStatusException();

            lock (_bufferSync) {
                while (_buffer.Length == 0) {
                    bool signaled = Monitor.Wait(_bufferSync, millisecondsTimeout);
                    if (!signaled)
                        throw new SCPClientTimeoutException();
                    if (_status != StreamStatus.Opened)
                        throw new SCPClientInvalidStatusException();
                }

                int retrieveSize = Math.Min(_buffer.Length, Math.Min(buffer.Length, maxLength));
                Buffer.BlockCopy(_buffer.RawBuffer, _buffer.RawBufferOffset, buffer, 0, retrieveSize);
                _buffer.RemoveHead(retrieveSize);
                return retrieveSize;
            }
        }

        /// <summary>
        /// Read one byte
        /// </summary>
        /// <param name="millisecondsTimeout">Timeout in milliseconds</param>
        /// <returns>Byte value</returns>
        /// <exception cref="SCPClientTimeoutException">Timeout has occurred</exception>
        public byte ReadByte(int millisecondsTimeout) {
            if (_status != StreamStatus.Opened)
                throw new SCPClientInvalidStatusException();

            lock (_bufferSync) {
                while (_buffer.Length < 1) {
                    bool signaled = Monitor.Wait(_bufferSync, millisecondsTimeout);
                    if (!signaled)
                        throw new SCPClientTimeoutException();
                    if (_status != StreamStatus.Opened)
                        throw new SCPClientInvalidStatusException();
                }

                byte b = _buffer[0];
                _buffer.RemoveHead(1);
                return b;
            }
        }

        /// <summary>
        /// Read data until specified byte value is read.
        /// </summary>
        /// <param name="terminator">Byte value to stop reading</param>
        /// <param name="millisecondsTimeout">Timeout in milliseconds</param>
        /// <returns>Byte array received.</returns>
        /// <exception cref="SCPClientTimeoutException">Timeout has occurred</exception>
        /// <exception cref="SCPClientException">Buffer overflow</exception>
        public byte[] ReadUntil(byte terminator, int millisecondsTimeout) {
            if (_status != StreamStatus.Opened)
                throw new SCPClientInvalidStatusException();

            lock (_bufferSync) {
                for(int dataOffset = 0; ; dataOffset++) {
                    while (_buffer.Length <= dataOffset) {
                        bool signaled = Monitor.Wait(_bufferSync, millisecondsTimeout);
                        if (!signaled)
                            throw new SCPClientTimeoutException();
                        if (_status != StreamStatus.Opened)
                            throw new SCPClientInvalidStatusException();
                    }

                    byte b = _buffer[dataOffset];
                    if (b == terminator) {
                        int dataLength = dataOffset + 1;
                        byte[] data = new byte[dataLength];
                        Buffer.BlockCopy(_buffer.RawBuffer, _buffer.RawBufferOffset, data, 0, dataLength);
                        _buffer.RemoveHead(dataLength);
                        return data;
                    }
                }
            }
        }

        #endregion

        #region SCPClientChannelEventReceiver Handlers

        private void OnDataReceived(DataFragment data) {
            lock (_bufferSync) {
                _buffer.Append(data);
                Monitor.PulseAll(_bufferSync);
            }
        }

        private void OnChannelStatusChanged(SCPChannelStatus newStatus) {
            if (newStatus == SCPChannelStatus.CLOSED) {
                lock (_statusSync) {
                    _status = StreamStatus.Closed;
                }
                lock (_bufferSync) {
                    Monitor.PulseAll(_bufferSync);
                }
            }
            else if (newStatus == SCPChannelStatus.ERROR) {
                lock (_statusSync) {
                    _status = StreamStatus.Error;
                }
                lock (_bufferSync) {
                    Monitor.PulseAll(_bufferSync);
                }
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// IDisposable implementation
        /// </summary>
        public void Dispose() {
            Close();
        }

        #endregion
    }

    /// <summary>
    /// Channel status
    /// </summary>
    internal enum SCPChannelStatus {
        UNKNOWN,
        READY,
        CLOSED,
        ERROR,
    }

    /// <summary>
    /// Delegate that handles channel data.
    /// </summary>
    /// <param name="data">Data buffer</param>
    internal delegate void DataReceivedDelegate(DataFragment data);

    /// <summary>
    /// Delegate that handles transition of the channel status.
    /// </summary>
    /// <param name="newStatus">New status</param>
    internal delegate void ChannelStatusChangedDelegate(SCPChannelStatus newStatus);

    /// <summary>
    /// Channel data handler for SCPClient
    /// </summary>
    internal class SCPClientChannelEventHandler : SimpleSSHChannelEventHandler {

        #region Private fields

        private volatile SCPChannelStatus _channelStatus = SCPChannelStatus.UNKNOWN;

        private readonly Object _statusSync = new object();
        private readonly Object _responseNotifier = new object();

        private readonly DataReceivedDelegate _dataHandler = null;
        private readonly ChannelStatusChangedDelegate _statusChangeHandler = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets channel status
        /// </summary>
        public SCPChannelStatus ChannelStatus {
            get {
                return _channelStatus;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataHandler">Data handler</param>
        /// <param name="statusChangeHandler">Channel status handler</param>
        public SCPClientChannelEventHandler(DataReceivedDelegate dataHandler, ChannelStatusChangedDelegate statusChangeHandler) {
            this._dataHandler = dataHandler;
            this._statusChangeHandler = statusChangeHandler;
        }

        #endregion

        public bool WaitStatus(SCPChannelStatus status, int millisecondsTimeout) {
            lock (_statusSync) {
                while (_channelStatus != status) {
                    bool acquired = Monitor.Wait(_statusSync, millisecondsTimeout);
                    if (!acquired) {
                        return false;
                    }
                }
                return true;
            }
        }

        #region ISSHChannelEventHandler

        public override void OnData(DataFragment data) {
#if DUMP_PACKET
            Dump("SCP: OnData", data);
#endif
            if (_dataHandler != null) {
                _dataHandler(data);
            }
        }

        public override void OnClosed(bool byServer) {
#if TRACE_RECEIVER
            System.Diagnostics.Debug.WriteLine("SCP: Closed");
#endif
            TransitStatus(SCPChannelStatus.CLOSED);
        }

        public override void OnError(Exception error) {
#if TRACE_RECEIVER
            System.Diagnostics.Debug.WriteLine("SCP: Error: " + error.Message);
#endif
            TransitStatus(SCPChannelStatus.ERROR);
        }

        public override void OnReady() {
#if TRACE_RECEIVER
            System.Diagnostics.Debug.WriteLine("SCP: OnChannelReady");
#endif
            TransitStatus(SCPChannelStatus.READY);
        }

        #endregion

        #region Private methods

        private void TransitStatus(SCPChannelStatus newStatus) {
            lock (_statusSync) {
                _channelStatus = newStatus;
                Monitor.PulseAll(_statusSync);
            }
            if (_statusChangeHandler != null) {
                _statusChangeHandler(newStatus);
            }
        }

#if DUMP_PACKET
        // for debug
        private void Dump(string caption, DataFragment data) {
            Dump(caption, data.Data, data.Offset, data.Length);
        }

        // for debug
        private void Dump(string caption, byte[] data, int offset, int length) {
            StringBuilder s = new StringBuilder();
            s.AppendLine(caption);
            s.Append("0--1--2--3--4--5--6--7--8--9--A--B--C--D--E--F-");
            for (int i = 0; i < length; i++) {
                byte b = data[offset + i];
                int pos = i % 16;
                if (pos == 0)
                    s.AppendLine();
                else
                    s.Append(' ');
                s.Append("0123456789abcdef"[b >> 4]).Append("0123456789abcdef"[b & 0xf]);
            }
            s.AppendLine().Append("0--1--2--3--4--5--6--7--8--9--A--B--C--D--E--F-");
            System.Diagnostics.Debug.WriteLine(s);
        }
#endif

        #endregion
    }


}
