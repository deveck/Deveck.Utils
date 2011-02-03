using System;
using System.Collections.Generic;
using System.Text;
using Wiffzack.Telecom;
using Wiffzack.Communication;
using System.Xml;
using Mommosoft.Capi;
using Wiffzack.Diagnostic.Log;
using Wiffzack.Printing.Client;

namespace Deveck.Utils.Devices.Telecom
{
    /// <summary>
    /// Implementiert das ITelecom interface für CAPI (ISDN) Geräte
    /// </summary>
    /// <remarks>
    /// Standardmäßig versucht diese Implementation bei allen gefundenen CAPI geräten zuzuhören,
    /// es werden keine Anrufe entgegen genommen und sollten somit den Gesamtbetrieb nicht beeinflussen
    /// </remarks>
    public class CapiDevice : ITelecom
    {
        /// <summary>
        /// Interface zu CAPI
        /// </summary>
        private CapiApplication _capiApplication;

        /// <summary>
        /// Konfiguration des CAPIDevice
        /// </summary>
        private CapiDeviceConfiguration _configuration;

        /// <summary>
        /// Entpunkt um Logeinträge lesbarer zu machen
        /// </summary>
        private string _endpoint;

        /// <summary>
        /// Logger
        /// </summary>
        private Logger _logger = LogManager.Global.GetLogger("CapiDevice");

        #region ITelecom Members

        public event TelecomIncomingcallDelegate IncomingCall;

        public void Initialize(ICommunication comm, XmlElement config, string endpoint)
        {
            _configuration = new CapiDeviceConfiguration(config);
            _endpoint = endpoint;

            try
            {
                _capiApplication = new CapiApplication();
                _capiApplication.IncomingPhysicalConnection += new EventHandler<IncomingPhysicalConnectionEventArgs>(_capiApplication_IncomingPhysicalConnection);

                foreach (Controller ctrl in _capiApplication.Controllers)
                {
                    try
                    {
                        ctrl.Listen(CIPMask.Telephony | CIPMask.Telephony7KHZ | CIPMask.Audio7KHZ | CIPMask.Audio31KHZ);
                        LogInfoEntry("Listening on '{0}'", ctrl);
                    }
                    catch (Exception ex)
                    {
                        LogErrorEntry("Error listening on '{0}': {1}", ctrl, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _capiApplication = null;
                LogErrorEntry("Error on initializing: {0}", ex);
            }

            
        }

        private void _capiApplication_IncomingPhysicalConnection(object sender, IncomingPhysicalConnectionEventArgs e)
        {
            string callingNumber = e.Connection.CallingPartyNumber;
            if (callingNumber != null && callingNumber.Trim().Equals(string.Empty) == false)
            {
                if (!callingNumber.StartsWith("+") && !callingNumber.StartsWith(_configuration.PrependLocalNumber))
                    callingNumber = _configuration.PrependLocalNumber + callingNumber;

                RaiseIncomingCall(new TelecomIncomingInfo(callingNumber, false));
            }
            else
                RaiseIncomingCall(new TelecomIncomingInfo("", true));
                



        }

        private void RaiseIncomingCall(TelecomIncomingInfo info)
        {
            if (IncomingCall != null)
                IncomingCall(this, info);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            
        }

        #endregion

        private void LogInfoEntry(string format, params object[] arguments)
        {
            _logger.Info(_endpoint + ": " + format, arguments);
        }

        private void LogWarnEntry(string format, params object[] arguments)
        {
            _logger.Warning(_endpoint + ": " + format, arguments);
        }

        private void LogErrorEntry(string format, params object[] arguments)
        {
            _logger.Fatal(_endpoint + ": " + format, arguments);
        }
    }
}
