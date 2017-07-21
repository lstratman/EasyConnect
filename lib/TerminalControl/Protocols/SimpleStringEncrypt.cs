/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SimpleStringEncrypt.cs,v 1.2 2011/10/27 23:21:57 kzmi Exp $
 */
using Poderosa;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Poderosa.Protocols {


    /// <summary>
    /// Simple string encyption with constant key.
    /// </summary>
    internal class SimpleStringEncrypt {

        // Method : DES
        //   Mode : CBC
        //     IV : 91 7c 0b f1 7e 3e 7e 33
        //    Key : 15 b1 84 59 15 60 29 32

        private const string CIV = "kXwL8X4+fjM=";
        private const string CKEY = "FbGEWRVgKTI=";

        private SymmetricAlgorithm mCSP = new DESCryptoServiceProvider();

        /// <summary>
        /// Decrypt string.
        /// </summary>
        /// <param name="s">Base64-encoded encrypted data</param>
        /// <returns>decrypted text or null if failed</returns>
        public string DecryptString(string s) {
            if (s == null)
                return null;
            if (s.Length == 0)
                return null;

            using (ICryptoTransform transform = this.mCSP.CreateDecryptor(Convert.FromBase64String(CKEY), Convert.FromBase64String(CIV))) {
                try {
                    byte[] buffer = Convert.FromBase64String(s);
                    using (MemoryStream stream = new MemoryStream()) {
                        using (CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write)) {
                            stream2.Write(buffer, 0, buffer.Length);
                            stream2.FlushFinalBlock();
                            stream2.Close();
                            return Encoding.UTF8.GetString(stream.ToArray());
                        }
                    }
                }
                catch (Exception exception) {
                    RuntimeUtil.SilentReportException(exception);
                    return null;
                }
            }
        }

        /// <summary>
        /// Encrypt string.
        /// </summary>
        /// <param name="text">Text to encrypt</param>
        /// <returns>Base64-encoded encrypted data or null if failed</returns>
        public string EncryptString(string text) {
            if (text == null)
                return null;

            try {
                using (ICryptoTransform transform = this.mCSP.CreateEncryptor(Convert.FromBase64String(CKEY), Convert.FromBase64String(CIV))) {
                    byte[] bytes = Encoding.UTF8.GetBytes(text);
                    using (MemoryStream stream = new MemoryStream()) {
                        using (CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write)) {
                            stream2.Write(bytes, 0, bytes.Length);
                            stream2.FlushFinalBlock();
                            stream2.Close();
                            return Convert.ToBase64String(stream.ToArray());
                        }
                    }
                }
            }
            catch (Exception exception) {
                RuntimeUtil.SilentReportException(exception);
                return null;
            }
        }
    }
}
