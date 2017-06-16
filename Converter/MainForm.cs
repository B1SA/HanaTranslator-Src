using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Collections;


namespace Translator
{
    public partial class MainForm : Form
    {
        // form layout determines layout of source adit and result windows
        // fs_Tab - 2 tabs with full source edit on the first tab and results on the second
        // fs_Edit - only source edit boxes
        // fs_EditResult - verticaly split screens with source on the tom and results at the bottom
        private enum FormLayout { fl_Tab, fl_Edit, fl_EditResult }

        private TranslatorTool tool;
        private List<Control> controlsList;
        private string lastResult;
		private FormLayout currentFormLayout = FormLayout.fl_Tab;

        public MainForm()
        {            
            InitializeComponent();
            SetLayout(FormLayout.fl_Edit);
        }

        private void SetLayout( FormLayout layout )
        {
            if (layout != currentFormLayout)
            {
                currentFormLayout = layout;

                switch (currentFormLayout)
                {
                    case FormLayout.fl_Edit:
                        splitLR.Parent = editArea;
                        inputFileControl.Parent = splitLR.Panel1;
                        outputFileControl.Parent = splitLR.Panel2;
                        sourceLeft.Parent = splitLR.Panel1;
                        sourceRight.Parent = splitLR.Panel2;

                        sourceLeft.Dock = DockStyle.None;
                        sourceLeft.Top = 25;
                        sourceLeft.Left = 0;
                        sourceLeft.Width = splitLR.Panel1.Width;
                        sourceLeft.Height = splitLR.Panel1.Height - 25;
                        sourceLeft.Anchor = (AnchorStyles) (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
                        sourceRight.Dock = DockStyle.None;
                        sourceRight.Top = 25;
                        sourceRight.Left = 0;
                        sourceRight.Width = splitLR.Panel2.Width;
                        sourceRight.Height = splitLR.Panel2.Height - 25;
                        sourceRight.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);

                        splitRight.Visible = false;
                        splitLeft.Visible = false;
                        resultLeft.Visible = false;
                        resultRight.Visible = false;

                        tabControl.Visible = false;
                        break;

                    case FormLayout.fl_EditResult:
                        splitLR.Parent = editArea;
                        inputFileControl.Parent = splitLR.Panel1;
                        outputFileControl.Parent = splitLR.Panel2;
                        sourceLeft.Parent = splitLeft.Panel1;
                        sourceRight.Parent = splitRight.Panel1;
                        resultLeft.Parent = splitLeft.Panel2;
                        resultRight.Parent = splitRight.Panel2;
                        sourceLeft.Dock = DockStyle.Fill;
                        sourceRight.Dock = DockStyle.Fill;

                        splitLeft.Dock = DockStyle.None;
                        splitLeft.Top = 25;
                        splitLeft.Left = 0;
                        splitLeft.Width = splitLR.Panel1.Width;
                        splitLeft.Height = splitLR.Panel1.Height - 25;
                        splitLeft.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
                        splitRight.Dock = DockStyle.None;
                        splitRight.Top = 25;
                        splitRight.Left = 0;
                        splitRight.Width = splitLR.Panel2.Width;
                        splitRight.Height = splitLR.Panel2.Height - 25;
                        splitRight.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);

                        splitRight.Visible = true;
                        splitLeft.Visible = true;
                        resultLeft.Visible = true;
                        resultRight.Visible = true;

                        tabControl.Visible = false;
                        break;

                    case FormLayout.fl_Tab:
                        tabControl.Parent = editArea;
                        splitLR.Parent = tabSource;
                        inputFileControl.Parent = splitLR.Panel1;
                        outputFileControl.Parent = splitLR.Panel2;
                        sourceLeft.Parent = splitLR.Panel1;
                        sourceRight.Parent = splitLR.Panel2;
                        resultLeft.Parent = splitResult.Panel1;
                        resultRight.Parent = splitResult.Panel2;

                        sourceLeft.Dock = DockStyle.None;
                        sourceLeft.Top = 25;
                        sourceLeft.Left = 0;
                        sourceLeft.Width = splitLR.Panel1.Width;
                        sourceLeft.Height = splitLR.Panel1.Height - 25;
                        sourceLeft.Anchor = (AnchorStyles) (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);
                        sourceRight.Dock = DockStyle.None;
                        sourceRight.Top = 25;
                        sourceRight.Left = 0;
                        sourceRight.Width = splitLR.Panel2.Width;
                        sourceRight.Height = splitLR.Panel2.Height - 25;
                        sourceRight.Anchor = (AnchorStyles)(AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top);


                        splitRight.Visible = false;
                        splitLeft.Visible = false;
                        resultLeft.Visible = true;
                        resultRight.Visible = true;

                        tabControl.Visible = true;                        
                        break;
                }
            }
        }

