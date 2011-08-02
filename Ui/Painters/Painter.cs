using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Deveck.Ui.Painters
{
    public abstract class Painter
    {
        public enum State
        {
            Normal,
            Pressed,
            Hover
        }

        public abstract void Paint(Graphics g, Rectangle position, State buttonState, string text, Image buttonImage, Font textFont, Rectangle? referencePosition);
    }

    public abstract class DoubleBrushPainter : Painter
    {
        

        protected abstract Color BorderColor(Painter.State state);
        protected abstract int BorderWidth(Painter.State state);
        protected abstract Brush UpperBrush(Painter.State state, Rectangle bounds);
        protected abstract Brush LowerBrush(Painter.State state, Rectangle bounds);
        protected abstract Color FontColor(Painter.State state);

        public override void Paint(Graphics g, Rectangle position, Painter.State buttonState, string text, Image buttonImage, Font textFont, Rectangle? referencePosition)
        {
            Rectangle upperRect;
            Rectangle lowerRect;

            if (buttonState != State.Pressed)
            {
                upperRect = new Rectangle(position.Left, position.Top, position.Width, position.Height / 2 - position.Height / 8);
                lowerRect = new Rectangle(position.Left, position.Top + position.Height / 2 - position.Height / 8, position.Width, position.Height / 2 + position.Height / 8);
            }
            else
            {
                upperRect = new Rectangle(position.Left, position.Top, position.Width, Math.Max(1, position.Height / 2));
                lowerRect = new Rectangle(position.Left, position.Top + position.Height / 2 , position.Width, Math.Max(1,position.Height / 2) );
            }

            g.FillRectangle(UpperBrush(buttonState, upperRect), upperRect);
            g.FillRectangle(LowerBrush(buttonState, lowerRect), lowerRect);

            Rectangle borderRect = new Rectangle(position.X, position.Y, position.Width - BorderWidth(buttonState), position.Height - BorderWidth(buttonState));

            g.DrawRectangle(new Pen(BorderColor(buttonState), BorderWidth(buttonState)), borderRect);


            Rectangle textBounds;

            if (buttonImage == null)
                textBounds = new Rectangle(
                   position.X + 2,
                   position.Y + 2,
                   position.Width - 4,
                   position.Height - 4);
            else
            {
                //Es gibt ein Bildchen, aus der maximalen Bildhöhe soll die Breite des Bildchens berechnet werden
                int imageHeight = position.Height - 10;

                double imageRatio = (double)buttonImage.Width / (double)buttonImage.Height;
                int imageWidth = (int)(imageRatio * (double)imageHeight);

                Rectangle imagePosition = new Rectangle(position.X + 5, position.Y + 5, imageWidth, imageHeight);
                textBounds = new Rectangle(imagePosition.Right + 2, position.Y + 2, position.Width - imagePosition.Width - 10, position.Height - 4);

                g.DrawImage(buttonImage, imagePosition, new Rectangle(0, 0, buttonImage.Width, buttonImage.Height), GraphicsUnit.Pixel);
            }

            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;
            format.FormatFlags = StringFormatFlags.NoClip
            | StringFormatFlags.FitBlackBox | StringFormatFlags.NoWrap;


            if (referencePosition != null)
            {

                //Ratio berechnen
                double xRatio = (double)position.Width / (double)referencePosition.Value.Width;
                double yRatio = (double)position.Height / (double)referencePosition.Value.Height;
                textFont = ScaledFont(textFont, xRatio, yRatio, text, textBounds, g, format);
            }


            //TextRenderer.DrawText(g, button.ButtonText, myFont, textBounds, FontColor(myState), Color.Transparent, TextFormatFlags.NoPadding | TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
            using (Brush frontBrush = new SolidBrush(FontColor(buttonState)))
            {
                //g.FillPath(frontBrush, GeneratePath(button.ButtonText, textBounds, myFont.Style));
                g.DrawString(text, textFont, frontBrush,
                    textBounds, format);
            }

        }

        protected Font ScaledFont(Font referenceFont, double xRatio, double yRatio, string text, Rectangle fitTo, Graphics g, StringFormat format)
        {
            //Zuerst die skalierten fonts anhand der y Skalierung berechnen
            float fontsize_YScaled = Math.Max(1, (int)(referenceFont.Size * yRatio));

            Font newFont = new Font(referenceFont.FontFamily, fontsize_YScaled, referenceFont.Style);
            //Überprüfen ob der gezeichnete Text (mit y skaliertem Font) passt
            SizeF textSize = g.MeasureString(text, newFont, fitTo.Width, format);

            if (textSize.Height <= fitTo.Height)
                return newFont;
            else
            {
                do
                {
                    newFont.Dispose();
                    newFont = null;

                    if (fontsize_YScaled <= 1)
                        return new Font(FontFamily.GenericSansSerif, 1, referenceFont.Style);

                    fontsize_YScaled -= 0.5f;
                    newFont = new Font(referenceFont.FontFamily, fontsize_YScaled, referenceFont.Style);
                    //Überprüfen ob der gezeichnete Text (mit y skaliertem Font) passt
                    textSize = g.MeasureString(text, newFont, fitTo.Width);

                    if (textSize.Height <= fitTo.Height)
                        return newFont;


                } while (textSize.Height <= fitTo.Height);
                return newFont;
            }



        }
    }

  
}
