using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    public class UseStatement : Statement
    {
        public Identifier Database { get; set; }

        public UseStatement(Identifier database)
        {
            Database = database;
        }
    }

    public class PrintStatement : Statement
    {
        public Expression Expression { get; set; }

        public PrintStatement(Expression expression)
        {
            Expression = expression;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("SELECT");
            asm.AddSpace();
            asm.Add(Expression);
            asm.AddSpace();
            asm.AddToken("FROM DUMMY");
            asm.End(this);
        }
    }

    public class GoStatement : Statement
    {
        public int Count { get; set; }

        public GoStatement(int count)
        {
            Count = count;
        }

        public override void Assembly(Assembler asm)
        {
            AddNote(Note.STRINGIFIER, ResStr.NO_GO_STATEMENT);
        }
    }
}
