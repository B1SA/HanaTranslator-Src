using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Translator
{
    #region Output

    public class OutputClause : GrammarNode
    {
        public IList<SelectItem> SelectItems { get; set; }
        public OutputClauseInto OutputClauseInto { get; set; }

        public OutputClause(IList<SelectItem> selectItems, OutputClauseInto outputClauseInto)
        {
            SelectItems = selectItems;
            OutputClauseInto = outputClauseInto;
        }
    }

    public class OutputClauseInto : GrammarNode
    {
    }

    public class VariableOutputClauseInto : OutputClauseInto
    {
        public String Variable { get; set; }
        public IList<Identifier> ColumnList { get; set; }

        public VariableOutputClauseInto(String variable, IList<Identifier> columnList)
        {
            Variable = variable;
            ColumnList = columnList;
        }
    }

    public class TableOutputClauseInto : OutputClauseInto
    {
        public DbObject OutputTable { get; set; }
        public IList<Identifier> ColumnList { get; set; }

        public TableOutputClauseInto(DbObject outputTable, IList<Identifier> columnList)
        {
            OutputTable = outputTable;
            ColumnList = columnList;
        }
    }
    #endregion //Output

    #region Statement
    abstract public class Statement : GrammarNode
    {
        public bool Terminate = true;
    }

    abstract public class WithSupportingStatement : Statement
    {
        public IList<WithCommonTable> WithClause { get; set; }
    }

    public class DeleteStatement : WithSupportingStatement
    {
        public TopClause TopClause { get; set; }
        public TableSource Table { get; set; }
        public OutputClause OutputClause { get; set; }
        public IList<TableSource> FromClause { get; set; }
        public WhereClauseSupportingCursor WhereClause { get; set; }
        public IList<QueryHint> OptionClause { get; set; }

        public DeleteStatement(TopClause topClause, TableSource table, OutputClause outputClause, IList<TableSource> fromClause, WhereClauseSupportingCursor whereClause, IList<QueryHint> optionClause)
        {
            TopClause = topClause;
            Table = table;
            OutputClause = outputClause;
            FromClause = fromClause;
            WhereClause = whereClause;
            OptionClause = optionClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (WithClause != null)
            {
                // NOT SUPPORTED
            }
            asm.AddToken("DELETE");
            if (TopClause != null)
            {
                // NOT SUPPORTED
            }
            asm.AddSpace();
            asm.AddToken("FROM");
            asm.IncreaseIndentation();
            asm.AddSpace();
            asm.Add(Table);
            asm.DecreaseIndentation();

            if (OutputClause != null)
            {
                // NOT SUPPORTED
            }
            if (FromClause != null)
            {
                // NOT SUPPORTED
            }
            if (WhereClause != null)
            {
                asm.AddSpace();
                asm.NewLine();
                asm.AddToken("WHERE");
                asm.IncreaseIndentation();
                asm.AddSpace();
                asm.Add(WhereClause);
                asm.DecreaseIndentation();
            }
            if (OptionClause != null)
            {
                // NOT SUPPORTED
            }
            asm.End(this);
        }
    }

    public class TruncateTableStatement : Statement
    {
        public DbObject Table { get; set; }

        public TruncateTableStatement(DbObject table)
        {
            Table = table;
        }
    }

    public abstract class InsertTarget : GrammarNode
    {
    }

    public class DbObjectInsertTarget : InsertTarget
    {
        public DbObjectTableSource TableSource { get; set; }

        public DbObjectInsertTarget(DbObject dbObject)
        {
            TableSource = new DbObjectTableSource(dbObject, null, null, null);
        }

        override public void Assembly(Assembler asm)
        {
            if (TableSource != null)
            {
                asm.Add(TableSource.DbObject);
            }
        }
    }

    public class VariableInsertTarget : InsertTarget
    {
        public string Variable { get; set; }

        public VariableInsertTarget(string variable)
        {
            Variable = variable;
        }
    }

    public class RowsetFunctionInsertTarget : InsertTarget
    {
        public RowsetFunction RowsetFunction { get; set; }
        public IList<TableHint> Hints { get; set; }

        public RowsetFunctionInsertTarget(RowsetFunction rowsetFunction, IList<TableHint> hints)
        {
            RowsetFunction = rowsetFunction;
            Hints = hints;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (RowsetFunction != null)
            {
                asm.Add(RowsetFunction);
                asm.AddSpace();
            }
            if (Hints != null)
            {
                asm.AddToken("WITH");
                asm.AddSpace();
                asm.Add("(");

                foreach (TableHint hint in Hints)
                {
                    asm.Add(hint);
                    if (hint != Hints.Last())
                    {
                        asm.Add(",");
                        asm.AddSpace();
                    }
                }
                asm.Add(")");
            }
            asm.End(this);
        }
    }

    public class DbObjectIndexTarget : GrammarNode
    {
        public DbObjectTableSource TableSource { get; set; }

        public DbObjectIndexTarget(DbObject dbObject)
        {
            TableSource = new DbObjectTableSource(dbObject, null, null, null);
        }

        override public void Assembly(Assembler asm)
        {
            if (TableSource != null)
            {
                asm.Add(TableSource.DbObject);
            }
        }
    }

    public abstract class ExecParam : GrammarNode
    {
    }

    public class ExecSqlParam : ExecParam
    {
        public Expression Expression { get; set; }
        public bool Output { get; set; }

        public ExecSqlParam(Expression expression, bool output)
        {
            Output = output;
            Expression = expression;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Expression);
            asm.End(this);
        }
    }

    public class ExecSpParam : ExecSqlParam
    {
        public VariableExpression Parameter { get; set; }

        public ExecSpParam(VariableExpression parameter, ExecSqlParam paramValue)
            : base(paramValue.Expression, paramValue.Output)
        {
            Parameter = parameter;
        }
    }

    public enum ContextType
    {
        LOGIN, USER
    }

    public class ContextClause : GrammarNode
    {
        public ContextType ContextType { get; set; }
        public String Name { get; set; }

        public ContextClause( ContextType type, String name)
        {
            ContextType = type;
            Name = name; 
        }
    }

    public abstract class ExecStatement : Statement
    {
    }

    public class ExecStatementSQL : ExecStatement
    {
        public Expression Expression { get; set; }
        public IList<ExecParam> Params { get; set; }
        public ContextClause Context { get; set; }
        public Identifier LinkedServer { get; set; }

        public ExecStatementSQL(Expression expression, IList<ExecParam> execParams, ContextClause context, Identifier linkedServer)
        {
            Expression = expression;
            Params = execParams;
            Context = context;
            LinkedServer = linkedServer;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("EXEC");
            asm.Add("(");
            if (Params != null)
                Expression.AddNote(Note.STRINGIFIER, ResStr.NO_PARAMS_IN_EXEC);
            /*    foreach (ExecParam param in Params)
                {
                    retVal += param.ToStringifier();
                    retVal += ",";
                }
             */
            asm.Add(Expression);
            asm.Add(")");
            if (Context != null)
            {
                //NOT SUPPORTED;
            }
            if (LinkedServer != null)
            {
                //NOT SUPPORTED
            }
            asm.End(this);
        }
    }

    public class ExecStatementSP : ExecStatement
    {
        public VariableExpression ReturnVariable { get; set; }
        public ExecModuleName ModuleName { get; set; }
        public IList<ExecParam> Params { get; set; }
        public IList<ExecOption> ExecOptions { get; set; }

        public ExecStatementSP(VariableExpression retVariable, ExecModuleName moduleName, IList<ExecParam> execParams, IList<ExecOption> execOptions)
        {
            ReturnVariable = retVariable;
            ModuleName = moduleName;
            Params = execParams;
            ExecOptions = execOptions;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("CALL");
            asm.AddSpace();
            asm.Add(ModuleName);

            if (Params != null)
            {
                asm.Add("(");
                foreach(ExecParam param in Params)
                {
                    asm.Add(param);
                    if (param != Params.Last())
                    {
                        asm.Add(",");
                        asm.AddSpace();
                    }
                }
                asm.Add(")");
            }

            if (ReturnVariable != null)
            {
                ReturnVariable.AddNote(Note.STRINGIFIER, ResStr.NO_RET_VAR_IN_EXEC);
            }
            asm.End(this);
        }
    }

    abstract public class ExecModuleName : GrammarNode
    {
    }

    public class DbObjectExecModuleName : ExecModuleName
    {
        public DbObject Name { get; set; }
        public int Number { get; set; }

        public DbObjectExecModuleName(DbObject name, int number)
        {
            Name = name;
            Number = number;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            // TODO object should not be modified during printing
            /*foreach (Identifier identifier in Name.Identifiers)
            {
                identifier.Type = IdentifierType.Plain;
            }*/

            if (Number >= 0)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_MODULE_NUMBER_IN_EXEC);
            }
            asm.Add(Name);
            asm.End(this);
        }
    }

    public class VariableExecModuleName : ExecModuleName
    {
        public VariableExpression Variable { get; set; }

        public VariableExecModuleName(VariableExpression variable)
        {
            Variable = variable;
        }

        // TODO support ToStringifier
    }

    abstract public class ExecOption : GrammarNode
    {
    }

    public class RecompileExecOption : ExecOption
    {
        public RecompileExecOption()
        {
        }

        // TODO support ToStringifier
    }

    public class SimpleResultSetsExecOption : ExecOption
    {
        public SimpleResultSetsType Type { get; set; }

        public SimpleResultSetsExecOption(SimpleResultSetsType type)
        {
            Type = type;
        }

        // TODO support ToStringifier
    }

    public enum SimpleResultSetsType
    {
        Undefined, None
    }

    public class ComplexResultSetsExecOption : ExecOption
    {
        public IList<ResultSetsDefinition> Definitions { get; set; }

        public ComplexResultSetsExecOption(IList<ResultSetsDefinition> definitions)
        {
            Definitions = definitions;
        }

        // TODO support ToStringifier
    }

    abstract public class ResultSetsDefinition : GrammarNode
    {
    }

    public class ColumnsResultSetsDefinition : ResultSetsDefinition
    {
        public IList<ResultSetsColumn> Columns { get; set; }

        public ColumnsResultSetsDefinition(IList<ResultSetsColumn> columns)
        {
            Columns = columns;
        }

        // TODO support ToStringifier
    }

    public class ResultSetsColumn : GrammarNode
    {
        public Identifier Name { get; set; }
        public DataType Type { get; set; }
        public Identifier Collation { get; set; }
        public bool? IsNull { get; set; }

        public ResultSetsColumn(Identifier name, DataType type, Identifier collation, bool? isNull)
        {
            Name = name;
            Type = type;
            Collation = collation;
            IsNull = isNull;
        }

        // TODO support ToStringifier
    }

    public class ObjectResultSetsDefinition : ResultSetsDefinition
    {
        public DbObject Object { get; set; }

        public ObjectResultSetsDefinition(DbObject obj)
        {
            Object = obj;
        }

        // TODO support ToStringifier
    }

    public class TypeResultSetsDefinition : ResultSetsDefinition
    {
        public DataType Type { get; set; }

        public TypeResultSetsDefinition(DataType type)
        {
            Type = type;
        }

        // TODO support ToStringifier
    }

    public class ForXmlResultSetsDefinition : ResultSetsDefinition
    {
        public ForXmlResultSetsDefinition()
        {
        }

        // TODO support ToStringifier
    }

    public abstract class ValuesClause : GrammarNode
    {
        virtual public void Add(List<Expression> record) { }
    }

    public class ValuesClauseValues : ValuesClause
    {
        public IList<List<Expression>> Values { get; set; }

        public ValuesClauseValues(List<Expression> record)
        {
            Values = new List<List<Expression>>();
            Values.Add(record);
        }

        override public void Add(List<Expression> record)
        {
            Values.Add(record);
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (Values != null)
            {
                asm.AddToken("VALUES (");
                foreach(Expression exp in Values[0])
                {
                    asm.Add(exp);
                    if (exp != Values[0].Last())
                    {
                        asm.AddToken(",");
                        asm.AddSpace();
                    }
                }
                asm.AddToken(")");
            }
            asm.End(this);
        }
    }

    public class ValuesClauseExec : ValuesClause
    {
        public ExecStatement ExecStatement { get; set; }

        public ValuesClauseExec(ExecStatement execStatement)
        {
            ExecStatement = execStatement;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(ExecStatement);
            asm.End(this);
        }
    }

    public class ValuesClauseSelect : ValuesClause
    {
        public SelectStatement Statement { get; set; }

        public ValuesClauseSelect(SelectStatement statement)
        {
            Statement = statement;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("(");
            asm.Add(Statement);
            asm.AddToken(")");
            asm.End(this);
        }
    }

    public class ValuesClauseDefault : ValuesClause
    {
    }

    public class InsertStatement : WithSupportingStatement
    {
        public TopClause TopClause { get; set; }
        public InsertTarget InsertTarget { get; set; }
        public ValuesClause ValuesClause { get; set; }
        public OutputClause OutputClause { get; set; }
        public IList<QueryHint> OptionClause { get; set; }
        public IList<Identifier> ColumnList { get; set; }

        public InsertStatement(TopClause topClause, InsertTarget insertTarget, IList<Identifier> columnList, OutputClause outputClause, ValuesClause valuesClause)
        {
            TopClause = topClause;
            InsertTarget = insertTarget;
            ColumnList = columnList;
            OutputClause = outputClause;
            ValuesClause = valuesClause;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (ValuesClause is ValuesClauseExec)
            {
                asm.Add(ValuesClause);
                InsertTarget.AddNote(Note.STRINGIFIER, ResStr.NO_INSERT_TARGET_IN_INSERT);
            }
            else if (InsertTarget != null)
            {
                asm.AddToken("INSERT INTO");
                asm.AddSpace();

                asm.Add(InsertTarget);

                if (ColumnList != null && ColumnList.Count > 0)
                {
                    asm.AddSpace();
                    asm.AddToken("(");

                    foreach (Identifier column in ColumnList)
                    {
                        asm.Add(column);
                        if (column != ColumnList.Last())
                        {
                            asm.AddToken(",");
                            asm.AddSpace();
                        }
                    }

                    asm.AddToken(")");
                }

                if (ValuesClause != null)
                {
                    asm.AddSpace();
                    asm.Add(ValuesClause);
                }
            }
            asm.End(this);
        }
    }
    
    public class UpdateStatement : WithSupportingStatement
    {
        static int id = 0;

        int currentId;
        public TopClause TopClause { get; set; }
        public TableSource TableSource { get; set; }
        public IList<SetItem> SetClause { get; set; }
        public OutputClause OutputClause { get; set; }
        public IList<TableSource> FromClause { get; set; }
        public WhereClauseSupportingCursor WhereClause { get; set; }
        public IList<QueryHint> OptionClause { get; set; }

        public UpdateStatement(TopClause topClause, TableSource table, IList<SetItem> setClause, OutputClause outputClause, IList<TableSource> fromClause, WhereClauseSupportingCursor whereClause, IList<QueryHint> optionClause)
        {
            currentId = id++;
            TopClause = topClause;
            TableSource = table;
            SetClause = setClause;
            OutputClause = outputClause;
            FromClause = fromClause;
            WhereClause = whereClause;
            OptionClause = optionClause;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (WithClause != null)
            {
                // NOT SUPPORTED
            }
            asm.AddToken("UPDATE");
            asm.IncreaseIndentation();
            if (TopClause != null)
            {
                asm.AddSpace();
                asm.Add(TopClause);
            }

            asm.AddSpace();
            asm.Add(TableSource);
            asm.AddSpace();
            asm.AddToken("SET");
            asm.AddSpace();
            foreach(SetItem item in SetClause)
            {
                asm.Add(item);
                if (item != SetClause.Last())
                {
                    asm.AddToken(",");
                    asm.AddSpace();
                }
            }
            asm.DecreaseIndentation();
            if (OutputClause != null)
            {
                // NOT SUPPORTED
            }
            if (FromClause != null)
            {
                // NOT SUPPORTED
                asm.AddSpace();
                asm.NewLine();
                asm.AddToken("FROM");
                asm.IncreaseIndentation();
                asm.AddSpace();
                foreach(TableSource table in FromClause)
                {
                    asm.Add(table);
                    if (table != FromClause.Last())
                    {
                        asm.Add(",");
                        asm.AddSpace();
                    }
                }
                asm.DecreaseIndentation();
            }
            if (WhereClause != null)
            {
                asm.AddSpace();
                asm.NewLine();
                asm.AddToken("WHERE");
                asm.IncreaseIndentation();
                asm.AddSpace();
                asm.Add(WhereClause);
                asm.DecreaseIndentation();
            }
            if (OptionClause != null)
            {
                // NOT SUPPORTED
            }
            asm.End(this);
        }
    }

    public class SelectStatement : WithSupportingStatement
    {
        public QueryExpression QueryExpression { get; set; }
        public ComputeClause ComputeClause { get; set; }
        public ForClauseType ForClause { get; set; }
        public IList<QueryHint> OptionClause { get; set; }

        public SelectStatement(QueryExpression queryExpression, ComputeClause computeClause, ForClauseType forClause, IList<QueryHint> optionClause)
        {
            QueryExpression = queryExpression;
            ComputeClause = computeClause;
            ForClause = forClause;
            OptionClause = optionClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (WithClause != null)
            {
                WithClause[0].Assembly(asm);
            }
            asm.Add(QueryExpression);
            if (ComputeClause != null)
            {
                AddNote(Note.STRINGIFIER, "COMPUTE clause is not supported for HANA");
            }

            switch (ForClause)
            {
                case ForClauseType.ForBrowse:
                    AddNote(Note.STRINGIFIER, "FOR BROWSE clause is not supported for HANA");
                    break;
            }
            asm.End(this);
       }

    }
    #endregion

    #region SetStatement

    abstract public class SetItem : GrammarNode
    {
    }

    public class SetItemColumn : SetItem
    {
        public DbObject ColumnName { get; set; }
        public SetAssignment Assignment { get; set; }

        public SetItemColumn(DbObject columnName, SetAssignment assignemnt)
        {
            ColumnName = columnName;
            Assignment = assignemnt;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(ColumnName);
            asm.AddSpace();
            asm.Add(Assignment);
            asm.End(this);
        }
    }

    public class SetItemVariable : SetItem
    {
        public VariableExpression Variable { get; set; }
        public SetAssignment Assignment { get; set; }

        public SetItemVariable(VariableExpression variable, SetAssignment assignment)
       {
           Variable = variable;
           Assignment = assignment;
       }
        
       public override void Assembly(Assembler asm)
       {
           asm.Begin(this);
           asm.Add(Variable.Value);
           asm.AddSpace();
           asm.AddToken(":=");
           asm.AddSpace();
           asm.Add(Assignment);
           asm.End(this);
       }

    }

    abstract public class SetAssignment : GrammarNode
    {
        public static string AssignmentToString(AssignmentType Assignment)
        {
            string assignment = string.Empty;
            switch (Assignment)
            {
                case AssignmentType.AddAssign:
                    assignment = "+";
                    break;
                case AssignmentType.SubAssign:
                    assignment = "-";
                    break;
                case AssignmentType.MulAssign:
                    assignment = "*";
                    break;
                case AssignmentType.DivAssign:
                    assignment = "+";
                    break;
                case AssignmentType.ModAssign:
                    // NOT SUPPORTED
                    break;
                case AssignmentType.AndAssign:
                    // NOT SUPPORTED
                    break;
                case AssignmentType.XorAssign:
                    // NOT SUPPORTED
                    break;
                case AssignmentType.OrAssign:
                    // NOT SUPPORTED
                    break;
                case AssignmentType.Assign:
                    assignment = "=";
                    break;
            }
            return assignment;
        }
    }

    public class SetColumnAssignmentEquals : SetAssignment
    {
        public Expression Expression { get; set; }

        public SetColumnAssignmentEquals(Expression expr)
        {
            Expression = expr;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("="); // TODO this is probably wrong
            asm.AddSpace();
            asm.Add(Expression);
            asm.End(this);
        }
    }

    public class SetColumnAssignmentWrite : SetAssignment
    {
        public SetNumber Offset { get; set; }
        public SetNumber Length { get; set; }
        public Expression Expression { get; set; }

        public SetColumnAssignmentWrite(Expression exp, SetNumber offset, SetNumber length)
        {
            Expression = exp;
            Offset = offset;
            Length = length;
        }

        public static bool IsDotWrite(DbObject dbObject)
        {
            // This is used in grammar to decide if a dbObject ends with something.WRITE.
            if (dbObject.Identifiers.Count >= 2)
            {
                Identifier firstLast = dbObject.Identifiers[dbObject.Identifiers.Count - 1];
                Identifier secondLast = dbObject.Identifiers[dbObject.Identifiers.Count - 2];
                if (firstLast.Type == IdentifierType.Plain && firstLast.Name.ToLowerInvariant() == "write" &&
                    secondLast.Name != String.Empty)
                {
                    return true;
                }
            }
            return false;
        }

        public static void RemoveDotWrite(DbObject dbObject)
        {
            // Remove the .WRITE suffix. Assume IsDotWrite was called and returned true.
            dbObject.Identifiers.RemoveAt(dbObject.Identifiers.Count - 1);
        }
    }

    public class SetColumnAssignmentOperator : SetAssignment
    {
        public AssignmentType Assignment { get; set; }
        public Expression Expression { get; set; }

        public SetColumnAssignmentOperator(AssignmentType assignment, Expression exp)
        {
            Assignment = assignment;
            Expression = exp;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(AssignmentToString(Assignment));
            asm.AddSpace();
            if (Expression != null)
            {
                asm.Add(Expression);
            }
            asm.End(this);
        }
    }

    public class SetVariableColumnAssignment : SetAssignment
    {
        public DbObject ColumnName { get; set; }
        public AssignmentType Assignment { get; set; }
        public Expression Expression { get; set; }

        public SetVariableColumnAssignment(DbObject columnName, AssignmentType assignment, Expression expression)
        {
            ColumnName = columnName;
            Assignment = assignment;
            Expression = expression;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(ColumnName);
            asm.AddSpace();
            asm.Add(AssignmentToString(Assignment));
            asm.AddSpace();
            if (Expression != null)
            {
                asm.Add(Expression);
            }
            asm.End(this);
        }
    }

    public class SetVariableAssignment : SetAssignment
    {
        public AssignmentType Assignment { get; set; }
        public Expression Expression { get; set; }

        public SetVariableAssignment(AssignmentType assignment, Expression expression)
        {
            Assignment = assignment;
            Expression = expression;
        }
        
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(AssignmentToString(Assignment));
            asm.AddSpace();
            if (Expression != null)
            {
                asm.Add(Expression);
            }
            asm.End(this);
        }
    }

    public enum SetDateFormatType
    {
        DMY, DYM, MDY, YMD, YDM, MYD
    }

    public class SetDateFormatStatement : SetStatement
    {
        public SetDateFormatType    DateFormat { get; set; }

        public SetDateFormatStatement(SetDateFormatType type)
        {
            DateFormat = type;
        }
    }

	public class SetDateFormatVariableStatement : SetStatement
    {
        public SetDateFormatVariableStatement(VariableExpression variable)
        {
            Variable = variable;
        }
    }

    public class SetDateFirstStatement : SetStatement
    {
        public int DateFirst { get; set; }

        public SetDateFirstStatement(int dateFirst)
        {
            DateFirst = dateFirst;
        }
    }

    public class SetDateFirstVariableStatement : SetStatement
    {
        public SetDateFirstVariableStatement(VariableExpression variable)
        {
            Variable = variable;
        }
    }

    public class SetLockTimeoutStatement : SetStatement
    {
        public int TimeOut { get; set; }

        public SetLockTimeoutStatement(int timeout)
        {
            TimeOut = timeout;
        }
    }

    public class SetIdentityInsertStatement : SetStatement
    {
        public DbObject DBObject { get; set; }
        public bool On { get; set; }

        public SetIdentityInsertStatement (DbObject dbObject, bool on)
        {
            On = on;
            DBObject = dbObject;
        }
    }

    abstract public class SetNumber : GrammarNode
    {

    }

    public class SetNumberInteger : SetNumber
    {
        public Int32 Value { get; set; }

        public SetNumberInteger(Int32 value)
        {
            Value = value;
        }
    }

    public class SetNumberNull : SetNumber
    {
        public SetNumberNull() { }
    }

    #endregion //SetStatement

    #region WithCommonTable
    public class WithCommonTable : GrammarNode
    {
        public Identifier Name { get; set; }
        public IList<Identifier> Columns { get; set; }
        public QueryExpression Query { get; set; }

        public WithCommonTable(Identifier name, IList<Identifier> columns, QueryExpression query)
        {
            Name = name;
            Columns = columns;
            Query = query;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddSpace();
            asm.AddToken("WITH");
            asm.AddSpace();
            asm.IncreaseIndentation();
            asm.Add(Name);
            asm.AddSpace();
            asm.Add("AS");
            asm.AddSpace();
            asm.AddToken("(");
            asm.Add(Query);
            asm.AddToken(")");
            asm.AddSpace();
            asm.DecreaseIndentation();
            asm.End(this);
        }
    }
    #endregion

    #region QueryExpression
    abstract public class QueryExpression : GrammarNode
    {
    }

    public enum QueryExpressionOperatorType
    {
        Union, UnionAll, Except, Intersect
    }

    public class OperatorQueryExpression : QueryExpression
    {
        public QueryExpression LeftExpression { get; set; }
        public QueryExpressionOperatorType Type { get; set; }
        public QueryExpression RightExpression { get; set; }

        public OperatorQueryExpression(QueryExpression left, QueryExpressionOperatorType type, QueryExpression right)
        {
            LeftExpression = left;
            Type = type;
            RightExpression = right;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(LeftExpression);
            asm.AddSpace();
            asm.NewLine();
            switch (Type)
            {
                case QueryExpressionOperatorType.Union:
                    asm.AddToken("UNION");
                    break;
                case QueryExpressionOperatorType.UnionAll:
                    asm.AddToken("UNION ALL");
                    break;
                case QueryExpressionOperatorType.Except:
                    asm.AddToken("EXCEPT");
                    break;
                case QueryExpressionOperatorType.Intersect:
                    asm.AddToken("INTERSECT");
                    break;
            }
            asm.AddSpace();
            asm.NewLine();
            asm.Add(RightExpression);
            asm.End(this);
        }
    }

    public class QuerySpecification : QueryExpression
    {
        public SelectClause SelectClause { get; set; }
        public DbObject IntoClause { get; set; }
        public IList<TableSource> FromClause { get; set; }
        public IList<GroupByItem> GroupByClause { get; set; }
        public Expression WhereClause { get; set; }
        public Expression HavingClause { get; set; }
        public IList<OrderByItem> OrderByClause { get; set; }

        public QuerySpecification(SelectClause selectClause, DbObject intoClause, IList<TableSource> fromClause,
            Expression whereClause, IList<GroupByItem> groupByClause, Expression havingClause, IList<OrderByItem> orderByClause)
        {
            SelectClause = selectClause;
            IntoClause = intoClause;
            FromClause = fromClause;
            WhereClause = whereClause;
            GroupByClause = groupByClause;
            HavingClause = havingClause;
            OrderByClause = orderByClause;
        }
        
        override public void Assembly(Assembler asm)
        {
            // TODO
            asm.Begin(this);
            asm.Add(SelectClause);
            if (FromClause != null)
            {
                asm.AddSpace();
                asm.NewLine();
                asm.AddToken("FROM");
                asm.IncreaseIndentation();
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
                asm.DecreaseIndentation();
            }
            if (WhereClause != null)
            {
                asm.AddSpace();
                asm.NewLine();
                asm.AddToken("WHERE");
                asm.IncreaseIndentation();
                asm.AddSpace();
                asm.Add(WhereClause);
                asm.DecreaseIndentation();
           }
           if (GroupByClause != null)
           {
                asm.AddSpace();
                asm.NewLine();
                asm.AddToken("GROUP BY");
                asm.IncreaseIndentation();
                asm.AddSpace();
                foreach(GroupByItem item in GroupByClause)
                {
                    asm.Add(item);
                    if (item != GroupByClause.Last())
                    {
                        asm.AddToken(",");
                        asm.AddSpace();
                    }
                }
                asm.DecreaseIndentation();
           }
           if (HavingClause != null)
           {
                asm.AddSpace();
                asm.AddToken("HAVING");
                asm.AddSpace();
                asm.Add(HavingClause);
           }
           if (OrderByClause != null)
           {
               asm.AddSpace();
               asm.NewLine();
               asm.AddToken("ORDER BY");
               asm.IncreaseIndentation();
               asm.AddSpace();
               foreach (OrderByItem item in OrderByClause)
               {
                   asm.Add(item);
                   if (item != OrderByClause.Last())
                   {
                       asm.Add(",");
                       asm.AddSpace();
                   }
               }
               asm.DecreaseIndentation();
           }
           asm.End(this);
        }
    }
    #endregion

    #region SelectClause
    public class SelectClause : GrammarNode
    {
        public bool IsDistinct { get; set; }
        public TopClause TopClause { get; set; }
        public IList<SelectItem> SelectItems { get; set; }

        public SelectClause(bool isDistinct, TopClause topClause, IList<SelectItem> selectItems)
        {
            IsDistinct = isDistinct;
            TopClause = topClause;
            SelectItems = selectItems;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("SELECT");
            asm.IncreaseIndentation();
            asm.AddSpace();
            if (IsDistinct)
            {
                asm.AddToken("DISTINCT");
                asm.AddSpace();
            }
            else
            {
                //retVal += "ALL ";
            }
            if (TopClause != null)
            {
                asm.Add(TopClause);
                asm.AddSpace();
            }

            foreach (SelectItem item in SelectItems)
            {
                asm.Add(item);
                if (item != SelectItems.Last())
                {
                    asm.AddToken(", ");
                    asm.Breakable();
                }
            }
            asm.DecreaseIndentation();
            asm.End(this);
        }
    }

    public class TopClause : GrammarNode
    {
        public Expression TopCount { get; set; }
        public bool IsPercent { get; set; }
        public bool IsWithTies { get; set; }

        public TopClause(Expression topCount, bool isPercent, bool isWithTies)
        {
            TopCount = topCount;
            IsPercent = isPercent;
            IsWithTies = isWithTies;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("TOP");
            asm.AddSpace();
            asm.Add(TopCount);
            if (IsPercent)
            {
                asm.AddSpace();
                asm.AddToken("PERCENT");
            }
            if (IsWithTies)
            {
                asm.AddSpace();
                asm.AddToken("WITH TIES");
            }
            asm.End(this);
        }
    }
    #endregion

    #region SelectItem
    abstract public class SelectItem : GrammarNode
    {
    }

    public class WildcardSelectItem : SelectItem
    {
        public WildcardSelectItem()
        {
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("*");
            asm.End(this);
        }
    }

    public class TableWildcardSelectItem : SelectItem
    {
        public DbObject Table { get; set; }

        public TableWildcardSelectItem(DbObject table)
        {
            Table = table;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Table);
            asm.AddToken(".*");
            asm.End(this);
        }
    }

    public class SpecialColumnSelectItem : SelectItem
    {
        public SelectSpecialColumnType Type { get; set; }
        public DbObject Table { get; set; }

        public SpecialColumnSelectItem(SelectSpecialColumnType type, DbObject table)
        {
            Type = type;
            Table = table;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Table);
            // TODO is this correct for HANA?
            switch (Type)
            {
                case SelectSpecialColumnType.Identity:
                    asm.AddToken(".$IDENTITY");
                    break;
                case SelectSpecialColumnType.RowGuid:
                    asm.AddToken(".$ROWGUID");
                    break;
            }
            asm.End(this);
        }
    }

    public enum SelectSpecialColumnType
    {
        Identity, RowGuid
    }

    public class ExpressionSelectItem : SelectItem
    {
        public Expression Expression { get; set; }
        public SelectAlias Alias { get; set; }

        [ExcludeFromChildrenList, System.ComponentModel.DefaultValue(false)]
        public override bool Hide
        {
            get
            {
                return Expression.Hide;
            }
        }

        public ExpressionSelectItem(Expression expression, SelectAlias alias)
        {
            Expression = expression;
            Alias = alias;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Expression);
            if (Alias != null)
            {
                asm.AddSpace();
                asm.AddToken("AS");
                asm.AddSpace();
                asm.Add(Alias);
            }
            asm.End(this);
        }
    }
    #endregion

    public class DollarActionSelectItem : SelectItem
    {
        public SelectAlias Alias { get; set; }

        public DollarActionSelectItem(SelectAlias alias)
        {
            Alias = alias;
        }

        override public void Assembly(Assembler asm)
        {
            // TODO is this correct for HANA?
            asm.Begin(this);
            asm.AddToken("$ACTION");

            if (Alias != null)
            {
                asm.AddToken("AS");
                asm.AddSpace();
                asm.Add(Alias);
            }
            asm.End(this);
        }
    }

    #region SelectAlias
    abstract public class SelectAlias : GrammarNode
    {
    }

    public class IdentifierSelectAlias : SelectAlias
    {
        public Identifier Identifier;

        public IdentifierSelectAlias(Identifier identifier)
        {
            Identifier = identifier;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Identifier);
            asm.End(this);
        }
    }

    public class StringLiteralSelectAlias : SelectAlias
    {
        public StringLiteral String;

        public StringLiteralSelectAlias(StringLiteral str)
        {
            String = str;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken(String.Type == StringLiteralType.Unicode ? "n" : "");
            asm.AddToken("\"");
            asm.Add(String.String);
            asm.AddToken("\"");
            asm.End(this);
        }
    }
    #endregion

    #region WhereClauseSupportingCursor

    public abstract class WhereClauseSupportingCursor : GrammarNode
    {

    }

    public class WhereClauseExpression : WhereClauseSupportingCursor
    {
        public Expression WhereClause { get; set; }

        public WhereClauseExpression(Expression whereClause)
        {
            WhereClause = whereClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(WhereClause);
            asm.End(this);
        }
    }

    public class WhereClauseCursor : WhereClauseSupportingCursor
    {
        public CursorSource Cursor { get; set; }

        public WhereClauseCursor(CursorSource cursorSource)
        {
            Cursor = cursorSource;
        }

        override public void Assembly(Assembler asm)
        {
            //HANA doesn't support cursor in delete statement
            AddNote(Note.STRINGIFIER, ResStr.NO_CURSOR_IN_WHERE_CLAUSE);
        }
    }

    #endregion //WhereClauseSupportingCursor


    #region TableSource
    abstract public class TableSource : GrammarNode
    {
    }

    public enum JoinType
    {
        Inner, Left, Right, Full, Cross
    }

    public enum JoinHint
    {
        None, Loop, Hash, Merge, Remote
    }

    public class VariableTableSource : TableSource
    {
        public string Variable { get; set; }
        public Identifier Alias { get; set; }

        public VariableTableSource( string variable,  Identifier alias)
        {
            Variable = variable;
            Alias = alias;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken(":");
            asm.Add(Variable);
            asm.End(this);
        }
    }

    public class VariableFunctionTableSource : TableSource
    {
        public string Variable { get; set; }
        public GenericScalarFunctionExpression Function { get; set; }
        public Identifier Alias { get; set; }
        public IList<Identifier> Columns { get; set; }

        public VariableFunctionTableSource(string variable, GenericScalarFunctionExpression function, Identifier alias, IList<Identifier> columns)
        {
            Variable = variable;
            Function = function;
            Alias = alias;
            Columns = columns;
        }
    }

    public class JoinedTableSource : TableSource
    {
        public TableSource LeftTableSource { get; set; }
        public JoinType Type { get; set; }
        public JoinHint Hint { get; set; }
        public TableSource RightTableSource { get; set; }
        public Expression Condition { get; set; }

        public JoinedTableSource(TableSource left, JoinType type, JoinHint hint, TableSource right, Expression condition)
        {
            Type = type;
            Hint = hint;
            LeftTableSource = left;
            RightTableSource = right;
            Condition = condition;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(LeftTableSource);
            asm.AddSpace();
            asm.NewLine();

            switch (Type)
            {
                case JoinType.Cross:
                    asm.AddToken("CROSS JOIN");
                    break;
                case JoinType.Full:
                    asm.AddToken("JOIN");
                    break;
                case JoinType.Inner:
                    asm.AddToken("INNER JOIN");
                    break;
                case JoinType.Left:
                    asm.AddToken("LEFT OUTER JOIN");
                    break;
                case JoinType.Right:
                    asm.AddToken("RIGHT OUTER JOIN");
                    break;
            }
            asm.AddSpace();

            asm.Add(RightTableSource);
            asm.IncreaseIndentation();
            if (Condition != null)
            {
                asm.AddSpace();
                asm.AddToken("ON");
                asm.AddSpace();
                asm.Add(Condition);
            }
            asm.DecreaseIndentation();
            asm.End(this);
        }
    }

    public class SubqueryTableSource : TableSource
    {
        public QueryExpression Query { get; set; }
        public Identifier Alias { get; set; }

        public SubqueryTableSource(QueryExpression query, Identifier alias)
        {
            Query = query;
            Alias = alias;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("(");
            asm.Add(Query);
            asm.AddToken(")");
            if (Alias != null)
            {
                asm.AddSpace();
                asm.AddToken("AS");
                asm.AddSpace();
                asm.Add(Alias);
            }
            asm.End(this);
        }
    }

    public class DbObjectTableSource : TableSource
    {
        public DbObject DbObject { get; set; }
        public Identifier Alias { get; set; }
        public TableSampleClause TableSampleClause { get; set; }
        public IList<TableHint> Hints { get; set; }

        public DbObjectTableSource(DbObject dbObject, Identifier alias, TableSampleClause tableSampleClause, IList<TableHint> hints)
        {
            DbObject = dbObject;
            Alias = alias;
            TableSampleClause = tableSampleClause;
            Hints = hints;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (DbObject != null)
            {
                asm.Add(DbObject);
            }
            if (Alias != null)
            {
                asm.AddSpace();
                asm.Add(Alias);
            }
            if (Hints != null)
            {
                foreach (TableHint hint in Hints)
                {
                    asm.AddSpace();
                    asm.Add(hint);
                }
            }
            asm.End(this);
        }
    }

    public class RowsetFunctionTableSource : TableSource
    {
        public RowsetFunction Function { get; set; }
        public Identifier Alias { get; set; }
        public IList<Identifier> Columns { get; set; }

        public RowsetFunctionTableSource(RowsetFunction function, Identifier alias, IList<Identifier> columns)
        {
            Function = function;
            Alias = alias;
            Columns = columns;
        }
    }

    public class NestedParensTableSource : TableSource
    {
        public TableSource TableSource { get; set; }

        public NestedParensTableSource(TableSource tableSource)
        {
            TableSource = tableSource;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("(");
            asm.Add(TableSource);
            asm.AddToken(")");
            asm.End(this);
        }
    }

    public class InsertDmlTableSource : TableSource
    {
        public Statement Statement { get; set; }
        public Identifier Alias { get; set; }
        public IList<Identifier> Columns { get; set; }

        public InsertDmlTableSource(Statement statement, Identifier alias, IList<Identifier> columns)
        {
            Statement = statement;
            Alias = alias;
            Columns = columns;
        }
    }
    #endregion

    public class TableSampleClause : GrammarNode
    {
        public Expression SampleNumber { get; set; }
        public PercentOrRowsType PercentOrRows { get; set; }
        public Expression RepeatSeed { get; set; }

        public TableSampleClause(Expression sampleNumber, PercentOrRowsType percentOrRows, Expression repeatSeed)
        {
            SampleNumber = sampleNumber;
            PercentOrRows = percentOrRows;
            RepeatSeed = repeatSeed;
        }
    }

    public enum PercentOrRowsType
    {
        Percent, Rows
    }

    #region TableHint, IndexValue
    abstract public class TableHint : GrammarNode
    {
    }

    public enum SimpleTableHintType
    {
        NoExpand, ForceScan, HoldLock, NoLock, NoWait, PagLock, ReadCommitted, 
        ReadCommittedLock, ReadPast, ReadUncommitted, RepeatableRead,
        RowLock, Serializable, TabLock, TabLockX, UpdLock, XLock
    }

    public class SimpleTableHint : TableHint
    {
        public SimpleTableHintType Type { get; set; }

        public SimpleTableHint(SimpleTableHintType type)
        {
            Type = type;
        }
    }

    public class IndexTableHint : TableHint
    {
        public IList<IndexValue> Indexes { get; set; }

        public IndexTableHint(IList<IndexValue> indexes)
        {
            Indexes = indexes;
        }
    }

    abstract public class IndexValue : GrammarNode
    {
    }

    public class IdentifierIndexValue : IndexValue
    {
        public Identifier Value { get; set; }

        public IdentifierIndexValue(Identifier value)
        {
            Value = value;
        }
    }

    public class IntegerIndexValue : IndexValue
    {
        public int Value { get; set; }

        public IntegerIndexValue(int value)
        {
            Value = value;
        }
    }

    public class ForceSeekTableHint : TableHint
    {
        // Note: Index can be null, Columns can be empty
        public IndexValue Index { get; set; }
        public IList<DbObject> Columns { get; set; }

        public ForceSeekTableHint(IndexValue index, IList<DbObject> columns)
        {
            Index = index;
            Columns = columns;
        }
    }

    public class SpatialWindowMaxCellsTableHint : TableHint
    {
        public int CellCount { get; set; }

        public SpatialWindowMaxCellsTableHint(int cellCount)
        {
            CellCount = cellCount;
        }
    }
    #endregion

    #region ComputeClause
    public class ComputeClause : GrammarNode
    {
        public Expression Argument { get; set; }
        public IList<ComputeFunction> Functions { get; set; }
        public IList<Expression> ByExpressions { get; set; }

        public ComputeClause(IList<ComputeFunction> functions, IList<Expression> byExpressions)
        {
            Functions = functions;
            ByExpressions = byExpressions;
        }
    }

    public class ComputeFunction : GrammarNode
    {
        public ComputeFunctionType Type { get; set; }
        public Expression Argument { get; set; }

        public ComputeFunction(ComputeFunctionType type, Expression argument)
        {
            Type = type;
            Argument = argument;
        }
    }

    public enum ComputeFunctionType
    {
        Avg, Count, Max, Min, StDev, StDevP, Var, VarP, Sum
    }
    #endregion

    #region ForClauseType
    public enum ForClauseType
    {
        None, ForBrowse
    }
    #endregion

    #region OrderByItem
    public class OrderByItem : GrammarNode
    {
        public Expression Expression { get; set; }
        public OrderDirection Direction { get; set; }

        public OrderByItem(Expression expression, OrderDirection direction)
        {
            Expression = expression;
            Direction = direction;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Expression);
            switch (Direction)
            {
                case OrderDirection.Ascending:
                    asm.AddSpace();
                    asm.AddToken("ASC");
                    break;
                case OrderDirection.Descending:
                    asm.AddSpace();
                    asm.AddToken("DESC");
                    break;
            }
            asm.Begin(this);
        }
    }
    #endregion

    #region GroupByItem, GroupingSet, GroupingSetItem, CompositeElement
    abstract public class GroupByItem : GrammarNode
    {
    }

    abstract public class GroupingSet : GrammarNode
    {
    }

    public class GroupingSetGrandTotal : GroupingSet
    {
        public GroupingSetGrandTotal()
        {
            // nothing to do here
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("()");
            asm.End(this);
        }
    }

    public class GroupingSetItemList : GroupingSet
    {
        public IList<GroupingSetItem> SetItemList { get; set; }

        public GroupingSetItemList(IList<GroupingSetItem> list)
        {
            SetItemList = list;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("(");
            foreach (GroupingSetItem item in SetItemList)
            {
                asm.Add(item);
                if (item != SetItemList.Last())
                {
                    asm.AddToken(", ");
                }
            }
            asm.AddToken(")");
            asm.End(this);
        }
    }

    abstract public class GroupingSetItem : GroupingSet
    {
    }

    public class ExpressionGroupingSetItem : GroupingSetItem
    {
        public Expression Expression { get; set; }
        public ExpressionGroupingSetItem(Expression ex)
        {
            Expression = ex;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Add(Expression);
        }
    }

    public class GroupingSpec : GroupByItem
    {
        public IList<CompositeElement> CompositeElements { get; set; }
        public RollupOrCube RollupOrCube { get; set; }

        public GroupingSpec(RollupOrCube roc, IList<CompositeElement> compositeElements)
        {
            RollupOrCube = roc;
            CompositeElements = compositeElements;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken(RollupOrCube == RollupOrCube.ROLLUP ? "ROLLUP" : "CUBE");
            asm.AddToken("(");
            foreach(CompositeElement elem in CompositeElements)
            {
                asm.Add(elem);
                if (elem != CompositeElements.Last())
                {
                    asm.AddToken(", ");
                }
            }
            asm.AddToken(")");
            asm.End(this);
        }
    }

    public enum RollupOrCube
    {
        ROLLUP, CUBE
    }

    public class ExpressionGroupBy : GroupByItem
    {
        public Expression Expression { get; set; }

        public ExpressionGroupBy(Expression ex)
        {
            Expression = ex;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Expression);
            asm.End(this);
        }
    }

    public class GroupingSpecGroupingSetItem : GroupingSetItem
    {
        public GroupingSpec GroupingSpec { get; set; }

        public GroupingSpecGroupingSetItem(GroupingSpec groupingSpec)
        {
            GroupingSpec = groupingSpec;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(GroupingSpec);
            asm.End(this);
        }
    }

    public class GroupingSetSpec : GroupByItem
    {
        public IList<GroupingSet> Sets { get; set; }

        public GroupingSetSpec(IList<GroupingSet> sets)
        {
            Sets = sets;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("GROUPING SETS");
            asm.AddSpace();
            asm.AddToken("(");
            foreach (GroupingSet set in Sets)
            {
                asm.Add(set);
                if (set != Sets.Last())
                {
                    asm.AddToken(", ");
                }
            }
            asm.AddToken(")");
            asm.End(this);
        }
    }

    public class GrandTotal : GroupByItem
    {
        public GrandTotal()
        {
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("()");
            asm.End(this);
        }
    }

    abstract public class CompositeElement : GrammarNode
    {
    }

    public class CompositeElementList : CompositeElement
    {
        public IList<CompositeElement> List { get; set; }

        public CompositeElementList(IList<CompositeElement> list)
        {
            List = list;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("(");
            foreach (CompositeElement elem in List)
            {
                asm.Add(elem);
                if (elem != List.Last())
                {
                    asm.AddToken(", ");
                }
            }
            asm.AddToken(")");
            asm.End(this);
        }
    }

    public class ExpressionCompositeElement : CompositeElement
    {
        public Expression Expression { get; set; }

        public ExpressionCompositeElement(Expression ex)
        {
            Expression = ex;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Expression);
            asm.End(this);
        }
    }
    #endregion

    #region QueryHint
    abstract public class QueryHint : GrammarNode
    {
    }

    public class SimpleQueryHint : QueryHint
    {
        public SimpleQueryHintType Type { get; set; }

        public SimpleQueryHint(SimpleQueryHintType type)
        {
            Type = type;
        }
    }

    public enum SimpleQueryHintType
    {
        HashGroup, OrderGroup, ConcatUnion, HashUnion, MergeUnion, LoopJoin, MergeJoin, HashJoin,
        ExpandViews, ForceOrder, IgnoreNonclusteredColumnstoreIndex, KeepPlan, KeepFixedPlan,
        OptimizeForUnknown, ParametrizationSimple, ParametrizationForced, Recompile, RobustPlan
    }

    public class FastQueryHint : QueryHint
    {
        public int RowCount { get; set; }

        public FastQueryHint(int rowCount)
        {
            RowCount = rowCount;
        }
    }

    public class MaxDOPQueryHint : QueryHint
    {
        public int DegreeOfParallelism { get; set; }

        public MaxDOPQueryHint(int degreeOfParallelism)
        {
            DegreeOfParallelism = degreeOfParallelism;
        }
    }

    public class MaxRecursionQueryHint : QueryHint
    {
        public int RecursionCount { get; set; }

        public MaxRecursionQueryHint(int recursionCount)
        {
            RecursionCount = recursionCount;
        }
    }

    public class OptimizeForQueryHint : QueryHint
    {
        public IList<OptimizeForVariable> Variables{ get; set; }

        public OptimizeForQueryHint(IList<OptimizeForVariable> variables)
        {
            Variables = variables;
        }
    }

    abstract public class OptimizeForVariable : GrammarNode
    {
        public string Variable { get; set; }

        public OptimizeForVariable(string variable)
        {
            Variable = variable;
        }
    }

    public class UnknownOptimizeForVariable : OptimizeForVariable
    {
        public UnknownOptimizeForVariable(string variable) : base(variable)
        {
        }
    }

    public class ConstantOptimizeForVariable : OptimizeForVariable
    {
        public ConstantExpression Value { get; set; }

        public ConstantOptimizeForVariable(string variable, ConstantExpression value)
            : base(variable)
        {
            Value = value;
        }
    }

    public class UsePlanQueryHint : QueryHint
    {
        public StringLiteral Plan { get; set; }

        public UsePlanQueryHint(StringLiteral plan)
        {
            Plan = plan;
        }
    }

    public class TableHintsQueryHint : QueryHint
    {
        public DbObject Table { get; set; }
        public IList<TableHint> TableHints { get; set; }

        public TableHintsQueryHint(DbObject table, IList<TableHint> tableHints)
        {
            Table = table;
            TableHints = tableHints;
        }
    }
    #endregion

    #region UpdateStatistics

    public enum StatisticRangeType
    {
        None, ALL, COLUMNS, INDEX
    }

    public enum StatisticsOptionType
    {
        FULLSCAN, SAMPLEPERCENT, SAMPLEROWS, RESAMPLE
    }

    public class StatisticsWithOption : GrammarNode
    {
        public StatisticsOptionType OptionType { get; set; }
        public int?                 Value { get; set; }

        public StatisticsWithOption(StatisticsOptionType optionType, int? val)
        {
            OptionType = optionType;
            Value = val;
        }
    }

    public class UpdateStatisticStatement : Statement
    {
        public DbObject                     DBObject { get; set; }
        public List<Identifier>             Identifiers { get; set; }
        public WithUpdateStatisticClause    WithClause { get; set; }

        public UpdateStatisticStatement( DbObject dbObject, List<Identifier> identifiers, WithUpdateStatisticClause statisticWithClause)
        {
            DBObject = dbObject;
            Identifiers = identifiers;
            WithClause = statisticWithClause;
        }
    }

    public class WithUpdateStatisticClause : GrammarNode
    {
        public StatisticsWithOption WithOption { get; set; }
        public StatisticRangeType?  RangeType { get; set; }
        public bool                 NoRecompute { get; set; }

        public WithUpdateStatisticClause(StatisticsWithOption withOption, StatisticRangeType? rangeType,  bool noRecompute)
        {
            NoRecompute = noRecompute;
            RangeType = rangeType;
            WithOption = withOption;
        }
    }

    #endregion // UpdateStatistics

    // Dummy statement that is at the beginning of every SQL file. It is used to
    // attach comments at the beginning of the file (before any real statements).
    public class SqlStartStatement : Statement
    {
        public SqlStartStatement()
        {
        }

        public override void Assembly(Assembler asm)
        {
        }
    }
}
