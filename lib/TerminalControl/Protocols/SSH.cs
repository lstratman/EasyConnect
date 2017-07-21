/*
 * Copyright 2004,2006 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *
 * $Id: SSH.cs,v 1.3 2011/10/27 23:21:57 kzmi Exp $
 */
using System;
using System.Text;

using Granados;
using Granados.PKI;
using System.Collections.Generic;

namespace Poderosa.Protocols {

    internal class LocalSSHUtil {

        /// <summary>
        /// Parse array of <see cref="CipherAlgorithm"/>.
        /// </summary>
        /// <remarks>
        /// Unknown algorithms are just ignored.
        /// </remarks>
        /// <param name="t">array of strings to convert</param>
        /// <returns>array of <see cref="CipherAlgorithm"/></returns>
        public static CipherAlgorithm[] ParseCipherAlgorithm(string[] t) {
            var list = new List<CipherAlgorithm>(t.Length);
            foreach (string a in t) {
                CipherAlgorithm algorithm;
                if (Enum.TryParse(a, out algorithm) && Enum.IsDefined(typeof(CipherAlgorithm), algorithm)) {
                    list.Add(algorithm);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Append missing algorithm to the list of <see cref="CipherAlgorithm"/>.
        /// </summary>
        /// <param name="algorithms">list of <see cref="CipherAlgorithm"/></param>
        /// <returns>new array of of <see cref="CipherAlgorithm"/></returns>
        public static CipherAlgorithm[] AppendMissingCipherAlgorithm(CipherAlgorithm[] algorithms) {
            var enumSet = new HashSet<CipherAlgorithm>((CipherAlgorithm[])Enum.GetValues(typeof(CipherAlgorithm)));
            foreach (var a in algorithms) {
                enumSet.Remove(a);
            }
            var listToAppend = new List<CipherAlgorithm>(enumSet);
            listToAppend.Sort((a1, a2) => a2.GetDefaultPriority().CompareTo(a1.GetDefaultPriority()));    // descending order

            var list = new List<CipherAlgorithm>(algorithms.Length + enumSet.Count);
            list.AddRange(algorithms);
            list.AddRange(listToAppend);
            return list.ToArray();
        }

        /// <summary>
        /// Parse array of <see cref="PublicKeyAlgorithm"/>.
        /// </summary>
        /// <remarks>
        /// Unknown algorithms are just ignored.
        /// </remarks>
        /// <param name="t">array of strings to convert</param>
        /// <returns>array of <see cref="PublicKeyAlgorithm"/></returns>
        public static PublicKeyAlgorithm[] ParsePublicKeyAlgorithm(string[] t) {
            var list = new List<PublicKeyAlgorithm>(t.Length);
            foreach (string a in t) {
                PublicKeyAlgorithm algorithm;
                if (Enum.TryParse(a, out algorithm) && Enum.IsDefined(typeof(PublicKeyAlgorithm), algorithm)) {
                    list.Add(algorithm);
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// Append missing algorithm to the list of <see cref="PublicKeyAlgorithm"/>.
        /// </summary>
        /// <param name="algorithms">list of <see cref="PublicKeyAlgorithm"/></param>
        /// <returns>new array of of <see cref="PublicKeyAlgorithm"/></returns>
        public static PublicKeyAlgorithm[] AppendMissingPublicKeyAlgorithm(PublicKeyAlgorithm[] algorithms) {
            var enumSet = new HashSet<PublicKeyAlgorithm>((PublicKeyAlgorithm[])Enum.GetValues(typeof(PublicKeyAlgorithm)));
            foreach (var a in algorithms) {
                enumSet.Remove(a);
            }
            var listToAppend = new List<PublicKeyAlgorithm>(enumSet);
            listToAppend.Sort((a1, a2) => a2.GetDefaultPriority().CompareTo(a1.GetDefaultPriority()));    // descending order

            var list = new List<PublicKeyAlgorithm>(algorithms.Length + listToAppend.Count);
            list.AddRange(algorithms);
            list.AddRange(listToAppend);
            return list.ToArray();
        }

        public static string SimpleEncrypt(string plain) {
            byte[] t = Encoding.ASCII.GetBytes(plain);
            if ((t.Length % 16) != 0) {
                byte[] t2 = new byte[t.Length + (16 - (t.Length % 16))];
                Array.Copy(t, 0, t2, 0, t.Length);
                for (int i = t.Length + 1; i < t2.Length; i++) //残りはダミー
                    t2[i] = t[i % t.Length];
                t = t2;
            }

            byte[] key = Encoding.ASCII.GetBytes("- BOBO VIERI 32-");
            Granados.Algorithms.Rijndael rijndael = new Granados.Algorithms.Rijndael();
            rijndael.InitializeKey(key);

            byte[] e = new byte[t.Length];
            rijndael.encryptCBC(t, 0, t.Length, e, 0);

            return Encoding.ASCII.GetString(Granados.Util.Base64.Encode(e));
        }
        public static string SimpleDecrypt(string enc) {
            byte[] t = Granados.Util.Base64.Decode(Encoding.ASCII.GetBytes(enc));
            byte[] key = Encoding.ASCII.GetBytes("- BOBO VIERI 32-");
            Granados.Algorithms.Rijndael rijndael = new Granados.Algorithms.Rijndael();
            rijndael.InitializeKey(key);

            byte[] d = new byte[t.Length];
            rijndael.decryptCBC(t, 0, t.Length, d, 0);

            return Encoding.ASCII.GetString(d); //パディングがあってもNULL文字になるので除去されるはず
        }
    }

    //鍵のチェック関係
    /// <summary>
    /// <ja>
    /// 鍵のチェック状況を示します。
    /// </ja>
    /// </summary>
    internal enum KeyCheckResult {
        /// <summary>
        /// <ja>正しい鍵</ja>
        /// <en>Correct key.</en>
        /// </summary>
        OK,
        /// <summary>
        /// <ja>保持している内容と異なる鍵</ja>
        /// <en>Key is different from held one.</en>
        /// </summary>
        Different,
        /// <summary>
        /// <ja>鍵情報が見つからない</ja>
        /// <en>Key is not exist.</en>
        /// </summary>
        NotExists
    }
}
