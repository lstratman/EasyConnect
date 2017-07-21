// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.Crypto;
using Granados.IO;
using Granados.Mono.Math;
using Granados.Util;
using System;
using System.Threading;

namespace Granados.SSH2 {

    /// <summary>
    /// SSH2 Packet type (message number)
    /// </summary>
    public enum SSH2PacketType {
        SSH_MSG_DISCONNECT = 1,
        SSH_MSG_IGNORE = 2,
        SSH_MSG_UNIMPLEMENTED = 3,
        SSH_MSG_DEBUG = 4,
        SSH_MSG_SERVICE_REQUEST = 5,
        SSH_MSG_SERVICE_ACCEPT = 6,

        SSH_MSG_KEXINIT = 20,
        SSH_MSG_NEWKEYS = 21,

        SSH_MSG_KEXDH_INIT = 30,
        SSH_MSG_KEXDH_REPLY = 31,

        // these message types are not defined for preventing confusion or mistake.
        // SSH_MSG_KEX_ECDH_INIT = 30,
        // SSH_MSG_KEX_ECDH_REPLY = 31,
        // SSH_MSG_ECMQV_INIT = 30,
        // SSH_MSG_ECMQV_REPLY = 31,

        SSH_MSG_USERAUTH_REQUEST = 50,
        SSH_MSG_USERAUTH_FAILURE = 51,
        SSH_MSG_USERAUTH_SUCCESS = 52,
        SSH_MSG_USERAUTH_BANNER = 53,

        SSH_MSG_USERAUTH_INFO_REQUEST = 60,
        SSH_MSG_USERAUTH_INFO_RESPONSE = 61,

        SSH_MSG_GLOBAL_REQUEST = 80,
        SSH_MSG_REQUEST_SUCCESS = 81,
        SSH_MSG_REQUEST_FAILURE = 82,

        SSH_MSG_CHANNEL_OPEN = 90,
        SSH_MSG_CHANNEL_OPEN_CONFIRMATION = 91,
        SSH_MSG_CHANNEL_OPEN_FAILURE = 92,
        SSH_MSG_CHANNEL_WINDOW_ADJUST = 93,
        SSH_MSG_CHANNEL_DATA = 94,
        SSH_MSG_CHANNEL_EXTENDED_DATA = 95,
        SSH_MSG_CHANNEL_EOF = 96,
        SSH_MSG_CHANNEL_CLOSE = 97,
        SSH_MSG_CHANNEL_REQUEST = 98,
        SSH_MSG_CHANNEL_SUCCESS = 99,
        SSH_MSG_CHANNEL_FAILURE = 100
    }

    /// <summary>
    /// <see cref="IPacketBuilder"/> specialized for SSH2.
    /// </summary>
    internal interface ISSH2PacketBuilder : IPacketBuilder {
    }

    /// <summary>
    /// SSH2 packet builder.
    /// </summary>
    /// <remarks>
    /// The instances of this class share single thread-local buffer.
    /// You should be careful that only single instance is used while constructing a packet.
    /// </remarks>
    internal class SSH2Packet : ISSH2PacketBuilder {
        private readonly ByteBuffer _payload;

        private static readonly ThreadLocal<ByteBuffer> _payloadBuffer =
            new ThreadLocal<ByteBuffer>(() => new ByteBuffer(0x1000, -1));

        private static readonly ThreadLocal<bool> _lockFlag = new ThreadLocal<bool>();

        protected const int SEQUENCE_NUMBER_FIELD_LEN = 4;
        protected const int PACKET_LENGTH_FIELD_LEN = 4;
        protected const int PADDING_LENGTH_FIELD_LEN = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">packet type (message number)</param>
        public SSH2Packet(SSH2PacketType type) {
            if (_lockFlag.Value) {
                throw new InvalidOperationException(
                    "simultaneous editing packet detected: " + typeof(SSH2Packet).FullName);
            }
            _lockFlag.Value = true;
            _payload = _payloadBuffer.Value;
            _payload.Clear();
            _payload.WriteUInt32(0);    // sequence_number field for computing MAC
            _payload.WriteUInt32(0);    // packet_length field
            _payload.WriteByte(0);      // padding_length field
            _payload.WriteByte((byte)type);
        }

        /// <summary>
        /// Implements <see cref="IPacketBuilder"/>
        /// </summary>
        /// <remarks>
        /// This buffer also contains additional fields preceding the payload field due to the performance reason.
        /// Please use <see cref="GetPayloadBytes()"/> to get the bytes of the playload field.
        /// </remarks>
        public ByteBuffer Payload {
            get {
                return _payload;
            }
        }

