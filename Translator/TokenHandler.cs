using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    class TokenHandler
    {
        static public string TokenName()
        {
            return "Undefined";
        }

        protected TokenHandler()
        {
            // empty
        }

        public virtual string GetTokenString(int tokenID)
        {
            // No token string for abstract Token handler
            return string.Empty;
        }

        public virtual string TranslateToken(string tokenString)
        {
            // No conversion
            return tokenString;
        }
    }
}
