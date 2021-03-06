using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Deveck.Utils.Factory;
using System.Collections;
using Deveck.Utils.Collections;

namespace Deveck.Utils.SimpleComm
{
    /// <summary>
    /// SimpleComm HID (Human Interface Device) (keyboard input) implementation
    /// </summary>
    /// <remarks>
    /// <para>
    /// Configuration:
    /// lock_to [string, default: ""]: Locks the HIDComm to certain keyboard
    /// </para>
    /// </remarks>
    [ClassIdentifier("simplecomm/win/hid")]
    public class HIDComm : NativeWindow, ICommunication
    {

        /// <summary>
        /// RawInput handler 
        /// It's static because only a single RawInput handler is supported per application
        /// </summary>
        private static InputDevice _inputDevice = null;

        /// <summary>
        /// lokaler buffer
        /// </summary>
        private List<Keys> _buffer = new List<Keys>();


        /// <summary>
        /// Gibt an ob der alt key gerade gedr�ckt ist,
        /// so dass am numpad ascii codes eingegeben werden k�nnen
        /// </summary>
        private bool _isAsciiKeyPressed = false;

        private IDictionary _config = null;

        [DllImport("user32")]
        private static extern int ToAscii(
            int uVirtKey,
            int uScanCode,
            byte[] lpbKeyState,
            byte[] lpwTransKey,
            int fuState);

        [DllImport("user32")]
        private static extern int GetKeyboardState(byte[] pbKeyState);


        public HIDComm()
        {
            CreateParams cp = new CreateParams();
            this.CreateHandle(cp);
        }


        protected override void WndProc(ref Message m)
        {
            if (_inputDevice != null)
                _inputDevice.ProcessMessage(ref m);
            
            base.WndProc(ref m);
        }


        #region ICommunication Members

        public event OnDataReceivedDelegate OnDataReceived;

#pragma warning disable 067
        public event Action<ICommunication> OnConnectionEstablished;

        public event Action<ICommunication> OnConnectionClosed;
#pragma warning restore 067

        public void SetupCommunication(IDictionary setup)
        {
            _config = setup;

            if (_config == null)
                _config = new Hashtable();

            lock (typeof(HIDComm))
            {
                if (_inputDevice == null)
                {
                    _inputDevice = new InputDevice(this.Handle);
                    _inputDevice.EnumerateDevices();
                }
            }

            _inputDevice.KeyDown += new InputDevice.DeviceEventHandler(InternalOnRawKeyPress);
            _inputDevice.KeyUp += new InputDevice.DeviceEventHandler(InternalOnRawKeyReleased);
        }

        public void AddRawKeyToBuffer(Keys myKey)
        {
            if (myKey == Keys.Menu)
            {
                CheckBuffer();
                _isAsciiKeyPressed = false;
            }
            else if ((myKey & Keys.Modifiers) == Keys.None && _isAsciiKeyPressed == false)
            {
                if (OnDataReceived != null)
                {
                    byte b = ConvertToString(myKey);
                    if (b != 0)
                        OnDataReceived(new byte[] { b }, 1);
                }

            }
            else
                _buffer.Add(myKey);
        }

        private void InternalOnRawKeyPress(object sender, InputDevice.KeyControlEventArgs e)
        {
            Debug.WriteLine(string.Format("PRESS[{0}]: {1}", e.Keyboard.deviceName,
			                              ((Keys)e.Keyboard.key).ToString()));

            string lockTo = CollectionHelper.ReadValue<string>(_config, "lock_to", null);
            if (lockTo != null && e.Keyboard.deviceName != lockTo)
                return;

            if ((Keys)e.Keyboard.key == Keys.Menu)
            {

                _isAsciiKeyPressed = true;
                
                lock (_buffer)
                    _buffer.Clear();
            }

        }

        private void InternalOnRawKeyReleased(object sender, InputDevice.KeyControlEventArgs e)
        {
            Debug.WriteLine("RELEASE: " + ((Keys)e.Keyboard.key).ToString());

            string lockTo = CollectionHelper.ReadValue<string>(_config, "lock_to", null);
            if (lockTo != null && e.Keyboard.deviceName != lockTo)
                return;

            lock (_buffer)
            {
                Keys myKey = (Keys)e.Keyboard.key;
                if (myKey == Keys.Menu)
                {
                    CheckBuffer();
                    _isAsciiKeyPressed = false;
                }
                else if ((myKey & Keys.Modifiers) == Keys.None && _isAsciiKeyPressed == false)
                {
                    if (OnDataReceived != null)
                    {
                        byte b = ConvertToString(myKey); 
                        if(b != 0)
                            OnDataReceived(new byte[] { b }, 1);
                    }
                    
                }
                else
                    _buffer.Add((Keys)e.Keyboard.key);
            }

        }

        private void CheckBuffer()
        {
            byte[] data = ConvertToString(_buffer.ToArray());

            if (OnDataReceived != null && data.Length > 0)
                OnDataReceived(data, data.Length);

            _buffer.Clear();
        }

        private byte ConvertToString(Keys key)
        {
            byte[] keyState = new byte[256];
            GetKeyboardState(keyState);
            byte[] translated = new byte[2];


            ToAscii((int)key, 0, keyState, translated, 0);

            return translated[0];
        }

        private byte[] ConvertToString(Keys[] keys)
        {
            string result = "";

            foreach (Keys k in keys)
            {
                byte[] translated = new byte[2];
                /*int dllResult = */ToAscii((int)k, 0, new byte[256], translated, 0);

                result += Encoding.ASCII.GetString(translated, 0, 1);
            }
            byte b = 0;

            if (byte.TryParse(result, out b))
                return new byte[] { b };
            else
                return new byte[] { };
        }

        public void SendData(byte[] data, int offset, int length)
        {
			throw new NotSupportedException("HIDs do not support sending data");
        }

        #endregion

        public InputDevice.DeviceInfo[] Devices
        {
            get { return _inputDevice.Devices; }
        }


        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
