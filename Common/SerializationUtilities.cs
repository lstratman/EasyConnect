using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace EasyConnect.Common
{
    public class SerializationUtilities
    {
        private SerializationUtilities()
        {
        }

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

        public static T Clone<T>(T source)
        {
            return (T)Clone((object)source);
        }
    }

    public static class SerializationInfoExtensions
    {
        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T) info.GetValue(name, typeof (T));
        }
    }
}
