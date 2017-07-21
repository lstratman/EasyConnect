// Copyright 2011-2017 The Poderosa Project.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.

using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using Granados.Crypto;
using Granados.PKI;
using Granados.Util;
using Granados.Mono.Math;

namespace Granados.Poderosa.KeyFormat {

    /// <summary>
    /// OpenSSH SSH2 private key loader
    /// </summary>
    internal class OpenSSHPrivateKeyLoader : ISSH2PrivateKeyLoader {

        private readonly string _keyFilePath;
        private readonly byte[] _keyFileImage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyFile">key file data</param>
        /// <param name="keyFilePath">Path of a key file</param>
        public OpenSSHPrivateKeyLoader(byte[] keyFile, string keyFilePath) {
            this._keyFilePath = keyFilePath;
            this._keyFileImage = keyFile;
        }

        /// <summary>
        /// Read OpenSSH SSH2 private key parameters.
        /// </summary>
        /// <param name="passphrase">passphrase for decrypt the key file</param>
        /// <param name="keyPair">key pair</param>
        /// <param name="comment">comment or empty if it didn't exist</param>
        public void Load(string passphrase, out KeyPair keyPair, out string comment) {
            if (_keyFileImage == null)
                throw new SSHException("A key file is not loaded yet");

            bool openssh;
            using (StreamReader sreader = GetStreamReader()) {
                string line = sreader.ReadLine();
                if (line == null) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected eof)");
                }

                if (line == PrivateKeyFileHeader.SSH2_OPENSSH_HEADER_OPENSSH) {
                    openssh = true;
                }
                else {
                    openssh = false;
                }
            }

