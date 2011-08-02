using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Deveck.Ui.Painters
{
    public  class WindowsStyledButtonPainter : Painter
    {
        /// <summary>
        /// Last drawn position,
        /// if position changes, brushed need to be recalculated
        /// </summary>
        protected Rectangle? _lastPosition = null;
        protected Painter.State? _lastState = null;

        /// <summary>
        /// Path of the upper shaded section
        /// </summary>
        protected GraphicsPath _upperGradientPath = null;
        protected Brush _upperGradientBrush = null;

        /// <summary>
        /// Bounding box of the upper gradient path
        /// </summary>
        protected Rectangle _upperGradientRect;


        protected Rectangle _leftBounds;
        protected Brush _leftBrush = null;

        protected Rectangle _middleBounds;
        protected Brush _middleBrush = null;

        protected Rectangle _rightBounds;
        protected Brush _rightBrush = null;

        protected Pen _borderPen = null;

        protected virtual void RecalcBrushes(Rectangle position, Painter.State state)
        {
            if (_lastPosition == null || _lastPosition.Value != position || _lastState == null ||_lastState.Value != state)
            {
                _lastState = state;
                if (state == State.Hover)
                    _borderPen = new Pen(Color.FromArgb(0x3c, 0x7f, 0xb1), 1);
                else if (state == State.Pressed)
                    _borderPen = new Pen(Color.FromArgb(0x18, 0x59, 0x8a), 1);
                else
                    _borderPen = new Pen(Color.FromArgb(0x9a, 0x9a, 0x9a), 1);

                _leftBounds = new Rectangle(position.X, position.Y, 8, position.Height);
                _leftBrush = CreateLeftBrush(_leftBounds, state);

                _middleBounds = new Rectangle(_leftBounds.Right, _leftBounds.Y, (int)((float)position.Width - 16), position.Height);
                _middleBrush = CreateMiddleBrush(_middleBounds, state);

                _rightBounds = new Rectangle(_middleBounds.Right, _leftBounds.Y, 8, position.Height);
                _rightBrush = CreateRightBrush(_rightBounds, state);

                _upperGradientPath = new GraphicsPath();
                _upperGradientPath.AddLine(position.X + 8, position.Y + 1, position.X + 8, position.Y + 5);
                _upperGradientPath.AddLine(position.X + 8, position.Y + 5, _middleBounds.Right, position.Y + 5);
                _upperGradientPath.AddLine(_middleBounds.Right, position.Y + 5, _rightBounds.Right - 1, position.Y + 1);
                _upperGradientPath.CloseAllFigures();

                _upperGradientRect = new Rectangle(position.X + 8, position.Y + 1, 10, 4);
                _upperGradientBrush = new LinearGradientBrush(_upperGradientRect, Color.FromArgb(0xed, 0xed, 0xed), Color.FromArgb(0xdd, 0xdd, 0xe0), LinearGradientMode.Vertical);

                _lastPosition = position;
            }
        }

        protected virtual Brush CreateLeftBrush(Rectangle bounds, Painter.State state)
        {
            if(state == State.Hover)
                return new LinearGradientBrush(bounds, Color.FromArgb(0xe3, 0xf4, 0xfc), Color.FromArgb(0xd6, 0xee, 0xfb), LinearGradientMode.Horizontal);
            else if (state == State.Pressed)
                return new LinearGradientBrush(bounds, Color.FromArgb(0xce, 0xed, 0xfa), Color.FromArgb(0xb5, 0xe4, 0xf7), LinearGradientMode.Horizontal);
            else
                return new LinearGradientBrush(bounds, Color.FromArgb(0xf5, 0xf5, 0xf5), Color.FromArgb(0xe9, 0xe9, 0xeb), LinearGradientMode.Horizontal);
        }

        protected virtual Brush CreateMiddleBrush(Rectangle bounds, Painter.State state)
        {
            if(state == State.Hover)
                return new SolidBrush(Color.FromArgb(0xa9, 0xdb, 0xf6));
            else if (state == State.Pressed)
                return new SolidBrush(Color.FromArgb(0x6f, 0xca, 0xf0));
            else
                return new SolidBrush(Color.FromArgb(0xd9, 0xda, 0xdc));
        }

        protected virtual Brush CreateRightBrush(Rectangle bounds, Painter.State state)
        {
            if(state == State.Hover)
                return new LinearGradientBrush(bounds, Color.FromArgb(0xa9, 0xdb, 0xf6), Color.FromArgb(0x9c, 0xca, 0xe3), LinearGradientMode.Horizontal);
            else if (state == State.Pressed)
                return new LinearGradientBrush(bounds, Color.FromArgb(0x6f, 0xca, 0xf0), Color.FromArgb(0x66, 0xba, 0xdd), LinearGradientMode.Horizontal);
            else
                return new LinearGradientBrush(bounds, Color.FromArgb(0xd5, 0xd5, 0xd8), Color.FromArgb(0xc0, 0xc0, 0xc4), LinearGradientMode.Horizontal);
        }


        public override void Paint(Graphics g, Rectangle position, Painter.State state, string text, Image buttonImage, Font textFont, Rectangle? referencePosition)
        {
            if (position.Height < 10 || position.Width < 20)
                return;

            RecalcBrushes(position, state);

            g.FillRectangle(_leftBrush, _leftBounds);
            g.FillRectangle(_middleBrush, _middleBounds);
            g.FillRectangle(_rightBrush, _rightBounds);
            DrawBorder(g, position, state);

            g.FillPath(_upperGradientBrush, _upperGradientPath);
        }

        public virtual void DrawBorder(Graphics g, Rectangle position, Painter.State state)
        {
            g.DrawRectangle(_borderPen, new Rectangle(position.X, position.Y, (int)(position.Width - _borderPen.Width), (int)(position.Height - _borderPen.Width)));
        }
    }
}
