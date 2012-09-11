using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace EasyConnect.Common
{
    /// <summary>
    /// Helper methods to encrypt and decrypt data using <see cref="Rijndael"/>.  Used for encrypting passwords for connections.
    /// </summary>
    public class CryptoUtilities
    {
        /// <summary>
        /// Privatized constructor to prevent instances of this class from being created.
        /// </summary>
        private CryptoUtilities()
        {
        }

        /// <summary>
        /// Decrypts <paramref name="data"/> by using the password bytes specified in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Password used to encrypt the data.</param>
        /// <param name="data">Data that we are to decrypt.</param>
        /// <returns>Decrypted data.</returns>
        public static byte[] Decrypt(SecureString key, byte[] data)
        {
            Rijndael encryption = Rijndael.Create();
            encryption.KeySize = 256;

            // Get the bytes for the password
            IntPtr marshalledKeyBytes = Marshal.SecureStringToGlobalAllocAnsi(key);
            byte[] keyBytes = new byte[encryption.KeySize / 8];
            byte[] decryptedData = new byte[data.Length];

            Marshal.Copy(marshalledKeyBytes, keyBytes, 0, Math.Min(keyBytes.Length, key.Length));

            // Set the encryption key to the key bytes and the IV to a predetermined string
            encryption.Key = keyBytes;
            encryption.IV = Convert.FromBase64String("QGWyKbe+W9H0mL2igm73jw==");

            MemoryStream memoryStream = new MemoryStream(data, 0, data.Length);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryption.CreateDecryptor(), CryptoStreamMode.Read);

            cryptoStream.Read(decryptedData, 0, decryptedData.Length);
            cryptoStream.Close();
            memoryStream.Close();

            // Clear the key bytes from memory
            for (int i = 0; i < keyBytes.Length; i++)
                keyBytes[i] = 0;

            Marshal.ZeroFreeGlobalAllocAnsi(marshalledKeyBytes);

            return decryptedData;
        }

        /// <summary>
        /// Encrypts <paramref name="data"/> by using the password bytes specified in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Password used to encrypt the data.</param>
        /// <param name="data">Data that we are to encrypt.</param>
        /// <returns>Encrypted data.</returns>
        public static byte[] Encrypt(SecureString key, byte[] data)
        {
            Rijndael encryption = Rijndael.Create();
            encryption.KeySize = 256;

            // Get the bytes for the password
            IntPtr marshalledKeyBytes = Marshal.SecureStringToGlobalAllocAnsi(key);
            byte[] keyBytes = new byte[encryption.KeySize / 8];

            Marshal.Copy(marshalledKeyBytes, keyBytes, 0, Math.Min(keyBytes.Length, key.Length));

            // Set the encryption key to the key bytes and the IV to a predetermined string
            encryption.Key = keyBytes;
            encryption.IV = Convert.FromBase64String("QGWyKbe+W9H0mL2igm73jw==");

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryption.CreateEncryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.Close();
            memoryStream.Close();

            // Clear the key bytes from memory
            for (int i = 0; i < keyBytes.Length; i++)
                keyBytes[i] = 0;

            Marshal.ZeroFreeGlobalAllocAnsi(marshalledKeyBytes);

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Encrypts a password specified in <paramref name="data"/> by using the password bytes specified in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Password used to encrypt the data.</param>
        /// <param name="data">Data that we are to encrypt.</param>
        /// <returns>Encrypted data.</returns>
        public static byte[] Encrypt(SecureString key, SecureString data)
        {
            IntPtr marshalledDataBytes = Marshal.SecureStringToGlobalAllocAnsi(data);
            byte[] dataBytes = new byte[data.Length];

            Marshal.Copy(marshalledDataBytes, dataBytes, 0, dataBytes.Length);

            byte[] encryptedData = Encrypt(key, dataBytes);

            // Clear the data bytes from memory
            for (int i = 0; i < dataBytes.Length; i++)
                dataBytes[i] = 0;

            Marshal.ZeroFreeGlobalAllocAnsi(marshalledDataBytes);

            return encryptedData;
        }
    }
}