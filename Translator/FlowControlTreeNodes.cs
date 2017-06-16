using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    public class IfStatement : Statement
    {
        public Expression Condition { get; set; }
        public BlockStatement TrueStatement { get; set; }
        public BlockStatement FalseStatement { get; set; }

        public IfStatement(Expression condition, Statement trueStatement, Statement falseStatement)
        {
            Condition = condition;
            TrueStatement = new BlockStatement(trueStatement);
            FalseStatement = new BlockStatement(falseStatement);
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("IF");
            asm.AddSpace();
            asm.Add(Condition);
            asm.AddSpace();
            asm.AddToken("THEN");
            asm.AddSpace();
            asm.IncreaseIndentation();
            asm.NewLine();
            asm.Add(TrueStatement);
            asm.DecreaseIndentation();
            if (FalseStatement.Count() != 0)
            {
                asm.NewLine();
                asm.AddToken("ELSE");
                asm.AddSpace();
                asm.IncreaseIndentation();
                asm.NewLine();
            }

            // This is needed in order to process comments after FalseStatement.
            asm.Add(FalseStatement);

            if (FalseStatement.Count() != 0)
            {
                asm.DecreaseIndentation();
            }
            asm.NewLine();
            asm.AddToken("END IF");
            asm.End(this);
        }
    }



    public class WhileStatement : Statement
    {
        public Expression Condition { get; set; }
        public BlockStatement Statement { get; set; }

        public WhileStatement(Expression condition, Statement statement)
        {
            Condition = condition;
            Statement = new BlockStatement(statement);
        }
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("WHILE");
            asm.AddSpace();
            asm.Add(Condition);
            asm.AddSpace();
            asm.AddToken("DO");
            asm.AddSpace();
            asm.IncreaseIndentation();
            asm.NewLine();
            asm.Add(Statement);
            asm.DecreaseIndentation();
            asm.NewLine();
            asm.AddToken("END WHILE");
            asm.End(this);
        }
    }

    public class BreakStatement : Statement
    {
        public BreakStatement()
        {
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("BREAK");
            asm.End(this);
        }
    }

    public class ContinueStatement : Statement
    {
        public ContinueStatement()
        {
        }
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("CONTINUE");
            asm.End(this);
        }
    }

    public class GotoStatement : Statement
    {
        public Identifier Label { get; set; }

        public GotoStatement(Identifier label)
        {
            Label = label;
        }
    }

    public class LabelStatement : Statement
    {
        public Identifier Label { get; set; }

        public LabelStatement(Identifier label)
        {
            Label = label;
        }
    }

    public class WaitForStatement : Statement
    {
        public DelayOrTime Type { get; set; }
        public Expression Time { get; set; }

        public WaitForStatement(DelayOrTime type, Expression time)
        {
            Type = type;
            Time = time;
        }
    }

    public enum DelayOrTime
    {
        Delay, Time
    }

    public class TryStatement : Statement
    {
        public BlockStatement TryStatements { get; set; }
        public BlockStatement CatchStatements { get; set; }

        public TryStatement(IList<Statement> tryStatements, IList<Statement> catchStatements)
        {
            TryStatements = new BlockStatement(tryStatements);
            CatchStatements = new BlockStatement(catchStatements);
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            AddNote(Note.STRINGIFIER, ResStr.NO_TRY_CATCH_STATEMENT);
            asm.Add(TryStatements);
            asm.Add(CatchStatements);
            asm.End(this);
        }
    }

    public class ThrowStatement : Statement
    {
        public Expression Error { get; set; }
        public Expression Message { get; set; }
        public Expression State { get; set; }

        public ThrowStatement(Expression error, Expression message, Expression state)
        {
            Error = error;
            Message = message;
            State = state;
        }
    }

    public class ReturnStatement : Statement
    {
        public Expression Result { get; set; }

        public ReturnStatement(Expression result)
        {
            Result = result;;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("RETURN");
            if (Result != null)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_RETURN_VALUE);
            }
            asm.End(this);
        }
    }

    public class BlockStatement : Statement
    {
        public IList<Statement> Statements { get; set; }

        public BlockStatement(IList<Statement> statements)
        {
            Statements = statements;
            Terminate = false;
        }

        public BlockStatement (Statement statement)
        {
            if (statement != null)
                Statements = new List<Statement> { statement };
            else
                Statements = new List<Statement> ();
            Terminate = false;
        }

        public BlockStatement()
        {
            Statements = new List<Statement>();
            Terminate = false;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (Statements != null)
            {
                foreach (Statement stmt in Statements)
                {
                    asm.Add(stmt);
                    if (stmt != Statements.Last())
                    {
                        asm.NewLine();
                    }
                }
            }
            asm.End(this);
        }

        public void InsertBefore(Statement refStmt, Statement inStmt)
        {
            int index = Statements.IndexOf(refStmt);
            Statements.Insert(index, inStmt);
        }

        public void AddStatement(Statement inStmt)
        {
            Statements.Add(inStmt);
        }

        public void ReplaceStatement(Statement original, Statement newOne)
        {
            int index = Statements.IndexOf(original);
            if (index != -1)
            {
                if (original.Comments.Count != 0)
                {
                    newOne.MoveCommentsFrom(original);
                }
                Statements[index] = newOne;
            }
        }

        public void RemoveStatement(Statement stmt)
        {
            int index = Statements.IndexOf(stmt);

            if (index > -1)
            {
                if (stmt.Comments.Count != 0)
                {
                    Statements[index - 1].MoveCommentsFrom(stmt);
                }
                Statements.RemoveAt(index);
            }
        }

        public int Count()
        {
            return Statements.Count;
        }

        public IList<Statement> GetStatements()
        {
            return Statements;
        }
    }
}
