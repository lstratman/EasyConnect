/*
 * Copyright 2011-2016 The Poderosa Project.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 */
using System;
using System.IO;

using Granados.Mono.Math;
using System.Text;
using System.Globalization;

#if UNITTEST
using NUnit.Framework;
#endif

namespace Granados.Poderosa.KeyFormat {

    /// <summary>
    /// Reads elements which are encoded by ASN.1 Basic Encoding Rules
    /// </summary>
    /// <remarks>
    /// Only SEQUENCE, BIT STRING, OCTET STRING, OBJECT IDENTIFIER and INTEGER are supported.
    /// </remarks>
    internal class BERReader {

        private readonly Stream strm;

        private const int LENGTH_INDEFINITE = -1;
        private const int TAG_INTEGER = 2;
        private const int TAG_BITSTRING = 3;
        private const int TAG_OCTETSTRING = 4;
        private const int TAG_OBJECTIDENTIFIER = 6;
        private const int TAG_SEQUENCE = 16;

        internal struct BERTagInfo {
            public int ClassBits;
            public bool IsConstructed;
            public int TagNumber;
            public int Length;
        }

        public enum TagClass {
            Universal = 0,
            Application = 1,
            ContextSpecific = 2,
            Private = 3,
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="s">stream to input</param>
        public BERReader(Stream s) {
            this.strm = s;
        }

        /// <summary>
        /// Read sequnce. (only check the value type)
        /// </summary>
        /// <returns>true if succeeded.</returns>
        public bool ReadSequence() {
            int len;
            return ReadTag(TagClass.Universal, true, TAG_SEQUENCE, out len);
        }

        /// <summary>
        /// Read tag. (only check the value type)
        /// </summary>
        /// <param name="tagClass">expected tag class</param>
        /// <param name="isConstructed">expected value of "constructed" flag</param>
        /// <param name="tagNumber">expected tag number</param>
        /// <param name="length">length of the value field will be stored</param>
        /// <returns>true if succeeded.</returns>
        public bool ReadTag(TagClass tagClass, bool isConstructed, int tagNumber, out int length) {
            BERTagInfo tagInfo = new BERTagInfo();
            if (ReadTagInfo(ref tagInfo)
                && tagInfo.ClassBits == (int)tagClass
                && tagInfo.IsConstructed == isConstructed
                && tagInfo.TagNumber == tagNumber) {

                length = tagInfo.Length;
                return true;
            }

            length = 0;
            return false;
        }

        /// <summary>
        /// Read integer.
        /// </summary>
        /// <param name="bigint">BigInteger instance will be stored if succeeded.</param>
        /// <returns>true if succeeded.</returns>
        public bool ReadInteger(out BigInteger bigint) {
            byte[] data;
            if (ReadBinary(TagClass.Universal, false, TAG_INTEGER, out data)) {
                bigint = new BigInteger(data);
                return true;
            }

            bigint = null;
            return false;
        }

        /// <summary>
        /// Read octet-string.
        /// </summary>
        /// <param name="str">byte array will be stored if succeeded.</param>
        /// <returns>true if succeeded.</returns>
        public bool ReadOctetString(out byte[] str) {
            return ReadBinary(TagClass.Universal, false, TAG_OCTETSTRING, out str);
        }

        /// <summary>
        /// Read object-identifier.
        /// </summary>
        /// <param name="oid">object identifier will be stored if succeeded.</param>
        /// <returns>true if succeeded.</returns>
        public bool ReadObjectIdentifier(out string oid) {
            byte[] data;
            if (ReadBinary(TagClass.Universal, false, TAG_OBJECTIDENTIFIER, out data)) {
                var s = new StringBuilder();
                s.Append((data[0] / 40).ToString(NumberFormatInfo.InvariantInfo));
                s.Append('.').Append((data[0] % 40).ToString(NumberFormatInfo.InvariantInfo));

                uint val = 0;
                for (int i = 1; i < data.Length; ++i) {
                    val = (val << 7) | (data[i] & 0x7fu);
                    if ((data[i] & 0x80) == 0) {
                        s.Append('.').Append(val.ToString(NumberFormatInfo.InvariantInfo));
                        val = 0;
                    }
                }

                oid = s.ToString();
                return true;
            }

            oid = null;
            return false;
        }

        /// <summary>
        /// Read bit string.
        /// </summary>
        /// <param name="bits">byte array will be stored if succeeded.</param>
        /// <returns>true if succeeded.</returns>
        public bool ReadBitString(out byte[] bits) {
            byte[] data;
            if (ReadBinary(TagClass.Universal, false, TAG_BITSTRING, out data)) {
                int unusedBits = data[0];
                int offsetBytes = unusedBits / 8;
                int offsetBits = unusedBits % 8;
                int bitDataLength = data.Length - 1 - offsetBytes;
                byte[] bitData = new byte[bitDataLength];
                ushort w = 0;
                for (int i = 0; i < bitDataLength; ++i) {
                    w = (ushort)((w << 8) | data[i + 1]);
                    bitData[i] = (byte)(w >> offsetBits);
                }
                bits = bitData;
                return true;
            }

            bits = null;
            return false;
        }

        private bool ReadBinary(TagClass tagClass, bool isConstructed, int tagNumber, out byte[] data) {
            int valueLength;
            if (ReadTag(tagClass, isConstructed, tagNumber, out valueLength)
                    && valueLength != LENGTH_INDEFINITE && valueLength > 0) {

                byte[] buff = new byte[valueLength];
                int len = strm.Read(buff, 0, valueLength);
                if (len == valueLength) {
                    data = buff;
                    return true;
                }
            }

            data = null;
            return false;
        }

