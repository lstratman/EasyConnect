// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.Mono.Math;
using Granados.Util;
using System;
using System.IO;
using System.Text;

namespace Granados.IO {

    /// <summary>
    /// An interface of the SSH packet which provides a payload buffer.
    /// </summary>
    internal interface IPacketBuilder {
        /// <summary>
        /// Payload buffer.
        /// </summary>
        ByteBuffer Payload {
            get;
        }
    }

    /// <summary>
    /// Extension methods for writing content of the payload.
    /// </summary>
    internal static class PacketBuilderMixin {

        /// <summary>
        /// Writes binary data.
        /// </summary>
        public static T Write<T>(this T packet, byte[] data) where T : IPacketBuilder {
            packet.Payload.Append(data);
            return packet;
        }

        /// <summary>
        /// Writes single byte.
        /// </summary>
        public static T WriteByte<T>(this T packet, byte val) where T : IPacketBuilder {
            packet.Payload.WriteByte(val);
            return packet;
        }

        /// <summary>
        /// Writes boolean value as a byte.
        /// </summary>
        public static T WriteBool<T>(this T packet, bool val) where T : IPacketBuilder {
            packet.Payload.WriteByte(val ? (byte)1 : (byte)0);
            return packet;
        }

        /// <summary>
        /// Writes Int32 value in the network byte order.
        /// </summary>
        public static T WriteInt32<T>(this T packet, int val) where T : IPacketBuilder {
            packet.Payload.WriteInt32(val);
            return packet;
        }

        /// <summary>
        /// Writes UInt32 value in the network byte order.
        /// </summary>
        public static T WriteUInt32<T>(this T packet, uint val) where T : IPacketBuilder {
            packet.Payload.WriteUInt32(val);
            return packet;
        }

        /// <summary>
        /// Writes UInt64 value in the network byte order.
        /// </summary>
        public static T WriteUInt64<T>(this T packet, ulong val) where T : IPacketBuilder {
            packet.Payload.WriteUInt64(val);
            return packet;
        }

        /// <summary>
        /// Writes Int64 value in the network byte order.
        /// </summary>
        public static T WriteInt64<T>(this T packet, long val) where T : IPacketBuilder {
            packet.Payload.WriteInt64(val);
            return packet;
        }

        /// <summary>
        /// Writes ASCII text according to the SSH 1.5/2.0 specification.
        /// </summary>
        public static T WriteString<T>(this T packet, string text) where T : IPacketBuilder {
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            WriteAsString(packet, bytes);
            return packet;
        }

        /// <summary>
        /// Writes UTF-8 text according to the SSH 1.5/2.0 specification.
        /// </summary>
        public static T WriteUTF8String<T>(this T packet, string text) where T : IPacketBuilder {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            WriteAsString(packet, bytes);
            return packet;
        }

        /// <summary>
        /// Writes byte string according to the SSH 1.5/2.0 specification.
        /// </summary>
        public static T WriteAsString<T>(this T packet, byte[] data) where T : IPacketBuilder {
            packet.Payload.WriteInt32(data.Length);
            if (data.Length > 0) {
                packet.Payload.Append(data);
            }
            return packet;
        }

        /// <summary>
        /// Writes byte string according to the SSH 1.5/2.0 specification.
        /// </summary>
        public static T WriteAsString<T>(this T packet, byte[] data, int offset, int length) where T : IPacketBuilder {
            packet.Payload.WriteInt32(length);
            if (length > 0) {
                packet.Payload.Append(data, offset, length);
            }
            return packet;
        }

        /// <summary>
        /// Writes random bytes.
        /// </summary>
        public static T WriteSecureRandomBytes<T>(this T packet, int length) where T : IPacketBuilder {
            packet.Payload.WriteSecureRandomBytes(length);
            return packet;
        }

        /// <summary>
        /// Overwrites Byte value.
        /// </summary>
        public static T OverwriteByte<T>(this T packet, int offset, byte val) where T : IPacketBuilder {
            packet.Payload.OverwriteByte(offset, val);
            return packet;
        }