        ~MainForm()
        {
            exitToolStripMenuItem_Click(this, EventArgs.Empty);
        }

        // this method uses Tags defined in UI for some controls!!!
        private void MainForm_Load(object sender, EventArgs e)
        {
            label1.Text = "";
            label12.Text = ResGUIStr.LABEL_CONFIG_FILE;

            // set UI elements from Tags
            cbCaseFixingValidation.Checked = cbCaseFixingValidation.Tag.ToString().Contains(":true");
            cbCreateProcedureInOutput.Checked = cbCreateProcedureInOutput.Tag.ToString().Contains(":true");
            cbFormatOutput.Checked = cbFormatOutput.Tag.ToString().Contains(":true");
            cbGenerateConversionComments.Checked = cbGenerateConversionComments.Tag.ToString().Contains(":false");

            configFileControl.OpenDialogFilter = "Text Files|*.txt|All Files|*.*";
            configFileControl.OpenDialogTitle = ResGUIStr.TITLE_SELECT_CONFIG_FILE;
            inputFileControl.OpenDialogFilter = "Sql Files|*.sql|All Files|*.*";
            inputFileControl.OpenDialogTitle = ResGUIStr.TITLE_SELECT_INPUT_FILE;
            outputFileControl.OpenDialogFilter = "Sql Files|*.sql|All Files|*.*";
            outputFileControl.OpenDialogTitle = ResGUIStr.TITLE_SELECT_OUTPUT_FILE;

            controlsList = new List<Control>();
            controlsList.Add(cbCaseFixingValidation);
            controlsList.Add(cbCreateProcedureInOutput);
            controlsList.Add(cbFormatOutput);
            controlsList.Add(cbGenerateConversionComments);
            controlsList.Add(tbHANAHostname);
            controlsList.Add(tbHANAPassword);
            controlsList.Add(tbHANASchema);
            controlsList.Add(tbHANAUser);
            /*controlsList.Add(tbMSDatabase);
            controlsList.Add(tbMSHostname);
            controlsList.Add(tbMSPassword);
            controlsList.Add(tbMSUser);
            */
            controlsList.Add(tbHANAPort);
            controlsList.Add(tbMSPort);
            controlsList.Add(inputFileControl);
            controlsList.Add(outputFileControl);

            TranslatorTool tool = new TranslatorTool();
            Config.Initialize(null);
            PrepareForm();
            if (Config.InputFile.Length == 0)
            {
                outputFileControl.FileName = "";
            }
            if (Config.isInitialized)
                configFileControl.FileName = Config.configFileName;
            tool.Close();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (sourceLeft.TextLength == 0)
                return;

            Thread thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Start();
        }

