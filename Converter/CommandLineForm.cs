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
    public partial class CommandLineForm : Form
    {
        public CommandLineForm()
        {
            InitializeComponent();
        }

        public void SetText(string str)
        {
            if (str == null)
                return;

            richTextBox1.AppendText(str);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
