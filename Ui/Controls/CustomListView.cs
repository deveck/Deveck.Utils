using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Deveck.Ui.Controls.Scrollbar;

namespace Deveck.Ui.Controls
{
    public class CustomListView : ListView
    {
        [StructLayout(LayoutKind.Sequential)]
        struct SCROLLINFO
        {
            public uint cbSize;
            public uint fMask;
            public int nMin;
            public int nMax;
            public uint nPage;
            public int nPos;
            public int nTrackPos;
        }

        private enum ScrollBarDirection
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }

        private enum ScrollInfoMask
        {
            SIF_RANGE = 0x1,
            SIF_PAGE = 0x2,
            SIF_POS = 0x4,
            SIF_DISABLENOSCROLL = 0x8,
            SIF_TRACKPOS = 0x10,
            SIF_ALL = SIF_RANGE + SIF_PAGE + SIF_POS + SIF_TRACKPOS
        }

        //fnBar values
        private enum SBTYPES
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }
        //lpsi values
        private enum LPCSCROLLINFO
        {
            SIF_RANGE = 0x0001,
            SIF_PAGE = 0x0002,
            SIF_POS = 0x0004,
            SIF_DISABLENOSCROLL = 0x0008,
            SIF_TRACKPOS = 0x0010,
            SIF_ALL = (SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS)
        }

        //ListView item information
        [StructLayoutAttribute(LayoutKind.Sequential)]
        private struct LVITEM
        {
            public uint mask;
            public int iItem;
            public int iSubItem;
            public uint state;
            public uint stateMask;
            public IntPtr pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
        }

        public enum ScrollBarCommands
        {
            SB_LINEUP = 0,
            SB_LINELEFT = 0,
            SB_LINEDOWN = 1,
            SB_LINERIGHT = 1,
            SB_PAGEUP = 2,
            SB_PAGELEFT = 2,
            SB_PAGEDOWN = 3,
            SB_PAGERIGHT = 3,
            SB_THUMBPOSITION = 4,
            SB_THUMBTRACK = 5,
            SB_TOP = 6,
            SB_LEFT = 6,
            SB_BOTTOM = 7,
            SB_RIGHT = 7,
            SB_ENDSCROLL = 8
        }

        private const UInt32 WM_VSCROLL = 0x0115;
        private const UInt32 WM_NCCALCSIZE = 0x83;

        private const UInt32 LVM_FIRST = 0x1000;
        private const UInt32 LVM_INSERTITEMA = (LVM_FIRST + 7);
		private const UInt32 LVM_INSERTITEMW = (LVM_FIRST + 77);
		private const UInt32 LVM_DELETEITEM = (LVM_FIRST + 8);
        private const UInt32 LVM_DELETEALLITEMS = (LVM_FIRST + 9);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        public delegate void ScrollPositionChangedDelegate(CustomListView listview, int pos);

        public event ScrollPositionChangedDelegate ScrollPositionChanged;
        public event Action<CustomListView> ItemAdded;
        public event Action<CustomListView> ItemsRemoved;

        private int _disableChangeEvents = 0;

        private void BeginDisableChangeEvents()
        {
            _disableChangeEvents++;
        }

        private void EndDisableChangeEvents()
        {
            if (_disableChangeEvents > 0)
                _disableChangeEvents--;
        }

        private ICustomScrollbar _vScrollbar = null;

        public ICustomScrollbar VScrollbar
        {
            get { return _vScrollbar; }
            set
            {
                if (value != null)
                {
                    UpdateScrollbar();

                    value.ValueChanged += new ScrollValueChangedDelegate(value_ValueChanged);
                }

                _vScrollbar = value;
            }
        }

        


        public CustomListView()
        {
            SelectedIndexChanged += new EventHandler(CustomListView_SelectedIndexChanged);
            
        }

        void CustomListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateScrollbar();
        }

        private void value_ValueChanged(ICustomScrollbar sender, int newValue)
        {
            if (_disableChangeEvents > 0)
                return;

            SetScrollPosition(_vScrollbar.Value);
        }

        public void GetScrollPosition(out int min, out int max, out int pos, out int smallchange, out int largechange)
        {
            SCROLLINFO scrollinfo = new SCROLLINFO();
            scrollinfo.cbSize = (uint)Marshal.SizeOf(typeof(SCROLLINFO));
            scrollinfo.fMask = (int)ScrollInfoMask.SIF_ALL;
            if (GetScrollInfo(this.Handle, (int)SBTYPES.SB_VERT, ref scrollinfo))
            {
                min = scrollinfo.nMin;
                max = scrollinfo.nMax;
                pos = scrollinfo.nPos;
                smallchange = 1;
                largechange = (int)scrollinfo.nPage;
            }
            else
            {
                min = 0;
                max = 0;
                pos = 0;
                smallchange = 0;
                largechange = 0;
            }
        }


        private void UpdateScrollbar()
        {
            if (_vScrollbar != null)
            {
                int max, min, pos, smallchange, largechange;
                GetScrollPosition(out min, out max, out pos, out smallchange, out largechange);

                BeginDisableChangeEvents();
                _vScrollbar.Value = pos;
                _vScrollbar.Maximum = max - largechange + 1;
                _vScrollbar.Minimum = min;
                _vScrollbar.SmallChange = smallchange;
                _vScrollbar.LargeChange = largechange;
                EndDisableChangeEvents();
            }
        }

        public void SetScrollPosition(int pos)
        {
            pos = Math.Min(Items.Count - 1, pos);

            if (pos < 0 || pos >= Items.Count)
                return;

            SuspendLayout();
            EnsureVisible(pos);

            for (int i = 0; i < 10; i++)
            {
                if(TopItem != null && TopItem.Index != pos)
                    TopItem = Items[pos];
            }

            ResumeLayout();            
        }


        protected void OnItemAdded()
        {
            if (_disableChangeEvents > 0) return;

            UpdateScrollbar();

            if (ItemAdded != null)
                ItemAdded(this);
        }

        protected void OnItemsRemoved()
        {
            if (_disableChangeEvents > 0) return;

            UpdateScrollbar();

            if (ItemsRemoved != null)
                ItemsRemoved(this);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (_vScrollbar != null)
                _vScrollbar.Value -= 3*Math.Sign(e.Delta);
        }

        protected override void WndProc(ref Message m)
        {
            

            if (m.Msg == WM_VSCROLL)
            {
                int max, min, pos, smallchange, largechange;
                GetScrollPosition(out min, out max, out pos, out smallchange, out largechange);

                if (ScrollPositionChanged != null)
                    ScrollPositionChanged(this, pos);

                if (_vScrollbar != null)
                    _vScrollbar.Value = pos;
            }
            else if (m.Msg == WM_NCCALCSIZE) // WM_NCCALCSIZE
            {
                int style = (int)GetWindowLong(this.Handle, GWL_STYLE);
                if ((style & WS_VSCROLL) == WS_VSCROLL)
                    SetWindowLong(this.Handle, GWL_STYLE, style & ~WS_VSCROLL);

            }
            else if (m.Msg == LVM_INSERTITEMA || m.Msg == LVM_INSERTITEMW)
                OnItemAdded();
            else if (m.Msg == LVM_DELETEITEM || m.Msg == LVM_DELETEALLITEMS)
                OnItemsRemoved();


            base.WndProc(ref m);

        }


        const int GWL_STYLE = -16;
        const int WS_VSCROLL = 0x00200000;


        public static int GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
                return (int)GetWindowLong32(hWnd, nIndex);
            else
                return (int)(long)GetWindowLongPtr64(hWnd, nIndex);
        }

        public static int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
        {
            if (IntPtr.Size == 4)
                return (int)SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            else
                return (int)(long)SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, int dwNewLong);

    }
}