        private void HighlightText(RichTextBox textBox)
        {
            string tbxText = textBox.Text.ToUpper();

            textBox.Select(0, tbxText.Length);
            textBox.SelectionColor = Color.Black;
            //Numbers
            System.Text.RegularExpressions.MatchCollection Numbers = System.Text.RegularExpressions.Regex.Matches(tbxText, @"\b[0-9]+\b");
            foreach (System.Text.RegularExpressions.Match match in Numbers)
            {
                string key = match.Groups[0].Value;
                int index = match.Index;
                int startIndex = 0;

                textBox.Select(index, key.Length);
                textBox.SelectionColor = Color.Red;
                startIndex = index + key.Length;
            }

            //Keywords
            List<string> KeyWords = new List<string>() { "ADD", "ALL", "ALTER", "AND", "ANY", "AS", "ASC", "BEGIN", "BETWEEN", "BREAK", "BROWSE", "BULK", "BY", "CASCADE", "CASE", "CHECK", "CLOSE", "CLUSTERED", "COALESCE", 
                    "COLLATE", "COLUMN", "COMMIT", "COMPUTE", "CONSTRAINT", "CONTAINS", "CONTINUE", "CONVERT", "CREATE", "CROSS", "CURRENT", "CURRENT_TIMESTAMP", "CURRENT_USER", "CURSOR", "DEALLOCATE", "DECLARE", 
                    "DEFAULT", "DELETE", "DESC", "DISTINCT", "DOUBLE", "DROP", "ELSE", "END", "ESCAPE", "EXCEPT", "EXEC", "EXECUTE", "EXISTS", "EXTERNAL", "FETCH", "FILLFACTOR", "FOR", "FOREIGN", "FREETEXT", "FROM", "FULL", 
                    "FUNCTION", "GOTO", "GROUP", "HAVING", "HOLDLOCK", "IDENTITY", "IF", "IN", "INDEX", "INNER", "INSERT", "INTERSECT", "INTO", "IS", "JOIN", "KEY", "LEFT", "LIKE", "MERGE", "NATIONAL", "NOCHECK",
                    "NONCLUSTERED", "NOT", "NULL", "NULLIF", "OF", "OFF", "ON", "OPEN", "OPENDATASOURCE", "OPENQUERY", "OPENROWSET", "OPENXML", "OPTION", "OR", "ORDER", "OUTER", "OVER", "PERCENT", "PLAN", "PRECISION", "PRIMARY", 
                    "PRINT", "PROC", "PROCEDURE", "RAISERROR", "REFERENCES", "REPLICATION", "RETURN", "RIGHT", "ROLLBACK", "ROWGUIDCOL", "SAVE", "SELECT", "SESSION_USER", "SET", "SOME", "STATISTICS", "SYSTEM_USER", "TABLE", "TABLESAMPLE", 
                    "THEN", "TO", "TOP", "TRAN", "TRANSACTION", "TRIGGER", "TRUNCATE", "TRY_CONVERT", "UNION", "UNIQUE", "UPDATE", "USE", "USER", "VALUES", "VARYING", "VIEW", "WAITFOR", "WHEN", "WHERE", "WHILE", "WITH" };

            foreach (string keyword in KeyWords)
            {
                //string key = match.Groups[0].Value;
                int index = -1;
                int startIndex = 0;
                while ((index = tbxText.IndexOf(keyword, startIndex)) != -1)
                {
                    int startSpace = index;
                    int endSpace = index;
                    //find space before the match
                    while (startSpace > 0 && !char.IsWhiteSpace(tbxText[startSpace]))
                        startSpace--;
                    //find space after the match
                    while (endSpace < tbxText.Length && !char.IsWhiteSpace(tbxText[endSpace]))
                        endSpace++;
                    if (System.Text.RegularExpressions.Regex.Match(tbxText.Substring(startSpace, endSpace - startSpace), @"\b" + keyword + @"\b").Success)
                    {
                        textBox.Select(index, keyword.Length);
                        textBox.SelectionColor = Color.Blue;
                        startIndex = index + keyword.Length;
                    }
                    else
                    {
                        startIndex = index + keyword.Length;
                        continue;
                    }
                }
            }

            //Strings
            System.Text.RegularExpressions.MatchCollection Strings = System.Text.RegularExpressions.Regex.Matches(tbxText, @"'.+?'");
            foreach (System.Text.RegularExpressions.Match match in Strings)
            {
                string key = match.Groups[0].Value;
                int index = match.Index;
                int startIndex = 0;

                textBox.Select(index, key.Length);
                textBox.SelectionColor = Color.Red;
                startIndex = index + key.Length;
            }

            //Comments
            string commentString = "--";
            int commentIndex = -1;
            int commentstartIndex = 0;
            while (commentstartIndex <= tbxText.Length && (commentIndex = tbxText.IndexOf(commentString, commentstartIndex)) != -1)
            {
                int commentLength = 0;
                int index = commentIndex;
                while (index < tbxText.Length && tbxText[index] != '\n')
                {
                    commentLength++;
                    index++;
                }
                textBox.Select(commentIndex, commentLength);
                textBox.SelectionColor = Color.Green;
                commentstartIndex = commentIndex + commentLength;
            }
            textBox.SelectionStart = 0;
            textBox.SelectionLength = 0;
            textBox.SelectionColor = Color.Black;

            //Multiline comments
            string multiLineStart = "/*";
            string multilineEnd = "*/";
            commentIndex = -1;
            commentstartIndex = 0;
            while (commentstartIndex <= tbxText.Length && (commentIndex = tbxText.IndexOf(multiLineStart, commentstartIndex)) != -1)
            {
                commentstartIndex = commentIndex;
                int endIndex = tbxText.IndexOf(multilineEnd, commentstartIndex + 2);
                if (endIndex != -1)
                {
                    textBox.Select(commentstartIndex, endIndex - commentstartIndex + 2);
                    textBox.SelectionColor = Color.Green;
                    commentstartIndex = endIndex + 2;
                }
                textBox.SelectionStart = 0;
                textBox.SelectionLength = 0;
                textBox.SelectionColor = Color.Black;
            }
        }

