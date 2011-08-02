using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Deveck.Ui.Painters;

namespace Deveck.Ui.Controls.Scrollbar
{
    public class CustomScrollbar : Control, ICustomScrollbar
    {
        private enum ScrollActionEnum
        {
            Down,
            Up,
            PageDown,
            PageUp
        }

        public enum ThumbStyleEnum
        {
            Auto,
            Large,
            Small
        }

        private volatile int _init = 0;

        public void BeginInit()
        {
            _init++;
        }

        public void EndInit()
        {
            if (_init > 0)
                _init--;

            if (_init == 0)
                SetBounds(_minimum, _maximum);
        }


        protected Painter.State _upperButtonState = Painter.State.Normal;
        protected Painter _upperButtonPainter;
        protected Painter.State _lowerButtonState = Painter.State.Normal;
        protected Painter _lowerButtonPainter;
        protected Painter _largeThumbPainter;
        protected Painter _smallThumbPainter;
        protected Painter.State _thumbState = Painter.State.Normal;
        protected Painter.State _beforeThumbState = Painter.State.Normal;
        protected Painter.State _afterThumbState = Painter.State.Normal;

        /// <summary>
        /// Contains the current position of the thumb
        /// </summary>
        protected Rectangle _currentThumbPosition;

        /// <summary>
        /// Rectangle before the thumb and after the "up"-button
        /// </summary>
        protected Rectangle _beforeThumb;

        /// <summary>
        /// Rectangle after the thumb and before the "down"-button
        /// </summary>
        protected Rectangle _afterThumb;

        protected ThumbStyleEnum _thumbStyle = ThumbStyleEnum.Auto;

        /// <summary>
        /// Tells the Scrollbar which thumb to use
        /// </summary>
        public ThumbStyleEnum ThumbStyle
        {
            get { return _thumbStyle; }
            set { _thumbStyle = value; }
        }

        /// <summary>
        /// Timer gets activated once the user activates an action and remains pressing the button
        /// </summary>
        private Timer _timer = new Timer();

        /// <summary>
        /// Action to perform once timer is activated
        /// </summary>
        private ScrollActionEnum _timerAction;

        /// <summary>
        ///  If the thumb gets moved by the mouse, moveThumbStart contains the 
        ///  start position of the mouse down
        /// </summary>
        private Point? _moveThumbStart = null;
        private int _moveThumbValueStart;
        private Rectangle _moveThumbRectStart;

        public void SetUpperButtonPainter(Painter upperButtonPainter)
        {
            _upperButtonPainter = upperButtonPainter;
        }

        public void SetLowerButtonPainter(Painter lowerButtonPainter)
        {
            _lowerButtonPainter = lowerButtonPainter;
        }

        public void SetLargeThumbPainter(Painter largeThumbPainter)
        {
            _largeThumbPainter = largeThumbPainter;
        }

        public void SetSmallThumbPainter(Painter smallThumbPainter)
        {
            _smallThumbPainter = smallThumbPainter;
        }


        /// <summary>
        /// Set to true of a custom backbrush is used
        /// </summary>
        protected bool _useCustomBackBrush = false;

        public override Color BackColor
        {
            get{ return base.BackColor; }
            set
            {
                base.BackColor = value;
                PrepareBack();
            }
        }

        protected Color _activeBackColor = Color.Gray;
        protected bool _useCustomActiveBackColor = false;

        public Color ActiveBackColor
        {
            get{ return _activeBackColor;}
            set
            {
                _activeBackColor = value;
                PrepareBack();
            }
        }

        public void SetCustomBackBrush(Brush brush, Brush activeBackBrush)
        {
            _useCustomBackBrush = brush != null;
            _backBrush = brush;

            _useCustomActiveBackColor = activeBackBrush != null;
            _activeBackBrush = activeBackBrush;
            PrepareBack();
        }

        


        protected Pen _borderPen;
        protected Brush _backBrush;
        protected Brush _activeBackBrush;

        public CustomScrollbar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            PrepareBack();

            _upperButtonPainter = new StackedPainters(
                new WindowsStyledButtonPainter(),
                new PainterFilterSize(
                    new SymbolPainter(SymbolPainter.SymbolEnum.TriangleUp, true, 1, Color.Black, Color.Black, Color.Black),
                    PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2));

            _lowerButtonPainter = new StackedPainters(
                new WindowsStyledButtonPainter(),
                new PainterFilterSize(
                    new SymbolPainter(SymbolPainter.SymbolEnum.TriangleDown, true, 1, Color.Black, Color.Black, Color.Black),
                    PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0, 0, 0, 0, 10, 2));

            _smallThumbPainter = new WindowsStyledButtonPainter();
            _largeThumbPainter = new StackedPainters(
                new WindowsStyledButtonPainter(),
                new PainterFilterSize(
                    new SymbolPainter(SymbolPainter.SymbolEnum.GripH, true, 1, Color.Black, Color.Black, Color.Black),
                    PainterFilterSize.Alignment.Center, PainterFilterSize.Alignment.Center, 0,0,0,0 ,10, 2)
                    );

            InvalidateThumbPosition();

