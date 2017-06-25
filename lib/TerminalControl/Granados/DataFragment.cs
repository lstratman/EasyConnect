// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.Util;
using System;

namespace Granados.IO {
    /// <summary>
    /// DataFragment represents one or more tuples of (byte[], offset, length).
    /// To reduce memory usage, the source byte[] will not be copied.
    /// If this behavior is not convenient, call Isolate() method.
    /// </summary>
    /// <exclude/>
    public class DataFragment {

        private readonly byte[] _data;
        private int _offset;
        private int _length;

        /// <summary>
        /// Constructor for assembly-internal use.
        /// </summary>
        /// <param name="capacity"></param>
        internal DataFragment(int capacity)
            : this(new byte[capacity], 0, 0) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">a byte array that contains the data that this object represents.</param>
        /// <param name="offset">start index of the data that this object represents.</param>
        /// <param name="length">number of bytes of the data that this object represents.</param>
        public DataFragment(byte[] data, int offset, int length) {
            _data = data;
            _offset = offset;
            _length = length;
        }

        /// <summary>
        /// Number of bytes.
        /// </summary>
        public int Length {
            get {
                return _length;
            }
        }

        /// <summary>
        /// Start index in <see cref="Data"/>.
        /// </summary>
        public int Offset {
            get {
                return _offset;
            }
        }