        /// <summary>
        /// Overwrites UInt32 value in network byte order.
        /// </summary>
        public static T OverwriteUInt32<T>(this T packet, int offset, uint val) where T : IPacketBuilder {
            packet.Payload.OverwriteUInt32(offset, val);
            return packet;
        }
    }

    /// <summary>
    /// Utility class for reading fields in the SSH packet.
    /// </summary>
    internal abstract class SSHDataReader {

        protected readonly byte[] _data;
        protected readonly int _initialIndex;
        protected readonly int _limit;
        protected int _currentIndex;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="image">a byte array to be read.</param>
        protected SSHDataReader(byte[] image) {
            _data = image;
            _initialIndex = _currentIndex = 0;
            _limit = image.Length;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">a data fragment to be read.</param>
        protected SSHDataReader(DataFragment data) {
            _data = data.Data;
            _initialIndex = _currentIndex = data.Offset;
            _limit = _currentIndex + data.Length;
        }

        /// <summary>
        /// Current reading position.
        /// </summary>
        public int Offset {
            get {
                return _currentIndex - _initialIndex;
            }
        }

        /// <summary>
        /// Number of bytes of the remaining data.
        /// </summary>
        public int RemainingDataLength {
            get {
                return _limit - _currentIndex;
            }
        }

        /// <summary>
        /// Boundary check.
        /// </summary>
        /// <param name="n">number of bytes to be read.</param>
        private void CheckEnd(int n) {
            if (_currentIndex + n > _limit) {
                throw new IOException(Strings.GetString("UnexpectedEOF"));
            }
        }

        /// <summary>
        /// Makes a new <see cref="DataFragment"/> that represents the view of the remaining data.
        /// </summary>
        /// <returns>a new <see cref="DataFragment"/>.</returns>
        public DataFragment GetRemainingDataView() {
            return new DataFragment(_data, _currentIndex, _limit - _currentIndex);
        }

        /// <summary>
        /// Makes a new <see cref="DataFragment"/> that represents the view of the remaining data.
        /// </summary>
        /// <param name="length">data length of the new view.</param>
        /// <returns>a new <see cref="DataFragment"/>.</returns>
        public DataFragment GetRemainingDataView(int length) {
            if (length < 0 || length > _limit - _currentIndex) {
                throw new ArgumentOutOfRangeException();
            }
            return new DataFragment(_data, _currentIndex, length);
        }

        /// <summary>
        /// Reads a byte value.
        /// </summary>
        /// <returns>byte value.</returns>
        public byte ReadByte() {
            const int LENGTH = 1;
            CheckEnd(LENGTH);
            byte ret = _data[_currentIndex];
            _currentIndex++;
            return ret;
        }

        /// <summary>
        /// Reads a bool value according to the SSH specification.
        /// </summary>
        /// <returns>a bool value.</returns>
        public bool ReadBool() {
            return ReadByte() != 0;
        }

        /// <summary>
        /// Reads a Int16 value in the network byte order.
        /// </summary>
        /// <returns>a Int16 value.</returns>
        public short ReadInt16() {
            return (short)ReadUInt16();
        }

        /// <summary>
        /// Reads a UInt16 value in the network byte order.
        /// </summary>
        /// <returns>a UInt16 value.</returns>
        public ushort ReadUInt16() {
            const int LENGTH = 2;
            CheckEnd(LENGTH);
            uint ret =
                  ((uint)_data[_currentIndex] << 8)
                | ((uint)_data[_currentIndex + 1]);
            _currentIndex += LENGTH;
            return (ushort)ret;
        }

        /// <summary>
        /// Reads a Int32 value in the network byte order.
        /// </summary>
        /// <returns>a Int32 value.</returns>
        public int ReadInt32() {
            return (int)ReadUInt32();
        }

        /// <summary>
        /// Reads a UInt32 value in the network byte order.
        /// </summary>
        /// <returns>a UInt32 value.</returns>
        public uint ReadUInt32() {
            const int LENGTH = 4;
            CheckEnd(LENGTH);
            uint ret =
                  ((uint)_data[_currentIndex] << 24)
                | ((uint)_data[_currentIndex + 1] << 16)
                | ((uint)_data[_currentIndex + 2] << 8)
                | ((uint)_data[_currentIndex + 3]);
            _currentIndex += LENGTH;
            return ret;
        }

        /// <summary>
        /// Reads a Int64 value in the network byte order.
        /// </summary>
        /// <returns>a Int64 value.</returns>
        public long ReadInt64() {
            return (long)ReadUInt64();
        }

        /// <summary>
        /// Reads a UInt64 value in the network byte order.
        /// </summary>
        /// <returns>a UInt64 value.</returns>
        public ulong ReadUInt64() {
            const int LENGTH = 8;
            CheckEnd(LENGTH);
            uint i1 =
                  ((uint)_data[_currentIndex] << 24)
                | ((uint)_data[_currentIndex + 1] << 16)
                | ((uint)_data[_currentIndex + 2] << 8)
                | ((uint)_data[_currentIndex + 3]);
            uint i2 =
                  ((uint)_data[_currentIndex + 4] << 24)
                | ((uint)_data[_currentIndex + 5] << 16)
                | ((uint)_data[_currentIndex + 6] << 8)
                | ((uint)_data[_currentIndex + 7]);
            _currentIndex += LENGTH;
            ulong ret = ((ulong)i1 << 32) | i2;
            return ret;
        }

        /// <summary>
        /// Reads bytes of the specified length.
        /// </summary>
        /// <param name="length">number of bytes.</param>
        /// <returns>byte array.</returns>
        public byte[] Read(int length) {
            byte[] image = new byte[length];
            Copy(length, image);
            return image;
        }

        /// <summary>
        /// Reads bytes of the specified length and copies them to the specified buffer.
        /// </summary>
        /// <param name="length">number of bytes.</param>
        /// <param name="buffrer">buffer</param>
        internal void Copy(int length, byte[] buffrer) {
            CheckEnd(length);
            Buffer.BlockCopy(_data, _currentIndex, buffrer, 0, length);
            _currentIndex += length;
        }

        /// <summary>
        /// Reads binary string according to the SSH specification.
        /// </summary>
        /// <returns>byte array.</returns>
        public byte[] ReadByteString() {
            int length = ReadInt32();
            if (length < 0) {
                throw new IOException("Invalid byte string length.");
            }
            return Read(length);
        }

        /// <summary>
        /// Reads binary string according to the SSH specification and copies it to the specified buffer.
        /// </summary>
        /// <param name="buffer">a buffer that the binary string is copied to</param>
        /// <returns>length of bytes copied</returns>
        internal int ReadByteStringInto(byte[] buffer) {
            int length = ReadInt32();
            if (length < 0) {
                throw new IOException("Invalid byte string length.");
            }
            if (buffer.Length < length) {
                throw new IOException("Insufficient buffer size.");
            }
            Copy(length, buffer);
            return length;
        }

        /// <summary>
        /// Reads ASCII string according to the SSH specification.
        /// </summary>
        /// <returns>string value.</returns>
        public string ReadString() {
            return ReadString(Encoding.ASCII);
        }

        /// <summary>
        /// Reads UTF-8 string according to the SSH specification.
        /// </summary>
        /// <returns>string value.</returns>
        public string ReadUTF8String() {
            return ReadString(Encoding.UTF8);
        }

        /// <summary>
        /// Reads text string according to the SSH specification.
        /// </summary>
        /// <param name="encoding">text encoding to use for decoding.</param>
        /// <returns>string value.</returns>
        private string ReadString(Encoding encoding) {
            int length = ReadInt32();
            if (length < 0) {
                throw new IOException("Invalid string length.");
            }
            CheckEnd(length);
            string s = encoding.GetString(_data, _currentIndex, length);
            _currentIndex += length;
            return s;
        }

        /// <summary>
        /// Reads a multiple precision integer according to the SSH specification.
        /// </summary>
        /// <returns>a multiple precision integer</returns>
        public abstract BigInteger ReadMPInt();
    }

