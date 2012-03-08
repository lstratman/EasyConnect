using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace EasyConnect
{
    public class SerializationHelper
    {
        private SerializationHelper()
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
