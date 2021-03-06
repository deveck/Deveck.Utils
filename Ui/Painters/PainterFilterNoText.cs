﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Deveck.Ui.Painters
{
    /// <summary>
    /// PAsses no text to the associated painter
    /// </summary>
    public class PainterFilterNoText : Painter
    {
        private Painter _subPainter;

        public PainterFilterNoText(Painter p)
        {
            _subPainter = p;
        }


        public override void Paint(Graphics g, Rectangle position, Painter.State buttonState, string text, Image buttonImage, Font textFont, Rectangle? referencePosition)
        {
            _subPainter.Paint(g, position, buttonState, "", buttonImage, textFont, referencePosition);
        }
    }
}
