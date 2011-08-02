using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Deveck.Ui.Painters
{
    public class Office2007BlackButtonPainter : DoubleBrushPainter
    {

        protected override Color BorderColor(Painter.State state)
        {
            if (state == State.Pressed)
                return Color.FromArgb(0x8b, 0x76, 0x54);
            else
                return Color.FromArgb(0x96, 0x9f, 0xa3);
        }

        protected override Color FontColor(Painter.State state)
        {
            return Color.FromArgb(0x46, 0x46, 0x46);
        }

        protected override int BorderWidth(Painter.State state)
        {
            return 1;
        }

        protected override Brush UpperBrush(Painter.State state, Rectangle bounds)
        {
            if (state == State.Normal)
                return new LinearGradientBrush(bounds, Color.FromArgb(0xd6, 0xde, 0xdf), Color.FromArgb(0xdb, 0xe2, 0xe4), LinearGradientMode.Vertical);
            else if (state == State.Hover)
                return new LinearGradientBrush(bounds, Color.FromArgb(0xfe, 0xfa, 0xe5), Color.FromArgb(0xfb, 0xe0, 0x91), LinearGradientMode.Vertical);
            else
                return new LinearGradientBrush(bounds, Color.FromArgb(0xcc, 0x96, 0x66), Color.FromArgb(0xff, 0xaa, 0x46), LinearGradientMode.Vertical);
        }

        protected override Brush LowerBrush(Painter.State state, Rectangle bounds)
        {
            if (state == State.Normal)
                return new LinearGradientBrush(bounds, Color.FromArgb(0xce, 0xd5, 0xd7), Color.FromArgb(0xdf, 0xe4, 0xe6), LinearGradientMode.Vertical);
            else if (state == State.Hover)
                return new LinearGradientBrush(bounds, Color.FromArgb(0xfe, 0xd2, 0x53), Color.FromArgb(0xff, 0xe3, 0x97), LinearGradientMode.Vertical);
            else
                return new LinearGradientBrush(bounds, Color.FromArgb(0xff, 0x9c, 0x26), Color.FromArgb(0xff, 0xc0, 0x4b), LinearGradientMode.Vertical);

        }
    }

    public class Office2007BlueButtonPainter : Office2007BlackButtonPainter
    {
        protected override Brush UpperBrush(State state, Rectangle bounds)
        {
            if(state == State.Normal)
                return new LinearGradientBrush(bounds, Color.FromArgb(0xc8, 0xdb, 0xef), Color.FromArgb(0xc6, 0xda, 0xf3), LinearGradientMode.Vertical);
            else
                return base.UpperBrush(state, bounds);
        }

        protected override Brush LowerBrush(State state, Rectangle bounds)
        {
            if (state == State.Normal)
                return new LinearGradientBrush(bounds, Color.FromArgb(0xbd, 0xd1, 0xea), Color.FromArgb(0xce, 0xdf, 0xf5), LinearGradientMode.Vertical);
            else
                return base.LowerBrush(state, bounds);
        }
    }
}
