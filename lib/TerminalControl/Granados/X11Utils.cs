// Copyright (c) 2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.IO;
using Granados.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Granados.X11 {

    /// <summary>
    /// Exception for X11 utilities
    /// </summary>
    internal class X11UtilException : Exception {
        public X11UtilException(string message)
            : base(message) {
        }
        public X11UtilException(string message, Exception exception)
            : base(message, exception) {
        }
    }

    /// <summary>
    /// X protocol message builder
    /// </summary>
    internal class XProtocolMessage {

        private readonly bool _bigEndian;
        private readonly byte[] workbuf = new byte[4];

        private readonly ByteBuffer _buffer = new ByteBuffer(1024, -1);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bigEndian">specifies that the byte-order is big-endian</param>
        public XProtocolMessage(bool bigEndian) {
            _bigEndian = bigEndian;
        }

        /// <summary>
        /// Return a <see cref="DataFragment"/> that represents the message data.
        /// </summary>
        /// <returns><see cref="DataFragment"/> object</returns>
        public DataFragment AsDataFragment() {
            return _buffer.AsDataFragment();
        }

        /// <summary>
        /// Clears the internal buffer.
        /// </summary>
        /// <returns>this instance</returns>
        public XProtocolMessage Clear() {
            _buffer.Clear();
            return this;
        }

        /// <summary>
        /// Appends a single byte.
        /// </summary>
        /// <param name="v">byte data</param>
        /// <returns>this instance</returns>
        public XProtocolMessage AppendByte(byte v) {
            _buffer.WriteByte(v);
            return this;
        }

        /// <summary>
        /// Appends a word value.
        /// </summary>
        /// <param name="v">word data</param>
        /// <returns>this instance</returns>
        public XProtocolMessage AppendUInt16(ushort v) {
            if (_bigEndian) {
                workbuf[0] = (byte)(v >> 8);
                workbuf[1] = (byte)v;
            }
            else {
                workbuf[0] = (byte)v;
                workbuf[1] = (byte)(v >> 8);
            }
            _buffer.Append(workbuf, 0, 2);
            return this;
        }

        /// <summary>
        /// Appends a double word value.
        /// </summary>
        /// <param name="v">double word data</param>
        /// <returns>this instance</returns>
        public XProtocolMessage AppendUInt32(uint v) {
            if (_bigEndian) {
                workbuf[0] = (byte)(v >> 24);
                workbuf[1] = (byte)(v >> 16);
                workbuf[2] = (byte)(v >> 8);
                workbuf[3] = (byte)v;
            }
            else {
                workbuf[0] = (byte)v;
                workbuf[1] = (byte)(v >> 8);
                workbuf[2] = (byte)(v >> 16);
                workbuf[3] = (byte)(v >> 24);
            }
            _buffer.Append(workbuf, 0, 4);
            return this;
        }

        /// <summary>
        /// Appends the byte string.
        /// </summary>
        /// <param name="s">byte string</param>
        /// <returns>this instance</returns>
        public XProtocolMessage AppendBytes(byte[] s) {
            _buffer.Append(s);
            return this;
        }

        /// <summary>
        /// Appends the padding bytes for the specified byte string.
        /// </summary>
        /// <param name="s">byte string</param>
        /// <returns>this instance</returns>
        public XProtocolMessage AppendPaddingBytesOf(byte[] s) {
            int pad = (4 - (s.Length % 4)) % 4;
            for (int i = 0; i < pad; ++i) {
                _buffer.WriteByte(0);
            }
            return this;
        }
    }

    /// <summary>
    /// Data reader for the X protocol message
    /// </summary>
    internal class XDataReader {

        private readonly bool _bigEndian;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bigEndian">specifies that the byte-order is big-endian</param>
        public XDataReader(bool bigEndian) {
            _bigEndian = bigEndian;
        }

        /// <summary>
        /// Reads a word value.
        /// </summary>
        /// <param name="buff">buffer</param>
        /// <param name="offset">index where the data is read</param>
        /// <returns>a value read from the buffer</returns>
        public ushort ReadUInt16(byte[] buff, int offset) {
            byte v1 = buff[offset];
            byte v2 = buff[offset + 1];
            if (_bigEndian) {
                return (ushort)((v1 << 8) | v2);
            }
            else {
                return (ushort)((v2 << 8) | v1);
            }
        }

        /// <summary>
        /// Reads a double word value.
        /// </summary>
        /// <param name="buff">buffer</param>
        /// <param name="offset">index where the data is read</param>
        /// <returns>a value read from the buffer</returns>
        public uint ReadUInt32(byte[] buff, int offset) {
            byte v1 = buff[offset];
            byte v2 = buff[offset + 1];
            byte v3 = buff[offset + 2];
            byte v4 = buff[offset + 3];
            if (_bigEndian) {
                return ((uint)v1 << 24)
                     | ((uint)v2 << 16)
                     | ((uint)v3 << 8)
                     | ((uint)v4);
            }
            else {
                return ((uint)v1)
                     | ((uint)v2 << 8)
                     | ((uint)v3 << 16)
                     | ((uint)v4 << 24);
            }
        }

        /// <summary>
        /// Reads byte string.
        /// </summary>
        /// <param name="buff">buffer</param>
        /// <param name="offset">index where the data is read</param>
        /// <param name="length">lengt of bytes to read</param>
        /// <returns>new byte array</returns>
        public byte[] ReadBytes(byte[] buff, int offset, int length) {
            byte[] data = new byte[length];
            Buffer.BlockCopy(buff, offset, data, 0, length);
            return data;
        }
    }

    /// <summary>
    /// Entry in the .Xauthority file.
    /// </summary>
    internal class XauthorityEntry {
        public readonly ushort Family; // connection type
        public readonly string Address;    // address or host name
        public readonly int Number;    // display number
        public readonly string Name;   // authorization protocol name
        public readonly byte[] Data;   // authorization protocol data

        public XauthorityEntry(ushort family, string address, int number, string name, byte[] data) {
            this.Family = family;
            this.Address = address;
            this.Number = number;
            this.Name = name;
            this.Data = data;
        }
    }

    /// <summary>
    /// .Xauthority file parser
    /// </summary>
    internal class XauthorityParser {

        private readonly byte[] _workbuf = new byte[4];
        private readonly XDataReader _bigEndianDataReader = new XDataReader(true);

        /// <summary>
        /// Constructor
        /// </summary>
        public XauthorityParser() {
        }

        /// <summary>
        /// Find an entry which is most matched to the display number and the connection type.
        /// </summary>
        /// <param name="xauthFile">path to the .Xauthority file</param>
        /// <param name="display">display number</param>
        /// <returns>best-matched entry if it was found. otherwise null.</returns>
        public XauthorityEntry FindBest(string xauthFile, int display) {
            // from Xauth.h
            const ushort FamilyWild = 65535;

            string hostName = Dns.GetHostEntry("").HostName;    // get fully qualified hostname

            XauthorityEntry candidate = null;

            foreach (var entry in ReadEntries(xauthFile)) {
                if (entry.Number != display) {
                    continue;
                }
                if (entry.Family == FamilyWild) {
                    candidate = entry;
                }
                if (entry.Address == hostName) {
                    return entry;   // best
                }
            }
            return candidate;
        }

        private XauthorityEntry[] ReadEntries(string xauthFile) {
            try {
                var entries = new List<XauthorityEntry>();

                using (var fs = new FileStream(xauthFile, FileMode.Open, FileAccess.Read)) {
                    while (true) {
                        ushort family;
                        if (!ReadUInt16BE(fs, out family)) {
                            break;
                        }

                        string address;
                        if (!ReadString(fs, 256, out address)) {
                            break;
                        }

                        int number;
                        if (!ReadNumber(fs, 5, out number)) {
                            break;
                        }

                        string name;
                        if (!ReadString(fs, 40, out name)) {
                            break;
                        }

                        byte[] data;
                        if (!ReadBytes(fs, 1024, out data)) {
                            break;
                        }

                        entries.Add(new XauthorityEntry(family, address, number, name, data));
                    }
                }

                return entries.ToArray();
            }
            catch (X11UtilException) {
                throw;
            }
            catch (Exception e) {
                throw new X11UtilException(Strings.GetString("ReadingXauthorityFileFailed"), e);
            }
        }

        private bool ReadBytes(Stream fs, int maxLength, out byte[] val) {
            ushort len;
            if (!ReadUInt16BE(fs, out len)) {
                val = null;
                return false;
            }
            if (len > maxLength) {
                val = null;
                return false;
            }
            byte[] buff = new byte[len];
            if (fs.Read(buff, 0, len) != len) {
                val = null;
                return false;
            }

            val = buff;
            return true;
        }

        private bool ReadString(Stream fs, int maxLength, out string val) {
            byte[] buff;
            if (!ReadBytes(fs, maxLength, out buff)) {
                val = null;
                return false;
            }
            try {
                val = Encoding.ASCII.GetString(buff);
                return true;
            }
            catch (Exception) {
                val = null;
                return false;
            }
        }

        private bool ReadNumber(Stream fs, int maxLength, out int val) {
            string text;
            if (!ReadString(fs, maxLength, out text)) {
                val = 0;
                return false;
            }
            return Int32.TryParse(text, out val);
        }

        private bool ReadUInt16BE(Stream fs, out ushort val) {
            int len = fs.Read(_workbuf, 0, 2);
            if (len != 2) {
                val = 0;
                return false;
            }
            val = _bigEndianDataReader.ReadUInt16(_workbuf, 0);
            return true;
        }
    }

}
