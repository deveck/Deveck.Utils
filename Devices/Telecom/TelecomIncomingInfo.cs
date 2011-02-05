using System;
using System.Collections.Generic;
using System.Text;

namespace Deveck.Utils.Devices.Telecom
{
    public class TelecomIncomingInfo
    {
        private DateTime _receiveTime = DateTime.Now;
        private bool _suppressed = false;
        private string _identifier;

        public DateTime ReceiveTime
        {
            get { return _receiveTime; }
        }

        public string Identifier
        {
            get { return _identifier; }
        }

        public bool Suppressed
        {
            get { return _suppressed; }
        }

        public TelecomIncomingInfo(string identifier, bool suppressed)
        {
            _identifier = identifier;
            _suppressed = suppressed;
        }
    }
}
