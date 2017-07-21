// Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Granados.PKI;
using Granados.Util;
using Granados.IO;
using Granados.Mono.Math;
using Granados.IO.SSH2;
using Granados.Crypto;
#if PODEROSA_KEYFORMAT
using Granados.Poderosa.KeyFormat;
#endif

namespace Granados.SSH2 {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class SSH2UserAuthKey {

        private const int MAGIC_VAL = 0x3f6ff9eb;

        private KeyPair _keypair;

        private string _comment;

        public SSH2UserAuthKey(KeyPair kp) {
            _keypair = kp;
            _comment = "";
        }
        public SSH2UserAuthKey(KeyPair kp, string comment) {
            _keypair = kp;
            _comment = comment;
        }

        public PublicKeyAlgorithm Algorithm {
            get {
                return _keypair.Algorithm;
            }
        }
        public KeyPair KeyPair {
            get {
                return _keypair;
            }
        }
        public string Comment {
            get {
                return _comment;
            }
        }

        public byte[] Sign(byte[] data) {
            switch (_keypair.Algorithm) {
                case PublicKeyAlgorithm.RSA:
                    return ((RSAKeyPair)_keypair).SignWithSHA1(data);
                case PublicKeyAlgorithm.DSA:
                    return ((DSAKeyPair)_keypair).Sign(new SHA1CryptoServiceProvider().ComputeHash(data));
                case PublicKeyAlgorithm.ECDSA_SHA2_NISTP256:
                case PublicKeyAlgorithm.ECDSA_SHA2_NISTP384:
                case PublicKeyAlgorithm.ECDSA_SHA2_NISTP521:
                    return ((ECDSAKeyPair)_keypair).Sign(data);
                case PublicKeyAlgorithm.ED25519:
                    return ((EDDSAKeyPair)_keypair).Sign(data);
                default:
                    throw new SSHException("unknown algorithm");
            }
        }
        public byte[] GetPublicKeyBlob() {
            SSH2PayloadImageBuilder imageBuilder =
                new SSH2PayloadImageBuilder()
                .WriteString(_keypair.Algorithm.GetAlgorithmName());
            _keypair.PublicKey.WriteTo(new SSH2KeyWriter(imageBuilder));
            return imageBuilder.GetBytes();
        }

        /// <summary>
        /// Adapter class that adds <see cref="IKeyWriter"/> functionality to the <see cref="ISSH2PacketBuilder"/>.
        /// </summary>
        private class SSH2KeyWriter : IKeyWriter {

            private readonly ISSH2PacketBuilder _builder;

            public SSH2KeyWriter(ISSH2PacketBuilder builder) {
                _builder = builder;
            }

            public void WriteString(string s) {
                _builder.WriteString(s);
            }

            public void WriteByteString(byte[] data) {
                _builder.WriteAsString(data);
            }

            public void WriteBigInteger(BigInteger bi) {
                _builder.WriteBigInteger(bi);
            }
        }


        public static byte[] PassphraseToKey(string passphrase, int length) {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] pp = Encoding.UTF8.GetBytes(passphrase);
            int hashlen = md5.HashSize / 8;
            byte[] buf = new byte[((length + hashlen) / hashlen) * hashlen];
            int offset = 0;

            while (offset < length) {
                MemoryStream s = new MemoryStream();
                s.Write(pp, 0, pp.Length);
                if (offset > 0)
                    s.Write(buf, 0, offset);
                Array.Copy(md5.ComputeHash(s.ToArray()), 0, buf, offset, hashlen);
                offset += hashlen;
                md5.Initialize();
            }

            byte[] key = new byte[length];
            Array.Copy(buf, 0, key, 0, length);
            return key;
        }

