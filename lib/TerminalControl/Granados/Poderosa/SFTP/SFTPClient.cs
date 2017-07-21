/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SFTPClient.cs,v 1.6 2012/05/05 12:42:45 kzmi Exp $
 */

//#define DUMP_PACKET
//#define TRACE_RECEIVER

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

using Granados.SSH2;
using Granados.IO;
using Granados.IO.SSH2;
using Granados.Util;
using Granados.Poderosa.FileTransfer;
using Granados.SSH;
using System.Threading.Tasks;

namespace Granados.Poderosa.SFTP {

    /// <summary>
    /// Statuses of the file transfer
    /// </summary>
    public enum SFTPFileTransferStatus {
        /// <summary>Not started</summary>
        None,
        /// <summary>Opening remote file is attempted</summary>
        Open,
        /// <summary>File was opened successfully and data has been requested</summary>
        Transmitting,
        /// <summary>Closing remote file is attempted</summary>
        Close,
        /// <summary>File was closed successfully</summary>
        CompletedSuccess,
        /// <summary>Error has occurred</summary>
        CompletedError,
        /// <summary>File transfer was aborted by the cancellation.</summary>
        CompletedAbort,
    }

    /// <summary>
    /// Delegate notifies progress of the file transfer.
    /// </summary>
    /// <param name="status">Status of the file transfer.</param>
    /// <param name="transmitted">Transmitted data length.</param>
    public delegate void SFTPFileTransferProgressDelegate(SFTPFileTransferStatus status, ulong transmitted);

    /// <summary>
    /// SFTP Client
    /// </summary>
    /// 
    /// <remarks>
    /// <para>This class is designed to be used in the worker thread.</para>
    /// <para>Some methods block thread while waiting for the response.</para>
    /// </remarks>
    /// 
    /// <remarks>
    /// Referenced specification:<br/>
    /// IETF Network Working Group Internet Draft<br/>
    /// SSH File Transfer Protocol<br/>
    /// draft-ietf-secsh-filexfer-02 (protocol version 3)
    /// </remarks>
    public class SFTPClient {

        #region Private fields

        private readonly ISSHChannel _channel;
        private readonly SFTPClientChannelEventHandler _eventHandler;

        private int _protocolTimeout = 5000;

        private Encoding _encoding = Encoding.UTF8;

        private uint _requestId = 0;

        private bool _closed = false;

        #endregion

        #region Private constants

        // Note: OpenSSH uses version 3
        private const int SFTP_VERSION = 3;

        // attribute flags
        private const uint SSH_FILEXFER_ATTR_SIZE = 0x00000001;
        private const uint SSH_FILEXFER_ATTR_UIDGID = 0x00000002;
        private const uint SSH_FILEXFER_ATTR_PERMISSIONS = 0x00000004;
        private const uint SSH_FILEXFER_ATTR_ACMODTIME = 0x00000008;
        private const uint SSH_FILEXFER_ATTR_EXTENDED = 0x80000000;

        // file open flag
        private const uint SSH_FXF_READ = 0x00000001;
        private const uint SSH_FXF_WRITE = 0x00000002;
        private const uint SSH_FXF_APPEND = 0x00000004;
        private const uint SSH_FXF_CREAT = 0x00000008;
        private const uint SSH_FXF_TRUNC = 0x00000010;
        private const uint SSH_FXF_EXCL = 0x00000020;

        #endregion

        #region Properties

