using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Deveck.Ui.Painters
{
    public class SymbolPainter : Painter
    {
        public static Painter Create(Painter backgroundPainter, SymbolEnum symbol, bool fill, int penWidth, Color forecolor, Color hoverColor, Color clickColor)
        {
            return new StackedPainters(
                new PainterFilterNoText(backgroundPainter),
                new SymbolPainter(symbol, fill, penWidth, forecolor, hoverColor, clickColor));
        }

        public enum SymbolEnum
        {
            /// <summary>
            /// Zeichnet ein Dreieck dass nach unten ausgefüllt ist
            /// </summary>
            TriangleDown,

            /// <summary>
            /// Zeichnet ein Dreieck nach oben
            /// </summary>
            TriangleUp,

            /// <summary>
            /// Zeichnet 3 horizontale Linien "Grip" zum anpacken mit der Maus
            /// </summary>
            GripH
        }

        private bool _fill;
        private SymbolEnum _symbol;
        private int _penWidth;
        private Color _color;
        private Pen _pen;
        private Brush _fillBrush;
        private Pen _hoverPen;
        private Pen _clickPen;

        private List<SymbolEnum> _noFill = new List<SymbolEnum>(new SymbolEnum[]{
            SymbolEnum.GripH
        });
            
        public SymbolPainter(SymbolEnum symbol, bool fill, int penWidth, Color forecolor, Color hoverPen, Color clickPen)
        {
            _symbol = symbol;
            _fill = fill;
            _penWidth = penWidth;
            _color = forecolor;
            _hoverPen = new Pen(hoverPen, penWidth);
            _clickPen = new Pen(clickPen, penWidth);
            _pen = new Pen(_color, penWidth);
            _fillBrush = new SolidBrush(forecolor);

            

        }

        private GraphicsPath BuildTriangleDown(Rectangle bounds)
        {
            int xPadding = bounds.Width/10;
            int yPadding = bounds.Height/10;
            int triangleHalf = Math.Max(0, bounds.Width /2 - xPadding);

            GraphicsPath triangle = new GraphicsPath();
            triangle.AddLine(bounds.Left + xPadding, bounds.Top + yPadding, bounds.Left + xPadding + 2 * triangleHalf, bounds.Top + yPadding);
            triangle.AddLine(bounds.Left + xPadding + 2 * triangleHalf, bounds.Top + yPadding, bounds.Left + xPadding + triangleHalf, bounds.Bottom - yPadding);
            triangle.CloseAllFigures();
            return triangle;
        }

        private GraphicsPath BuildTriangleUp(Rectangle bounds)
        {
            int xPadding = bounds.Width / 10;
            int yPadding = bounds.Height / 10;
            int triangleHalf = Math.Max(0, bounds.Width / 2 - xPadding);

            GraphicsPath triangle = new GraphicsPath();
            triangle.AddLine(bounds.Left + xPadding, bounds.Bottom - yPadding, bounds.Left + xPadding + 2 * triangleHalf, bounds.Bottom - yPadding);
            triangle.AddLine(bounds.Left + xPadding + 2 * triangleHalf, bounds.Bottom - yPadding, bounds.Left + xPadding + triangleHalf, bounds.Top + yPadding);
            triangle.CloseAllFigures();
            return triangle;
        }

        private GraphicsPath BuildGripH(Rectangle bounds)
        {
            int xPadding = bounds.Width / 10;
            int yPadding = bounds.Height / 10;
            

            int half = (int)((double)(bounds.Height - 2*yPadding) / 2.0);

            GraphicsPath grip = new GraphicsPath();
            grip.AddLine(bounds.Left + xPadding, bounds.Top + yPadding, bounds.Right - xPadding, bounds.Top + yPadding);
            grip.StartFigure();
            grip.AddLine(bounds.Left + xPadding, bounds.Top + yPadding + half, bounds.Right - xPadding, bounds.Top + yPadding+ half);
            grip.StartFigure();
            grip.AddLine(bounds.Left + xPadding, bounds.Top + yPadding + 2*half, bounds.Right - xPadding, bounds.Top + yPadding + 2*half);

            return grip;
        }

        public override void Paint(Graphics g, Rectangle position, Painter.State buttonState, string text, Image buttonImage, Font textFont, Rectangle? referencePosition)
        {
            GraphicsPath path;

            Pen myPen = _pen;

            if (buttonState == State.Hover)
                myPen = _hoverPen;
            else if (buttonState == State.Pressed)
                myPen = _clickPen;

            if (_symbol == SymbolEnum.TriangleDown)
                path = BuildTriangleDown(position);
            else if (_symbol == SymbolEnum.TriangleUp)
                path = BuildTriangleUp(position);
            else if (_symbol == SymbolEnum.GripH)
                path = BuildGripH(position);
            else
                throw new NotImplementedException("Symbol not implemented");

            if (_fill && _noFill.Contains(_symbol) == false)
            {
                g.FillPath(_fillBrush, path);

                if (buttonState == State.Hover)
                    g.DrawPath(myPen, path);
                else if (buttonState == State.Pressed)
                    g.DrawPath(myPen, path);
            }
            else
                g.DrawPath(myPen, path);
        }
    }
}