        /// <summary>
        /// Get actual bytes of the payload field.
        /// </summary>
        /// <returns>bytes of the payload field</returns>
        public byte[] GetPayloadBytes() {
            const int IGNORE_LEN = SEQUENCE_NUMBER_FIELD_LEN + PACKET_LENGTH_FIELD_LEN + PADDING_LENGTH_FIELD_LEN;
            int payloadLen = _payload.Length - IGNORE_LEN;
            byte[] payloadBytes = new byte[payloadLen];
            Buffer.BlockCopy(
                _payload.RawBuffer,
                _payload.RawBufferOffset + IGNORE_LEN,
                payloadBytes,
                0,
                payloadLen);
            return payloadBytes;
        }

        /// <summary>
        /// Get actual size of the payload field.
        /// </summary>
        /// <returns>number of bytes of the payload field</returns>
        public int GetPayloadSize() {
            const int IGNORE_LEN = SEQUENCE_NUMBER_FIELD_LEN + PACKET_LENGTH_FIELD_LEN + PADDING_LENGTH_FIELD_LEN;
            int payloadLen = _payload.Length - IGNORE_LEN;
            return payloadLen;
        }

        /// <summary>
        /// Get packet type (message number).
        /// </summary>
        /// <returns>a packet type (message number)</returns>
        public SSH2PacketType GetPacketType() {
            const int MESSAGE_NUMBER_OFFSET = SEQUENCE_NUMBER_FIELD_LEN + PACKET_LENGTH_FIELD_LEN + PADDING_LENGTH_FIELD_LEN;
            return (SSH2PacketType)_payload.RawBuffer[_payload.RawBufferOffset + MESSAGE_NUMBER_OFFSET];
        }

        /// <summary>
        /// Allow this packet to be reused.
        /// </summary>
        public void Recycle() {
            _lockFlag.Value = false;
        }

        /// <summary>
        /// Gets the binary image of this packet.
        /// </summary>
        /// <param name="cipher">cipher algorithm, or null if no encryption.</param>
        /// <param name="mac">MAC algorithm, or null if no MAC.</param>
        /// <param name="sequenceNumber">sequence number</param>
        /// <returns>data</returns>
        public DataFragment GetImage(Cipher cipher, MAC mac, uint sequenceNumber) {
            BeforeBuildImage();
            ByteBuffer image = BuildImage(cipher, mac, sequenceNumber);
            Recycle();
            return image.AsDataFragment();
        }

        /// <summary>
        /// Modifies payload before making a packet image.
        /// Derived class can override this method for preparing sub-protocol packet.
        /// </summary>
        protected virtual void BeforeBuildImage() {
        }

        /// <summary>
        /// Build packet binary data
        /// </summary>
        /// <param name="cipher">cipher algorithm, or null if no encryption.</param>
        /// <param name="mac">MAC algorithm, or null if no MAC.</param>
        /// <param name="sequenceNumber">sequence number</param>
        /// <returns>a byte buffer</returns>
        private ByteBuffer BuildImage(Cipher cipher, MAC mac, uint sequenceNumber) {
            int blockSize = (cipher != null) ? cipher.BlockSize : 8;
            int payloadLength = _payload.Length - (SEQUENCE_NUMBER_FIELD_LEN + PACKET_LENGTH_FIELD_LEN + PADDING_LENGTH_FIELD_LEN);
            int paddingLength = blockSize - (PACKET_LENGTH_FIELD_LEN + PADDING_LENGTH_FIELD_LEN + payloadLength) % blockSize;
            if (paddingLength < 4) {
                paddingLength += blockSize;
            }
            int packetLength = PADDING_LENGTH_FIELD_LEN + payloadLength + paddingLength;

            int offset = 0;
            _payload.OverwriteUInt32(offset, sequenceNumber);
            offset += SEQUENCE_NUMBER_FIELD_LEN;
            _payload.OverwriteUInt32(offset, (uint)packetLength);
            offset += PACKET_LENGTH_FIELD_LEN;
            _payload.OverwriteByte(offset, (byte)paddingLength);

            // padding
            _payload.WriteSecureRandomBytes(paddingLength);

            // compute MAC
            byte[] macCode;
            if (mac != null) {
                macCode = mac.ComputeHash(_payload.RawBuffer, _payload.RawBufferOffset, _payload.Length);
            }
            else {
                macCode = null;
            }

            // remove sequence_number field
            _payload.RemoveHead(SEQUENCE_NUMBER_FIELD_LEN);

            // encrypt
            if (cipher != null) {
                cipher.Encrypt(
                    _payload.RawBuffer, _payload.RawBufferOffset, _payload.Length,
                    _payload.RawBuffer, _payload.RawBufferOffset);
            }

            // append MAC
            if (macCode != null) {
                _payload.Append(macCode);
            }

            return _payload;
        }
    }

