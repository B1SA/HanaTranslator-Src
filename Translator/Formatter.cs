using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    public class Formatter : Assembler
    {
        const int MAX_LINE_LENGTH = 120;
        const string INDENTATION = "    ";

        string _Statement;
        public string Statement
        {
            set
            {
                return;
            }
            get
            {
                return _Statement;
            }
        }
        int IndentationLevel;
        int LastBreakable;

        void Append(string toAppend)
        {
            if (_Statement.Length - _Statement.LastIndexOf(Environment.NewLine) + toAppend.Length > MAX_LINE_LENGTH)
            {
                if (LastBreakable > 0)
                {
                    string tmp = Environment.NewLine;
                    for (int i = 0; i < IndentationLevel; i++)
                    {
                        tmp += INDENTATION;
                    }
                    _Statement = _Statement.Insert(LastBreakable, tmp);
                    LastBreakable = 0;
                }
                else
                {
                    NewLine();
                }
            }
            _Statement += toAppend;
        }

        string GetIndentationString()
        {
            string ret = string.Empty;
            for (int i = 0; i < IndentationLevel; i++)
            {
                ret += INDENTATION;
            }
            return ret;
        }

        override public void NewLine()
        {
            if (_Statement.Length != 0)
            {
                _Statement += Environment.NewLine;
                _Statement += GetIndentationString();
            }
        }

        override public void IncreaseIndentation()
        {
            IndentationLevel++;
        }
        override public void DecreaseIndentation()
        {
            IndentationLevel--;
        }

        override public void Breakable()
        {
            LastBreakable = _Statement.Length;
        }


        override public void Clear()
        {
            _Statement = string.Empty;
        }

        public Formatter()
        {
            _Statement = string.Empty;
            IndentationLevel = 0;
            LastBreakable = 0;
        }

        //space
        override public void AddSpace()
        {
            Append(" ");
        }
        override public void AddToken(string right)
        {
            Append(right);
        }

        // basic types
        override public void Add(string right)
        {
            Append(right);
        }
        override public void Add(decimal right)
        {
            Append(right.ToString());
        }
        override public void Add(int right)
        {
            Append(right.ToString());
        }

        void AddNotes(string notes)
        {
            if (notes.Length != 0)
            {
                notes = Environment.NewLine + notes;
                if (notes.EndsWith(Environment.NewLine))
                {
                    notes  = notes.TrimEnd();
                }
                string newStr = Environment.NewLine + GetIndentationString();
                notes = notes.Replace(Environment.NewLine, newStr);
                _Statement += notes;
            }
        }

        override public void Add(Statement stmt)
        {
            string notes = string.Empty;
            stmt.Assembly(this);
            if (stmt.Terminate)
            {
                if (!stmt.Hide)
                    AddToken(";");
                notes = stmt.ReturnNotes();
                AddNotes(notes);
            }

            if (notes.Length > 0 && stmt.Comments.Count > 0)
            {
                if (!stmt.Comments[0].NewLine)
                {
                    NewLine();
                }
            }

            HandleComments(stmt);
        }

        override public void Add(GrammarNode node)
        {
            node.Assembly(this);
            HandleComments(node);
        }

        override public void HandleComments(GrammarNode node)
        {
            foreach (Comment comment in node.Comments)
            {
                bool isMultiline = false;
                if (comment.NewLine)
                {
                    NewLine();
                }
                if (comment.Type == CommentType.SingleLine)
                {
                    Append("--");
                }
                else
                {
                    Append("/*");
                    isMultiline = true;
                }
                Append(comment.Text);
                if (isMultiline)
                {
                    Append("*/");
                }
            }
        }
    }
}
