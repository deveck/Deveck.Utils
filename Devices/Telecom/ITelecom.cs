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
        /// Incoming call detected
        /// </summary>
        event TelecomIncomingCallDelegate IncomingCall;

        /// <summary>
        /// Initialize the incoming-call-provider
        /// </summary>
        /// <param name="comm"></param>
        /// <param name="config"></param>
        void Initialize(ICommunication comm, IDictionary config);

    }
}
