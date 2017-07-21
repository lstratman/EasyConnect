/*
 Copyright (c) 2005 Poderosa Project, All Rights Reserved.
 This file is a part of the Granados SSH Client Library that is subject to
 the license included in the distributed package.
 You may not use this file except in compliance with the license.

  I implemented this algorithm with reference to following products though the algorithm is known publicly.
    * MindTerm ( AppGate Network Security )
 
 $Id: RSA.cs,v 1.5 2011/11/08 12:24:05 kzmi Exp $
*/
using System;
using System.Diagnostics;
using System.Security.Cryptography;

using Granados.Util;
using Granados.Mono.Math;
using Granados.IO.SSH2;

namespace Granados.PKI {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class RSAKeyPair : KeyPair, ISigner, IVerifier {

        private RSAPublicKey _publickey;
        private BigInteger _d;
        private BigInteger _u;
        private BigInteger _p;
        private BigInteger _q;

        public RSAKeyPair(BigInteger e, BigInteger d, BigInteger n, BigInteger u, BigInteger p, BigInteger q) {
            _publickey = new RSAPublicKey(e, n);
            _d = d;
            _u = u;
            _p = p;
            _q = q;
        }
        public BigInteger D {
            get {
                return _d;
            }
        }
        public BigInteger U {
            get {
                return _u;
            }
        }
        public BigInteger P {
            get {
                return _p;
            }
        }
        public BigInteger Q {
            get {
                return _q;
            }
        }
        public override PublicKeyAlgorithm Algorithm {
            get {
                return PublicKeyAlgorithm.RSA;
            }
        }
        public byte[] Sign(byte[] data) {
            BigInteger pe = PrimeExponent(_d, _p);
            BigInteger qe = PrimeExponent(_d, _q);

            BigInteger result = SignCore(new BigInteger(data), pe, qe);

            return result.GetBytes();
        }

        public void Verify(byte[] data, byte[] expected) {
            _publickey.Verify(data, expected);
        }

        public byte[] SignWithSHA1(byte[] data) {
            byte[] hash = new SHA1CryptoServiceProvider().ComputeHash(data);

            byte[] buf = new byte[hash.Length + PKIUtil.SHA1_ASN_ID.Length];
            Array.Copy(PKIUtil.SHA1_ASN_ID, 0, buf, 0, PKIUtil.SHA1_ASN_ID.Length);
            Array.Copy(hash, 0, buf, PKIUtil.SHA1_ASN_ID.Length, hash.Length);

            BigInteger x = new BigInteger(buf);
            //Debug.WriteLine(x.ToString(16));
            int padLen = (_publickey._n.BitCount() + 7) / 8;

            BigInteger xx = RSAUtil.PKCS1PadType1(x.GetBytes(), padLen);
            byte[] result = Sign(xx.GetBytes());
            return result;
        }

        private BigInteger SignCore(BigInteger input, BigInteger pe, BigInteger qe) {
            BigInteger p2 = (input % _p).ModPow(pe, _p);
            BigInteger q2 = (input % _q).ModPow(qe, _q);

            if (p2 == q2)
                return p2;

            BigInteger k;
            if (q2 > p2) {
                k = (q2 - p2) % _q;
            }
            else {
                // add multiple of _q greater than _p
                BigInteger d = _q + (_p / _q) * _q;
                k = (d + q2 - p2) % _q;
            }
            k = (k * _u) % _q;

            BigInteger result = k * _p + p2;

            return result;
        }

        public override PublicKey PublicKey {
            get {
                return _publickey;
            }
        }

        private static BigInteger PrimeExponent(BigInteger privateExponent, BigInteger prime) {
            BigInteger pe = prime - new BigInteger(1);
            return privateExponent % pe;
        }

        public RSAParameters ToRSAParameters() {
            RSAParameters p = new RSAParameters();
            p.D = _d.GetBytes();
            p.Exponent = _publickey.Exponent.GetBytes();
            p.Modulus = _publickey.Modulus.GetBytes();
            p.P = _p.GetBytes();
            p.Q = _q.GetBytes();
            BigInteger pe = PrimeExponent(_d, _p);
            BigInteger qe = PrimeExponent(_d, _q);
            p.DP = pe.GetBytes();
            p.DQ = qe.GetBytes();
            p.InverseQ = _u.GetBytes();
            return p;
        }

