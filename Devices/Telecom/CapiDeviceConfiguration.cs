using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using Deveck.Utils.Collections;

namespace Deveck.Utils.Devices.Telecom
{
    public class CapiDeviceConfiguration
    {
        /// <summary>
        /// Local numbers get sent withoput leading zeros, set this to your usual local number identifier
        /// </summary>
        public string PrependLocalNumber = "0";


        public CapiDeviceConfiguration()
        {
        }

        public CapiDeviceConfiguration(IDictionary config)
        {
            Load(config);
        }

        private void Load(IDictionary config)
        {
            PrependLocalNumber = CollectionHelper.ReadValue<string>(config, "PrependLocalNumber", "0");
        }
      
    }

}
