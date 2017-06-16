using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Antlr.Runtime;
using System.Globalization;
using System.Threading;

namespace Translator
{

    public class TranslatorTool
    {
        public TranslatorTool()
        {            
        }

        public TranslatorTool(string configPath)
        {
            Config.Initialize(null, configPath);
            ApplyLocalSettings();
        }

        public bool ProcessConfigFile(string[] args, out bool startUI)
        {
            startUI = false;
            Config.Initialize(args);

            if (Config.Version)
            {
                Config.DisplayVersion();
                return true;
            }

            ApplyLocalSettings();

            if (Config.UseGUI == true)
            {
                startUI = true;
                return true;
            }

            if (Config.Help || Config.InputFile == "")
            {
                Config.DisplayHelp();
                return true;
            }

            if (Config.CreateCommandLine)
            {
                string CommandLine = Config.GenerateCommandLine();
                Console.WriteLine(CommandLine);
                return true;
            }

            if (Config.OutputFile == String.Empty && Config.DumpFile == String.Empty)
            {
                Config.DisplayHelp();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Main conversion function when used in GUI mode
        /// </summary>
        /// <param name="args"></param>
        /// <param name="inputQuery"></param>
        /// <param name="resultSummary"></param>
        /// <param name="numOfStatements"></param>
        /// <param name="numOfErrors"></param>
        /// <returns></returns>
        public string RunConversion(string[] args, string inputQuery, out string resultSummary, out int numOfStatements, out int numOfErrors)
        {
            Config.Initialize(args, null);
            ApplyLocalSettings();

            if (inputQuery == null || inputQuery.Length == 0)
            {
                resultSummary = "";
                numOfErrors = 0;
                numOfStatements = 0;
                return "";
            }

            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);

            StringBuilder sbSummary = new StringBuilder();
            StringWriter writerSummary = new StringWriter(sbSummary);
            StatusReporter.Initialize(writerSummary);

            numOfStatements = TranslateQueryInt(writer, inputQuery, writerSummary);

            writer.Close();
            writerSummary.Close();

            numOfErrors = 0;
            resultSummary = writerSummary.ToString();
            return sb.ToString();
        }

        /// <summary>
        /// API function for usage in add-ons
        /// </summary>
        /// <param name="inputQuery"></param>
        /// <param name="numOfStatement"></param>
        /// <param name="numOfErrors"></param>
        /// <returns></returns>
        public string TranslateQuery(string inputQuery, out int numOfStatement, out int numOfErrors)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);

            numOfStatement = TranslateQueryInt(writer, inputQuery, null);
            writer.Close();

            NotesScanner ns = ScanNotes(null, StatusReporter.OutputQueries);
            numOfErrors = ns.GetMsgCount(Note.STRINGIFIER) + ns.GetMsgCount(Note.ERR_MODIFIER);

            return sb.ToString();
        }

        /// <summary>
        /// Main function used when called command line mode.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="startUI"></param>
        public void Run(string[] args, out bool startUI)
        {
            if (ProcessConfigFile(args, out startUI))
            {
                return;
            }

            StatusReporter.Initialize(Console.Out);
            StreamWriter writer = CreateOutputFile(Config.OutputFile);
            
            String input = ReadInput();
            int numOfStatements = TranslateQueryInt(writer, input, Console.Out);

            if (writer != null)
            {
                writer.Flush();
                writer.Close();
            }

            DbUtil.ReleaseSingleton();
        }

        /// <summary>
        /// Main and only fucntion for conversion 
        /// </summary>
        /// <param name="writer">Output writer for translations, can be null</param>
        /// <param name="input"></param>
        /// /// <param name="infoWriter"></param>
        /// <returns>Returns number of processed input statements</returns>
        private int TranslateQueryInt(TextWriter writer, string input, TextWriter infoWriter)
        {
            Tokenizer tokenizer = new Tokenizer(Config.TokenFile, input);
            input = tokenizer.tokenizeInputStatements();

            int numOfStatements = 1;

            if (tokenizer.OnlyTokenInput() == false)
            {
                IList<Statement> statements = ParseStatements(input);
                DumpStatements(statements);

                Statement translatedStatement;
                TranslateStatements(statements, infoWriter, out translatedStatement);

                if (writer != null)
                {
                    PrintOutput(translatedStatement, writer, tokenizer);
                }
                PrintSummary(infoWriter);
            }
            else
            {
                if (writer != null)
                {
                    writer.Write(input);
                }
                PrintSummary(infoWriter);
            }

            return numOfStatements;
        }

