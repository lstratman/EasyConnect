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

namespace Granados.SSH1 {

    /// <summary>
    /// SSH1 Packet type (message number)
    /// </summary>
    public enum SSH1PacketType {
        SSH_MSG_DISCONNECT = 1,
        SSH_SMSG_PUBLIC_KEY = 2,
        SSH_CMSG_SESSION_KEY = 3,
        SSH_CMSG_USER = 4,
        SSH_CMSG_AUTH_RSA = 6,
        SSH_SMSG_AUTH_RSA_CHALLENGE = 7,
        SSH_CMSG_AUTH_RSA_RESPONSE = 8,
        SSH_CMSG_AUTH_PASSWORD = 9,
        SSH_CMSG_REQUEST_PTY = 10,
        SSH_CMSG_WINDOW_SIZE = 11,
        SSH_CMSG_EXEC_SHELL = 12,
        SSH_CMSG_EXEC_CMD = 13,
        SSH_SMSG_SUCCESS = 14,
        SSH_SMSG_FAILURE = 15,
        SSH_CMSG_STDIN_DATA = 16,
        SSH_SMSG_STDOUT_DATA = 17,
        SSH_SMSG_STDERR_DATA = 18,
        SSH_CMSG_EOF = 19,
        SSH_SMSG_EXITSTATUS = 20,
        SSH_MSG_CHANNEL_OPEN_CONFIRMATION = 21,
        SSH_MSG_CHANNEL_OPEN_FAILURE = 22,
        SSH_MSG_CHANNEL_DATA = 23,
        SSH_MSG_CHANNEL_CLOSE = 24,
        SSH_MSG_CHANNEL_CLOSE_CONFIRMATION = 25,
        SSH_SMSG_X11_OPEN = 27,
        SSH_CMSG_PORT_FORWARD_REQUEST = 28,
        SSH_MSG_PORT_OPEN = 29,
        SSH_CMSG_AGENT_REQUEST_FORWARDING = 30,
        SSH_SMSG_AGENT_OPEN = 31,
        SSH_MSG_IGNORE = 32,
        SSH_CMSG_EXIT_CONFIRMATION = 33,
        SSH_CMSG_X11_REQUEST_FORWARDING = 34,
        SSH_MSG_DEBUG = 36
    }

    /// <summary>
    /// <see cref="IPacketBuilder"/> specialized for SSH1.
    /// </summary>
    internal interface ISSH1PacketBuilder : IPacketBuilder {
    }

    /// <summary>
    /// SSH1 packet builder.
    /// </summary>
    /// <remarks>
    /// The instances of this class share single thread-local buffer.
    /// You should be careful that only single instance is used while constructing a packet.
    /// </remarks>
    internal class SSH1Packet : ISSH1PacketBuilder {
        private readonly SSH1PacketType _type;
        private readonly ByteBuffer _payload;

        private static readonly ThreadLocal<ByteBuffer> _payloadBuffer =
            new ThreadLocal<ByteBuffer>(() => new ByteBuffer(0x1000, -1));

        private static readonly ThreadLocal<ByteBuffer> _imageBuffer =
            new ThreadLocal<ByteBuffer>(() => new ByteBuffer(0x1000, -1));

        private static readonly ThreadLocal<bool> _lockFlag = new ThreadLocal<bool>();

