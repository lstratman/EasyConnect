using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using EasyConnect.Protocols.Rdp;

namespace EasyConnect
{
    public class Options
    {
        public Options()
        {
            RdpDefaults = new RdpConnection();
        }

        public RdpConnection RdpDefaults
        {
            get;
            set;
        }

        public static Options Load()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Options));

            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml"))
                return new Options();

            using (XmlReader optionsFileReader = new XmlTextReader(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml"))
            {
                return (Options)xmlSerializer.Deserialize(optionsFileReader);
            }
        }

        public void Save()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Options));

            using (XmlWriter optionsFileWriter = new XmlTextWriter(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\EasyConnect\\Options.xml", new UnicodeEncoding()))
            {
                xmlSerializer.Serialize(optionsFileWriter, this);
                optionsFileWriter.Flush();
            }
        }
    }
}
