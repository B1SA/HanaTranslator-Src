using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    #region HAHAConcatExpression
    public class HANAConcatExpression : Expression
    {
        public Expression LeftExpression { get; set; }
        public Expression RightExpression { get; set; }
        
        public HANAConcatExpression(Expression left, Expression right)
        {
            LeftExpression = left;
            RightExpression = right;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.SetFlag(Assembler.FLAG_HANA_CONCAT);
            asm.Add(LeftExpression);
            asm.AddSpace();
            asm.Add("||");
            asm.AddSpace();
            asm.Add(RightExpression);
            asm.ClearFlag(Assembler.FLAG_HANA_CONCAT);
            asm.End(this);
        }
    }
    #endregion

    #region HANASetSchemaStatement
    public class HANASetSchemaStatement : Statement
    {
        public Identifier Database { get; set; }

        public HANASetSchemaStatement(Identifier database)
        {
            Database = database;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("SET SCHEMA ");
            asm.Add(Database);
            asm.End(this);
        }
    }
    #endregion

    #region HANANotSupported
    public class HANANotSupportedExpression : Expression
    {
        public HANANotSupportedExpression()
        {
            Hide = true;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddSpace();
            asm.Add("### UNSUPPORTED ###");
            asm.AddSpace();
            asm.End(this);
        }
    }

    public class HANANotSupportedWithCommonTable : WithCommonTable
    {
        public HANANotSupportedWithCommonTable() : base (null, null, null)
        {
            Hide = true;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddSpace();
            asm.Add("### UNSUPPORTED WITH CLAUSE ###");
            asm.AddSpace();
            asm.End(this);
        }
    }

    public class HANANotSupportedValuesClause : ValuesClause
    {
        public HANANotSupportedValuesClause()
        {
            Hide = true;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddSpace();
            asm.Add("### UNSUPPORTED VALUES CLAUSE ###");
            asm.AddSpace();
            asm.End(this);
        }
    }

    public class HANANotSupportedAlterTableAction : AlterTableAction
    {
        public HANANotSupportedAlterTableAction()
        {
            Hide = true;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddSpace();
            asm.Add("### UNSUPPORTED ALTER TABLE ###");
            asm.AddSpace();
            asm.End(this);
        }
    }

    public class HANANotSupportedExecStatementSP : ExecOption
    {
        public HANANotSupportedExecStatementSP()
        {
            Hide = true;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddSpace();
            asm.Add("### UNSUPPORTED EXEC SP ###");
            asm.AddSpace();
            asm.End(this);
        }
    }

    public class HANANotSupportedStatement : Statement
    {
        public HANANotSupportedStatement()
        {
            Hide = true;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddSpace();
            asm.Add("### UNSUPPORTED STATEMENT ###");
            asm.AddSpace();
            asm.End(this);
        }
    }

    public class HANANotSupportedAlterProcedureStatement : Statement
    {
        public HANANotSupportedAlterProcedureStatement()
        {
            Hide = true;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddSpace();
            asm.Add("### UNSUPPORTED ALTER PROCEDURE ###");
            asm.AddSpace();
            asm.End(this);
        }
    }

    public class HANANotSupportedOutputClause : OutputClause
    {
        public HANANotSupportedOutputClause()
            : base(null, null)
        {
            Hide = true;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddSpace();
            asm.Add("### UNSUPPORTED OUTPUT STATEMENT ###");
            asm.AddSpace();
            asm.End(this);
        }
    }

    public class HANANotSupportSimpleStatementinTriggers : Statement
    {
        public HANANotSupportSimpleStatementinTriggers()
        {
            Hide = true;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddSpace();
            asm.Add("### UNSUPPORTED STATEMENT ###");
            asm.AddSpace();
            asm.End(this);
        }
    }

    #endregion
}
