using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Deveck.Utils.Factory;

namespace Deveck.Utils.SimpleComm
{

    /// <summary>
    /// SimpleComm TCP implementation
    /// </summary>
    [ClassIdentifier("simplecomm/tcp")]
    public class NetworkComm:ICommunication
    {
        private const int BUFFER_SIZE = 1024;

        private struct StateObj
        {
            public byte[] data;
        }


        private TcpListener _listener = null;
        private TcpClient _client = null;

        /// <summary>
        /// Startet den TCP Listener
        /// </summary>
        private void StartListener()
        {
            _listener = new TcpListener(IPAddress.Parse(_config.ConnectionSettings.LocalIP), 
                _config.ConnectionSettings.Port);
            _listener.Start();
        }


        /// <summary>
        /// Akzeptiert die nächste eingehende Verbindung
        /// </summary>
        private void AcceptNextConnection()
        {
            _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), null);
        }

        /// <summary>
        /// Callback das aufgerufen wird wenn ein Client verbunden hat
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallback(IAsyncResult ar)
        {
            _client = _listener.EndAcceptTcpClient(ar);
            _listener.Stop();
            if (OnConnectionEstablished != null)
                OnConnectionEstablished(this);

            StartRead();

        }


        /// <summary>
        /// Wartet auf neue Daten am Stream
        /// </summary>
        private void StartRead()
        {
            lock (_client)
            {
                StateObj state;
                state.data = new byte[BUFFER_SIZE];
                _client.GetStream().BeginRead(state.data, 0, BUFFER_SIZE, new AsyncCallback(ReadCallback), state);
            }
        }

        /// <summary>
        /// Wird aufgerufen wenn neue Daten vom Netzwerk Stream gelesen worden sind
        /// </summary>
        /// <param name="ar"></param>
        private void ReadCallback(IAsyncResult ar)
        {
           
            StateObj state = (StateObj)ar.AsyncState;

            int read;
            lock(_client)
                read = _client.GetStream().EndRead(ar);

            if (read == 0)
            {
                lock(_client)
                    _client.Close();

                if (OnConnectionClosed != null)
                    OnConnectionClosed(this);

                _listener.Start();
                AcceptNextConnection();
            }
            else
            {
                if (OnDataReceived != null)
                    OnDataReceived(state.data, read);

                StartRead();
            }
        }

        #region ICommunication Members

        public event OnDataReceivedDelegate OnDataReceived;
        public event Action<ICommunication> OnConnectionEstablished;
        public event Action<ICommunication> OnConnectionClosed;
        
        public void SetupCommunication(System.Xml.XmlElement setup)
        {
            _config.ConnectionSettings = new NetworkConfig.ConnectionSettingsConfig();
            _config.ConnectionSettings.Load(setup);

            StartListener();
            AcceptNextConnection();


        }

        public void SendData(byte[] data, int offset, int length)
        {
            if (_client == null) return;

            lock (_client)
            {
                if (_client != null && _client.Connected)
                    _client.GetStream().Write(data, offset, length);
            }
            
        }       

        public void Dispose()
        {
            if (_client != null && _client.Connected)
                _client.Close();

            _listener.Stop();
            
        }

        #endregion
    }
}