        private const int PACKET_LENGTH_FIELD_LEN = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">packet type (message number)</param>
        public SSH1Packet(SSH1PacketType type) {
            if (_lockFlag.Value) {
                throw new InvalidOperationException(
                    "simultaneous editing packet detected: " + typeof(SSH1Packet).FullName);
            }
            _lockFlag.Value = true;
            _type = type;
            _payload = _payloadBuffer.Value;
            _payload.Clear();
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
        /// Get packet type (message number).
        /// </summary>
        /// <returns>a packet type (message number)</returns>
        public SSH1PacketType GetPacketType() {
            return _type;
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
        /// <returns>data</returns>
        public DataFragment GetImage(Cipher cipher = null) {
            ByteBuffer image = BuildImage();
            if (cipher != null) {
                cipher.Encrypt(
                    image.RawBuffer, image.RawBufferOffset + PACKET_LENGTH_FIELD_LEN, image.Length - PACKET_LENGTH_FIELD_LEN,
                    image.RawBuffer, image.RawBufferOffset + PACKET_LENGTH_FIELD_LEN);
            }
            Recycle();
            return image.AsDataFragment();
        }

        /// <summary>
        /// Build packet binary data
        /// </summary>
        /// <returns>a byte buffer</returns>
        private ByteBuffer BuildImage() {
            ByteBuffer image = _imageBuffer.Value;
            image.Clear();
            int packetLength = _payload.Length + 5; //type and CRC
            int paddingLength = 8 - (packetLength % 8);
            image.WriteInt32(packetLength);
            image.WriteSecureRandomBytes(paddingLength);
            image.WriteByte((byte)_type);
            if (_payload.Length > 0) {
                image.Append(_payload);
            }
            uint crc = CRC.Calc(
                        image.RawBuffer,
                        image.RawBufferOffset + PACKET_LENGTH_FIELD_LEN,
                        image.Length - PACKET_LENGTH_FIELD_LEN);
            image.WriteUInt32(crc);
            return image;
        }
    }

    /// <summary>
    /// SSH1 packet payload builder.
    /// </summary>
    /// <remarks>
    /// This class is used for constructing temporary byte image according to the SSH1 format.
    /// </remarks>
    internal class SSH1PayloadImageBuilder : ISSH1PacketBuilder {

        private readonly ByteBuffer _payload;

        /// <summary>
        /// Constructor
        /// </summary>
        public SSH1PayloadImageBuilder()
            : this(0x1000) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="initialCapacity">initial capacity of the payload buffer.</param>
        public SSH1PayloadImageBuilder(int initialCapacity) {
            _payload = new ByteBuffer(0x1000, -1);
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
    }

    /// <summary>
    /// Extension methods for <see cref="ISSH1PacketBuilder"/>.
    /// </summary>
    /// <seealso cref="PacketBuilderMixin"/>
    internal static class SSH1PacketBuilderMixin {

        /// <summary>
        /// Writes BigInteger according to the SSH 1.5 specification
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="data"></param>
        public static T WriteBigInteger<T>(this T packet, BigInteger data) where T : ISSH1PacketBuilder {
            byte[] image = data.GetBytes();
            int bits = image.Length * 8;
            packet.Payload.WriteUInt16((ushort)bits);
            packet.Payload.Append(image);
            return packet;
        }

    }

    /// <summary>
    /// <see cref="IDataHandler"/> that extracts SSH packet from the data stream
    /// and passes it to another <see cref="IDataHandler"/>.
    /// </summary>
    internal class SSH1Packetizer : FilterDataHandler {
        private const int MIN_PACKET_LENGTH = 5;
        private const int MAX_PACKET_LENGTH = 262144;
        private const int MAX_PACKET_DATA_SIZE = MAX_PACKET_LENGTH + (8 - (MAX_PACKET_LENGTH % 8)) + 4;

        private readonly ByteBuffer _inputBuffer = new ByteBuffer(MAX_PACKET_DATA_SIZE, MAX_PACKET_DATA_SIZE * 16);
        private readonly ByteBuffer _packetImage = new ByteBuffer(36000, MAX_PACKET_DATA_SIZE * 2);
        private Cipher _cipher;
        private readonly object _cipherSync = new object();
        private bool _checkMAC;
        private int _packetLength;

        private bool _hasError = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handler">a handler that SSH packets are passed to</param>
        public SSH1Packetizer(IDataHandler handler)
            : base(handler) {
            _cipher = null;
            _checkMAC = false;
            _packetLength = -1;
        }

        /// <summary>
        /// Set cipher settings.
        /// </summary>
        /// <param name="cipher">cipher algorithm, or null if not specified.</param>
        /// <param name="checkMac">specifies whether CRC check is performed.</param>
        public void SetCipher(Cipher cipher, bool checkMac) {
            lock (_cipherSync) {
                _cipher = cipher;
                _checkMAC = checkMac;
            }
        }

        /// <summary>
        /// Implements <see cref="FilterDataHandler"/>.
        /// </summary>
        /// <param name="data">fragment of the data stream</param>
        protected override void FilterData(DataFragment data) {
            lock (_cipherSync) {
                try {
                    if (_hasError) {
                        return;
                    }

                    _inputBuffer.Append(data.Data, data.Offset, data.Length);

                    ProcessBuffer();
                }
                catch (Exception ex) {
                    OnError(ex);
                }
            }
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
                OnDataInternal(packet);
            }
        }

        /// <summary>
        /// Extracts SSH packet from the internal buffer.
        /// </summary>
        /// <returns>
        /// true if one SSH packet has been extracted.
        /// in this case, _packetImage contains Packet Type field and Data field of the SSH packet.
        /// </returns>
        private bool ConstructPacket() {
            const int PACKET_LENGTH_FIELD_LEN = 4;
            const int CHECK_BYTES_FIELD_LEN = 4;

            if (_packetLength < 0) {
                if (_inputBuffer.Length < PACKET_LENGTH_FIELD_LEN) {
                    return false;
                }

                uint packetLength = SSHUtil.ReadUInt32(_inputBuffer.RawBuffer, _inputBuffer.RawBufferOffset);
                _inputBuffer.RemoveHead(PACKET_LENGTH_FIELD_LEN);

                if (packetLength < MIN_PACKET_LENGTH || packetLength > MAX_PACKET_LENGTH) {
                    throw new SSHException(String.Format("invalid packet length : {0}", packetLength));
                }

                _packetLength = (int)packetLength;
            }

            int paddingLength = 8 - (_packetLength % 8);
            int requiredLength = paddingLength + _packetLength;

            if (_inputBuffer.Length < requiredLength) {
                return false;
            }

            _packetImage.Clear();
            _packetImage.Append(_inputBuffer, 0, requiredLength);   // Padding, Packet Type, Data, and Check fields
            _inputBuffer.RemoveHead(requiredLength);

            if (_cipher != null) {
                _cipher.Decrypt(
                    _packetImage.RawBuffer, _packetImage.RawBufferOffset, requiredLength,
                    _packetImage.RawBuffer, _packetImage.RawBufferOffset);
            }

            if (_checkMAC) {
                uint crc = CRC.Calc(
                            _packetImage.RawBuffer,
                            _packetImage.RawBufferOffset,
                            requiredLength - CHECK_BYTES_FIELD_LEN);
                uint expected = SSHUtil.ReadUInt32(
                            _packetImage.RawBuffer,
                            _packetImage.RawBufferOffset + requiredLength - CHECK_BYTES_FIELD_LEN);
                if (crc != expected) {
                    throw new SSHException("CRC Error");
                }
            }

            // retain only Packet Type and Data fields
            _packetImage.RemoveHead(paddingLength);
            _packetImage.RemoveTail(CHECK_BYTES_FIELD_LEN);

            // sanity check
            if (_packetImage.Length != _packetLength - CHECK_BYTES_FIELD_LEN) {
                throw new InvalidOperationException();
            }

            // prepare for the next packet
            _packetLength = -1;

            return true;
        }
    }
}
