using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Deveck.Utils.Factory;
using System.Collections;
using Deveck.Utils.Collections;

namespace Deveck.Utils.SimpleComm
{

    /// <summary>
    /// SimpleComm TCP Client/Server implementation. This is SimpleComm, so the server 
    /// only supports a sinlge connection and is just for prototyping and testing
    /// </summary>
    /// <remarks>
    /// <para>Configuration:
    /// <list type="">
    /// <item>local_ip [string, default:0.0.0.0]: specifies the local interface to listen on</item>
    /// <item>remote_ip [string, mandatory if in connection mode]: specifies the ip to connect to</item>
    /// <item>port [integer]: specifies the local port to listen on or the port to connect to</item>
    /// <item>listen [bool, default: true]: specifies if this instance should listen for connection[1] or if it should connect to the remote host</item>
    /// </list>
    /// </para>
    /// </remarks>
    [ClassIdentifier("simplecomm/general/tcp")]
    public class NetworkComm:ICommunication
    {
        private const int BUFFER_SIZE = 1024;

        private struct StateObj
        {
            public byte[] data;
        }


		private IDictionary _config = null;
		
        private TcpListener _listener = null;
        private TcpClient _client = null;

        /// <summary>
        /// Starts the tcp listener
        /// </summary>
        private void StartListener()
        {
            _listener = new TcpListener(IPAddress.Parse(CollectionHelper.ReadValue<string>(_config, "local_ip", "0.0.0.0")), 
                CollectionHelper.ReadValue<int>(_config, "port"));
            _listener.Start();
        }


        /// <summary>
        /// Accepts the next incoming connection
        /// </summary>
        private void AcceptNextConnection()
        {
            _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), null);
        }

        /// <summary>
        /// A client has connected
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
        /// Wait for some data
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
        /// Some data has been read
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
        
        public void SetupCommunication(IDictionary setup)
        {
			_config = setup;

			if(CollectionHelper.ReadValue<bool>(_config, "listen", true))
			{
            	StartListener();
            	AcceptNextConnection();
			}
			else
				Connect();
				

        }
		
		private void Connect()
		{
			_client = new TcpClient();
			_client.Connect(new IPEndPoint(IPAddress.Parse(
			                        CollectionHelper.ReadValue<string>(_config, "remote_ip")),
			                        CollectionHelper.ReadValue<int>(_config, "port")));
			
            if (OnConnectionEstablished != null)
                OnConnectionEstablished(this);

			StartRead();
		}

			
		/// <summary>
		///Send data to the other endpoint 
		/// </summary>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
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