            if (openssh) {
                (new OpenSSHFileLoader(_keyFileImage)).Load(passphrase, out keyPair, out comment);
            }
            else {
                (new OpenSSHPKCSFileLoader(_keyFileImage)).Load(passphrase, out keyPair, out comment);
            }
        }

        private StreamReader GetStreamReader() {
            MemoryStream mem = new MemoryStream(_keyFileImage, false);
            return new StreamReader(mem, Encoding.ASCII);
        }
    }


    /// <summary>
    /// OpenSSH PEM/PKCS file loader
    /// </summary>
    internal class OpenSSHPKCSFileLoader {

        private readonly byte[] _keyFileImage;

        private enum PEMKeyType {
            RSA,
            DSA,
            ECDSA,
        }

        public OpenSSHPKCSFileLoader(byte[] image) {
            _keyFileImage = image;
        }

        /// <summary>
        /// Read private key parameters.
        /// </summary>
        /// <param name="passphrase">passphrase for decrypt the key file</param>
        /// <param name="keyPair">key pair</param>
        /// <param name="comment">comment or empty if it didn't exist</param>
        public void Load(string passphrase, out KeyPair keyPair, out string comment) {
            PEMKeyType keyType;
            String base64Text;
            bool encrypted = false;
            CipherAlgorithm? encryption = null;
            byte[] iv = null;
            int keySize = 0;
            int ivSize = 0;

            using (StreamReader sreader = GetStreamReader()) {
                string line = sreader.ReadLine();
                if (line == null)
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected eof)");

                if (line == PrivateKeyFileHeader.SSH2_OPENSSH_HEADER_RSA)
                    keyType = PEMKeyType.RSA;
                else if (line == PrivateKeyFileHeader.SSH2_OPENSSH_HEADER_DSA)
                    keyType = PEMKeyType.DSA;
                else if (line == PrivateKeyFileHeader.SSH2_OPENSSH_HEADER_ECDSA)
                    keyType = PEMKeyType.ECDSA;
                else
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected key type)");

                string footer = line.Replace("BEGIN", "END");

                StringBuilder buf = new StringBuilder();
                comment = String.Empty;
                while (true) {
                    line = sreader.ReadLine();
                    if (line == null)
                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected eof)");
                    if (line == footer)
                        break;
                    if (line.IndexOf(':') >= 0) {
                        if (line.StartsWith("Proc-Type:")) {
                            string[] w = line.Substring("Proc-Type:".Length).Trim().Split(',');
                            if (w.Length < 1)
                                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid Proc-Type)");
                            if (w[0] != "4")
                                throw new SSHException(Strings.GetString("UnsupportedPrivateKeyFormat")
                                            + " (" + Strings.GetString("Reason_UnsupportedProcType") + ")");
                            if (w.Length >= 2 && w[1] == "ENCRYPTED")
                                encrypted = true;
                        }
                        else if (line.StartsWith("DEK-Info:")) {
                            string[] w = line.Substring("DEK-Info:".Length).Trim().Split(',');
                            if (w.Length < 2)
                                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid DEK-Info)");
                            switch (w[0]) {
                                case "DES-EDE3-CBC":
                                    encryption = CipherAlgorithm.TripleDES;
                                    ivSize = 8;
                                    keySize = 24;
                                    break;
                                case "AES-128-CBC":
                                    encryption = CipherAlgorithm.AES128;
                                    ivSize = 16;
                                    keySize = 16;
                                    break;
                                default:
                                    throw new SSHException(Strings.GetString("UnsupportedPrivateKeyFormat")
                                            + " (" + Strings.GetString("Reason_UnsupportedEncryptionType") + ")");
                            }
                            iv = HexToByteArray(w[1]);
                            if (iv == null || iv.Length != ivSize)
                                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid IV)");
                        }
                    }
                    else
                        buf.Append(line);
                }
                base64Text = buf.ToString();
            }

            byte[] keydata = Base64.Decode(Encoding.ASCII.GetBytes(base64Text));

            if (encrypted) {
                if (!encryption.HasValue || iv == null)
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (missing encryption type or IV)");
                byte[] key = PassphraseToKey(passphrase, iv, keySize);
                Cipher cipher = CipherFactory.CreateCipher(SSHProtocol.SSH2, encryption.Value, key, iv);
                if (keydata.Length % cipher.BlockSize != 0)
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid key data size)");
                cipher.Decrypt(keydata, 0, keydata.Length, keydata, 0);
            }

            using (MemoryStream keyDataStream = new MemoryStream(keydata, false)) {
                BERReader reader = new BERReader(keyDataStream);
                if (!reader.ReadSequence())
                    throw new SSHException(Strings.GetString("WrongPassphrase"));
                if (keyType == PEMKeyType.RSA) {
                    /* from OpenSSL rsa_asn1.c
                     * 
                     * ASN1_SIMPLE(RSA, version, LONG),
                     * ASN1_SIMPLE(RSA, n, BIGNUM),
                     * ASN1_SIMPLE(RSA, e, BIGNUM),
                     * ASN1_SIMPLE(RSA, d, BIGNUM),
                     * ASN1_SIMPLE(RSA, p, BIGNUM),
                     * ASN1_SIMPLE(RSA, q, BIGNUM),
                     * ASN1_SIMPLE(RSA, dmp1, BIGNUM),
                     * ASN1_SIMPLE(RSA, dmq1, BIGNUM),
                     * ASN1_SIMPLE(RSA, iqmp, BIGNUM)
                     */
                    BigInteger v, n, e, d, p, q, dmp1, dmq1, iqmp;
                    if (!reader.ReadInteger(out v) ||
                        !reader.ReadInteger(out n) ||
                        !reader.ReadInteger(out e) ||
                        !reader.ReadInteger(out d) ||
                        !reader.ReadInteger(out p) ||
                        !reader.ReadInteger(out q) ||
                        !reader.ReadInteger(out dmp1) ||
                        !reader.ReadInteger(out dmq1) ||
                        !reader.ReadInteger(out iqmp)) {

                        throw new SSHException(Strings.GetString("WrongPassphrase"));
                    }

                    BigInteger u = p.ModInverse(q);	// inverse of p mod q
                    keyPair = new RSAKeyPair(e, d, n, u, p, q);
                }
                else if (keyType == PEMKeyType.DSA) {
                    /* from OpenSSL dsa_asn1.c
                     * 
                     * ASN1_SIMPLE(DSA, version, LONG),
                     * ASN1_SIMPLE(DSA, p, BIGNUM),
                     * ASN1_SIMPLE(DSA, q, BIGNUM),
                     * ASN1_SIMPLE(DSA, g, BIGNUM),
                     * ASN1_SIMPLE(DSA, pub_key, BIGNUM),
                     * ASN1_SIMPLE(DSA, priv_key, BIGNUM)
                     */
                    BigInteger v, p, q, g, y, x;
                    if (!reader.ReadInteger(out v) ||
                        !reader.ReadInteger(out p) ||
                        !reader.ReadInteger(out q) ||
                        !reader.ReadInteger(out g) ||
                        !reader.ReadInteger(out y) ||
                        !reader.ReadInteger(out x)) {

                        throw new SSHException(Strings.GetString("WrongPassphrase"));
                    }
                    keyPair = new DSAKeyPair(p, g, q, y, x);
                }
                else if (keyType == PEMKeyType.ECDSA) {
                    /* from OpenSSL ec_asn1.c
                     *
                     * ASN1_SIMPLE(EC_PRIVATEKEY, version, LONG),
                     * ASN1_SIMPLE(EC_PRIVATEKEY, privateKey, ASN1_OCTET_STRING),
                     * ASN1_EXP_OPT(EC_PRIVATEKEY, parameters, ECPKPARAMETERS, 0),
                     *   ------
                     *   ASN1_SIMPLE(ECPKPARAMETERS, value.named_curve, ASN1_OBJECT),
                     *   ------
                     * ASN1_EXP_OPT(EC_PRIVATEKEY, publicKey, ASN1_BIT_STRING, 1)
                     */
                    int len;
                    byte[] privateKey;
                    byte[] publicKey;
                    string namedCurve;
                    BigInteger v;
                    if (!reader.ReadInteger(out v) ||
                        !reader.ReadOctetString(out privateKey) ||
                        !reader.ReadTag(BERReader.TagClass.ContextSpecific, true, 0, out len) ||
                        !reader.ReadObjectIdentifier(out namedCurve) ||
                        !reader.ReadTag(BERReader.TagClass.ContextSpecific, true, 1, out len) ||
                        !reader.ReadBitString(out publicKey)) {

                        throw new SSHException(Strings.GetString("WrongPassphrase"));
                    }

                    EllipticCurve curve = EllipticCurve.FindByOID(namedCurve);
                    if (curve == null) {
                        throw new SSHException(Strings.GetString("UnsupportedEllipticCurveInKeyPair"));
                    }

                    ECPoint ecPublicKeyPoint;
                    if (!ECPoint.Parse(publicKey, curve, out ecPublicKeyPoint)) {
                        throw new SSHException(Strings.GetString("KeysAreBroken"));
                    }

                    var ecKeyPair = new ECDSAKeyPair(curve, new ECDSAPublicKey(curve, ecPublicKeyPoint), new BigInteger(privateKey));

                    if (!ecKeyPair.CheckKeyConsistency()) {
                        throw new SSHException(Strings.GetString("KeysAreBroken"));
                    }

                    keyPair = ecKeyPair;
                }
                else {
                    throw new SSHException("Unknown file type. This should not happen.");
                }
            }
        }

        private StreamReader GetStreamReader() {
            MemoryStream mem = new MemoryStream(_keyFileImage, false);
            return new StreamReader(mem, Encoding.ASCII);
        }

        private static byte[] HexToByteArray(string text) {
            if (text.Length % 2 != 0) {
                return null;
            }
            return BigIntegerConverter.ParseHex(text);
        }

        private static byte[] PassphraseToKey(string passphrase, byte[] iv, int length) {
            const int HASH_SIZE = 16;
            const int SALT_SIZE = 8;
            byte[] pp = Encoding.UTF8.GetBytes(passphrase);
            byte[] buf = new byte[((length + HASH_SIZE - 1) / HASH_SIZE) * HASH_SIZE];
            int offset = 0;
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider()) {
                while (offset < length) {
                    if (offset > 0)
                        md5.TransformBlock(buf, 0, offset, null, 0);
                    md5.TransformBlock(pp, 0, pp.Length, null, 0);
                    md5.TransformFinalBlock(iv, 0, SALT_SIZE);
                    Buffer.BlockCopy(md5.Hash, 0, buf, offset, HASH_SIZE);
                    offset += HASH_SIZE;
                    md5.Initialize();
                }
            }
            byte[] key = new byte[length];
            Buffer.BlockCopy(buf, 0, key, 0, length);
            return key;
        }
    }

    /// <summary>
    /// OpenSSH proprietary format file loader
    /// </summary>
    internal class OpenSSHFileLoader {

        private readonly byte[] _keyFileImage;

        public OpenSSHFileLoader(byte[] image) {
            _keyFileImage = image;
        }

        /// <summary>
        /// Read private key parameters with OpenSSH format.
        /// </summary>
        /// <param name="passphrase">passphrase for decrypt the key file</param>
        /// <param name="keyPair">key pair</param>
        /// <param name="comment">comment or empty if it didn't exist</param>
        public void Load(string passphrase, out KeyPair keyPair, out string comment) {
            // Note:
            //  The structure of the private key format is described in "PROTOCOL.key" file in OpenSSH sources.

            String base64Text;
            using (StreamReader sreader = GetStreamReader()) {
                string line = sreader.ReadLine();
                if (line == null) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected eof)");
                }

                if (line != PrivateKeyFileHeader.SSH2_OPENSSH_HEADER_OPENSSH) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected key type)");
                }

                string footer = line.Replace("BEGIN", "END");

                StringBuilder buf = new StringBuilder();
                while (true) {
                    line = sreader.ReadLine();
                    if (line == null) {
                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected eof)");
                    }
                    if (line == footer) {
                        break;
                    }
                    buf.Append(line);
                }
                base64Text = buf.ToString();
            }

            byte[] blob = Base64.Decode(Encoding.ASCII.GetBytes(base64Text));

            int numKeys;    // number of keys
            byte[][] publicKeyBlobs;
            byte[] privateKeyData;
            bool decrypted;

            using (var blobStream = new MemoryStream(blob, false)) {
                if (!CheckMagic(blobStream)) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unsupported format)");
                }

                string cipherName = ReadString(blobStream);
                CipherAlgorithm? cipherAlgorithm;
                int cipherKeySize;
                int cipherIVSize;
                switch (cipherName) {
                    case "none":
                        cipherAlgorithm = null;
                        cipherKeySize = 0;
                        cipherIVSize = 0;
                        break;
                    case "aes128-cbc":
                        cipherAlgorithm = CipherAlgorithm.AES128;
                        cipherKeySize = 16;
                        cipherIVSize = 16;  // use block size
                        break;
                    case "aes192-cbc":
                        cipherAlgorithm = CipherAlgorithm.AES192;
                        cipherKeySize = 24;
                        cipherIVSize = 16;  // use block size
                        break;
                    case "aes256-cbc":
                        cipherAlgorithm = CipherAlgorithm.AES256;
                        cipherKeySize = 32;
                        cipherIVSize = 16;  // use block size
                        break;
                    default:
                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unsupported cipher)");
                }

                string kdfName = ReadString(blobStream);
                if (kdfName != "bcrypt" && kdfName != "none") {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unsupported kdf)");
                }

                if ((cipherName == "none") != (kdfName == "none")) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid cipher)");
                }

                byte[] kdfOptions = ReadBytes(blobStream);
                if (kdfOptions == null) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (missing kdf options)");
                }
                byte[] kdfSalt;
                uint kdfRounds;
                byte[] key;
                byte[] iv;
                if (kdfName == "none") {
                    kdfSalt = null;
                    kdfRounds = 0;
                    key = null;
                    iv = null;
                }
                else {
                    if (!ReadKdfOptions(kdfOptions, out kdfSalt, out kdfRounds)) {
                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid kdf options)");
                    }

                    if (passphrase == null || passphrase.Length == 0) {
                        throw new SSHException(Strings.GetString("WrongPassphrase"));
                    }

                    // prepare decryption
                    Bcrypt bcrypt = new Bcrypt();
                    byte[] tmpkey = bcrypt.BcryptPbkdf(passphrase, kdfSalt, kdfRounds, cipherKeySize + cipherIVSize);
                    if (tmpkey == null) {
                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid kdf options)");
                    }
                    key = new byte[cipherKeySize];
                    Buffer.BlockCopy(tmpkey, 0, key, 0, cipherKeySize);
                    iv = new byte[cipherIVSize];
                    Buffer.BlockCopy(tmpkey, cipherKeySize, iv, 0, cipherIVSize);
                }

                if (!ReadInt32(blobStream, out numKeys) || numKeys < 0) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (missing keys)");
                }

                publicKeyBlobs = new byte[numKeys][];
                for (int i = 0; i < numKeys; ++i) {
                    byte[] data = ReadBytes(blobStream);
                    if (data == null) {
                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (missing public keys)");
                    }
                    publicKeyBlobs[i] = data;
                }

                privateKeyData = ReadBytes(blobStream);
                if (privateKeyData == null) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (missing private keys)");
                }

                if (cipherAlgorithm.HasValue && key != null && iv != null) {
                    // decrypt private keys
                    Cipher cipher = CipherFactory.CreateCipher(SSHProtocol.SSH2, cipherAlgorithm.Value, key, iv);
                    cipher.Decrypt(privateKeyData, 0, privateKeyData.Length, privateKeyData, 0);
                    decrypted = true;
                }
                else {
                    decrypted = false;
                }
            }

            using (var privateKeysStream = new MemoryStream(privateKeyData, false)) {
                uint check1, check2;
                if (!ReadUInt32(privateKeysStream, out check1) ||
                    !ReadUInt32(privateKeysStream, out check2)) {
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (invalid private keys)");
                }
                if (check1 != check2) {
                    throw new SSHException(decrypted ?
                        Strings.GetString("WrongPassphrase") : Strings.GetString("NotValidPrivateKeyFile"));
                }

                for (int i = 0; i < numKeys; ++i) {
                    string privateKeyType = ReadString(privateKeysStream);

                    using (var publicKeyBlobStream = new MemoryStream(publicKeyBlobs[i], false)) {
                        string publicKeyType = ReadString(publicKeyBlobStream);
                        if (publicKeyType != privateKeyType) {
                            throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (key type unmatched)");
                        }

                        switch (privateKeyType) {
                            case "ssh-ed25519": {
                                    byte[] pk = ReadBytes(privateKeysStream);
                                    if (pk == null) {
                                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile"));
                                    }
                                    byte[] sk = ReadBytes(privateKeysStream);
                                    if (sk == null) {
                                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile"));
                                    }
                                    string cmnt = ReadString(privateKeysStream);   // comment
                                    if (cmnt == null) {
                                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile"));
                                    }

                                    byte[] publicKey = ReadBytes(publicKeyBlobStream);
                                    if (publicKey == null) {
                                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile"));
                                    }

                                    // sanity check
                                    if (!AreEqual(publicKey, pk)) {
                                        throw new SSHException(Strings.GetString("WrongPassphrase"));
                                    }

                                    // first 32 bytes of secret key is used as a private key for ed25519
                                    byte[] privateKey = new byte[32];
                                    if (sk.Length < privateKey.Length) {
                                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile"));
                                    }
                                    Buffer.BlockCopy(sk, 0, privateKey, 0, privateKey.Length);

                                    var curve = EdwardsCurve.FindByAlgorithm(PublicKeyAlgorithm.ED25519);
                                    if (curve != null) {
                                        var kp = new EDDSAKeyPair(curve, new EDDSAPublicKey(curve, publicKey), privateKey);
                                        if (!kp.CheckKeyConsistency()) {
                                            throw new SSHException(Strings.GetString("NotValidPrivateKeyFile"));
                                        }
                                        keyPair = kp;
                                        comment = cmnt;
                                        return;
                                    }
                                }
                                break;
                            default:
                                // unsupported key type; check the next key.
                                break;
                        }
                    }
                }
            }

            throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (supported private key was not found)");
        }

        private bool AreEqual(byte[] a1, byte[] a2) {
            if (a1.Length != a2.Length) {
                return false;
            }
            for (int i = 0; i < a1.Length; ++i) {
                if (a1[i] != a2[i]) {
                    return false;
                }
            }
            return true;
        }

        private StreamReader GetStreamReader() {
            MemoryStream mem = new MemoryStream(_keyFileImage, false);
            return new StreamReader(mem, Encoding.ASCII);
        }

        private bool CheckMagic(Stream s) {
            byte[] magic = Encoding.ASCII.GetBytes("openssh-key-v1\0");
            byte[] buff = new byte[magic.Length];
            if (s.Read(buff, 0, magic.Length) != magic.Length) {
                return false;
            }
            for (int i = 0; i < magic.Length; ++i) {
                if (buff[i] != magic[i]) {
                    return false;
                }
            }
            return true;
        }

        private string ReadString(Stream s) {
            byte[] buff = ReadBytes(s);
            if (buff == null) {
                return null;
            }
            return Encoding.ASCII.GetString(buff);
        }

        private byte[] ReadBytes(Stream s) {
            int sz;
            if (!ReadInt32(s, out sz)) {
                return null;
            }
            byte[] buff = new byte[sz];
            if (s.Read(buff, 0, sz) != sz) {
                return null;
            }
            return buff;
        }

        private bool ReadInt32(Stream s, out int val) {
            uint v;
            bool r = ReadUInt32(s, out v);
            val = (int)v;
            return r;
        }

        private bool ReadUInt32(Stream s, out uint val) {
            uint v = 0;
            for (int i = 0; i < 4; ++i) {
                int n = s.ReadByte();
                if (n < 0) {
                    val = 0;
                    return false;
                }
                v = (v << 8) | (uint)(n & 0xff);
            }
            val = v;
            return true;
        }

        private bool ReadKdfOptions(byte[] options, out byte[] salt, out uint rounds) {
            using (var s = new MemoryStream(options, false)) {
                salt = ReadBytes(s);
                if (salt == null) {
                    rounds = 0;
                    return false;
                }

                if (!ReadUInt32(s, out rounds)) {
                    return false;
                }

                return true;
            }
        }

    }

}
