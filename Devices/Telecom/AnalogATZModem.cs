using System;
using System.Collections.Generic;
using System.Text;
using Wiffzack.Communication;
using System.Xml;
using System.Threading;
using Wiffzack.Diagnostic.Log;
using Wiffzack.Printing.Client;
using Wiffzack.Client.DisplayUnit.Telecom;
using System.Windows.Forms;

namespace Deveck.Utils.Devices.Telecom
{
    /// <summary>
    /// Implementiert das ITelecom Interface für Analoge Modems 
    /// die den ATZ satz verstehen
    /// </summary>
    public class AnalogATZModem:ITelecom
    {
        #region ITelecom Member

        /// <summary>
        /// Wird aufgerufen wenn ein eingehender Anruf entdeckt wurde
        /// und die Caller Informationen gesendet wurden
        /// </summary>
        public event TelecomIncomingcallDelegate IncomingCall;

        /// <summary>
        /// Schnittstelle über die mit dem Modem kommuniziert wird
        /// </summary>
        private ICommunication _comm = null;

        /// <summary>
        /// Konfigurationsdaten
        /// </summary>
        private XmlElement _config = null;

        /// <summary>
        /// Empfangsbuffer
        /// </summary>
        private StringBuilder _receiveBuffer = new StringBuilder();

        /// <summary>
        /// Wird gesetzt wenn gerade ein Commando übertragen wird,
        /// und darauf gewartet wird bis die statusmeldung eintrifft
        /// </summary>
        private AutoResetEvent _transmittingCommand = new AutoResetEvent(false);

        /// <summary>
        /// Irgendjemand wartet auf eine statusmeldung
        /// </summary>
        private volatile bool _waitingForStatus = false;

        /// <summary>
        /// Gibt an ob der letzte empfangene Status "OK" oder "ERROR" war
        /// </summary>
        private volatile bool _lastStatusSuccessful = false;

        /// <summary>
        /// Es wurde ein RING empfangen aber noch kein zugehöriges NMBR
        /// </summary>
        private volatile bool _inRingStatus = false;

        /// <summary>
        /// Derzeitige Konfiguration in geparster form
        /// </summary>
        private AnalogATZModemConfiguration _modemConfiguration = null;

        private string _endpoint = "";

        /// <summary>
        /// Logger
        /// </summary>
        private Logger _logger = LogManager.Global.GetLogger("Wiffzack.Logger");

        /// <summary>
        /// Initialisiert das Modem
        /// </summary>
        /// <param name="comm"></param>
        /// <param name="config"></param>
        public void Initialize(ICommunication comm, XmlElement config, string endpoint)
        {
            _comm = comm;
            _config = config;
            _endpoint = endpoint;
            _modemConfiguration = new AnalogATZModemConfiguration(config);

            _comm.OnDataReceived += new OnDataReceivedDelegate(_comm_OnDataReceived);

            _logger.Info("Initialising Telecom Device '{0}'", endpoint);

            if (TransmitCommand("ATZ") == false ||
             TransmitCommand(_modemConfiguration.CommandInitializeCallerId) == false)
            {
                _logger.Fatal("Initialization of Telecom Device '{0}' failed!", endpoint);
            }
            else
                _logger.Info("Telecom Device '{0}' initialized", endpoint);
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
                Console.WriteLine("Received: {0}", received);
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
            //Manche Modems wollen jedes mal neu initialisiert werden
            bool result1 = TransmitCommand("ATZ");
            bool result2 = TransmitCommand(_modemConfiguration.CommandInitializeCallerId);

            if (result1 == false || result2 == false)
            {
                _logger.Fatal("Reinitialization of modem device failed");
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
        /// 
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