        public static RSAKeyPair GenerateNew(int bits, Rng rnd) {
            BigInteger one = new BigInteger(1);
            BigInteger p = null;
            BigInteger q = null;
            BigInteger t = null;
            BigInteger p_1 = null;
            BigInteger q_1 = null;
            BigInteger phi = null;
            BigInteger G = null;
            BigInteger F = null;
            BigInteger e = null;
            BigInteger d = null;
            BigInteger u = null;
            BigInteger n = null;

            bool finished = false;

            while (!finished) {
                p = BigInteger.GeneratePseudoPrime(bits / 2);
                q = BigInteger.GeneratePseudoPrime(bits - (bits / 2));

                if (p == 0) {
                    continue;
                }
                else if (q < p) {
                    t = q;
                    q = p;
                    p = t;
                }

                t = p.GCD(q);
                if (t != one) {
                    continue;
                }

                p_1 = p - one;
                q_1 = q - one;
                phi = p_1 * q_1;
                G = p_1.GCD(q_1);
                F = phi / G;

                e = one << 5;
                e = e - one;
                do {
                    e = e + (one + one);
                    t = e.GCD(phi);
                } while (t != one);

                // !!! d = e.modInverse(F);
                d = e.ModInverse(phi);
                n = p * q;
                u = p.ModInverse(q);

                finished = true;
            }

            return new RSAKeyPair(e, d, n, u, p, q);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class RSAPublicKey : PublicKey, IVerifier {


        internal BigInteger _e;
        internal BigInteger _n;


        public RSAPublicKey(BigInteger exp, BigInteger mod) {
            _e = exp;
            _n = mod;
        }
        public override PublicKeyAlgorithm Algorithm {
            get {
                return PublicKeyAlgorithm.RSA;
            }
        }
        public BigInteger Exponent {
            get {
                return _e;
            }
        }
        public BigInteger Modulus {
            get {
                return _n;
            }
        }

        public void Verify(byte[] data, byte[] expected) {
            if (VerifyBI(data) != new BigInteger(expected))
                throw new VerifyException("Failed to verify");
        }
        private BigInteger VerifyBI(byte[] data) {
            return new BigInteger(data).ModPow(_e, _n);
        }
        public void VerifyWithSHA1(byte[] data, byte[] expected) {
            BigInteger result = VerifyBI(data);
            byte[] finaldata = RSAUtil.StripPKCS1Pad(result, 1).GetBytes();

            if (finaldata.Length != PKIUtil.SHA1_ASN_ID.Length + expected.Length)
                throw new VerifyException("result is too short");
            else {
                byte[] r = new byte[finaldata.Length];
                Array.Copy(PKIUtil.SHA1_ASN_ID, 0, r, 0, PKIUtil.SHA1_ASN_ID.Length);
                Array.Copy(expected, 0, r, PKIUtil.SHA1_ASN_ID.Length, expected.Length);
                if (!SSHUtil.ByteArrayEqual(r, finaldata)) {
                    throw new VerifyException("failed to verify");
                }
            }
        }

        public override void WriteTo(IKeyWriter writer) {
            writer.WriteBigInteger(_e);
            writer.WriteBigInteger(_n);
        }

        internal static RSAPublicKey ReadFrom(SSH2DataReader reader) {
            BigInteger exp = reader.ReadMPInt();
            BigInteger mod = reader.ReadMPInt();
            return new RSAPublicKey(exp, mod);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class RSAUtil {

        /// <summary>
        /// Make encoded message (EM) as described in PKCS#1
        /// </summary>
        /// <param name="input">input bytes</param>
        /// <param name="len">total byte length of the result</param>
        /// <param name="rng">random number generator</param>
        /// <returns>new bits</returns>
        public static BigInteger PKCS1PadType2(byte[] input, int len, Rng rng) {
            // |00|02|<----- PS ----->|00|<-------- input -------->|
            // |<---------------------- len ---------------------->|

            int padLen = len - input.Length - 3;
            if (padLen < 8) {
                throw new ArgumentException("message too long");
            }

            byte[] pad = new byte[padLen];
            rng.GetBytes(pad);
            for (int i = 0; i < pad.Length; i++) {
                if (pad[i] == 0) {
                    pad[i] = (byte)(1 + rng.GetInt(255));
                }
            }

            byte[] buf = new byte[len];
            buf[1] = 2;
            Buffer.BlockCopy(pad, 0, buf, 2, pad.Length);
            Buffer.BlockCopy(input, 0, buf, padLen + 3, input.Length);

            return new BigInteger(buf);
        }

        /// <summary>
        /// Make encoded message (EM) as described in PKCS#1
        /// </summary>
        /// <param name="input">input bytes</param>
        /// <param name="len">total byte length of the result</param>
        /// <returns>new bits</returns>
        public static BigInteger PKCS1PadType1(byte[] input, int len) {
            // |00|01|<----- PS ----->|00|<-------- input -------->|
            // |<---------------------- len ---------------------->|

            int padLen = len - input.Length - 3;
            if (padLen < 8) {
                throw new ArgumentException("message too long");
            }

            byte[] buf = new byte[len];
            buf[1] = 1;
            for (int i = 0; i < padLen; i++) {
                buf[i + 2] = 0xff;
            }
            Buffer.BlockCopy(input, 0, buf, padLen + 3, input.Length);

            return new BigInteger(buf);
        }

        /// <summary>
        /// Extract an message from the encoded message (EM) described in PKCS#1
        /// </summary>
        /// <param name="input">encoded message bits</param>
        /// <param name="type">type number (1 or 2)</param>
        /// <returns>message bits</returns>
        public static BigInteger StripPKCS1Pad(BigInteger input, int type) {
            byte[] strip = input.GetBytes();
            int stripLen = strip.Length;

            int i = 0;
            while (true) {
                if (i >= stripLen) {
                    throw new ArgumentException("Invalid EM format");
                }
                if (strip[i] != 0) {
                    break;
                }
                i++;
            }

            if (strip[i] != type) {
                throw new ArgumentException(String.Format("Invalid PKCS1 padding {0}", type));
            }
            i++;

            int padLen = 0;
            while (true) {
                if (i >= stripLen) {
                    throw new ArgumentException("Invalid EM format");
                }
                byte b = strip[i];
                if (b == 0) {
                    break;
                }
                if (type == 1 && b != 0xff) {
                    throw new ArgumentException("Invalid PKCS1 padding");
                }
                padLen++;
                i++;
            }

            if (padLen < 8) {
                throw new ArgumentException("Invalid PKCS1 padding");
            }

            i++;    // skip 0x00
            if (i >= stripLen) {
                throw new ArgumentException("Invalid PKCS1 padding, corrupt data");
            }

            byte[] val = new byte[stripLen - i];
            Buffer.BlockCopy(strip, i, val, 0, val.Length);
            return new BigInteger(val);
        }
    }
}