        public void ApplyLocalSettings()
        {
            CultureInfo culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
        }

        public void Close()
        {
            DbUtil.ReleaseSingleton();
        }

        string ReadInput()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Config.InputFile))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (FileNotFoundException e)
            {
                throw new Exception(String.Format(ResStr.MSG_INPUT_FILE_WAS_NOT_FOUND, Config.InputFile), e);
            }
            catch (ArgumentException e)
            {
                throw new Exception(String.Format(ResStr.MSG_INVALID_INPUT_FILE_NAME, Config.InputFile), e);
            }
        }

        IList<Statement> ParseStatements(string text)
        {
            // The parser internally works in Unicode.
            byte[] buffer = Encoding.Unicode.GetBytes(text);
            using (Stream stream = new MemoryStream(buffer))
            {
                ANTLRInputStream input = new ANTLRLowerCaseInputStream(stream, Encoding.Unicode);

                Lexer lexer = new TransactSqlLexer(input);

                CommentedTokenStream tokens = new CommentedTokenStream(lexer);
                SetupCommentHandling(tokens);

                TransactSqlParser parser = new TransactSqlParser(tokens);
                IList<Statement> statements;

                using (StringWriter errorWriter = new StringWriter())
                {
                    parser.TraceDestination = errorWriter;

                    statements = parser.sql();
                    PrintParseErrors(errorWriter.ToString());
                }

                return statements;
            }
        }

        void PrintParseErrors(string errors)
        {
            using (StringReader errorReader = new StringReader(errors))
            {
                while (true)
                {
                    string line = errorReader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    line = line.Trim();
                    if (line != String.Empty)
                    {
                        Console.WriteLine(ResStr.MSG_STATEMENT_NOT_SUPPORTED, line);
                    }
                }
            }
        }

        void DumpStatements(IList<Statement> statements)
        {
            string dumpFile = Config.DumpFile;
            if (dumpFile != "")
            {
                using (StreamWriter writer = CreateOutputFile(dumpFile))
                {
                    foreach (Statement statement in statements)
                    {
                        writer.Write(TreePrinter.Print("statement", statement));
                    }
                }
            }
        }

        /// <summary>
        /// Wraps input statements into block statement. Create create procedure statement if required by config.
        /// </summary>
        /// <param name="inputStatements">List of input statements</param>
        /// <param name="mod">Modifier</param>
        /// <returns></returns>
        private BlockStatement WrapInputStatements(IList<Statement> inputStatements, Modifier mod)
        {
            BlockStatement retStatement;

            // inputStatements[0] is always StartSQLStatement
            if (Config.CreateProcedure && !(inputStatements[1] is CreateProcedureStatement))
            {
                CreateProcedureStatement procedure = new CreateProcedureStatement(new DbObject(new Identifier(IdentifierType.Plain, mod.ProcPool.GetNewProcedureName())), -1,
                        new List<ProcedureParameter> { }, null, false, inputStatements as List<Statement>);
                procedure.Declarations = new BlockStatement();
                retStatement = new BlockStatement(procedure);
            }
            else
            {
                retStatement = new BlockStatement(inputStatements);
            }

            return retStatement;
        }

        /// <summary>
        /// Translate input statements. Returns output statement.
        /// </summary>
        /// <param name="statements"></param>
        /// <param name="infoWriter"></param>
        /// <param name="statementsCount"></param>
        /// <param name="translatedStatement"></param>
        void TranslateStatements(IList<Statement> statements, TextWriter infoWriter, out Statement translatedStatement)
        {            
            Modifier md = new Modifier();
            BlockStatement RootStatement = WrapInputStatements(statements, md);

            StatusReporter.SetStage(ResStr.MSG_SCAN_INPUT_STATEMENTS, string.Empty);
            StatusReporter.SetInputQueries(RootStatement);
            StatusReporter.Message("       " + ResStr.MSG_INPUT_STATEMENTS_FOUND + StatusReporter.InputQueriesCount);

            StatusReporter.SetStage(ResStr.MSG_CONVERSION_STAGE, ResStr.MSG_CONVERSION_STEP);
            
            md.Scan(RootStatement);
            Statement translated = md.Statement;

            StatusReporter.SetOutputQueries(translated, StatusReporter.GetCount());

            CaseFixStatements(translated, infoWriter);
            translatedStatement = translated;

            StatusReporter.Message("\n\n" + ResStr.MSG_DIFFERENT_STATEMENTS_COUNT);
            StatusReporter.Finish();
        }

        /// <summary>
        /// Call case fixing for given statements
        /// </summary>
        /// <param name="translatedStatement">Statements to be translated</param>
        /// <param name="infoWriter">Stream for info/summary, may be null.</param>
        private void CaseFixStatements(Statement translatedStatement, TextWriter infoWriter)
        {
            if (translatedStatement == null)
            {
                return;
            }

            CharacterCaseFixer fixer = new CharacterCaseFixer(Config.DBServer, Config.DBSchema, Config.DBUser, Config.DBPasswd);
            bool displayWarn = true;

            if (Config.UseCaseFixer)
            {
                StatusReporter.SetStage(ResStr.MSG_CASEFIXING_STEP);
            }

            if (translatedStatement is BlockStatement)
            {
                foreach (Statement st in ((BlockStatement)translatedStatement).Statements)
                {
                    displayWarn = CaseFixStatement(fixer, st, infoWriter, displayWarn);
                }
            }
            else
            {
                displayWarn = CaseFixStatement(fixer, translatedStatement, infoWriter, true);
            }
        }

        /// <summary>
        /// Apply case fixcing for given statement. Return new value for display warning
        /// </summary>
        /// <param name="fixer">Initialized case fixed object</param>
        /// <param name="statement">Statement for case fixing</param>
        /// <param name="infoWriter">Writter for info/summary stream</param>
        /// <param name="displayWarning">Display info warning or not</param>
        /// <returns>Returns false when display warning have been displayed</returns>
        bool CaseFixStatement(CharacterCaseFixer fixer, Statement statement, TextWriter infoWriter, bool displayWarning)
        {
            fixer.ClearIdentifiersTables();
            fixer.Scan(statement);

            if (Config.UseCaseFixer && (statement is SqlStartStatement) == false)
            {
                string res = fixer.CorrectIdentifiers();
                if (!string.IsNullOrEmpty(res) && displayWarning)
                {
                    if (infoWriter != null)
                    {
                        infoWriter.WriteLine(ResStr.MSG_UNABLE_TO_VERIFY_IDENTFIERS);
                        infoWriter.WriteLine(ResStr.MSG_TECHNICAL_INFO + " " + res);
                    }
                    displayWarning = false;
                }
            }

            return displayWarning;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="translated"></param>
        /// <param name="writer"></param>
        /// <param name="varTokenizer"></param>
        void PrintOutput(Statement translated, TextWriter writer, Tokenizer tokenizer)
        {
            if (translated == null)
            {
                return;
            }
           
            String output = string.Empty;
            if (Config.Formatter)
            {
                Formatter formatter = new Formatter();
                formatter.Add(translated);
                output = formatter.Statement;
            }
            else
            {
                Stringifier stringifier = new Stringifier();
                stringifier.Add(translated);
                output = stringifier.Statement;
            }

            writer.Write(tokenizer.detokenizeInputStatements(output));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        void PrintSummary(TextWriter writer)
        {
            if (writer == null)
            {
                return;
            }            

            NotesScanner ns = ScanNotes(writer, StatusReporter.OutputQueries);

            Dictionary<string, string> msgs = new Dictionary<string, string>();

            msgs[Note.CASEFIXER] = ResStr.MSG_CORRECTED_IDENTIFIERS;
            msgs[Note.ERR_CASEFIXER] = ResStr.MSG_COLUMNS_NOT_FOUND;
            msgs[Note.STRINGIFIER] = ResStr.MSG_SUMINFO_UNSUPPORTED_FEATURES;
            msgs[Note.ERR_MODIFIER] = ResStr.MSG_ERRORS_LIMITATIONS;

            writer.WriteLine("\n------------------------------------");
            ns.DisplaySummaryInfo(writer, msgs);
            //writer.WriteLine(ResStr.MSG_NUM_OF_OK_QUERIES, numQueries - nokQueries);
            //writer.WriteLine(ResStr.MSG_NUM_OF_NOK_QUERIES, nokQueries);
            writer.WriteLine(ResStr.MSG_NUM_OF_QUERIES, StatusReporter.InputQueriesCount);
        }

        NotesScanner ScanNotes(TextWriter writer, Statement statement)
        {
            NotesScanner ns = new NotesScanner(writer);

            ns.SetFilter(Config.CommentsFilter);
            ns.WriteNotesToWritter(false);
            ns.ClearSummaryInfo();

            ns.Scan(StatusReporter.OutputQueries);
            ns.SetFilter(Config.InfoFilter);

            return ns;
        }

        StreamWriter CreateOutputFile(string path)
        {
             if (path == String.Empty)
             {
                 // We want create dummy writer do special case, when 
                 return null;
             }

            string dirName = Path.GetDirectoryName(path);
            if (dirName.Length > 0)
            {
                Directory.CreateDirectory(dirName);
            }

            try
            {
                return new StreamWriter(path, false, Encoding.UTF8);
            }
            catch(UnauthorizedAccessException)
            {
                Console.WriteLine(ResStr.MSG_OUTPUT_FILE_NOT_SAVED);
            }
            return null;
        }

        class ANTLRLowerCaseInputStream : ANTLRInputStream
        {
            public ANTLRLowerCaseInputStream(Stream input, Encoding encoding)
                : base(input, encoding)
            {
            }

            public override int LA(int i)
            {
                int la = base.LA(i);
                return la <= 0 ? la : Convert.ToInt32(Char.ToLowerInvariant(Convert.ToChar(la)));
            }
        }

        // This is called by parser when consuming a Default channel token that has some preceding
        // Comments channel tokens. We will append the tokens to the most recently created GrammarNode.
        // Unfortunately, token may be consumed multiple times (because of predicates) and only the
        // last one is the "real" consumption.
        // So we have to remember where we have put each comment, and if we see the same comment again,
        // remove it from the previous node and put it to current one.
        Dictionary<int, GrammarNode> _commentedNodes = new Dictionary<int, GrammarNode>();

        void SetupCommentHandling(CommentedTokenStream tokens)
        {
            _commentedNodes.Clear();
            tokens.PrecedingComments += HandlePrecedingComments;
        }

        void HandlePrecedingComments(object o, CommentedTokenStream.PrecedingCommentsEventArgs e)
        {
            bool lastWasNewLine = false;
            foreach (IToken comment in e.Comments)
            {
                if (IsNewLineComment(comment))
                {
                    // New line comments are not added to the tree, they are used just to mark
                    // that a comment starts on a new line.
                    lastWasNewLine = true;
                }
                else
                {
                    GrammarNode commentedNode;
                    if (_commentedNodes.TryGetValue(comment.TokenIndex, out commentedNode))
                    {
                        // The comment was already used, re-attach it.
                        commentedNode.RemoveComment(comment);
                    }

                    _commentedNodes[comment.TokenIndex] = GrammarNode.LastGrammarNode;
                    GrammarNode.LastGrammarNode.AppendComment(lastWasNewLine, comment);

                    lastWasNewLine = false;
                }
            }
        }

        bool IsNewLineComment(IToken token)
        {
            return token.Text == "\n" || token.Text == "\r";
        }
    }

}
