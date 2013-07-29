using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyConnect.Common
{
	/// <summary>
	/// Helper methods for serializing and deserializing data.
	/// </summary>
	public class SerializationUtilities
	{
		/// <summary>
		/// Privatized constructor to prevent instances of this class from being created.
		/// </summary>
		private SerializationUtilities()
		{
		}

		/// <summary>
		/// Clones an object by serializing and deserializing it using <see cref="BinaryFormatter"/>.
		/// </summary>
		/// <param name="source">Object that we are to clone.</param>
		/// <returns>A new copy of <paramref name="source"/>.</returns>
		public static object Clone(object source)
		{
			using (MemoryStream bytes = new MemoryStream())
			{
				IFormatter formatter = new BinaryFormatter();

				formatter.Serialize(bytes, source);
				bytes.Position = 0;

				return formatter.Deserialize(bytes);
			}
		}

		/// <summary>
		/// Clones an object by serializing and deserializing it using <see cref="BinaryFormatter"/>.
		/// </summary>
		/// <typeparam name="T">Type of the object that we are to clone.</typeparam>
		/// <param name="source">Object that we are to clone.</param>
		/// <returns>A new copy of <paramref name="source"/>.</returns>
		public static T Clone<T>(T source)
		{
			return (T) Clone((object) source);
		}
	}

	/// <summary>
	/// Extension methods for <see cref="SerializationInfo"/>.
	/// </summary>
	public static class SerializationInfoExtensions
	{
		/// <summary>
		/// Strongly typed version of <see cref="SerializationInfo.GetValue"/>.
		/// </summary>
		/// <typeparam name="T">Type of the value that is to be returned.</typeparam>
		/// <param name="info">Serialization context that we are reading data from.</param>
		/// <param name="name">Name of the property to read from <paramref name="info"/>.</param>
		/// <returns>A strongly typed value for <paramref name="name"/>.</returns>
		public static T GetValue<T>(this SerializationInfo info, string name)
		{
			return (T) info.GetValue(name, typeof (T));
		}
	}
}