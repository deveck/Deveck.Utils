using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Deveck.Utils.Devices.Telecom
{
    public class AnalogATZModemConfiguration
    {
        public string Ring = "RING";
        public string ConfirmOk = "OK";
        public string ConfirmError = "ERROR";
        public string CommandInitializeCallerId = "AT#CID=1";
        public string InfoNumberField = "NMBR";
        public string SuppressedCallerId = "OUT OF ORDER";
        public int TransmitCommandTimeout = 1000;

        public AnalogATZModemConfiguration()
        {
        }

        public AnalogATZModemConfiguration(string data)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);
            Load(doc.DocumentElement);
        }

        public AnalogATZModemConfiguration(XmlElement config)
        {
            Load(config);
        }

        private void Load(XmlElement config)
        {
            Ring = ReadStringValue(config, "Ring", Ring);
            ConfirmOk = ReadStringValue(config, "ConfirmOk", ConfirmOk);
            ConfirmError = ReadStringValue(config, "ConfirmError", ConfirmError);
            CommandInitializeCallerId = ReadStringValue(config, "InitializeCallerIdCommand", CommandInitializeCallerId);
            InfoNumberField = ReadStringValue(config, "InfoNumberField", InfoNumberField);
            SuppressedCallerId = ReadStringValue(config, "InfoSuppressedCallerId", SuppressedCallerId);
            TransmitCommandTimeout = ReadIntValue(config, "TransmitCommandTimeout", 1000);

        }

        public XmlElement Save()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("Settings"));
            WriteValue(doc.DocumentElement, "Ring", Ring);
            WriteValue(doc.DocumentElement, "ConfirmOk", ConfirmOk);
            WriteValue(doc.DocumentElement, "ConfirmError", ConfirmError);
            WriteValue(doc.DocumentElement, "InitializeCallerIdCommand", CommandInitializeCallerId);
            WriteValue(doc.DocumentElement, "InfoNumberField", InfoNumberField);
            WriteValue(doc.DocumentElement, "InfoSuppressedCallerId", SuppressedCallerId);
            WriteValue(doc.DocumentElement, "TransmitCommandTimeout", TransmitCommandTimeout);
            return doc.DocumentElement;
        }


        private void WriteValue(XmlElement config, string elementName, object value)
        {
            XmlElement newNode = config.OwnerDocument.CreateElement(elementName);
            newNode.InnerText = value.ToString();
            config.AppendChild(newNode);
        }

        private string ReadStringValue(XmlElement config, string elementName, string defaultValue)
        {
            try
            {
                XmlElement node = (XmlElement)config.SelectSingleNode(elementName);

                if (node != null)
                {
                    return node.InnerText;
                }
                else
                    return defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        private int ReadIntValue(XmlElement config, string elementName, int defaultValue)
        {
            try
            {
                string value = ReadStringValue(config, elementName, defaultValue.ToString());

                return int.Parse(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }
    }

}