        /*
         * Format style note
         *  ---- BEGIN SSH2 ENCRYPTED PRIVATE KEY ----
         *  Comment: *******
         *  <base64-encoded body>
         *  ---- END SSH2 ENCRYPTED PRIVATE KEY ----
         * 
         *  body = MAGIC_VAL || body-length || type(string) || encryption-algorithm-name(string) || encrypted-body(string)
         *  encrypted-body = array of BigInteger(algorithm-specific)
         */
#if !PODEROSA_KEYFORMAT
        public static SSH2UserAuthKey FromSECSHStyleStream(Stream strm, string passphrase) {
            StreamReader r = new StreamReader(strm, Encoding.ASCII);
            string l = r.ReadLine();
            if (l == null || l != "---- BEGIN SSH2 ENCRYPTED PRIVATE KEY ----")
                throw new SSHException("Wrong key format");

            string comment = "";
            l = r.ReadLine();
            StringBuilder buf = new StringBuilder();
            while (l != "---- END SSH2 ENCRYPTED PRIVATE KEY ----") {
                if (l.IndexOf(':') == -1)
                    buf.Append(l);
                else if (l[l.Length - 1] == '\\')
                    buf.Append(l, 0, l.Length - 1);
                else if (l.StartsWith("Comment: "))
                    comment = l.Substring("Comment: ".Length);

                l = r.ReadLine();
                if (l == null)
                    throw new SSHException("Key is broken");
            }
            r.Close();

            byte[] keydata = Base64.Decode(Encoding.ASCII.GetBytes(buf.ToString()));
            //Debug.WriteLine(DebugUtil.DumpByteArray(keydata));


            SSH2DataReader re = new SSH2DataReader(keydata);
            int magic = re.ReadInt32();
            if (magic != MAGIC_VAL)
                throw new SSHException("key file is broken");
            int privateKeyLen = re.ReadInt32();
            string type = Encoding.ASCII.GetString(re.ReadString());

            string ciphername = Encoding.ASCII.GetString(re.ReadString());
            int bufLen = re.ReadInt32();
            if (ciphername != "none") {
                CipherAlgorithm algo = CipherFactory.SSH2NameToAlgorithm(ciphername);
                byte[] key = PassphraseToKey(passphrase, CipherFactory.GetKeySize(algo));
                Cipher c = CipherFactory.CreateCipher(SSHProtocol.SSH2, algo, key);
                byte[] tmp = new Byte[re.Image.Length - re.Offset];
                c.Decrypt(re.Image, re.Offset, re.Image.Length - re.Offset, tmp, 0);
                re = new SSH2DataReader(tmp);
            }

            int parmLen = re.ReadInt32();
            if (parmLen < 0 || parmLen > re.Rest)
                throw new SSHException(Strings.GetString("WrongPassphrase"));

            if (type.IndexOf("if-modn") != -1) {
                //mindterm mistaken this order of BigIntegers
                BigInteger e = re.ReadBigIntWithBits();
                BigInteger d = re.ReadBigIntWithBits();
                BigInteger n = re.ReadBigIntWithBits();
                BigInteger u = re.ReadBigIntWithBits();
                BigInteger p = re.ReadBigIntWithBits();
                BigInteger q = re.ReadBigIntWithBits();
                return new SSH2UserAuthKey(new RSAKeyPair(e, d, n, u, p, q), comment);
            }
            else if (type.IndexOf("dl-modp") != -1) {
                if (re.ReadInt32() != 0)
                    throw new SSHException("DSS Private Key File is broken");
                BigInteger p = re.ReadBigIntWithBits();
                BigInteger g = re.ReadBigIntWithBits();
                BigInteger q = re.ReadBigIntWithBits();
                BigInteger y = re.ReadBigIntWithBits();
                BigInteger x = re.ReadBigIntWithBits();
                return new SSH2UserAuthKey(new DSAKeyPair(p, g, q, y, x), comment);
            }
            else
                throw new SSHException("unknown authentication method " + type);

        }
#endif
        public static SSH2UserAuthKey FromSECSHStyleFile(string filename, string passphrase) {
#if PODEROSA_KEYFORMAT
            PrivateKeyLoader loader = new PrivateKeyLoader(filename);
            KeyPair keyPair;
            string comment;
            loader.LoadSSH2PrivateKey(passphrase, out keyPair, out comment);
            return new SSH2UserAuthKey(keyPair, comment);
#else
            return FromSECSHStyleStream(new FileStream(filename, FileMode.Open, FileAccess.Read), passphrase);
#endif
        }

