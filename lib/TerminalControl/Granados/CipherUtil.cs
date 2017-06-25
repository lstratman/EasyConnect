/*
 Copyright (c) 2005-2016 Poderosa Project, All Rights Reserved.
 This file is a part of the Granados SSH Client Library that is subject to
 the license included in the distributed package.
 You may not use this file except in compliance with the license.
*/

using System;

namespace Granados.Crypto {
    /// <summary>
    /// 
    /// </summary>
    /// <exclude/>
    public class CipherUtil {
        //Little Endian
        internal static uint GetIntLE(byte[] src, int offset) {
            return ((uint)src[offset] |
                ((uint)(src[offset + 1]) << 8) |
                ((uint)(src[offset + 2]) << 16) |
                ((uint)(src[offset + 3]) << 24));
        }

        internal static void PutIntLE(uint val, byte[] dest, int offset) {
            dest[offset] = (byte)(val & 0xff);
            dest[offset + 1] = (byte)((val >> 8) & 0xff);
            dest[offset + 2] = (byte)((val >> 16) & 0xff);
            dest[offset + 3] = (byte)((val >> 24) & 0xff);
        }

        //Big Endian
        internal static uint GetIntBE(byte[] src, int offset) {
            return (((uint)(src[offset]) << 24) |
                ((uint)(src[offset + 1]) << 16) |
                ((uint)(src[offset + 2]) << 8) |
                ((uint)src[offset + 3]));
        }

        internal static void PutIntBE(uint val, byte[] dest, int offset) {
            dest[offset] = (byte)((val >> 24) & 0xff);
            dest[offset + 1] = (byte)((val >> 16) & 0xff);
            dest[offset + 2] = (byte)((val >> 8) & 0xff);
            dest[offset + 3] = (byte)(val & 0xff);
        }

        internal static void BlockXor(byte[] src, int s_offset, int len, byte[] dest, int d_offset) {
            for (; len > 0; len--)
                dest[d_offset++] ^= src[s_offset++];
        }

        public static int memcmp(byte[] d1, int o1, byte[] d2, int o2, int len) {
            for (int i = 0; i < len; i++) {
                byte b1 = d1[o1 + i];
                byte b2 = d2[o2 + i];
                if (b1 < b2)
                    return -1;
                else if (b1 > b2)
                    return 1;
            }
            return 0;
        }
    }

    /// <summary>
    /// Converter for the convenience of using the big integer.
    /// </summary>
    internal static class BigIntegerConverter {

        /// <summary>
        /// Parse string as the bit string form.
        /// </summary>
        /// <example>
        /// "10100110100" --> { 0x05, 0x34 }
        /// </example>
        /// <param name="s">a string consists of '0' and '1'</param>
        /// <returns>byte string</returns>
        /// <exception cref="ArgumentException">parse error</exception>
        public static byte[] ParseBinary(string s) {
            int bytes = (s.Length + 7) / 8;
            byte[] data = new byte[bytes];
            int dataIndex = 0;

            byte mask = (byte)(1 << ((s.Length - 1) % 8));
            byte b = 0;
            foreach (char ch in s) {
                switch (ch) {
                    case '1':
                        b |= mask;
                        break;
                    case '0':
                        break;
                    default:
                        throw new ArgumentException("not a binary string form");
                }
                mask >>= 1;
                if (mask == 0) {
                    data[dataIndex] = b;
                    b = 0;
                    mask = 0x80;
                    ++dataIndex;
                }
            }
            return data;
        }

        /// <summary>
        /// Parse string as the hexadecimal string form.
        /// </summary>
        /// <example>
        /// "a351f21" --> { 0x0a, 0x35, 0x1f, 0x21 }
        /// </example>
        /// <param name="s">a string consists of '0'..'9','a'..'f','A'..'F'</param>
        /// <returns>byte string</returns>
        /// <exception cref="ArgumentException">parse error</exception>
        public static byte[] ParseHex(string s) {
            int bytes = (s.Length + 1) / 2;
            byte[] data = new byte[bytes];
            int dataIndex = 0;

            bool highbits = (s.Length % 2 == 0);
            byte b = 0;
            foreach (char ch in s) {
                byte v;
                switch (ch) {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        v = (byte)(ch - '0');
                        break;
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                        v = (byte)(ch - 'a' + 10);
                        break;
                    case 'A':
                    case 'B':
                    case 'C':
                    case 'D':
                    case 'E':
                    case 'F':
                        v = (byte)(ch - 'A' + 10);
                        break;
                    default:
                        throw new ArgumentException("invalid hex number");
                }

                if (highbits) {
                    b |= (byte)(v << 4);
                    highbits = false;
                }
                else {
                    b |= v;
                    data[dataIndex] = b;
                    b = 0;
                    highbits = true;
                    ++dataIndex;
                }
            }
            return data;
        }
    }
}
