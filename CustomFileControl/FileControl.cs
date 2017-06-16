using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace CustomFileControl
{
    public partial class FileControl : UserControl
    {
        // delegates for buttons
        public delegate void OpenClickedHandler();
        public delegate void SaveClickedHandler();

        // set OpenDialogFilter and OpenDialogTitle before using this
        // otherwise a standard values will be used
        public string OpenDialogFilter { get; set; }
        public string OpenDialogTitle { get; set; }
        // path rerturned by the open file dialog
        public string OpenDialogFileName { get; set; }

        public FileControl()
        {
            InitializeComponent();

            OpenDialogFileName = "";
            OpenDialogFilter = "All Files|*.*";
            OpenDialogTitle = "Select a File";

            label1.Paint += new PaintEventHandler(this.OnLabelPaint);
            
            label1.ContextMenuStrip = createCopyMenu(label1_Click); 
        }

        // event for delegate
        [Category("Action")]
        [Description("Fires when the Open button is clicked.")]
        public event OpenClickedHandler OpenButtonClicked;
        protected virtual void OnOpenClicked()
        {
            // If an event has no subscribers registerd, it will
            // evaluate to null. The test checks that the value is not
            // null, ensuring that there are subsribers before
            // calling the event itself.
            if (OpenButtonClicked != null)
            {
                OpenButtonClicked();  // Notify Subscribers
            }
        }
        public void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (OpenDialogFilter == null)
            {
                dialog.Filter = "All Files|*.*";
            }
            else
            {
                dialog.Filter = OpenDialogFilter;
            }
            if (OpenDialogTitle == null)
            {
                dialog.Title = "Select a File";
            }
            else
            {
                dialog.Title = OpenDialogTitle;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OpenDialogFileName = dialog.FileName;
                FileName = OpenDialogFileName;
                OnOpenClicked();
            }
            
        }

        // event for delegate
        [Category("Action")]
        [Description("Fires when the Save button is clicked.")]
        public event SaveClickedHandler SaveButtonClicked;
        protected virtual void OnSaveClicked()
        {
            // If an event has no subscribers registerd, it will
            // evaluate to null. The test checks that the value is not
            // null, ensuring that there are subsribers before
            // calling the event itself.
            if (SaveButtonClicked != null)
            {
                SaveButtonClicked();  // Notify Subscribers
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            OnSaveClicked();
        }

        // Read / Write Property for the File Name. This Property
        // will be visible in the containing application.
        [Category("Appearance")]
        [Description("Gets or sets the file name in the text box")]
        public string FileName
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        private void OnLabelPaint(object theSender, PaintEventArgs theArgs)
        {
            Rectangle rect = label1.Bounds;

            TextRenderer.DrawText(theArgs.Graphics, label1.Text, label1.Font, 
               rect, label1.ForeColor, label1.BackColor,
                TextFormatFlags.Left | TextFormatFlags.PathEllipsis);
        }

        ContextMenuStrip createCopyMenu(EventHandler specificAction)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripItem copyCmd = contextMenu.Items.Add("Copy");
            copyCmd.Click += specificAction;
            return contextMenu;
        }

        void label1_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(label1.Text, false);
        }

        private void button1_MouseHover(object sender, EventArgs e)
        {
            if (button1.Text.Length == 0)
            {
                System.Windows.Forms.ToolTip ToolTip1 = new System.Windows.Forms.ToolTip();
                ToolTip1.SetToolTip(this.button1, "Open");
            }
        }

        private void button2_MouseHover(object sender, EventArgs e)
        {
            if (button2.Text.Length == 0)
            {
                System.Windows.Forms.ToolTip ToolTip2 = new System.Windows.Forms.ToolTip();
                ToolTip2.SetToolTip(this.button2, "Save");
            }
        }

    }

}
