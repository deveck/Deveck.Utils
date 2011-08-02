using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


namespace Deveck.Ui.Painters
{
    public class PainterFilterSize : Painter
    {
        public enum Alignment
        {
            Center,
            Near,
            Far
        }

        private Alignment _hAlign;
        private Alignment _vAlign;
        private int _paddingTop;
        private int _paddingRight;
        private int _paddingLeft;
        private int _paddingBottom;
        private int _maxWidth;

        // width/height = t
        private double _targetRatio;

        private Painter _subPainter;

        public PainterFilterSize(Painter subPainter, Alignment hAlign, Alignment vAlign, 
            int paddingTop, int paddingLeft, int paddingRight, int paddingBottom, 
            int maxWidth,
            double targetRatio)
        {
            _hAlign = hAlign;
            _vAlign = vAlign;
            _paddingTop = paddingTop;
            _paddingBottom = paddingBottom;
            _paddingLeft = paddingLeft;
            _paddingRight = paddingRight;

            _maxWidth = maxWidth;
            _targetRatio = targetRatio;
            _subPainter = subPainter;
        }
        public override void Paint(Graphics g, Rectangle position, Painter.State buttonState, string text, Image buttonImage, Font textFont, Rectangle? referencePosition)
        {
            Rectangle layoutArea = Rectangle.FromLTRB(position.X + _paddingLeft,
                position.Y + _paddingTop,
                position.Right - _paddingRight,
                position.Bottom - _paddingBottom);

            if (layoutArea.Width <= 0 || layoutArea.Height <= 0)
                return;

            double layoutRatio = (double)layoutArea.Width / (double)layoutArea.Height;

            Rectangle targetRect;
            //Breite maximieren
            if (layoutRatio < _targetRatio)
            {   
                int targetWidth = layoutArea.Width;

                if(_maxWidth > 0)
                    targetWidth = Math.Min(layoutArea.Width, _maxWidth);

                int targetHeight = (int)((double)targetWidth / _targetRatio);

                targetRect = new Rectangle(layoutArea.X, layoutArea.Y, targetWidth, targetHeight);

                
            }
            //Höhe maximieren
            else
            {
                int targetWidth = (int)((double)layoutArea.Height * _targetRatio);
                int targetHeight = layoutArea.Height;

                if (_maxWidth > 0 && targetWidth > _maxWidth)
                {
                    targetWidth = _maxWidth;
                    targetHeight = (int)((double)targetHeight / _targetRatio);
                }

                targetRect = new Rectangle(layoutArea.X, layoutArea.Y, targetWidth, targetHeight);

                
            }

            if (_vAlign == Alignment.Far)
                targetRect = new Rectangle(targetRect.X, layoutArea.Bottom - targetRect.Height, targetRect.Width, targetRect.Height);
            else if(_vAlign == Alignment.Center)
                targetRect = new Rectangle(targetRect.X, layoutArea.Top + (int)((double)layoutArea.Height / 2.0d - (double)targetRect.Height / 2.0d),
                    targetRect.Width, targetRect.Height);

            if (_hAlign == Alignment.Far)
                targetRect = new Rectangle(layoutArea.Right - targetRect.Width, targetRect.Y, targetRect.Width, targetRect.Height);
            else if(_hAlign == Alignment.Center)
                targetRect = new Rectangle(targetRect.X + (int)((double)layoutArea.Width / 2.0d - (double)targetRect.Width / 2.0d),
                    targetRect.Y, targetRect.Width, targetRect.Height);

            _subPainter.Paint(g, targetRect, buttonState, text, buttonImage, textFont, referencePosition);
        }
    }
}