    /// <summary>
    /// SSH2 packet payload builder.
    /// </summary>
    /// <remarks>
    /// This class is used for constructing temporary byte image according to the SSH2 format.
    /// </remarks>
    internal class SSH2PayloadImageBuilder : ISSH2PacketBuilder {

        private readonly ByteBuffer _payload;

        /// <summary>
        /// Constructor
        /// </summary>
        public SSH2PayloadImageBuilder()
            : this(0x1000) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialCapacity">initial capacity of the payload buffer.</param>
        public SSH2PayloadImageBuilder(int initialCapacity) {
            _payload = new ByteBuffer(initialCapacity, -1);
        }

        /// <summary>
        /// Implements <see cref="IPacketBuilder"/>
        /// </summary>
        public ByteBuffer Payload {
            get {
                return _payload;
            }
        }

        /// <summary>
        /// Current length of the payload.
        /// </summary>
        public int Length {
            get {
                return _payload.Length;
            }
        }

        /// <summary>
        /// Clear the <see cref="Payload"/>.
        /// </summary>
        public void Clear() {
            _payload.Clear();
        }

        /// <summary>
        /// Get content of the <see cref="Payload"/>.
        /// </summary>
        /// <returns>byte array</returns>
        public byte[] GetBytes() {
            return _payload.GetBytes();
        }

        /// <summary>
        /// Wrap this buffer.
        /// </summary>
        /// <returns>new <see cref="DataFragment"/> instance that wraps payload buffer.</returns>
        public DataFragment AsDataFragment() {
            return _payload.AsDataFragment();
        }
    }

    /// <summary>
    /// Extension methods for <see cref="ISSH2PacketBuilder"/>.
    /// </summary>
    /// <seealso cref="PacketBuilderMixin"/>
    internal static class SSH2PacketBuilderMixin {

        /// <summary>
        /// Writes BigInteger according to the SSH 2.0 specification
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="data"></param>
        public static T WriteBigInteger<T>(this T packet, BigInteger data) where T : ISSH2PacketBuilder {
            byte[] bi = data.GetBytes();
            int biLen = bi.Length;
            if ((bi[0] & 0x80) != 0) {
                packet.Payload.WriteInt32(biLen + 1);
                packet.Payload.WriteByte(0);
                packet.Payload.Append(bi);
            }
            else {
                packet.Payload.WriteInt32(biLen);
                packet.Payload.Append(bi);
            }
            return packet;
        }

    }

    /// <summary>
    /// <see cref="IDataHandler"/> that extracts SSH packet from the data stream
    /// and passes it to another <see cref="IDataHandler"/>.
    /// </summary>
    internal class SSH2Packetizer : IDataHandler {
        // RFC4253: The minimum size of a packet is 16 (or the cipher block size, whichever is larger) bytes.
        private const int MIN_PACKET_LENGTH = 12;    // exclude packet_length field (4 bytes)
        private const int MAX_PACKET_LENGTH = 0x80000; //there was the case that 64KB is insufficient

        private readonly IDataHandler _nextHandler;

        private readonly ByteBuffer _inputBuffer = new ByteBuffer(MAX_PACKET_LENGTH, MAX_PACKET_LENGTH * 16);
        private readonly ByteBuffer _packetImage = new ByteBuffer(36000, MAX_PACKET_LENGTH * 2);
        private int _packetLength;
        private uint _sequence;
        private Cipher _cipher;
        private readonly object _cipherSync = new object();
        private MAC _mac;
        private int _macLength;

        private bool _hasError = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handler">a handler that SSH packets are passed to</param>
        public SSH2Packetizer(IDataHandler handler) {
            _nextHandler = handler;
            _sequence = 0;
            _cipher = null;
            _mac = null;
            _macLength = 0;
            _packetLength = -1;
        }

        /// <summary>
        /// Set cipher settings.
        /// </summary>
        /// <param name="cipher">cipher algorithm, or null if not specified.</param>
        /// <param name="mac">MAC algorithm, or null if not specified.</param>
        public void SetCipher(Cipher cipher, MAC mac) {
            lock (_cipherSync) {
                _cipher = cipher;
                _mac = mac;
                _macLength = (_mac != null) ? _mac.Size : 0;
            }
        }

        /// <summary>
        /// Implements <see cref="IDataHandler"/>.
        /// </summary>
        /// <param name="data">fragment of the data stream</param>
        public void OnData(DataFragment data) {
            try {
                if (_hasError) {
                    return;
                }

                _inputBuffer.Append(data);

                ProcessBuffer();
            }
            catch (Exception ex) {
                OnError(ex);
            }
        }