        /// <summary>
        /// Protocol timeout in milliseconds.
        /// </summary>
        public int ProtocolTimeout {
            get {
                return _protocolTimeout;
            }
            set {
                _protocolTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets encoding to convert path name or file name.
        /// </summary>
        public Encoding Encoding {
            get {
                return _encoding;
            }
            set {
                _encoding = value;
            }
        }

        /// <summary>
        /// Gets whether the channel has been closed
        /// </summary>
        public bool IsClosed {
            get {
                return _closed;
            }
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Opens SFTP channel and creates a new instance.
        /// </summary>
        /// <param name="connection">SSH2 connection object</param>
        /// <returns>New instance.</returns>
        public static SFTPClient OpenSFTPChannel(ISSHConnection connection) {
            ISSHChannel channel = null;
            SFTPClientChannelEventHandler eventHandler =
                connection.OpenSubsystem(
                    (ch) => {
                        channel = ch;
                        return new SFTPClientChannelEventHandler();
                    },
                    "sftp"
                );
            return new SFTPClient(channel, eventHandler);
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="channel">SSH2 channel object</param>
        /// <param name="eventHandler">event handler object</param>
        private SFTPClient(ISSHChannel channel, SFTPClientChannelEventHandler eventHandler) {
            this._channel = channel;
            this._eventHandler = eventHandler;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize
        /// </summary>
        /// <remarks>
        /// Send SSH_FXP_INIT and receive SSH_FXP_VERSION.
        /// </remarks>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status</exception>
        /// <exception cref="Exception">An exception which was thrown while processing the response.</exception>
        public void Init() {
            WaitReady();
            CheckStatus();

            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_INIT)
                    .WriteInt32(SFTP_VERSION);

            bool result = false;

            _eventHandler.ClearResponseBuffer();
            Transmit(packet);
            _eventHandler.WaitResponse(
                (packetType, dataReader) => {
                    if (packetType == SFTPPacketType.SSH_FXP_VERSION) {
                        int version = dataReader.ReadInt32();
                        Debug.WriteLine("SFTP: SSH_FXP_VERSION => " + version);

                        result = true;   // OK, received SSH_FXP_VERSION

                        while (dataReader.RemainingDataLength > 4) {
                            string extensionText = dataReader.ReadUTF8String();
                            Debug.WriteLine("SFTP: SSH_FXP_VERSION => " + extensionText);
                        }

                        return true;    // processed
                    }

                    return false;   // ignored
                },
                _protocolTimeout
            );

            // sanity check
            if (!Volatile.Read(ref result)) {
                throw new SFTPClientException("Missing SSH_FXP_VERSION");
            }
        }

        /// <summary>
        /// Waits for channel status to be "READY".
        /// The current thread is blocked until the status comes to "READY" or "CLOSED".
        /// </summary>
        /// <returns>Whether the channel status is READY.</returns>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        private bool WaitReady() {
            lock (_eventHandler.StatusChangeNotifier) {
                if (_eventHandler.ChannelStatus == SFTPChannelStatus.UNKNOWN) {
                    bool signaled = Monitor.Wait(_eventHandler.StatusChangeNotifier, _protocolTimeout);
                    if (!signaled)
                        throw new SFTPClientTimeoutException();
                }
                return (_eventHandler.ChannelStatus == SFTPChannelStatus.READY);
            }
        }

        #endregion

        #region Close

        /// <summary>
        /// Close channel.
        /// </summary>
        public void Close() {
            CheckStatus();

            _closed = true;
            _channel.SendEOF();
            _channel.Close();
        }

        #endregion

        #region Path operations

        /// <summary>
        /// Gets canonical path.
        /// </summary>
        /// <param name="path">Path to be canonicalized.</param>
        /// <returns>Canonical path.</returns>
        /// <exception cref="SFTPClientErrorException">Operation failed.</exception>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status.</exception>
        public string GetRealPath(string path) {
            CheckStatus();

            uint requestId = ++_requestId;

            byte[] pathData = _encoding.GetBytes(path);
            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_REALPATH)
                        .WriteUInt32(requestId)
                        .WriteAsString(pathData);

            bool result = false;
            string realPath = null;

            _eventHandler.ClearResponseBuffer();
            Transmit(packet);
            _eventHandler.WaitResponse(
                (packetType, dataReader) => {
                    if (packetType == SFTPPacketType.SSH_FXP_STATUS) {
                        SFTPClientErrorException exception = SFTPClientErrorException.Create(dataReader);
                        if (exception.ID == requestId) {
                            throw exception;
                        }
                    }
                    else if (packetType == SFTPPacketType.SSH_FXP_NAME) {
                        uint id = dataReader.ReadUInt32();
                        if (id == requestId) {
                            uint count = (uint)dataReader.ReadInt32();

                            if (count > 0) {
                                // use Encoding object with replacement fallback
                                Encoding encoding = Encoding.GetEncoding(
                                                    _encoding.CodePage,
                                                    EncoderFallback.ReplacementFallback,
                                                    DecoderFallback.ReplacementFallback);

                                SFTPFileInfo fileInfo = ReadFileInfo(dataReader, encoding);
                                realPath = fileInfo.FileName;
                            }

                            result = true;   // OK, received SSH_FXP_NAME
                            return true;    // processed
                        }
                    }

                    return false;   // ignored
                },
                _protocolTimeout
            );

            // sanity check
            if (!Volatile.Read(ref result)) {
                throw new SFTPClientException("Missing SSH_FXP_NAME");
            }

            return realPath;
        }

        #endregion

        #region Directory operations

        /// <summary>
        /// Gets directory entries in the specified directory path.
        /// </summary>
        /// <param name="directoryPath">Directory path.</param>
        /// <returns>Array of the file information.</returns>
        /// <exception cref="SFTPClientErrorException">Operation failed.</exception>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status.</exception>
        public SFTPFileInfo[] GetDirectoryEntries(string directoryPath) {
            CheckStatus();

            uint requestId = ++_requestId;

            while (directoryPath != "/" && directoryPath.EndsWith("/")) {
                directoryPath = directoryPath.Substring(0, directoryPath.Length - 1);
            }

            byte[] handle = OpenDir(requestId, directoryPath);

            List<SFTPFileInfo> files = new List<SFTPFileInfo>();

            while (true) {
                ICollection<SFTPFileInfo> tmpList = ReadDir(requestId, handle);
                if (tmpList.Count == 0)
                    break;
                files.AddRange(tmpList);
            }

            CloseHandle(requestId, handle);
            return files.ToArray();
        }

        /// <summary>
        /// Create directory.
        /// </summary>
        /// <param name="path">Directory path to create.</param>
        /// <exception cref="SFTPClientErrorException">Operation failed.</exception>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status</exception>
        /// <exception cref="Exception">An exception which was thrown while processing the response.</exception>
        public void CreateDirectory(string path) {
            CheckStatus();

            uint requestId = ++_requestId;

            byte[] pathData = _encoding.GetBytes(path);

            TransmitPacketAndWaitForStatusOK(
                requestId,
                new SFTPPacket(SFTPPacketType.SSH_FXP_MKDIR)
                    .WriteUInt32(requestId)
                    .WriteAsString(pathData)
                    .WriteInt32(0)   // attributes flag
            );
        }

        /// <summary>
        /// Remove directory.
        /// </summary>
        /// <param name="path">Directory path to remove.</param>
        /// <exception cref="SFTPClientErrorException">Operation failed.</exception>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status</exception>
        /// <exception cref="Exception">An exception which was thrown while processing the response.</exception>
        public void RemoveDirectory(string path) {
            CheckStatus();

            uint requestId = ++_requestId;

            byte[] pathData = _encoding.GetBytes(path);

            TransmitPacketAndWaitForStatusOK(
                requestId,
                new SFTPPacket(SFTPPacketType.SSH_FXP_RMDIR)
                    .WriteUInt32(requestId)
                    .WriteAsString(pathData)
            );
        }

        #endregion

        #region File operations

        /// <summary>
        /// Get file informations.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="lstat">Specifies to use lstat. Symbolic link is not followed and informations about the symbolic link are returned.</param>
        /// <returns>File attribute informations</returns>
        /// <exception cref="SFTPClientErrorException">Operation failed.</exception>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status</exception>
        /// <exception cref="Exception">An exception which was thrown while processing the response.</exception>
        public SFTPFileAttributes GetFileInformations(string path, bool lstat) {
            CheckStatus();

            uint requestId = ++_requestId;

            byte[] pathData = _encoding.GetBytes(path);
            SFTPPacket packet =
                new SFTPPacket(lstat ? SFTPPacketType.SSH_FXP_LSTAT : SFTPPacketType.SSH_FXP_STAT)
                        .WriteUInt32(requestId)
                        .WriteAsString(pathData);

            bool result = false;
            SFTPFileAttributes attributes = null;

            _eventHandler.ClearResponseBuffer();
            Transmit(packet);
            _eventHandler.WaitResponse(
                delegate(SFTPPacketType packetType, SSHDataReader dataReader) {
                    if (packetType == SFTPPacketType.SSH_FXP_STATUS) {
                        SFTPClientErrorException exception = SFTPClientErrorException.Create(dataReader);
                        if (exception.ID == requestId) {
                            throw exception;
                        }
                    }
                    else if (packetType == SFTPPacketType.SSH_FXP_ATTRS) {
                        uint id = dataReader.ReadUInt32();
                        if (id == requestId) {
                            attributes = ReadFileAttributes(dataReader);
                            result = true;   // Ok, received SSH_FXP_ATTRS
                            return true;    // processed
                        }
                    }

                    return false;   // ignored
                },
                _protocolTimeout
            );

            // sanity check
            if (!Volatile.Read(ref result)) {
                throw new SFTPClientException("Missing SSH_FXP_ATTRS");
            }

            return attributes;
        }

        /// <summary>
        /// Remove file.
        /// </summary>
        /// <param name="path">File path to remove.</param>
        /// <exception cref="SFTPClientErrorException">Operation failed.</exception>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status</exception>
        /// <exception cref="Exception">An exception which was thrown while processing the response.</exception>
        public void RemoveFile(string path) {
            CheckStatus();

            uint requestId = ++_requestId;

            byte[] pathData = _encoding.GetBytes(path);
            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_REMOVE)
                    .WriteUInt32(requestId)
                    .WriteAsString(pathData);

            TransmitPacketAndWaitForStatusOK(requestId, packet);
        }

        /// <summary>
        /// Rename file or directory.
        /// </summary>
        /// <param name="oldPath">Old path.</param>
        /// <param name="newPath">New path.</param>
        /// <exception cref="SFTPClientErrorException">Operation failed.</exception>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status</exception>
        /// <exception cref="Exception">An exception which was thrown while processing the response.</exception>
        public void Rename(string oldPath, string newPath) {
            CheckStatus();

            uint requestId = ++_requestId;

            byte[] oldPathData = _encoding.GetBytes(oldPath);
            byte[] newPathData = _encoding.GetBytes(newPath);
            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_RENAME)
                    .WriteUInt32(requestId)
                    .WriteAsString(oldPathData)
                    .WriteAsString(newPathData);

            TransmitPacketAndWaitForStatusOK(requestId, packet);
        }

