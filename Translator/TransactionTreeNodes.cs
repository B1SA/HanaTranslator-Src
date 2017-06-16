using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    public class BeginTransactionStatement : Statement
    {
        public TransactionName Name { get; set; }
        public bool WithMark { get; set; }
        public StringLiteral MarkString { get; set; }

        public BeginTransactionStatement(TransactionName name, bool withMark, StringLiteral markString)
        {
            Name = name;
            WithMark = withMark;
            MarkString = markString;
        }
    }

    public class CommitTransactionStatement : Statement
    {
        public TransactionName Name { get; set; }

        public CommitTransactionStatement(TransactionName name)
        {
            Name = name;
        }
    }

    public class RollbackTransactionStatement : Statement
    {
        public TransactionName Name { get; set; }

        public RollbackTransactionStatement(TransactionName name)
        {
            Name = name;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (Name != null)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_NAMED_TRANSACTIONS);
            }
            asm.AddToken("ROLLBACK");
            asm.End(this);
        }
    }

    public class SaveTransactionStatement : Statement
    {
        public TransactionName Name { get; set; }

        public SaveTransactionStatement(TransactionName name)
        {
            Name = name;
        }
    }

    abstract public class TransactionName : GrammarNode
    {
    }

    public class IdentifierTransactionName : TransactionName
    {
        public Identifier Value { get; set; }

        public IdentifierTransactionName(Identifier value)
        {
            Value = value;
        }
    }

    public class VariableTransactionName : TransactionName
    {
        public VariableExpression Value { get; set; }

        public VariableTransactionName(VariableExpression value)
        {
            Value = value;
        }
    }
}