        internal bool ReadTagInfo(ref BERTagInfo tagInfo) {
            return ReadTag(ref tagInfo.ClassBits, ref tagInfo.IsConstructed, ref tagInfo.TagNumber) && ReadLength(ref tagInfo.Length);
        }

        private bool ReadTag(ref int cls, ref bool constructed, ref int tagnum) {
            int n = strm.ReadByte();
            if (n == -1)
                return false;
            cls = (n >> 6) & 0x3;
            constructed = ((n & 0x20) != 0);
            if ((n & 0x1f) != 0x1f) {
                tagnum = n & 0x1f;
                return true;
            }

            int num = 0;
            int bits = 0;
            while (true) {
                n = strm.ReadByte();
                if (n == -1)
                    return false;
                num = (num << 7) | (n & 0x7f);
                if (bits == 0) {
                    bits = 7;
                    for (int mask = 0x40; bits != 0; mask >>= 1, bits--) {
                        if ((n & mask) != 0)
                            break;
                    }
                }
                else {
                    bits += 7;
                    if (bits > 31)
                        return false;
                }
                if ((n & 0x80) == 0)
                    break;
            }
            tagnum = num;
            return true;
        }

        private bool ReadLength(ref int length) {
            int n = strm.ReadByte();
            if (n == -1)
                return false;
            if (n == 0x80) {
                length = LENGTH_INDEFINITE;
                return true;
            }
            if ((n & 0x80) == 0) {
                length = n & 0x7f;
                return true;
            }

            int octets = n & 0x7f;
            int num = 0;
            int bits = 0;
            for (int i = 0; i < octets; i++) {
                n = strm.ReadByte();
                if (n == -1)
                    return false;
                num = (num << 8) | (n & 0xff);
                bits += 8;
                if (bits > 31)
                    return false;
            }
            length = num;
            return true;
        }

    }

#if UNITTEST

    [TestFixture]
    public class BERReaderTest {

        [Test]
        public void TestLargeTag() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x7f, 0x87, 0xef, 0xab, 0xb7, 0x6e, 0x03, 0x02, 0x01, 0x01 }
            )) {
                BERReader reader = new BERReader(mem);
                BERReader.BERTagInfo tagInfo = new BERReader.BERTagInfo();
                Assert.True(reader.ReadTagInfo(ref tagInfo));
                Assert.AreEqual(1, tagInfo.ClassBits);
                Assert.AreEqual(true, tagInfo.IsConstructed);
                Assert.AreEqual(0x7deadbee, tagInfo.TagNumber);
                Assert.AreEqual(3, tagInfo.Length);
            }
        }

        [Test]
        public void TestIncompleteTag() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x7f, 0x87, 0xef, 0xab, 0xb7, 0x6e }
            )) {
                BERReader reader = new BERReader(mem);
                BERReader.BERTagInfo tagInfo = new BERReader.BERTagInfo();
                Assert.False(reader.ReadTagInfo(ref tagInfo));
            }
        }

        [Test]
        public void TestIncompleteSequenceTag() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x30 }
            )) {
                BERReader reader = new BERReader(mem);
                Assert.False(reader.ReadSequence());
            }
        }

        [Test]
        public void TestNonSequenceTag() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x02, 0x01, 0x01 }
            )) {
                BERReader reader = new BERReader(mem);
                Assert.False(reader.ReadSequence());
            }
        }

        [Test]
        public void TestSequenceTag1() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x30, 0x03, 0x02, 0x01, 0x01 }
            )) {
                BERReader reader = new BERReader(mem);
                Assert.True(reader.ReadSequence());
            }
        }

        [Test]
        public void TestIndefiniteSequenceTag() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x30, 0x80, 0x02, 0x01, 0x01, 0x00, 0x00 }
            )) {
                BERReader reader = new BERReader(mem);
                Assert.True(reader.ReadSequence());
            }
        }

        [Test]
        public void TestIncompleteIntegerTag() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x02, 0x04, 0x12, 0x34, 0x56 }
            )) {
                BERReader reader = new BERReader(mem);
                BigInteger n;
                Assert.False(reader.ReadInteger(out n));
            }
        }

        [Test]
        public void TestNonIntegerTag() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x30, 0x03, 0x02, 0x01, 0x01 }
            )) {
                BERReader reader = new BERReader(mem);
                BigInteger n;
                Assert.False(reader.ReadInteger(out n));
            }
        }

        [Test]
        public void TestIntegerTag1() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x02, 0x01, 0xa3 }
            )) {
                BERReader reader = new BERReader(mem);
                BigInteger n;
                Assert.True(reader.ReadInteger(out n));
                Assert.AreEqual("163", n.ToString());
            }
        }

        [Test]
        public void TestIntegerTag3() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x02, 0x01, 0x00 }
            )) {
                BERReader reader = new BERReader(mem);
                BigInteger n;
                Assert.True(reader.ReadInteger(out n));
                Assert.AreEqual("0", n.ToString());
            }
        }

        [Test]
        public void TestIntegerTag4() {
            using (MemoryStream mem = new MemoryStream(
                new byte[] { 0x02, 0x09, 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0, 0x01 }
            )) {
                BERReader reader = new BERReader(mem);
                BigInteger n;
                Assert.True(reader.ReadInteger(out n));
                Assert.AreEqual("123456789ABCDEF001", n.ToString(16));
            }
        }

    }

#endif

}
