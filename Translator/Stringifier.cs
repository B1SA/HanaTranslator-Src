using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    public class Stringifier : Assembler
    {
        private static Stringifier _Stringifier = null;
        public string Statement;

        public static object GetProcessed(GrammarNode node)
        {
            if (_Stringifier == null)
            {
                _Stringifier = new Stringifier();
            }

            _Stringifier.Statement = "";
            node.Assembly(_Stringifier);
            return _Stringifier.Statement;
        }

        override public void Clear()
        {
            Statement = string.Empty;
        }

        public Stringifier()
        {
            Statement = string.Empty;
        }

        //space
        override public void AddSpace()
        {
            Statement += " ";
        }

        // basic types
        override public void Add(string right)
        {
            Statement += right;
        }
        override public void Add(decimal right)
        {
            Statement += right;
        }
        override public void Add(int right)
        {
            Statement += right;
        }
        override public void AddToken(string right)
        {
            Statement += right;
        }

        override public void Add(Statement stmt)
        {
            stmt.Assembly(this);
            if (stmt.Terminate)
            {
                if (!stmt.Hide)
                    Statement += ";" + Environment.NewLine;
                Statement += stmt.ReturnNotes();
            }
            HandleComments(stmt);
        }

        override public void Add(GrammarNode node)
        {
            node.Assembly(this);
        }

        override public void HandleComments(GrammarNode node)
        {
            foreach (Comment comment in node.Comments)
            {
                bool isMultiline = false;
                if (comment.NewLine && !Statement.EndsWith(Environment.NewLine))
                {
                    Statement += Environment.NewLine;
                }
                if (comment.Type == CommentType.SingleLine)
                {
                    Statement += "--";
                }
                else
                {
                    Statement += "/*";
                    isMultiline = true;
                }
                Add(comment.Text);
                if (isMultiline)
                {
                    Statement += "*/" + Environment.NewLine;
                }
                else
                {
                    Statement += Environment.NewLine;
                }
            }
        }
    }
}
