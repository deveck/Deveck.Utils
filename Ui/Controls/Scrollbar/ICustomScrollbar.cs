using System;
using System.Collections.Generic;
using System.Text;

namespace Deveck.Ui.Controls.Scrollbar
{
    public delegate void ScrollValueChangedDelegate(ICustomScrollbar sender, int newValue);

    public interface ICustomScrollbar
    {
        event ScrollValueChangedDelegate ValueChanged;

        int LargeChange { get; set; }
        int SmallChange { get; set; }

        int Maximum { get; set; }
        int Minimum { get; set; }        
        int Value { get; set; }
        
    }
}
