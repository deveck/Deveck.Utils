using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Mommosoft.Capi;
using Deveck.Utils.Factory;
using System.Diagnostics;
using Deveck.Utils.SimpleComm;
using System.Collections;

namespace Deveck.Utils.Devices.Telecom
{
    /// <summary>
    /// Implements the ITelecom interface for CAPI devices
    /// </summary>
    /// <remarks>
    /// this implementation listens on all attached capi devices, but
    /// does not answer any calls, so the operation should not be affected in any way
    /// 
    /// Thanks go out to http://capi.codeplex.com/ for its great CAPI.NET library
    /// </remarks>
    [ClassIdentifier("telecom/win/capi")]
    public class CapiDevice : ITelecom
    {
        /// <summary>
        /// Capi Interface
        /// </summary>
        private CapiApplication _capiApplication;

        /// <summary>
        /// Configuration of the capi device
        /// </summary>
        private CapiDeviceConfiguration _configuration;

        #region ITelecom Members

        public event TelecomIncomingCallDelegate IncomingCall;

        public void Initialize(ICommunication comm, IDictionary config)
        {
            _configuration = new CapiDeviceConfiguration(config);

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
            Debug.WriteLine(string.Format("INFO: " + format, arguments));
        }

        private void LogWarnEntry(string format, params object[] arguments)
        {
            Debug.WriteLine(string.Format("WARN: " + format, arguments));
        }

        private void LogErrorEntry(string format, params object[] arguments)
        {
            Debug.WriteLine(string.Format("ERROR: " + format, arguments));
        }
    }
}
