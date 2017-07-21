/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SSHComPrivateKeyLoader.cs,v 1.1 2011/11/03 16:27:38 kzmi Exp $
 */
using System;
using System.IO;
using System.Text;

using Granados.Crypto;
using Granados.IO.SSH2;
using Granados.PKI;
using Granados.SSH2;
using Granados.Util;
using Granados.Mono.Math;

namespace Granados.Poderosa.KeyFormat {

    /// <summary>
    /// SSH.com SSH2 private key loader
    /// </summary>
    internal class SSHComPrivateKeyLoader : ISSH2PrivateKeyLoader {

        private readonly string keyFilePath;
        private readonly byte[] keyFile;

        private const int MAGIC = 0x3f6ff9eb;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyFile">key file data</param>
        /// <param name="keyFilePath">Path of a key file</param>
        public SSHComPrivateKeyLoader(byte[] keyFile, string keyFilePath) {
            this.keyFilePath = keyFilePath;
            this.keyFile = keyFile;
        }

        /// <summary>
        /// Read SSH.com SSH2 private key parameters.
        /// </summary>
        /// <param name="passphrase">passphrase for decrypt the key file</param>
        /// <param name="keyPair">key pair</param>
        /// <param name="comment">comment or empty if it didn't exist</param>
        /// <exception cref="SSHException">failed to parse</exception>
        public void Load(string passphrase, out KeyPair keyPair, out string comment) {
            if (keyFile == null)
                throw new SSHException("A key file is not loaded yet");

            String base64Text;
            using (StreamReader sreader = GetStreamReader()) {
                string line = sreader.ReadLine();
                if (line == null || line != PrivateKeyFileHeader.SSH2_SSHCOM_HEADER)
                    throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (missing header)");

                StringBuilder buf = new StringBuilder();
                comment = String.Empty;
                while (true) {
                    line = sreader.ReadLine();
                    if (line == null)
                        throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (unexpected eof)");
                    if (line == PrivateKeyFileHeader.SSH2_SSHCOM_FOOTER)
                        break;
                    if (line.IndexOf(':') >= 0) {
                        if (line.StartsWith("Comment: "))
                            comment = line.Substring("Comment: ".Length);
                    }
                    else if (line[line.Length - 1] == '\\')
                        buf.Append(line, 0, line.Length - 1);
                    else
                        buf.Append(line);
                }
                base64Text = buf.ToString();
            }

            byte[] keydata = Base64.Decode(Encoding.ASCII.GetBytes(base64Text));
            //Debug.WriteLine(DebugUtil.DumpByteArray(keydata));

            SSH2DataReader reader = new SSH2DataReader(keydata);
            int magic = reader.ReadInt32();
            if (magic != MAGIC)
                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile") + " (magic code unmatched)");
            int privateKeyLen = reader.ReadInt32();
            string type = reader.ReadString();

            string ciphername = reader.ReadString();
            int bufLen = reader.ReadInt32();
            if (ciphername != "none") {
                CipherAlgorithm algo = CipherFactory.SSH2NameToAlgorithm(ciphername);
                byte[] key = SSH2UserAuthKey.PassphraseToKey(passphrase, CipherFactory.GetKeySize(algo));
                Cipher c = CipherFactory.CreateCipher(SSHProtocol.SSH2, algo, key);
                byte[] tmp = new Byte[keydata.Length - reader.Offset];
                c.Decrypt(keydata, reader.Offset, keydata.Length - reader.Offset, tmp, 0);
                reader = new SSH2DataReader(tmp);
            }

            int parmLen = reader.ReadInt32();
            if (parmLen < 0 || parmLen > reader.RemainingDataLength)
                throw new SSHException(Strings.GetString("WrongPassphrase"));

            if (type.IndexOf("if-modn") != -1) {
                //mindterm mistaken this order of BigIntegers
                BigInteger e = ReadBigIntWithBits(reader);
                BigInteger d = ReadBigIntWithBits(reader);
                BigInteger n = ReadBigIntWithBits(reader);
                BigInteger u = ReadBigIntWithBits(reader);
                BigInteger p = ReadBigIntWithBits(reader);
                BigInteger q = ReadBigIntWithBits(reader);
                keyPair = new RSAKeyPair(e, d, n, u, p, q);
            }
            else if (type.IndexOf("dl-modp") != -1) {
                if (reader.ReadInt32() != 0)
                    throw new SSHException(Strings.GetString("UnsupportedPrivateKeyFormat")
                            + " (" + Strings.GetString("Reason_UnsupportedDSAKeyFormat") + ")");
                BigInteger p = ReadBigIntWithBits(reader);
                BigInteger g = ReadBigIntWithBits(reader);
                BigInteger q = ReadBigIntWithBits(reader);
                BigInteger y = ReadBigIntWithBits(reader);
                BigInteger x = ReadBigIntWithBits(reader);
                keyPair = new DSAKeyPair(p, g, q, y, x);
            }
            else
                throw new SSHException(Strings.GetString("UnsupportedAuthenticationMethod"));
        }

        private StreamReader GetStreamReader() {
            MemoryStream mem = new MemoryStream(keyFile, false);
            return new StreamReader(mem, Encoding.ASCII);
        }

        /// <summary>
        /// Reads a multiple precision integer.
        /// </summary>
        /// <returns>a multiple precision integer</returns>
        private BigInteger ReadBigIntWithBits(SSH2DataReader reader) {
            int bits = reader.ReadInt32();
            int bytes = (bits + 7) / 8;
            byte[] biData = reader.Read(bytes);
            return new BigInteger(biData);
        }
    }

}