        // this method uses Tags defined in UI for some controls!!!
        private void Run()
        {
            string input = "";
            string output = "";

            // this runs on the UI thread, otherwise it would be not thread safe 
            // and we would get errors while debugging btnRun... and label1...
            this.Invoke((MethodInvoker)delegate
            {
                btnRun.Enabled = false;
                label1.Text = ResStr.INF_TRANSLATOR_RUNNING;
                input = sourceLeft.Text;
            });
            
            tool = new TranslatorTool();

            StringBuilder stringBuilder = GenerateCommandLine();
            string[] strings = stringBuilder.ToString().TrimEnd(' ').Split(' ');

            int numOfStatements;
            int numOfErrors;

            try
            {
                tool.ApplyLocalSettings();
                output = tool.RunConversion(strings, input, out lastResult, out numOfStatements, out numOfErrors);
            }
            catch
            {
                output = ResStr.ERR_CRITICAL_SYNTAX_ERROR;
            }

            //MessageBox.Show(lastResult);
            
            tool.Close();

            this.Invoke((MethodInvoker)delegate
            {
                btnRun.Enabled = true;
                label1.Text = "";
                sourceRight.Clear();
                sourceRight.AppendText(output, Color.Black);
                HighlightText(sourceLeft);
                HighlightText(sourceRight);                
                ShowResults();
            });
        }