        /// <summary>
        /// Set permissions of the file or directory.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="permissions">Permissions to set.</param>
        /// <exception cref="SFTPClientErrorException">Operation failed.</exception>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status</exception>
        /// <exception cref="Exception">An exception which was thrown while processing the response.</exception>
        public void SetPermissions(string path, int permissions) {
            CheckStatus();

            uint requestId = ++_requestId;

            byte[] pathData = _encoding.GetBytes(path);
            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_SETSTAT)
                    .WriteUInt32(requestId)
                    .WriteAsString(pathData)
                    .WriteUInt32(SSH_FILEXFER_ATTR_PERMISSIONS)
                    .WriteUInt32((uint)permissions & 0xfffu /* 07777 */);

            TransmitPacketAndWaitForStatusOK(requestId, packet);
        }

        #endregion

        #region File transfer

        /// <summary>
        /// Download a file.
        /// </summary>
        /// <remarks>
        /// Even if download failed, local file is not deleted.
        /// </remarks>
        /// 
        /// <param name="remotePath">Remote file path to download.</param>
        /// <param name="localPath">Local file path to save.</param>
        /// <param name="cancellation">An object to request the cancellation. Set null if the cancellation is not needed.</param>
        /// <param name="progressDelegate">Delegate to notify progress. Set null if notification is not needed.</param>
        /// 
        /// <exception cref="SFTPClientErrorException">Operation failed.</exception>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status.</exception>
        /// <exception cref="SFTPClientException">Error.</exception>
        /// <exception cref="Exception">Error.</exception>
        public void DownloadFile(string remotePath, string localPath, Cancellation cancellation, SFTPFileTransferProgressDelegate progressDelegate) {
            CheckStatus();

            uint requestId = ++_requestId;

            ulong transmitted = 0;

            Exception pendingException = null;

            if (progressDelegate != null) {
                progressDelegate(SFTPFileTransferStatus.Open, transmitted);
            }

            byte[] handle;
            try {
                handle = OpenFile(requestId, remotePath, SSH_FXF_READ);
            }
            catch (Exception) {
                if (progressDelegate != null) {
                    progressDelegate(SFTPFileTransferStatus.CompletedError, transmitted);
                }
                throw;
            }

            bool hasError = false;
            bool dataFinished = false;
            try {
                using (FileStream fileStream = new FileStream(localPath, FileMode.Create, FileAccess.Write, FileShare.Read)) {

                    var dataToSave = new AtomicBox<DataFragment>();
                    var cancelTask = new CancellationTokenSource();
                    var cancelToken = cancelTask.Token;

                    Task writeFileTask = Task.Run(() => {
                        while (true) {
                            DataFragment df = null;
                            while (true) {
                                if (dataToSave.TryGet(ref df, 500)) {
                                    break;
                                }
                                if (cancelToken.IsCancellationRequested) {
                                    return;
                                }
                            }

                            if (df == null) {
                                dataFinished = true;
                                return; // end of file
                            }

                            fileStream.Write(df.Data, df.Offset, df.Length);
                        }
                    }, cancelToken);

                    try {
                        // fixed buffer size is used.
                        // it is very difficult to decide optimal buffer size
                        // because the server may change the packet size
                        // depending on the available window size.
                        const int buffSize = 0x10000;
                        // use multiple buffers cyclically.
                        // at least 3 buffers are required.
                        DataFragment[] dataFrags =
                            {
                                new DataFragment(new byte[buffSize], 0, buffSize),
                                new DataFragment(new byte[buffSize], 0, buffSize),
                                new DataFragment(new byte[buffSize], 0, buffSize),
                            };
                        int buffIndex = 0;

                        while (true) {
                            if (cancellation != null && cancellation.IsRequested) {
                                break;
                            }

                            DataFragment df = dataFrags[buffIndex];
                            buffIndex = (buffIndex + 1) % 3;

                            int length = ReadFile(requestId, handle, transmitted, buffSize, df.Data);
                            if (length == 0) {
                                df = null;  // end of file
                            } else {
                                df.SetLength(0, length);
                            }

                            // pass to the writing task
                            if (!dataToSave.TrySet(df, 1000)) {
                                throw new Exception("write error");
                            }

                            transmitted += (ulong)length;

                            if (progressDelegate != null) {
                                progressDelegate(SFTPFileTransferStatus.Transmitting, transmitted);
                            }

                            if (length == 0) {
                                writeFileTask.Wait(1000);
                                break; // EOF
                            }
                        }
                    }
                    finally {
                        if (!writeFileTask.IsCompleted) {
                            cancelTask.Cancel();
                        }
                        writeFileTask.Wait();
                    }
                }
            }
            catch (Exception e) {
                if (e is AggregateException) {
                    pendingException = ((AggregateException)e).InnerExceptions[0];
                }
                else {
                    pendingException = e;
                }

                hasError = true;
            }

            try {
                if (progressDelegate != null) {
                    progressDelegate(SFTPFileTransferStatus.Close, transmitted);
                }

                CloseHandle(requestId, handle);

                if (progressDelegate != null) {
                    SFTPFileTransferStatus status =
                        hasError ? SFTPFileTransferStatus.CompletedError :
                        dataFinished ? SFTPFileTransferStatus.CompletedSuccess : SFTPFileTransferStatus.CompletedAbort;
                    progressDelegate(status, transmitted);
                }
            }
            catch (Exception) {
                if (progressDelegate != null) {
                    progressDelegate(SFTPFileTransferStatus.CompletedError, transmitted);
                }
                throw;
            }

            if (pendingException != null) {
                throw new SFTPClientException(pendingException.Message, pendingException);
            }
        }

