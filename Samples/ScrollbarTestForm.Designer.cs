namespace Deveck.Utils.Samples
{
    partial class ScrollbarTestForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.list = new Deveck.Ui.Controls.CustomListView();
            this.scrollbar1 = new Deveck.Ui.Controls.Scrollbar.CustomScrollbar();
            this.scrollbar2 = new Deveck.Ui.Controls.Scrollbar.CustomScrollbar();
            this.scrollbar3 = new Deveck.Ui.Controls.Scrollbar.CustomScrollbar();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.buttonAddItems = new System.Windows.Forms.Button();
            this.buttonRemoveItems = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // list
            // 
            this.list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.list.FullRowSelect = true;
            this.list.HideSelection = false;
            this.list.Location = new System.Drawing.Point(12, 12);
            this.list.MultiSelect = false;
            this.list.Name = "list";
            this.list.Size = new System.Drawing.Size(288, 303);
            this.list.TabIndex = 0;
            this.list.UseCompatibleStateImageBehavior = false;
            this.list.View = System.Windows.Forms.View.Details;
            this.list.VScrollbar = null;
            // 
            // scrollbar1
            // 
            this.scrollbar1.ActiveBackColor = System.Drawing.Color.Gray;
            this.scrollbar1.LargeChange = 10;
            this.scrollbar1.Location = new System.Drawing.Point(306, 12);
            this.scrollbar1.Maximum = 99;
            this.scrollbar1.Minimum = 0;
            this.scrollbar1.Name = "scrollbar1";
            this.scrollbar1.Size = new System.Drawing.Size(41, 303);
            this.scrollbar1.SmallChange = 1;
            this.scrollbar1.TabIndex = 1;
            this.scrollbar1.Text = "customScrollbar1";
            this.scrollbar1.ThumbStyle = Deveck.Ui.Controls.Scrollbar.CustomScrollbar.ThumbStyleEnum.Auto;
            this.scrollbar1.Value = 0;
            // 
            // scrollbar2
            // 
            this.scrollbar2.ActiveBackColor = System.Drawing.Color.Gray;
            this.scrollbar2.LargeChange = 10;
            this.scrollbar2.Location = new System.Drawing.Point(353, 12);
            this.scrollbar2.Maximum = 99;
            this.scrollbar2.Minimum = 0;
            this.scrollbar2.Name = "scrollbar2";
            this.scrollbar2.Size = new System.Drawing.Size(41, 303);
            this.scrollbar2.SmallChange = 1;
            this.scrollbar2.TabIndex = 2;
            this.scrollbar2.Text = "customScrollbar2";
            this.scrollbar2.ThumbStyle = Deveck.Ui.Controls.Scrollbar.CustomScrollbar.ThumbStyleEnum.Auto;
            this.scrollbar2.Value = 0;
            // 
            // scrollbar3
            // 
            this.scrollbar3.ActiveBackColor = System.Drawing.Color.Gray;
            this.scrollbar3.LargeChange = 10;
            this.scrollbar3.Location = new System.Drawing.Point(400, 12);
            this.scrollbar3.Maximum = 99;
            this.scrollbar3.Minimum = 0;
            this.scrollbar3.Name = "scrollbar3";
            this.scrollbar3.Size = new System.Drawing.Size(41, 303);
            this.scrollbar3.SmallChange = 1;
            this.scrollbar3.TabIndex = 3;
            this.scrollbar3.Text = "customScrollbar3";
            this.scrollbar3.ThumbStyle = Deveck.Ui.Controls.Scrollbar.CustomScrollbar.ThumbStyleEnum.Auto;
            this.scrollbar3.Value = 0;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 235;
            // 
            // buttonAddItems
            // 
            this.buttonAddItems.Location = new System.Drawing.Point(12, 330);
            this.buttonAddItems.Name = "buttonAddItems";
            this.buttonAddItems.Size = new System.Drawing.Size(139, 39);
            this.buttonAddItems.TabIndex = 4;
            this.buttonAddItems.Text = "Add items";
            this.buttonAddItems.UseVisualStyleBackColor = true;
            this.buttonAddItems.Click += new System.EventHandler(this.buttonAddItems_Click);
            // 
            // buttonRemoveItems
            // 
            this.buttonRemoveItems.Location = new System.Drawing.Point(161, 330);
            this.buttonRemoveItems.Name = "buttonRemoveItems";
            this.buttonRemoveItems.Size = new System.Drawing.Size(139, 39);
            this.buttonRemoveItems.TabIndex = 5;
            this.buttonRemoveItems.Text = "Remove items";
            this.buttonRemoveItems.UseVisualStyleBackColor = true;
            this.buttonRemoveItems.Click += new System.EventHandler(this.buttonRemoveItems_Click);
            // 
            // ScrollbarTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(465, 376);
            this.Controls.Add(this.buttonRemoveItems);
            this.Controls.Add(this.buttonAddItems);
            this.Controls.Add(this.scrollbar3);
            this.Controls.Add(this.scrollbar2);
            this.Controls.Add(this.scrollbar1);
            this.Controls.Add(this.list);
            this.Name = "ScrollbarTestForm";
            this.Text = "ScrollbarTest";
            this.ResumeLayout(false);

        }

        #endregion

        private Deveck.Ui.Controls.CustomListView list;
        private Deveck.Ui.Controls.Scrollbar.CustomScrollbar scrollbar1;
        private Deveck.Ui.Controls.Scrollbar.CustomScrollbar scrollbar2;
        private Deveck.Ui.Controls.Scrollbar.CustomScrollbar scrollbar3;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button buttonAddItems;
        private System.Windows.Forms.Button buttonRemoveItems;
    }
}