        // used when creating command line for Run, now commented out because TranslateQuery is used
        private void FillStringBuilder(StringBuilder stringBuilder)
        {
            string str;
            foreach (Control c in controlsList)
            {
                Type t = c.GetType();
                if (c.GetType() == typeof(CheckBox) && c.Tag != null)
                {
                    CheckBox cb = (CheckBox)c;

                    str = cb.Tag.ToString();
                    if (str.Contains("-c"))
                    {
                        if (!cb.Checked)
                        {
                            stringBuilder.Append(str.Substring(str.IndexOf(':') + 1, 2) + " ");
                        }
                    }
                    else
                    {
                        if (cb.Checked)
                        {
                            stringBuilder.Append(str.Substring(str.IndexOf(':') + 1, 2) + " ");
                        }
                    }
                }
                else if (c.GetType() == typeof(TextBox) && c.Tag != null)
                {
                    TextBox tb = (TextBox)c;
                    str = tb.Tag.ToString();
                    if (str.Contains("-s"))
                    {
                        if (tb.Text.Length > 0 && tbHANAPort.Text.Length > 0)
                        {
                            stringBuilder.Append(str.Substring(str.IndexOf(':') + 1, 2) + " " +
                                tb.Text + ":" + tbHANAPort.Text + " ");
                        }
                    }
                    else if (str.Contains("-ms"))
                    {
                        if (tb.Text.Length > 0 && tbMSPort.Text.Length > 0)
                        {
                            stringBuilder.Append(str.Substring(str.IndexOf(':') + 1, 3) + " " +
                                tb.Text + ":" + tbMSPort.Text + " ");
                        }
                    }
                    else if (str.Contains("-m"))
                    {
                        if (tb.Text.Length > 0)
                        {
                            stringBuilder.Append(str.Substring(str.IndexOf(':') + 1, 3) + " " +
                                tb.Text + " ");
                        }
                    }
                    else if (str.Equals("-") || str.Equals("--"))
                    {
                        // ignore
                    }
                    else
                    {
                        if (tb.Text.Length > 0)
                        {
                            stringBuilder.Append(str.Substring(str.IndexOf(':') + 1, 2) + " " +
                                tb.Text + " ");
                        }
                    }
                }
            }
        }

        private void configFileControl_OpenButtonClicked()
        {
            tool = new TranslatorTool(configFileControl.OpenDialogFileName);
            PrepareForm();    
            tool.Close();            
        }

        private void PrepareForm()
        {
            // set UI elements from config file
            cbCaseFixingValidation.Checked = Config.UseCaseFixer;
            cbCreateProcedureInOutput.Checked = Config.CreateProcedure;
            cbFormatOutput.Checked = Config.Formatter;
            cbGenerateConversionComments.Checked = !Config.DisableComments;
            if (Config.DBServer.Length > 0)
            {
                tbHANAHostname.Text = Config.DBServer.Substring(0, Config.DBServer.IndexOf(':'));
                tbHANAPort.Text = Config.DBServer.Substring(Config.DBServer.IndexOf(':') + 1);
            }
            tbHANAPassword.Text = Config.DBPasswd;
            tbHANASchema.Text = Config.DBSchema;
            tbHANAUser.Text = Config.DBUser;
            /*tbMSDatabase.Text = Config.MSDatabase;
            if (Config.MSServer.Length > 0)
            {
                tbMSHostname.Text = Config.MSServer.Substring(0, Config.MSServer.IndexOf(':'));
                tbMSPort.Text = Config.MSServer.Substring(Config.MSServer.IndexOf(':') + 1);
            }
            tbMSPassword.Text = Config.MSPasswd;
            tbMSUser.Text = Config.MSUser;
            */
            inputFileControl.FileName = Config.InputFile;
            outputFileControl.FileName = Config.OutputFile;

            fillSourcePanel(sourceLeft, inputFileControl.FileName);
            fillSourcePanel(sourceRight, outputFileControl.FileName);
            HighlightText(sourceLeft);
            HighlightText(sourceRight);
        }

