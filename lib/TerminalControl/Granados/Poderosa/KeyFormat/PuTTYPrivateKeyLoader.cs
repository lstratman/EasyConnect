// Copyright 2011-2017 The Poderosa Project.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using Granados.Crypto;
using Granados.PKI;
using Granados.IO.SSH2;
using Granados.Util;
using Granados.Mono.Math;

namespace Granados.Poderosa.KeyFormat {

    /// <summary>
    /// PuTTY SSH2 private key loader
    /// </summary>
    internal class PuTTYPrivateKeyLoader : ISSH2PrivateKeyLoader {

        private readonly string keyFilePath;
        private readonly byte[] keyFile;

        private enum KeyType {
            RSA,
            DSA,
            ECDSA,
            ED25519,
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyFile">key file data</param>
        /// <param name="keyFilePath">Path of a key file</param>
        public PuTTYPrivateKeyLoader(byte[] keyFile, string keyFilePath) {
            this.keyFilePath = keyFilePath;
            this.keyFile = keyFile;
        }


        /// <summary>
        /// Read PuTTY SSH2 private key parameters.
        /// </summary>
        /// <param name="passphrase">passphrase for decrypt the key file</param>
        /// <param name="keyPair">key pair</param>
        /// <param name="comment">comment or empty if it didn't exist</param>
        public void Load(string passphrase, out KeyPair keyPair, out string comment) {
            if (keyFile == null)
                throw new SSHException("A key file is not loaded yet");

            int version;
            string keyTypeName;
            KeyType keyType;
            string encryptionName;
            CipherAlgorithm? encryption;
            byte[] publicBlob;
            byte[] privateBlob;
            string privateMac;
            string privateHash;

            using (StreamReader sreader = GetStreamReader()) {
                //*** Read header and key type
                ReadHeaderLine(sreader, out version, out keyTypeName);

                if (keyTypeName == "ssh-rsa")
                    keyType = KeyType.RSA;
                else if (keyTypeName == "ssh-dss")
                    keyType = KeyType.DSA;
                else if (keyTypeName.StartsWith("ecdsa-sha2-"))
                    keyType = KeyType.ECDSA;
                else if (keyTypeName == "ssh-ed25519")
                    keyType = KeyType.ED25519;
                else
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected key type)");

                //*** Read encryption
                ReadItemLine(sreader, "Encryption", out encryptionName);

                if (encryptionName == "aes256-cbc")
                    encryption = CipherAlgorithm.AES256;
                else if (encryptionName == "none") {
                    encryption = null;
                    passphrase = "";    // prevent HMAC error
                }
                else
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected encryption)");

                //*** Read comment
                ReadItemLine(sreader, "Comment", out comment);

                //*** Read public lines
                string publicLinesStr;
                ReadItemLine(sreader, "Public-Lines", out publicLinesStr);
                int publicLines;
                if (!Int32.TryParse(publicLinesStr, out publicLines) || publicLines < 0)
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid public lines)");

                ReadBlob(sreader, publicLines, out publicBlob);

                //*** Read private lines
                string privateLinesStr;
                ReadItemLine(sreader, "Private-Lines", out privateLinesStr);
                int privateLines;
                if (!Int32.TryParse(privateLinesStr, out privateLines) || privateLines < 0)
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid private lines)");

                ReadBlob(sreader, privateLines, out privateBlob);

