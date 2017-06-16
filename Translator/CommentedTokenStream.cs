using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using Antlr.Runtime;

namespace Translator
{
    class CommentedTokenStream : CommonTokenStream
    {
        List<IToken> _commentTokens = new List<IToken>();

        public static class TokenChannels
        {
            public const int Default = Antlr.Runtime.TokenChannels.Default;
            public const int Comment = 64;
            public const int Hidden = Antlr.Runtime.TokenChannels.Hidden;
        }

        public CommentedTokenStream(ITokenSource tokenSource)
            : this(tokenSource, TokenChannels.Default)
        {
        }

        public CommentedTokenStream(ITokenSource tokenSource, int defaultChannel)
            : base(tokenSource, defaultChannel)
        {
        }

        // When consuming a Default channel token, process all preceding Comment channel tokens (up to the previous Default channel token).
        public override void Consume()
        {
            if (_p == -1)
            {
                Setup();
            }

            if (_tokens[_p].Channel == Channel)
            {
                RaisePrecedingCommentsEvent();
            }

            _p++;
            _p = SkipOffTokenChannels(_p);
        }

        void RaisePrecedingCommentsEvent()
        {
            if (PrecedingComments != null)
            {
                int commentStart = _p;

                // Find start of the block of comments preceding current token.
                while (commentStart > 0 && _tokens[commentStart - 1].Channel == TokenChannels.Comment)
                {
                    commentStart--;
                }

                // Avoid going through the algorithm if there are no comments.
                if (commentStart == _p)
                {
                    return;
                }

                List<IToken> comments = new List<IToken>();
                for (int commentIndex = commentStart; commentIndex < _p; commentIndex++)
                {
                    comments.Add(_tokens[commentIndex]);
                }

                PrecedingComments(this, new PrecedingCommentsEventArgs(_tokens[_p], comments));
            }
        }

        public class PrecedingCommentsEventArgs : EventArgs
        {
            public PrecedingCommentsEventArgs(IToken token, List<IToken> comments)
            {
                Token = token;
                Comments = comments;
            }

            public IToken Token { get; private set; }
            public List<IToken> Comments { get; private set; }
        }

        public event EventHandler<PrecedingCommentsEventArgs> PrecedingComments;
    }
}
