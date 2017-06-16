using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    abstract public class Expression : GrammarNode
    {

    }

    #region ExistsExpression
    public class ExistsExpression : Expression
    {
        public QueryExpression Query { get; set; }

        public ExistsExpression(QueryExpression query)
        {
            Query = query;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("EXISTS");
            asm.AddSpace();
            asm.AddToken("(");
            asm.Add(Query);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region SearchExpression
    public class SearchExpression : Expression
    {
        public SearchOperatorType Type { get; set; }
        public SearchExpressionTarget Target { get; set; }
        public StringLiteral Condition { get; set; }
        public SearchExpressionLanguage Language { get; set; }

        public SearchExpression(SearchOperatorType type, SearchExpressionTarget target, StringLiteral condition, SearchExpressionLanguage language)
        {
            Type = type;
            Target = target;
            Condition = condition;
            Language = language;
        }
    }

    public enum SearchOperatorType
    {
        Contains, FreeText
    }

    abstract public class SearchExpressionTarget : GrammarNode
    {
    }

    public class SearchExpressionColumnTarget : SearchExpressionTarget
    {
        public IList<DbObject> Columns { get; set; }

        public SearchExpressionColumnTarget(IList<DbObject> columns)
        {
            Columns = columns;
        }
    }

    public class SearchExpressionWildcardTarget : SearchExpressionTarget
    {
        public SearchExpressionWildcardTarget()
        {
        }
    }

    abstract public class SearchExpressionLanguage : GrammarNode
    {
    }

    public class SearchExpressionStringLanguage : SearchExpressionLanguage
    {
        public StringLiteral Language { get; set; }

        public SearchExpressionStringLanguage(StringLiteral language)
        {
            Language = language;
        }
    }

    public class SearchExpressionIntLanguage : SearchExpressionLanguage
    {
        public int Language { get; set; }

        public SearchExpressionIntLanguage(int language)
        {
            Language = language;
        }
    }
    #endregion

    #region AllAnyExpression
    public class AllAnyExpression : Expression
    {
        public Expression Target { get; set; }
        public ComparisonOperatorType ComparisonOperator { get; set; }
        public AllAnyOperatorType AllAnyOperator { get; set; }
        public QueryExpression Query { get; set; }

        public AllAnyExpression(Expression target, ComparisonOperatorType comparisonOperator, AllAnyOperatorType allAnyOperator, QueryExpression query)
        {
            Target = target;
            ComparisonOperator = comparisonOperator;
            AllAnyOperator = allAnyOperator;
            Query = query;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);

            asm.Add(Target);
            asm.Add(ComparisonExpression.OperatorToStringifier(ComparisonOperator));

            asm.AddToken(AllAnyOperator == AllAnyOperatorType.All ? "ALL" : "ANY");
            asm.AddSpace();
            asm.AddToken("(");

            if (Query != null)
            {
                asm.Add(Query);
            }
            else
            {
                asm.Add(string.Empty);
            }

            asm.AddToken(")");

            asm.End(this);
        }
    }

    public enum AllAnyOperatorType
    {
        All, Any
    }
    #endregion

    #region BetweenExpression
    public class BetweenExpression : Expression
    {
        public Expression Target { get; set; }
        public bool IsNot { get; set; }
        public Expression From { get; set; }
        public Expression To { get; set; }

        public BetweenExpression(Expression target, bool isNot, Expression from, Expression to)
        {
            Target = target;
            IsNot = isNot;
            From = from;
            To = to;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (IsNot)
            {
                asm.Add(Target);
                asm.AddSpace();
                asm.AddToken("NOT BETWEEN");
                asm.AddSpace();
                asm.Add(From);
                asm.AddSpace();
                asm.AddToken("AND");
                asm.AddSpace();
                asm.Add(To);
            }
            else
            {
                asm.Add(Target);
                asm.AddSpace();
                asm.AddToken("BETWEEN");
                asm.AddSpace();
                asm.Add(From);
                asm.AddSpace();
                asm.AddToken("AND");
                asm.AddSpace();
                asm.Add(To);
            }
            asm.End(this);
        }
    }
    #endregion

    #region InSubqueryExpression
    public class InSubqueryExpression : Expression
    {
        public Expression Target { get; set; }
        public bool IsNot { get; set; }
        public QueryExpression Query { get; set; }

        public InSubqueryExpression(Expression target, bool isNot, QueryExpression query)
        {
            Target = target;
            IsNot = isNot;
            Query = query;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Target);
            if (IsNot)
            {
                asm.AddSpace();
                asm.AddToken("NOT");
            }
            asm.AddSpace();
            asm.AddToken("IN");
            asm.AddSpace();
            asm.AddToken("(");
            if (Query != null)
            {
                asm.Add(Query);
            }
            else
            {
                asm.Add(string.Empty);
            }
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region InListExpression
    public class InListExpression : Expression
    {
        public Expression Target { get; set; }
        public bool IsNot { get; set; }
        public IList<Expression> List { get; set; }

        public InListExpression(Expression target, bool isNot, IList<Expression> list)
        {
            Target = target;
            IsNot = isNot;
            List = list;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Target);
            if (IsNot)
            {
                asm.AddSpace();
                asm.AddToken("NOT");
            }
            asm.AddSpace();
            asm.AddToken("IN");
            asm.AddSpace();
            asm.AddToken("(");
            foreach (Expression exp in List)
            {
                asm.Add(exp);
                if (exp != List.Last())
                {
                    asm.AddToken(",");
                }
            }
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region LikeExpression
    public class LikeExpression : Expression
    {
        public Expression Target { get; set; }
        public bool IsNot { get; set; }
        public Expression Filter { get; set; }
        public Expression Escape { get; set; }

        public LikeExpression(Expression target, bool isNot, Expression filter, Expression escape)
        {
            Target = target;
            IsNot = isNot;
            Filter = filter;
            Escape = escape;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Target);
            asm.AddSpace();
            asm.AddToken(IsNot ? "NOT " : string.Empty);
            asm.AddToken("LIKE");
            asm.AddSpace();
            asm.Add(Filter);
            if (Escape != null)
            {
                asm.AddSpace();
                asm.AddToken("ESCAPE");
                asm.AddSpace();
                asm.Add(Escape);
            }
            asm.End(this);
        }
    }
    #endregion

    #region IsNullExpression
    public class IsNullExpression : Expression
    {
        public Expression Target { get; set; }
        public bool IsNot { get; set; }

        public IsNullExpression(Expression target, bool isNot)
        {
            Target = target;
            IsNot = isNot;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Target);
            asm.AddSpace();
            asm.AddToken("IS");
            if (IsNot)
            {
                asm.AddSpace();
                asm.AddToken("NOT");
            }
            asm.AddSpace();
            asm.AddToken("NULL");
            asm.End(this);
        }
    }
    #endregion

    #region BinaryLogicalExpression
    public class BinaryLogicalExpression : Expression
    {
        public Expression LeftExpression { get; set; }
        public BinaryLogicalOperatorType Operator { get; set; }
        public Expression RightExpression { get; set; }

        public BinaryLogicalExpression(Expression left, BinaryLogicalOperatorType op, Expression right)
        {
            LeftExpression = left;
            Operator = op;
            RightExpression = right;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(LeftExpression);
            switch (Operator)
            {
                case BinaryLogicalOperatorType.And:
                    asm.AddSpace();
                    asm.AddToken("AND");
                    asm.Breakable();
                    asm.AddSpace();
                    break;
                case BinaryLogicalOperatorType.Or:
                    asm.AddSpace();
                    asm.AddToken("OR");
                    asm.Breakable();
                    asm.AddSpace();
                    break;
            }
            asm.Add(RightExpression);
            asm.End(this);
        }
    }

    public enum BinaryLogicalOperatorType
    {
        And, Or
    }
    #endregion

    #region LogicalNotExpression
    public class LogicalNotExpression : Expression
    {
        public Expression Expression { get; set; }

        public LogicalNotExpression(Expression expression)
        {
            Expression = expression;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("NOT");
            asm.AddSpace();
            asm.Add(Expression);
            asm.End(this);
        }
    }
    #endregion

    #region ComparisonExpression
    public class ComparisonExpression : Expression
    {
        public Expression LeftExpression { get; set; }
        public ComparisonOperatorType Operator { get; set; }
        public Expression RightExpression { get; set; }

        public ComparisonExpression(Expression left, ComparisonOperatorType op, Expression right)
        {
            LeftExpression = left;
            Operator = op;
            RightExpression = right;
        }

        public static string OperatorToStringifier(ComparisonOperatorType Operator)
        {
            switch (Operator)
            {
                case ComparisonOperatorType.Equal:
                    return " = ";
                case ComparisonOperatorType.GreaterThan:
                    return " > ";
                case ComparisonOperatorType.GreaterThanOrEqual:
                    return " >= ";
                case ComparisonOperatorType.LessThan:
                    return " < ";
                case ComparisonOperatorType.LessThanOrEqual:
                    return " <= ";
                case ComparisonOperatorType.NotEqual:
                    return " <> ";
            }
            return string.Empty;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(LeftExpression);
            asm.Add(OperatorToStringifier(Operator));
            asm.Add(RightExpression);
            asm.End(this);
        }
    }

    public enum ComparisonOperatorType
    {
        Equal, NotEqual, LessThanOrEqual, LessThan, GreaterThanOrEqual, GreaterThan
    }
    #endregion

    #region UnaryAddExpression
    public class UnaryAddExpression : Expression
    {
        public Expression Expression { get; set; }
        public UnaryAddOperatorType Operator { get; set; }

        public UnaryAddExpression(UnaryAddOperatorType op, Expression expression)
        {
            Operator = op;
            Expression = expression;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Operator)
            {
                case UnaryAddOperatorType.Plus:
                    asm.AddToken("+");
                    asm.Add(Expression);
                    break;
                case UnaryAddOperatorType.Minus:
                    asm.AddToken("-");
                    asm.Add(Expression);
                    break;
            }
            asm.End(this);
        }
    }

    public enum UnaryAddOperatorType
    {
        Plus, Minus
    }
    #endregion

    #region BinaryAddExpression
    public class BinaryAddExpression : Expression
    {
        public Expression LeftExpression { get; set; }
        public BinaryAddOperatorType Operator { get; set; }
        public Expression RightExpression { get; set; }

        public BinaryAddExpression(Expression left, BinaryAddOperatorType op, Expression right)
        {
            LeftExpression = left;
            Operator = op;
            RightExpression = right;
        }

        private bool IncludesStringConcat(BinaryAddExpression exp)
        {
            if (exp == null)
            {
                return false;
            }

            if (exp.LeftExpression is HANAConcatExpression || exp.RightExpression is HANAConcatExpression)
            {
                if ((exp.LeftExpression is IntegerConstantExpression && exp.RightExpression is HANAConcatExpression) ||
                    (exp.RightExpression is IntegerConstantExpression && exp.LeftExpression is HANAConcatExpression))
                {
                    return false;
                }

                return true;
            }

            if (exp.LeftExpression is BinaryAddExpression && exp.RightExpression is BinaryAddExpression)
            {
                return IncludesStringConcat((BinaryAddExpression)exp.LeftExpression) || IncludesStringConcat((BinaryAddExpression)exp.RightExpression);
            }

            return false;
        }

        override public void Assembly(Assembler asm) 
        {
            asm.Begin(this);
            asm.Add(LeftExpression);
            asm.AddSpace();

            if (Operator == BinaryAddOperatorType.Minus)
            {
                asm.AddToken("-");
            }
            else
            {
                if (asm.IsFlag(Assembler.FLAG_HANA_CONCAT) || IncludesStringConcat(this))
                {
                    asm.AddToken("||");
                }
                else
                {
                    asm.AddToken("+");
                }
            }

            asm.Breakable();
            asm.AddSpace();
            asm.Add(RightExpression);
            asm.End(this);
        }
    }

    public enum BinaryAddOperatorType
    {
        Plus, Minus
    }
    #endregion

    #region BinaryBitwiseExpression
    public class BinaryBitwiseExpression : Expression
    {
        public Expression LeftExpression { get; set; }
        public BinaryBitwiseOperatorType Operator { get; set; }
        public Expression RightExpression { get; set; }

        public BinaryBitwiseExpression(Expression left, BinaryBitwiseOperatorType op, Expression right)
        {
            LeftExpression = left;
            Operator = op;
            RightExpression = right;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Operator)
            {
                case BinaryBitwiseOperatorType.Or:
                    AddNote(Note.STRINGIFIER, ResStr.NO_BITWISE_OR_OPERATOR);
                    asm.End(this);
                    return;
                case BinaryBitwiseOperatorType.Xor:
                    AddNote(Note.STRINGIFIER, ResStr.NO_BITWISE_XOR_OPERATOR);
                    asm.End(this);
                    return;
            }

            switch (Operator)
            {
                case BinaryBitwiseOperatorType.And:
                    asm.AddToken("BITAND(");
                    asm.Add(LeftExpression);
                    asm.AddToken(",");
                    asm.Add(RightExpression);
                    asm.AddToken(")");
                    break;
            }
            asm.End(this);
        }
    }

    public enum BinaryBitwiseOperatorType
    {
        And, Or, Xor
    }
    #endregion

    #region MultiplyExpression
    public class MultiplyExpression : Expression
    {
        public Expression LeftExpression { get; set; }
        public MultiplyOperatorType Operator { get; set; }
        public Expression RightExpression { get; set; }

        public MultiplyExpression(Expression left, MultiplyOperatorType op, Expression right)
        {
            LeftExpression = left;
            Operator = op;
            RightExpression = right;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(LeftExpression);
            switch (Operator)
            {
                case MultiplyOperatorType.Divide:
                    asm.AddSpace();
                    asm.AddToken("/");
                    asm.AddSpace();
                    break;
                case MultiplyOperatorType.Modulo:
                    asm.AddSpace();
                    asm.AddToken("%");
                    asm.AddSpace();
                    break;
                case MultiplyOperatorType.Multiply:
                    asm.AddSpace();
                    asm.AddToken("*");
                    asm.AddSpace();
                    break;
            }
            asm.Add(RightExpression);
            asm.End(this);
        }
    }

    public enum MultiplyOperatorType
    {
        Multiply, Divide, Modulo
    }
    #endregion

    #region BitwiseNotExpression
    public class BitwiseNotExpression : Expression
    {
        public Expression Expression { get; set; }

        public BitwiseNotExpression(Expression expression)
        {
            Expression = expression;
        }
    }
    #endregion

    public class CollationExpression : Expression
    {
        public Expression Expression { get; set; }
        public Identifier Collation { get; set; }

        public CollationExpression(Expression expression, Identifier collation)
        {
            Expression = expression;
            Collation = collation;
        }
    }

    public class VariableExpression : Expression
    {
        public String Value { get; set; }
        public bool IsArgument = true;

        public VariableExpression(String value)
        {
            Value = value;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (IsArgument)
                asm.AddToken(":");
            asm.Add(Value);
            asm.End(this);
        }
    }

    #region DbObjectExpression
    public class DbObjectExpression : Expression
    {
        public DbObject Value { get; set; }

        public DbObjectExpression(DbObject value)
        {
            Value = value;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Value);
            asm.End(this);
        }
    }
    #endregion

    #region SubqueryExpression
    public class SubqueryExpression : Expression
    {
        public SelectStatement Query { get; set; }

        public SubqueryExpression(SelectStatement query)
        {
            Query = query;
            Query.Terminate = false;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.NewLine();
            asm.AddToken("(");
            asm.Add(Query);
            asm.AddToken(")");
            asm.End(this);
        }

    }
    #endregion

    public class ParensExpression : Expression
    {
        public Expression Expression { get; set; }

        public ParensExpression(Expression expression)
        {
            Expression = expression;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("(");
            asm.Add(Expression);
            asm.AddToken(")");
            asm.End(this);
        }
    }

    abstract public class ConstantExpression : Expression
    {
    }

    public class DefaultExpression : Expression
    {
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add("DEFAULT");
            asm.End(this);
        }
    }

    public class StringConstantExpression : ConstantExpression
    {
        public StringLiteral Value { get; set; }

        public StringConstantExpression(StringLiteral value)
        {
            Value = value;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Value);
            asm.End(this);
        }
    }

    #region HexConstantExpression
    public class HexConstantExpression : ConstantExpression
    {
        public string Value { get; set; }

        public HexConstantExpression(string value)
        {
            Value = value;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Value);
            asm.End(this);
        }
    }
    #endregion

    #region DateTimeConstantExpression
    public class DateTimeConstantExpression : ConstantExpression
    {
        // TODO better would be to store a DateTime type value instead of string
        public string Value { get; set; }

        public DateTimeConstantExpression(string value)
        {
            Value = value;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("'");
            asm.Add(Value);
            asm.AddToken("'");
            asm.End(this);
        }
    }
    #endregion

    #region IntegerConstantExpression
    public class IntegerConstantExpression : ConstantExpression
    {
        public int Value { get; set; }

        public IntegerConstantExpression(int value)
        {
            Value = value;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Value.ToString());
            asm.End(this);
        }
    }
    #endregion

    public class DecimalConstantExpression : ConstantExpression
    {
        public decimal Value { get; set; }

        public DecimalConstantExpression(decimal value)
        {
            Value = value;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Value.ToString());
            asm.End(this);
        }
    }

    #region RealConstantExpression
    public class RealConstantExpression : ConstantExpression
    {
        public RealLiteral Value { get; set; }

        public RealConstantExpression(RealLiteral value)
        {
            Value = value;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Value);
            asm.End(this);
        }
    }
    #endregion

    public class MoneyConstantExpression : ConstantExpression
    {
        public MoneyLiteral Value { get; set; }

        public MoneyConstantExpression(MoneyLiteral value)
        {
            Value = value;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Value);
            asm.End(this);
        }
    }

    #region NullConstantExpression
    public class NullConstantExpression : ConstantExpression
    {
        public NullConstantExpression()
        {
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("NULL");
            asm.End(this);
        }
    }

    #endregion
    
    #region DateFormatExpression
    public class DateFormatExpression : Expression
    {
        public StringConstantExpression OriginalDate { get; set; }
        public DateFormatExpressionType Operator { get; set; }

        public DateFormatExpression(StringConstantExpression originalDate, DateFormatExpressionType dateFormatOperator)
        {
            OriginalDate = originalDate;
            Operator = dateFormatOperator;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            StringLiteral dateToken = new StringLiteral(StringLiteralType.ASCII, string.Empty);
            StringLiteral zeroLiteral = new StringLiteral(StringLiteralType.ASCII, "0");
            //if OriginalDate contains / separator, the original separator  will be replaced by - 
            //if OriginalDate is 8digit format without separator, separator will be added
            string[] date = null;
            string timeStamp = null;
            if (OriginalDate != null)
            {
                string origDate = OriginalDate.Value.String.Trim(' ');
                string[] ts = origDate.Split(' ');

                if (ts.Length == 2)
                {
                    //also we have time stamp part
                    timeStamp = ts[1];
                }
                date = ts[0].Split('/');

                if (date.Length == 1 && date[0].Length == 8 && !date[0].Contains("."))
                {
                    date = new string[3];
                    date[2] = origDate.Substring(0, 4);
                    date[1] = origDate.Substring(4, 2);
                    date[0] = origDate.Substring(6, 2);
                }
            }
            if (date != null)
            {
                switch (Operator)
                {
                    case DateFormatExpressionType.WithoutSeparator:
                        for (int i = date.Length - 1; i >= 0; i--)
                        {
                            dateToken.String += (date[i].Length > 1 ? date[i] : zeroLiteral.String + date[i]);
                        }
                        break;
                    case DateFormatExpressionType.WithSeparator:
                        for (int i = date.Length - 1; i >= 0; i--)
                        {
                            if (i >= 1)
                            {
                                dateToken.String += date[i] + "-";
                            }
                            else
                            {
                                dateToken.String += date[i];
                            }
                        }

                        if (timeStamp != null)
                        {
                            dateToken.String += " " + timeStamp;
                        }
                        break;
                }
            }
            dateToken.Assembly(asm);
            asm.End(this);
        }
    }

    public enum DateFormatExpressionType
    {
        WithoutSeparator, WithSeparator
    }
    #endregion

    #region BuiltinVariableExpression
    public class BuiltinVariableExpression : Expression
    {
        public BuiltinVariableType Type { get; set; }

        public BuiltinVariableExpression(BuiltinVariableType type)
        {
            Type = type;
        }

        override public void Assembly(Assembler asm)
        {
            //TODO
            asm.Begin(this);
            asm.Add(Type.ToString());
            asm.End(this);
        }
    }

    public enum BuiltinVariableType
    {
        // Configuration Functions
        DATEFIRST,
        DBTS,
        LANGID,
        LANGUAGE,
        LOCK_TIMEOUT,
        MAX_CONNECTIONS,
        MAX_PRECISION,
        NESTLEVEL,
        OPTIONS,
        REMSERVER,
        SERVERNAME,
        SERVICENAME,
        SPID,
        TEXTSIZE,
        VERSION,
        // Cursor Functions
        CURSOR_ROWS,
        FETCH_STATUS,
        // Metadata Functions
        PROCID,
        // System Functions
        ERROR,
        IDENTITY,
        ROWCOUNT,
        TRANCOUNT,
        // System Statistical Functions
        CONNECTIONS,
        CPU_BUSY,
        IDLE,
        IO_BUSY,
        PACKET_ERRORS,
        PACK_RECEIVED,
        PACK_SENT,
        TIMETICKS,
        TOTAL_ERRORS,
        TOTAL_READ,
        TOTAL_WRITE
    }
    #endregion

    #region GenericScalarFunctionExpression
    public class GenericScalarFunctionExpression : Expression
    {
        public Identifier Name { get; set; }
        public IList<Expression> Arguments { get; set; }

        public GenericScalarFunctionExpression(Identifier name, IList<Expression> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Name);
            asm.AddToken("(");
            if (Arguments != null)
            {
                foreach (Expression exp in Arguments)
                {
                    asm.Add(exp);
                    if (exp != Arguments.Last())
                    {
                        asm.AddToken(", ");
                    }
                }
            }
            asm.AddToken(")");
            asm.End(this);
        }

        /// <summary>
        /// Returns true when function returns string.
        /// </summary>
        /// <returns></returns>
        public bool ReturnsString()
        {
            string name = Name.Name.ToUpper();
            bool ret = false;

            switch(name)
            {
                case "RTRIM":
                case "LTRIM":
                case "CHAR":
                case "NCHAR":
                case "UPPER":
                case "LOWER":
                case "LEFT":
                case "RIGHT":
                case "CONCAT":
                case "SUBSTRING":
                case "SPACE":
                    ret = true;
                    break;
            }

            return ret;
        }
    }
    #endregion
    
    #region ParameterlessFunctionExpression
    public class ParameterlessFunctionExpression : Expression
    {
        public ParameterlessFunctionType Type { get; set; }

        public ParameterlessFunctionExpression(ParameterlessFunctionType type)
        {
            Type = type;
        }
        
        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Type)
            {
                case ParameterlessFunctionType.CurrentTimestamp:
                    asm.AddToken("CURRENT_TIMESTAMP");
                    break;
                case ParameterlessFunctionType.CurrentUser:
                    asm.AddToken("CURRENT_USER");
                    break;
                case ParameterlessFunctionType.SessionUser:
                    // TODO
                    break;
                case ParameterlessFunctionType.SystemUser:
                    // TODO
                    break;
                case ParameterlessFunctionType.MinActiveRowVersion:
                    // NOT SUPPORTED
                    break;
                case ParameterlessFunctionType.HANACurrentUser:
                    asm.AddToken("CURRENT_USER");
                    break;
                case ParameterlessFunctionType.HANACurrentUTCTimeStamp:
                    asm.AddToken("CURRENT_UTCTIMESTAMP");
                    break;
                case ParameterlessFunctionType.HANACurrentSchema:
                    asm.AddToken("CURRENT_SCHEMA");
                    break;
            }
            asm.End(this);
        }
    }

    public enum ParameterlessFunctionType
    {
        CurrentTimestamp, CurrentUser, SessionUser, SystemUser, MinActiveRowVersion, HANACurrentUser, HANACurrentUTCTimeStamp, HANACurrentSchema
    }
    #endregion

    #region CastFunctionExpression
    public class CastFunctionExpression : Expression
    {
        public Expression Target { get; set; }
        public DataType Type { get; set; }

        public CastFunctionExpression(Expression target, DataType type)
        {
            Target = target;
            Type = type;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("CAST(");
            asm.Add(Target);
            asm.AddSpace();
            asm.AddToken("AS");
            asm.AddSpace();
            asm.Add(Type);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region ConvertFunctionExpression
    public class ConvertFunctionExpression : Expression
    {
        public bool TryConvert { get; set; }
        public DataType Type { get; set; }
        public Expression Target { get; set; }
        public int Style { get; set; }

        public ConvertFunctionExpression(bool tryConvert, DataType type, Expression target, int style)
        {
            TryConvert = tryConvert;
            Type = type;
            Target = target;
            Style = style;
        }
    }
    #endregion

    #region ParseFunctionExpression
    public class ParseFunctionExpression : Expression
    {
        public bool TryParse { get; set; }
        public Expression Target { get; set; }
        public Identifier Type { get; set; }
        public Expression Culture { get; set; }

        public ParseFunctionExpression(bool tryParse, Expression target, Identifier type, Expression culture)
        {
            TryParse = tryParse;
            Target = target;
            Type = type;
            Culture = culture;
        }
    }
    #endregion

    #region DatenameFunctionExpression
    public class DatenameFunctionExpression : Expression
    {
        public Identifier Part { get; set; }
        public Expression Target { get; set; }

        public DatenameFunctionExpression(Identifier part, Expression target)
        {
            Part = part;
            Target = target;
        }
    }
    #endregion

    #region DatepartFunctionExpression
    public class DatepartFunctionExpression : Expression
    {
        public Identifier Part { get; set; }
        public Expression Target { get; set; }

        public DatepartFunctionExpression(Identifier part, Expression target)
        {
            Part = part;
            Target = target;
        }
    }
    #endregion

    #region YearFunctionExpression
    public class YearFunctionExpression : Expression
    {
        public Expression Target { get; set; }

        public YearFunctionExpression(Expression target)
        {
            Target = target;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("YEAR(");
            asm.Add(Target);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region ToCharFunctionExpression
    public class ToCharFunctionExpression : Expression
    {
        public Expression Target { get; set; }
        public Identifier Part { get; set; }

        public ToCharFunctionExpression(Expression target, Identifier part)
        {
            Target = target;
            Part = part;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("TO_CHAR(");
            asm.Add(Target);
            asm.AddToken(",");
            asm.AddSpace();
            asm.AddToken("'");
            asm.Add(Part);
            asm.AddToken("')");
            asm.End(this);
        }
    }
    #endregion

    #region DayOfYearFunctionExpression
    public class DayOfYearFunctionExpression : Expression
    {
        public Expression Target { get; set; }

        public DayOfYearFunctionExpression(Expression target)
        {
            Target = target;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("DAYOFYEAR(");
            asm.Add(Target);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region WeekDayFunctionExpression
    public class WeekDayFunctionExpression : Expression
    {
        public Expression Target { get; set; }

        public WeekDayFunctionExpression(Expression target)
        {
            Target = target;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("WEEKDAY(");
            asm.Add(Target);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region HourFunctionExpression
    public class HourFunctionExpression : Expression
    {
        public Expression Target { get; set; }

        public HourFunctionExpression(Expression target)
        {
            Target = target;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("HOUR(");
            asm.Add(Target);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region MinuteFunctionExpression
    public class MinuteFunctionExpression : Expression
    {
        public Expression Target { get; set; }

        public MinuteFunctionExpression(Expression target)
        {
            Target = target;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("MINUTE(");
            asm.Add(Target);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region SecondFunctionExpression
    public class SecondFunctionExpression : Expression
    {
        public Expression Target { get; set; }

        public SecondFunctionExpression(Expression target)
        {
            Target = target;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("SECOND(");
            asm.Add(Target);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region MonthFunctionExpression
    public class MonthFunctionExpression : Expression
    {
        public Expression Target { get; set; }

        public MonthFunctionExpression(Expression target)
        {
            Target = target;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("MONTH(");
            asm.Add(Target);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region WeekFunctionExpression
    public class WeekFunctionExpression : Expression
    {
        public Expression Target { get; set; }

        public WeekFunctionExpression(Expression target)
        {
            Target = target;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("WEEK(");
            asm.Add(Target);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region DayOfMonthFunctionExpression
    public class DayOfMonthFunctionExpression : Expression
    {
        public Expression Target { get; set; }

        public DayOfMonthFunctionExpression(Expression target)
        {
            Target = target;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("DAYOFMONTH(");
            asm.Add(Target);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region NextValueFunctionExpression
    // TODO add support for OVER clause
    public class NextValueFunctionExpression : Expression
    {
        public DbObject Sequence { get; set; }
        public OverClause OverClause { get; set; }

        public NextValueFunctionExpression(DbObject sequence, OverClause overClause)
        {
            Sequence = sequence;
            OverClause = overClause;
        }

        override public void Assembly(Assembler asm)
        {
            // OverClause, NOT SUPPORTED
            asm.Add(Sequence);
            asm.Add(".NEXTVAL");
        }

    }
    #endregion

    #region ChecksumFunctionExpression
    public class ChecksumFunctionExpression : Expression
    {
        // A small hack: If Expressions contains no elements, it is treated like CHECKSUM(*)
        // (as if there was a single asterisk argument).
        public bool IsBinary { get; set; }
        public IList<Expression> Expressions { get; set; }

        public ChecksumFunctionExpression(bool isBinary, IList<Expression> expressions)
        {
            IsBinary = isBinary;
            Expressions = expressions;
        }
    }
    #endregion

    #region CaseFunctionExpression, CaseWhenClause
    public class CaseFunctionExpression : Expression
    {
        public Expression Target { get; set; }
        public IList<CaseWhenClause> WhenClauses { get; set; }
        public Expression ElseExpression { get; set; }

        public CaseFunctionExpression(Expression target, IList<CaseWhenClause> whenClauses, Expression elseExpression)
        {
            Target = target;
            WhenClauses = whenClauses;
            ElseExpression = elseExpression;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.NewLine();
            asm.AddToken("CASE");

            if (Target != null)
            {
                asm.AddSpace();
                asm.Add(Target);
            }
            asm.IncreaseIndentation();
            foreach (CaseWhenClause cls in WhenClauses)
            {
                asm.AddSpace();
                asm.NewLine();
                asm.Add(cls);
            }
            if (ElseExpression != null)
            {
                asm.AddSpace();
                asm.NewLine();
                asm.AddToken("ELSE");
                asm.AddSpace();
                asm.Add(ElseExpression);
            }

            asm.AddSpace();
            asm.DecreaseIndentation();
            asm.NewLine();
            asm.AddToken("END");
            asm.End(this);
        }
    }
    
    #region IifFunctionExpression
    public class IifFunctionExpression : Expression
    {
        public Expression BooleanExpression { get; set; }
        public Expression TrueExpression { get; set; }
        public Expression FalseExpression { get; set; }

        public IifFunctionExpression(Expression booleanExpression, Expression trueExpression, Expression falseExpression)
        {
            BooleanExpression = booleanExpression;
            TrueExpression = trueExpression;
            FalseExpression = falseExpression;
        }
    }
    #endregion

    #region ChooseFunctionExpression
    public class ChooseFunctionExpression : Expression
    {
        public Expression IndexExpression { get; set; }
        public Expression FirstMandatoryOption { get; set; }
        public IList<Expression> RestOfOptions { get; set; }

        public ChooseFunctionExpression(Expression indexExpression, Expression firstMandatoryOption, IList<Expression> restOfOptions)
        {
            IndexExpression = indexExpression;
            FirstMandatoryOption = firstMandatoryOption;
            RestOfOptions = restOfOptions;
        }
    }
    #endregion

    public class CaseWhenClause : GrammarNode
    {
        public Expression Condition { get; set; }
        public Expression Result { get; set; }

        public CaseWhenClause(Expression condition, Expression result)
        {
            Condition = condition;
            Result = result;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("WHEN");
            asm.AddSpace();
            asm.Add(Condition);
            asm.AddSpace();
            asm.AddToken("THEN");
            asm.AddSpace();
            asm.Add(Result);
            asm.End(this);
        }
    }
    #endregion

    #region IdentityFunctionExpression
    public class IdentityFunctionExpression : Expression
    {
        public DataType DataType { get; set; }
        public int Seed { get; set; }
        public int Increment { get; set; }

        public IdentityFunctionExpression(DataType dataType, int seed, int increment)
        {
            DataType = dataType;
            Seed = seed;
            Increment = increment;
        }

        override public void Assembly(Assembler asm)
        {
            // TODO this can't be correct
            asm.Begin(this);
            if (Seed != 0 || Increment != 0)
            {
                asm.Add(",");
                asm.AddSpace();
                asm.Add(Seed);
                asm.Add(",");
                asm.AddSpace();
                asm.Add(Increment);
            }
            asm.End(this);
        }
    }
    #endregion

    #region RankingFunctionExpression
    public class RankingFunctionExpression : Expression
    {
        public RankingFunctionType Type { get; set; }
        public OverClause OverClause { get; set; }

        public RankingFunctionExpression(RankingFunctionType type, OverClause overClause)
        {
            Type = type;
            OverClause = overClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Type)
            {
                case RankingFunctionType.Rank:
                    asm.AddToken("RANK");
                    break;
                case RankingFunctionType.DenseRank:
                    asm.AddToken("DENSE_RANK");
                    break;
                case RankingFunctionType.RowNumber:
                    asm.AddToken("ROW_NUMBER");
                    break;
            }
            asm.AddToken("()");
            asm.AddSpace();
            if (OverClause != null)
            {
                asm.AddToken("OVER");
                asm.AddSpace();
                asm.AddToken("(");
                this.OverClause.Assembly(asm);
                asm.AddToken(")");
            }
            asm.End(this);
        }
    }

    public enum RankingFunctionType
    {
        Rank, DenseRank, RowNumber
    }
    #endregion

    #region NTileFunctionExpression
    public class NTileFunctionExpression : Expression
    {
        public Expression GroupCount { get; set; }
        public OverClause OverClause { get; set; }

        public NTileFunctionExpression(Expression groupCount, OverClause overClause)
        {
            GroupCount = groupCount;
            OverClause = overClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("NTILE");
            asm.AddToken("(");
            GroupCount.Assembly(asm);
            asm.AddToken(")");
            asm.AddSpace();
            if (OverClause != null)
            {
                asm.AddToken("OVER");
                asm.AddSpace();
                asm.AddToken("(");
                this.OverClause.Assembly(asm);
                asm.AddToken(")");
            }
            asm.End(this);
        }
    }
    #endregion

    #region SimpleAggregateFunctionExpression
    public class SimpleAggregateFunctionExpression : Expression
    {
        public SimpleAggregateFunctionType Type { get; set; }
        public bool IsDistinct { get; set; }
        public Expression Target { get; set; }
        public OverClause OverClause { get; set; }

        public SimpleAggregateFunctionExpression(SimpleAggregateFunctionType type, bool isDistinct, Expression target, OverClause overClause)
        {
            Type = type;
            IsDistinct = isDistinct;
            Target = target;
            OverClause = overClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Type)
            {
                case SimpleAggregateFunctionType.Avg:
                    asm.AddToken("AVG");
                    break;
                case SimpleAggregateFunctionType.ChecksumAgg:
                    asm.AddToken("CHECKSUM_AGG");
                    return;
                case SimpleAggregateFunctionType.Max:
                    asm.AddToken("MAX");
                    break;
                case SimpleAggregateFunctionType.Min:
                    asm.AddToken("MIN");
                    break;
                case SimpleAggregateFunctionType.Sum:
                    asm.AddToken("SUM");
                    break;
                case SimpleAggregateFunctionType.StDev:
                    asm.AddToken("STDEV");
                    break;
                case SimpleAggregateFunctionType.HANAStDev:
                    asm.AddToken("STDDEV");
                    break;
                case SimpleAggregateFunctionType.StDevP:
                    asm.AddToken("STDEVP");
                    return;
                case SimpleAggregateFunctionType.Var:
                    asm.AddToken("VAR");
                    break;
                case SimpleAggregateFunctionType.VarP:
                    asm.AddToken("VARP");
                    return;
            }
            asm.AddToken("(");
            if (IsDistinct)
            {
                asm.AddToken("DISTINCT");
                asm.AddSpace();
            }
            asm.Add(Target);
            asm.AddToken(")");
            if (this.OverClause != null)
            {
                asm.AddToken("OVER");
                asm.AddSpace();
                asm.AddToken("(");
                this.OverClause.Assembly(asm);
                asm.AddToken(")");
            }
            asm.End(this);
        }
    }

    public enum SimpleAggregateFunctionType
    {
        Avg, ChecksumAgg, Max, Min, Sum, StDev, StDevP, Var, VarP, HANAStDev
    }
    #endregion

    #region ExpressionCountFunctionExpression
    public class ExpressionCountFunctionExpression : Expression
    {
        public CountFunctionType Type { get; set; }
        public bool IsDistinct { get; set; }
        public Expression Target { get; set; }
        public OverClause OverClause { get; set; }

        public ExpressionCountFunctionExpression(CountFunctionType type, bool isDistinct, Expression target, OverClause overClause)
        {
            Type = type;
            IsDistinct = isDistinct;
            Target = target;
            OverClause = overClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("COUNT");
            asm.AddToken("(");
            if (IsDistinct)
            {
                asm.AddToken(IsDistinct ? "DISTINCT" : "ALL");
                asm.AddSpace();
            }
            asm.Add(Target);
            asm.AddToken(")");
            asm.End(this);
        }
    }
    #endregion

    #region AsteriskCountFunctionExpression
    public class AsteriskCountFunctionExpression : Expression
    {
        public CountFunctionType Type { get; set; }
        public OverClause OverClause { get; set; }

        public AsteriskCountFunctionExpression(CountFunctionType type, OverClause overClause)
        {
            Type = type;
            OverClause = overClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("COUNT(*)");
            asm.End(this);
        }
    }

    public enum CountFunctionType
    {
        Count, CountBig
    }
    #endregion

    #region CumePercentFunctionExpression
    public class CumePercentFunctionExpression : Expression
    {
        public CumePercentFunctionType Type { get; set; }
        public OverClause OverClause { get; set; }

        public CumePercentFunctionExpression(CumePercentFunctionType type, OverClause overClause)
        {
            Type = type;
            OverClause = overClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Type)
            {
                case CumePercentFunctionType.CumeDist:
                    asm.AddToken("CUME_DIST");
                    break;
                case CumePercentFunctionType.PercentRank:
                    asm.AddToken("PERCENT_RANK");
                    break;
            }
            asm.AddToken("()");
            asm.AddSpace();
            if (OverClause != null)
            {
                asm.AddToken("over");
                asm.AddSpace();
                asm.AddToken("(");
                this.OverClause.Assembly(asm);
                asm.AddToken(")");
            }
            asm.End(this);
        }
    }

    public enum CumePercentFunctionType
    {
        CumeDist, PercentRank
    }
    #endregion

    #region FirstLastValueFunctionExpression
    public class FirstLastValueFunctionExpression : Expression
    {
        public FirstLastValueFunctionType Type { get; set; }
        public Expression Target { get; set; }
        public OverClause OverClause { get; set; }

        public FirstLastValueFunctionExpression(FirstLastValueFunctionType type, Expression target, OverClause overClause)
        {
            Type = type;
            Target = target;
            OverClause = overClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Type)
            {
                case FirstLastValueFunctionType.FirstValue:
                    asm.AddToken("FIRST_VALUE");
                    break;
                case FirstLastValueFunctionType.LastValue:
                    asm.AddToken("LAST_VALUE");
                    break;
            }
            asm.AddToken("(");
            this.Target.Assembly(asm);
            asm.AddToken(")");
            asm.AddSpace();
            if (OverClause != null)
            {
                asm.AddToken("OVER");
                asm.AddSpace();
                asm.AddToken("(");
                this.OverClause.Assembly(asm);
                asm.AddToken(")");
            }
            asm.End(this);
        }
    }

    public enum FirstLastValueFunctionType
    {
        FirstValue, LastValue
    }
    #endregion

    #region LagLeadFunctionExpression
    public class LagLeadFunctionExpression : Expression
    {
        public LagLeadFunctionType Type { get; set; }
        public Expression Target { get; set; }
        public Expression Offset { get; set; }
        public Expression Default { get; set; }
        public OverClause OverClause { get; set; }

        public LagLeadFunctionExpression(LagLeadFunctionType type, Expression target, Expression offset, Expression deflt, OverClause overClause)
        {
            Type = type;
            Target = target;
            Offset = offset;
            Default = deflt;
            OverClause = overClause;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (this.Type)
            {
                case LagLeadFunctionType.Lag:
                    asm.AddToken("LAG");
                    break;
                case LagLeadFunctionType.Lead:
                    asm.AddToken("LEAD");
                    break;
            }
            asm.AddSpace();
            asm.AddToken("(");
            this.Target.Assembly(asm);
            if (this.Offset != null)
            {
                asm.AddToken(",");
                this.Offset.Assembly(asm);
            }
            if (this.Default != null)
            {
                asm.AddToken(",");
                this.Default.Assembly(asm);
            }
            asm.AddToken(")");
            asm.AddSpace();
            if (OverClause != null)
            {
                asm.AddToken("OVER");
                asm.AddSpace();
                asm.AddToken("(");
                this.OverClause.Assembly(asm);
                asm.AddToken(")");
            }
            asm.End(this);
        }
    }

    public enum LagLeadFunctionType
    {
        Lag, Lead
    }
    #endregion

    #region PercentileContDiscFunctionExpression
    public class PercentileContDiscFunctionExpression : Expression
    {
        public PercentileContDiscFunctionType Type { get; set; }
        public Decimal Percentile { get; set; }
        public Expression OrderByExpression { get; set; }
        public OrderDirection OrderByDirection { get; set; }
        public OverClause OverClause { get; set; }

        public PercentileContDiscFunctionExpression(PercentileContDiscFunctionType type, Decimal percentile, 
            Expression orderByExpression, OrderDirection orderByDirection, OverClause overClause)
        {
            Type = type;
            Percentile = percentile;
            OrderByExpression = orderByExpression;
            OrderByDirection = orderByDirection;
            OverClause = overClause;
        }
    }

    public enum PercentileContDiscFunctionType
    {
        PercentileCont, PercentileDisc
    }
    #endregion

    #region OverClause
    public class OverClause : GrammarNode
    {
        public IList<Expression> PartitionByClause { get; set; }
        public IList<OrderByItem> OrderByClause { get; set; }
        public RowsRangeClause RowsRangeClause { get; set; }

        public OverClause(IList<Expression> partitionByClause, IList<OrderByItem> orderByClause, RowsRangeClause rowsRangeClause)
        {
            PartitionByClause = partitionByClause;
            OrderByClause = orderByClause;
            RowsRangeClause = rowsRangeClause;
        }

        override public void Assembly(Assembler asm)
        {
            if (PartitionByClause != null)
            {                
                asm.AddToken("PARTITION BY");
                asm.AddSpace();
                foreach (Expression exp in PartitionByClause)
                {
                    exp.Assembly(asm);
                    if (PartitionByClause.Last() != exp && PartitionByClause.Count > 0)
                    {
                        asm.AddToken(",");
                    }
                    asm.End(exp);
                }
            }
            if (OrderByClause != null)
            {
                asm.AddSpace();
                asm.AddToken("ORDER BY");
                asm.AddSpace();
                foreach (OrderByItem ordcls in OrderByClause)
                {
                    asm.Begin(ordcls);
                    ordcls.Assembly(asm);
                    asm.End(ordcls);
                }
            }
        }
    }
    #endregion

    #region RowsRangeClause
    public class RowsRangeClause : GrammarNode
    {
        public RowsOrRangeType RowsOrRange { get; set; }
        public WindowFrameExtent Window { get; set; }

        public RowsRangeClause(RowsOrRangeType rowsOrRange, WindowFrameExtent window)
        {
            RowsOrRange = rowsOrRange;
            Window = window;
        }
    }

    public enum RowsOrRangeType
    {
        Rows, Range
    }
    #endregion

    #region WindowFrameExtent
    abstract public class WindowFrameExtent : GrammarNode
    {
    }

    public class BetweenWindowFrameExtent : WindowFrameExtent
    {
        public WindowFrameExtent From { get; set; }
        public WindowFrameExtent To { get; set; }

        public BetweenWindowFrameExtent(WindowFrameExtent from, WindowFrameExtent to)
        {
            From = from;
            To = to;
        }
    }

    public class SimpleWindowFrameExtent : WindowFrameExtent
    {
        public SimpleWindowFrameExtentType Type { get; set; }

        public SimpleWindowFrameExtent(SimpleWindowFrameExtentType type)
        {
            Type = type;
        }
    }

    public enum SimpleWindowFrameExtentType
    {
        UnboundedPreceding, CurrentRow, UnboundedFollowing
    }

    public class IntegerWindowFrameExtent : WindowFrameExtent
    {
        public int Count { get; set; }
        public IntegerWindowFrameExtentType Type { get; set; }

        public IntegerWindowFrameExtent(int count, IntegerWindowFrameExtentType type)
        {
            Count = count;
            Type = type;
        }
    }

    public enum IntegerWindowFrameExtentType
    {
        Preceding, Following
    }
    #endregion

    abstract public class RowsetFunction : GrammarNode
    {
    }

    public class OpenDataSourceFunction : RowsetFunction
    {
        public Expression Provider { get; set; }
        public Expression InitString { get; set; }
        public DbObject Table { get; set; }

        public OpenDataSourceFunction(Expression provider, Expression initString, DbObject table)
        {
            Provider = provider;
            InitString = initString;
            Table = table;
        }
    }

    public class OpenQueryFunction : RowsetFunction
    {
        public Identifier LinkedServer { get; set; }
        public Expression Query { get; set; }

        public OpenQueryFunction(Identifier linkedServer, Expression query)
        {
            LinkedServer = linkedServer;
            Query = query;
        }
    }

    public class OpenRowsetQueryFunction : RowsetFunction
    {
        public StringLiteral ProviderName { get; set; }
        public OpenRowsetProviderSpec ProviderSpec { get; set; }
        public OpenRowsetSource Source { get; set; }

        public OpenRowsetQueryFunction(StringLiteral providerName, OpenRowsetProviderSpec providerSpec, OpenRowsetSource source)
        {
            ProviderName = providerName;
            ProviderSpec = providerSpec;
            Source = source;
        }
    }

    abstract public class OpenRowsetProviderSpec : GrammarNode
    {
    }

    public class StructuredOpenRowsetProviderSpec : OpenRowsetProviderSpec
    {
        public StringLiteral DataSource { get; set; }
        public StringLiteral UserId { get; set; }
        public StringLiteral Password { get; set; }

        public StructuredOpenRowsetProviderSpec(StringLiteral dataSource, StringLiteral userId, StringLiteral password)
        {
            DataSource = dataSource;
            UserId = userId;
            Password = password;
        }
    }

    public class StringOpenRowsetProviderSpec : OpenRowsetProviderSpec
    {
        public StringLiteral ProviderString { get; set; }

        public StringOpenRowsetProviderSpec(StringLiteral providerString)
        {
            ProviderString = providerString;
        }
    }

    abstract public class OpenRowsetSource : GrammarNode
    {
    }

    public class DbObjectOpenRowsetSource : OpenRowsetSource
    {
        public DbObject Table { get; set; }

        public DbObjectOpenRowsetSource(DbObject table)
        {
            Table = table;
        }
    }

    public class QueryOpenRowsetSource : OpenRowsetSource
    {
        public StringLiteral Query { get; set; }

        public QueryOpenRowsetSource(StringLiteral query)
        {
            Query = query;
        }
    }

    public class OpenRowsetBulkFunction : RowsetFunction
    {
        public StringLiteral DataFile { get; set; }
        public OpenRowsetBulkFormat Format { get; set; }

        public OpenRowsetBulkFunction(StringLiteral dataFile, OpenRowsetBulkFormat format)
        {
            DataFile = dataFile;
            Format = format;
        }
    }

    abstract public class OpenRowsetBulkFormat : GrammarNode
    {
    }

    public class FormatFileOpenRowsetBulkFormat : OpenRowsetBulkFormat
    {
        public StringLiteral Path { get; set; }
        public IList<BulkOption> BulkOptions { get; set; }

        public FormatFileOpenRowsetBulkFormat(StringLiteral path, IList<BulkOption> bulkOptions)
        {
            Path = path;
            BulkOptions = bulkOptions;
        }
    }

    abstract public class BulkOption : GrammarNode
    {
    }

    public class StringBulkOption : BulkOption
    {
        public StringBulkOptionType Type { get; set; }
        public StringLiteral Value { get; set; }

        public StringBulkOption(StringBulkOptionType type, StringLiteral value)
        {
            Type = type;
            Value = value;
        }
    }

    public enum StringBulkOptionType
    {
        CodePage, ErrorFile
    }

    public class IntBulkOption : BulkOption
    {
        public IntBulkOptionType Type { get; set; }
        public int Value { get; set; }

        public IntBulkOption(IntBulkOptionType type, int value)
        {
            Type = type;
            Value = value;
        }
    }

    public enum IntBulkOptionType
    {
        FirstRow, LastRow, MaxErrors, RowsPerBatch
    }

    public class OrderBulkOption : BulkOption
    {
        public IList<OrderedColumn> Columns { get; set; }
        public bool Unique { get; set; }

        public OrderBulkOption(IList<OrderedColumn> columns, bool unique)
        {
            Columns = columns;
            Unique = unique;
        }
    }

    public class LobOpenRowsetBulkFormat : OpenRowsetBulkFormat
    {
        public LobType LobType { get; set; }

        public LobOpenRowsetBulkFormat(LobType lobType)
        {
            LobType = lobType;
        }
    }

    public enum LobType
    {
        Blob, Clob, Nclob
    }

    public class OpenXmlFunction : RowsetFunction
    {
        public Expression Document { get; set; }
        public Expression RowPattern { get; set; }
        public Expression Flags { get; set; }
        public OpenXmlWithClause WithClause { get; set; }

        public OpenXmlFunction(Expression document, Expression rowPattern, Expression flags, OpenXmlWithClause withClause)
        {
            Document = document;
            RowPattern = rowPattern;
            Flags = flags;
            WithClause = withClause;
        }
    }

    abstract public class OpenXmlWithClause : GrammarNode
    {
    }

    public class ColumnSpecOpenXmlWithClause : OpenXmlWithClause
    {
        public IList<OpenXmlColumnSpec> Columns { get; set; }

        public ColumnSpecOpenXmlWithClause(IList<OpenXmlColumnSpec> columns)
        {
            Columns = columns;
        }
    }

    public class OpenXmlColumnSpec : GrammarNode
    {
        public Identifier Name { get; set; }
        public DataType Type { get; set; }
        public StringLiteral Pattern { get; set; }

        public OpenXmlColumnSpec(Identifier name, DataType type, StringLiteral pattern)
        {
            Name = name;
            Type = type;
            Pattern = pattern;
        }
    }

    public class TableOpenXmlWithClause : OpenXmlWithClause
    {
        public DbObject Table { get; set; }

        public TableOpenXmlWithClause(DbObject table)
        {
            Table = table;
        }
    }
}
