/*
 Copyright (c) 2015 Poderosa Project, All Rights Reserved.
 This file is a part of the Granados SSH Client Library that is subject to
 the license included in the distributed package.
 You may not use this file except in compliance with the license.
*/
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Granados {

    /// <summary>
    /// Random number generator
    /// </summary>
    public interface Rng {
        /// <summary>
        /// Fills an array of bytes with random values.
        /// </summary>
        /// <param name="data">array to fill with random values.</param>
        void GetBytes(byte[] data);

        /// <summary>
        /// Returns a random number between 0 and maxValue-1
        /// </summary>
        int GetInt(int maxValue);
    }

    /// <summary>
    /// Random number generation utility
    /// </summary>
    public static class RngManager {

#if true
        // for .Net 2.0
        [ThreadStatic]
#endif
        private static RNGCryptoServiceProvider _coreRng;

        /// <summary>
        /// Get a new instance which implements Rng using <see cref="System.Random"/>
        /// </summary>
        /// <remarks>An instance returned should be used in the same thread.</remarks>
        /// <returns>a new instance</returns>
        public static Rng GetSystemRandomRng() {
            return new SystemRandomRng();
        }

        /// <summary>
        /// Get a new instance which implements Rng using <see cref="System.Random"/>
        /// </summary>
        /// <remarks>An instance returned should be used in the same thread.</remarks>
        /// <returns>a new instance</returns>
        public static Rng GetSecureRng() {
            if (_coreRng == null) {
                _coreRng = new RNGCryptoServiceProvider();
            }
            return new SecureRng(_coreRng);
        }

        /// <summary>
        /// implementation of Rng
        /// </summary>
        /// <remarks>An instance returned should be used in the same thread.</remarks>
        private class SecureRng : Rng {

            private readonly RNGCryptoServiceProvider _rng;

            public SecureRng(RNGCryptoServiceProvider rng) {
                _rng = rng;
            }

            public void GetBytes(byte[] data) {
                _rng.GetBytes(data);
            }

            public int GetInt(int maxValue) {
                if (maxValue < 0) {
                    throw new ArgumentOutOfRangeException("maxValue");
                }
                byte[] rbits = new byte[4];
                GetBytes(rbits);
                uint r = BitConverter.ToUInt32(rbits, 0);
                return (int)((((long)r) * maxValue) >> 32);
            }
        }

        /// <summary>
        /// implementation of Rng
        /// </summary>
        private class SystemRandomRng : Rng {

            private readonly Random _random = new Random();

            public void GetBytes(byte[] data) {
                _random.NextBytes(data);
            }

            public int GetInt(int maxValue) {
                return _random.Next(maxValue);
            }
        }
    }
}

namespace Granados.Util {

    /// <summary>
    /// A class provides pre-buffered random bytes.
    /// </summary>
    internal class SecureRandomBuffer {

        private readonly byte[] _buffer = new byte[256];
        private int _remains = 0;

        private static readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();
        private static readonly ThreadLocal<SecureRandomBuffer> _threadLocalInstance
            = new ThreadLocal<SecureRandomBuffer>(() => new SecureRandomBuffer());

        /// <summary>
        /// Get pre-buffered random bytes.
        /// </summary>
        /// <param name="buff">an array that random bytes are copied to.</param>
        /// <param name="offset">start index of the array.</param>
        /// <param name="length">number of bytes to be copied.</param>
        public static void GetRandomBytes(byte[] buff, int offset, int length) {
            _threadLocalInstance.Value.GetRandomBytesInternal(buff, offset, length);
        }

        /// <summary>
        /// Get pre-buffered random bytes.
        /// </summary>
        /// <param name="buff">an array that random bytes are copied to.</param>
        public static void GetRandomBytes(byte[] buff) {
            _threadLocalInstance.Value.GetRandomBytesInternal(buff, 0, buff.Length);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private SecureRandomBuffer() {
        }

        /// <summary>
        /// Get pre-buffered random bytes.
        /// </summary>
        /// <param name="buff">an array that random bytes are copied to.</param>
        /// <param name="offset">start index of the array.</param>
        /// <param name="length">number of bytes to be copied.</param>
        private void GetRandomBytesInternal(byte[] buff, int offset, int length) {
            int idx = offset;
            int len = length;
            while (len > 0) {
                if (_remains <= 0) {
                    _rng.GetBytes(_buffer);
                    _remains = _buffer.Length;
                }
                int copyLen = Math.Min(_remains, len);
                Buffer.BlockCopy(_buffer, _buffer.Length - _remains, buff, idx, copyLen);
                idx += copyLen;
                len -= copyLen;
                _remains -= copyLen;
            }
        }
    }

}
