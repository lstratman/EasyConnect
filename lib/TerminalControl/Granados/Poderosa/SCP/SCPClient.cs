/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SCPClient.cs,v 1.2 2012/05/05 12:42:45 kzmi Exp $
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

using Granados.SSH1;
using Granados.SSH2;
using Granados.IO;
using Granados.IO.SSH2;
using Granados.Util;
using Granados.Poderosa.FileTransfer;

namespace Granados.Poderosa.SCP {

    /// <summary>
    /// Statuses of the file transfer
    /// </summary>
    public enum SCPFileTransferStatus {
        /// <summary>Not started</summary>
        None,
        /// <summary>Creating a local or remote directory</summary>
        CreateDirectory,
        /// <summary>Created a local or remote directory</summary>
        DirectoryCreated,
        /// <summary>Opening remote or local file</summary>
        Open,
        /// <summary>Data is transmitting</summary>
        Transmitting,
        /// <summary>File was closed successfully</summary>
        CompletedSuccess,
        /// <summary>File transfer was aborted by cancellation</summary>
        CompletedAbort,
    }

    /// <summary>
    /// Delegate notifies progress of the file transfer.
    /// </summary>
    /// <param name="localFullPath">Full path of the local file or local directory.</param>
    /// <param name="fileName">Name of the file or directory.</param>
    /// <param name="status">Status of the file transfer.</param>
    /// <param name="fileSize">File size. Zero if target is a directory.</param>
    /// <param name="transmitted">Transmitted data length. Zero if target is a directory.</param>
    public delegate void SCPFileTransferProgressDelegate(
            string localFullPath, string fileName, SCPFileTransferStatus status, ulong fileSize, ulong transmitted);

    /// <summary>
    /// SCP Client
    /// </summary>
    /// 
    /// <remarks>
    /// <para>This class is designed to be used in the worker thread.</para>
    /// <para>Each method blocks thread while transmitting the files.</para>
    /// </remarks>
    public class SCPClient {

        #region Private classes

        /// <summary>
        /// File or directory entry to create.
        /// Used in sink mode.
        /// </summary>
        private class SCPEntry {
            public readonly bool IsDirectory;
            public readonly int Permissions;
            public readonly long FileSize;
            public readonly string Name;

            public SCPEntry(bool isDirectory, int permissions, long fileSize, string name) {
                IsDirectory = isDirectory;
                Permissions = permissions;
                FileSize = fileSize;
                Name = name;
            }
        }

        /// <summary>
        /// Time modification informations.
        /// Used in sink mode.
        /// </summary>
        private class SCPModTime {
            public readonly DateTime MTime;
            public readonly DateTime ATime;

            public SCPModTime(DateTime mtime, DateTime atime) {
                MTime = mtime;
                ATime = atime;
            }
        }

        #endregion

        #region Private fields

        private readonly ISSHConnection _connection;

        private Encoding _encoding = Encoding.UTF8;

        private int _defaultFilePermissions = 0x1a4;        // 0644
        private int _defaultDirectoryPermissions = 0x1ed;   // 0755

        private int _protocolTimeout = 5000;

        private static readonly byte[] ZERO = new byte[] { 0 };
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0);

        #endregion

        #region Private constants

        private const int FILE_TRANSFER_BLOCK_SIZE = 0x8000;

        private const byte LF = 0xa;

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
        /// Unix permissions used when creating a remote file.
        /// </summary>
        public int DefaultFilePermissions {
            get {
                return _defaultFilePermissions;
            }
            set {
                _defaultFilePermissions = value;
            }
        }

