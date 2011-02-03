using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;

namespace Deveck.Utils.SimpleComm
{

    public delegate void OnDataReceivedDelegate(byte[] data, int length);


    /// <summary>
    /// A simple communication layer
    /// </summary>
    public interface ICommunication:IDisposable
    {
        
        /// <summary>
        /// Some data has been received
        /// </summary>
        event OnDataReceivedDelegate OnDataReceived;

        /// <summary>
        /// the connection has been established. Only occurs if the 
        /// target transport support "connecting" and "disconnecting"
        /// </summary>
        event Action<ICommunication> OnConnectionEstablished;

        /// <summary>
        /// the connection has been closed. Only occurs if the 
        /// target transport support "connecting" and "disconnecting"
        /// </summary>
        event Action<ICommunication> OnConnectionClosed;

        /// <summary>
        /// Sets up the communication
        /// </summary>
        /// <param name="setup"></param>
        void SetupCommunication(IDictionary setup);

        /// <summary>
        /// Sends data
        /// </summary>
        void SendData(byte[] data, int offset, int length);
    }
}
