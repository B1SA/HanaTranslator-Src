using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Translator
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        public void SetText(string str)
        {
            if (str == null)
                return;

            label1.Text = str;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