        /// <summary>
        /// A byte array that contains the data that this object represents.
        /// Actual data are the bytes in the range of Data[Offset] .. Data[Offset + Length - 1].
        /// </summary>
        public byte[] Data {
            get {
                return _data;
            }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="index">index.</param>
        /// <returns>a byte value at the index.</returns>
        /// <exception cref="IndexOutOfRangeException">invalid index.</exception>
        public byte this[int index] {
            get {
                if (index < 0 || index >= _length) {
                    throw new IndexOutOfRangeException();
                }
                return _data[_offset + index];
            }
        }

        /// <summary>
        /// An alias of <see cref="Duplicate"/>.
        /// </summary>
        /// <returns>a new instance that has the duplicated bytes.</returns>
        public DataFragment Isolate() {
            return Duplicate();
        }

        /// <summary>
        /// Duplicate data.
        /// </summary>
        /// <returns>a new instance that has the duplicated bytes.</returns>
        public virtual DataFragment Duplicate() {
            byte[] data = GetBytes();
            return new DataFragment(data, 0, data.Length);
        }

        /// <summary>
        /// Set offset and length. (assembly-internal use only)
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        internal void SetLength(int offset, int length) {
            if (offset + length > _data.Length) {
                throw new ArgumentOutOfRangeException();
            }
            _offset = offset;
            _length = length;
        }

        /// <summary>
        /// Get a copy of the data.
        /// </summary>
        /// <returns>a new byte array.</returns>
        public byte[] GetBytes() {
            byte[] data = new byte[_length];
            Buffer.BlockCopy(_data, _offset, data, 0, _length);
            return data;
        }

        /// <summary>
        /// Make another view of this instance.
        /// </summary>
        /// <param name="startIndex">start index for the new view.</param>
        public DataFragment MakeView(int startIndex) {
            if (startIndex < 0 || startIndex > _length) {
                throw new ArgumentOutOfRangeException();
            }
            DataFragment newInstance = new DataFragment(_data, _offset + startIndex, _length - startIndex);
            return newInstance;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    internal class SimpleMemoryStream {
        private byte[] _buffer;
        private int _offset;

        public SimpleMemoryStream(int capacity) {
            Init(capacity);
        }
        public SimpleMemoryStream() {
            Init(512);
        }
        private void Init(int capacity) {
            _buffer = new byte[capacity];
            Reset();
        }

        public int Length {
            get {
                return _offset;
            }
        }
        public byte[] UnderlyingBuffer {
            get {
                return _buffer;
            }
        }
        public void Reset() {
            _offset = 0;
        }
        public void SetOffset(int value) {
            _offset = value;
        }
        public byte[] ToNewArray() {
            byte[] r = new byte[_offset];
            Buffer.BlockCopy(_buffer, 0, r, 0, _offset);
            return r;
        }

        private void AssureSize(int size) {
            if (_buffer.Length < size) {
                byte[] t = new byte[Math.Max(size, _buffer.Length * 2)];
                Buffer.BlockCopy(_buffer, 0, t, 0, _buffer.Length);
                _buffer = t;
            }
        }

        public void Write(byte[] data, int offset, int length) {
            AssureSize(_offset + length);
            Buffer.BlockCopy(data, offset, _buffer, _offset, length);
            _offset += length;
        }
        public void Write(byte[] data) {
            Write(data, 0, data.Length);
        }
        public void Write(DataFragment data) {
            Write(data.Data, data.Offset, data.Length);
        }
        public void WriteByte(byte b) {
            AssureSize(_offset + 1);
            _buffer[_offset++] = b;
        }
    }

    /// <summary>
    /// Byte buffer
    /// </summary>
    internal class ByteBuffer {

        private byte[] _buff;
        private int _offset;
        private int _length;
        private readonly int _maxCapacity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialCapacity">initial capacity in bytes.</param>
        /// <param name="maxCapacity">maximum capacity in bytes. negative value means unlimited.</param>
        public ByteBuffer(int initialCapacity, int maxCapacity) {
            if (maxCapacity >= 0 && maxCapacity < initialCapacity) {
                throw new ArgumentException("maximum capacity is smaller than initial capacity.");
            }

            _maxCapacity = maxCapacity;
            _buff = new byte[initialCapacity];
            _offset = _length = 0;
        }

        /// <summary>
        /// Number of bytes in this buffer.
        /// </summary>
        public int Length {
            get {
                return _length;
            }
        }

        /// <summary>
        /// Internal buffer.
        /// </summary>
        public byte[] RawBuffer {
            get {
                return _buff;
            }
        }

        /// <summary>
        /// Offset of the internal buffer.
        /// </summary>
        public int RawBufferOffset {
            get {
                return _offset;
            }
        }

        /// <summary>
        /// Byte accessor
        /// </summary>
        /// <param name="index">index</param>
        /// <returns>byte value</returns>
        /// <exception cref="IndexOutOfRangeException">invalid index was specified.</exception>
        public byte this[int index] {
            get {
                if (index < 0 || index >= _length) {
                    throw new IndexOutOfRangeException();
                }
                return _buff[_offset + index];
            }
        }

        /// <summary>
        /// Clear buffer.
        /// </summary>
        public void Clear() {
            _offset = _length = 0;
        }

        /// <summary>
        /// Append data.
        /// </summary>
        /// <param name="data">byte array</param>
        public void Append(byte[] data) {
            Append(data, 0, data.Length);
        }

        /// <summary>
        /// Append data.
        /// </summary>
        /// <param name="data">byte array</param>
        /// <param name="offset">start index of the byte array</param>
        /// <param name="length">byte count to copy</param>
        public void Append(byte[] data, int offset, int length) {
            MakeRoom(length);
            Buffer.BlockCopy(data, offset, _buff, _offset + _length, length);
            _length += length;
        }

        /// <summary>
        /// Append data which is read from another buffer.
        /// </summary>
        /// <param name="buffer">another buffer</param>
        public void Append(ByteBuffer buffer) {
            Append(buffer._buff, buffer._offset, buffer._length);
        }

        /// <summary>
        /// Append <see cref="DataFragment"/>.
        /// </summary>
        /// <param name="data">data</param>
        public void Append(DataFragment data) {
            Append(data.Data, data.Offset, data.Length);
        }

        /// <summary>
        /// Append data which is read from another buffer.
        /// </summary>
        /// <param name="buffer">another buffer</param>
        /// <param name="offset">start index of the data to copy</param>
        /// <param name="length">byte count to copy</param>
        public void Append(ByteBuffer buffer, int offset, int length) {
            buffer.CheckRange(offset, length);
            Append(buffer._buff, buffer._offset + offset, length);
        }

        /// <summary>
        /// Append data with writer function.
        /// </summary>
        /// <param name="length">number of bytes to write</param>
        /// <param name="writer">a function writes data to the specified buffer</param>
        public void Append(int length, Action<byte[], int> writer) {
            MakeRoom(length);
            writer(_buff, _offset + _length);
            _length += length;
        }

        /// <summary>
        /// Overwrite data with writer function.
        /// </summary>
        /// <param name="offset">index to start to write</param>
        /// <param name="length">number of bytes to write</param>
        /// <param name="writer">a function writes data to the specified buffer</param>
        public void Overwrite(int offset, int length, Action<byte[], int> writer) {
            CheckRange(offset, length);
            writer(_buff, _offset + offset);
        }

        /// <summary>
        /// Remove bytes from the head of the buffer.
        /// </summary>
        /// <param name="length">number of bytes to remove</param>
        public void RemoveHead(int length) {
            if (length >= _length) {
                _offset = _length = 0;
            }
            else {
                _offset += length;
                _length -= length;
            }
        }

        /// <summary>
        /// Remove bytes from the tail of the buffer.
        /// </summary>
        /// <param name="length">number of bytes to remove</param>
        public void RemoveTail(int length) {
            if (length >= _length) {
                _offset = _length = 0;
            }
            else {
                _length -= length;
            }
        }

        /// <summary>
        /// Make room in the internal buffer
        /// </summary>
        /// <param name="size">number of bytes needed</param>
        private void MakeRoom(int size) {
            if (_offset + _length + size <= _buff.Length) {
                return;
            }

            int requiredSize = _length + size;
            if (requiredSize <= _buff.Length) {
                Buffer.BlockCopy(_buff, _offset, _buff, 0, _length);
                _offset = 0;
                return;
            }

            if (_maxCapacity >= 0 && requiredSize > _maxCapacity) {
                throw new InvalidOperationException(
                    String.Format("buffer size reached limit ({0} bytes). required {1} bytes.", _maxCapacity, requiredSize));
            }

            int newCapacity = RoundUp(requiredSize);
            if (_maxCapacity >= 0 && newCapacity > _maxCapacity) {
                newCapacity = _maxCapacity;
            }

            byte[] newBuff = new byte[newCapacity];
            Buffer.BlockCopy(_buff, _offset, newBuff, 0, _length);
            _offset = 0;
            _buff = newBuff;
        }

        /// <summary>
        /// Round up to power of two.
        /// </summary>
        /// <param name="size">size</param>
        /// <returns>the value power of two.</returns>
        private static int RoundUp(int size) {
            if (size <= 16)
                return 16;
            size--;
            size |= size >> 1;
            size |= size >> 2;
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;
            return size + 1;
        }

        /// <summary>
        /// Checks if the specified range was valid on this buffer.
        /// </summary>
        /// <param name="index">index of the data</param>
        /// <param name="length">number of bytes</param>
        private void CheckRange(int index, int length) {
            if (index < 0 || index >= _length) {
                throw new ArgumentOutOfRangeException("invalid index");
            }
            if (index + length > _length) {
                throw new ArgumentOutOfRangeException("invalid length");
            }
        }

        /// <summary>
        /// Returns the contents of this buffer.
        /// </summary>
        /// <returns>new byte array</returns>
        public byte[] GetBytes() {
            byte[] data = new byte[_length];
            Buffer.BlockCopy(_buff, _offset, data, 0, _length);
            return data;
        }

        /// <summary>
        /// Wrap this buffer.
        /// </summary>
        /// <returns>new <see cref="DataFragment"/> instance that wraps this buffer.</returns>
        public DataFragment AsDataFragment() {
            return new DataFragment(_buff, _offset, _length);
        }

        /// <summary>
        /// Create a new <see cref="DataFragment"/>.
        /// </summary>
        /// <returns>new <see cref="DataFragment"/> instance that have data copied from this buffer.</returns>
        public DataFragment ToDataFragment() {
            byte[] data = GetBytes();
            return new DataFragment(data, 0, data.Length);
        }

        #region Writer methods

        /// <summary>
        /// Write Byte value.
        /// </summary>
        /// <param name="val">the value to be written</param>
        public void WriteByte(byte val) {
            const int LENGTH = 1;
            MakeRoom(LENGTH);
            int index = _offset + _length;

            _buff[index] = val;

            _length += LENGTH;
        }

        /// <summary>
        /// Overwrite Byte value.
        /// </summary>
        /// <param name="offset">offset index of the buffer</param>
        /// <param name="val">the value to be written</param>
        public void OverwriteByte(int offset, byte val) {
            const int LENGTH = 1;
            CheckRange(offset, LENGTH);
            int index = _offset + offset;

            _buff[index] = val;
        }

        /// <summary>
        /// Write UInt16 value in network byte order.
        /// </summary>
        /// <param name="val">the value to be written</param>
        public void WriteUInt16(ushort val) {
            const int LENGTH = 2;
            MakeRoom(LENGTH);
            int index = _offset + _length;

            _buff[index + 0] = (byte)(val >> 8);
            _buff[index + 1] = (byte)(val);

            _length += LENGTH;
        }

        /// <summary>
        /// Write Int16 value in network byte order.
        /// </summary>
        /// <param name="val">the value to be written</param>
        public void WriteInt16(short val) {
            WriteUInt16((ushort)val);
        }

        /// <summary>
        /// Write UInt32 value in network byte order.
        /// </summary>
        /// <param name="val">the value to be written</param>
        public void WriteUInt32(uint val) {
            const int LENGTH = 4;
            MakeRoom(LENGTH);
            int index = _offset + _length;

            _buff[index + 0] = (byte)(val >> 24);
            _buff[index + 1] = (byte)(val >> 16);
            _buff[index + 2] = (byte)(val >> 8);
            _buff[index + 3] = (byte)(val);

            _length += LENGTH;
        }

        /// <summary>
        /// Overwrite UInt32 value in network byte order.
        /// </summary>
        /// <param name="offset">offset index of the buffer</param>
        /// <param name="val">the value to be written</param>
        public void OverwriteUInt32(int offset, uint val) {
            const int LENGTH = 4;
            CheckRange(offset, LENGTH);
            int index = _offset + offset;

            _buff[index + 0] = (byte)(val >> 24);
            _buff[index + 1] = (byte)(val >> 16);
            _buff[index + 2] = (byte)(val >> 8);
            _buff[index + 3] = (byte)(val);
        }

        /// <summary>
        /// Write Int32 value in network byte order.
        /// </summary>
        /// <param name="val">the value to be written</param>
        public void WriteInt32(int val) {
            WriteUInt32((uint)val);
        }

        /// <summary>
        /// Write UInt64 value in network byte order.
        /// </summary>
        /// <param name="val">the value to be written</param>
        public void WriteUInt64(ulong val) {
            const int LENGTH = 8;
            MakeRoom(LENGTH);
            int index = _offset + _length;

            _buff[index + 0] = (byte)(val >> 56);
            _buff[index + 1] = (byte)(val >> 48);
            _buff[index + 2] = (byte)(val >> 40);
            _buff[index + 3] = (byte)(val >> 32);
            _buff[index + 4] = (byte)(val >> 24);
            _buff[index + 5] = (byte)(val >> 16);
            _buff[index + 6] = (byte)(val >> 8);
            _buff[index + 7] = (byte)(val);

            _length += LENGTH;
        }

        /// <summary>
        /// Write Int64 value in network byte order.
        /// </summary>
        /// <param name="val">the value to be written</param>
        public void WriteInt64(long val) {
            WriteUInt64((ulong)val);
        }

        /// <summary>
        /// Write random bytes.
        /// </summary>
        /// <param name="length">number of bytes</param>
        public void WriteSecureRandomBytes(int length) {
            MakeRoom(length);
            int index = _offset + _length;

            SecureRandomBuffer.GetRandomBytes(_buff, index, length);

            _length += length;
        }

        #endregion
    }
}
