using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    public class DeclareStatement : Statement
    {
        public IList<VariableDeclaration> Declarations { get; set; }

        public DeclareStatement(IList<VariableDeclaration> declarations)
        {
            Declarations = declarations;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Declarations[0]);
            asm.End(this);
        }
    }

    abstract public class VariableDeclaration : GrammarNode
    {
    }

    public class ScalarVariableDeclaration : VariableDeclaration
    {
        public VariableExpression Variable { get; set; }
        public DataType Type { get; set; }
        public Expression Value { get; set; }

        public ScalarVariableDeclaration(VariableExpression variable, DataType type, Expression value)
        {
            Variable = variable;
            Type = type;
            Value = value;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Variable);
            asm.AddSpace();
            asm.Add(Type);
            if (Value != null)
            {
                asm.AddSpace();
                asm.AddToken(":=");
                asm.AddSpace();
                asm.Add(Value);
            }
            asm.End(this);
        }
    }

    public class CursorForUpdate : GrammarNode
    {
        public List<Identifier> Columns { get; set; }

        public CursorForUpdate(List<Identifier> columns)
        {
            Columns = columns;
        }
    }

    public class CursorVariableDeclaration : VariableDeclaration
    {
        public CursorSource Name { get; set; }
        public SelectStatement Statement { get; set; }
        public List<CursorProperty> Properties { get; set; }
        public CursorForUpdate ForUpdateClause { get; set; }

        public CursorVariableDeclaration(CursorSource name, List<CursorProperty> properties, SelectStatement statement, CursorForUpdate forUpdate)
        {
            Name = name;
            Statement = statement;
            Properties = properties;
            ForUpdateClause = forUpdate;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("CURSOR");
            asm.AddSpace();
            asm.Add(Name);
            asm.AddSpace();
            asm.AddToken("FOR");
            asm.AddSpace();
            asm.Add(Statement);
            asm.End(this);
        }
    }

    public class OpenCursorStatement : Statement
    {
        CursorSource Source { get; set; }

        public OpenCursorStatement(CursorSource source)
        {
            Source = source;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("OPEN");
            asm.AddSpace();
            asm.Add(Source);
            asm.End(this);
        }
    }

    public class CursorSource : GrammarNode
    {
        public Identifier Name { get; set; }
        public bool Global { get; set; }
        public VariableExpression VarName { get; set; }

        public CursorSource(bool global, Identifier name)
        {
            Name = name;
            Global = global;
        }

        public CursorSource(VariableExpression varName)
        {
            VarName = varName;
            Global = false;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (Name != null)
            {
                asm.Add(Name);
            }
            else if (VarName != null)
            {
                asm.Add(VarName);
            }
            asm.End(this);
        }
    }

    public class CursorProperty : GrammarNode
    {
        CursorPropertyType Type { get; set; }

        public CursorProperty(CursorPropertyType type)
        {
            Type = type;
        }
    }

    public enum CursorPropertyType
    {
        Local,
        Global,
        ForwardOnly,
        Scroll,
        Static,
        Keyset,
        Dynamic,
        FastForward,
        ReadOnly,
        ScrollLocks,
        Optimistic,
        TypeWarning
    }

    public class FetchCursorStatement : Statement
    {
        List<FetchCursorOption> Options { get; set; }
        CursorSource Source { get; set; }
        List<VariableExpression> VariableList;

        public FetchCursorStatement(List<FetchCursorOption> options, CursorSource source, List<VariableExpression> variableList)
        {
            Options = options;
            Source = source;
            VariableList = variableList;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("FETCH");
            asm.AddSpace();
            asm.Add(Source);
            if (VariableList != null)
            {
                asm.AddSpace();
                asm.AddToken("INTO");
                asm.AddSpace();
                foreach (VariableExpression var in VariableList)
                {
                    asm.Add(var);
                    if (var != VariableList.Last())
                    {
                        asm.AddToken(",");
                    }
                }
            }
            asm.End(this);
        }
    }

    public class FetchCursorOption : GrammarNode
    {
        public FetchCursorOptionType Type { get; set; }

        public FetchCursorOption(FetchCursorOptionType type)
        {
            Type = type;
        }
    }

    public enum FetchCursorOptionType
    {
        NEXT, PRIOR, FIRST, LAST, ABSOLUTE, RELATIVE
    }

    public class DeallocateStatement : Statement
    {
        CursorSource Source { get; set; }

        public DeallocateStatement(CursorSource source)
        {
            Source = source;
        }
    }

    public class CloseStatement : Statement
    {
        CursorSource Source { get; set; }

        public CloseStatement(CursorSource source)
        {
            Source = source;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("CLOSE");
            asm.AddSpace();
            asm.Add(Source);
            asm.End(this);
        }
    }

    public class TableVariableDeclaration : VariableDeclaration
    {
        public VariableExpression Variable { get; set; }
        public IList<CreateTableDefinition> Definition { get; set; }

        public TableVariableDeclaration(VariableExpression variable, IList<CreateTableDefinition> definition)
        {
            Variable = variable;
            Definition = definition;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Variable);
            asm.AddSpace();
            asm.Add(Variable.Value + "_TYPE");
            asm.End(this);
        }
    }

    public class SetStatement : Statement
    {
        public VariableExpression Variable { get; set; }
        public AssignmentType Operator { get; set; }
        public Expression Expression { get; set; }


        public SetStatement(VariableExpression variable, AssignmentType op, Expression expression)
        {
            Variable = variable;
            Operator = op;
            Expression = expression;
        }

        public SetStatement()
        {
            Variable = null;
            Operator = AssignmentType.Assign;
            Expression = null;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Operator)
            {
                case AssignmentType.XorAssign:
                    AddNote(Note.STRINGIFIER, ResStr.NO_BITWISE_XOR_OPERATOR);
                    asm.End(this);
                    return;
                case AssignmentType.OrAssign:
                    AddNote(Note.STRINGIFIER, ResStr.NO_BITWISE_OR_OPERATOR);
                    asm.End(this);
                    return;
            }
            asm.Add(Variable);
            asm.AddSpace();
            asm.AddToken(":=");
            asm.AddSpace();
            switch (Operator)
            {
                case AssignmentType.AddAssign:
                    asm.AddToken(":");
                    asm.Add(Variable);
                    asm.AddToken(" + ");
                    asm.Add(Expression);
                    break;
                case AssignmentType.AndAssign:
                    asm.AddToken("BITAND(");
                    asm.AddToken(":");
                    asm.Add(Variable);
                    asm.AddToken(", ");
                    asm.Add(Expression);
                     asm.AddToken(")");
                    break;
                case AssignmentType.Assign:
                    asm.Add(Expression);
                    break;
                case AssignmentType.DivAssign:
                    asm.AddToken(":");
                    asm.Add(Variable);
                    asm.AddToken(" / ");
                    asm.Add(Expression);
                    break;
                case AssignmentType.ModAssign:
                    asm.AddToken("MOD(");
                    asm.AddToken(":");
                    asm.Add(Variable);
                    asm.AddToken(", ");
                    asm.Add(Expression);
                    asm.AddToken(")");
                    break;
                case AssignmentType.MulAssign:
                    asm.AddToken(":");
                    asm.Add(Variable);
                    asm.AddToken(" * ");
                    asm.Add(Expression);
                    break;
                case AssignmentType.SubAssign:
                    asm.AddToken(":");
                    asm.Add(Variable);
                    asm.AddToken(" - ");
                    asm.Add(Expression);
                    break;
            }
            asm.End(this);
        }
    }

    public class SetSpecialStatement : SetStatement
    {
        public SetOptionType Type { get; set; }
        public bool OnOrOff { get; set; }

        public SetSpecialStatement(SetOptionType type, bool onOrOff)
        {
            Type = type;
            OnOrOff = onOrOff;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.End(this);
        }
    }

    public enum SetOptionType
    {
        ANSI_WARNINGS, ANSI_NULLS, ANSI_DEFAULTS, ANSI_PADDING,
        ANSI_NULL_DFLT_ON, ANSI_NULL_DFLT_OFF,

        ARITHABORT, ARITHIGNORE,
        FORCEPLAN, FMTONLY, IMPLICIT_TRANSACTIONS,
        NOCOUNT, NOEXEC, NUMERIC_ROUNDABORT, 
        PARSEONLY, QUOTED_IDENTIFIER, REMOTE_PROC_TRANSACTIONS,

        SHOWPLAN_ALL, SHOWPLAN_TEXT, SHOWPLAN_XML, 
        CONCAT_NULL_YIELDS_NULL, CURSOR_CLOSE_ON_COMMIT, 
        STATISTICS_XML, STATISTICS_IO, STATISTICS_PROFILE, STATISTICS_TIME, 
        
        XACT_ABORT
    }

    public class SelectVariableStatement : Statement
    {
        public IList<SelectVariableItem> Items { get; set; }
        public IList<TableSource> FromClause { get; set; }

        public SelectVariableStatement(IList<SelectVariableItem> items)
        {
            Items = items;
        }

        public SelectVariableStatement(IList<SelectVariableItem> items, IList<TableSource> fromClause)
        {
            Items = items;
            FromClause = fromClause;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("SELECT");
            asm.AddSpace();
            foreach (SelectVariableItem item in Items)
            {
                asm.Add(item.Expression);
                if (item != Items.Last())
                {
                    asm.AddToken(", ");
                }
            }
            asm.AddSpace();
            asm.AddToken("INTO");
            asm.AddSpace();
            foreach (SelectVariableItem item in Items)
            {
                asm.Add(item.Variable);
                if (item != Items.Last())
                {
                    asm.AddToken(", ");
                }
            }
            asm.AddSpace();
            if (FromClause != null)
            {
                asm.AddToken("FROM");
                asm.AddSpace();
                foreach (TableSource table in FromClause)
                {
                    asm.Add(table);
                    if (table != FromClause.Last())
                    {
                        asm.AddToken(",");
                        asm.AddSpace();
                        asm.NewLine();
                    }
                }
            }
            else
            {
                asm.AddToken("FROM DUMMY");
            }
            asm.End(this);
        }
    }

    public class SelectVariableItem : SelectItem
    {
        public VariableExpression Variable { get; set; }
        public AssignmentType Operator { get; set; }
        public Expression Expression { get; set; }

        public SelectVariableItem(VariableExpression variable, AssignmentType op, Expression expression)
        {
            Variable = variable;
            Operator = op;
            Expression = expression;
        }
    }
}