        // this method uses Tags defined in UI for some controls!!!
        private void configFileControl_SaveButtonClicked()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text Files|*.txt|All Files|*.*";
            dialog.FileName = "config.txt";
            dialog.Title = ResGUIStr.TITLE_SAVE_CONFIG_FILE;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(dialog.FileName, false, Encoding.UTF8);
                FillWriter(writer);
                writer.Close();
                configFileControl.FileName = dialog.FileName;
            }
        }

        private void FillWriter(StreamWriter writer)
        {
            string configWithComments = Config.LoadConfigFileWithComments(configFileControl.FileName);
            string[] separators = { System.Environment.NewLine };
            string[] lines = configWithComments.Split(separators, StringSplitOptions.None);
            
            // hashtable for storing used status of controls
            Hashtable usedControls = new Hashtable(controlsList.Count);
            foreach (Control c in controlsList)
            {
                usedControls.Add(c, false);
            }

            foreach (string line in lines)
            {
                if (line.StartsWith("//"))
                {
                    writer.WriteLine(line);
                }
                else
                {
                    if (line.Length == 0)
                    {
                        continue;
                    }
                    Control c = FindControlFromConfigName(line);
                    if (c == null)
                    {
                        continue;
                    }

                    // mark this control as used
                    usedControls[c] = true;
                    
                    AddOneControlToWriter(writer, c);
                }
            }
            // add all controls not found in the input config file
            foreach (Control uc in usedControls.Keys)
            {
                if (usedControls[uc].Equals(false))
                {
                    // add corresponding config.option to output config file
                    // special case for hostname that splits into server and port in the form
                    if (uc.Tag.ToString().Equals("-") || uc.Tag.ToString().Equals("--"))
                    {
                        // already added with -s or -ms respectively
                    }
                    else
                    {
                        AddOneControlToWriter(writer, uc);
                    }
                }
            }
        }

        private void AddOneControlToWriter(StreamWriter writer, Control c)
        {
            string str = "";

            Type t = c.GetType();
            if (c.GetType() == typeof(CheckBox) && c.Tag != null)
            {
                CheckBox cb = (CheckBox)c;
                str = cb.Tag.ToString();
                if (str.Contains("-c"))
                {
                    writer.WriteLine(str.Substring(0, str.IndexOf(':')) + "=" +
                        (cb.Checked ? "false" : "true"));
                }
                else
                {
                    writer.WriteLine(str.Substring(0, str.IndexOf(':')) + "=" +
                        (cb.Checked ? "true" : "false"));
                }
            }
            else if (c.GetType() == typeof(TextBox) && c.Tag != null)
            {
                TextBox tb = (TextBox)c;
                str = tb.Tag.ToString();
                if (str.Contains("-s"))
                {
                    writer.WriteLine(str.Substring(0, str.IndexOf(':')) + "=" +
                        tb.Text + ":" + tbHANAPort.Text);
                }
                else if (str.Contains("-ms"))
                {
                    writer.WriteLine(str.Substring(0, str.IndexOf(':')) + "=" +
                        tb.Text + ":" + tbMSPort.Text);
                }
                else
                {
                    writer.WriteLine(str.Substring(0, str.IndexOf(':')) + "=" + tb.Text);
                }
            }
            else if (c.GetType() == typeof(CustomFileControl.FileControl) && c.Tag != null)
            {
                CustomFileControl.FileControl fc = (CustomFileControl.FileControl)c;
                str = fc.Tag.ToString();
                writer.WriteLine(str.Substring(0, str.IndexOf(':')) + "=" +
                    fc.FileName);
            }
        }
        
        private Control FindControlFromConfigName(string configName)
        {
            Control outControl;

            foreach (Control c in controlsList)
            {
                if (c.Tag.ToString().StartsWith(configName))
                {
                    outControl = c;
                    return outControl;
                }
            }

            return null;
        }

        private void btnHANATestConnection_Click(object sender, EventArgs e)
        {
            DbUtil util = new DbUtil();
            util.Connect(tbHANAHostname.Text + ":" + tbHANAPort.Text, tbHANASchema.Text,
                tbHANAUser.Text, tbHANAPassword.Text);
            TestConnectionForm form = new TestConnectionForm();
            if (util.IsConnected && util.SchemaExist(tbHANASchema.Text))
            {
                form.SetText("Connection successfull.");
                form.SetDetails(util.ConnectionInfo);
            }
            else
            {
                form.SetText("Connection failed.");
                form.SetDetails(util.ConnectionInfo);
            }
            form.Show();
            util.Dispose();
        }

        private void btnMSTestConnection_Click(object sender, EventArgs e)
        {
            DbUtilMSSQL util = new DbUtilMSSQL();
            util.Connect(tbMSHostname.Text + "," + tbMSPort.Text, 
                tbMSDatabase.Text, tbMSUser.Text, tbMSPassword.Text);
            TestConnectionForm form = new TestConnectionForm();
            if (util.IsConnected && util.DatabaseExist(tbMSDatabase.Text))
            {
                form.SetText("Connection successfull.");
                form.SetDetails(util.ConnectionInfo);
            }
            else
            {
                form.SetText("Connection failed.");
                form.SetDetails(util.ConnectionInfo);
            }
            form.Show();
            util.Dispose();
        }

        private void inputFileControl_OpenButtonClicked()
        {
            fillSourcePanel(sourceLeft, inputFileControl.OpenDialogFileName);
            HighlightText(sourceLeft);
        }

        private void inputFileControl_SaveButtonClicked()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Sql Files|*.sql|All Files|*.*";
            dialog.Title = "Save Input File";
            dialog.FileName = inputFileControl.FileName;
            if (dialog.FileName.Length == 0)
            {
                dialog.FileName = "input.sql";
            }
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(dialog.FileName, false, Encoding.UTF8);
                writer.Write(sourceLeft.Text);
                writer.Close();
                inputFileControl.FileName = dialog.FileName;
            }
        }

        private void outputFileControl_OpenButtonClicked()
        {
            fillSourcePanel(sourceRight, outputFileControl.OpenDialogFileName);
            HighlightText(sourceRight);
        }

        private void outputFileControl_SaveButtonClicked()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Sql Files|*.sql|All Files|*.*";
            dialog.Title = "Save Output File";
            dialog.FileName = outputFileControl.FileName;
            if (dialog.FileName.Length == 0)
            {
                dialog.FileName = "output.sql";
            }
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(dialog.FileName, false, Encoding.UTF8);
                writer.Write(sourceRight.Text); 
                writer.Close();
                outputFileControl.FileName = dialog.FileName;
            }
        }

        private void fillSourcePanel(RichTextBox box, string fileName)
        {
            if (fileName == null)
            {
                return;
            }
            else if (fileName.Equals(""))
            {
                return;
            }
            else if (!File.Exists(fileName))
            {
                return;
            }

            box.Clear();

            List<string> list = new List<string>();
            
            using (StreamReader reader = new StreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line); // Add to list.
                }
            }
            foreach (string str in list)
            {
                box.AppendText(str, Color.Black);
                box.AppendText(Environment.NewLine, Color.Black);
            }
        }

        private void saveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configFileControl_SaveButtonClicked();
        }

        private void loadConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            configFileControl.button1_Click(this, EventArgs.Empty);
            configFileControl_OpenButtonClicked();
        }

        private void loadInputFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inputFileControl.button1_Click(this, EventArgs.Empty);
            inputFileControl_OpenButtonClicked();
        }

        private void saveOutputFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            outputFileControl_SaveButtonClicked();
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StreamWriter writer;
            //exit button sends this as sender object; temporary solution how no to save config file when exit button is hit
            if (sender != this)
            {
                if (configFileControl.FileName != null && configFileControl.FileName.Length > 0)
                {
                    try
                    {
                        writer = new StreamWriter(configFileControl.FileName, false, Encoding.UTF8);
                        FillWriter(writer);
                        writer.Close();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        MessageBox.Show(ResStr.MSG_CONFIG_FILE_NOT_SAVED);
                    }
                }
                else
                {
                    configFileControl_SaveButtonClicked();
                }
            }
            if (inputFileControl.FileName != null && inputFileControl.FileName.Length > 0)
            {
                try
                {
                    writer = new StreamWriter(inputFileControl.FileName, false, Encoding.UTF8);
                    writer.Write(sourceLeft.Text);
                    writer.Close();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(ResStr.MSG_INPUT_FILE_NOT_SAVED);
                }
            }
            else
            {
                inputFileControl_SaveButtonClicked();
            }
            if (outputFileControl.FileName != null && outputFileControl.FileName.Length > 0)
            {
                try
                {
                    writer = new StreamWriter(outputFileControl.FileName, false, Encoding.UTF8);
                    writer.Write(sourceRight.Text);
                    writer.Close();
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(ResStr.MSG_OUTPUT_FILE_NOT_SAVED);
                }
            }
            else
            {
                outputFileControl_SaveButtonClicked();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAllToolStripMenuItem_Click(this, EventArgs.Empty);
            Application.Exit();
        }

        private void runConversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnRun_Click(sender, e);
        }

        private void generateCommandLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder stringBuilder = GenerateCommandLine();
            CommandLineForm f = new CommandLineForm();
            f.SetText(stringBuilder.ToString());
            f.Show();
        }

        private void aboutHANATranslatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm f = new AboutForm();
            f.SetText(ResGUIStr.TITLE_SQL_CONVERTER_NAME + Environment.NewLine + ResGUIStr.ABOUT_TRADE_MARK);
            f.Show();
        }

        private void btnGenerateCommandLine_Click(object sender, EventArgs e)
        {
            StringBuilder stringBuilder = GenerateCommandLine();
            CommandLineForm f = new CommandLineForm();
            f.SetText("Converter.exe " + stringBuilder.ToString());
            f.Show();
        }

        private StringBuilder GenerateCommandLine()
        {
            StringBuilder stringBuilder = new StringBuilder();
            FillStringBuilder(stringBuilder);
            return stringBuilder;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ShowResults();
        }

        private void ShowResults()
        {
            if (lastResult == null)
                return;

            ResultsForm f = new ResultsForm();
            f.SetText(lastResult);
            f.Show();
        }

        private void selectFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Font f = sourceLeft.Font;
            Color c = sourceLeft.ForeColor;

            fontDialog1.Font = f;
            fontDialog1.ShowApply = true;
            fontDialog1.ShowEffects = false;
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                sourceLeft.Font = fontDialog1.Font;
                sourceRight.Font = fontDialog1.Font;
            }
        }

        private void fontDialog1_Apply(object sender, EventArgs e)
        {
            sourceLeft.Font = fontDialog1.Font;
            sourceRight.Font = fontDialog1.Font;
        }

		private void splitRight_SplitterMoved(object sender, SplitterEventArgs e)
        {
            splitLeft.SplitterDistance = splitRight.SplitterDistance;            
        }

        private void splitLeft_SplitterMoved(object sender, SplitterEventArgs e)
        {
            splitRight.SplitterDistance = splitLeft.SplitterDistance;
        }

        private void viewChange_Click(object sender, EventArgs e)
        {
            switch (currentFormLayout)
            {
                case FormLayout.fl_Edit:
                    SetLayout(FormLayout.fl_EditResult);
                    break;

                case FormLayout.fl_EditResult:
                    SetLayout(FormLayout.fl_Tab);
                    break;

                case FormLayout.fl_Tab:
                    SetLayout(FormLayout.fl_Edit);
                    break;
            }            
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == 0)
            {
                splitLR.SplitterDistance = splitResult.SplitterDistance;
            }
            else
            {
                splitResult.SplitterDistance = splitLR.SplitterDistance;
            }
		}

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (toolStripButton3.Checked == true)
            {
                toolStripButton3.Checked = false;
                configFileControl.Visible = false;
                groupBox1.Visible = false;
                groupBox2.Visible = false;
                //groupBox3.Visible = false;
            }
            else
            {
                toolStripButton3.Checked = true;
                configFileControl.Visible = true;
                groupBox1.Visible = true;
                groupBox2.Visible = true;
                //groupBox3.Visible = true;
            }
            SetEditAreaSize();
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            SetEditAreaSize();
        }

        private void SetEditAreaSize()
        {
            if (toolStripButton3.Checked == false)
            {
                editArea.Location = new Point(12, 49 );
                editArea.Height = this.Height - 49 - button3.Height - 55;
                editArea.Width = this.Width - 40;
            }
            else
            {
                editArea.Location = new Point(12, 49 + configFileControl.Height + groupBox1.Height + 20);
                editArea.Height = this.Height - editArea.Location.Y - button3.Height - 55;
                editArea.Width = this.Width - 40;
            }
        }
    }

    // this is an extension to richtextbox that enables text coloring per appended strings
    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
    
}
