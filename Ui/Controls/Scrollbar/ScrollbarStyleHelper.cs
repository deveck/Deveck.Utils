using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Deveck.Ui.Painters;

namespace Deveck.Ui.Controls.Scrollbar
{
    public static class ScrollbarStyleHelper
    {
        public enum StyleTypeEnum
        {
            Default,
            Black,
            Blue
        }

        public static void ApplyStyle(CustomScrollbar scrollbar, StyleTypeEnum styleType)
        {
            if (styleType == StyleTypeEnum.Default)
            {
                scrollbar.SetCustomBackBrush(null, null);

                scrollbar.SetUpperButtonPainter(
                    new StackedPainters(
                        new WindowsStyledButtonPainter(),
                        new PainterFilterSize(
                            new SymbolPainter(SymbolPainter.SymbolEnum.TriangleUp, true, 1, Color.Black, Color.Black, Color.Black),
                            PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2)));

                scrollbar.SetLowerButtonPainter(
                    new StackedPainters(
                        new WindowsStyledButtonPainter(),
                        new PainterFilterSize(
                            new SymbolPainter(SymbolPainter.SymbolEnum.TriangleDown, true, 1, Color.Black, Color.Black, Color.Black),
                            PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2)));

                scrollbar.SetSmallThumbPainter(new WindowsStyledButtonPainter());

                scrollbar.SetLargeThumbPainter(
                    new StackedPainters(
                        new WindowsStyledButtonPainter(),
                        new PainterFilterSize(
                            new SymbolPainter(SymbolPainter.SymbolEnum.GripH, true, 1, Color.Black, Color.Black, Color.Black),
                            PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2)
                            ));
            }
            else if (styleType == StyleTypeEnum.Black)
            {
                scrollbar.SetCustomBackBrush(null, null);
                scrollbar.SetUpperButtonPainter(
                    new StackedPainters(
                        new PainterFilterNoText(new Office2007BlackButtonPainter()),
                        new PainterFilterSize(
                            new SymbolPainter(SymbolPainter.SymbolEnum.TriangleUp, true, 1, Color.Black, Color.Black, Color.Black),
                            PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2)));

                scrollbar.SetLowerButtonPainter(
                    new StackedPainters(
                        new PainterFilterNoText(new Office2007BlackButtonPainter()),
                        new PainterFilterSize(
                            new SymbolPainter(SymbolPainter.SymbolEnum.TriangleDown, true, 1, Color.Black, Color.Black, Color.Black),
                            PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2)));

                scrollbar.SetSmallThumbPainter(new PainterFilterNoText(new Office2007BlackButtonPainter()));

                scrollbar.SetLargeThumbPainter(
                    new StackedPainters(
                        new PainterFilterNoText(new Office2007BlackButtonPainter()),
                        new PainterFilterSize(
                            new SymbolPainter(SymbolPainter.SymbolEnum.GripH, true, 1, Color.Black, Color.Black, Color.Black),
                            PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2)
                            ));
            }
            else if (styleType == StyleTypeEnum.Blue)
            {
                scrollbar.SetCustomBackBrush(null, null);
                scrollbar.SetUpperButtonPainter(
                    new StackedPainters(
                        new PainterFilterNoText(new Office2007BlueButtonPainter()),
                        new PainterFilterSize(
                            new SymbolPainter(SymbolPainter.SymbolEnum.TriangleUp, true, 1, Color.Black, Color.Black, Color.Black),
                            PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2)));

                scrollbar.SetLowerButtonPainter(
                    new StackedPainters(
                        new PainterFilterNoText(new Office2007BlueButtonPainter()),
                        new PainterFilterSize(
                            new SymbolPainter(SymbolPainter.SymbolEnum.TriangleDown, true, 1, Color.Black, Color.Black, Color.Black),
                            PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2)));

                scrollbar.SetSmallThumbPainter(new PainterFilterNoText(new Office2007BlueButtonPainter()));

                scrollbar.SetLargeThumbPainter(
                    new StackedPainters(
                        new PainterFilterNoText(new Office2007BlueButtonPainter()),
                        new PainterFilterSize(
                            new SymbolPainter(SymbolPainter.SymbolEnum.GripH, true, 1, Color.Black, Color.Black, Color.Black),
                            PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2)
                            ));
            }
        }
    }
}
