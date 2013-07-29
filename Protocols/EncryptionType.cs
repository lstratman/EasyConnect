using System.Security.Cryptography;

namespace EasyConnect.Protocols
{
	/// <summary>
	/// Type of encryption that should be used to protect passwords and other sensitive data in settings files.
	/// </summary>
	public enum EncryptionType
	{
		/// <summary>
		/// A <see cref="Rijndael"/> object should be used to encrypt the data.
		/// </summary>
		Rijndael,

		/// <summary>
		/// A <see cref="RSACryptoServiceProvider"/> object should be used to encrypt the data.
		/// </summary>
		Rsa
	}
}