        /// <summary>
        /// Upload a file.
        /// </summary>
        /// 
        /// <param name="localPath">Local file path to upload.</param>
        /// <param name="remotePath">Remote file path to write.</param>
        /// <param name="cancellation">An object to request the cancellation. Set null if the cancellation is not needed.</param>
        /// <param name="progressDelegate">Delegate to notify progress. Set null if notification is not needed.</param>
        /// 
        /// <exception cref="SFTPClientErrorException">Operation failed.</exception>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="SFTPClientInvalidStatusException">Invalid status.</exception>
        /// <exception cref="SFTPClientException">Error.</exception>
        /// <exception cref="Exception">Error.</exception>
        public void UploadFile(string localPath, string remotePath, Cancellation cancellation, SFTPFileTransferProgressDelegate progressDelegate) {
            CheckStatus();

            uint requestId = ++_requestId;

            ulong transmitted = 0;

            Exception pendingException = null;

            bool hasError = false;
            bool dataFinished = false;
            byte[] handle = null;
            try {
                using (FileStream fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {

                    if (progressDelegate != null) {
                        progressDelegate(SFTPFileTransferStatus.Open, transmitted);
                    }

                    handle = OpenFile(requestId, remotePath, SSH_FXF_WRITE | SSH_FXF_CREAT | SSH_FXF_TRUNC);

                    var dataToSend = new AtomicBox<DataFragment>();
                    var cancelTask = new CancellationTokenSource();
                    var cancelToken = cancelTask.Token;

                    Task readFileTask = Task.Run(() => {
                        // SSH_FXP_WRITE header part
                        //   4 bytes : packet length
                        //   1 byte  : message type (SSH_FXP_WRITE)
                        //   4 bytes : request id
                        //   4 bytes : handle length
                        //   n bytes : handle
                        //   8 bytes : offset
                        //   4 bytes : length of the datagram
                        int buffSize = _channel.MaxChannelDatagramSize - 25 - handle.Length;
                        // use multiple buffers cyclically.
                        // at least 3 buffers are required.
                        DataFragment[] dataFrags =
                            {
                                new DataFragment(new byte[buffSize], 0, buffSize),
                                new DataFragment(new byte[buffSize], 0, buffSize),
                                new DataFragment(new byte[buffSize], 0, buffSize),
                            };
                        int buffIndex = 0;

                        while (true) {
                            if (cancelToken.IsCancellationRequested) {
                                return;
                            }

                            DataFragment df = dataFrags[buffIndex];
                            buffIndex = (buffIndex + 1) % 3;

                            int length = fileStream.Read(df.Data, 0, df.Data.Length);
                            if (length == 0) {
                                df = null;  // end of file
                            }
                            else {
                                df.SetLength(0, length);
                            }

                            // pass to the sending loop
                            while (true) {
                                if (dataToSend.TrySet(df, 500)) {
                                    break;
                                }
                                if (cancelToken.IsCancellationRequested) {
                                    return;
                                }
                            }

                            if (length == 0) {
                                return; // end of file
                            }
                        }
                    }, cancelToken);

                    try {
                        while (true) {
                            if (cancellation != null && cancellation.IsRequested) {
                                break;
                            }

                            DataFragment dataFrag = null;
                            if (!dataToSend.TryGet(ref dataFrag, 1000)) {
                                throw new Exception("read error");
                            }

                            if (dataFrag == null) {
                                dataFinished = true;
                                break;
                            }

                            WriteFile(requestId, handle, transmitted, dataFrag.Data, dataFrag.Length);

                            transmitted += (ulong)dataFrag.Length;

                            if (progressDelegate != null) {
                                progressDelegate(SFTPFileTransferStatus.Transmitting, transmitted);
                            }
                        }
                    }
                    finally {
                        if (!readFileTask.IsCompleted) {
                            cancelTask.Cancel();
                        }
                        readFileTask.Wait();
                    }
                }   // using
            }
            catch (Exception e) {
                if (e is AggregateException) {
                    pendingException = ((AggregateException)e).InnerExceptions[0];
                }
                else {
                    pendingException = e;
                }

                hasError = true;
            }

            try {
                if (handle != null) {
                    if (progressDelegate != null)
                        progressDelegate(SFTPFileTransferStatus.Close, transmitted);

                    CloseHandle(requestId, handle);
                }

                if (progressDelegate != null) {
                    SFTPFileTransferStatus status =
                        hasError ? SFTPFileTransferStatus.CompletedError :
                        dataFinished ? SFTPFileTransferStatus.CompletedSuccess : SFTPFileTransferStatus.CompletedAbort;
                    progressDelegate(status, transmitted);
                }
            }
            catch (Exception) {
                if (progressDelegate != null) {
                    progressDelegate(SFTPFileTransferStatus.CompletedError, transmitted);
                }
                throw;
            }

            if (pendingException != null) {
                throw new SFTPClientException(pendingException.Message, pendingException);
            }
        }

        #endregion

        #region Private methods about status

        private void TransmitPacketAndWaitForStatusOK(uint requestId, SFTPPacket packet) {
            bool result = false;

            _eventHandler.ClearResponseBuffer();
            Transmit(packet);
            _eventHandler.WaitResponse(
                (packetType, dataReader) => {
                    if (packetType == SFTPPacketType.SSH_FXP_STATUS) {
                        SFTPClientErrorException exception = SFTPClientErrorException.Create(dataReader);
                        if (exception.ID == requestId) {
                            if (exception.Code == SFTPStatusCode.SSH_FX_OK) {
                                result = true;   // Ok, received SSH_FX_OK
                                return true;    // processed
                            }
                            else {
                                throw exception;
                            }
                        }
                    }

                    return false;   // ignored
                },
                _protocolTimeout
            );

            // sanity check
            if (!Volatile.Read(ref result)) {
                throw new SFTPClientException("Missing SSH_FXP_STATUS");
            }
        }

        #endregion

        #region Private methods about file

        private byte[] OpenFile(uint requestId, string filePath, uint flags) {
            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_OPEN)
                    .WriteUInt32(requestId)
                    .WriteAsString(_encoding.GetBytes(filePath))
                    .WriteUInt32(flags)
                    .WriteUInt32(0);  // attribute flag

            return WaitHandle(packet, requestId);
        }

