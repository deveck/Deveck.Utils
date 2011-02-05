using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading;
using System.Windows.Forms;
using Deveck.Utils.SimpleComm;
using System.Collections;

namespace Deveck.Utils.Devices.Telecom
{
    /// <summary>
    /// Analog modem implementation of ITelecom
    /// </summary>
    public class AnalogATZModem:ITelecom
    {
        #region ITelecom Member

        /// <summary>
        /// Gets raised on an incoming call
        /// </summary>
        public event TelecomIncomingCallDelegate IncomingCall;

        private ICommunication _comm = null;

        /// <summary>
        /// configuration data
        /// </summary>
        private IDictionary _config = null;


        private StringBuilder _receiveBuffer = new StringBuilder();

        /// <summary>
        /// Is set on command response receive
        /// </summary>
        private AutoResetEvent _transmittingCommand = new AutoResetEvent(false);

        /// <summary>
        /// Someone is waiting for status
        /// </summary>
        private volatile bool _waitingForStatus = false;

        private volatile bool _lastStatusSuccessful = false;

        /// <summary>
        /// RING but no NMBR
        /// </summary>
        private volatile bool _inRingStatus = false;

        /// <summary>
        /// Derzeitige Konfiguration in geparster form
        /// </summary>
        private AnalogATZModemConfiguration _modemConfiguration = null;

        /// <summary>
        /// Initialisiert das Modem
        /// </summary>
        /// <param name="comm"></param>
        /// <param name="config"></param>
        public void Initialize(ICommunication comm, IDictionary config)
        {
            _comm = comm;
            _config = config;

            _comm.OnDataReceived += new OnDataReceivedDelegate(_comm_OnDataReceived);


            if (TransmitCommand("ATZ") == false ||
             TransmitCommand(_modemConfiguration.CommandInitializeCallerId) == false)
            {
                throw new Exception("Initialization of Telecom Device failed!");
            }
        }

        

        #endregion
        
        /// <summary>
        /// Es wurden Daten vom Modem empfangen
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        private void _comm_OnDataReceived(byte[] data, int length)
        {
            bool reinitialize = false;
            lock (_receiveBuffer)
            {
                string received = Encoding.ASCII.GetString(data, 0, length);
                _receiveBuffer.Append(received);
                reinitialize = CheckBuffer();
            }

            if (reinitialize)
            {
                MethodInvoker reinit = new MethodInvoker(Reinitialize);
                reinit.BeginInvoke(null, null);
            }
        }


        private void Reinitialize()
        {
            bool result1 = TransmitCommand("ATZ");
            bool result2 = TransmitCommand(_modemConfiguration.CommandInitializeCallerId);

            if (result1 == false || result2 == false)
            {
                throw new Exception("Reinitialization of modem device failed");
            }
        }

        /// <summary>
        /// Überprüft den Buffer auf gültige sequenzen
        /// </summary>
        private bool CheckBuffer()
        {
            string receiveBuffer = _receiveBuffer.ToString();

            if (receiveBuffer.Contains("\r") && receiveBuffer.Contains("\n") == false)
                receiveBuffer = receiveBuffer.Replace("\r", "\r\n");

            if (receiveBuffer.Contains("\n"))
            {
                string[] lines = receiveBuffer.Split('\n');
                _receiveBuffer.Remove(0, _receiveBuffer.Length);
                _receiveBuffer.Append(lines[lines.Length - 1]);

                for (int i = 0; i < lines.Length - 1; i++)
                {
                    string realLine = lines[i].Trim('\r');
                    realLine = realLine.Trim();
                    if (_waitingForStatus && realLine == _modemConfiguration.ConfirmOk)
                    {
                        _lastStatusSuccessful = true;
                        _transmittingCommand.Set();
                    }
                    else if (_waitingForStatus && realLine == _modemConfiguration.ConfirmError)
                    {
                        _lastStatusSuccessful = false;
                        _transmittingCommand.Set();
                    }
                    else if (_inRingStatus == false && realLine == _modemConfiguration.Ring)
                        _inRingStatus = true;
                    else if (lines[i].Contains("="))
                    {
                        string[] keyValue = realLine.Split('=');

                        if (keyValue.Length > 0)
                        {
                            for (int x = 0; x < keyValue.Length; x++)
                                keyValue[x] = keyValue[x].Trim();
                        }

                        if (keyValue.Length == 2)
                        {
                            if (keyValue[0] == _modemConfiguration.InfoNumberField && _inRingStatus)
                            {
                                TelecomIncomingInfo incomingInfo = new TelecomIncomingInfo(keyValue[1], keyValue[1] == _modemConfiguration.SuppressedCallerId);
                                _inRingStatus = false;

                                if (IncomingCall != null)
                                    IncomingCall(this, incomingInfo);

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Transmit command and wait for response
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private bool TransmitCommand(string command)
        {
            lock (_transmittingCommand)
            {
                _transmittingCommand.Reset();
                _waitingForStatus = true;
                _lastStatusSuccessful = false;
                _comm.SendData(Encoding.ASCII.GetBytes(command + "\r\n"), 0, command.Length + 2);
                _transmittingCommand.WaitOne(_modemConfiguration.TransmitCommandTimeout, false);
                _waitingForStatus = false;
                if (_lastStatusSuccessful)
                    return true;
                else
                    return false;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            lock (_receiveBuffer)
            {
                if(_receiveBuffer.Length > 0)
                    _receiveBuffer.Remove(0, _receiveBuffer.Length);

                _comm.Dispose();
            }
        }

        #endregion

  
        
    }
}