            _timer.Tick += new EventHandler(_timer_Tick);

        }


        protected void PrepareBack()
        {
            if (!_useCustomBackBrush)
                _backBrush = new SolidBrush(BackColor);

            if (!_useCustomActiveBackColor)
                _activeBackBrush = new SolidBrush(ActiveBackColor);
        }


        #region Painting methods
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            PaintBack(e);

            Rectangle upperButton = RectUpperButton();
            Rectangle lowerButton = RectLowerButton();
            _upperButtonPainter.Paint(e.Graphics, upperButton, _upperButtonState, "", null, null, null);
            _lowerButtonPainter.Paint(e.Graphics, lowerButton, _lowerButtonState, "", null, null, null);

            if(_currentThumbPosition.Height > 0)
                GetThumbPainter().Paint(e.Graphics, _currentThumbPosition, _thumbState, "", null, null, null);
        }

        protected void PaintBack(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(_backBrush, this.ClientRectangle);

            if (_beforeThumbState == Painter.State.Pressed)
                e.Graphics.FillRectangle(_activeBackBrush, _beforeThumb);
            if (_afterThumbState == Painter.State.Pressed)
                e.Graphics.FillRectangle(_activeBackBrush, _afterThumb);
        }

        protected Rectangle RectUpperButton()
        {
            return new Rectangle(2, 2, ClientSize.Width - 4, 30);
        }

        protected Rectangle RectLowerButton()
        {
            return new Rectangle(2, ClientSize.Height - 32, ClientSize.Width - 4, 30);
        }

        protected Painter GetThumbPainter()
        {
            if (GetRealThumbStyle() == ThumbStyleEnum.Small)
                return _smallThumbPainter;
            else
                return _largeThumbPainter;
        }

        protected ThumbStyleEnum GetRealThumbStyle()
        {
            ThumbStyleEnum myThumbStyle = _thumbStyle;

            if (myThumbStyle == ThumbStyleEnum.Auto && _maximum - _minimum >= 200)
                myThumbStyle = ThumbStyleEnum.Small;
            else
                myThumbStyle = ThumbStyleEnum.Large;

            return myThumbStyle;
        }
        #endregion

        #region Mouse events
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //The thumb gets actively moved by the mouse
            if (_moveThumbStart != null)
            {
                int offset = e.Y - _moveThumbStart.Value.Y;
                //Find the nearest valid thumb position for Y + offset and translate it
                //to the corresponding value
                double barHeight = ClientSize.Height - 4 - 2 * 30 - _moveThumbRectStart.Height;
                double smallChangeHeight = barHeight / (double)(_maximum - _minimum);


                double lastOffset = Math.Abs(offset);
                int currentValIncrease = 0;
                while (true)
                {
                    if (offset > 0)
                        currentValIncrease++;
                    else
                        currentValIncrease--;

                    double currentOffset = Math.Abs( (double)smallChangeHeight * (double)currentValIncrease - (double)offset);
                    if (currentOffset > lastOffset || (currentOffset == 0 && lastOffset == 0))
                        break;

                    lastOffset = currentOffset;

                }
                if (currentValIncrease > 1)
                    SetValue(_moveThumbValueStart + currentValIncrease - 1);
                else if (currentValIncrease < -1)
                    SetValue(_moveThumbValueStart + currentValIncrease + 1);
                return;
            }

            if (AnyStateHasStatus(Painter.State.Pressed))
                return;


            if (RectUpperButton().Contains(e.Location) && _upperButtonState != Painter.State.Hover)
            {
                ResetAllButtonState();
                _upperButtonState = Painter.State.Hover;
                Invalidate();
            }
            else if (RectLowerButton().Contains(e.Location) && _lowerButtonState != Painter.State.Hover)
            {
                ResetAllButtonState();
                _lowerButtonState = Painter.State.Hover;
                Invalidate();
            }
            else if (_currentThumbPosition.Contains(e.Location) && _thumbState != Painter.State.Hover)
            {
                ResetAllButtonState();
                _thumbState = Painter.State.Hover;
                Invalidate();
            }
            else if (_beforeThumb.Contains(e.Location) && _beforeThumbState != Painter.State.Hover)
            {
                ResetAllButtonState();
                _beforeThumbState = Painter.State.Hover;
                Invalidate();
            }
            else if (_afterThumb.Contains(e.Location) && _afterThumbState != Painter.State.Hover)
            {
                ResetAllButtonState();
                _afterThumbState = Painter.State.Hover;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (RectUpperButton().Contains(e.Location) && _upperButtonState != Painter.State.Pressed)
            {
                ScrollAndActivateTimer(ScrollActionEnum.Up);
                ResetAllButtonState();
                _upperButtonState = Painter.State.Pressed;
                Invalidate();
            }
            else if (RectLowerButton().Contains(e.Location) && _lowerButtonState != Painter.State.Pressed)
            {
                ScrollAndActivateTimer(ScrollActionEnum.Down);
                ResetAllButtonState();
                _lowerButtonState = Painter.State.Pressed;
                Invalidate();
            }
            else if (_currentThumbPosition.Contains(e.Location) && _thumbState != Painter.State.Pressed)
            {
                ResetAllButtonState();
                _thumbState = Painter.State.Pressed;
                _moveThumbStart = e.Location;
                _moveThumbValueStart = _currentValue;
                _moveThumbRectStart = _currentThumbPosition;
                Invalidate();
            }
            else if (_beforeThumb.Contains(e.Location) && _beforeThumbState != Painter.State.Pressed)
            {
                ScrollAndActivateTimer(ScrollActionEnum.PageUp);
                ResetAllButtonState();
                _beforeThumbState = Painter.State.Pressed;
                Invalidate();
            }
            else if (_afterThumb.Contains(e.Location) && _afterThumbState != Painter.State.Pressed)
            {
                ScrollAndActivateTimer(ScrollActionEnum.PageDown);
                ResetAllButtonState();
                _afterThumbState = Painter.State.Pressed;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _timer.Stop();

            base.OnMouseUp(e);

            ResetAllButtonState();
            Invalidate();
        }

        private void ResetAllButtonState()
        {
            _moveThumbStart = null;
            _thumbState = Painter.State.Normal;
            _lowerButtonState = Painter.State.Normal;
            _upperButtonState = Painter.State.Normal;
            _beforeThumbState = Painter.State.Normal;
            _afterThumbState = Painter.State.Normal;
        }

        private bool AnyStateHasStatus(Painter.State state)
        {
            return _thumbState == state || _lowerButtonState == state ||
                _upperButtonState == state || _beforeThumbState == state ||
                _afterThumbState == state;
        }
        #endregion

        #region Resize
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            InvalidateThumbPosition();
        }
        #endregion

        #region Do the scroll
        private void Scroll(ScrollActionEnum scrollAction)
        {
            int change = SmallChange;

            if (scrollAction == ScrollActionEnum.Up)
                change = -SmallChange;
            else if (scrollAction == ScrollActionEnum.PageDown)
                change = LargeChange;
            else if (scrollAction == ScrollActionEnum.PageUp)
                change = -LargeChange;

            SetValue(_currentValue + change);
        }

        private void ScrollAndActivateTimer(ScrollActionEnum scrollAction)
        {
            
            Scroll(scrollAction);
            _timerAction = scrollAction;
            _timer.Interval = 500;
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Interval = 100;
            Scroll(_timerAction);
        }
        #endregion

        /// <summary>
        /// Recalculates the current position of the thumb
        /// </summary>
        protected void InvalidateThumbPosition()
        {
            if (_init > 0) return;

            if (_minimum == _maximum)
            {
                _currentThumbPosition = new Rectangle(0, 0, 0, 0);
                return;
            }
            //Calculate the size of the thumb, thumb is positioned in origin
            if (GetRealThumbStyle() == ThumbStyleEnum.Small)
                _currentThumbPosition = new Rectangle(2, 32, ClientSize.Width - 4, 15);
            else
                _currentThumbPosition = new Rectangle(2, 32, ClientSize.Width - 4, 30);

            //Move the thumb to the correct position (depending on the current scroll value)
            double barHeight = ClientSize.Height - 4 - 2 * 30 - _currentThumbPosition.Height;
            double smallChangeHeight = barHeight / (double)(_maximum - _minimum);

            int thumbTopOffset = (int)(smallChangeHeight * (double)_currentValue);
            _currentThumbPosition.Offset(0, thumbTopOffset);


            _beforeThumb = Rectangle.FromLTRB(2, RectUpperButton().Bottom, ClientSize.Width - 2, _currentThumbPosition.Top);
            _afterThumb = Rectangle.FromLTRB(2, _currentThumbPosition.Bottom, ClientSize.Width - 2, RectLowerButton().Top);
            this.Invalidate();
        }

        protected void SetBounds(int minimum, int maximum)
        {
            if (_init > 0) return;

            if (minimum > maximum)
                minimum = maximum;

            _minimum = minimum;
            _maximum = maximum;

            SetValue(_currentValue);

        }
        protected void SetValue(int newValue)
        {
            if (_init > 0) return;

            if (newValue < _minimum)
                _currentValue = _minimum;
            else if (newValue > _maximum)
                _currentValue = _maximum;
            else
                _currentValue = newValue;

            InvalidateThumbPosition();

            if (ValueChanged != null)
                ValueChanged(this, _currentValue);
        }

        #region ICustomScrollbar Members

        public event ScrollValueChangedDelegate ValueChanged;
        protected int _largeChange = 10;
        protected int _smallChange = 1;
        protected int _maximum = 99;
        protected int _minimum = 0;
        protected int _currentValue = 0;

        public int LargeChange
        {
            get { return _largeChange; }
            set { _largeChange = value; }
        }

        public int SmallChange
        {
            get { return _smallChange; }
            set { _smallChange = value; }
        }

        public int Maximum
        {
            get { return _maximum; }
            set { SetBounds(_minimum, value); }
        }

        public int Minimum
        {
            get { return _minimum; }
            set { SetBounds(value, _maximum); }
        }

        public int Value
        {
            get { return _currentValue; }
            set { SetValue(value); }
        }

        #endregion
    }
}
