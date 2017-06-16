using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using Antlr.Runtime;

namespace Translator 
{
    public class Program
    {
        public static int Main(string[] args)
        {
            int result = 0;

#if !DEBUG
            try
            {
#endif // !DEBUG
                Program program = new Program();
                program.Run(args);
#if !DEBUG
            }
            catch (Exception e)
            {
                Console.WriteLine(ResStr.MSG_INTERNAL_ERROR, e.Message);

                result = -1;
            }
#endif // !DEBUG

#if DEBUG
            Console.WriteLine();
            Console.WriteLine(ResStr.MSG_PRESS_ANY_KEY_TO_CONTINUE);
            Console.ReadKey();
#endif // DEBUG

            return result;
        }

        public void Run(string[] args)
        {
            Config.Initialize(args);

            if (Config.Version)
            {
                Config.DisplayVersion();
                return;
            }

            if (Config.Help || Config.InputFile == "")
            {
                Config.DisplayHelp();
                return;
            }

            ApplyLocalSettings();

            String input = ReadInput();
            IList<Statement> statements = ParseStatements(input);

            DumpStatements(statements);
            TranslateStatements(statements);
        }

        public void RunTranslation(string[] args, string configFile)
        {
            Config.Initialize(args, configFile);

            ApplyLocalSettings();

            String input = ReadInput();
            IList<Statement> statements = ParseStatements(input);

            DumpStatements(statements);
            TranslateStatements(statements);
        }

        void ApplyLocalSettings()
        {
            CultureInfo culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
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

        void TranslateStatements(IList<Statement> statements)
        {
            string outputFile = Config.OutputFile;
            Dictionary<string, string> msgs = new Dictionary<string, string>();
            int errStatement = 0;

            msgs[Note.CASEFIXER] = ResStr.MSG_CORRECTED_IDENTIFIERS;
            msgs[Note.ERR_CASEFIXER] = ResStr.MSG_COLUMNS_NOT_FOUND;
            msgs[Note.STRINGIFIER] = ResStr.MSG_SUMINFO_UNSUPPORTED_FEATURES;
            msgs[Note.ERR_MODIFIER] = ResStr.MSG_ERRORS_LIMITATIONS;

            if (outputFile != "")
            {
                using (StreamWriter writer = CreateOutputFile(outputFile))
                {
                    CharacterCaseFixer fixer = new CharacterCaseFixer(Config.DBServer, Config.DBSchema, Config.DBUser, Config.DBPasswd);
                    NotesScanner ns = new NotesScanner(writer);
                    
                    ns.SetFilter(Config.CommentsFilter);
                    ns.WriteNotesToWritter(false);

                    bool displayWarning = true;
                    int idx = 1;


                    Console.WriteLine(ResStr.MSG_TRANSLATING_QUERY, idx++, statements.Count);
                    Modifier md = new Modifier();
                    BlockStatement RootStatement;
                    if (Config.CreateProcedure && !(statements[1] is CreateProcedureStatement))
                    {
                        CreateProcedureStatement procedure = new CreateProcedureStatement(new DbObject(new Identifier(IdentifierType.Plain, md.ProcPool.GetNewProcedureName())), -1,
                                new List<ProcedureParameter> { }, null, false, statements as List<Statement>);
                        procedure.Declarations = new BlockStatement();
                        RootStatement = new BlockStatement(procedure);
                    }
                    else
                    {
                        RootStatement = new BlockStatement(statements);
                    }
                    md.Scan(RootStatement);

                    ns.ClearSummaryInfo();
                    ns.SetFilter(Config.CommentsFilter);

                    Statement translated = md.Statement;
                    
                    if (translated != null)
                    {
                        if (translated is BlockStatement)
                        {
                            foreach (Statement st in ((BlockStatement)translated).Statements)
                            {
                                fixer.ClearIdentifiersTables();
                                fixer.Scan(st);

                                if (Config.UseCaseFixer && (translated is SqlStartStatement) == false)
                                {
                                    string res = fixer.CorrectIdentifiers();
                                    if (!string.IsNullOrEmpty(res) && displayWarning)
                                    {
                                        Console.WriteLine(ResStr.MSG_UNABLE_TO_VERIFY_IDENTFIERS);
                                        Console.WriteLine(ResStr.MSG_TECHNICAL_INFO + " " + res);
                                        displayWarning = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            fixer.ClearIdentifiersTables();
                            fixer.Scan(translated);

                            if (Config.UseCaseFixer && (translated is SqlStartStatement) == false)
                            {
                                string res = fixer.CorrectIdentifiers();
                                if (!string.IsNullOrEmpty(res) && displayWarning)
                                {
                                    Console.WriteLine(ResStr.MSG_UNABLE_TO_VERIFY_IDENTFIERS);
                                    Console.WriteLine(ResStr.MSG_TECHNICAL_INFO + " " + res);
                                    displayWarning = false;
                                }
                            }
                        }

                        if (Config.Formatter)
                        {
                            Formatter formatter = new Formatter();
                            formatter.Add(translated);
                            writer.Write(formatter.Statement);
                        }
                        else
                        {
                            Stringifier stringifier = new Stringifier();
                            stringifier.Add(translated);
                            writer.Write(stringifier.Statement);
                        }

                        ns.Scan(translated);
                    }

                    ns.SetFilter(Config.InfoFilter);
                    ns.DisplaySummaryInfo(Console.Out, msgs);

                    if (ns.NotesCount(Note.STRINGIFIER) > 0 || ns.NotesCount(Note.ERR_MODIFIER) > 0 || ns.NotesCount(Note.ERR_CASEFIXER) > 0)
                    {
                        ++errStatement;
                    }
                }

                DbUtil.ReleaseSingleton();

                Console.WriteLine("=================================================================");
                Console.WriteLine(ResStr.MSG_NUM_OF_OK_QUERIES, statements.Count - errStatement);
                Console.WriteLine(ResStr.MSG_NUM_OF_NOK_QUERIES, errStatement);
                Console.WriteLine(ResStr.MSG_NUM_OF_QUERIES, statements.Count);
            }
        }

        StreamWriter CreateOutputFile(string path)
        {
            string dirName = Path.GetDirectoryName(path);
            if (dirName.Length > 0)
            {
                Directory.CreateDirectory(dirName);
            }

            return new StreamWriter(path, false, Encoding.UTF8);
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