        /// <summary>
        /// Unix permissions used when creating a remote directory.
        /// </summary>
        public int DefaultDirectoryPermissions {
            get {
                return _defaultDirectoryPermissions;
            }
            set {
                _defaultDirectoryPermissions = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connection">SSH connection. Currently only SSH2 connection is accepted.</param>
        public SCPClient(ISSHConnection connection) {
            this._connection = connection;
        }

        #endregion

        #region Upload

        /// <summary>
        /// Upload files or directories.
        /// </summary>
        /// <remarks>
        /// <para>Unfortunately, Granados sends a command line in the ASCII encoding.
        /// So the "remotePath" must be an ASCII text.</para>
        /// </remarks>
        /// <param name="localPath">Local path (Windows' path)</param>
        /// <param name="remotePath">Remote path (Unix path)</param>
        /// <param name="recursive">Specifies recursive mode</param>
        /// <param name="preserveTime">Specifies to preserve time of the directory or file.</param>
        /// <param name="cancellation">An object to request the cancellation. Set null if the cancellation is not needed.</param>
        /// <param name="progressDelegate">Delegate to notify progress. Set null if notification is not needed.</param>
        public void Upload(string localPath, string remotePath, bool recursive, bool preserveTime,
                            Cancellation cancellation,
                            SCPFileTransferProgressDelegate progressDelegate) {
            if (!IsAscii(remotePath))
                throw new SCPClientException("Remote path must consist of ASCII characters.");

            bool isDirectory = Directory.Exists(localPath);
            if (!File.Exists(localPath) && !isDirectory)
                throw new SCPClientException("File or directory not found: " + localPath);

            if (isDirectory && !recursive)
                throw new SCPClientException("Cannot copy directory in non-recursive mode");

            string absLocalPath = Path.GetFullPath(localPath);

            string command = "scp -t ";
            if (recursive)
                command += "-r ";
            if (preserveTime)
                command += "-p ";
            command += EscapeUnixPath(remotePath);

            using (SCPChannelStream stream = new SCPChannelStream()) {
                stream.Open(_connection, command, _protocolTimeout);
                CheckResponse(stream);

                if (isDirectory) {  // implies recursive mode
                    UploadDirectory(absLocalPath, stream, preserveTime, cancellation, progressDelegate);
                }
                else {
                    UploadFile(absLocalPath, stream, preserveTime, cancellation, progressDelegate);
                }
            }
        }

        private bool UploadDirectory(string fullPath, SCPChannelStream stream, bool preserveTime,
                        Cancellation cancellation,
                        SCPFileTransferProgressDelegate progressDelegate) {

            Debug.Assert(fullPath != null);

            if (cancellation != null && cancellation.IsRequested) {
                return false;   // cancel
            }

            string directoryName = Path.GetFileName(fullPath);
            if (directoryName != null) { // not a root directory
                if (progressDelegate != null)
                    progressDelegate(fullPath, directoryName, SCPFileTransferStatus.CreateDirectory, 0, 0);

                if (preserveTime) {
                    SendModTime(
                        stream,
                        Directory.GetLastWriteTimeUtc(fullPath),
                        Directory.GetLastAccessTimeUtc(fullPath)
                    );
                }

                string line = new StringBuilder()
                    .Append('D')
                    .Append(GetPermissionsText(true))
                    .Append(" 0 ")
                    .Append(directoryName)
                    .Append('\n')
                    .ToString();
                stream.Write(_encoding.GetBytes(line));
                CheckResponse(stream);

                if (progressDelegate != null)
                    progressDelegate(fullPath, directoryName, SCPFileTransferStatus.DirectoryCreated, 0, 0);
            }

            foreach (String fullSubDirPath in Directory.GetDirectories(fullPath)) {
                bool continued = UploadDirectory(fullSubDirPath, stream, preserveTime, cancellation, progressDelegate);
                if (!continued)
                    return false;   // cancel
            }

            foreach (String fullFilePath in Directory.GetFiles(fullPath)) {
                bool continued = UploadFile(fullFilePath, stream, preserveTime, cancellation, progressDelegate);
                if (!continued)
                    return false;   // cancel
            }

            if (directoryName != null) { // not a root directory
                string line = "E\n";
                stream.Write(_encoding.GetBytes(line));
                CheckResponse(stream);
            }

            return true;    // continue
        }

        private bool UploadFile(string fullPath, SCPChannelStream stream, bool preserveTime,
                        Cancellation cancellation,
                        SCPFileTransferProgressDelegate progressDelegate) {

            Debug.Assert(fullPath != null);

            string fileName = Path.GetFileName(fullPath);
            FileInfo fileInfo = new FileInfo(fullPath);
            long fileSize = fileInfo.Length;
            ulong transmitted = 0;

            if (progressDelegate != null)
                progressDelegate(fullPath, fileName, SCPFileTransferStatus.Open, (ulong)fileSize, transmitted);

            if (preserveTime) {
                SendModTime(
                    stream,
                    File.GetLastWriteTimeUtc(fullPath),
                    File.GetLastAccessTimeUtc(fullPath)
                );
            }

            using (FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                string line = new StringBuilder()
                    .Append('C')
                    .Append(GetPermissionsText(false))
                    .Append(' ')
                    .Append(fileSize.ToString(NumberFormatInfo.InvariantInfo))
                    .Append(' ')
                    .Append(fileName)
                    .Append('\n')
                    .ToString();
                stream.Write(_encoding.GetBytes(line));
                CheckResponse(stream);

                byte[] buff = new byte[stream.GetPreferredDatagramSize()];

                long remain = fileSize;
                while (remain > 0) {
                    if (cancellation != null && cancellation.IsRequested) {
                        if (progressDelegate != null)
                            progressDelegate(fullPath, fileName, SCPFileTransferStatus.CompletedAbort, (ulong)fileSize, transmitted);
                        return false;   // cancel
                    }

                    int readLength = fileStream.Read(buff, 0, buff.Length);
                    if (readLength <= 0)
                        break;
                    if (readLength > remain)
                        readLength = (int)remain;
                    stream.Write(buff, readLength);
                    remain -= readLength;

                    transmitted += (ulong)readLength;
                    if (progressDelegate != null)
                        progressDelegate(fullPath, fileName, SCPFileTransferStatus.Transmitting, (ulong)fileSize, transmitted);
                }

                stream.Write(ZERO);
                CheckResponse(stream);
            }

            if (progressDelegate != null)
                progressDelegate(fullPath, fileName, SCPFileTransferStatus.CompletedSuccess, (ulong)fileSize, transmitted);

            return true;
        }

        private void SendModTime(SCPChannelStream stream, DateTime mtime, DateTime atime) {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);
            long mtimeSec = (mtime.Ticks - epoch.Ticks) / 10000000L;
            long mtimeUSec = ((mtime.Ticks - epoch.Ticks) % 10000000L) / 10L;
            long atimeSec = (atime.Ticks - epoch.Ticks) / 10000000L;
            long atimeUSec = ((atime.Ticks - epoch.Ticks) % 10000000L) / 10L;
            string line = new StringBuilder()
                    .Append('T')
                    .Append(mtimeSec.ToString(NumberFormatInfo.InvariantInfo))
                    .Append(' ')
                    .Append(mtimeUSec.ToString(NumberFormatInfo.InvariantInfo))
                    .Append(' ')
                    .Append(atimeSec.ToString(NumberFormatInfo.InvariantInfo))
                    .Append(' ')
                    .Append(atimeUSec.ToString(NumberFormatInfo.InvariantInfo))
                    .Append('\n')
                    .ToString();
            stream.Write(_encoding.GetBytes(line));
            CheckResponse(stream);
        }

        #endregion

        #region Download

        /// <summary>
        /// Download files or directories.
        /// </summary>
        /// <remarks>
        /// <para>Unfortunately, Granados sends a command line in the ASCII encoding.
        /// So the "remotePath" must be an ASCII text.</para>
        /// </remarks>
        /// <param name="remotePath">Remote path (Unix path)</param>
        /// <param name="localPath">Local path (Windows' path)</param>
        /// <param name="recursive">Specifies recursive mode</param>
        /// <param name="preserveTime">Specifies to preserve time of the directory or file.</param>
        /// <param name="cancellation">An object to request the cancellation. Set null if the cancellation is not needed.</param>
        /// <param name="progressDelegate">Delegate to notify progress. Set null if notification is not needed.</param>
        public void Download(string remotePath, string localPath, bool recursive, bool preserveTime,
                        Cancellation cancellation,
                        SCPFileTransferProgressDelegate progressDelegate) {

            if (!IsAscii(remotePath))
                throw new SCPClientException("Remote path must consist of ASCII characters.");

            string absLocalPath = Path.GetFullPath(localPath);

            string command = "scp -f ";
            if (recursive)
                command += "-r ";
            if (preserveTime)
                command += "-p ";
            command += EscapeUnixPath(remotePath);

            string localBasePath = null;    // local directory to store
            SCPModTime modTime = null;

            Stack<string> localBasePathStack = new Stack<string>();

            using (SCPChannelStream stream = new SCPChannelStream()) {
                stream.Open(_connection, command, _protocolTimeout);
                stream.Write(ZERO);

                while (true) {
                    byte[] lineBytes = stream.ReadUntil(LF, _protocolTimeout);
                    if (lineBytes[0] == 1 || lineBytes[0] == 2) {
                        // Warning or Error
                        string message = _encoding.GetString(lineBytes, 1, lineBytes.Length - 2);
                        throw new SCPClientException(message);
                    }

                    if (lineBytes[0] == 0x43 /*'C'*/ || lineBytes[0] == 0x44 /*'D'*/) {
                        SCPEntry entry;
                        try {
                            entry = ParseEntry(lineBytes);
                        }
                        catch (Exception e) {
                            SendError(stream, e.Message);
                            throw;
                        }

                        if (entry.IsDirectory) {
                            string directoryPath = DeterminePathToCreate(localBasePath, absLocalPath, entry);
                            bool continued = CreateDirectory(stream, directoryPath, modTime, cancellation, progressDelegate);
                            if (!continued)
                                break;
                            modTime = null;
                            localBasePathStack.Push(localBasePath);
                            localBasePath = directoryPath;
                        }
                        else {
                            string filePath = DeterminePathToCreate(localBasePath, absLocalPath, entry);
                            bool continued = CreateFile(stream, filePath, entry, modTime, cancellation, progressDelegate);
                            if (!continued)
                                break;
                            modTime = null;
                            if (!recursive)
                                break;
                        }
                    }
                    else if (lineBytes[0] == 0x54 /*'T'*/) {
                        if (preserveTime) {
                            try {
                                modTime = ParseModTime(lineBytes);
                            }
                            catch (Exception e) {
                                SendError(stream, e.Message);
                                throw;
                            }
                        }
                        stream.Write(ZERO);
                    }
                    else if (lineBytes[0] == 0x45 /*'E'*/) {
                        if (localBasePathStack.Count > 0) {
                            localBasePath = localBasePathStack.Pop();
                            if (localBasePath == null)
                                break;
                        }
                        stream.Write(ZERO);
                    }
                    else {
                        SendError(stream, "Invalid control");
                        throw new SCPClientException("Invalid control");
                    }
                }

            }
        }

        private string DeterminePathToCreate(string localBasePath, string initialLocalPath, SCPEntry entry) {
            Debug.Assert(initialLocalPath != null);

            if (localBasePath == null) {    // first creation
                // use initialLocalPath
                if (Directory.Exists(initialLocalPath))
                    return Path.Combine(initialLocalPath, entry.Name);
                else
                    return initialLocalPath;
            }
            else {
                return Path.Combine(localBasePath, entry.Name);
            }
        }

        private bool CreateDirectory(SCPChannelStream stream, string directoryPath, SCPModTime modTime,
                            Cancellation cancellation,
                            SCPFileTransferProgressDelegate progressDelegate) {

            if (cancellation != null && cancellation.IsRequested) {
                return false;   // cancel
            }

            string directoryName = Path.GetFileName(directoryPath);

            if (progressDelegate != null)
                progressDelegate(directoryPath, directoryName, SCPFileTransferStatus.CreateDirectory, 0, 0);

            if (!Directory.Exists(directoryPath)) { // skip if already exists
                try {
                    Directory.CreateDirectory(directoryPath);
                }
                catch (Exception e) {
                    SendError(stream, "failed to create a directory");
                    throw new SCPClientException("Failed to create a directory: " + directoryPath, e);
                }
            }

            if (modTime != null) {
                try {
                    Directory.SetLastWriteTimeUtc(directoryPath, modTime.MTime);
                    Directory.SetLastAccessTimeUtc(directoryPath, modTime.ATime);
                }
                catch (Exception e) {
                    SendError(stream, "failed to modify time of a directory");
                    throw new SCPClientException("Failed to modify time of a directory: " + directoryPath, e);
                }
            }

            stream.Write(ZERO);

            if (progressDelegate != null)
                progressDelegate(directoryPath, directoryName, SCPFileTransferStatus.DirectoryCreated, 0, 0);

            return true;
        }

        private bool CreateFile(SCPChannelStream stream, string filePath, SCPEntry entry, SCPModTime modTime,
                    Cancellation cancellation,
                    SCPFileTransferProgressDelegate progressDelegate) {

            string fileName = Path.GetFileName(filePath);
            ulong transmitted = 0;

            if (progressDelegate != null)
                progressDelegate(filePath, fileName, SCPFileTransferStatus.Open, (ulong)entry.FileSize, transmitted);

            FileStream fileStream;
            try {
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            }
            catch (Exception e) {
                SendError(stream, "failed to create a file");
                throw new SCPClientException("Failed to create a file: " + filePath, e);
            }
            stream.Write(ZERO);

            using (fileStream) {
                byte[] buff = new byte[FILE_TRANSFER_BLOCK_SIZE];
                long remain = entry.FileSize;
                try {
                    while (remain > 0) {
                        if (cancellation != null && cancellation.IsRequested) {
                            if (progressDelegate != null)
                                progressDelegate(filePath, fileName, SCPFileTransferStatus.CompletedAbort, (ulong)entry.FileSize, transmitted);
                            return false;   // cancel
                        }

                        int maxLength = (int)Math.Min((long)buff.Length, remain);
                        int readLength = stream.Read(buff, maxLength, _protocolTimeout);
                        fileStream.Write(buff, 0, readLength);
                        remain -= readLength;

                        transmitted += (ulong)readLength;
                        if (progressDelegate != null)
                            progressDelegate(filePath, fileName, SCPFileTransferStatus.Transmitting, (ulong)entry.FileSize, transmitted);
                    }
                }
                catch (Exception e) {
                    SendError(stream, "failed to write to a file");
                    throw new SCPClientException("Failed to write to a file: " + filePath, e);
                }
            }

            if (modTime != null) {
                try {
                    File.SetLastWriteTimeUtc(filePath, modTime.MTime);
                    File.SetLastAccessTimeUtc(filePath, modTime.ATime);
                }
                catch (Exception e) {
                    SendError(stream, "failed to modify time of a file");
                    throw new SCPClientException("Failed to modify time of a file: " + filePath, e);
                }
            }

            CheckResponse(stream);
            stream.Write(ZERO);

            if (progressDelegate != null)
                progressDelegate(filePath, fileName, SCPFileTransferStatus.CompletedSuccess, (ulong)entry.FileSize, transmitted);

            return true;
        }

        private Regex _regexEntry = new Regex("^([CD])([0-7]+) +([0-9]+) +(.+)$");
        private Regex _regexModTime = new Regex("^T([0-9]+) +([0-9]+) +([0-9]+) +([0-9]+)$");

        private SCPEntry ParseEntry(byte[] lineData) {
            string line = _encoding.GetString(lineData, 0, lineData.Length - 1);
            Match m = _regexEntry.Match(line);
            if (!m.Success)
                throw new SCPClientException("Unknown entry: " + line);

            bool isDirectory = (m.Groups[1].Value == "D");

            int permissions = 0;
            foreach (char c in m.Groups[2].Value.ToCharArray()) {
                permissions = (permissions << 3) | ((int)c - (int)'0');
            }

            long fileSize = Int64.Parse(m.Groups[3].Value);
            string name = m.Groups[4].Value;

            return new SCPEntry(isDirectory, permissions, fileSize, name);
        }

        private SCPModTime ParseModTime(byte[] lineData) {
            string line = _encoding.GetString(lineData, 0, lineData.Length - 1);
            Match m = _regexModTime.Match(line);
            if (!m.Success)
                throw new SCPClientException("Unknown format: " + line);

            long mtimeSec = Int64.Parse(m.Groups[1].Value);
            long mtimeUSec = Int64.Parse(m.Groups[2].Value);
            long atimeSec = Int64.Parse(m.Groups[3].Value);
            long atimeUSec = Int64.Parse(m.Groups[4].Value);

            DateTime mtime = new DateTime(EPOCH.Ticks + mtimeSec * 10000000L + mtimeUSec * 10);
            DateTime atime = new DateTime(EPOCH.Ticks + atimeSec * 10000000L + atimeUSec * 10);

            return new SCPModTime(mtime, atime);
        }

        private void SendError(SCPChannelStream stream, string message) {
            message = "Poderosa: " + message.Replace('\n', ' ');
            byte[] messageBytes = _encoding.GetBytes(message);
            byte[] data = new byte[messageBytes.Length + 2];
            data[0] = 1;
            Buffer.BlockCopy(messageBytes, 0, data, 1, messageBytes.Length);
            data[messageBytes.Length + 1] = LF;
            stream.Write(data);
        }

        #endregion

        #region Other private methods

        /// <summary>
        /// Read a byte and check the status code.
        /// </summary>
        /// <param name="stream">Channel stream</param>
        /// <exception cref="SCPClientException">Response was a warning or an error.</exception>
        private void CheckResponse(SCPChannelStream stream) {
            byte response = stream.ReadByte(_protocolTimeout);
            if (response == 0) {
                // OK
                return;
            }

            if (response == 1 || response == 2) {
                // Warning or Error
                // followed by a message which is terminated by LF
                byte[] messageData = stream.ReadUntil(LF, _protocolTimeout);
                string message = _encoding.GetString(messageData, 0, messageData.Length - 1);
                throw new SCPClientException(message);
            }

            throw new SCPClientException("Invalid response");
        }

        private string GetPermissionsText(bool isDirectory) {
            int perm = isDirectory ? _defaultDirectoryPermissions : _defaultFilePermissions;
            return new StringBuilder()
                .Append("01234567"[(perm >> 9) & 0x7])
                .Append("01234567"[(perm >> 6) & 0x7])
                .Append("01234567"[(perm >> 3) & 0x7])
                .Append("01234567"[perm & 0x7])
                .ToString();
        }

        private string EscapeUnixPath(string path) {
            return Regex.Replace(path, @"[\\\][{}()<>|'"" ]", @"\$0");
        }

        private bool IsAscii(string s) {
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];
                if (c < '\u0020' || c > '\u007e')
                    return false;
            }
            return true;
        }

        #endregion
    }
}