        private int ReadFile(uint requestId, byte[] handle, ulong offset, int length, byte[] buffer) {
            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_READ)
                    .WriteUInt32(requestId)
                    .WriteAsString(handle)
                    .WriteUInt64(offset)
                    .WriteInt32(length);

            int? readLength = null;

            _eventHandler.ClearResponseBuffer();
            Transmit(packet);
            _eventHandler.WaitResponse(
                (packetType, dataReader) => {
                    if (packetType == SFTPPacketType.SSH_FXP_STATUS) {
                        SFTPClientErrorException exception = SFTPClientErrorException.Create(dataReader);
                        if (exception.ID == requestId) {
                            if (exception.Code == SFTPStatusCode.SSH_FX_EOF) {
                                readLength = 0;
                                return true;    // processed
                            }
                            else {
                                throw exception;
                            }
                        }
                    }
                    else if (packetType == SFTPPacketType.SSH_FXP_DATA) {
                        uint id = dataReader.ReadUInt32();
                        if (id == requestId) {
                            readLength = dataReader.ReadByteStringInto(buffer);
                            return true;    // processed
                        }
                    }

                    return false;   // ignored
                },
                _protocolTimeout
            );

            // sanity check
            Thread.MemoryBarrier();
            if (!readLength.HasValue) {
                throw new SFTPClientException("Missing SSH_FXP_DATA");
            }