        public void WritePrivatePartInSECSHStyleFile(Stream dest, string comment, string passphrase) {

            //step1 key body
            SSH2DataWriter wr = new SSH2DataWriter();
            wr.WriteInt32(0); //this field is filled later
            if (_keypair.Algorithm == PublicKeyAlgorithm.RSA) {
                RSAKeyPair rsa = (RSAKeyPair)_keypair;
                RSAPublicKey pub = (RSAPublicKey)_keypair.PublicKey;
                wr.WriteBigIntWithBits(pub.Exponent);
                wr.WriteBigIntWithBits(rsa.D);
                wr.WriteBigIntWithBits(pub.Modulus);
                wr.WriteBigIntWithBits(rsa.U);
                wr.WriteBigIntWithBits(rsa.P);
                wr.WriteBigIntWithBits(rsa.Q);
            }
            else {
                DSAKeyPair dsa = (DSAKeyPair)_keypair;
                DSAPublicKey pub = (DSAPublicKey)_keypair.PublicKey;
                wr.WriteInt32(0);
                wr.WriteBigIntWithBits(pub.P);
                wr.WriteBigIntWithBits(pub.G);
                wr.WriteBigIntWithBits(pub.Q);
                wr.WriteBigIntWithBits(pub.Y);
                wr.WriteBigIntWithBits(dsa.X);
            }

            int padding_len = 0;
            if (passphrase != null) {
                padding_len = 8 - (int)wr.Length % 8;
                wr.Write(new byte[padding_len]);
            }
            byte[] encrypted_body = wr.ToByteArray();
            SSHUtil.WriteIntToByteArray(encrypted_body, 0, encrypted_body.Length - padding_len - 4);

            //encrypt if necessary
            if (passphrase != null) {
                Cipher c = CipherFactory.CreateCipher(SSHProtocol.SSH2, CipherAlgorithm.TripleDES, PassphraseToKey(passphrase, 24));
                Debug.Assert(encrypted_body.Length % 8 == 0);
                byte[] tmp = new Byte[encrypted_body.Length];
                c.Encrypt(encrypted_body, 0, encrypted_body.Length, tmp, 0);
                encrypted_body = tmp;
            }

            //step2 make binary key data
            wr = new SSH2DataWriter();
            wr.WriteInt32(MAGIC_VAL);
            wr.WriteInt32(0); //for total size
            wr.WriteString(_keypair.Algorithm == PublicKeyAlgorithm.RSA ?
                "if-modn{sign{rsa-pkcs1-sha1},encrypt{rsa-pkcs1v2-oaep}}" :
                "dl-modp{sign{dsa-nist-sha1},dh{plain}}");

            wr.WriteString(passphrase == null ? "none" : "3des-cbc");
            wr.WriteAsString(encrypted_body);

            byte[] rawdata = wr.ToByteArray();
            SSHUtil.WriteIntToByteArray(rawdata, 4, rawdata.Length); //fix total length

            //step3 write final data
            StreamWriter sw = new StreamWriter(dest, Encoding.ASCII);
            sw.WriteLine("---- BEGIN SSH2 ENCRYPTED PRIVATE KEY ----");
            if (comment != null)
                WriteKeyFileBlock(sw, "Comment: " + comment, true);
            WriteKeyFileBlock(sw, Encoding.ASCII.GetString(Base64.Encode(rawdata)), false);
            sw.WriteLine("---- END SSH2 ENCRYPTED PRIVATE KEY ----");
            sw.Close();

        }

        public void WritePublicPartInSECSHStyle(Stream dest, string comment) {
            StreamWriter sw = new StreamWriter(dest, Encoding.ASCII);
            sw.WriteLine("---- BEGIN SSH2 PUBLIC KEY ----");
            if (comment != null)
                WriteKeyFileBlock(sw, "Comment: " + comment, true);
            WriteKeyFileBlock(sw, FormatBase64EncodedPublicKeyBody(), false);
            sw.WriteLine("---- END SSH2 PUBLIC KEY ----");
            sw.Close();

        }
        public void WritePublicPartInOpenSSHStyle(Stream dest) {
            StreamWriter sw = new StreamWriter(dest, Encoding.ASCII);
            sw.Write(_keypair.Algorithm.GetAlgorithmName());
            sw.Write(' ');
            sw.WriteLine(FormatBase64EncodedPublicKeyBody());
            sw.Close();
        }
        private string FormatBase64EncodedPublicKeyBody() {
            return Encoding.ASCII.GetString(Base64.Encode(GetPublicKeyBlob()));
        }

        private static void WriteKeyFileBlock(StreamWriter sw, string data, bool escape_needed) {
            char[] d = data.ToCharArray();
            int cursor = 0;
            const int maxlen = 70;
            while (cursor < d.Length) {
                if (maxlen >= d.Length - cursor)
                    sw.WriteLine(d, cursor, d.Length - cursor);
                else {
                    if (escape_needed) {
                        sw.Write(d, cursor, maxlen - 1);
                        sw.WriteLine('\\');
                        cursor--;
                    }
                    else
                        sw.WriteLine(d, cursor, maxlen);
                }

                cursor += maxlen;
            }
        }
    }

}
