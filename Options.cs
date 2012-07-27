using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using EasyConnect.Protocols;

namespace EasyConnect
{
    [Serializable]
    public class Options
    {
        public bool AutoHideToolbar
        {
            get;
            set;
        }

        public static Options Load()
        {
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml"))
                return new Options();

            using (XmlReader reader = new XmlTextReader(new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml", FileMode.Open)))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Options));
                return (Options)serializer.Deserialize(reader);
            }
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(GetType());
            
            using (FileStream fileStream = new FileStream(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml", FileMode.Create))
            {
                serializer.Serialize(fileStream, this);
            }
        }
    }
}
