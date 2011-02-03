using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;
using Deveck.Utils.Factory;
using Deveck.Utils.Collections;
using System.Collections;

namespace Deveck.Utils.SimpleComm
{
    /// <summary>
    /// SimpleComm Rs232(serial) implementation
    /// </summary>
    /// <remarks>
    /// <para>Configuration: 
    /// <list type="">
    /// <item>port_name[string]: Specifies the name of the serial port to use</item>
    /// <item>baud_rate[int]: Specifies the baud_rate to use</item>
    /// <item>parity[Parity, default:none]: Specifies the parity to use</item>
    /// <item>stop_bits[StopBits, default:one]: Specifies the stop bits to use</item>
    /// <item>read_buffer[int, default:4096]: Specifies the size of the read buffer</item>
    /// <item>write_buffer[int, default:4096]: Specifies the size of the write buffer</item>
    /// </list>
    /// </para>
    /// </remarks>
    [ClassIdentifier("simplecomm/general/rs232")]
    public class SerialComm:ICommunication
    {
        private const int BUFFER_SIZE = 256;

        /// <summary>
        /// Auto Open the port on setup
        /// </summary>
        private bool _autoOpen = true;

        public bool AutoOpen
        {
            get { return _autoOpen; }
            set { _autoOpen = value; }
        }

        private struct StateObj
        {
            public byte[] data;
        }

        private SerialPort _port = null;

        private IDictionary _configuration;


        public SerialComm()
        {
        }

        public SerialComm(IDictionary configuration)
        {
            _configuration = configuration;
            LoadConfig();
        }
        
        /// <summary>
        /// Waits for data on the serial port
        /// </summary>
        private void StartRead()
        {
            try
            {
                
                lock (_port)
                {
                    StateObj state;
                    state.data = new byte[BUFFER_SIZE];

                    _port.BaseStream.BeginRead(state.data, 0, BUFFER_SIZE, new AsyncCallback(ReadCallback), state);
                }
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Some data has been read
        /// </summary>
        /// <param name="ar"></param>
        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                int read;
                lock (_port)
                    read = _port.BaseStream.EndRead(ar);

                StateObj state = (StateObj)ar.AsyncState;
                if (OnDataReceived != null)
                    OnDataReceived(state.data, read);

                StartRead();
            }
            //Connection closed
            catch (IOException)
            {
                if (OnConnectionClosed != null)
                    OnConnectionClosed(this);
                //_port.Close();
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// Hack for some devices that need some pins on a certain level.
        /// </summary>
        /// <param name="RTS"></param>
        /// <param name="DTR"></param>
        public void SetLines(bool RTS, bool DTR)
        {
            lock (_port)
            {
                _port.RtsEnable = RTS;
                _port.DtrEnable = DTR;
            }
        }

        #region ICommunication Members

        public event OnDataReceivedDelegate OnDataReceived;

        public event Action<ICommunication> OnConnectionEstablished;

        public event Action<ICommunication> OnConnectionClosed;

        public void SetupCommunication(IDictionary setup)
        {
            _configuration = setup;
            LoadConfig();           
        }


        public void LoadConfig()
        {
            _port = new SerialPort(CollectionHelper.ReadValue<string>(_configuration, "port_name"),
                CollectionHelper.ReadValue<int>(_configuration, "baud_rate"),
                CollectionHelper.ReadValue<Parity>(_configuration, "parity", Parity.None),
                CollectionHelper.ReadValue<int>(_configuration, "data_bits", 8),
                CollectionHelper.ReadValue<StopBits>(_configuration, "stop_bits", StopBits.One));


            _port.ReadBufferSize = Math.Min(4096, CollectionHelper.ReadValue<int>(_configuration, "read_buffer", 4096));
            _port.WriteBufferSize = Math.Min(4096, CollectionHelper.ReadValue<int>(_configuration, "write_buffer", 4096));

            if(_autoOpen)
                Open();
        }


        public void SendData(byte[] data, int offset, int length)
        {
            lock (_port)
            {
                _port.BaseStream.Write(data, offset, length);
            }
        }


        public void Dispose()
        {
            if (_port != null && _port.IsOpen)
                _port.Close();
        }

        #endregion

        /// <summary>
        /// Manually opens the port (not part of ICommuniaction)
        /// </summary>
        public void Open()
        {
            _port.Open();

            if (OnConnectionEstablished != null)
                OnConnectionEstablished(this);

            StartRead();
        }

        /// <summary>
        /// Manually closes the port (not part of ICommunication)
        /// </summary>
        public void Close()
        {
            if (_port != null && _port.IsOpen)
            {
                lock(_port)
                    _port.Close();
            }
        }

    }
}
