using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Deveck.Ui.Painters
{
    /// <summary>
    /// Chain painters. They get called in the same order than they are added
    /// </summary>
    public class StackedPainters : Painter
    {
        private List<Painter> _painters;

        public StackedPainters(params Painter[] painters)
        {
            _painters = new List<Painter>(painters);
        }

        public override void Paint(Graphics g, Rectangle position, Painter.State buttonState, string text, Image buttonImage, Font textFont, Rectangle? referencePosition)
        {
            foreach (Painter p in _painters)
                p.Paint(g, position, buttonState, text, buttonImage, textFont, referencePosition);

        }
    }
}
