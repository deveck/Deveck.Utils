using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Deveck.Utils.SimpleComm;
using System.Collections;

namespace Deveck.Utils.Devices.Telecom
{
    public delegate void TelecomIncomingCallDelegate(ITelecom sender, TelecomIncomingInfo info);

    
    /// <summary>
    /// Enables communication with modems or other incoming-call-devices
    /// </summary>
    public interface ITelecom:IDisposable
    {
        /// <summary>
        /// Ein eingehender Anruf wurde endeckt
        /// </summary>
        event TelecomIncomingCallDelegate IncomingCall;

        /// <summary>
        /// Initialisiert und startet den Provider
        /// </summary>
        /// <param name="comm"></param>
        /// <param name="config"></param>
        void Initialize(ICommunication comm, IDictionary config, string endpoint);

    }
}
