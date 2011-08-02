using System;
using System.Collections.Generic;
using System.Text;

namespace Deveck.Ui.Controls.Scrollbar
{
    public class ScrollbarCollector : ICustomScrollbar
    {
        private List<ICustomScrollbar> _attachedScrollbars = new List<ICustomScrollbar>();
        private bool _disableChangeEvents = false;

        public ScrollbarCollector(params ICustomScrollbar[] attachedScrollbars)
        {
            _attachedScrollbars.AddRange(attachedScrollbars);

            foreach(ICustomScrollbar scrollbar in _attachedScrollbars)
                scrollbar.ValueChanged += new ScrollValueChangedDelegate(scrollbar_ValueChanged);

        }

        private void scrollbar_ValueChanged(ICustomScrollbar sender, int newValue)
        {
            if (_disableChangeEvents) return;

            _disableChangeEvents = true;
            foreach (ICustomScrollbar scrollbar in _attachedScrollbars)
            {
                if (scrollbar != sender)
                    scrollbar.Value = newValue;
            }
            _disableChangeEvents = false;
            _value = newValue;
            if (ValueChanged != null)
                ValueChanged(this, newValue);
        }
        #region ICustomScrollbar Members

        public event ScrollValueChangedDelegate ValueChanged;
        

        private int _largeChange = 10;
        public int LargeChange
        {
            get { return _largeChange; }
            set
            {
                _largeChange = value;
                foreach (ICustomScrollbar attachedScrollbar in _attachedScrollbars)
                    attachedScrollbar.LargeChange = value;
            }
        }

        private int _smallChange = 1;
        public int SmallChange
        {
            get { return _smallChange; }
            set
            {
                _smallChange = value;
                foreach (ICustomScrollbar attachedScrollbar in _attachedScrollbars)
                    attachedScrollbar.SmallChange = value;
            }
        }

        private int _maximum = 99;
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                _maximum = value;
                foreach (ICustomScrollbar attachedScrollbar in _attachedScrollbars)
                    attachedScrollbar.Maximum = value;
            }
        }

        private int _minimum = 0;
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;
                foreach (ICustomScrollbar attachedScrollbar in _attachedScrollbars)
                    attachedScrollbar.Minimum = value;
            }
        }

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                _value = value;
                foreach (ICustomScrollbar attachedScrollbar in _attachedScrollbars)
                    attachedScrollbar.Value = value;
            }
        }

        #endregion
    }
}
