// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.IO;
using Granados.IO.SSH1;
using Granados.Mono.Math;
using Granados.PKI;
using Granados.SSH1;
using Granados.SSH2;
using Granados.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

namespace Granados.AgentForwarding {

    /// <summary>
    /// An interface for handling authentication agent forwarding.
    /// </summary>
    public interface IAgentForwardingAuthKeyProvider {
        /// <summary>
        /// A property that indicates whether this provider is active.
        /// </summary>
        /// <remarks>
        /// This property will be checked on each time a request message from the server has been received.
        /// </remarks>
        bool IsAuthKeyProviderEnabled {
            get;
        }

        /// <summary>
        /// Returns SSH1 authentication keys that are available for the authentication.
        /// </summary>
        /// <returns>SSH1 authentication keys</returns>
        SSH1UserAuthKey[] GetAvailableSSH1UserAuthKeys();

        /// <summary>
        /// Returns SSH2 authentication keys that are available for the authentication.
        /// </summary>
        /// <returns>SSH2 authentication keys</returns>
        SSH2UserAuthKey[] GetAvailableSSH2UserAuthKeys();
    }

    /// <summary>
    /// Agent forwarding message types (OpenSSH's protocol)
    /// </summary>
    internal enum OpenSSHAgentForwardingMessageType {
        //--- from client
        // for SSH1 keys
        SSH_AGENTC_REQUEST_RSA_IDENTITIES = 1,
        SSH_AGENTC_RSA_CHALLENGE = 3,
        // for SSH2 keys
        SSH2_AGENTC_REQUEST_IDENTITIES = 11,
        SSH2_AGENTC_SIGN_REQUEST = 13,

        //--- from agent
        // for SSH1 keys
        SSH_AGENT_RSA_IDENTITIES_ANSWER = 2,
        SSH_AGENT_RSA_RESPONSE = 4,
        // for SSH2 keys
        SSH2_AGENT_IDENTITIES_ANSWER = 12,
        SSH2_AGENT_SIGN_RESPONSE = 14,
        // generic
        SSH_AGENT_FAILURE = 5,
        SSH_AGENT_SUCCESS = 6,
    }

    /// <summary>
    /// Message builder for the agent forwarding response. (OpenSSH's protocol)
    /// </summary>
    internal class OpenSSHAgentForwardingMessage : ISSH1PacketBuilder, ISSH2PacketBuilder {

        private readonly ByteBuffer _payload = new ByteBuffer(16 * 1024, -1);

        #region IPacketBuilder

