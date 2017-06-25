// Copyright (c) 2017 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

using Granados.Crypto;
using Granados.IO.SSH2;
using Granados.Mono.Math;
using Granados.Util;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;

namespace Granados.PKI {

    /// <summary>
    /// Edwards Curve parameters
    /// </summary>
    public abstract class EdwardsCurve {

        /// <summary>
        /// Curve name
        /// </summary>
        public abstract string CurveName {
            get;
        }

        /// <summary>
        /// Public key algorithm that this curve is used.
        /// </summary>
        public abstract PublicKeyAlgorithm PublicKeyAlgorithm {
            get;
        }

        /// <summary>
        /// Sign
        /// </summary>
        /// <param name="privateKey">private key</param>
        /// <param name="data">data to be signed</param>
        /// <param name="signature">signature returned</param>
        /// <returns>true if the signing succeeded, otherwise false.</returns>
        public abstract bool Sign(byte[] privateKey, byte[] data, out byte[] signature);

        /// <summary>
        /// Verify
        /// </summary>
        /// <param name="publicKey">public key</param>
        /// <param name="signature">signature to verify</param>
        /// <param name="data">data</param>
        /// <returns>true if the verification passed, otherwise false.</returns>
        public abstract bool Verify(byte[] publicKey, byte[] signature, byte[] data);

        /// <summary>
        /// Checks validity of the key pair.
        /// </summary>
        /// <param name="privateKey">private key</param>
        /// <param name="publicKey">public key</param>
        /// <returns>true if the key pair was valid.</returns>
        public abstract bool IsValidKeyPair(byte[] privateKey, byte[] publicKey);

        //-----------------------------------------------------------------------
        // Curve definitions
        //-----------------------------------------------------------------------

        private static readonly ConcurrentDictionary<string, EdwardsCurve> _curveDict =
                                            new ConcurrentDictionary<string, EdwardsCurve>();

        public static EdwardsCurve FindByName(string name) {
            EdwardsCurve curve;
            if (_curveDict.TryGetValue(name, out curve)) {
                return curve;
            }

            switch (name) {
                case "edwards25519":
                    curve = new CurveEd25519();
                    break;
                default:
                    // unknown curve
                    return null;
            }

            _curveDict.TryAdd(name, curve);
            return curve;
        }

        public static EdwardsCurve FindByAlgorithm(PublicKeyAlgorithm algorithm) {
            switch (algorithm) {
                case PublicKeyAlgorithm.ED25519:
                    return FindByName("edwards25519");
                default:
                    return null;
            }
        }
    }
    
    /// <summary>
    /// Ed25519 curve parameters
    /// </summary>
    public class CurveEd25519 : EdwardsCurve {
        private const int _b = 256;       // number of bits
        private const int _c = 3;         // base 2 logarithm of cofactor; cofactor = 8
        private const int _n = 254;       // c <= n < b

        private readonly BigInteger _p;   // prime number defining the finite field
        private readonly BigInteger _d;   // element in the finite field GF(p)
        private readonly BigInteger _bx;  // x coordinate of a point on the curve
        private readonly BigInteger _by;  // y coordinate of a point on the curve
        private readonly BigInteger _l;   // odd prime

        private readonly string _curveName;
        private readonly PublicKeyAlgorithm _publicKeyAlgorithm;

        /// <summary>
        /// A point that was represented as the extended coordinates.
        /// </summary>
        private class Ed25519Point {
            // based on draft-irtf-cfrg-eddsa-08 "Edwards-curve Digital Signature Algorithm (EdDSA)"

            public readonly BigInteger X;
            public readonly BigInteger Y;
            public readonly BigInteger Z;
            public readonly BigInteger T;