    internal abstract class SSHDataWriter {
        protected SimpleMemoryStream _strm;

        public SSHDataWriter() {
            _strm = new SimpleMemoryStream();
        }

        public byte[] ToByteArray() {
            return _strm.ToNewArray();
        }

        public int Length {
            get {
                return _strm.Length;
            }
        }
        public void Reset() {
            _strm.Reset();
        }
        public void SetOffset(int value) {
            _strm.SetOffset(value);
        }
        public byte[] UnderlyingBuffer {
            get {
                return _strm.UnderlyingBuffer;
            }
        }

        public void Write(byte[] data) {
            _strm.Write(data, 0, data.Length);
        }
        public void Write(byte[] data, int offset, int count) {
            _strm.Write(data, offset, count);
        }
        public void WriteByte(byte data) {
            _strm.WriteByte(data);
        }
        public void WriteBool(bool data) {
            _strm.WriteByte(data ? (byte)1 : (byte)0);
        }

        public void WriteInt32(int data) {
            WriteUInt32((uint)data);
        }

        public void WriteUInt32(uint data) {
            _strm.WriteByte((byte)(data >> 24));
            _strm.WriteByte((byte)(data >> 16));
            _strm.WriteByte((byte)(data >> 8));
            _strm.WriteByte((byte)data);
        }

