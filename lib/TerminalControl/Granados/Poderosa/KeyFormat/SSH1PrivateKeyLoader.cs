/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SSH1PrivateKeyLoader.cs,v 1.1 2011/11/03 16:27:38 kzmi Exp $
 */
using System;
using System.Text;
using System.Security.Cryptography;

using Granados.Crypto;
using Granados.IO.SSH1;
using Granados.Util;
using Granados.Mono.Math;

namespace Granados.Poderosa.KeyFormat {

    /// <summary>
    /// SSH1 private key loader
    /// </summary>
    internal class SSH1PrivateKeyLoader : ISSH1PrivateKeyLoader {

        private readonly string keyFilePath;
        private readonly byte[] keyFile;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyFile">key file data</param>
        /// <param name="keyFilePath">Path of a key file</param>
        public SSH1PrivateKeyLoader(byte[] keyFile, string keyFilePath) {
            this.keyFilePath = keyFilePath;
            this.keyFile = keyFile;
        }

        /// <summary>
        /// Read SSH1 private key parameters.
        /// </summary>
        /// <param name="passphrase">passphrase for decrypt the key file</param>
        /// <param name="modulus">private key parameter</param>
        /// <param name="publicExponent">private key parameter</param>
        /// <param name="privateExponent">private key parameter</param>
        /// <param name="primeP">private key parameter</param>
        /// <param name="primeQ">private key parameter</param>
        /// <param name="crtCoefficient">private key parameter</param>
        /// <param name="comment">comment</param>
        /// <exception cref="SSHException">failed to parse</exception>
        public void Load(
                            string passphrase,
                            out BigInteger modulus,
                            out BigInteger publicExponent,
                            out BigInteger privateExponent,
                            out BigInteger primeP,
                            out BigInteger primeQ,
                            out BigInteger crtCoefficient,
                            out string comment) {

            if (keyFile == null)
                throw new SSHException("A key file is not loaded yet");
            byte[] hdr = Encoding.ASCII.GetBytes(PrivateKeyFileHeader.SSH1_HEADER);
            if (!ByteArrayUtil.ByteArrayStartsWith(keyFile, hdr))
                throw new SSHException(Strings.GetString("NotValidPrivateKeyFile"));

            SSH1DataReader reader = new SSH1DataReader(keyFile);
            reader.Read(hdr.Length);

            byte[] cipher = reader.Read(2); //first 2 bytes indicates algorithm and next 8 bytes is space
            reader.Read(8);

            modulus = reader.ReadMPInt();
            publicExponent = reader.ReadMPInt();
            comment = reader.ReadString();
            byte[] prvt = reader.GetRemainingDataView().GetBytes();
            //必要なら復号
            CipherAlgorithm algo = (CipherAlgorithm)cipher[1];
            if (algo != 0) {
                Cipher c = CipherFactory.CreateCipher(SSHProtocol.SSH1, algo, SSH1PassphraseToKey(passphrase));
                c.Decrypt(prvt, 0, prvt.Length, prvt, 0);
            }

            SSH1DataReader prvtreader = new SSH1DataReader(prvt);
            byte[] mark = prvtreader.Read(4);
            if (mark[0] != mark[2] || mark[1] != mark[3])
                throw new SSHException(Strings.GetString("WrongPassphrase"));

            privateExponent = prvtreader.ReadMPInt();
            crtCoefficient = prvtreader.ReadMPInt();
            primeP = prvtreader.ReadMPInt();
            primeQ = prvtreader.ReadMPInt();
        }

        //key from passphrase
        private static byte[] SSH1PassphraseToKey(string passphrase) {
            byte[] t = Encoding.ASCII.GetBytes(passphrase);
            byte[] md5 = new MD5CryptoServiceProvider().ComputeHash(t);
            byte[] result = new byte[24];
            Buffer.BlockCopy(md5, 0, result, 0, 16);
            Buffer.BlockCopy(md5, 0, result, 16, 8);
            return result;
        }

    }


}
