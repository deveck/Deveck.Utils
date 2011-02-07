using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Deveck.Utils.Collections;
using System.Collections;

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

        public AnalogATZModemConfiguration(IDictionary config)
        {
            Load(config);
        }

        private void Load(IDictionary config)
        {
            Ring = CollectionHelper.ReadValue<string>(config, "Ring", Ring);
            ConfirmOk = CollectionHelper.ReadValue<string>(config, "ConfirmOk", ConfirmOk);
            ConfirmError = CollectionHelper.ReadValue<string>(config, "ConfirmError", ConfirmError);
            CommandInitializeCallerId = CollectionHelper.ReadValue<string>(config, "InitializeCallerIdCommand", CommandInitializeCallerId);
            InfoNumberField = CollectionHelper.ReadValue<string>(config, "InfoNumberField", InfoNumberField);
            SuppressedCallerId = CollectionHelper.ReadValue<string>(config, "InfoSuppressedCallerId", SuppressedCallerId);
            TransmitCommandTimeout = CollectionHelper.ReadValue<int>(config, "TransmitCommandTimeout", 1000);

        }
    }

}
