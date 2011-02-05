using System;
using System.Collections.Generic;
using System.Text;

namespace Deveck.Utils.Devices.Telecom
{
    public class TelecomDeviceInfo
    {
        /// <summary>
        /// Verbundenes Device
        /// </summary>
        private ITelecom _telecomDevice;
        
        /// <summary>
        /// Zugehörige Konfiguration
        /// </summary>
        private Wiffzack.Client.DisplayUnit.MyUnitConfig.TelecomInterfacesConfig.InterfaceConfig _interfaceConfig;

        /// <summary>
        /// Bereits eingegangene zwischengespeicherte anrufe auf diesem gerät
        /// </summary>
        private List<TelecomIncomingInfo> _incomingCallInfos = new List<TelecomIncomingInfo>();

        public ITelecom TelecomDevice
        {
            get { return _telecomDevice; }
        }

        public Wiffzack.Client.DisplayUnit.MyUnitConfig.TelecomInterfacesConfig.InterfaceConfig InterfaceConfig
        {
            get { return _interfaceConfig; }
        }

        public TelecomDeviceInfo(ITelecom telecomDevice, Wiffzack.Client.DisplayUnit.MyUnitConfig.TelecomInterfacesConfig.InterfaceConfig interfaceConfig)
        {
            _telecomDevice = telecomDevice;
            _interfaceConfig = interfaceConfig;
        }


        public TelecomIncomingInfo[] IncomingCallInfos
        {
            get { return _incomingCallInfos.ToArray(); }
        }

        /// <summary>
        /// Fügt einen eingehenden Anruf hinzu und speichert
        /// die derzeitigen Anrufe lokal
        /// </summary>
        /// <param name="info"></param>
        public void AddIncomingCall(TelecomIncomingInfo info)
        {
            lock (_incomingCallInfos)
            {
                _incomingCallInfos.Add(info);

                //Die Liste muss natürlich auf einige Nummern beschränkt sein,
                //sonst wächst sie über alle Grenzen hinweg
                if (_interfaceConfig.LocalQueueLength > 0 &&
                    _incomingCallInfos.Count > _interfaceConfig.LocalQueueLength)
                {
                    while (_incomingCallInfos.Count > _interfaceConfig.LocalQueueLength)
                        _incomingCallInfos.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Gibt an ob der angegebene Endpunkt von diesem Device bearbeitet werden kann
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public bool CanHandle(string endpoint)
        {
            return _interfaceConfig.Endpoint == endpoint;
        }

        
    }
}