        /// <summary>
        /// 
        /// </summary>
        public ByteBuffer Payload {
            get {
                return _payload;
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageType">message type</param>
        public OpenSSHAgentForwardingMessage(OpenSSHAgentForwardingMessageType messageType) {
            _payload.WriteUInt32(0);    // message length (set later)
            _payload.WriteByte((byte)messageType);
        }

        /// <summary>
        /// Gets entire binary image of the message.
        /// </summary>
        /// <returns>binary image of the message</returns>
        public DataFragment GetImage() {
            int messageLength = _payload.Length - 4;
            _payload.OverwriteUInt32(0, (uint)messageLength);
            return _payload.AsDataFragment();
        }
    }

    /// <summary>
    /// Agent forwarding message handler (OpenSSH's protocol)
    /// </summary>
    internal class OpenSSHAgentForwardingMessageHandler : SimpleSSHChannelEventHandler {

        private const uint SSH_AGENT_OLD_SIGNATURE = 1;

        private readonly ISSHChannel _channel;
        private readonly IAgentForwardingAuthKeyProvider _authKeyProvider;

        private readonly ByteBuffer _buffer = new ByteBuffer(16 * 1024, 64 * 1024);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="channel">channel object</param>
        /// <param name="authKeyProvider">authentication key provider</param>
        public OpenSSHAgentForwardingMessageHandler(ISSHChannel channel, IAgentForwardingAuthKeyProvider authKeyProvider) {
            _channel = channel;
            _authKeyProvider = authKeyProvider;
        }

        /// <summary>
        /// Handles channel data
        /// </summary>
        /// <param name="data">channel data</param>
        public override void OnData(DataFragment data) {
            _buffer.Append(data);

            if (_buffer.Length >= 4) {
                uint messageLength = SSHUtil.ReadUInt32(_buffer.RawBuffer, _buffer.RawBufferOffset);
                if (_buffer.Length >= 4 + messageLength) {
                    DataFragment message = new DataFragment(_buffer.RawBuffer, _buffer.RawBufferOffset + 4, (int)messageLength);
                    try {
                        ProcessMessage(message);
                    }
                    catch (Exception e) {
                        Debug.WriteLine(e.Message);
                        Debug.WriteLine(e.StackTrace);
                    }
                    _buffer.RemoveHead(4 + (int)messageLength);
                }
            }
        }

        /// <summary>
        /// Process forwarded message.
        /// </summary>
        /// <param name="message">a forwarded message</param>
        private void ProcessMessage(DataFragment message) {
            if (_authKeyProvider == null || !_authKeyProvider.IsAuthKeyProviderEnabled) {
                SendFailure();
                return;
            }

            SSH1DataReader reader = new SSH1DataReader(message);
            OpenSSHAgentForwardingMessageType messageType = (OpenSSHAgentForwardingMessageType)reader.ReadByte();
            switch (messageType) {
                // for SSH1 keys
                case OpenSSHAgentForwardingMessageType.SSH_AGENTC_REQUEST_RSA_IDENTITIES:
                    SSH1Identities();
                    break;
                case OpenSSHAgentForwardingMessageType.SSH_AGENTC_RSA_CHALLENGE: {
                        reader.ReadUInt32();    // ignored
                        BigInteger e = reader.ReadMPInt();
                        BigInteger n = reader.ReadMPInt();
                        BigInteger encryptedChallenge = reader.ReadMPInt();
                        byte[] sessionId = reader.Read(16);
                        uint responseType = reader.ReadUInt32();

                        SSH1IRSAChallenge(e, n, encryptedChallenge, sessionId, responseType);
                    }
                    break;
                // for SSH2 keys
                case OpenSSHAgentForwardingMessageType.SSH2_AGENTC_REQUEST_IDENTITIES:
                    SSH2Identities();
                    break;
                case OpenSSHAgentForwardingMessageType.SSH2_AGENTC_SIGN_REQUEST: {
                        byte[] blob = reader.ReadByteString();
                        byte[] data = reader.ReadByteString();
                        uint flags = reader.ReadUInt32();

                        SSH2Sign(blob, data, flags);
                    }
                    break;
                default:
                    SendFailure();
                    break;
            }
        }

        /// <summary>
        /// List SSH1 RSA keys
        /// </summary>
        private void SSH1Identities() {
            var authKeys = _authKeyProvider.GetAvailableSSH1UserAuthKeys() ?? new SSH1UserAuthKey[0];
            var message = new OpenSSHAgentForwardingMessage(OpenSSHAgentForwardingMessageType.SSH_AGENT_RSA_IDENTITIES_ANSWER);
            message.WriteInt32(authKeys.Length);
            foreach (var key in authKeys) {
                message.WriteInt32(key.PublicModulus.BitCount());
                SSH1PacketBuilderMixin.WriteBigInteger(message, key.PublicExponent);
                SSH1PacketBuilderMixin.WriteBigInteger(message, key.PublicModulus);
                message.WriteString(key.Comment);
            }
            Send(message);
        }

        /// <summary>
        /// SSH1 RSA challenge
        /// </summary>
        /// <param name="e">public exponent</param>
        /// <param name="n">public modulus</param>
        /// <param name="encryptedChallenge">encrypted challenge</param>
        /// <param name="sessionId">session id</param>
        /// <param name="responseType">response type</param>
        private void SSH1IRSAChallenge(BigInteger e, BigInteger n, BigInteger encryptedChallenge, byte[] sessionId, uint responseType) {
            if (responseType != 1) {
                SendFailure();
                return;
            }

            SSH1UserAuthKey key = SSH1FindKey(e, n);
            if (key == null) {
                SendFailure();
                return;
            }

            BigInteger challenge = key.decryptChallenge(encryptedChallenge);
            byte[] rawchallenge = RSAUtil.StripPKCS1Pad(challenge, 2).GetBytes();
            byte[] hash;
            using (var md5 = new MD5CryptoServiceProvider()) {
                md5.TransformBlock(rawchallenge, 0, rawchallenge.Length, rawchallenge, 0);
                md5.TransformFinalBlock(sessionId, 0, sessionId.Length);
                hash = md5.Hash;
            }

            Send(
                new OpenSSHAgentForwardingMessage(OpenSSHAgentForwardingMessageType.SSH_AGENT_RSA_RESPONSE)
                    .Write(hash)
            );
        }

        /// <summary>
        /// Find a SSH1 key
        /// </summary>
        /// <param name="e">public exponent</param>
        /// <param name="n">public modulus</param>
        /// <returns>matched key object, or null if not found.</returns>
        private SSH1UserAuthKey SSH1FindKey(BigInteger e, BigInteger n) {
            var authKeys = _authKeyProvider.GetAvailableSSH1UserAuthKeys();
            if (authKeys == null) {
                return null;
            }
            return authKeys.FirstOrDefault(key => (key.PublicModulus == n && key.PublicExponent == e));
        }

        /// <summary>
        /// List SSH2 keys
        /// </summary>
        private void SSH2Identities() {
            var authKeys = _authKeyProvider.GetAvailableSSH2UserAuthKeys() ?? new SSH2UserAuthKey[0];
            var message = new OpenSSHAgentForwardingMessage(OpenSSHAgentForwardingMessageType.SSH2_AGENT_IDENTITIES_ANSWER);
            message.WriteInt32(authKeys.Length);
            foreach (var key in authKeys) {
                byte[] blob = key.GetPublicKeyBlob();
                message.WriteAsString(blob);
                message.WriteUTF8String(key.Comment);
            }
            Send(message);
        }

        /// <summary>
        /// SSH2 private key signature
        /// </summary>
        private void SSH2Sign(byte[] blob, byte[] data, uint flags) {
            if ((flags & SSH_AGENT_OLD_SIGNATURE) != 0) {
                SendFailure();
                return;
            }

            SSH2UserAuthKey key = SSH2FindKey(blob);
            if (key == null) {
                SendFailure();
                return;
            }

            SSH2PayloadImageBuilder image = new SSH2PayloadImageBuilder();
            image.WriteString(key.Algorithm.GetAlgorithmName());
            image.WriteAsString(key.Sign(data));
            byte[] signatureBlob = image.GetBytes();

            Send(
                new OpenSSHAgentForwardingMessage(OpenSSHAgentForwardingMessageType.SSH2_AGENT_SIGN_RESPONSE)
                    .WriteAsString(signatureBlob)
            );
        }

        /// <summary>
        /// Find a SSH2 key
        /// </summary>
        /// <param name="blob">key blob</param>
        /// <returns>matched key object, or null if not found.</returns>
        private SSH2UserAuthKey SSH2FindKey(byte[] blob) {
            var authKeys = _authKeyProvider.GetAvailableSSH2UserAuthKeys();
            if (authKeys == null) {
                return null;
            }
            return authKeys.FirstOrDefault(key => SSHUtil.ByteArrayEqual(blob, key.GetPublicKeyBlob()));
        }

        /// <summary>
        /// Sends SSH_AGENT_FAILURE message.
        /// </summary>
        private void SendFailure() {
            Send(
                new OpenSSHAgentForwardingMessage(OpenSSHAgentForwardingMessageType.SSH_AGENT_FAILURE)
            );
        }

        /// <summary>
        /// Sends a message.
        /// </summary>
        /// <param name="message">a message object</param>
        private void Send(OpenSSHAgentForwardingMessage message) {
            _channel.Send(message.GetImage());
        }
    }

}
