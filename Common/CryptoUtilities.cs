using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace EasyConnect.Common
{
    public class CryptoUtilities
    {
        private CryptoUtilities()
        {
        }

        public static byte[] Decrypt(SecureString key, byte[] data)
        {
            Rijndael encryption = Rijndael.Create();
            encryption.KeySize = 256;

            IntPtr marshalledKeyBytes = Marshal.SecureStringToGlobalAllocAnsi(key);
            byte[] keyBytes = new byte[encryption.KeySize / 8];
            byte[] decryptedData = new byte[data.Length];

            Marshal.Copy(marshalledKeyBytes, keyBytes, 0, Math.Min(keyBytes.Length, key.Length));

            encryption.Key = keyBytes;
            encryption.IV = Convert.FromBase64String("QGWyKbe+W9H0mL2igm73jw==");

            MemoryStream memoryStream = new MemoryStream(data, 0, data.Length);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryption.CreateDecryptor(),
                                                         CryptoStreamMode.Read);

            cryptoStream.Read(decryptedData, 0, decryptedData.Length);
            cryptoStream.Close();
            memoryStream.Close();

            for (int i = 0; i < keyBytes.Length; i++)
                keyBytes[i] = 0;

            Marshal.ZeroFreeGlobalAllocAnsi(marshalledKeyBytes);

            return decryptedData;
        }

        public static byte[] Encrypt(SecureString key, byte[] data)
        {
            Rijndael encryption = Rijndael.Create();
            encryption.KeySize = 256;

            IntPtr marshalledKeyBytes = Marshal.SecureStringToGlobalAllocAnsi(key);
            byte[] keyBytes = new byte[encryption.KeySize / 8];

            Marshal.Copy(marshalledKeyBytes, keyBytes, 0, Math.Min(keyBytes.Length, key.Length));

            encryption.Key = keyBytes;
            encryption.IV = Convert.FromBase64String("QGWyKbe+W9H0mL2igm73jw==");

            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryption.CreateEncryptor(),
                                                         CryptoStreamMode.Write);

            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.Close();
            memoryStream.Close();

            for (int i = 0; i < keyBytes.Length; i++)
                keyBytes[i] = 0;

            Marshal.ZeroFreeGlobalAllocAnsi(marshalledKeyBytes);

            return memoryStream.ToArray();
        }

        public static byte[] Encrypt(SecureString key, string data)
        {
            return Encrypt(key, Encoding.ASCII.GetBytes(data));
        }

        public static byte[] Encrypt(SecureString key, SecureString data)
        {
            IntPtr marshalledDataBytes = Marshal.SecureStringToGlobalAllocAnsi(data);
            byte[] dataBytes = new byte[data.Length];

            Marshal.Copy(marshalledDataBytes, dataBytes, 0, dataBytes.Length);

            byte[] encryptedData = Encrypt(key, dataBytes);

            for (int i = 0; i < dataBytes.Length; i++)
                dataBytes[i] = 0;

            Marshal.ZeroFreeGlobalAllocAnsi(marshalledDataBytes);

            return encryptedData;
        }
    }
}