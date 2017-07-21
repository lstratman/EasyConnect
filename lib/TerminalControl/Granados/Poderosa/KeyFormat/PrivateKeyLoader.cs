/*
 * Copyright 2011 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: PrivateKeyLoader.cs,v 1.1 2011/11/03 16:27:38 kzmi Exp $
 */
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Security.Cryptography;

using Granados.Crypto;
using Granados.IO.SSH1;
using Granados.IO.SSH2;
using Granados.PKI;
using Granados.SSH2;
using Granados.Util;
using Granados.Mono.Math;

namespace Granados.Poderosa.KeyFormat {

    internal class PrivateKeyLoader {

        private readonly string keyFilePath;
        private readonly byte[] keyFile;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">Path of a key file to load</param>
        /// <exception cref="SSHException">failed to read the key file</exception>
        public PrivateKeyLoader(string path) {
            byte[] data;
            using (FileStream fin = new FileStream(path, FileMode.Open)) {
                long readLen = fin.Length;
                if (readLen > Int32.MaxValue)
                    throw new SSHException("Key file is too large.");
                data = new byte[readLen];
                int len = fin.Read(data, 0, data.Length);
                if (len != readLen)
                    throw new SSHException("Couldn't read the key file.");
            }
            keyFilePath = path;
            keyFile = data;
        }

        /// <summary>
        /// Detect file format of a SSH private key file.
        /// </summary>
        /// <returns>format type</returns>
        /// <exception cref="IOException">File I/O error</exception>
        public PrivateKeyFileFormat ProbeFormat() {
            if (keyFile == null)
                return PrivateKeyFileFormat.UNKNOWN;

            if (ByteArrayUtil.ByteArrayStartsWith(keyFile, Encoding.ASCII.GetBytes(PrivateKeyFileHeader.SSH1_HEADER)))
                return PrivateKeyFileFormat.SSH1;

            if (ByteArrayUtil.ByteArrayStartsWith(keyFile, Encoding.ASCII.GetBytes(PrivateKeyFileHeader.SSH2_OPENSSH_HEADER_RSA))
                || ByteArrayUtil.ByteArrayStartsWith(keyFile, Encoding.ASCII.GetBytes(PrivateKeyFileHeader.SSH2_OPENSSH_HEADER_DSA))
                || ByteArrayUtil.ByteArrayStartsWith(keyFile, Encoding.ASCII.GetBytes(PrivateKeyFileHeader.SSH2_OPENSSH_HEADER_ECDSA))
                || ByteArrayUtil.ByteArrayStartsWith(keyFile, Encoding.ASCII.GetBytes(PrivateKeyFileHeader.SSH2_OPENSSH_HEADER_OPENSSH)))
                return PrivateKeyFileFormat.SSH2_OPENSSH;

            if (ByteArrayUtil.ByteArrayStartsWith(keyFile, Encoding.ASCII.GetBytes(PrivateKeyFileHeader.SSH2_SSHCOM_HEADER)))
                return PrivateKeyFileFormat.SSH2_SSHCOM;

            if (ByteArrayUtil.ByteArrayStartsWith(keyFile, Encoding.ASCII.GetBytes(PrivateKeyFileHeader.SSH2_PUTTY_HEADER_1))
                || ByteArrayUtil.ByteArrayStartsWith(keyFile, Encoding.ASCII.GetBytes(PrivateKeyFileHeader.SSH2_PUTTY_HEADER_2)))
                return PrivateKeyFileFormat.SSH2_PUTTY;

            return PrivateKeyFileFormat.UNKNOWN;
        }

        /// <summary>
        /// Read SSH1 private key parameters.
        /// </summary>
        /// <param name="passphrase">passphrase for decrypt the key file</param>
        /// <param name="modulus">private key parameter is set</param>
        /// <param name="publicExponent">private key parameter is set</param>
        /// <param name="privateExponent">private key parameter is set</param>
        /// <param name="primeP">private key parameter is set</param>
        /// <param name="primeQ">private key parameter is set</param>
        /// <param name="crtCoefficient">private key parameter is set</param>
        /// <param name="comment">comment</param>
        /// <exception cref="SSHException">failed to parse</exception>
        public void LoadSSH1PrivateKey(
            string passphrase,
            out BigInteger modulus,
            out BigInteger publicExponent,
            out BigInteger privateExponent,
            out BigInteger primeP,
            out BigInteger primeQ,
            out BigInteger crtCoefficient,
            out string comment) {

            PrivateKeyFileFormat format = ProbeFormat();

            ISSH1PrivateKeyLoader loader;
            if (format == PrivateKeyFileFormat.SSH1)
                loader = new SSH1PrivateKeyLoader(keyFile, keyFilePath);
            else
                throw new SSHException(Strings.GetString("UnsupportedPrivateKeyFormat"));

            loader.Load(passphrase, out modulus, out publicExponent, out privateExponent, out primeP, out primeQ, out crtCoefficient, out comment);
        }

        /// <summary>
        /// Read SSH2 private key parameters.
        /// </summary>
        /// <param name="passphrase">passphrase for decrypt the key file</param>
        /// <param name="keyPair">key pair is set</param>
        /// <param name="comment">comment is set. empty if it didn't exist</param>
        /// <exception cref="SSHException">failed to parse</exception>
        public void LoadSSH2PrivateKey(
            string passphrase,
            out KeyPair keyPair,
            out string comment) {

            PrivateKeyFileFormat format = ProbeFormat();

            ISSH2PrivateKeyLoader loader;
            if (format == PrivateKeyFileFormat.SSH2_SSHCOM)
                loader = new SSHComPrivateKeyLoader(keyFile, keyFilePath);
            else if (format == PrivateKeyFileFormat.SSH2_OPENSSH)
                loader = new OpenSSHPrivateKeyLoader(keyFile, keyFilePath);
            else if (format == PrivateKeyFileFormat.SSH2_PUTTY)
                loader = new PuTTYPrivateKeyLoader(keyFile, keyFilePath);
            else
                throw new SSHException(Strings.GetString("UnsupportedPrivateKeyFormat"));

            loader.Load(passphrase, out keyPair, out comment);
        }
    }


}
