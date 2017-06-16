namespace Translator
{
    partial class MainForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveConfigToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadInputFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveOutputFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runConversionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateCommandLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutHANATranslatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnRun = new System.Windows.Forms.ToolStripButton();
            this.btnGenerateCommandLine = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.cbGenerateConversionComments = new System.Windows.Forms.CheckBox();
            this.cbCreateProcedureInOutput = new System.Windows.Forms.CheckBox();
            this.cbFormatOutput = new System.Windows.Forms.CheckBox();
            this.cbCaseFixingValidation = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnHANATestConnection = new System.Windows.Forms.Button();
            this.tbHANASchema = new System.Windows.Forms.TextBox();
            this.tbHANAPort = new System.Windows.Forms.TextBox();
            this.tbHANAPassword = new System.Windows.Forms.TextBox();
            this.tbHANAUser = new System.Windows.Forms.TextBox();
            this.tbHANAHostname = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnMSTestConnection = new System.Windows.Forms.Button();
            this.tbMSDatabase = new System.Windows.Forms.TextBox();
            this.tbMSPort = new System.Windows.Forms.TextBox();
            this.tbMSPassword = new System.Windows.Forms.TextBox();
            this.tbMSUser = new System.Windows.Forms.TextBox();
            this.tbMSHostname = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabSource = new System.Windows.Forms.TabPage();
            this.splitLR = new System.Windows.Forms.SplitContainer();
            this.inputFileControl = new CustomFileControl.FileControl();
            this.splitLeft = new System.Windows.Forms.SplitContainer();
            this.sourceLeft = new System.Windows.Forms.RichTextBox();
            this.resultLeft = new System.Windows.Forms.DataGridView();
            this.outputFileControl = new CustomFileControl.FileControl();
            this.splitRight = new System.Windows.Forms.SplitContainer();
            this.sourceRight = new System.Windows.Forms.RichTextBox();
            this.resultRight = new System.Windows.Forms.DataGridView();
            this.tabResult = new System.Windows.Forms.TabPage();
            this.splitResult = new System.Windows.Forms.SplitContainer();
            this.editArea = new System.Windows.Forms.Panel();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.configFileControl = new CustomFileControl.FileControl();
            this.label12 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabSource.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLR)).BeginInit();
            this.splitLR.Panel1.SuspendLayout();
            this.splitLR.Panel2.SuspendLayout();
            this.splitLR.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).BeginInit();
            this.splitLeft.Panel1.SuspendLayout();
            this.splitLeft.Panel2.SuspendLayout();
            this.splitLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).BeginInit();
            this.splitRight.Panel1.SuspendLayout();
            this.splitRight.Panel2.SuspendLayout();
            this.splitRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.resultRight)).BeginInit();
            this.tabResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitResult)).BeginInit();
            this.splitResult.SuspendLayout();
            this.editArea.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.commendToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1169, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadConfigToolStripMenuItem,
            this.saveConfigToolStripMenuItem,
            this.loadInputFileToolStripMenuItem,
            this.saveOutputFileToolStripMenuItem,
            this.saveAllToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadConfigToolStripMenuItem
            // 
            this.loadConfigToolStripMenuItem.Name = "loadConfigToolStripMenuItem";
            this.loadConfigToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.loadConfigToolStripMenuItem.Text = "Select Configuration File";
            this.loadConfigToolStripMenuItem.Click += new System.EventHandler(this.loadConfigToolStripMenuItem_Click);
            // 
            // saveConfigToolStripMenuItem
            // 
            this.saveConfigToolStripMenuItem.Name = "saveConfigToolStripMenuItem";
            this.saveConfigToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.saveConfigToolStripMenuItem.Text = "Save Configuration File";
            this.saveConfigToolStripMenuItem.Click += new System.EventHandler(this.saveConfigToolStripMenuItem_Click);
            // 
            // loadInputFileToolStripMenuItem
            // 
            this.loadInputFileToolStripMenuItem.Name = "loadInputFileToolStripMenuItem";
            this.loadInputFileToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.loadInputFileToolStripMenuItem.Text = "Select Input File";
            this.loadInputFileToolStripMenuItem.Click += new System.EventHandler(this.loadInputFileToolStripMenuItem_Click);
            // 
            // saveOutputFileToolStripMenuItem
            // 
            this.saveOutputFileToolStripMenuItem.Name = "saveOutputFileToolStripMenuItem";
            this.saveOutputFileToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.saveOutputFileToolStripMenuItem.Text = "Save Output File";
            this.saveOutputFileToolStripMenuItem.Click += new System.EventHandler(this.saveOutputFileToolStripMenuItem_Click);
            // 
            // saveAllToolStripMenuItem
            // 
            this.saveAllToolStripMenuItem.Name = "saveAllToolStripMenuItem";
            this.saveAllToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.saveAllToolStripMenuItem.Text = "Save All";
            this.saveAllToolStripMenuItem.Click += new System.EventHandler(this.saveAllToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(203, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // commendToolStripMenuItem
            // 
            this.commendToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runConversionToolStripMenuItem,
            this.generateCommandLineToolStripMenuItem,
            this.selectFontToolStripMenuItem});
            this.commendToolStripMenuItem.Name = "commendToolStripMenuItem";
            this.commendToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.commendToolStripMenuItem.Text = "Command";
            // 
            // runConversionToolStripMenuItem
            // 
            this.runConversionToolStripMenuItem.Image = global::Translator.Properties.Resources.execute;
            this.runConversionToolStripMenuItem.Name = "runConversionToolStripMenuItem";
            this.runConversionToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.runConversionToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.runConversionToolStripMenuItem.Text = "Run Converter";
            this.runConversionToolStripMenuItem.Click += new System.EventHandler(this.runConversionToolStripMenuItem_Click);
            // 
            // generateCommandLineToolStripMenuItem
            // 
            this.generateCommandLineToolStripMenuItem.Image = global::Translator.Properties.Resources.file_bin;
            this.generateCommandLineToolStripMenuItem.Name = "generateCommandLineToolStripMenuItem";
            this.generateCommandLineToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.generateCommandLineToolStripMenuItem.Text = "Generate Command Line";
            this.generateCommandLineToolStripMenuItem.Click += new System.EventHandler(this.generateCommandLineToolStripMenuItem_Click);
            // 
            // selectFontToolStripMenuItem
            // 
            this.selectFontToolStripMenuItem.Name = "selectFontToolStripMenuItem";
            this.selectFontToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.selectFontToolStripMenuItem.Text = "Font";
            this.selectFontToolStripMenuItem.Click += new System.EventHandler(this.selectFontToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutHANATranslatorToolStripMenuItem});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.aboutToolStripMenuItem.Text = "About";
            // 
            // aboutHANATranslatorToolStripMenuItem
            // 
            this.aboutHANATranslatorToolStripMenuItem.Name = "aboutHANATranslatorToolStripMenuItem";
            this.aboutHANATranslatorToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.aboutHANATranslatorToolStripMenuItem.Text = "About SQL Converter";
            this.aboutHANATranslatorToolStripMenuItem.Click += new System.EventHandler(this.aboutHANATranslatorToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRun,
            this.btnGenerateCommandLine,
            this.toolStripSeparator1,
            this.toolStripButton3,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1169, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnRun
            // 
            this.btnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRun.Image = global::Translator.Properties.Resources.execute;
            this.btnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(23, 22);
            this.btnRun.Text = "Run Converter";
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnGenerateCommandLine
            // 
            this.btnGenerateCommandLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnGenerateCommandLine.Image = global::Translator.Properties.Resources.file_bin;
            this.btnGenerateCommandLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnGenerateCommandLine.Name = "btnGenerateCommandLine";
            this.btnGenerateCommandLine.Size = new System.Drawing.Size(23, 22);
            this.btnGenerateCommandLine.Text = "Generate Command Line";
            this.btnGenerateCommandLine.Click += new System.EventHandler(this.btnGenerateCommandLine_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.Checked = true;
            this.toolStripButton3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = global::Translator.Properties.Resources.administration;
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "Show/Hide Configuration Area";
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::Translator.Properties.Resources.administration;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Change Layout";
            this.toolStripButton1.Visible = false;
            this.toolStripButton1.Click += new System.EventHandler(this.viewChange_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox5);
            this.groupBox1.Controls.Add(this.cbGenerateConversionComments);
            this.groupBox1.Controls.Add(this.cbCreateProcedureInOutput);
            this.groupBox1.Controls.Add(this.cbFormatOutput);
            this.groupBox1.Controls.Add(this.cbCaseFixingValidation);
            this.groupBox1.Location = new System.Drawing.Point(13, 93);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(379, 125);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General Settings";
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(201, 20);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(110, 17);
            this.checkBox5.TabIndex = 1;
            this.checkBox5.Text = "Result verification";
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.Visible = false;
            // 
            // cbGenerateConversionComments
            // 
            this.cbGenerateConversionComments.AutoSize = true;
            this.cbGenerateConversionComments.Location = new System.Drawing.Point(7, 89);
            this.cbGenerateConversionComments.Name = "cbGenerateConversionComments";
            this.cbGenerateConversionComments.Size = new System.Drawing.Size(170, 17);
            this.cbGenerateConversionComments.TabIndex = 0;
            this.cbGenerateConversionComments.Tag = "DisableComments:-c:false";
            this.cbGenerateConversionComments.Text = "Provide Conversion Comments";
            this.cbGenerateConversionComments.UseVisualStyleBackColor = true;
            // 
            // cbCreateProcedureInOutput
            // 
            this.cbCreateProcedureInOutput.AutoSize = true;
            this.cbCreateProcedureInOutput.Location = new System.Drawing.Point(7, 66);
            this.cbCreateProcedureInOutput.Name = "cbCreateProcedureInOutput";
            this.cbCreateProcedureInOutput.Size = new System.Drawing.Size(114, 17);
            this.cbCreateProcedureInOutput.TabIndex = 0;
            this.cbCreateProcedureInOutput.Tag = "CreateProcedure:-P:false";
            this.cbCreateProcedureInOutput.Text = "Create Procedures";
            this.cbCreateProcedureInOutput.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.cbCreateProcedureInOutput.UseVisualStyleBackColor = true;
            // 
            // cbFormatOutput
            // 
            this.cbFormatOutput.AutoSize = true;
            this.cbFormatOutput.Location = new System.Drawing.Point(7, 43);
            this.cbFormatOutput.Name = "cbFormatOutput";
            this.cbFormatOutput.Size = new System.Drawing.Size(93, 17);
            this.cbFormatOutput.TabIndex = 0;
            this.cbFormatOutput.Tag = "Formatter:-F:false";
            this.cbFormatOutput.Text = "Format Output";
            this.cbFormatOutput.UseVisualStyleBackColor = true;
            // 
            // cbCaseFixingValidation
            // 
            this.cbCaseFixingValidation.AutoSize = true;
            this.cbCaseFixingValidation.Location = new System.Drawing.Point(7, 20);
            this.cbCaseFixingValidation.Name = "cbCaseFixingValidation";
            this.cbCaseFixingValidation.Size = new System.Drawing.Size(97, 17);
            this.cbCaseFixingValidation.TabIndex = 0;
            this.cbCaseFixingValidation.Tag = "UseCaseFixer:-f:false";
            this.cbCaseFixingValidation.Text = "Use Case Fixer";
            this.cbCaseFixingValidation.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnHANATestConnection);
            this.groupBox2.Controls.Add(this.tbHANASchema);
            this.groupBox2.Controls.Add(this.tbHANAPort);
            this.groupBox2.Controls.Add(this.tbHANAPassword);
            this.groupBox2.Controls.Add(this.tbHANAUser);
            this.groupBox2.Controls.Add(this.tbHANAHostname);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(398, 93);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(363, 125);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "SAP HANA Server";
            // 
            // btnHANATestConnection
            // 
            this.btnHANATestConnection.Location = new System.Drawing.Point(257, 100);
            this.btnHANATestConnection.Name = "btnHANATestConnection";
            this.btnHANATestConnection.Size = new System.Drawing.Size(105, 23);
            this.btnHANATestConnection.TabIndex = 10;
            this.btnHANATestConnection.Text = "Test Connection";
            this.btnHANATestConnection.UseVisualStyleBackColor = true;
            this.btnHANATestConnection.Click += new System.EventHandler(this.btnHANATestConnection_Click);
            // 
            // tbHANASchema
            // 
            this.tbHANASchema.Location = new System.Drawing.Point(258, 47);
            this.tbHANASchema.Name = "tbHANASchema";
            this.tbHANASchema.Size = new System.Drawing.Size(85, 20);
            this.tbHANASchema.TabIndex = 9;
            this.tbHANASchema.Tag = "DBSchema:-d";
            // 
            // tbHANAPort
            // 
            this.tbHANAPort.Location = new System.Drawing.Point(257, 18);
            this.tbHANAPort.Name = "tbHANAPort";
            this.tbHANAPort.Size = new System.Drawing.Size(86, 20);
            this.tbHANAPort.TabIndex = 8;
            this.tbHANAPort.Tag = "-";
            // 
            // tbHANAPassword
            // 
            this.tbHANAPassword.Location = new System.Drawing.Point(106, 75);
            this.tbHANAPassword.Name = "tbHANAPassword";
            this.tbHANAPassword.PasswordChar = '*';
            this.tbHANAPassword.Size = new System.Drawing.Size(87, 20);
            this.tbHANAPassword.TabIndex = 7;
            this.tbHANAPassword.Tag = "DBPasswd:-p";
            // 
            // tbHANAUser
            // 
            this.tbHANAUser.Location = new System.Drawing.Point(106, 47);
            this.tbHANAUser.Name = "tbHANAUser";
            this.tbHANAUser.Size = new System.Drawing.Size(87, 20);
            this.tbHANAUser.TabIndex = 6;
            this.tbHANAUser.Tag = "DBUser:-u";
            // 
            // tbHANAHostname
            // 
            this.tbHANAHostname.Location = new System.Drawing.Point(107, 18);
            this.tbHANAHostname.Name = "tbHANAHostname";
            this.tbHANAHostname.Size = new System.Drawing.Size(86, 20);
            this.tbHANAHostname.TabIndex = 5;
            this.tbHANAHostname.Tag = "DBServer:-s";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(211, 51);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(46, 13);
            this.label6.TabIndex = 4;
            this.label6.Text = "Schema";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(212, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "Port";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 79);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "DB User Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "DB User Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Server Name";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnMSTestConnection);
            this.groupBox3.Controls.Add(this.tbMSDatabase);
            this.groupBox3.Controls.Add(this.tbMSPort);
            this.groupBox3.Controls.Add(this.tbMSPassword);
            this.groupBox3.Controls.Add(this.tbMSUser);
            this.groupBox3.Controls.Add(this.tbMSHostname);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Location = new System.Drawing.Point(767, 93);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(390, 125);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "MS SQL Server";
            this.groupBox3.Visible = false;
            // 
            // btnMSTestConnection
            // 
            this.btnMSTestConnection.Location = new System.Drawing.Point(284, 100);
            this.btnMSTestConnection.Name = "btnMSTestConnection";
            this.btnMSTestConnection.Size = new System.Drawing.Size(105, 23);
            this.btnMSTestConnection.TabIndex = 21;
            this.btnMSTestConnection.Text = "Test Connection";
            this.btnMSTestConnection.UseVisualStyleBackColor = true;
            this.btnMSTestConnection.Click += new System.EventHandler(this.btnMSTestConnection_Click);
            // 
            // tbMSDatabase
            // 
            this.tbMSDatabase.Location = new System.Drawing.Point(281, 47);
            this.tbMSDatabase.Name = "tbMSDatabase";
            this.tbMSDatabase.Size = new System.Drawing.Size(85, 20);
            this.tbMSDatabase.TabIndex = 20;
            this.tbMSDatabase.Tag = "MSDatabase:-md";
            // 
            // tbMSPort
            // 
            this.tbMSPort.Location = new System.Drawing.Point(280, 18);
            this.tbMSPort.Name = "tbMSPort";
            this.tbMSPort.Size = new System.Drawing.Size(86, 20);
            this.tbMSPort.TabIndex = 19;
            this.tbMSPort.Tag = "--";
            // 
            // tbMSPassword
            // 
            this.tbMSPassword.Location = new System.Drawing.Point(106, 75);
            this.tbMSPassword.Name = "tbMSPassword";
            this.tbMSPassword.PasswordChar = '*';
            this.tbMSPassword.Size = new System.Drawing.Size(87, 20);
            this.tbMSPassword.TabIndex = 18;
            this.tbMSPassword.Tag = "MSPasswd:-mp";
            // 
            // tbMSUser
            // 
            this.tbMSUser.Location = new System.Drawing.Point(106, 47);
            this.tbMSUser.Name = "tbMSUser";
            this.tbMSUser.Size = new System.Drawing.Size(87, 20);
            this.tbMSUser.TabIndex = 17;
            this.tbMSUser.Tag = "MSUser:-mu";
            // 
            // tbMSHostname
            // 
            this.tbMSHostname.Location = new System.Drawing.Point(107, 18);
            this.tbMSHostname.Name = "tbMSHostname";
            this.tbMSHostname.Size = new System.Drawing.Size(86, 20);
            this.tbMSHostname.TabIndex = 16;
            this.tbMSHostname.Tag = "MSServer:-ms";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(211, 51);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Database";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(212, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(26, 13);
            this.label8.TabIndex = 14;
            this.label8.Text = "Port";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 79);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(96, 13);
            this.label9.TabIndex = 13;
            this.label9.Text = "DB User Password";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 51);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(78, 13);
            this.label10.TabIndex = 12;
            this.label10.Text = "DB User Name";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(7, 22);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(69, 13);
            this.label11.TabIndex = 11;
            this.label11.Text = "Server Name";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 609);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(1033, 614);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(124, 23);
            this.button3.TabIndex = 10;
            this.button3.Text = "Conversion Report";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(903, 614);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(124, 23);
            this.button4.TabIndex = 9;
            this.button4.Text = "Run verification";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Visible = false;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabSource);
            this.tabControl.Controls.Add(this.tabResult);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1142, 364);
            this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.tabControl.TabIndex = 11;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabSource
            // 
            this.tabSource.Controls.Add(this.splitLR);
            this.tabSource.Location = new System.Drawing.Point(4, 22);
            this.tabSource.Name = "tabSource";
            this.tabSource.Padding = new System.Windows.Forms.Padding(3);
            this.tabSource.Size = new System.Drawing.Size(1134, 338);
            this.tabSource.TabIndex = 0;
            this.tabSource.Text = "Source";
            this.tabSource.UseVisualStyleBackColor = true;
            // 
            // splitLR
            // 
            this.splitLR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitLR.Location = new System.Drawing.Point(3, 3);
            this.splitLR.Name = "splitLR";
            // 
            // splitLR.Panel1
            // 
            this.splitLR.Panel1.Controls.Add(this.inputFileControl);
            this.splitLR.Panel1.Controls.Add(this.splitLeft);
            // 
            // splitLR.Panel2
            // 
            this.splitLR.Panel2.Controls.Add(this.outputFileControl);
            this.splitLR.Panel2.Controls.Add(this.splitRight);
            this.splitLR.Size = new System.Drawing.Size(1128, 332);
            this.splitLR.SplitterDistance = 501;
            this.splitLR.TabIndex = 0;
            // 
            // inputFileControl
            // 
            this.inputFileControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.inputFileControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.inputFileControl.FileName = "";
            this.inputFileControl.Location = new System.Drawing.Point(0, 0);
            this.inputFileControl.Margin = new System.Windows.Forms.Padding(0);
            this.inputFileControl.Name = "inputFileControl";
            this.inputFileControl.OpenDialogFileName = "";
            this.inputFileControl.OpenDialogFilter = "All Files|*.*";
            this.inputFileControl.OpenDialogTitle = "Select a File";
            this.inputFileControl.Size = new System.Drawing.Size(501, 22);
            this.inputFileControl.TabIndex = 1;
            this.inputFileControl.Tag = "InputFile:-i";
            this.inputFileControl.OpenButtonClicked += new CustomFileControl.FileControl.OpenClickedHandler(this.inputFileControl_OpenButtonClicked);
            this.inputFileControl.SaveButtonClicked += new CustomFileControl.FileControl.SaveClickedHandler(this.inputFileControl_SaveButtonClicked);
            // 
            // splitLeft
            // 
            this.splitLeft.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitLeft.Location = new System.Drawing.Point(0, 25);
            this.splitLeft.Name = "splitLeft";
            this.splitLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitLeft.Panel1
            // 
            this.splitLeft.Panel1.Controls.Add(this.sourceLeft);
            // 
            // splitLeft.Panel2
            // 
            this.splitLeft.Panel2.Controls.Add(this.resultLeft);
            this.splitLeft.Size = new System.Drawing.Size(501, 307);
            this.splitLeft.SplitterDistance = 230;
            this.splitLeft.TabIndex = 0;
            this.splitLeft.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitLeft_SplitterMoved);
            // 
            // sourceLeft
            // 
            this.sourceLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sourceLeft.Location = new System.Drawing.Point(0, 0);
            this.sourceLeft.Name = "sourceLeft";
            this.sourceLeft.Size = new System.Drawing.Size(501, 230);
            this.sourceLeft.TabIndex = 0;
            this.sourceLeft.Text = "";
            // 
            // resultLeft
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.resultLeft.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.resultLeft.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.resultLeft.DefaultCellStyle = dataGridViewCellStyle2;
            this.resultLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultLeft.Location = new System.Drawing.Point(0, 0);
            this.resultLeft.Name = "resultLeft";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.resultLeft.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.resultLeft.Size = new System.Drawing.Size(501, 73);
            this.resultLeft.TabIndex = 0;
            // 
            // outputFileControl
            // 
            this.outputFileControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.outputFileControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.outputFileControl.FileName = "";
            this.outputFileControl.Location = new System.Drawing.Point(0, 0);
            this.outputFileControl.Margin = new System.Windows.Forms.Padding(0);
            this.outputFileControl.Name = "outputFileControl";
            this.outputFileControl.OpenDialogFileName = "";
            this.outputFileControl.OpenDialogFilter = "All Files|*.*";
            this.outputFileControl.OpenDialogTitle = "Select a File";
            this.outputFileControl.Size = new System.Drawing.Size(623, 22);
            this.outputFileControl.TabIndex = 1;
            this.outputFileControl.Tag = "OutputFile:-o";
            this.outputFileControl.OpenButtonClicked += new CustomFileControl.FileControl.OpenClickedHandler(this.outputFileControl_OpenButtonClicked);
            this.outputFileControl.SaveButtonClicked += new CustomFileControl.FileControl.SaveClickedHandler(this.outputFileControl_SaveButtonClicked);
            // 
            // splitRight
            // 
            this.splitRight.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitRight.Location = new System.Drawing.Point(0, 25);
            this.splitRight.Name = "splitRight";
            this.splitRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitRight.Panel1
            // 
            this.splitRight.Panel1.Controls.Add(this.sourceRight);
            // 
            // splitRight.Panel2
            // 
            this.splitRight.Panel2.Controls.Add(this.resultRight);
            this.splitRight.Size = new System.Drawing.Size(623, 307);
            this.splitRight.SplitterDistance = 229;
            this.splitRight.TabIndex = 0;
            this.splitRight.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitRight_SplitterMoved);
            // 
            // sourceRight
            // 
            this.sourceRight.Location = new System.Drawing.Point(0, 0);
            this.sourceRight.Name = "sourceRight";
            this.sourceRight.Size = new System.Drawing.Size(623, 229);
            this.sourceRight.TabIndex = 0;
            this.sourceRight.Text = "";
            // 
            // resultRight
            // 
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.resultRight.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.resultRight.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.resultRight.DefaultCellStyle = dataGridViewCellStyle5;
            this.resultRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultRight.Location = new System.Drawing.Point(0, 0);
            this.resultRight.Name = "resultRight";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.resultRight.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.resultRight.Size = new System.Drawing.Size(623, 74);
            this.resultRight.TabIndex = 0;
            // 
            // tabResult
            // 
            this.tabResult.Controls.Add(this.splitResult);
            this.tabResult.Location = new System.Drawing.Point(4, 22);
            this.tabResult.Name = "tabResult";
            this.tabResult.Padding = new System.Windows.Forms.Padding(3);
            this.tabResult.Size = new System.Drawing.Size(1134, 338);
            this.tabResult.TabIndex = 1;
            this.tabResult.Text = "Result";
            this.tabResult.UseVisualStyleBackColor = true;
            // 
            // splitResult
            // 
            this.splitResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitResult.Location = new System.Drawing.Point(3, 3);
            this.splitResult.Name = "splitResult";
            this.splitResult.Size = new System.Drawing.Size(1128, 363);
            this.splitResult.SplitterDistance = 560;
            this.splitResult.TabIndex = 0;
            // 
            // editArea
            // 
            this.editArea.Controls.Add(this.tabControl);
            this.editArea.Location = new System.Drawing.Point(15, 221);
            this.editArea.Name = "editArea";
            this.editArea.Size = new System.Drawing.Size(1142, 364);
            this.editArea.TabIndex = 12;
            // 
            // fontDialog1
            // 
            this.fontDialog1.Apply += new System.EventHandler(this.fontDialog1_Apply);
            // 
            // configFileControl
            // 
            this.configFileControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.configFileControl.FileName = "";
            this.configFileControl.Location = new System.Drawing.Point(16, 63);
            this.configFileControl.Margin = new System.Windows.Forms.Padding(0);
            this.configFileControl.Name = "configFileControl";
            this.configFileControl.OpenDialogFileName = "";
            this.configFileControl.OpenDialogFilter = "All Files|*.*";
            this.configFileControl.OpenDialogTitle = "Select a File";
            this.configFileControl.Size = new System.Drawing.Size(745, 22);
            this.configFileControl.TabIndex = 0;
            this.configFileControl.OpenButtonClicked += new CustomFileControl.FileControl.OpenClickedHandler(this.configFileControl_OpenButtonClicked);
            this.configFileControl.SaveButtonClicked += new CustomFileControl.FileControl.SaveClickedHandler(this.configFileControl_SaveButtonClicked);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(13, 49);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(41, 13);
            this.label12.TabIndex = 13;
            this.label12.Text = "label12";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1169, 649);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.configFileControl);
            this.Controls.Add(this.editArea);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Name = "MainForm";
            this.Text = "SQL Converter";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabSource.ResumeLayout(false);
            this.splitLR.Panel1.ResumeLayout(false);
            this.splitLR.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLR)).EndInit();
            this.splitLR.ResumeLayout(false);
            this.splitLeft.Panel1.ResumeLayout(false);
            this.splitLeft.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitLeft)).EndInit();
            this.splitLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.resultLeft)).EndInit();
            this.splitRight.Panel1.ResumeLayout(false);
            this.splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitRight)).EndInit();
            this.splitRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.resultRight)).EndInit();
            this.tabResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitResult)).EndInit();
            this.splitResult.ResumeLayout(false);
            this.editArea.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem commendToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnRun;
        private System.Windows.Forms.ToolStripButton btnGenerateCommandLine;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox cbGenerateConversionComments;
        private System.Windows.Forms.CheckBox cbCreateProcedureInOutput;
        private System.Windows.Forms.CheckBox cbFormatOutput;
        private System.Windows.Forms.CheckBox cbCaseFixingValidation;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox tbHANASchema;
        private System.Windows.Forms.TextBox tbHANAPort;
        private System.Windows.Forms.TextBox tbHANAPassword;
        private System.Windows.Forms.TextBox tbHANAUser;
        private System.Windows.Forms.TextBox tbHANAHostname;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnHANATestConnection;
        private CustomFileControl.FileControl configFileControl;
		private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSource;
        private System.Windows.Forms.SplitContainer splitLR;
        private System.Windows.Forms.SplitContainer splitLeft;
        private System.Windows.Forms.DataGridView resultLeft;
        private System.Windows.Forms.SplitContainer splitRight;
        private System.Windows.Forms.DataGridView resultRight;
        private System.Windows.Forms.TabPage tabResult;
        private System.Windows.Forms.Button btnMSTestConnection;
        private System.Windows.Forms.TextBox tbMSDatabase;
        private System.Windows.Forms.TextBox tbMSPort;
        private System.Windows.Forms.TextBox tbMSPassword;
        private System.Windows.Forms.TextBox tbMSUser;
        private System.Windows.Forms.TextBox tbMSHostname;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private CustomFileControl.FileControl inputFileControl;
        private CustomFileControl.FileControl outputFileControl;
        private System.Windows.Forms.RichTextBox sourceLeft;
        private System.Windows.Forms.ToolStripMenuItem loadConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveConfigToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadInputFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveOutputFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runConversionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateCommandLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutHANATranslatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem selectFontToolStripMenuItem;
        private System.Windows.Forms.FontDialog fontDialog1;
		private System.Windows.Forms.Panel editArea;
        private System.Windows.Forms.SplitContainer splitResult;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.RichTextBox sourceRight;
        private System.Windows.Forms.Label label12;
    }
}