        /// <summary>
        /// Implemens <see cref="IDataHandler"/>.
        /// </summary>
        public void OnClosed() {
            _nextHandler.OnClosed();
        }

        /// <summary>
        /// Implemens <see cref="IDataHandler"/>.
        /// </summary>
        public void OnError(Exception error) {
            _nextHandler.OnError(error);
        }

        /// <summary>
        /// Extracts SSH packet from the internal buffer and passes it to the next handler.
        /// </summary>
        private void ProcessBuffer() {
            while (true) {
                bool hasPacket;
                try {
                    hasPacket = ConstructPacket();
                }
                catch (Exception) {
                    _hasError = true;
                    throw;
                }

                if (!hasPacket) {
                    return;
                }

                DataFragment packet = _packetImage.ToDataFragment();    // duplicate bytes
                _nextHandler.OnData(packet);
            }
        }

        /// <summary>
        /// Extracts SSH packet from the internal buffer.
        /// </summary>
        /// <returns>
        /// true if one SSH packet has been extracted.
        /// in this case, _packetImage contains payload part of the SSH packet.
        /// </returns>
        private bool ConstructPacket() {
            const int SEQUENCE_NUMBER_FIELD_LEN = 4;
            const int PACKET_LENGTH_FIELD_LEN = 4;
            const int PADDING_LENGTH_FIELD_LEN = 1;

            lock (_cipherSync) {
                if (_packetLength < 0) {
                    int headLen = (_cipher != null) ? _cipher.BlockSize : 4;

                    if (_inputBuffer.Length < headLen) {
                        return false;
                    }

                    _packetImage.Clear();
                    _packetImage.WriteUInt32(_sequence);
                    _packetImage.Append(_inputBuffer, 0, headLen);
                    _inputBuffer.RemoveHead(headLen);

                    int headOffset = _packetImage.RawBufferOffset + SEQUENCE_NUMBER_FIELD_LEN;

                    if (_cipher != null) {
                        // decrypt first block
                        _cipher.Decrypt(
                            _packetImage.RawBuffer, headOffset, headLen,
                            _packetImage.RawBuffer, headOffset);
                    }

                    uint packetLength = SSHUtil.ReadUInt32(_packetImage.RawBuffer, headOffset);

                    if (packetLength < MIN_PACKET_LENGTH || packetLength >= MAX_PACKET_LENGTH) {
                        throw new SSHException(String.Format("invalid packet length : {0}", packetLength));
                    }

                    _packetLength = (int)packetLength;
                }

                int packetHeadLen = _packetImage.Length;    // size already read in
                int requiredLength = SEQUENCE_NUMBER_FIELD_LEN + PACKET_LENGTH_FIELD_LEN + _packetLength + _macLength - packetHeadLen;

                if (_inputBuffer.Length < requiredLength) {
                    return false;
                }

                _packetImage.Append(_inputBuffer, 0, requiredLength);
                _inputBuffer.RemoveHead(requiredLength);

                if (_cipher != null) {
                    // decrypt excluding MAC
                    int headOffset = _packetImage.RawBufferOffset + packetHeadLen;
                    _cipher.Decrypt(
                        _packetImage.RawBuffer, headOffset, requiredLength - _macLength,
                        _packetImage.RawBuffer, headOffset);
                }

                int paddingLength = _packetImage[SEQUENCE_NUMBER_FIELD_LEN + PACKET_LENGTH_FIELD_LEN];
                if (paddingLength < 4) {
                    throw new SSHException(String.Format("invalid padding length : {0}", paddingLength));
                }

                int payloadLength = _packetLength - PADDING_LENGTH_FIELD_LEN - paddingLength;

                if (_mac != null) {
                    int contentLen = SEQUENCE_NUMBER_FIELD_LEN + PACKET_LENGTH_FIELD_LEN + _packetLength;
                    byte[] result = _mac.ComputeHash(_packetImage.RawBuffer, _packetImage.RawBufferOffset, contentLen);

                    if (result.Length != _macLength ||
                        !SSHUtil.ByteArrayEqual(result, 0, _packetImage.RawBuffer, _packetImage.RawBufferOffset + contentLen, _macLength)) {
                        throw new SSHException("MAC mismatch");
                    }
                }

                // retain only payload
                _packetImage.RemoveHead(SEQUENCE_NUMBER_FIELD_LEN + PACKET_LENGTH_FIELD_LEN + PADDING_LENGTH_FIELD_LEN);
                _packetImage.RemoveTail(_macLength + paddingLength);

                // sanity check
                if (_packetImage.Length != payloadLength) {
                    throw new InvalidOperationException();
                }

                // prepare for the next packet
                ++_sequence;
                _packetLength = -1;

                return true;
            }
        }
    }
}
