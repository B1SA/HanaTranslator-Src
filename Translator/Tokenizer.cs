using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Translator
{
    using TokenMatch = Tuple<string, string, TokenHandler>;

    class Tokenizer
    {
        protected bool onlyTokenInput = false;                      //true when input statement is only token

        private String tokenFile;
        private String inputQuery;
        private Dictionary<string, TokenHandler> tokenPatterns = new Dictionary<string, TokenHandler>();

        protected List<TokenMatch> tokenValueMatch = new List<TokenMatch>();

        private VariableTokenizer varTokenHandler = new VariableTokenizer();
        private IdentifierTokenizer idTokenHandler = new IdentifierTokenizer();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenFile"></param>
        /// <param name="inputQuery"></param>
        public Tokenizer(string tokenFile, string inputQuery)
        {
            this.tokenFile = tokenFile;
            this.inputQuery = inputQuery;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool OnlyTokenInput()
        {
            return onlyTokenInput;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenName"></param>
        /// <returns></returns>
        private TokenHandler GetTokenHandlerByName(string tokenName)
        {
            if (string.Equals(tokenName, VariableTokenizer.TokenName(), StringComparison.OrdinalIgnoreCase)) {
                return varTokenHandler;
            }

            if (string.Equals(tokenName, IdentifierTokenizer.TokenName(), StringComparison.OrdinalIgnoreCase)) {
                return idTokenHandler;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configFilePath"></param>
        private void LoadTokenFile(string configFilePath)
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
                        Console.WriteLine(String.Format(ResStr.MSG_INVALID_TOKEN_OPTIONS, line));
                        continue;
                    }

                    // Assign corresponding token handler to the key
                    TokenHandler th = GetTokenHandlerByName(tokens[0]);
                    if (th != null) {
                        tokenPatterns.Add(tokens[1], th);
                    }
                    else
                    {
                        Console.WriteLine(String.Format(ResStr.MSG_INVALID_TOKEN_OPTIONS, line));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string[] GetConfigFileLines(string filename)
        {
            if (filename == String.Empty)
                return new string[0];
            try
            {
                return File.ReadAllLines(filename);
            }
            catch
            {
                Console.WriteLine(ResStr.MSG_NO_TOKEN_FILE);
                return new string[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String tokenizeInputStatements()
        {
            LoadTokenFile(tokenFile);

            //tokenize
            if (tokenPatterns.Count == 0)
            {
                return this.inputQuery;
            }

            String outQuery = this.inputQuery;
            int tokenNum = 1;

            foreach (KeyValuePair<string, TokenHandler> pair in tokenPatterns)
            {
                while (true)
                {
                    Match match = Regex.Match(outQuery, pair.Key);

                    if (match.Success)
                    {
                        if (QueryIsTokenOnly(match.Value, outQuery))
                        {
                            onlyTokenInput = true;
                            return this.inputQuery;
                        }

                        string tokenID = pair.Value.GetTokenString(tokenNum);
                        tokenValueMatch.Add(new Tuple<string, string, TokenHandler>(tokenID, match.Value, pair.Value));

                        //Replace method replaces all occurence of the string match.value in the outputQuery
                        outQuery = outQuery.Replace(match.Value, tokenID);
                        tokenNum++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            

            return outQuery;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="translatedStatement"></param>
        /// <returns></returns>
        public string detokenizeInputStatements(string translatedStatement)
        {
            String outputQuery = translatedStatement;

            foreach (TokenMatch tuple in tokenValueMatch)
            {
                // Translate tokens strings
                string translatedToken = tuple.Item3.TranslateToken(tuple.Item1);

                outputQuery = outputQuery.Replace(translatedToken, tuple.Item2);

                //If tokens were used inside string constants,use original value to revert
                outputQuery = outputQuery.Replace(tuple.Item1, tuple.Item2);
            }

            return outputQuery;
        }

        /*
         * Input query is consider as token only when is equals to:
         * customToken
         * customToken;
         * SELECT customToken
         * SELECT customToken;
         */
        virtual protected bool QueryIsTokenOnly(string pattern, string query)
        {
            string trimmQuery = query.Trim().TrimEnd(';');

            if (pattern.Length == trimmQuery.Length)
            {
                // whole input query is: customToken
                // whole input query is: customToken;
                return true;
            }

            string[] arr = query.Split(' ');

            if (arr.Length == 2 && (arr[0].Trim().ToUpper() == "SELECT"))
            {
                trimmQuery = arr[1].Trim().TrimEnd(';');
                if (trimmQuery.Length == pattern.Length)
                {
                    //whole input query is: SELECT customToken
                    //whole input query is: SELECT customToken;
                    return true;
                }
            }

            return false;
        }
    }
}
