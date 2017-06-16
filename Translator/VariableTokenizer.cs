using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace Translator
{
    class VariableTokenizer : TokenHandler
    {
        static new public string TokenName()
        {
            return "VarToken";
        }

        public VariableTokenizer(): base()
        {
            // empty
        }

        public override string GetTokenString(int tokenID)
        {
            // No token string for abstract Token handler
            return "@Token" + tokenID.ToString("D5");
        }

        public override string TranslateToken(string tokenString)
        {
            // TSQL variable style was transformed to SQL Script variable style
            return tokenString.Replace('@', ':');
        }
    }
}
