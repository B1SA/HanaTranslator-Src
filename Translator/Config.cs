using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Translator
{
    public static class Config
    {
        public static string DEFAULT_CONFIG = "config.txt";
        public static bool isInitialized = false;
        public static string configFileName = "";
        
        public static void Initialize(string[] args)
        {
            Initialize(args, DEFAULT_CONFIG);
        }

        public static void Initialize(string[] args, string configPath)
        {
            if (configPath != null)
            {
                LoadConfigFile(configPath);
            }
            else
            {
                // needed because these options dosn't get reset to false from args
                // (when false, it is missing in args and ParseArguments will leave it unchanged)
                // (when true, it is in args and ParseArguments will set it to true)
                foreach (Option o in options)
                {
                    if (o.GetType() == typeof(BoolOption))
                    {
                        BoolOption bo = (BoolOption)o;
                        bo.SetValue("false");
                    }
                }
            }
            if (args != null)
            {
                if (args.Length != 0)
                {
                    ParseArguments(args);
                }
            }

            if (UseCaseFixer == true)
            {
                if (DBServer == string.Empty || DBSchema == string.Empty || DBUser == string.Empty || DBPasswd == string.Empty)
                {
                    Console.WriteLine(String.Format(ResStr.MSG_SERVER_PARAMETER_MISSING));
                    if (DBServer == string.Empty)
                    {
                        Console.WriteLine(String.Format(ResStr.MSG_SERVER_DBSERVER_MISSING));
                    }
                    if (DBSchema == string.Empty)
                    {
                        Console.WriteLine(String.Format(ResStr.MSG_SERVER_DBSCHEMA_MISSING));
                    }
                    if (DBUser == string.Empty)
                    {
                        Console.WriteLine(String.Format(ResStr.MSG_SERVER_DBUSER_MISSING));
                    }
                    if (DBPasswd == string.Empty)
                    {
                        Console.WriteLine(String.Format(ResStr.MSG_SERVER_DBPASSWD_MISSING));
                    }
                }
            }
        }

        public static String[] CommentsFilter = new String[] { Note.CASEFIXER, Note.ERR_CASEFIXER, Note.STRINGIFIER, Note.MODIFIER, Note.ERR_MODIFIER };
        public static String[] InfoFilter = new String[] { Note.CASEFIXER, Note.ERR_CASEFIXER, Note.STRINGIFIER, Note.ERR_MODIFIER };

        public static string InputFile { get; private set; }
        public static string OutputFile { get; private set; }
        public static string DumpFile { get; private set; }
        public static string TokenFile { get; private set; }

        public static string DBServer { get; private set; }
        public static string DBSchema { get; private set; }
        public static string DBUser { get; private set; }
        public static string DBPasswd { get; private set; }

        public static bool UseCaseFixer { get; private set; }
        public static bool DisableComments { get; private set; }
        public static bool Help { get; private set; }
        public static bool Formatter { get; private set; }
        public static bool CreateProcedure { get; private set; }
        public static bool Version { get; private set; }
        public static bool UseGUI { get; private set; }

        public static bool CreateCommandLine { get; private set; }

        public static string MSServer { get; private set; }
        public static string MSDatabase { get; private set; }
        public static string MSUser { get; private set; }
        public static string MSPasswd { get; private set; }

        // Add new options here:
        static List<Option> options = new List<Option>
        {
            //               ConfigName   Argument  Setter Delegate         Default Value   Description
            new BoolOption  ("GUI",             "g",   v => UseGUI = v,        false,         ResStr.INF_GUI),
            new StringOption("InputFile",       "i",   v => InputFile = v,                     ResStr.INF_PATH_TO_INPUT),
            new StringOption("OutputFile",      "o",   v => OutputFile = v,    "output.txt",   ResStr.INF_PATH_TO_OUTPUT),
            new StringOption("DumpFile",        "D",   v => DumpFile = v,                      null),
            new StringOption("DBServer",        "s",   v => DBServer = v,                      ResStr.INF_SAP_HANA_SERVER_NAME),
            new StringOption("DBSchema",        "d",   v => DBSchema = v,                      ResStr.INF_SAP_HANA_SCHEMA),
            new StringOption("DBUser",          "u",   v => DBUser = v,                        ResStr.INF_SAP_HANA_USER_NAME),
            new StringOption("DBPasswd",        "p",   v => DBPasswd = v,                      ResStr.INF_SAP_HANA_PWD),
            new BoolOption  ("UseCaseFixer",    "f",   v => UseCaseFixer = v,   false,         ResStr.INF_ENABLE_CASE_FIXER_ALGORITHM),
            new BoolOption  ("DisableComments", "c",   v => DisableComments = v,false,         ResStr.INF_EXCLUDE_TOOLS_COMMENTS_IN_OUTPUT),
            new BoolOption  ("Help",            "h",   v => Help = v,           false,         ResStr.INF_DISPLAY_HELP),
            new BoolOption  ("Formatter",       "F",   v => Formatter = v,      false,         ResStr.INF_ENABLE_FORMATTER),
            new BoolOption  ("CreateProcedure", "P",   v => CreateProcedure = v,false,         ResStr.INF_AUTO_SP_FOR_COMPLEX_QUERIES),
            new BoolOption  ("Version",         "v",   v => Version = v,        false,         ResStr.INF_PRINT_VERSION),
            new BoolOption  ("CreateCommandLine", "C", v => CreateCommandLine = v, false,  ResStr.INF_CREATE_COMMAND_LINE),
            new StringOption("TokenFile",       "t",   v => TokenFile = v,                     ResStr.INF_PATH_TO_TOKEN),
            /*new StringOption("MSServer",        "ms",   v => MSServer = v,                      ResStr.INF_MS_SERVER_NAME),
            new StringOption("MSDatabase",      "md",   v => MSDatabase = v,                    ResStr.INF_MS_DATABASE),
            new StringOption("MSUser",          "mu",   v => MSUser = v,                        ResStr.INF_MS_USER_NAME),
            new StringOption("MSPasswd",        "mp",   v => MSPasswd = v,                      ResStr.INF_MS_PWD)
             * */
        };

        static void LoadConfigFile()
        {
            LoadConfigFile(DEFAULT_CONFIG);
        }

        static void LoadConfigFile( string configFilePath )
        {
            foreach (string line in GetConfigFileLines(configFilePath))
            {
                // Ignore comments and blank lines.
                if (!line.StartsWith("//") && line.Any(c => !Char.IsWhiteSpace(c)))
                {
                    // Split line to key and value.
                    string[] tokens = line.Split('=').Select(t => t.Trim()).ToArray();
                    if (tokens.Length != 2)
                    {
                        throw new Exception(String.Format(ResStr.MSG_INVALID_CONFIG_OPTIONS, line));
                    }

                    // Find option corresponding to the key.
                    Option option = options.FirstOrDefault(o => o.ConfigName == tokens[0]);
                    if (option == null)
                    {
                        throw new Exception(String.Format(ResStr.MSG_INVALID_CONFIG_OPTIONS, line));
                    }

                    option.SetValue(tokens[1]);
                }
            }
        }

        public static string LoadConfigFileWithComments(string configFilePath)
        {
            string comments = "";
            foreach (string line in GetConfigFileLines(configFilePath))
            {
                // Ignore comments and blank lines.
                if (!line.StartsWith("//") && line.Any(c => !Char.IsWhiteSpace(c)))
                {
                    // Split line to key and value.
                    string[] tokens = line.Split('=').Select(t => t.Trim()).ToArray();
                    if (tokens.Length != 2)
                    {
                        throw new Exception(String.Format(ResStr.MSG_INVALID_CONFIG_OPTIONS, line));
                    }

                    comments += tokens[0] + System.Environment.NewLine;
                }
                else if (line.StartsWith("//"))
                {
                    comments += line + System.Environment.NewLine;
                }
                else
                {
                    comments += System.Environment.NewLine;
                }
            }
            return comments;
        }

        static string[] GetConfigFileLines(string filename)
        {
            try
            {
                isInitialized = true;
                configFileName = filename;
                return File.ReadAllLines(filename);
            }
            catch
            {
                Console.WriteLine(ResStr.MSG_NO_CONFIG_FILE);
                isInitialized = false;
                return new string[0];
            }
        }

        static void ParseArguments(string[] args)
        {
            if (args == null) return;
            
            try
            {
                int argIndex = 0;
                while (argIndex < args.Length)
                {
                    if (args[argIndex].Length == 0)
                        break;

                    // Find option corresponding to the argument.
                    Option option = options.FirstOrDefault(o => o.ArgumentName == args[argIndex]);
                    if (option == null)
                    {
                        throw new Exception(String.Format(ResStr.MSG_INVALID_PARAMETER, args[argIndex]));
                    }

                    option.ParseArguments(args, ref argIndex);
                    argIndex++;
                }
            }
            catch
            {
                // If anything bad happens while parsing parameters, display usage information.
                DisplayHelp();
                throw;
            }

        }

        public static string GenerateCommandLine()
        {
            StringBuilder sResult = new StringBuilder("Converter.exe");
            
            // return command line string from options
            foreach (string line in GetConfigFileLines(DEFAULT_CONFIG))
            {
                // Ignore comments and blank lines.
                if (!line.StartsWith("//") && line.Any(c => !Char.IsWhiteSpace(c)))
                {
                    // Split line to key and value.
                    string[] tokens = line.Split('=').Select(t => t.Trim()).ToArray();
                    if (tokens.Length != 2)
                    {
                        throw new Exception(String.Format(ResStr.MSG_INVALID_CONFIG_OPTIONS, line));
                    }

                    // Find option corresponding to the key.
                    Option option = options.FirstOrDefault(o => o.ConfigName == tokens[0]);
                    if (option == null)
                    {
                        throw new Exception(String.Format(ResStr.MSG_INVALID_CONFIG_OPTIONS, line));
                    }

                    if (option.GetType() == typeof(StringOption) || 
                        (option.GetType() == typeof(BoolOption) && tokens[1].Equals("true")))
                    {
                        sResult.Append(' ');
                        sResult.Append(option.ArgumentName);
                    }

                    if (option.GetType() != typeof(BoolOption))
                    //if (!tokens[1].Equals("false") && !tokens[1].Equals("true"))
                    {
                        sResult.Append(' ');
                        sResult.Append(tokens[1]);
                    }
                }
            }

            return sResult.ToString();
        }

        public static void DisplayHelp()
        {
            DisplayVersion();
            Console.WriteLine(ResStr.INF_TRANSLATE_TSQL_INTO_HANA);
            Console.WriteLine();
            Console.WriteLine(ResStr.INF_USAGE);
            Console.WriteLine(ResStr.INF_TRANSLATOR_OPTIONS);
            Console.WriteLine();
            Console.WriteLine(ResStr.INF_SUPPORTED_OPTIONS);

            foreach (Option option in options)
            {
                if (option.Description != null)
                {
                    Console.WriteLine("    {0}  {1} ({2}).", option.ArgumentName, option.Description, option.ConfigName);
                }
            }

            Console.WriteLine();
            Console.WriteLine(ResStr.INF_EXAMPLES);
            Console.WriteLine("    " + ResStr.INF_EXAMPLE_1ST_LINE);
            Console.WriteLine("    " + ResStr.INF_EXAMPLE_2ND_LINE);
        }

        public static void DisplayVersion()
        {
            String name = (Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute).Title;
            String Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine(ResStr.ABOUT, name, Version);
        }


        abstract class Option
        {
            public string ConfigName { get; private set; }
            public string ArgumentName { get; private set; }
            public string Description { get; private set; }

            public Option(string configName, string argumentChar, string description)
            {
                ConfigName = configName;
                ArgumentName = String.Format("-{0}", argumentChar);
                Description = description;
            }

            abstract public void ParseArguments(string[] args, ref int argIndex);
            abstract public void SetValue(string value);
        }

        class StringOption : Option
        {
            Action<string> Setter;

            public StringOption(string configName, string argumentChar, Action<string> setter, string description)
                : this(configName, argumentChar, setter, "", description)
            {
            }

            public StringOption(string configName, string argumentChar, Action<string> setter, string defaultValue, string description)
                : base(configName, argumentChar, description)
            {
                Setter = setter;
                Setter(defaultValue);
            }

            override public void ParseArguments(string[] args, ref int argIndex)
            {
                // Move to next argument (which should be parameter value).
                argIndex++;

                if (argIndex >= args.Length)
                {
                    throw new Exception(String.Format(ResStr.MSG_NO_VALUE_PROVIDED_FOR_PARAM, ArgumentName));
                }

                Setter(args[argIndex]);
            }

            override public void SetValue(string value)
            {
                Setter(value);
            }
        }

        class BoolOption : Option
        {
            Action<bool> Setter;

            public BoolOption(string configName, string argumentChar, Action<bool> setter, string description)
                : this(configName, argumentChar, setter, false, description)
            {
            }

            public BoolOption(string configName, string argumentChar, Action<bool> setter, bool defaultValue, string description)
                : base(configName, argumentChar, description)
            {
                Setter = setter;
                Setter(defaultValue);
            }

            override public void ParseArguments(string[] args, ref int argIndex)
            {
                // Just set the value to true, there are no additional arguments.
                Setter(true);
            }

            override public void SetValue(string value)
            {
                bool boolValue;
                if (!Boolean.TryParse(value, out boolValue))
                {
                    throw new Exception(String.Format(
                        ResStr.MSG_INVALID_VALUE_FOR_BOOLEAN, value, ConfigName));
                }

                Setter(boolValue);
            }
        }
    }
}