            public Ed25519Point(BigInteger x, BigInteger y, BigInteger z, BigInteger t) {
                X = x;
                Y = y;
                Z = z;
                T = t;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CurveEd25519() {
            _curveName = "edwards25519";
            _publicKeyAlgorithm = PublicKeyAlgorithm.ED25519;
            _p = new BigInteger(BigIntegerConverter.ParseHex(
                    // 2^255 - 19
                    "7fffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffed"
                ));
            _d = BigInteger.Parse(
                    "37095705934669439343138083508754565189542113879843219016388785533085940283555"
                );
            _bx = BigInteger.Parse(
                    "15112221349535400772501151409588531511454012693041857206046113283949847762202"
                );
            _by = BigInteger.Parse(
                    "46316835694926478169428394003475163141307993866256225615783033603165251855960"
                );
            _l = new BigInteger(BigIntegerConverter.ParseHex(
                    // 2^252 + 0x14def9dea2f79cd65812631a5cf5d3ed
                    "1000000000000000000000000000000014def9dea2f79cd65812631a5cf5d3ed"
                ));
        }

        /// <summary>
        /// Curve name
        /// </summary>
        public override string CurveName {
            get {
                return _curveName;
            }
        }

        /// <summary>
        /// Public key algorithm that this curve is used.
        /// </summary>
        public override PublicKeyAlgorithm PublicKeyAlgorithm {
            get {
                return _publicKeyAlgorithm;
            }
        }

        /// <summary>
        /// Checks validity of the key pair.
        /// </summary>
        /// <param name="privateKey">private key</param>
        /// <param name="publicKey">public key</param>
        /// <returns>true if the key pair was valid.</returns>
        public override bool IsValidKeyPair(byte[] privateKey, byte[] publicKey) {
            if (privateKey.Length != 32) {
                return false;
            }
            if (publicKey.Length != 32) {
                return false;
            }

            using (var sha512 = new SHA512CryptoServiceProvider()) {
                byte[] hash;
                hash = sha512.ComputeHash(privateKey);

                byte[] sdata = new byte[32];
                Buffer.BlockCopy(hash, 0, sdata, 0, 32);
                sdata[0] &= (byte)((0xff << _c) & 0xff);  // clear lower bits
                sdata[31] &= (byte)((1 << (_n % 8)) - 1); // clear higher bits
                sdata[31] |= (byte)(1 << (_n % 8)); // set top bit
                Array.Reverse(sdata);   // to big endian
                var s = new BigInteger(sdata);

                var G = GetBasePoint();
                byte[] A;
                if (!EncodePoint(PointMul(s, G), out A)) {
                    return false;
                }

                if (publicKey.Length != A.Length) {
                    return false;
                }
                for (int i = 0; i < A.Length; ++i) {
                    if (publicKey[i] != A[i]) {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Sign
        /// </summary>
        /// <param name="privateKey">private key</param>
        /// <param name="data">data to be signed</param>
        /// <param name="signature">signature returned</param>
        /// <returns>true if the signing succeeded, otherwise false.</returns>
        public override bool Sign(byte[] privateKey, byte[] data, out byte[] signature) {
            // based on draft-irtf-cfrg-eddsa-08 "Edwards-curve Digital Signature Algorithm (EdDSA)"

            if (privateKey.Length != 32) {
                signature = null;
                return false;
            }

            using (var sha512 = new SHA512CryptoServiceProvider()) {
                byte[] hash;
                hash = sha512.ComputeHash(privateKey);

                byte[] sdata = new byte[32];
                Buffer.BlockCopy(hash, 0, sdata, 0, 32);
                sdata[0] &= (byte)((0xff << _c) & 0xff);  // clear lower bits
                sdata[31] &= (byte)((1 << (_n % 8)) - 1); // clear higher bits
                sdata[31] |= (byte)(1 << (_n % 8)); // set top bit
                Array.Reverse(sdata);   // to big endian
                var s = new BigInteger(sdata);

                var G = GetBasePoint();
                byte[] A;
                if (!EncodePoint(PointMul(s, G), out A)) {
                    signature = null;
                    return false;
                }

                sha512.Initialize();
                sha512.TransformBlock(hash, 32, 32, null, 0);
                sha512.TransformFinalBlock(data, 0, data.Length);
                byte[] rdata = sha512.Hash;
                Array.Reverse(rdata);   // to big endian
                var r = new BigInteger(rdata) % this._l;

                byte[] R;
                if (!EncodePoint(PointMul(r, G), out R)) {
                    signature = null;
                    return false;
                }

                sha512.Initialize();
                sha512.TransformBlock(R, 0, R.Length, null, 0);
                sha512.TransformBlock(A, 0, A.Length, null, 0);
                sha512.TransformFinalBlock(data, 0, data.Length);
                byte[] kdata = sha512.Hash;
                Array.Reverse(kdata);   // to big endian
                var k = new BigInteger(kdata) % this._l;

                var S = (r + k * s) % this._l;

                byte[] sig = new byte[64];
                Buffer.BlockCopy(R, 0, sig, 0, R.Length);   // copy 32 bytes
                byte[] wS = S.GetBytes();
                Array.Reverse(wS);  // to little endian
                Buffer.BlockCopy(wS, 0, sig, 32, wS.Length);
                signature = sig;
                return true;
            }
        }

        /// <summary>
        /// Verify
        /// </summary>
        /// <param name="publicKey">public key</param>
        /// <param name="signature">signature to verify</param>
        /// <param name="data">data</param>
        /// <returns>true if the verification passed, otherwise false.</returns>
        public override bool Verify(byte[] publicKey, byte[] signature, byte[] data) {
            // based on draft-irtf-cfrg-eddsa-08 "Edwards-curve Digital Signature Algorithm (EdDSA)"

            Ed25519Point A;
            if (!DecodePoint(publicKey, out A)) {
                return false;
            }
            if (signature.Length != 64) {
                return false;
            }
            byte[] Rs = new byte[32];
            Buffer.BlockCopy(signature, 0, Rs, 0, 32);
            Ed25519Point R;
            if (!DecodePoint(Rs, out R)) {
                return false;
            }
            byte[] Ss = new byte[32];
            Buffer.BlockCopy(signature, 32, Ss, 0, 32);
            Array.Reverse(Ss);  // to big endian
            var S = new BigInteger(Ss);
            if (S >= this._l) {
                return false;
            }

            BigInteger k;
            using (var sha512 = new SHA512CryptoServiceProvider()) {
                sha512.TransformBlock(Rs, 0, Rs.Length, null, 0);
                sha512.TransformBlock(publicKey, 0, publicKey.Length, null, 0);
                sha512.TransformFinalBlock(data, 0, data.Length);
                byte[] kdata = sha512.Hash;
                Array.Reverse(kdata);   // to big endian
                k = new BigInteger(kdata) % this._l;
            }

            var p1 = PointMul(S, GetBasePoint());
            var p2 = PointAdd(R, PointMul(k, A));

            // check equality of points
            // x1 / z1 == x2 / z2  <==>  x1 * z2 == x2 * z1
            if ((p1.X * p2.Z) % this._p != (p2.X * p1.Z) % this._p) {
                return false;
            }
            // y1 / z1 == y2 / z2  <==>  y1 * z2 == y2 * z1
            if ((p1.Y * p2.Z) % this._p != (p2.Y * p1.Z) % this._p) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Encodes a point to the octet string.
        /// </summary>
        /// <param name="q">a point</param>
        /// <param name="data">octet string returned</param>
        /// <returns>true if encoding succeeded, otherwise false.</returns>
        private bool EncodePoint(Ed25519Point q, out byte[] data) {
            // based on draft-irtf-cfrg-eddsa-08 "Edwards-curve Digital Signature Algorithm (EdDSA)"

            const int DATALEN = 32;
            var zinv = q.Z.ModInverse(this._p);
            var x = q.X * zinv % this._p;
            var y = q.Y * zinv % this._p;
            byte[] yBytes = y.GetBytes();
            byte[] w = new byte[DATALEN];
            Buffer.BlockCopy(yBytes, 0, w, w.Length - yBytes.Length, yBytes.Length);
            Array.Reverse(w);   // to little endian
            if (x % 2u != 0) {
                w[DATALEN - 1] |= 0x80;   // set MSB
            }
            data = w;
            return true;
        }

        /// <summary>
        /// Decode the octet string to a point.
        /// </summary>
        /// <param name="data">point data</param>
        /// <param name="point"></param>
        /// <returns>true if decoding succeeded, otherwise false.</returns>
        private bool DecodePoint(byte[] data, out Ed25519Point point) {
            // based on draft-irtf-cfrg-eddsa-08 "Edwards-curve Digital Signature Algorithm (EdDSA)"

            const int DATALEN = 32;
            if (data.Length != DATALEN) {
                point = null;
                return false;
            }
            // note that the byte order of the data is little endian
            byte[] w = (byte[])data.Clone();
            int sign = w[DATALEN - 1] >> 7; // MSB represents the sign of x
            w[DATALEN - 1] &= 0x7f;
            Array.Reverse(w);   // to big endian
            var y = new BigInteger(w);
            if (y >= _p) {
                point = null;
                return false;
            }
            // recover x
            var y2 = y * y;
            var x2 = (((this._p + y2 - 1) % this._p) * (this._d * y2 + 1).ModInverse(this._p)) % this._p;
            BigInteger x;
            if (x2 == 0) {
                if (sign != 0) {
                    point = null;
                    return false;
                }
                else {
                    x = 0;
                }
            }
            else {
                x = x2.ModPow((this._p + 3) >> 3, this._p);
                if ((x * x) % this._p != x2) {
                    // square root of -1
                    x = x * (new BigInteger(2)).ModPow((this._p - 1) >> 2, this._p) % this._p;
                    if ((x * x) % this._p != x2) {
                        point = null;
                        return false;
                    }
                }
                if ((x % 2u) != (uint)sign) {
                   x = _p - x;
                }
            }

            point = new Ed25519Point(x, y, 1, (x * y) % this._p);
            return true;
        }

        /// <summary>
        /// Returns a base point.
        /// </summary>
        /// <returns>base point</returns>
        private Ed25519Point GetBasePoint() {
            // based on draft-irtf-cfrg-eddsa-08 "Edwards-curve Digital Signature Algorithm (EdDSA)"
            return new Ed25519Point(this._bx, this._by, 1, this._bx * this._by % this._p);
        }

        /// <summary>
        /// Point addition
        /// </summary>
        /// <param name="p1">point</param>
        /// <param name="p2">point</param>
        /// <returns>result</returns>
        private Ed25519Point PointAdd(Ed25519Point p1, Ed25519Point p2) {
            // based on draft-irtf-cfrg-eddsa-08 "Edwards-curve Digital Signature Algorithm (EdDSA)"
            var A = (this._p + p1.Y - p1.X) * (this._p + p2.Y - p2.X) % this._p;
            var B = (p1.Y + p1.X) * (p2.Y + p2.X) % this._p;
            var C = (p1.T << 1) * p2.T * this._d % this._p;
            var D = (p1.Z << 1) * p2.Z % this._p;
            var E = this._p + B - A;
            var F = this._p + D - C;
            var G = D + C;
            var H = B + A;
            var X3 = E * F % this._p;
            var Y3 = G * H % this._p;
            var Z3 = F * G % this._p;
            var T3 = E * H % this._p;
            return new Ed25519Point(X3, Y3, Z3, T3);
        }

        /// <summary>
        /// Point multiplication
        /// </summary>
        /// <param name="k">scalar value</param>
        /// <param name="p">point</param>
        /// <returns>result</returns>
        private Ed25519Point PointMul(BigInteger k, Ed25519Point p) {
            Ed25519Point mp = p;
            Ed25519Point q = new Ed25519Point(0, 1, 1, 0);  // Neutral element
            int kBitCount = k.BitCount();
            byte[] kBytes = k.GetBytes();
            int kOffset = kBytes.Length - 1;
            for (int i = 0; i < kBitCount; ++i) {
                if (i > 0) {
                    mp = PointAdd(mp, mp);
                }
                if ((kBytes[kOffset - i / 8] & (byte)(1 << (i % 8))) != 0) {
                    q = PointAdd(q, mp);
                }
            }
            return q;
        }
 
#if DEBUG
        // Test of signing and verifying using test vectors
        // http://ed25519.cr.yp.to/python/sign.input
        internal void Test() {
            using (var reader = new System.IO.StreamReader(@"sign.input")) {
                int skip = 0;
                int count = 0;
                while (true) {
                    string line = reader.ReadLine();
                    if (line == null) {
                        break;
                    }
                    count++;
                    if (count <= skip) {
                        continue;
                    }
                    System.Diagnostics.Debug.WriteLine("Line {0}", count);
                    
                    string[] w = line.Split(':');
                    byte[][] b = w.Select(s => BigIntegerConverter.ParseHex(s)).ToArray();
                    byte[] privateKey = new byte[32];
                    Buffer.BlockCopy(b[0], 0, privateKey, 0, 32);
                    byte[] publicKey = b[1];
                    byte[] message = b[2];
                    byte[] signature = new byte[64];
                    Buffer.BlockCopy(b[3], 0, signature, 0, 64);

                    byte[] sig;
                    if (!Sign(privateKey, message, out sig)) {
                        throw new Exception("signing failed");
                    }
                    if (sig.Length != signature.Length) {
                        throw new Exception("invalid sign length");
                    }
                    for (int i = 0; i < signature.Length; ++i) {
                        if (sig[i] != signature[i]) {
                            throw new Exception("signs doesn't match");
                        }
                    }
                    if (!Verify(publicKey, signature, message)) {
                        throw new Exception("verification failed");
                    }
                }
            }
        }
#endif
    }

    /// <summary>
    /// EDDSA public key
    /// </summary>
    public class EDDSAPublicKey : PublicKey {

        private readonly EdwardsCurve _curve;
        private readonly byte[] _publicKey;

        /// <summary>
        /// Octet string of the public key.
        /// </summary>
        public byte[] Bytes {
            get {
                return _publicKey;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="curve">curve</param>
        /// <param name="publicKey">public key</param>
        public EDDSAPublicKey(EdwardsCurve curve, byte[] publicKey) {
            _curve = curve;
            _publicKey = publicKey;
        }

        public override PublicKeyAlgorithm Algorithm {
            get {
                return _curve.PublicKeyAlgorithm;
            }
        }

        public override void WriteTo(IKeyWriter writer) {
            writer.WriteByteString(_publicKey);
        }

        internal static EDDSAPublicKey ReadFrom(PublicKeyAlgorithm algorithm, SSH2DataReader reader) {
            EdwardsCurve curve = EdwardsCurve.FindByAlgorithm(algorithm);
            if (curve == null) {
                throw new SSHException(Strings.GetString("UnsupportedEllipticCurve"));
            }

            byte[] q = reader.ReadByteString();
            return new EDDSAPublicKey(curve, q);
        }

        /// <summary>
        /// Verifies signature with this public key
        /// </summary>
        /// <param name="signature">signature</param>
        /// <param name="data">data</param>
        /// <exception cref="VerifyException">verification failure</exception>
        public void Verify(byte[] signature, byte[] data) {
            if (!_curve.Verify(_publicKey, signature, data)) {
                throw new VerifyException("verification failed");
            }
        }
    }

    /// <summary>
    /// EDDSA key pair
    /// </summary>
    public class EDDSAKeyPair : KeyPair, ISigner, IVerifier {

        private readonly EdwardsCurve _curve;
        private readonly EDDSAPublicKey _publicKey;
        private readonly byte[] _privateKey;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="curve">curve</param>
        /// <param name="publicKey">public key</param>
        /// <param name="privateKey">private key</param>
        public EDDSAKeyPair(EdwardsCurve curve, EDDSAPublicKey publicKey, byte[] privateKey) {
            _curve = curve;
            _publicKey = publicKey;
            _privateKey = privateKey;
        }

        public override PublicKey PublicKey {
            get {
                return _publicKey;
            }
        }

        public override PublicKeyAlgorithm Algorithm {
            get {
                return _curve.PublicKeyAlgorithm;
            }
        }

        /// <summary>
        /// Implements <see cref="ISigner"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public byte[] Sign(byte[] data) {
            byte[] signature;
            if (!_curve.Sign(this._privateKey, data, out signature)) {
                throw new SSHException("signing failed");
            }
            return signature;
        }

        /// <summary>
        /// Implements <see cref="IVerifier"/>.
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="data"></param>
        public void Verify(byte[] signature, byte[] data) {
            _publicKey.Verify(signature, data);
        }

        /// <summary>
        /// Check consistency of public key and private key.
        /// </summary>
        /// <returns>true if keys are valid.</returns>
        public bool CheckKeyConsistency() {
            return _curve.IsValidKeyPair(_privateKey, _publicKey.Bytes);
        }
    }

}