            return readLength.Value;
        }

        private void WriteFile(uint requestId, byte[] handle, ulong offset, byte[] buff, int length) {
            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_WRITE)
                    .WriteUInt32(requestId)
                    .WriteAsString(handle)
                    .WriteUInt64(offset)
                    .WriteAsString(buff, 0, length);

            TransmitPacketAndWaitForStatusOK(requestId, packet);
        }

        #endregion

        #region Private methods about directory

        private byte[] OpenDir(uint requestId, string directoryPath) {
            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_OPENDIR)
                    .WriteUInt32(requestId)
                    .WriteAsString(_encoding.GetBytes(directoryPath));

            return WaitHandle(packet, requestId);
        }

        private ICollection<SFTPFileInfo> ReadDir(uint requestId, byte[] handle) {
            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_READDIR)
                    .WriteUInt32(requestId)
                    .WriteAsString(handle);

            List<SFTPFileInfo> fileList = new List<SFTPFileInfo>();
            bool result = false;

            _eventHandler.ClearResponseBuffer();
            Transmit(packet);
            _eventHandler.WaitResponse(
                (packetType, dataReader) => {
                    if (packetType == SFTPPacketType.SSH_FXP_STATUS) {
                        SFTPClientErrorException exception = SFTPClientErrorException.Create(dataReader);
                        if (exception.ID == requestId) {
                            if (exception.Code == SFTPStatusCode.SSH_FX_EOF) {
                                result = true;
                                return true;    // processed
                            }
                            else {
                                throw exception;
                            }
                        }
                    }
                    else if (packetType == SFTPPacketType.SSH_FXP_NAME) {
                        uint id = dataReader.ReadUInt32();
                        if (id == requestId) {
                            uint count = (uint)dataReader.ReadInt32();

                            // use Encoding object with replacement fallback
                            Encoding encoding = Encoding.GetEncoding(
                                                _encoding.CodePage,
                                                EncoderFallback.ReplacementFallback,
                                                DecoderFallback.ReplacementFallback);

                            for (int i = 0; i < count; i++) {
                                SFTPFileInfo fileInfo = ReadFileInfo(dataReader, encoding);
                                fileList.Add(fileInfo);
                            }

                            result = true;   // OK, received SSH_FXP_NAME

                            return true;    // processed
                        }
                    }

                    return false;   // ignored
                },
                _protocolTimeout
            );

            // sanity check
            if (!Volatile.Read(ref result)) {
                throw new SFTPClientException("Missing SSH_FXP_NAME");
            }

            return fileList;
        }

        private SFTPFileInfo ReadFileInfo(SSHDataReader dataReader, Encoding encoding) {
            byte[] fileNameData = dataReader.ReadByteString();
            string fileName = encoding.GetString(fileNameData);
            byte[] longNameData = dataReader.ReadByteString();
            string longName = encoding.GetString(longNameData);

            SFTPFileAttributes attributes = ReadFileAttributes(dataReader);

            return new SFTPFileInfo(fileName, longName, attributes);
        }

        private SFTPFileAttributes ReadFileAttributes(SSHDataReader dataReader) {
            ulong fileSize = 0;
            uint uid = 0;
            uint gid = 0;
            uint permissions = 0666;
            uint atime = 0;
            uint mtime = 0;

            uint flags = (uint)dataReader.ReadInt32();

            if ((flags & SSH_FILEXFER_ATTR_SIZE) != 0) {
                fileSize = dataReader.ReadUInt64();
            }

            if ((flags & SSH_FILEXFER_ATTR_UIDGID) != 0) {
                uid = dataReader.ReadUInt32();
                gid = dataReader.ReadUInt32();
            }

            if ((flags & SSH_FILEXFER_ATTR_PERMISSIONS) != 0) {
                permissions = dataReader.ReadUInt32();
            }

            if ((flags & SSH_FILEXFER_ATTR_ACMODTIME) != 0) {
                atime = dataReader.ReadUInt32();
                mtime = dataReader.ReadUInt32();
            }

            if ((flags & SSH_FILEXFER_ATTR_EXTENDED) != 0) {
                int count = dataReader.ReadInt32();
                for (int i = 0; i < count; i++) {
                    dataReader.ReadString();    // extended type
                    dataReader.ReadString();    // extended data
                }
            }

            return new SFTPFileAttributes(fileSize, uid, gid, permissions, atime, mtime);
        }

        #endregion

        #region Private methods about handle

        private byte[] WaitHandle(SFTPPacket requestPacket, uint requestId) {
            byte[] handle = null;
            _eventHandler.ClearResponseBuffer();
            Transmit(requestPacket);
            _eventHandler.WaitResponse(
                delegate(SFTPPacketType packetType, SSHDataReader dataReader) {
                    if (packetType == SFTPPacketType.SSH_FXP_STATUS) {
                        SFTPClientErrorException exception = SFTPClientErrorException.Create(dataReader);
                        if (exception.ID == requestId)
                            throw exception;
                    }
                    else if (packetType == SFTPPacketType.SSH_FXP_HANDLE) {
                        uint id = dataReader.ReadUInt32();
                        if (id == requestId) {
                            handle = dataReader.ReadByteString();
                            return true;    // processed
                        }
                    }

                    return false;   // ignored
                },
                _protocolTimeout
            );

            // sanity check
            if (Volatile.Read(ref handle) == null) {
                throw new SFTPClientException("Missing SSH_FXP_HANDLE");
            }

            return handle;
        }

        private void CloseHandle(uint requestId, byte[] handle) {
            SFTPPacket packet =
                new SFTPPacket(SFTPPacketType.SSH_FXP_CLOSE)
                    .WriteUInt32(requestId)
                    .WriteAsString(handle);

            TransmitPacketAndWaitForStatusOK(requestId, packet);
        }

        #endregion

        #region Other private methods

        private void Transmit(SFTPPacket packet) {
            _channel.Send(packet.GetImage());
        }

        private void CheckStatus() {
            if (_closed || _eventHandler.ChannelStatus != SFTPChannelStatus.READY)
                throw new SFTPClientInvalidStatusException();
        }

        #endregion
    }

    /// <summary>
    /// Channel status
    /// </summary>
    internal enum SFTPChannelStatus {
        UNKNOWN,
        READY,
        CLOSED,
        ERROR,
    }

    /// <summary>
    /// Delegate that handles SFTP packet data.
    /// </summary>
    /// <param name="packetType">SFTP packet type</param>
    /// <param name="dataReader">Data reader which can read SFTP data.</param>
    /// <returns>true if the packet was processed.</returns>
    internal delegate bool DataReceivedDelegate(SFTPPacketType packetType, SSHDataReader dataReader);

    /// <summary>
    /// Channel data handler for SFTPClient
    /// </summary>
    internal class SFTPClientChannelEventHandler : SimpleSSHChannelEventHandler {

        #region Private fields

        private SFTPChannelStatus _channelStatus = SFTPChannelStatus.UNKNOWN;

        private readonly object _statusChangeNotifier = new object();

        private readonly ByteBuffer _dataBuffer = new ByteBuffer(0x10000, -1);
        private readonly object _dataBufferSync = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Gets an object for synchronizing change of the channel status.
        /// </summary>
        public object StatusChangeNotifier {
            get {
                return _statusChangeNotifier;
            }
        }

        /// <summary>
        /// Gets channel status
        /// </summary>
        public SFTPChannelStatus ChannelStatus {
            get {
                return _channelStatus;
            }
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Crear response buffer
        /// </summary>
        public void ClearResponseBuffer() {
            lock (_dataBufferSync) {
                _dataBuffer.Clear();
            }
        }

        /// <summary>
        /// Wait for response.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Caller should lock ResponseNotifier before send a request packet,
        /// and this method should be called in the lock-block.
        /// </para>
        /// </remarks>
        /// <param name="responseHandler">delegate which handles response data</param>
        /// <param name="millisecondsTimeout">timeout in milliseconds</param>
        /// <exception cref="SFTPClientTimeoutException">Timeout has occured.</exception>
        /// <exception cref="Exception">an exception which was thrown while executing responseHandler.</exception>
        public void WaitResponse(DataReceivedDelegate responseHandler, int millisecondsTimeout) {
            lock (_dataBufferSync) {
                bool processed = false;
                while (!processed) {
                    while (_dataBuffer.Length < 4) {
                        if (!Monitor.Wait(_dataBufferSync, millisecondsTimeout)) {
                            throw new SFTPClientTimeoutException();
                        }
                    }

                    int totalSize = SSHUtil.ReadInt32(_dataBuffer.RawBuffer, _dataBuffer.RawBufferOffset);

                    while (_dataBuffer.Length < 4 + totalSize) {
                        if (!Monitor.Wait(_dataBufferSync, millisecondsTimeout)) {
                            throw new SFTPClientTimeoutException();
                        }
                    }

                    _dataBuffer.RemoveHead(4);      // length field

                    if (totalSize >= 1) {
                        SSH2DataReader reader = new SSH2DataReader(
                                    new DataFragment(_dataBuffer.RawBuffer, _dataBuffer.RawBufferOffset, totalSize));
                        SFTPPacketType packetType = (SFTPPacketType)reader.ReadByte();
                        if (responseHandler != null) {
                            processed = responseHandler(packetType, reader);
                        }
                        else {
                            processed = true;
                        }
                    }

                    _dataBuffer.RemoveHead(totalSize);
                }
            }
        }

        #endregion

        #region ISSHChannelEventHandler

        public override void OnData(DataFragment data) {
#if DUMP_PACKET
            Dump("SFTP: OnData", data, offset, length);
#endif
            lock (_dataBufferSync) {
                _dataBuffer.Append(data);
                Monitor.PulseAll(_dataBufferSync);
            }
        }

        public override void OnClosed(bool byServer) {
            lock (StatusChangeNotifier) {
#if TRACE_RECEIVER
                System.Diagnostics.Debug.WriteLine("SFTP: Closed");
#endif
                TransitStatus(SFTPChannelStatus.CLOSED);
            }
        }

        public override void OnError(Exception error) {
            lock (StatusChangeNotifier) {
#if TRACE_RECEIVER
                System.Diagnostics.Debug.WriteLine("SFTP: Error: " + error.Message);
#endif
                TransitStatus(SFTPChannelStatus.ERROR);
            }
        }

        public override void OnReady() {
            lock (StatusChangeNotifier) {
#if TRACE_RECEIVER
                System.Diagnostics.Debug.WriteLine("SFTP: OnChannelReady");
#endif
                TransitStatus(SFTPChannelStatus.READY);
            }
        }

        #endregion

        #region Private methods

        private void TransitStatus(SFTPChannelStatus newStatus) {
            _channelStatus = newStatus;
            Monitor.PulseAll(StatusChangeNotifier);
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
