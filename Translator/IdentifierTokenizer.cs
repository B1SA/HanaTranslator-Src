using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    class IdentifierTokenizer : TokenHandler
    {
        static new public string TokenName()
        {
            return "IdToken";
        }

        public IdentifierTokenizer()
            : base()
        {
            // empty
        }

        public override string GetTokenString(int tokenID)
        {
            // No token string for abstract Token handler
            return "\"Token" + tokenID.ToString("D5") + "\"";
        }
    }
}
