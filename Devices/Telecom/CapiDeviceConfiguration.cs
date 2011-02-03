using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Wiffzack.Services.Utils;

namespace Deveck.Utils.Devices.Telecom
{
    public class CapiDeviceConfiguration
    {
        /// <summary>
        /// Inländische Nummern werden ohne führende '0' gesendet z.b. 6641234567,
        /// allerdings ist diese Anzeige vermutlich ISDN Modem abhängig. Sollte eine nummer
        /// Ohne führender 0 bzw ohne + kommen wird eine null (lokale Nummer) angehängt
        /// </summary>
        public string PrependLocalNumber = "0";


        public CapiDeviceConfiguration()
        {
        }

        public CapiDeviceConfiguration(string data)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);
            Load(doc.DocumentElement);
        }

        public CapiDeviceConfiguration(XmlElement config)
        {
            Load(config);
        }

        private void Load(XmlElement config)
        {
            PrependLocalNumber = XmlHelper.ReadString(config, "PrependLocalNumber");
        }

        public XmlElement Save()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("Settings"));
            XmlHelper.WriteString(doc.DocumentElement, "PrependLocalNumber", PrependLocalNumber);

            return doc.DocumentElement;
        }


       
    }

}