        public void WriteInt64(long data) {
            WriteUInt64((ulong)data);
        }

        public void WriteUInt64(ulong data) {
            WriteUInt32((uint)(data >> 32));
            WriteUInt32((uint)data);
        }

        public abstract void WriteBigInteger(BigInteger data);

        public void WriteString(string data) {
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            WriteInt32(bytes.Length);
            if (bytes.Length > 0)
                Write(bytes);
        }

        public void WriteAsString(byte[] data) {
            WriteInt32(data.Length);
            if (data.Length > 0)
                Write(data);
        }
        public void WriteAsString(byte[] data, int offset, int length) {
            WriteInt32(length);
            if (length > 0)
                Write(data, offset, length);
        }
    }
}

namespace Granados.IO.SSH1 {

    /// <summary>
    /// Utility class for reading fields in the SSH1 packet.
    /// </summary>
    internal class SSH1DataReader : SSHDataReader {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="image">a byte array to be read.</param>
        public SSH1DataReader(byte[] image)
            : base(image) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">a data fragment to be read.</param>
        public SSH1DataReader(DataFragment data)
            : base(data) {
        }

        /// <summary>
        /// Reads a multiple precision integer according to the SSH1 specification.
        /// </summary>
        /// <returns>a multiple precision integer</returns>
        public override BigInteger ReadMPInt() {
            int bits = ReadInt16();
            int bytes = (bits + 7) / 8;
            byte[] biData = Read(bytes);
            return new BigInteger(biData);
        }
    }

}

namespace Granados.IO.SSH2 {

    /// <summary>
    /// Utility class for reading fields in the SSH2 packet.
    /// </summary>
    internal class SSH2DataReader : SSHDataReader {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="image">a byte array to be read.</param>
        public SSH2DataReader(byte[] image)
            : base(image) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">a data fragment to be read.</param>
        public SSH2DataReader(DataFragment data)
            : base(data) {
        }

        /// <summary>
        /// Reads a multiple precision integer according to the SSH2 specification.
        /// </summary>
        /// <returns>a multiple precision integer</returns>
        public override BigInteger ReadMPInt() {
            return new BigInteger(ReadByteString());
        }

    }

    internal class SSH2DataWriter : SSHDataWriter {
        //writes mpint in SSH2 format
        public override void WriteBigInteger(BigInteger data) {
            byte[] t = data.GetBytes();
            int len = t.Length;
            if (t[0] >= 0x80) {
                WriteInt32(++len);
                WriteByte((byte)0);
            }
            else
                WriteInt32(len);
            Write(t);
        }

        public void WriteBigIntWithBits(BigInteger bi) {
            WriteInt32(bi.BitCount());
            Write(bi.GetBytes());
        }

    }
}
