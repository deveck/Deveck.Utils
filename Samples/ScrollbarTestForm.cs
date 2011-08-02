using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Deveck.Ui.Controls.Scrollbar;

namespace Deveck.Utils.Samples
{
    public partial class ScrollbarTestForm : Form
    {
        public ScrollbarTestForm()
        {
            InitializeComponent();

            ScrollbarStyleHelper.ApplyStyle(scrollbar2, ScrollbarStyleHelper.StyleTypeEnum.Blue);
            ScrollbarStyleHelper.ApplyStyle(scrollbar3, ScrollbarStyleHelper.StyleTypeEnum.Black);

            list.VScrollbar = new ScrollbarCollector(scrollbar1, scrollbar2, scrollbar3);
        }

        private void buttonAddItems_Click(object sender, EventArgs e)
        {
            int currentItemCount = list.Items.Count;
            for (int i = currentItemCount; i < currentItemCount + 100; i++)
                list.Items.Add("Item " + i.ToString());

            list.Items[list.Items.Count - 1].Selected = true;
        }

        private void buttonRemoveItems_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 100; i++)
                list.Items.RemoveAt(list.Items.Count - 1);
        }
    }
}