                //*** Read private MAC
                ReadPrivateMACLine(sreader, version, out privateMac, out privateHash);
            }

            if (encryption.HasValue) {
                byte[] key = PuTTYPassphraseToKey(passphrase);
                byte[] iv = new byte[16];
                Cipher cipher = CipherFactory.CreateCipher(SSHProtocol.SSH2, encryption.Value, key, iv);
                if (privateBlob.Length % cipher.BlockSize != 0)
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid key data size)");
                cipher.Decrypt(privateBlob, 0, privateBlob.Length, privateBlob, 0);
            }

            bool verified = Verify(version, privateMac, privateHash,
                                passphrase, keyTypeName, encryptionName, comment, publicBlob, privateBlob);
            if (!verified) {
                if (encryption.HasValue)
                    throw new SSHException(Strings.GetString("WrongPassphrase"));
                else
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (HMAC verification failed)");
            }

            if (keyType == KeyType.RSA) {
                SSH2DataReader reader = new SSH2DataReader(publicBlob);
                string magic = reader.ReadString();
                if (magic != "ssh-rsa")
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (missing magic)");

                BigInteger e = reader.ReadMPInt();
                BigInteger n = reader.ReadMPInt();

                reader = new SSH2DataReader(privateBlob);
                BigInteger d = reader.ReadMPInt();
                BigInteger p = reader.ReadMPInt();
                BigInteger q = reader.ReadMPInt();
                BigInteger iqmp = reader.ReadMPInt();

                BigInteger u = p.ModInverse(q);

                keyPair = new RSAKeyPair(e, d, n, u, p, q);
            }
            else if (keyType == KeyType.DSA) {
                SSH2DataReader reader = new SSH2DataReader(publicBlob);
                string magic = reader.ReadString();
                if (magic != "ssh-dss")
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (missing magic)");

                BigInteger p = reader.ReadMPInt();
                BigInteger q = reader.ReadMPInt();
                BigInteger g = reader.ReadMPInt();
                BigInteger y = reader.ReadMPInt();

                reader = new SSH2DataReader(privateBlob);
                BigInteger x = reader.ReadMPInt();

                keyPair = new DSAKeyPair(p, g, q, y, x);
            }
            else if (keyType == KeyType.ECDSA) {
                SSH2DataReader reader = new SSH2DataReader(publicBlob);
                string algorithmName = reader.ReadString();
                string curveName = reader.ReadString();
                byte[] publicKeyPt = reader.ReadByteString();

                reader = new SSH2DataReader(privateBlob);
                BigInteger privateKey = reader.ReadMPInt();

                EllipticCurve curve = EllipticCurve.FindByName(curveName);
                if (curve == null) {
                    throw new SSHException(Strings.GetString("UnsupportedEllipticCurve") + " : " + curveName);
                }
                ECPoint publicKey;
                if (!ECPoint.Parse(publicKeyPt, curve, out publicKey)) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (parsing public key failed)");
                }

                keyPair = new ECDSAKeyPair(curve, new ECDSAPublicKey(curve, publicKey), privateKey);

                if (!((ECDSAKeyPair)keyPair).CheckKeyConsistency()) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid key pair)");
                }
            }
            else if (keyType == KeyType.ED25519) {
                SSH2DataReader reader = new SSH2DataReader(publicBlob);
                string algorithmName = reader.ReadString();
                byte[] publicKey = reader.ReadByteString();

                reader = new SSH2DataReader(privateBlob);
                byte[] privateKey = reader.ReadByteString();

                EdwardsCurve curve = EdwardsCurve.FindByAlgorithm(PublicKeyAlgorithm.ED25519);
                if (curve == null) {
                    throw new SSHException(Strings.GetString("UnsupportedEllipticCurve"));
                }

                keyPair = new EDDSAKeyPair(curve, new EDDSAPublicKey(curve, publicKey), privateKey);

                if (!((EDDSAKeyPair)keyPair).CheckKeyConsistency()) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid key pair)");
                }
            }
            else {
                throw new SSHException("Unknown file type. This should not happen.");
            }
        }

        private void ReadHeaderLine(StreamReader sreader, out int version, out string keyTypeName) {
            string line = sreader.ReadLine();
            if (line == null)
                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected eof)");

            if (line.StartsWith(PrivateKeyFileHeader.SSH2_PUTTY_HEADER_1))
                version = 1;
            else if (line.StartsWith(PrivateKeyFileHeader.SSH2_PUTTY_HEADER_2))
                version = 2;
            else
                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected format type)");

            keyTypeName = GetValueOf(line);
        }

        private void ReadItemLine(StreamReader sreader, string itemName, out string itemValue) {
            string line = sreader.ReadLine();
            if (line == null)
                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected eof)");

            if (!line.StartsWith(itemName + ":"))
                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (missing " + itemName + ")");

            itemValue = GetValueOf(line);
        }

        private void ReadPrivateMACLine(StreamReader sreader, int version, out string privateMac, out string privateHash) {
            string line = sreader.ReadLine();
            if (line == null)
                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected eof)");

            if (line.StartsWith("Private-MAC:")) {
                privateMac = GetValueOf(line);
                privateHash = null;
            }
            else if (version == 1 && line.StartsWith("Private-Hash:")) {
                privateMac = null;
                privateHash = GetValueOf(line);
            }
            else {
                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile")
                        + " (missing " + (version == 1 ? "Private-Hash" : "Private-MAC") + ")");
            }
        }

        private void ReadBlob(StreamReader sreader, int lines, out byte[] blob) {
            StringBuilder base64Buff = new StringBuilder();
            for (int i = 0; i < lines; i++) {
                string line = sreader.ReadLine();
                if (line == null)
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected eof)");
                base64Buff.Append(line);
            }
            blob = Base64.Decode(Encoding.ASCII.GetBytes(base64Buff.ToString()));
        }

        private static byte[] PuTTYPassphraseToKey(string passphrase) {
            const int HASH_SIZE = 20;
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] pp = Encoding.UTF8.GetBytes(passphrase);

            byte[] buf = new byte[HASH_SIZE * 2];

            sha1.TransformBlock(new byte[] { 0, 0, 0, 0 }, 0, 4, null, 0);
            sha1.TransformFinalBlock(pp, 0, pp.Length);
            Buffer.BlockCopy(sha1.Hash, 0, buf, 0, HASH_SIZE);
            sha1.Initialize();
            sha1.TransformBlock(new byte[] { 0, 0, 0, 1 }, 0, 4, null, 0);
            sha1.TransformFinalBlock(pp, 0, pp.Length);
            Buffer.BlockCopy(sha1.Hash, 0, buf, HASH_SIZE, HASH_SIZE);
            sha1.Clear();

            byte[] key = new byte[32];
            Buffer.BlockCopy(buf, 0, key, 0, key.Length);
            return key;
        }

        private bool Verify(int version, string privateMac, string privateHash,
                string passphrase, string keyTypeName, string encryptionName, string comment, byte[] publicBlob, byte[] privateBlob) {

            byte[] macData;
            using (MemoryStream macDataBuff = new MemoryStream()) {
                if (version == 1) {
                    WriteMacData(macDataBuff, privateBlob);
                }
                else {
                    WriteMacData(macDataBuff, keyTypeName);
                    WriteMacData(macDataBuff, encryptionName);
                    WriteMacData(macDataBuff, comment);
                    WriteMacData(macDataBuff, publicBlob);
                    WriteMacData(macDataBuff, privateBlob);
                }
                macDataBuff.Close();
                macData = macDataBuff.ToArray();
            }

            if (privateMac != null) {
                SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
                byte[] a = Encoding.ASCII.GetBytes("putty-private-key-file-mac-key");
                sha1.TransformBlock(a, 0, a.Length, null, 0);
                byte[] b = Encoding.UTF8.GetBytes(passphrase);
                sha1.TransformFinalBlock(b, 0, b.Length);
                byte[] key = sha1.Hash;
                sha1.Clear();

                System.Security.Cryptography.HMACSHA1 hmacsha1 = new System.Security.Cryptography.HMACSHA1(key);
                byte[] hash = hmacsha1.ComputeHash(macData);
                hmacsha1.Clear();
                string mac = BinToHex(hash);
                return mac == privateMac;
            }
            else if (privateHash != null) {
                SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
                byte[] hash = sha1.ComputeHash(macData);
                sha1.Clear();
                string mac = BinToHex(hash);
                return mac == privateHash;
            }
            else {
                return true;
            }
        }

        // Extract value from "Name: value" line
        private static string GetValueOf(string line) {
            int p = line.IndexOf(':');
            if (p < 0)
                return null;
            if (p + 1 >= line.Length || line[p + 1] != ' ')
                return null;
            return line.Substring(p + 2);
        }

        private static void WriteMacData(MemoryStream mem, string s) {
            WriteMacData(mem, Encoding.UTF8.GetBytes(s));
        }

        private static void WriteMacData(MemoryStream mem, byte[] data) {
            byte[] dw = BitConverter.GetBytes(data.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(dw);
            mem.Write(dw, 0, dw.Length);
            mem.Write(data, 0, data.Length);
        }

        private static string BinToHex(byte[] data) {
            StringBuilder s = new StringBuilder();
            foreach (byte b in data) {
                s.Append(b.ToString("x2", NumberFormatInfo.InvariantInfo));
            }
            return s.ToString();
        }

        private StreamReader GetStreamReader() {
            MemoryStream mem = new MemoryStream(keyFile, false);
            return new StreamReader(mem, Encoding.ASCII);
        }
    }


}
