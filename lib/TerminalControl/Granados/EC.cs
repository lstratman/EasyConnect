// Copyright (c) 2016 Poderosa Project, All Rights Reserved.
// This file is a part of the Granados SSH Client Library that is subject to
// the license included in the distributed package.
// You may not use this file except in compliance with the license.

#define USE_WNAF_POINT_MULTIPLICATION

using Granados.Crypto;
using Granados.IO.SSH2;
using Granados.Mono.Math;
using Granados.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Granados.PKI {

    /// <summary>
    /// Elliptic curve point
    /// </summary>
    /// <remarks>
    /// This class cannot represent a point at infinity.
    /// </remarks>
    public class ECPoint {

        public readonly BigInteger X;
        public readonly BigInteger Y;

        internal bool Validated;

        public ECPoint(ECPoint p)
            : this(p.X, p.Y) {
        }

        public ECPoint(BigInteger x, BigInteger y) {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Parses an elliptic curve point stored in the uncompressed form.
        /// </summary>
        /// <remarks>
        /// Note that this method doesn't check consistency or validity of the values.
        /// </remarks>
        /// <param name="data">octet data</param>
        /// <returns>EC point object if succeeded. otherwise null.</returns>
        public static ECPoint ParseUncompressed(byte[] data) {
            if (IsZero(data)) {
                // point at infinity (not supported)
                return null;
            }

            if (data.Length < 3) {
                // invalid length
                return null;
            }

            if (data[0] != 0x04) {
                // not the uncompressed form
                return null;
            }

            if ((data.Length - 1) % 2 != 0) {
                // invalid length
                return null;
            }

            int elementLen = (data.Length - 1) / 2;
            byte[] dataX = new byte[elementLen];
            Buffer.BlockCopy(data, 1, dataX, 0, elementLen);
            byte[] dataY = new byte[elementLen];
            Buffer.BlockCopy(data, elementLen + 1, dataY, 0, elementLen);
            return new ECPoint(new BigInteger(dataX), new BigInteger(dataY));
        }

        /// <summary>
        /// Parses an elliptic curve point.
        /// </summary>
        /// <param name="data">octet data</param>
        /// <param name="ec">elliptic curve domain parameters</param>
        /// <param name="p">an elliptic curve point object</param>
        /// <returns>true if parsing and validation succeeded</returns>
        public static bool Parse(byte[] data, EllipticCurve ec, out ECPoint p) {
            if (IsZero(data)) {
                // point at infinity
                p = null;
                return false;
            }

            if (data.Length < 2) {
                // invalid length
                p = null;
                return false;
            }

            if (data[0] == 0x04) {
                ECPoint tmp = ParseUncompressed(data);
                if (tmp == null) {
                    p = null;
                    return false;
                }
                if (!ec.ValidatePoint(tmp)) {
                    p = null;
                    return false;
                }
                p = tmp;
                p.Validated = true;
                return true;
            }

            // Compressed form of EC point is not supported.
            // OpenSSL, which is used by OpenSSH for cryptography, disables
            // EC point compression by default due to the patent reason.

            p = null;
            return false;
        }

        private static bool IsZero(byte[] data) {
            foreach (byte b in data) {
                if (b != 0) {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// A class represents a point at infinity
    /// </summary>
    public class ECPointAtInfinity : ECPoint {
        /// <summary>
        /// Constructor
        /// </summary>
        public ECPointAtInfinity()
            : base(null, null) {
        }
    }

    /// <summary>
    /// Abstract class for elliptic curve parameters.
    /// </summary>
    public abstract class EllipticCurve {

        /// <summary>
        /// Public key algorithm which uses this curve
        /// </summary>
        public abstract PublicKeyAlgorithm PublicKeyAlgorithm {
            get;
        }

        /// <summary>
        /// Curve name (identifier)
        /// </summary>
        public abstract string CurveName {
            get;
        }

        /// <summary>
        /// Base point G
        /// </summary>
        public abstract ECPoint BasePoint {
            get;
        }

        /// <summary>
        /// Order of G
        /// </summary>
        public abstract BigInteger Order {
            get;
        }

        /// <summary>
        /// Cofactor
        /// </summary>
        public abstract BigInteger Cofactor {
            get;
        }

        /// <summary>
        /// Validates if the point satisfies the equation of the elliptic curve.
        /// </summary>
        /// <param name="x">value of X</param>
        /// <param name="y">value of Y</param>
        /// <returns>true if the values satisfy.</returns>
        public abstract bool ValidatePoint(BigInteger x, BigInteger y);

        /// <summary>
        /// Calculate point multiplication
        /// </summary>
        /// <param name="k">scalar</param>
        /// <param name="t">point</param>
        /// <param name="infinityToNull">
        /// if result was point-at-infinity, and this parameter was true,
        /// null is returned instead of <see cref="ECPointAtInfinity"/>.
        /// </param>
        /// <returns>point on the curve, point at infinity, or null if failed</returns>
        public abstract ECPoint PointMul(BigInteger k, ECPoint t, bool infinityToNull);

        /// <summary>
        /// Calculate point multiplication
        /// </summary>
        /// <param name="k1">scalar</param>
        /// <param name="k2">scalar</param>
        /// <param name="t">point</param>
        /// <param name="infinityToNull">
        /// if result was point-at-infinity, and this parameter was true,
        /// null is returned instead of <see cref="ECPointAtInfinity"/>.
        /// </param>
        /// <returns>point on the curve, point at infinity, or null if failed</returns>
        public abstract ECPoint PointMul(BigInteger k1, BigInteger k2, ECPoint t, bool infinityToNull);

        /// <summary>
        /// Calculate point addition
        /// </summary>
        /// <param name="t1">point</param>
        /// <param name="t2">point</param>
        /// <param name="infinityToNull">
        /// if result was point-at-infinity, and this parameter was true,
        /// null is returned instead of <see cref="ECPointAtInfinity"/>.
        /// </param>
        /// <returns>point on the curve, point at infinity, or null if failed</returns>
        public abstract ECPoint PointAdd(ECPoint t1, ECPoint t2, bool infinityToNull);

        /// <summary>
        /// Validates if the point satisfies the equation of the elliptic curve.
        /// </summary>
        /// <param name="t">point</param>
        /// <returns>true if the values satisfy.</returns>
        public bool ValidatePoint(ECPoint t) {
            if (t is ECPointAtInfinity) {
                return false;
            }

            if (t.Validated) {
                return true;
            }

            if (ValidatePoint(t.X, t.Y)) {
                t.Validated = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Generates new key pair.
        /// </summary>
        /// <returns>key pair</returns>
        public ECDSAKeyPair GenerateKeyPair() {
            using (var rng = new RNGCryptoServiceProvider()) {
                return GenerateKeyPair(rng);
            }
        }

        /// <summary>
        /// Generates new key pair.
        /// </summary>
        /// <param name="rng">random number generator</param>
        /// <returns>key pair</returns>
        public ECDSAKeyPair GenerateKeyPair(RandomNumberGenerator rng) {
            int curveSize = this.Order.BitCount();
            while (true) {
                BigInteger k = BigInteger.GenerateRandom(curveSize, rng);
                if (k == 0 || k >= this.Order) {
                    continue;
                }

                ECPoint R = this.PointMul(k, this.BasePoint, true);
                if (R != null) {
                    return new ECDSAKeyPair(this, new ECDSAPublicKey(this, R), k);
                }
            }
        }

        /// <summary>
        /// Convert a point to the octet string.
        /// </summary>
        /// <param name="point">point</param>
        /// <returns>octet string</returns>
        public byte[] ConvertPointToOctetString(ECPoint point) {
            int byteLength = (this.Order.BitCount() + 7) / 8;
            byte[] buff = new byte[1 + byteLength * 2];

            buff[0] = 0x04; // uncompressed form

            byte[] x = point.X.GetBytes();
            if (x.Length > byteLength) {
                throw new SSHException("invalid public key value");
            }
            Buffer.BlockCopy(x, 0, buff, 1 + byteLength - x.Length, x.Length);

            byte[] y = point.Y.GetBytes();
            if (y.Length > byteLength) {
                throw new SSHException("invalid public key value");
            }
            Buffer.BlockCopy(y, 0, buff, 1 + byteLength * 2 - y.Length, y.Length);

            return buff;
        }

        //-----------------------------------------------------------------------
        // Curve definitions
        //-----------------------------------------------------------------------

        private static readonly ConcurrentDictionary<string, EllipticCurve> _curves
                                = new ConcurrentDictionary<string, EllipticCurve>();

        /// <summary>
        /// Find curve by OID
        /// </summary>
        /// <param name="oid"></param>
        /// <returns>curve parameters, or null if not found.</returns>
        public static EllipticCurve FindByOID(string oid) {
            string identifier;
            switch (oid) {
                case "1.2.840.10045.3.1.7": // nistp256 secp256r1
                    identifier = "nistp256";
                    break;

                case "1.3.132.0.34":    // nistp384 secp384r1
                    identifier = "nistp384";
                    break;

                case "1.3.132.0.35":    // nistp521 secp521r1
                    identifier = "nistp521";
                    break;

                default:
                    return null;    // not found
            }

            return FindByName(identifier);
        }

        /// <summary>
        /// Find curve by identifier
        /// </summary>
        /// <param name="name">curve identifier</param>
        /// <returns>curve parameters, or null if not found.</returns>
        public static EllipticCurve FindByName(string name) {
            EllipticCurve curve;
            if (_curves.TryGetValue(name, out curve)) {
                return curve;
            }

            switch (name) {
                case "nistp256":
                    curve = new EllipticCurveFp(
                        algorithm: PublicKeyAlgorithm.ECDSA_SHA2_NISTP256,
                        curveName: "nistp256",
                        p: new BigInteger(BigIntegerConverter.ParseHex(
                                "FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFF" +
                                "FFFFFFFF"
                            )),
                        a: new BigInteger(BigIntegerConverter.ParseHex(
                                "FFFFFFFF00000001000000000000000000000000FFFFFFFFFFFFFFFF" +
                                "FFFFFFFC"
                            )),
                        b: new BigInteger(BigIntegerConverter.ParseHex(
                                "5AC635D8AA3A93E7B3EBBD55769886BC651D06B0CC53B0F63BCE3C3E" +
                                "27D2604B"
                            )),
                        G: ECPoint.ParseUncompressed(BigIntegerConverter.ParseHex(
                                "046B17D1F2E12C4247F8BCE6E563A440F277037D812DEB33A0" +
                                "F4A13945D898C2964FE342E2FE1A7F9B8EE7EB4A7C0F9E162BCE3357" +
                                "6B315ECECBB6406837BF51F5"
                            )),
                        n: new BigInteger(BigIntegerConverter.ParseHex(
                                "FFFFFFFF00000000FFFFFFFFFFFFFFFFBCE6FAADA7179E84F3B9CAC2" +
                                "FC632551"
                            )),
                        h: new BigInteger(0x01)
                    );
                    break;

                case "nistp384":
                    curve = new EllipticCurveFp(
                        algorithm: PublicKeyAlgorithm.ECDSA_SHA2_NISTP384,
                        curveName: "nistp384",
                        p: new BigInteger(BigIntegerConverter.ParseHex(
                                "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" +
                                "FFFFFFFEFFFFFFFF0000000000000000FFFFFFFF"
                            )),
                        a: new BigInteger(BigIntegerConverter.ParseHex(
                                "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" +
                                "FFFFFFFEFFFFFFFF0000000000000000FFFFFFFC"
                            )),
                        b: new BigInteger(BigIntegerConverter.ParseHex(
                                "B3312FA7E23EE7E4988E056BE3F82D19181D9C6EFE8141120314088F" +
                                "5013875AC656398D8A2ED19D2A85C8EDD3EC2AEF"
                            )),
                        G: ECPoint.ParseUncompressed(BigIntegerConverter.ParseHex(
                                "04AA87CA22BE8B05378EB1C71EF320AD746E1D3B628BA79B98" +
                                "59F741E082542A385502F25DBF55296C3A545E3872760AB73617DE4A" +
                                "96262C6F5D9E98BF9292DC29F8F41DBD289A147CE9DA3113B5F0B8C0" +
                                "0A60B1CE1D7E819D7A431D7C90EA0E5F"
                            )),
                        n: new BigInteger(BigIntegerConverter.ParseHex(
                                "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFC7634D81" +
                                "F4372DDF581A0DB248B0A77AECEC196ACCC52973"
                            )),
                        h: new BigInteger(0x01)
                    );
                    break;

                case "nistp521":
                    curve = new EllipticCurveFp(
                        algorithm: PublicKeyAlgorithm.ECDSA_SHA2_NISTP521,
                        curveName: "nistp521",
                        p: new BigInteger(BigIntegerConverter.ParseHex(
                                "01FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" +
                                "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" +
                                "FFFFFFFFFFFFFFFFFFFFFFFF"
                            )),
                        a: new BigInteger(BigIntegerConverter.ParseHex(
                                "01FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" +
                                "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" +
                                "FFFFFFFFFFFFFFFFFFFFFFFC"
                            )),
                        b: new BigInteger(BigIntegerConverter.ParseHex(
                                "0051953EB9618E1C9A1F929A21A0B68540EEA2DA725B99B315F3" +
                                "B8B489918EF109E156193951EC7E937B1652C0BD3BB1BF073573DF88" +
                                "3D2C34F1EF451FD46B503F00"
                            )),
                        G: ECPoint.ParseUncompressed(BigIntegerConverter.ParseHex(
                                "0400C6858E06B70404E9CD9E3ECB662395B4429C648139053F" +
                                "B521F828AF606B4D3DBAA14B5E77EFE75928FE1DC127A2FFA8DE3348" +
                                "B3C1856A429BF97E7E31C2E5BD66011839296A789A3BC0045C8A5FB4" +
                                "2C7D1BD998F54449579B446817AFBD17273E662C97EE72995EF42640" +
                                "C550B9013FAD0761353C7086A272C24088BE94769FD16650"
                            )),
                        n: new BigInteger(BigIntegerConverter.ParseHex(
                                "01FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" +
                                "FFFFFFFFFFFFFFFA51868783BF2F966B7FCC0148F709A5D03BB5C9B8" +
                                "899C47AEBB6FB71E91386409"
                            )),
                        h: new BigInteger(0x01)
                    );
                    break;

                default:
                    return null;    // not found
            }

            _curves.TryAdd(name, curve);
            return curve;
        }

#if DEBUG
        // Tests point-multiplication using test vectors
        // http://point-at-infinity.org/ecc/nisttv
        internal static void TestPointMultiplication() {
            using (var reader = new System.IO.StreamReader(@"nisttv")) {
                EllipticCurve curve = null;
                string ks = null;
                BigInteger k = null;
                BigInteger x = null;
                BigInteger y = null;
                int testCount = 0;

                while (true) {
                    string line = reader.ReadLine();
                    if (line == null) {
                        break;
                    }
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"Curve:\s+(\w+)");
                    if (match.Success) {
                        string curveName = "nist" + match.Groups[1].Value.ToLowerInvariant();
                        curve = FindByName(curveName);
                        if (curve != null) {
                            Debug.WriteLine("Test " + curve.CurveName);
                        }
                        ks = null;
                        k = x = y = null;
                        testCount = 0;
                        continue;
                    }

                    if (line.StartsWith("k = ") && curve != null) {
                        ks = line.Substring(4).Trim();
                        k = BigInteger.Parse(ks);
                        continue;
                    }

                    if (line.StartsWith("x = ") && curve != null) {
                        x = new BigInteger(BigIntegerConverter.ParseHex(line.Substring(4).Trim()));
                        continue;
                    }

                    if (line.StartsWith("y = ") && curve != null) {
                        y = new BigInteger(BigIntegerConverter.ParseHex(line.Substring(4).Trim()));

                        if (k != null && x != null) {
                            ECPoint p = curve.PointMul(k, curve.BasePoint, true);
                            if (p == null || p is ECPointAtInfinity) {
                                throw new Exception("test failed");
                            }
                            if (p.X != x) {
                                throw new Exception("test failed: X doesn't match");
                            }
                            if (p.Y != y) {
                                throw new Exception("test failed: Y doesn't match");
                            }
                            ++testCount;
                            Debug.WriteLine("Pass #{0} : {1}", testCount, ks);
                        }

                        k = x = y = null;
                    }
                }
            }
        }

        // Tests public key validation using test vectors from NIST CAVP.
        // http://csrc.nist.gov/groups/STM/cavp/digital-signatures.html
        internal static void TestPKV() {
            using (var reader = new System.IO.StreamReader(@"186-3ecdsatestvectors\PKV.rsp")) {
                EllipticCurve curve = null;
                string qxs = null;
                BigInteger qx = null;
                BigInteger qy = null;
                string result = null;
                int testCount = 0;

                while (true) {
                    string line = reader.ReadLine();
                    if (line == null) {
                        break;
                    }
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"\[([-\w]+)\]");
                    if (match.Success) {
                        string curveName = "nist" + match.Groups[1].Value.ToLowerInvariant().Replace("-", "");
                        curve = FindByName(curveName);
                        if (curve != null) {
                            Debug.WriteLine("Test " + curve.CurveName);
                        }
                        qx = qy = null;
                        qxs = null;
                        result = null;
                        testCount = 0;
                        continue;
                    }

                    if (line.StartsWith("Qx = ") && curve != null) {
                        qxs = line.Substring(5).Trim();
                        qx = new BigInteger(BigIntegerConverter.ParseHex(qxs));
                        continue;
                    }

                    if (line.StartsWith("Qy = ") && curve != null) {
                        qy = new BigInteger(BigIntegerConverter.ParseHex(line.Substring(5).Trim()));
                        continue;
                    }

                    if (line.StartsWith("Result = ") && curve != null) {
                        result = line.Substring(9, 1);

                        if (qx != null && qy != null) {
                            var pk = new ECDSAPublicKey(curve, new ECPoint(qx, qy));
                            string r = pk.IsValid() ? "P" : "F";
                            if (r != result) {
                                throw new Exception("validation result doesn't match");
                            }
                            ++testCount;
                            Debug.WriteLine("Pass #{0} : {1}", testCount, qxs);
                        }

                        qx = qy = null;
                        qxs = null;
                        result = null;
                    }
                }
            }
        }

        // Tests key pair validation using test vectors from NIST CAVP.
        // http://csrc.nist.gov/groups/STM/cavp/digital-signatures.html
        internal static void TestKeyPair() {
            using (var reader = new System.IO.StreamReader(@"186-3ecdsatestvectors\KeyPair.rsp")) {
                EllipticCurve curve = null;
                string ds = null;
                BigInteger d = null;
                BigInteger qx = null;
                BigInteger qy = null;
                int testCount = 0;

                while (true) {
                    string line = reader.ReadLine();
                    if (line == null) {
                        break;
                    }
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"\[([-\w]+)\]");
                    if (match.Success) {
                        string curveName = "nist" + match.Groups[1].Value.ToLowerInvariant().Replace("-", "");
                        curve = FindByName(curveName);
                        if (curve != null) {
                            Debug.WriteLine("Test " + curve.CurveName);
                        }
                        d = qx = qy = null;
                        ds = null;
                        testCount = 0;
                        continue;
                    }

                    if (line.StartsWith("d = ") && curve != null) {
                        ds = line.Substring(4).Trim();
                        d = new BigInteger(BigIntegerConverter.ParseHex(ds));
                        continue;
                    }

                    if (line.StartsWith("Qx = ") && curve != null) {
                        qx = new BigInteger(BigIntegerConverter.ParseHex(line.Substring(5).Trim()));
                        continue;
                    }

                    if (line.StartsWith("Qy = ") && curve != null) {
                        qy = new BigInteger(BigIntegerConverter.ParseHex(line.Substring(5).Trim()));

                        if (d != null && qx != null) {
                            var pk = new ECDSAPublicKey(curve, new ECPoint(qx, qy));
                            var kp = new ECDSAKeyPair(curve, pk, d);
                            if (!kp.CheckKeyConsistency()) {
                                throw new Exception("validation failed");
                            }
                            ++testCount;
                            Debug.WriteLine("Pass #{0} : {1}", testCount, ds);
                        }

                        d = qx = qy = null;
                        ds = null;
                    }
                }
            }
        }

        // Tests signature verification using test vectors from NIST CAVP.
        // http://csrc.nist.gov/groups/STM/cavp/digital-signatures.html
        internal static void TestSignatureVerification() {
            using (var reader = new System.IO.StreamReader(@"186-3ecdsatestvectors\SigVer.rsp")) {
                EllipticCurve curve = null;
                byte[] msg = null;
                BigInteger qx = null;
                BigInteger qy = null;
                BigInteger r = null;
                BigInteger s = null;
                string result = null;
                int testCount = 0;

                while (true) {
                    string line = reader.ReadLine();
                    if (line == null) {
                        break;
                    }
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"\[([-\w]+),(SHA-\d+)\]");
                    if (match.Success) {
                        string curveName = "nist" + match.Groups[1].Value.ToLowerInvariant().Replace("-", "");
                        curve = FindByName(curveName);
                        if (curve != null) {
                            using (var hashFunc = ECDSAHashAlgorithmChooser.Choose(curve)) {
                                var hashName = "SHA-" + hashFunc.HashSize.ToString();
                                if (hashName == match.Groups[2].Value) {
                                    Debug.WriteLine("Test " + curve.CurveName);
                                }
                                else {
                                    // hash function doesn't match
                                    curve = null;
                                }
                            }
                        }
                        msg = null;
                        qx = qy = r = s = null;
                        result = null;
                        testCount = 0;
                        continue;
                    }

                    if (line.StartsWith("Msg = ") && curve != null) {
                        msg = BigIntegerConverter.ParseHex(line.Substring(6).Trim());
                        continue;
                    }

                    if (line.StartsWith("Qx = ") && curve != null) {
                        qx = new BigInteger(BigIntegerConverter.ParseHex(line.Substring(5).Trim()));
                        continue;
                    }

                    if (line.StartsWith("Qy = ") && curve != null) {
                        qy = new BigInteger(BigIntegerConverter.ParseHex(line.Substring(5).Trim()));
                        continue;
                    }

                    if (line.StartsWith("R = ") && curve != null) {
                        r = new BigInteger(BigIntegerConverter.ParseHex(line.Substring(4).Trim()));
                        continue;
                    }

                    if (line.StartsWith("S = ") && curve != null) {
                        s = new BigInteger(BigIntegerConverter.ParseHex(line.Substring(4).Trim()));
                        continue;
                    }

                    if (line.StartsWith("Result = ") && curve != null) {
                        result = line.Substring(9, 1);

                        if (msg != null && qx != null && qy != null && r != null && s != null) {
                            var pk = new ECDSAPublicKey(curve, new ECPoint(qx, qy));
                            var buf = new SSH2DataWriter();
                            buf.WriteBigInteger(r);
                            buf.WriteBigInteger(s);
                            var sig = buf.ToByteArray();
                            string verRes;
                            try {
                                pk.Verify(sig, msg);
                                verRes = "P";
                            }
                            catch (VerifyException) {
                                verRes = "F";
                            }
                            if (verRes != result) {
                                throw new Exception("verification result doesn't match");
                            }
                            ++testCount;
                            Debug.WriteLine("Pass #{0}", testCount);
                        }

                        msg = null;
                        qx = qy = r = s = null;
                        result = null;
                    }
                }
            }
        }
#endif
    }

    /// <summary>
    /// Elliptic curve domain parameters over Fp
    /// </summary>
    public class EllipticCurveFp : EllipticCurve {

        private readonly PublicKeyAlgorithm _algorithm;
        private readonly string _curveName;

        /// <summary>
        /// Public key algorithm which uses this curve
        /// </summary>
        public override PublicKeyAlgorithm PublicKeyAlgorithm {
            get {
                return _algorithm;
            }
        }

        /// <summary>
        /// Curve name (identifier)
        /// </summary>
        public override string CurveName {
            get {
                return _curveName;
            }
        }

        /// <summary>
        /// Base point G
        /// </summary>
        public override ECPoint BasePoint {
            get {
                return G;
            }
        }

        /// <summary>
        /// Order of G
        /// </summary>
        public override BigInteger Order {
            get {
                return n;
            }
        }

        /// <summary>
        /// Cofactor
        /// </summary>
        public override BigInteger Cofactor {
            get {
                return h;
            }
        }

        /// <summary>
        /// Odd prime
        /// </summary>
        public readonly BigInteger p;

        /// <summary>
        /// Curve parameter
        /// </summary>
        public readonly BigInteger a;

        /// <summary>
        /// Curve parameter
        /// </summary>
        public readonly BigInteger b;

        /// <summary>
        /// Base point
        /// </summary>
        public readonly ECPoint G;

        /// <summary>
        /// Order n of G
        /// </summary>
        public readonly BigInteger n;

        /// <summary>
        /// Cofactor
        /// </summary>
        public readonly BigInteger h;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="algorithm">public key algorithm which uses this curve</param>
        /// <param name="curveName">curve name</param>
        /// <param name="p">odd prime</param>
        /// <param name="a">curve parameter</param>
        /// <param name="b">curve parameter</param>
        /// <param name="G">base point</param>
        /// <param name="n">order n of G</param>
        /// <param name="h">cofactor</param>
        public EllipticCurveFp(
                PublicKeyAlgorithm algorithm, string curveName,
                BigInteger p, BigInteger a, BigInteger b, ECPoint G, BigInteger n, BigInteger h) {
            this._algorithm = algorithm;
            this._curveName = curveName;
            this.p = p;
            this.a = a;
            this.b = b;
            this.G = G;
            this.n = n;
            this.h = h;
        }

        /// <summary>
        /// Validate if the point satisfies the equation of the elliptic curve.
        /// </summary>
        /// <param name="x">value of X</param>
        /// <param name="y">value of Y</param>
        /// <returns>true if the values satisfy.</returns>
        public override bool ValidatePoint(BigInteger x, BigInteger y) {
            if (x == 0 || x >= p || y == 0 || y >= p) {
                return false;
            }
            BigInteger y2 = (y * y) % p;
            BigInteger fx = ((x * x + a) * x + b) % p;
            return y2 == fx;
        }

        /// <summary>
        /// Calculate point multiplication
        /// </summary>
        /// <param name="k">scalar</param>
        /// <param name="t">point</param>
        /// <param name="infinityToNull">
        /// if result was point-at-infinity, and this parameter was true,
        /// null is returned instead of <see cref="ECPointAtInfinity"/>.
        /// </param>
        /// <returns>point on the curve, point at infinity, or null if failed</returns>
        public override ECPoint PointMul(BigInteger k, ECPoint t, bool infinityToNull) {
            if (t == null) {
                return null;
            }
            BigInteger.ModulusRing ring = new BigInteger.ModulusRing(p);
            ECPoint r;
            if (!PointMul(ring, t, k, out r)) {
                return null;
            }
            if (infinityToNull && r is ECPointAtInfinity) {
                return null;
            }
            return r;
        }

        /// <summary>
        /// Calculate point multiplication
        /// </summary>
        /// <param name="k1">scalar</param>
        /// <param name="k2">scalar</param>
        /// <param name="t">point</param>
        /// <param name="infinityToNull">
        /// if result was point-at-infinity, and this parameter was true,
        /// null is returned instead of <see cref="ECPointAtInfinity"/>.
        /// </param>
        /// <returns>point on the curve, point at infinity, or null if failed</returns>
        public override ECPoint PointMul(BigInteger k1, BigInteger k2, ECPoint t, bool infinityToNull) {
            BigInteger.ModulusRing ring = new BigInteger.ModulusRing(p);
            BigInteger k = ring.Multiply(k1, k2);
            return PointMul(k, t, infinityToNull);
        }

        /// <summary>
        /// Calculate point addition
        /// </summary>
        /// <param name="t1">point</param>
        /// <param name="t2">point</param>
        /// <param name="infinityToNull">
        /// if result was point-at-infinity, and this parameter was true,
        /// null is returned instead of <see cref="ECPointAtInfinity"/>.
        /// </param>
        /// <returns>point, or null if failed</returns>
        public override ECPoint PointAdd(ECPoint t1, ECPoint t2, bool infinityToNull) {
            if (t1 == null || t2 == null) {
                return null;
            }
            BigInteger.ModulusRing ring = new BigInteger.ModulusRing(p);
            ECPoint r;
            if (!PointAdd(ring, t1, t2, out r)) {
                return null;
            }
            if (infinityToNull && r is ECPointAtInfinity) {
                return null;
            }
            return r;
        }

        /// <summary>
        /// Point addition over the curve
        /// </summary>
        private bool PointAdd(
                BigInteger.ModulusRing ring,
                ECPoint p1,
                ECPoint p2,
                out ECPoint p3) {

            if (p1 is ECPointAtInfinity) {
                p3 = p2;
                return true;
            }
            if (p2 is ECPointAtInfinity) {
                p3 = p1;
                return true;
            }

            if (p1.X == p2.X) {
                if (p1.Y == p2.Y) {
                    return PointDouble(ring, p1, out p3);
                }
                else {
                    p3 = new ECPointAtInfinity();
                    return true;
                }
            }

            // x3 = {(y2 - y1)/(x2 - x1)}^2 - (x1 + x2)
            // y3 = {(y2 - y1)/(x2 - x1)} * (x1 - x3) - y1

            try {
                BigInteger x1 = p1.X;
                BigInteger y1 = p1.Y;
                BigInteger x2 = p2.X;
                BigInteger y2 = p2.Y;

                BigInteger lambda = ring.Multiply(ring.Difference(y2, y1), ring.Difference(x2, x1).ModInverse(p));
                BigInteger x3 = ring.Difference(ring.Multiply(lambda, lambda), x1 + x2);
                BigInteger y3 = ring.Difference(ring.Multiply(lambda, ring.Difference(x1, x3)), y1);
                p3 = new ECPoint(x3, y3);
                return true;
            }
            catch (Exception) {
                p3 = null;
                return false;
            }
        }

        /// <summary>
        /// Point dooubling over the curve
        /// </summary>
        private bool PointDouble(
                BigInteger.ModulusRing ring,
                ECPoint p1,
                out ECPoint p3) {

            if (p1 is ECPointAtInfinity) {
                p3 = p1;
                return true;
            }

            if (p1.Y == 0) {
                p3 = new ECPointAtInfinity();
                return true;
            }

            // x3 = {(3 * x1^2 + a)/(2 * y1)}^2 - (2 * x1)
            // y3 = {(3 * x1^2 + a)/(2 * y1)} * (x1 - x3) - y1

            try {
                BigInteger x1 = p1.X;
                BigInteger y1 = p1.Y;

                BigInteger x1_2 = ring.Multiply(x1, x1);
                BigInteger lambda = ring.Multiply(x1_2 + x1_2 + x1_2 + a, (y1 + y1).ModInverse(p));
                BigInteger x3 = ring.Difference(ring.Multiply(lambda, lambda), x1 + x1);
                BigInteger y3 = ring.Difference(ring.Multiply(lambda, ring.Difference(x1, x3)), y1);
                p3 = new ECPoint(x3, y3);
                return true;
            }
            catch (Exception) {
                p3 = null;
                return false;
            }
        }

#if USE_WNAF_POINT_MULTIPLICATION
        /// <summary>
        /// Point multiplication over the curve
        /// </summary>
        private bool PointMul(
                BigInteger.ModulusRing ring,
                ECPoint p1,
                BigInteger k,
                out ECPoint p2) {

            //
            // Uses Width-w NAF method
            //

            if (p1 is ECPointAtInfinity) {
                p2 = p1;
                return true;
            }

            const int W = 6;
            const uint TPW = 1u << W;   // 2^W
            const uint TPWD = 1u << (W - 1);   // 2^(W-1)

            // precompute point multiplication : {1 .. 2^(W-1)-1}P.
            // array is allocated for {0 .. 2^(W-1)-1}P, and only elements at the odd index are used.
            ECPoint[] precomp = new ECPoint[TPWD];
            ECPoint[] precompNeg = new ECPoint[TPWD];   // -{1 .. 2^(W-1)-1}P; points are set on demand.

            {
                ECPoint t = p1;
                ECPoint t2;
                if (!PointDouble(ring, t, out t2)) {
                    goto Failure;
                }
                for (uint i = 1; i < TPWD; i += 2) {
                    if (i != 1) {
                        if (!PointAdd(ring, t, t2, out t)) {
                            goto Failure;
                        }
                    }
                    precomp[i] = t;
                }
            }

            Stack<sbyte> precompIndex;

            {
                byte[] d = k.GetBytes();
                int bitCount = k.BitCount();
                int bitIndex = 0;
                int byteOffset = d.Length - 1;
                bool noMoreBits = false;
                uint bitBuffer = 0;
                const uint WMASK = (1u << W) - 1;

                precompIndex = new Stack<sbyte>(bitCount + 1);

                if (bitIndex < bitCount) {
                    bitBuffer = (uint)(d[byteOffset] & WMASK);
                    bitIndex += W;
                }
                else {
                    noMoreBits = true;
                }

                while (!noMoreBits || bitBuffer != 0) {
                    if ((bitBuffer & 1) != 0) { // bits % 2 == 1
                        uint m = bitBuffer & WMASK; // m = bits % TPW;
                        if ((m & TPWD) != 0) {  // test m >= 2^(W-1)
                            // m is odd; thus
                            // (2^(W-1) + 1) <= m <= (2^W - 1)
                            sbyte index = (sbyte)((int)m - (int)TPW);  // -2^(W-1)+1 .. -1
                            precompIndex.Push(index);
                            bitBuffer = (bitBuffer & ~WMASK) + TPW; // bits -= m - 2^W
                            // a carried bit by adding 2^W is retained in the bit buffer
                        }
                        else {
                            // 1 <= m <= (2^(W-1) - 1)
                            sbyte index = (sbyte)m; // odd index
                            precompIndex.Push(index);
                            bitBuffer = (bitBuffer & ~WMASK); // bits -= m
                        }
                    }
                    else {
                        precompIndex.Push(0);
                    }

                    // shift bits
                    if (bitIndex < bitCount) {
                        // load next bit into the bit buffer (add to the carried bits in the bit buffer)
                        bitBuffer += (uint)((d[byteOffset - bitIndex / 8] >> (bitIndex % 8)) & 1) << W;
                        ++bitIndex;
                    }
                    else {
                        noMoreBits = true;
                    }
                    bitBuffer >>= 1;
                }
            }

            {
                ECPoint p = null;

                while (precompIndex.Count > 0) {
                    if (p != null) {
                        if (!PointDouble(ring, p, out p)) {
                            goto Failure;
                        }
                    }

                    ECPoint pre;
                    int index = precompIndex.Pop();
                    if (index > 0) {
                        pre = precomp[index];
                    }
                    else if (index < 0) {
                        pre = precompNeg[-index];
                        if (pre == null) {
                            // on EC over Fp, P={x, y} and -P={x, -y}
                            pre = precomp[-index];
                            if (!(pre is ECPointAtInfinity)) {
                                pre = new ECPoint(pre.X, ring.Difference(0, pre.Y));
                            }
                            precompNeg[-index] = pre;
                        }
                    }
                    else {
                        continue;
                    }

                    if (p != null) {
                        if (!PointAdd(ring, p, pre, out p)) {
                            goto Failure;
                        }
                    }
                    else {
                        p = pre;
                    }
                }

                if (p == null) {
                    // case of k = 0
                    goto Failure;
                }

                // succeeded
                p2 = p;
                return true;
            }

        Failure:
            p2 = null;
            return false;
        }

#else

        /// <summary>
        /// Point multiplication over the curve
        /// </summary>
        private bool PointMul(
                BigInteger.ModulusRing ring,
                ECPoint p1,
                BigInteger k,
                out ECPoint p2) {

            //
            // Windowed method
            //

            const int W = 4;
            const uint TPW = 1u << W;   // 2^W

            // precompute point multiplication : {1 .. 2^W-1}P.
            // array is allocated for {0 .. 2^W-1}P, and index 0 is never used.
            ECPoint[] precomp = new ECPoint[TPW];

            {
                ECPoint t = p1;
                precomp[1] = t;
                if (!PointDouble(ring, t, out t)) {
                    goto Failure;
                }
                precomp[2] = t;
                for (uint i = 3; i < TPW; ++i) {
                    if (!PointAdd(ring, t, p1, out t)) {
                        goto Failure;
                    }
                    precomp[i] = t;
                }
            }

            byte[] bits = k.GetBytes();

            ECPoint p = null;

            foreach (byte b in bits) {
                for (int f = 0; f < 2; ++f) {
                    if (p != null) {
                        for (int i = 0; i < 4; ++i) {
                            if (!PointDouble(ring, p, out p)) {
                                goto Failure;
                            }
                        }
                    }

                    int precompIndex = (f == 0) ? (b >> 4) : (b & 0xf);
                    if (precompIndex != 0) {
                        if (p != null) {
                            if (!PointAdd(ring, p, precomp[precompIndex], out p)) {
                                goto Failure;
                            }
                        }
                        else {
                            p = precomp[precompIndex];
                        }
                    }
                }
            }

            if (p == null) {
                // case of k = 0
                goto Failure;
            }

            // succeeded
            p2 = p;
            return true;

        Failure:
            p2 = null;
            return false;
        }
#endif
    }

#if false
    // Elliptic curve over F2^m is not supported for now.

    /// <summary>
    /// Elliptic curve domain parameters over F2^m
    /// </summary>
    public class EllipticCurveF2m : EllipticCurve {

        private readonly PublicKeyAlgorithm _algorithm;
        private readonly string _curveName;

        /// <summary>
        /// Public key algorithm which uses this curve
        /// </summary>
        public override PublicKeyAlgorithm PublicKeyAlgorithm {
            get {
                return _algorithm;
            }
        }

        /// <summary>
        /// Curve name (identifier)
        /// </summary>
        public override string CurveName {
            get {
                return _curveName;
            }
        }

        /// <summary>
        /// Order of G
        /// </summary>
        public override BigInteger Order {
            get {
                return n;
            }
        }

        /// <summary>
        /// Exponent of 2
        /// </summary>
        public readonly int m;

        /// <summary>
        /// Exponents of x
        /// </summary>
        public readonly int[] Exp;

        /// <summary>
        /// Curve parameter
        /// </summary>
        public readonly byte[] a;

        /// <summary>
        /// Curve parameter
        /// </summary>
        public readonly byte[] b;

        /// <summary>
        /// Base point
        /// </summary>
        public readonly ECPoint G;

        /// <summary>
        /// Order n of G
        /// </summary>
        public readonly BigInteger n;

        /// <summary>
        /// Cofactor
        /// </summary>
        public readonly BigInteger h;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="algorithm">public key algorithm which uses this curve</param>
        /// <param name="curveName">curve name</param>
        /// <param name="m">exponent of 2</param>
        /// <param name="exp">exponents of x</param>
        /// <param name="a">curve parameter</param>
        /// <param name="b">curve parameter</param>
        /// <param name="G">base point</param>
        /// <param name="n">order n of G</param>
        /// <param name="h">cofactor</param>
        public EllipticCurveF2m(
                PublicKeyAlgorithm algorithm, string curveName,
                int m, int[] exp, byte[] a, byte[] b, ECPoint G, BigInteger n, BigInteger h) {
            this._algorithm = algorithm;
            this._curveName = curveName;
            this.m = m;
            this.Exp = exp;
            this.a = a;
            this.b = b;
            this.G = G;
            this.n = n;
            this.h = h;
        }

        /// <summary>
        /// Validate if the point satisfies the equation of the elliptic curve.
        /// </summary>
        /// <param name="x">value of X</param>
        /// <param name="y">value of Y</param>
        /// <returns>true if the values satisfy.</returns>
        public override bool ValidatePoint(BigInteger x, BigInteger y) {
            throw new NotImplementedException();
            /*
            BinaryPolynomial xval = new BinaryPolynomial(x);
            BinaryPolynomial yval = new BinaryPolynomial(y);
            BinaryPolynomial aval = new BinaryPolynomial(a);
            BinaryPolynomial bval = new BinaryPolynomial(b);
            BinaryPolynomial left = (yval + xval) * yval;
            BinaryPolynomial right = (xval + aval) * (xval * xval) + bval;
            return left == right;
             */
        }

        /// <summary>
        /// Calculate multiplication of G
        /// </summary>
        /// <param name="k">scalar value for multiplication</param>
        /// <returns>point</returns>
        public override ECPoint BasePointMul(BigInteger k) {
            throw new NotImplementedException();
        }
    }
#endif

    /// <summary>
    /// Elliptic curve cryptography public key
    /// </summary>
    public class ECDSAPublicKey : PublicKey, IVerifier {

        private readonly EllipticCurve _curve;
        private readonly ECPoint _point;

        public string CurveName {
            get {
                return _curve.CurveName;
            }
        }

        public ECPoint Point {
            get {
                return _point;
            }
        }

        public ECDSAPublicKey(EllipticCurve curve, ECPoint point) {
            _curve = curve;
            _point = point;
        }

        public override PublicKeyAlgorithm Algorithm {
            get {
                return _curve.PublicKeyAlgorithm;
            }
        }

        public void Verify(byte[] data, byte[] expected) {
            SSH2DataReader reader = new SSH2DataReader(data);
            BigInteger r = reader.ReadMPInt();
            BigInteger s = reader.ReadMPInt();
            if (r == 0 || r >= _curve.Order || s == 0 || s >= _curve.Order) {
                goto Fail;
            }

            if (!IsValid()) {
                goto Fail;
            }

            BigInteger order = _curve.Order;

            byte[] hash = HashForSigning(expected, _curve);
            BigInteger e = new BigInteger(hash);

            BigInteger.ModulusRing nring = new BigInteger.ModulusRing(order);

            try {
                BigInteger sInv = s.ModInverse(order);
                BigInteger u1 = nring.Multiply(e, sInv);
                BigInteger u2 = nring.Multiply(r, sInv);

                ECPoint p1 = _curve.PointMul(u1, _curve.BasePoint, true);
                ECPoint p2 = _curve.PointMul(u2, Point, true);
                if (p1 == null || p2 == null) {
                    goto Fail;
                }
                ECPoint R = _curve.PointAdd(p1, p2, true);
                if (R == null) {
                    goto Fail;
                }

                BigInteger v = R.X % order;

                if (v != r) {
                    goto Fail;
                }
            }
            catch (Exception) {
                goto Fail;
            }

            return;

        Fail:
            throw new VerifyException("Failed to verify");
        }

        public override void WriteTo(IKeyWriter writer) {
            byte[] publicKeyOctetString = _curve.ConvertPointToOctetString(_point);
            writer.WriteString(_curve.CurveName);
            writer.WriteByteString(publicKeyOctetString);
        }

        internal static ECDSAPublicKey ReadFrom(SSH2DataReader reader) {
            string curveName = reader.ReadString();
            byte[] q = reader.ReadByteString();

            EllipticCurve curve = EllipticCurve.FindByName(curveName);
            if (curve == null) {
                throw new SSHException(Strings.GetString("UnsupportedEllipticCurve") + " : " + curveName);
            }

            ECPoint p;
            if (!ECPoint.Parse(q, curve, out p)) {
                throw new SSHException(Strings.GetString("InvalidECPublicKey"));
            }

            return new ECDSAPublicKey(curve, p);
        }

        /// <summary>
        /// Check public key
        /// </summary>
        /// <returns>true if this key is valid.</returns>
        public bool IsValid() {
            return _curve.ValidatePoint(Point);
        }

        /// <summary>
        /// Convert a point of public key to an octet string.
        /// </summary>
        /// <returns></returns>
        public byte[] ToOctetString() {
            return _curve.ConvertPointToOctetString(_point);
        }

        /// <summary>
        /// Hash data for signing
        /// </summary>
        /// <param name="data">data to be hashed</param>
        /// <param name="curve">elliptic curve for determining hashing algorithm</param>
        /// <returns></returns>
        internal byte[] HashForSigning(byte[] data, EllipticCurve curve) {

            byte[] hash;
            using (var hashAlgorithm = ECDSAHashAlgorithmChooser.Choose(curve)) {
                hash = hashAlgorithm.ComputeHash(data);
            }

            return ExtractLeftBits(hash, curve.Order.BitCount());
        }

        private byte[] ExtractLeftBits(byte[] src, int bits) {
            if (src.Length * 8 <= bits) {
                return src;
            }
            int bytes = (bits + 7) / 8;
            byte[] buff = new byte[bytes];
            int shift = (8 - bits % 8) % 8;
            byte prev = 0;
            for (int i = 0; i < bytes; ++i) {
                byte b = src[i];
                buff[i] = (byte)((byte)(prev << (8 - shift)) | (byte)(b >> shift));
                prev = b;
            }
            return buff;
        }
    }

    /// <summary>
    /// Elliptic curve key pair
    /// </summary>
    public class ECDSAKeyPair : KeyPair, ISigner, IVerifier {

        private readonly EllipticCurve _curve;
        private readonly ECDSAPublicKey _publicKey;
        private readonly BigInteger _privateKey;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="curve">elliptic curve</param>
        /// <param name="publicKey">public key</param>
        /// <param name="privateKey">private key</param>
        public ECDSAKeyPair(EllipticCurve curve, ECDSAPublicKey publicKey, BigInteger privateKey) {
            _curve = curve;
            _publicKey = publicKey;
            _privateKey = privateKey;
        }

        /// <summary>
        /// Public key
        /// </summary>
        public override PublicKey PublicKey {
            get {
                return _publicKey;
            }
        }

        /// <summary>
        /// Public key algorithm
        /// </summary>
        public override PublicKeyAlgorithm Algorithm {
            get {
                return _curve.PublicKeyAlgorithm;
            }
        }

        /// <summary>
        /// Public key as a point
        /// </summary>
        public ECPoint PublicKeyPoint {
            get {
                return _publicKey.Point;
            }
        }

        /// <summary>
        /// Private key
        /// </summary>
        public BigInteger PrivateKey {
            get {
                return _privateKey;
            }
        }

        /// <summary>
        /// Verification
        /// </summary>
        /// <param name="data"></param>
        /// <param name="expected"></param>
        public void Verify(byte[] data, byte[] expected) {
            _publicKey.Verify(data, expected);
        }

        /// <summary>
        /// Signing
        /// </summary>
        /// <param name="data">data to sign</param>
        /// <returns>signature blob</returns>
        public byte[] Sign(byte[] data) {
            BigInteger order = _curve.Order;

            byte[] hash = _publicKey.HashForSigning(data, _curve);
            BigInteger e = new BigInteger(hash);

            using (var rng = new RNGCryptoServiceProvider()) {

                BigInteger.ModulusRing nring = new BigInteger.ModulusRing(order);

                for (int tries = 0; tries < 10; ++tries) {
                    try {
                        ECDSAKeyPair tempKeyPair = _curve.GenerateKeyPair(rng);

                        BigInteger r = tempKeyPair.PublicKeyPoint.X % order;
                        if (r == 0) {
                            continue;
                        }

                        BigInteger k = tempKeyPair.PrivateKey;
                        BigInteger s = nring.Multiply(k.ModInverse(order), e + nring.Multiply(r, _privateKey));
                        if (s == 0) {
                            continue;
                        }

                        SSH2DataWriter writer = new SSH2DataWriter();
                        writer.WriteBigInteger(r);
                        writer.WriteBigInteger(s);

                        return writer.ToByteArray();
                    }
                    catch (Exception ex) {
                        Debug.WriteLine(ex);
                    }
                }
            }

            throw new SSHException(Strings.GetString("FailedSigning"));
        }

        /// <summary>
        /// Check consistency of public key and private key.
        /// </summary>
        /// <returns>true if keys are valid.</returns>
        public bool CheckKeyConsistency() {
            if (!_publicKey.IsValid()) {
                return false;
            }

            if (_privateKey == 0 || _privateKey >= _curve.Order) {
                return false;
            }

            ECPoint q = _curve.PointMul(_privateKey, _curve.BasePoint, true);
            if (q == null) {
                return false;
            }

            return q.X == _publicKey.Point.X && q.Y == _publicKey.Point.Y;
        }
    }

    /// <summary>
    /// A utility class for choosing hashing algorithm for ECDSA.
    /// </summary>
    public static class ECDSAHashAlgorithmChooser {

        /// <summary>
        /// Chooses a hashing algorithm.
        /// </summary>
        /// <remarks>
        /// Hashing algorithm is determined according to the curve size as described in RFC5656.
        /// </remarks>
        /// <param name="curve">elliptic curve</param>
        /// <returns>new instance of the hashing algorithm</returns>
        public static HashAlgorithm Choose(EllipticCurve curve) {
            int orderBits = curve.Order.BitCount();

            if (orderBits <= 256) {
                return new SHA256CryptoServiceProvider();
            }
            else if (orderBits <= 384) {
                return new SHA384CryptoServiceProvider();
            }
            else {
                return new SHA512CryptoServiceProvider();
            }
        }
    }
}
