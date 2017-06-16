using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;

namespace Translator
{
    public class ModifiedList : List<object>
    {
        public Object Peek()
        {
            return this.Last();
        }

        public void Push(Object node)
        {
            this.Add(node);
        }

        public Object Pop()
        {
            Object ret = this.Last();
            this.Remove(ret);
            return ret;
        }
    }

    public class VariablesPool
    {
        const string strBase = "temp_var_";
        int id = 0;

        public VariablesPool()
        {
            id = 0;
        }

        public string GetNewVariableName()
        {
            return strBase + id++;
        }
    }

    public class ProceduresPool
    {
        const string strBase = "temp_procedure_";
        int id = 0;

        public ProceduresPool()
        {
            id = 0;
        }

        public string GetNewProcedureName()
        {
            return strBase + id++;
        }
    }
    public class Modifier : Scanner
    {
        delegate GrammarNode CreateNewExprDelegate();

        #region Constants
        const string ZERO_TIMESTAMP = "1900-01-01 00:00:00.000";
        #endregion

        #region Properties
        public BlockStatement Statement = null;

        ModifiedList _NewParrent = new ModifiedList();
        Stack<object> _OldParrent = new Stack<object>();
        VariablesPool VarPool = new VariablesPool();
        public ProceduresPool ProcPool = new ProceduresPool();
        #endregion

        CreateAlterProcedureStatement GetNearestProcedure()
        {
            foreach (object obj in _NewParrent.Reverse<Object>())
            {
                if (obj is CreateAlterProcedureStatement)
                {
                    return (obj as CreateAlterProcedureStatement);
                }
            }
            return null;
        }

        BlockStatement GetNearestFather()
        {
            foreach (object obj in _NewParrent.Reverse<Object>())
            {
                if (obj is BlockStatement)
                {
                    return (obj as BlockStatement);
                }
                if (obj is CreateAlterProcedureStatement)
                {
                    return (obj as CreateAlterProcedureStatement).Statements;
                }
            }
            return null;
        }

        Statement GetNearestStatement()
        {
            foreach (object obj in _NewParrent.Reverse<Object>())
            {
                if (obj is Statement)
                {
                    return (obj as Statement);
                }
            }
            return null;
        }

        void MoveAllCommentsToNearestStatement(GrammarNode node)
        {
            if (node is Statement)
                return;

            if (node.Comments.Count == 0)
                return;

            Statement stmt = GetNearestStatement();
            if (stmt != null)
            {
                stmt.MoveCommentsFrom(node);
            }
        }

        public override void Scan(GrammarNode node)
        {
            if (node == null)
            {
                return;
            }

            // Clone node
            object newParrent = _NewParrent.Count > 0 ? _NewParrent.Peek() : null;
            object oldParrent = _OldParrent.Count > 0 ? _OldParrent.Peek() : null;
            object nodeClone = null;

            if (Statement == null && node is BlockStatement)
            {
                nodeClone = node.Clone();
                Statement = (nodeClone as BlockStatement);
            }
            else
            {
                FieldInfo fiListParrent = newParrent.GetType().GetField("List");
                if (fiListParrent != null || newParrent is IList)
                {
                    Type nodeType = node.GetType();
                    bool toClone = true;

                    if (nodeType.IsGenericType)
                    {
                        if (nodeType.GetGenericTypeDefinition().Name.Contains("GrammarNodeList"))
                        {
                            Type innerType = GetTemplateType(nodeType);
                            Type clonedTypeList = typeof(List<>);
                            Type i2 = clonedTypeList.MakeGenericType(innerType);
                            nodeClone = Activator.CreateInstance(i2, null);
                            toClone = false;
                        }
                    }

                    if (toClone)
                    {
                        nodeClone = node.Clone();
                    }

                    object list = null;
                    if (fiListParrent != null)
                    {
                        list = fiListParrent.GetValue(newParrent);
                    }
                    else
                    {
                        list = newParrent;
                    }
                    MethodInfo mListToArray = list.GetType().GetMethod("Add");
                    mListToArray.Invoke(list, new object[] { nodeClone });
                }
                else
                {
                    FieldInfo fiList = node.GetType().GetField("List");
                    if (fiList != null || node is IList)
                    {
                        Type[] argType = fiList.FieldType.GetGenericArguments();
                        Type innerType = argType[0];
                        Type generic = typeof(List<>);
                        Type lType = null;

                        if (innerType.IsGenericType)
                        {
                            Type innerListType = GetTemplateType(innerType);
                            Type g2 = typeof(List<>);
                            Type i2 = g2.MakeGenericType(innerListType);

                            lType = generic.MakeGenericType(i2);
                        }
                        else
                        {
                            lType = generic.MakeGenericType(innerType);
                        }

                        nodeClone = Activator.CreateInstance(lType, null);

                        MethodInfo mi = newParrent.GetType().GetMethod(ChildInfo.Setter);
                        mi.Invoke(newParrent, new object[] { nodeClone });
                    }
                    else
                    {
                        nodeClone = node.Clone();

                        MethodInfo mi = newParrent.GetType().GetMethod(ChildInfo.Setter);
                        mi.Invoke(newParrent, new object[] { nodeClone });
                    }
                }
            }

            // Save state
            _OldParrent.Push(node);
            _NewParrent.Push(nodeClone);

            // And scan
            MoveAllCommentsToNearestStatement(node);
            base.Scan(node);

            // Restore state
            _OldParrent.Pop();
            _NewParrent.Pop();
        }

        private Type CreateListItemType(Type genericType)
        {
            if (genericType.IsGenericType)
            {
                Type generic = typeof(List<>);
                Type tType = GetTemplateType(genericType);
                Type finalType = CreateListItemType(tType);

                return (finalType == null) ? null : generic.MakeGenericType(finalType);
            }
            else if (typeof(GrammarNode).IsAssignableFrom(genericType))
            {
                return genericType;
            }

            return null;
        }

        private Type GetTemplateType(Type template)
        {
            Type[] argType = template.GetGenericArguments();
            return argType[0];
        }

        virtual public bool PostAction(CursorVariableDeclaration node)
        {
            if (node.ForUpdateClause != null)
            {
                node.ForUpdateClause = null;
                node.AddNote(Note.MODIFIER, ResStr.NO_FOR_UPDATE_IN_CURSOR);
            }

            return true;
        }

        virtual public bool PostAction(CreateScalarFunctionStatement node)
        {
            CreateProcedureStatement newNode = _NewParrent.Peek() as CreateProcedureStatement;

            if (newNode == null)
            {
                //some error occured!!!
                return false;
            }

            if (node.FunctionOption != null)
            {
                newNode.AddNote(Note.MODIFIER, ResStr.NO_FUNCTION_OPTION);
            }

            newNode.AddNote(Note.ERR_MODIFIER, ResStr.NO_FUNCTION_RETURN_VALUE);

            return true;
        }

        virtual public bool PostAction(CreateFunctionStatement node)
        {
            CreateProcedureStatement newNode = _NewParrent.Peek() as CreateProcedureStatement;

            if (newNode == null)
            {
                //some error occured!!!
                return false;
            }

            if (node.FunctionOption != null)
            {
                newNode.AddNote(Note.MODIFIER, ResStr.NO_FUNCTION_OPTION);
            }

            return true;
        }

        virtual public bool PostAction(CreateProcedureStatement node)
        {
            if (node.ForReplication)
            {
                node.ForReplication = false;
                node.AddNote(Note.MODIFIER, String.Format(ResStr.NO_OPTION_FOR_PROCEDURE, "REPLICATION"));
            }

            if (node.Options != null)
            {
                IList<ProcedureOption> toRemove = new List<ProcedureOption>();
                foreach (ProcedureOption opt in node.Options)
                {
                    SimpleProcedureOption spo = opt as SimpleProcedureOption;

                    if (spo != null)
                    {
                        if (spo.Type == SimpleProcedureOptionType.Encryption)
                        {
                            toRemove.Add(spo);
                            node.AddNote(Note.MODIFIER, String.Format(ResStr.NO_OPTION_FOR_PROCEDURE, "ENCRYPTION"));
                        }

                        if (spo.Type == SimpleProcedureOptionType.Recompile)
                        {
                            toRemove.Add(spo);
                            node.AddNote(Note.MODIFIER, String.Format(ResStr.NO_OPTION_FOR_PROCEDURE, "RECOMPILE"));
                        }
                    }

                    if (opt is ExecuteAsProcedureOption)
                    {
                        toRemove.Add(opt);
                        node.AddNote(Note.MODIFIER, String.Format(ResStr.NO_OPTION_FOR_PROCEDURE, "EXECUTE AS"));
                    }
                }

                foreach (ProcedureOption po in toRemove)
                {
                    node.Options.Remove(po);
                }
            }

            return true;
        }

        virtual public bool Action(DatepartFunctionExpression exp)
        {
            CreateNewExprDelegate create = delegate
            {
                string ident = string.Empty;
                bool needDateConversion = false;
                switch (exp.Part.Name.ToLowerInvariant())
                {
                    case "year":
                    case "yyyy":
                    case "yy":
                        ident = "YEAR";
                        needDateConversion = true;
                        break;
                    case "month":
                    case "mm":
                    case "m":
                        ident = "MONTH";
                        needDateConversion = true;
                        break;
                    case "week":
                    case "wk":
                    case "ww":
                        ident = "WEEK";
                        needDateConversion = true;
                        break;
                    case "minute":
                    case "mi":
                    case "n":
                        ident = "MINUTE";
                        break;
                    case "second":
                    case "ss":
                    case "s":
                        ident = "SECOND";
                        break;
                    case "dw":
                        ident = "WEEKDAY";

                        // These children should be probably skipped for ScanChildren in ReplaceExpression,
                        // but probably its no harm, as they should be already HANA compliant

                        List<Expression> args = new List<Expression>(new Expression[] { exp.Target });
                        if (args[0] is StringConstantExpression)
                        {
                            args[0] = new DateFormatExpression(args[0] as StringConstantExpression, DateFormatExpressionType.WithSeparator);
                        }
                        GenericScalarFunctionExpression dwExp = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, ident), args);

                        IntegerConstantExpression oneExp = new IntegerConstantExpression(1);

                        return new BinaryAddExpression(dwExp, BinaryAddOperatorType.Plus, oneExp);
                    case "hour":
                    case "hh":
                        ident = "HOUR";
                        break;
                    case "dayofyear":
                    case "dy":
                    case "y":
                        ident = "DAYOFYEAR";
                        needDateConversion = true;
                        break;
                    case "day":
                    case "dd":
                    case "d":
                        ident = "DAYOFMONTH";
                        needDateConversion = true;
                        break;
                    case "isowk":
                    case "isoww":
                        ident = "ISOWEEK";
                        break;
                }
                if (string.IsNullOrEmpty(ident))
                {
                    HANANotSupportedExpression ns = new HANANotSupportedExpression();
                    ns.AddNote(Note.ERR_MODIFIER, ResStr.NO_FUNCTION_DATEPART);
                    return ns;
                }
                else
                {
                    List<Expression> args = new List<Expression>(new Expression[] { exp.Target });
                    if (needDateConversion && args[0] is StringConstantExpression)
                    {
                        args[0] = new DateFormatExpression(args[0] as StringConstantExpression, DateFormatExpressionType.WithSeparator);
                    }
                    return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, ident), args);
                }
            };

            return CreateNewExpression(create, exp);
        }
        virtual public bool Action(DatenameFunctionExpression exp)
        {
            CreateNewExprDelegate create = delegate
            {
                switch (exp.Part.Name.ToUpperInvariant())
                {
                    case "YEAR":
                    case "YY":
                    case "YYYY":
                        return new YearFunctionExpression(exp.Target);
                    case "QUARTER":
                    case "QQ":
                    case "Q":
                        return new ToCharFunctionExpression(exp.Target, new Identifier(IdentifierType.Plain, "Q"));
                    case "MONTH":
                    case "MM":
                    case "M":
                        return new MonthFunctionExpression(exp.Target);
                    case "WEEK":
                    case "WK":
                    case "WW":
                        return new WeekFunctionExpression(exp.Target);
                    case "DAY":
                    case "DD":
                    case "D":
                        return new DayOfMonthFunctionExpression(exp.Target);
                    case "DAYOFYEAR":
                    case "DY":
                    case "Y":
                        return new DayOfYearFunctionExpression(exp.Target);
                    case "WEEKDAY":
                    case "DW":
                        return new WeekDayFunctionExpression(exp.Target);
                    case "HOUR":
                    case "HH":
                        return new HourFunctionExpression(exp.Target);
                    case "MINUTE":
                    case "MI":
                    case "N":
                        return new MinuteFunctionExpression(exp.Target);
                    case "SECOND":
                    case "SS":
                    case "S":
                        return new SecondFunctionExpression(exp.Target);
                }

                HANANotSupportedExpression ns = new HANANotSupportedExpression();
                ns.AddNote(Note.ERR_MODIFIER, ResStr.NO_FUNCTION_DATENAME);
                return ns;
            };

           return CreateNewExpression(create, exp);
        }

        virtual public bool Action(CreateFunctionStatement stmt)
        {
            CreateNewExprDelegate create = delegate
            {
                GrammarNode gn = new CreateProcedureStatement(stmt.Name, -1, stmt.FunctionParams, null, false, stmt.FunctionBody.Statements );

                gn.AddNote(Note.MODIFIER, ResStr.WARN_FUNCTION_CONVERTED_TO_PROC);
                return gn;
            };

            return CreateNewExpression(create, stmt);
        }

        virtual public bool Action(CreateTableValuedFunctionStatement stmt)
        {
            CreateNewExprDelegate create = delegate
            {
                List<Statement> lst = new List<Statement>();
                lst.Add(stmt.QueryStatement);

                GrammarNode gn = new CreateProcedureStatement(stmt.Name, -1, stmt.FunctionParams, null, false, lst);

                gn.AddNote(Note.MODIFIER, ResStr.WARN_FUNCTION_CONVERTED_TO_PROC);
                return gn;
            };

            return CreateNewExpression(create, stmt);
        }

        //Returns true when node is generic function, but not the specified one
        protected bool ExpressionReturnsDate(Expression exp)
        {
            if (exp is GenericScalarFunctionExpression)
            {
                if ((exp as GenericScalarFunctionExpression).Name.Name.ToUpper() == "GETDATE" || (exp as GenericScalarFunctionExpression).Name.Name.ToUpper() == "DATEADD")
                {
                    return true;
                }
            }

            if (exp is DbObjectExpression)
            {
                //we expect DB object type is Date...
                return true;
            }

            if (exp is CastFunctionExpression)
            {
                DataType dType = (exp as CastFunctionExpression).Type;

                if (dType is SimpleBuiltinDataType)
                {
                    if ((dType as SimpleBuiltinDataType).Type == SimpleBuiltinDataTypeType.Date || (dType as SimpleBuiltinDataType).Type == SimpleBuiltinDataTypeType.DateTime)
                    {
                        return true;
                    }
                }
            }
                
            if (exp is DateFormatExpression)
            {
                return true;
            }

            return false;
        }

        virtual protected GrammarNode ActionDateDiff(GenericScalarFunctionExpression exp)
        {
            Stringifier str = new Stringifier();
            string identDateDiff = string.Empty;
            int div = 1, multi = 1;
            str.Clear();
            (exp.Arguments[0] as DbObjectExpression).Assembly(str);
            string datePartId = str.Statement.ToLowerInvariant().Replace("\"", "");

            switch (datePartId)
            {
                case "year":
                case "yy":
                case "yyyy":
                    identDateDiff = "YEAR";
                    break;
                case "quarter":
                case "qq":
                case "q":
                    //untranslated
                    break;
                case "month":
                case "mm":
                case "m":
                    //untranslated
                    break;
                case "week":
                case "wk": 
                case "ww":
                    //untranslated
                    break;
                case "day":
                case "dd":
                case "d":
                case "dayofyear":
                case "dy":
                case "y":
                case "weekday":
                case "dw":
                case "w":
                    identDateDiff = "DAYS_BETWEEN";
                    break;
                case "hour":
                case "hh":
                    identDateDiff = "SECONDS_BETWEEN";
                    div = 3600;
                    break;
                case "minute":
                case "mi":
                case "n":
                    identDateDiff = "SECONDS_BETWEEN";
                    div = 60;
                    break;
                case "second":
                case "ss":
                case "s":
                    identDateDiff = "SECONDS_BETWEEN";
                    break;
                case "millisecond":
                case "ms":
                    identDateDiff = "NANO100_BETWEEN";
                    div = 10000;
                    break;
                case "microsecond":
                case "mcs":
                    identDateDiff = "NANO100_BETWEEN";
                    div = 10;
                    break;
                case "nanosecond":
                case "ns":
                    identDateDiff = "NANO100_BETWEEN";
                    multi = 100;
                    break;
            }
            if (string.IsNullOrEmpty(identDateDiff))
            {
                HANANotSupportedExpression ns = new HANANotSupportedExpression();
                ns.AddNote(Note.ERR_MODIFIER, ResStr.NO_FUNCTION_DATEDIFF);
                return ns;
            }

            List<Expression> args = new List<Expression>();
            bool noteFunction = false, noteInteger = false;
            if (exp.Arguments.Count > 1)
            {
                args.AddRange(exp.Arguments.Skip(1));

                if (args[0] is StringConstantExpression)
                {
                    args[0] = new DateFormatExpression(args[0] as StringConstantExpression, DateFormatExpressionType.WithSeparator);
                }
                if (args.Count == 2 && args[1] is StringConstantExpression)
                {
                    args[1] = new DateFormatExpression(args[1] as StringConstantExpression, DateFormatExpressionType.WithSeparator);
                }

                if ((args[0] is IntegerConstantExpression) ||
                    (args.Count > 1 && args[1] is IntegerConstantExpression))
                {
                    noteFunction = true;
                }
                if (!ExpressionReturnsDate(args[0]))
                {
                    args[0] = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "ADD_DAYS"),
                        new List<Expression> { new DateTimeConstantExpression(ZERO_TIMESTAMP), args[0] });
                }
                if (args.Count > 1 && !ExpressionReturnsDate(args[1]))
                {
                    args[1] = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "ADD_DAYS"),
                        new List<Expression> { new DateTimeConstantExpression(ZERO_TIMESTAMP), args[1] });
                }

                noteInteger = args[0] is IntegerConstantExpression;

                if (args.Count == 2)
                {
                    noteInteger |= noteInteger |= args[1] is IntegerConstantExpression;
                }
            }

            GrammarNode ret = null;

            if (identDateDiff.Equals("YEAR"))
            {
                GenericScalarFunctionExpression arg1 = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "YEAR"),
                            new List<Expression> { args[0] });
                GenericScalarFunctionExpression arg2 = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "YEAR"),
                            new List<Expression> { args[1] });

                ret = new BinaryAddExpression(arg2, BinaryAddOperatorType.Minus, arg1);
            }
            else
            {
                ret = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, identDateDiff), args);
                if (div > 1)
                {
                    ret = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "FLOOR"), 
                        new List<Expression> { new MultiplyExpression((Expression)ret, MultiplyOperatorType.Divide, new IntegerConstantExpression(div)) });
                }
                else if (multi > 1)
                {
                    IntegerConstantExpression hundredExp = new IntegerConstantExpression(multi);
                    ret = new MultiplyExpression(new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, identDateDiff), args), MultiplyOperatorType.Multiply, hundredExp);
                }
            }

            if (noteFunction)
            {
                ret.AddNote(Note.MODIFIER, ResStr.WARN_FUNCTION_AS_ARGUMENT_IN_DATE_FUNCTIONS);
            }
            if (noteInteger)
            {
                ret.AddNote(Note.MODIFIER, ResStr.NO_INTEGER_AS_ARGUMENT_IN_DATE_FUNCTION);
            }
            return ret;
        }

        virtual protected GrammarNode ActionDateAdd(GenericScalarFunctionExpression exp)
        {
            Stringifier str = new Stringifier();
            string identDateAdd = string.Empty;
            int multi = 1;

            (exp.Arguments[0] as DbObjectExpression).Assembly(str);
            string datePartId = str.Statement.ToLowerInvariant().Replace("\"", "");
            switch (datePartId)
            {
                case "day":
                case "dd":
                case "d":
                case "dayofyear":
                case "dy":
                case "y":
                case "weekday":
                case "dw":
                case "w":
                    identDateAdd = "ADD_DAYS";
                    break;
                case "month":
                case "mm":
                case "m":
                    identDateAdd = "ADD_MONTHS";
                    break;
                case "week":
                case "ww":
                case "wk":
                    identDateAdd = "ADD_DAYS";
                    multi = 7;
                    break;
                case "quarter":
                case "qq":
                case "q":
                    identDateAdd = "ADD_MONTHS";
                    multi = 3;
                    break;
                case "year":
                case "yy":
                case "yyyy":
                    identDateAdd = "ADD_YEARS";
                    break;
                case "hour":
                case "hh":
                    identDateAdd = "ADD_SECONDS";
                    multi = 3600;
                    break;
                case "minute":
                case "mi":
                case "n":
                    identDateAdd = "ADD_SECONDS";
                    multi = 60;
                    break;
                case "second":
                case "ss":
                case "s":
                    identDateAdd = "ADD_SECONDS";
                    break;
            }
            if (string.IsNullOrEmpty(identDateAdd))
            {
                HANANotSupportedExpression ns = new HANANotSupportedExpression();
                ns.AddNote(Note.ERR_MODIFIER, ResStr.NO_FUNCTION_DATEADD);
                return ns;
            }
            else
            {
                List<Expression> args = new List<Expression>();
                bool noteFunction = false, noteInteger = false;
                if (exp.Arguments.Count > 1)
                {
                    args.AddRange(exp.Arguments.Skip(1).Reverse());

                    if (args[0] is StringConstantExpression)
                    {
                        args[0] = new DateFormatExpression(args[0] as StringConstantExpression, DateFormatExpressionType.WithSeparator);
                    }

                    if (!ExpressionReturnsDate(args[0]))
                    {
                        if (args[0] is IntegerConstantExpression)
                        {
                            noteInteger = true;
                        }

                        args[0] = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "ADD_DAYS"),
                            new List<Expression> { new DateTimeConstantExpression(ZERO_TIMESTAMP), args[0] });                        
                    }
                }

                if (multi > 1)
                {
                    if (NeedParensToExpression(args[1]))
                    {
                        args[1] = new MultiplyExpression(new ParensExpression(args[1]), MultiplyOperatorType.Multiply, new IntegerConstantExpression(multi));
                    }
                    else
                    {
                        args[1] = new MultiplyExpression(args[1], MultiplyOperatorType.Multiply, new IntegerConstantExpression(multi));
                    }
                }

                GrammarNode ret = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, identDateAdd), args);
                if (noteFunction)
                {
                    ret.AddNote(Note.MODIFIER, ResStr.WARN_FUNCTION_AS_ARGUMENT_IN_DATE_FUNCTIONS);
                }
                if (noteInteger)
                {
                    ret.AddNote(Note.MODIFIER, ResStr.NO_INTEGER_AS_ARGUMENT_IN_DATE_FUNCTION);
                }
                return ret;
            }
        }

        private bool NeedParensToExpression(Expression exp)
        {
            if (exp is DbObjectExpression || exp is GenericScalarFunctionExpression || exp is IntegerConstantExpression)
            {
                return false;
            }

            return true;
        }

        virtual public bool Action(GenericScalarFunctionExpression exp)
        {
            CreateNewExprDelegate create = delegate
            {
                switch (exp.Name.Name.ToUpperInvariant())
                {
                    case "SUSER_NAME":
                        return new ParameterlessFunctionExpression(ParameterlessFunctionType.HANACurrentUser);
                    case "GETUTCDATE":
                        return new ParameterlessFunctionExpression(ParameterlessFunctionType.HANACurrentUTCTimeStamp);
                    case "DB_NAME":
                        return new ParameterlessFunctionExpression(ParameterlessFunctionType.HANACurrentSchema);
                    case "LEN":
                        return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "LENGTH"), exp.Arguments);
                    case "DATEADD":
                        return ActionDateAdd(exp);
                    case "DAY":
                        return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "DAYOFMONTH"), exp.Arguments);
                    case "DATEDIFF":
                        return ActionDateDiff(exp);
                    case "GETDATE":
                        return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "NOW"), null);
                    case "LOG":
                        return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "LN"), exp.Arguments);
                    case "LOG10":
                        return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "LOG"),
                            new List<Expression> { new IntegerConstantExpression(10), exp.Arguments[0]});
                    case "ISNULL":
                        return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "IFNULL"), exp.Arguments);
                    case "SPACE":
                        return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "LPAD"),
                            new List<Expression> { new StringConstantExpression(new StringLiteral(StringLiteralType.ASCII, " ")), exp.Arguments[0]});
                    case "SQUARE":
                        return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "POWER"), 
                            new List<Expression> { exp.Arguments[0], new IntegerConstantExpression(2) });
                    case "PI":
                        return new DecimalConstantExpression(3.14159265358979m);
                    case "RAND":
                        {
                            GenericScalarFunctionExpression ns = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "RAND"), null);
                            ns.AddNote(Note.MODIFIER, ResStr.NO_RAND_WITH_SEED);
                            return ns;
                        }
                    case "RADIANS":
                        {
                            // Rad = Deg * PI / 180
                            ParensExpression ns = new ParensExpression(new MultiplyExpression(exp.Arguments[0], MultiplyOperatorType.Multiply, new DecimalConstantExpression(0.01745329252m)));
                            ns.AddNote(Note.MODIFIER, ResStr.NO_RAD_DEG_FUNCTIONS);
                            return ns;
                        }
                    case "DEGREES":
                        {
                            // Deg = Rad * 180 / PI
                            ParensExpression ns = new ParensExpression(new MultiplyExpression(exp.Arguments[0], MultiplyOperatorType.Multiply, new DecimalConstantExpression(57.295779513083m)));
                            ns.AddNote(Note.MODIFIER, ResStr.NO_RAD_DEG_FUNCTIONS);
                            return ns;
                        }
                    case "ATN2":
                        return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "ATAN2"), exp.Arguments);
                    case "ROUND":
                        List<Expression> argsRound = new List<Expression>(exp.Arguments);
                        if (argsRound.Count > 2)
                        {
                            argsRound.RemoveRange(2, argsRound.Count - 2);
                        }
                        return new GenericScalarFunctionExpression(exp.Name, argsRound);
                    case "CHARINDEX":
                        List<Expression> argsLocate = new List<Expression>(exp.Arguments);
                        if (argsLocate.Count > 2)
                        {
                            argsLocate.RemoveRange(2, argsLocate.Count - 2);
                        }

                        //swap order of arguments
                        Expression tmpExp = argsLocate[0];
                        argsLocate[0] = argsLocate[1];
                        argsLocate[1] = tmpExp;

                        return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "LOCATE"), argsLocate);
                    case "GROUPING":
                        {
                            HANANotSupportedExpression ns = new HANANotSupportedExpression();
                            ns.AddNote(Note.ERR_MODIFIER, ResStr.NO_FUNCTION_GROUPING);
                            return ns;
                        }
                    case "EOMONTH":
                        {
                            List<Expression> args = new List<Expression>();
                            IntegerConstantExpression offset = new IntegerConstantExpression(0);

                            args.Add(new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "ADD_MONTHS"),
                                        new List<Expression> { new DateFormatExpression(exp.Arguments[0] as StringConstantExpression, DateFormatExpressionType.WithoutSeparator) }));
                            args.Add(new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "- DAYOFMONTH"),
                                        new List<Expression> { new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "ADD_MONTHS"),
                                        new List<Expression> { new DateFormatExpression(exp.Arguments[0] as StringConstantExpression,DateFormatExpressionType.WithoutSeparator) })}));
                            if (exp.Arguments.Count > 1 && exp.Arguments[1] is IntegerConstantExpression)
                            {
                                offset = new IntegerConstantExpression((exp.Arguments[1] as IntegerConstantExpression).Value);
                                offset.Value++;
                            }
                            args[0] = (new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "ADD_MONTHS"),
                                    new List<Expression> { new DateFormatExpression(exp.Arguments[0] as StringConstantExpression, DateFormatExpressionType.WithoutSeparator), offset }));
                            args[1] = (new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "- DAYOFMONTH"),
                                        new List<Expression> { new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "ADD_MONTHS"),
                                        new List<Expression> { new DateFormatExpression(exp.Arguments[0] as StringConstantExpression, DateFormatExpressionType.WithoutSeparator), offset })}));

                            return new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "ADD_DAYS"), args);
                        }
                }
                return null;
            };

            return CreateNewExpression(create, exp);
        }

        virtual public bool Action(CollationExpression exp)
        {
            CreateNewExprDelegate create = delegate
            {
                Expression newExp = (Expression)exp.Expression.Clone();
                newExp.AddNote(Note.ERR_MODIFIER, ResStr.NO_COLLATE_SPECIFICATION);
                return newExp;
            };

            return CreateNewExpression(create, exp);
        }

        virtual public bool Action(IifFunctionExpression exp)
        {
            CreateNewExprDelegate create = delegate
            {
                List<CaseWhenClause> list = new List<CaseWhenClause>();
                list.Add(new CaseWhenClause(exp.BooleanExpression, exp.TrueExpression));
                Expression newExp = new CaseFunctionExpression(null, list, exp.FalseExpression);
                return newExp;
            };
            return CreateNewExpression(create, exp);
        }

        virtual public bool Action(ChooseFunctionExpression exp)
        {
            CreateNewExprDelegate create = delegate
            {
                List<CaseWhenClause> list = new List<CaseWhenClause>();
                list.Add(new CaseWhenClause(new ComparisonExpression(exp.IndexExpression, ComparisonOperatorType.Equal, new IntegerConstantExpression(1)), exp.FirstMandatoryOption));
                for (int i = 2; i <= exp.RestOfOptions.Count() + 1; i++)
                {
                    list.Add(new CaseWhenClause(new ComparisonExpression(exp.IndexExpression, ComparisonOperatorType.Equal, new IntegerConstantExpression(i)), exp.RestOfOptions.ElementAt(i-2)));
                }
                Expression newExp = new CaseFunctionExpression(null, list, new NullConstantExpression());
                return newExp;
            };
            return CreateNewExpression(create, exp);
        }

        virtual public bool Action(SimpleAggregateFunctionExpression exp)
        {
            switch (exp.Type)
            {
                case SimpleAggregateFunctionType.StDev:
                    CreateNewExprDelegate createHANAStDev = delegate
                    {
                        SimpleAggregateFunctionExpression newExp = new SimpleAggregateFunctionExpression(
                            SimpleAggregateFunctionType.HANAStDev, exp.IsDistinct, exp.Target, exp.OverClause
                            );
                        newExp.AddNote(Note.MODIFIER, ResStr.MOD_STDEV_TO_STDDEV);
                        return newExp;
                    };

                    return CreateNewExpression(createHANAStDev, exp);
                case SimpleAggregateFunctionType.ChecksumAgg:
                    goto case SimpleAggregateFunctionType.StDevP;
                case SimpleAggregateFunctionType.VarP:
                    goto case SimpleAggregateFunctionType.StDevP;
                case SimpleAggregateFunctionType.StDevP:
                    CreateNewExprDelegate createHANANotSupp = delegate
                    {
                        string funcName = string.Empty;
                        switch (exp.Type)
                        {
                            case SimpleAggregateFunctionType.ChecksumAgg:
                                funcName = "CHECKSUM_AGG";
                                break;
                            case SimpleAggregateFunctionType.StDevP:
                                funcName = "STDEVP";
                                break;
                            case SimpleAggregateFunctionType.VarP:
                                funcName = "VARP";
                                break;
                        }
                        HANANotSupportedExpression ns = new HANANotSupportedExpression();
                        ns.AddNote(Note.ERR_MODIFIER, string.Format(ResStr.NO_FUNCTION_SIMPLE_AGGR, funcName));
                        return ns;
                    };

                    return CreateNewExpression(createHANANotSupp, exp);
            }

            return Action (exp as Expression);
        }

        virtual public bool Action(BitwiseNotExpression exp)
        {
            return ReplaceWithEmptyNode(exp, "HANANotSupportedExpression", Note.ERR_MODIFIER, ResStr.NO_FUNCTION_BITWISENOT);
        }

        virtual public bool Action(ConvertFunctionExpression exp)
        {
            CreateNewExprDelegate create = delegate
            {
                GrammarNode node = new CastFunctionExpression(exp.Target, exp.Type);
                node.AddNote(Note.MODIFIER, ResStr.WARN_NO_FUNCTION_CONVERT);
                return node;
            };
            
            return CreateNewExpression(create, exp);
        }

        private bool ExpressionReturnsString(Expression exp)
        {
            if (exp is IntegerConstantExpression || exp is DbObjectExpression)
            {
                return false;
            }

            if (exp is CastFunctionExpression)
            {
                if ((exp as CastFunctionExpression).Type is StringWithLengthDataType)
                {
                    StringWithLengthDataType sType = (StringWithLengthDataType)(exp as CastFunctionExpression).Type;

                    if (sType.Type == StringWithLengthDataTypeType.Char || sType.Type == StringWithLengthDataTypeType.NChar ||
                        sType.Type == StringWithLengthDataTypeType.VarChar || sType.Type == StringWithLengthDataTypeType.NVarChar)
                    {
                        return true;
                    }
                }
            }

            if (exp is GenericScalarFunctionExpression)
            {
                if ((exp as GenericScalarFunctionExpression).ReturnsString())
                {
                    return true;
                }
            }

            if (exp is StringConstantExpression)
            {
                StringConstantExpression strExp = (StringConstantExpression)exp;
                string strToTest = strExp.Value.String;

                Match match = Regex.Match(strToTest, @"[-+]?\d+");
                if (match.Success && match.Value.Length == strToTest.Trim().Length)
                {
                    return false;
                }

                return true;
            }

            if (exp is BinaryAddExpression)
            {
                BinaryAddExpression bae = (BinaryAddExpression)exp;
                return bae.Operator == BinaryAddOperatorType.Plus && (ExpressionReturnsString(bae.LeftExpression) || ExpressionReturnsString(bae.RightExpression));
            }

            return false;
        }

        /// <summary>
        /// Cover special cases
        /// </summary>
        /// <param name="leftExp"></param>
        /// <param name="rigthExp"></param>
        /// <returns>True if both expression should be string-concated, instead of aritmetic plus</returns>
        private bool IsStringConcatExpression(Expression leftExp, Expression rightExp)
        {
            if ((leftExp is StringConstantExpression && rightExp is DbObjectExpression) ||
                (rightExp is StringConstantExpression && leftExp is DbObjectExpression))
            {
                return true;
            }

            if (leftExp is StringConstantExpression && rightExp is StringConstantExpression)
            {
                return true;
            }

            if (ExpressionReturnsString(leftExp) || ExpressionReturnsString(rightExp))
            {
                return true;
            }

            return false;
        }

        virtual public bool Action(BinaryAddExpression exp)
        {
            CreateNewExprDelegate create = delegate
            {
                if (exp.Operator == BinaryAddOperatorType.Plus)
                {
                    if (IsStringConcatExpression(exp.LeftExpression, exp.RightExpression))
                    {
                        return new HANAConcatExpression(exp.LeftExpression, exp.RightExpression);
                    }

                }
                return null;
            };

            return CreateNewExpression(create, exp);
        }

        virtual public bool Action(RankingFunctionExpression exp)
        {
            string name = "";
            switch (exp.Type)
            {
                case RankingFunctionType.Rank:
                    name = "RANK";
                    break;
                case RankingFunctionType.DenseRank:
                    name = "DENSE_RANK";
                    break;
                case RankingFunctionType.RowNumber:
                    name = "ROW_NUMBER";
                    break;
            }
            if (!string.IsNullOrEmpty(name))
            {
                CreateNewExprDelegate create = delegate
                {
                    return new RankingFunctionExpression(exp.Type, exp.OverClause);
                };
            }
            return true;
        }

        virtual public bool Action(NTileFunctionExpression exp)
        {
            CreateNewExprDelegate create = delegate
            {
                return new NTileFunctionExpression(exp.GroupCount, exp.OverClause);
            };

            return CreateNewExpression(create, exp);
        }

        virtual public bool Action(QuerySpecification qsp)
        {
            if (qsp.FromClause == null)
            {
                qsp.AddNote(Note.MODIFIER, ResStr.WARN_DUMMY_TABLE);

                (_NewParrent.Peek() as QuerySpecification).FromClause = new List<TableSource>();
                (_NewParrent.Peek() as QuerySpecification).FromClause.Add(new DbObjectTableSource(new DbObject(new Identifier(IdentifierType.Plain, "DUMMY")), null, null, null));
            }
            return true;
        }
        virtual public bool Action(ExecStatementSQL exp)
        {
            if (exp.Params != null)
            {
                exp.AddNote(Note.ERR_MODIFIER, ResStr.NO_PARAMS_IN_EXEC);
            }

            if (exp.Context != null)
            {
                exp.AddNote(Note.ERR_MODIFIER, ResStr.NO_AS_CLAUSE);
            }

            if (exp.LinkedServer != null)
            {
                exp.AddNote(Note.ERR_MODIFIER, ResStr.NO_LINKED_SERVER);
            }
            return true;
        }

        virtual public bool Action(DeleteStatement statement)
        {
            DeleteStatement stmt = _NewParrent.Peek() as DeleteStatement;

            if (statement.FromClause != null)
            {
                stmt.AddNote(Note.ERR_MODIFIER, ResStr.NO_FROM_CLAUSE);
            }

            if (statement.TopClause != null)
            {
                stmt.AddNote(Note.ERR_MODIFIER, ResStr.NO_TOP_CLAUSE);
            }

            if (statement.WithClause != null)
            {
                stmt.AddNote(Note.ERR_MODIFIER, ResStr.NO_WITH_CLAUSE);
            }

            if (statement.OptionClause != null)
            {
                stmt.AddNote(Note.ERR_MODIFIER, ResStr.NO_OPTION_CLAUSE);
            }

            stmt.WithClause = null;
            stmt.TopClause = null;
            stmt.FromClause = null;
            stmt.OptionClause = null;
            stmt.OutputClause = null;

            return true;
        }

        virtual public bool Action(DbObjectTableSource tableSource)
        {
            if (tableSource.Hints != null || tableSource.TableSampleClause != null)
            {
                DbObjectTableSource newTable = new DbObjectTableSource(tableSource.DbObject, tableSource.Alias, null, null);

                if (tableSource.Hints != null)
                {
                    newTable.AddNote(Note.MODIFIER, ResStr.NO_TABLE_HINTS);
                }

                if (tableSource.TableSampleClause != null)
                {
                    newTable.AddNote(Note.MODIFIER, ResStr.NO_TABLESAMPLE);
                }

                ReplaceNode(newTable);
                return false;
            }

            return true;
        }

        virtual public bool Action(AlterViewStatement node)
        {
            node.AddNote(Note.ERR_MODIFIER, ResStr.NO_ALTER_VIEW);

            // not supported, don't bother with childs
            return false;
        }

        virtual public bool Action(DeallocateStatement node)
        {
            DeallocateStatement stmt = _NewParrent.Peek() as DeallocateStatement;
            stmt.Hide = true;
            stmt.Terminate = false;
            stmt.AddNote(Note.ERR_MODIFIER, ResStr.NO_DEALLOCATE_STATEMENT);

            // not supported, don't bother with childs
            return false;
        }

        virtual public bool Action(MultiplyExpression exp)
        {
            CreateNewExprDelegate create = delegate
            {
                if (exp.Operator == MultiplyOperatorType.Modulo)
                {
                    GenericScalarFunctionExpression ns = new GenericScalarFunctionExpression(new Identifier(IdentifierType.Plain, "MOD"),
                            new List<Expression> { exp.LeftExpression, exp.RightExpression });                        
                    return ns;
                }
                return null;
            };
            
            return CreateNewExpression(create, exp);
        }

        virtual public bool Action(ValuesClauseDefault exp)
        {
            return ReplaceWithEmptyNode(exp, "HANANotSupportedValuesClause", Note.ERR_MODIFIER, ResStr.NO_CLAUSE_DEFAULT_VALUES);
        }

        virtual public bool Action(SelectClause exp)
        {
            if (exp.TopClause != null)
            {
                if (exp.TopClause.IsPercent)
                {
                    exp.AddNote(Note.ERR_MODIFIER, ResStr.NO_PERCENT);
                    exp.TopClause.IsPercent = false;
                }

                if (exp.TopClause.IsWithTies)
                {
                    exp.AddNote(Note.ERR_MODIFIER, ResStr.NO_TOP_WITH_TIES);
                    exp.TopClause.IsWithTies = false;
                }
            }

            return Action(exp as GrammarNode);
        }

        virtual public bool PostAction(CreateViewStatement node)
        {
            CreateViewStatement newNode = (CreateViewStatement)_NewParrent.Peek();

            if (node.CheckOption)
            {
                newNode.CheckOption = false;
                newNode.AddNote(Note.ERR_MODIFIER, ResStr.NO_CHECK_OPTION);
            }

            if (node.Attributes != null)
            {
                newNode.Attributes = null;
                newNode.AddNote(Note.ERR_MODIFIER, ResStr.NO_ATTRIBUTE_IN_CREATE_VIEW);
            }

            newNode.Statement.Terminate = false;

            return false;
        }

        virtual public bool PostAction(AlterViewStatement node)
        {
            (_NewParrent.Peek() as Statement).Hide = true;
            return false;
        }

        virtual public bool PostAction(DropViewStatement node)
        {
            if (node.Views.Count > 1)
            {
                DropViewStatement newNode = (DropViewStatement)_NewParrent.Peek();
                IList<DbObject> views = newNode.Views;

                //split one drop to several drop statements
                BlockStatement statement = GetNearestFather();
                foreach(DbObject view in views)
                {
                    DropViewStatement singleDrop = new DropViewStatement(new List<DbObject>() {view});
                    if (view == views.Last())
                    {
                        singleDrop.AddNote(Note.MODIFIER, ResStr.WARN_DROP_VIEW_DIVIDED);
                    }
                    statement.AddStatement(singleDrop);
                }
                statement.RemoveStatement(newNode);
                return true;
            }

            return false;
        }


        virtual public bool PostAction(DropIndexStatement node)
        {
            DropIndexStatement newNode = (DropIndexStatement)_NewParrent.Peek();
            IList<DropIndexAction> actions = newNode.Actions;

            if (actions.Count > 1)
            {
                //split insert into several insert statemnts
                BlockStatement statement = GetNearestFather();
                foreach (DropIndexAction action in actions)
                {
                    DropIndexStatement singleDrop = new DropIndexStatement(new List<DropIndexAction>() { action });
                    if (action == actions.Last())
                    {
                        singleDrop.AddNote(Note.MODIFIER, ResStr.WARN_DROP_INDEX_DIVIDED);
                        if (GetNearestProcedure() != null)
                        {
                            singleDrop.AddNote(Note.MODIFIER, ResStr.WARN_DROP_INDEX_IN_PROC);
                        }
                    }

                    statement.AddStatement(singleDrop);

                    if (action.Options != null)
                    {
                        action.Options = null;
                        singleDrop.AddNote(Note.ERR_MODIFIER, ResStr.NO_WITH_OPTIONS_FOR_DROP_INDEX);
                    }

                    if (action.TableSource != null)
                    {
                        action.TableSource = null;
                        singleDrop.AddNote(Note.ERR_MODIFIER, ResStr.NO_TABLE_FOR_INDEX);
                    }
                }
                statement.RemoveStatement(_NewParrent.Peek() as Statement);
                return true;
            }
            else
            {
                if (GetNearestProcedure() != null)
                {
                    newNode.AddNote(Note.MODIFIER, ResStr.WARN_DROP_INDEX_IN_PROC);
                }
            }

            return false;
        }

        virtual public bool PostAction(GoStatement stmt)
        {
            (_NewParrent.Peek() as Statement).Hide = true;
            return false;
        }

        virtual public bool PostAction(AlterIndexStatement node)
        {
            AlterIndexStatement newNode = (AlterIndexStatement)_NewParrent.Peek();

            if (node.TableSource != null)
            {
                newNode.TableSource = null;
                newNode.AddNote(Note.ERR_MODIFIER, ResStr.NO_TABLE_FOR_INDEX);
            }

            if (node.Index.IsEmpty)
            {
                newNode.AddNote(Note.ERR_MODIFIER, ResStr.NO_ALL_INDEXES);
            }

            if (node.Action is DisableAlterIndexAction)
            {
                newNode.Action = null;
                newNode.AddNote(Note.ERR_MODIFIER, String.Format(ResStr.ONLY_REBUILD_IN_ALTER_INDEX, "DISABLE"));
            }

            if (node.Action is ReorganizeAlterIndexAction)
            {
                newNode.Action = null;
                newNode.AddNote(Note.ERR_MODIFIER, String.Format(ResStr.ONLY_REBUILD_IN_ALTER_INDEX, "REORGANIZE"));
            }

            if (node.Action is SetAlterIndexAction)
            {
                newNode.Action = null;
                newNode.AddNote(Note.ERR_MODIFIER, String.Format(ResStr.ONLY_REBUILD_IN_ALTER_INDEX, "SET"));
            }

            if (node.Action is RebuildAlterIndexAction)
            {
                RebuildAlterIndexAction rebuild = (RebuildAlterIndexAction)newNode.Action;
                
                if (rebuild.Options != null )
                {
                    rebuild.AddNote(Note.ERR_MODIFIER, String.Format(ResStr.NO_OPTION_ALLOWED_IN_REBUILD, "WITH")); 
                    rebuild.Options = null;
                }

                if (rebuild.Partition != null)
                {
                    rebuild.AddNote(Note.ERR_MODIFIER, String.Format(ResStr.NO_OPTION_ALLOWED_IN_REBUILD, "PARTITION"));
                    rebuild.Options = null;
                }
            }

            return false;
        }

        virtual public bool PostAction(UpdateStatement statement)
        {
            UpdateStatement ret = _NewParrent.Peek() as UpdateStatement;

            if (statement.TopClause != null)
            {
                ret.AddNote(Note.ERR_MODIFIER, ResStr.NO_TOP_CLAUSE);
                if (statement.TopClause.IsPercent)
                {
                    ret.AddNote(Note.ERR_MODIFIER, ResStr.NO_PERCENT);
                }
                ret.TopClause = null;
            }

            if (statement.OptionClause != null)
            {
                ret.AddNote(Note.ERR_MODIFIER, ResStr.NO_OPTION_CLAUSE);
                ret.OptionClause = null;
            }

            if (statement.OutputClause != null)
            {
                ret.AddNote(Note.ERR_MODIFIER, ResStr.NO_OUTPUT_CLAUSE);
                ret.OutputClause = null;
            }

            return false;
        }

        virtual public bool PostAction(DropTableStatement node)
        {
            //divide statement into multiple statements
            DropTableStatement newNode = (DropTableStatement)_NewParrent.Peek();
            List<DbObjectTableSource> tableSources = (List<DbObjectTableSource>)newNode.TableSources;

            if (tableSources.Count > 1)
            {
                //split insert into several insert statemnts
                BlockStatement statement = GetNearestFather();
                foreach (DbObjectTableSource tableSource in tableSources)
                {
                    DropTableStatement rowStatement = new DropTableStatement(new List<DbObjectTableSource> { (DbObjectTableSource)tableSource.Clone() });
                    if (tableSource == tableSources.Last())
                    {
                        rowStatement.AddNote(Note.MODIFIER, ResStr.WARN_DROP_DIVIDED);
                    }
                    statement.AddStatement(rowStatement);
                }

                return false;
            }

            return false;
        }

        virtual public bool PostAction(SelectStatement stmt)
        {
            if ((stmt.QueryExpression is QuerySpecification) &&
                ((stmt.QueryExpression as QuerySpecification).IntoClause != null))
            {
                SelectStatement newStmt = (_NewParrent.Peek() as SelectStatement);

                BlockStatement statement = GetNearestFather();
                QuerySpecification qsp = (newStmt.QueryExpression as QuerySpecification);
                CreateTableStatement create = new CreateTableStatement(qsp.IntoClause, false, null, null, null, null, null, qsp);
                statement.AddStatement(create);

                // remove old statement
                statement.RemoveStatement(newStmt);
                return false;
            }
            if (stmt.QueryExpression is QuerySpecification)
            {
               if ((stmt.QueryExpression as QuerySpecification).SelectClause.SelectItems.Where(s => s is SelectVariableItem).Count() > 0)
               {
                   BlockStatement father = GetNearestFather();
                   SelectStatement oStmt = _NewParrent.Peek() as SelectStatement;
                   List<SelectVariableItem> list = new List<SelectVariableItem>();
                   foreach (SelectItem itm in (oStmt.QueryExpression as QuerySpecification).SelectClause.SelectItems.Where(s => s is SelectVariableItem))
                   {
                       list.Add(itm as SelectVariableItem);
                   }
                   SelectVariableStatement nStmt;
                   if (stmt.QueryExpression is QuerySpecification)
                   {
                       nStmt = new SelectVariableStatement(list, (oStmt.QueryExpression as QuerySpecification).FromClause);                   
                   }
                   else
                   {
                       nStmt = new SelectVariableStatement(list);
                   }

                   //remove VariableItems
                   foreach (SelectVariableItem itm in list)
                   {
                       (oStmt.QueryExpression as QuerySpecification).SelectClause.SelectItems.Remove(itm);
                   }

                   //if items list is empty remopve old statement
                   if ((oStmt.QueryExpression as QuerySpecification).SelectClause.SelectItems.Count() == 0)
                   {
                       father.ReplaceStatement(oStmt, nStmt);
                   }
                   else
                   {
                       father.AddStatement(nStmt);
                       father.RemoveStatement(oStmt);
                   }
               }
            }

            return false;
        }

        virtual public bool PostAction(ValuesClauseSelect cls)
        {
            (_NewParrent.Peek() as ValuesClauseSelect).Statement.Terminate = false;
            return false;
        }

        virtual public bool PostAction(SqlStartStatement stmt)
        {
            //CreateAlterProcedureStatement proc = GetNearestProcedure();
            (_NewParrent.Peek() as SqlStartStatement).Hide = true;
            return false;
        }

        virtual public bool PostAction(CreateBaseTypeStatement stmt)
        {
            (_NewParrent.Peek() as CreateBaseTypeStatement).Hide = true;
            return false;
        }

        virtual public bool Action(UseStatement stmt)
        {
            HANASetSchemaStatement ret = new HANASetSchemaStatement(stmt.Database);
            GetNearestFather().ReplaceStatement(_NewParrent.Peek() as Statement, ret);
            return true;
        }

        virtual public bool PostAction(TriggerAction act)
        {
            List<Statement> toRemove = new List<Statement>();
            TriggerAction nAct = _NewParrent.Peek() as TriggerAction;
            foreach (Statement stmt in nAct.Statements.Statements)
            {
                if (stmt is SelectStatement)
                {
                    toRemove.Add(stmt);
                }
            }
            foreach (Statement stmt in toRemove)
            {
                Statement nStmt = new HANANotSupportSimpleStatementinTriggers();
                nStmt.AddNote(Note.MODIFIER, ResStr.NO_SIMPLE_STATEMENTS_IN_TRIGGERS);
                nAct.Statements.ReplaceStatement(stmt, nStmt);
            }

            return false;
        }

        virtual public bool Action(WithCommonTable exp)
        {
            CreateNewExprDelegate create = delegate
            {
                if (exp.Query != null)
                {
                    Action(exp.Query);
                }
                if (exp.Name != null)
                {
                    exp.Name.Type = IdentifierType.Quoted;
                }
                return exp;
            };

            return CreateNewExpression(create, exp);
        }

        virtual public bool Action(InsertStatement statement)
        {
            InsertStatement ret = _NewParrent.Peek() as InsertStatement;

            if (statement.TopClause != null)
            {
                ret.AddNote(Note.ERR_MODIFIER, ResStr.NO_TOP_CLAUSE);
                ret.TopClause = null;
            }

            if (statement.OutputClause != null)
            {
                ret.AddNote(Note.ERR_MODIFIER, ResStr.NO_OUTPUT_CLAUSE);
                ret.OutputClause = null;
            }

            if (statement.ValuesClause is ValuesClauseExec)
            {
                ValuesClauseExec valuesclause = (ValuesClauseExec)statement.ValuesClause;
                if (valuesclause.ExecStatement is ExecStatementSP)
                {
                    ret.InsertTarget.AddNote(Note.ERR_MODIFIER, ResStr.NO_EXEC_SP_IN_INSERT);
                    (ret.ValuesClause as ValuesClauseExec).ExecStatement.Terminate = false;
                }
                else if (valuesclause.ExecStatement is ExecStatement)
                {
                    ret.InsertTarget.AddNote(Note.ERR_MODIFIER, ResStr.NO_EXEC_IN_INSERT);
                    (ret.ValuesClause as ValuesClauseExec).ExecStatement.Terminate = false;
                }
            }

            return true;
        }

        virtual public bool PostAction(InsertStatement node)
        {
            //divide statement into multiple statements
            if (node.ValuesClause is ValuesClauseValues)
            {
                InsertStatement newNode = (InsertStatement)_NewParrent.Peek();
                ValuesClauseValues values = (ValuesClauseValues)newNode.ValuesClause;

                if (values.Values.Count > 1)
                {
                    //split insert into several insert statemnts
                    BlockStatement statement = GetNearestFather();
                    foreach (List<Expression> row in values.Values)
                    {
                        InsertStatement rowStatement = new InsertStatement(newNode.TopClause, newNode.InsertTarget, newNode.ColumnList, newNode.OutputClause, null);
                        rowStatement.ValuesClause = new ValuesClauseValues(row);
                        if (row == values.Values.Last())
                        {
                            rowStatement.AddNote(Note.MODIFIER, ResStr.WARN_INSERT_DIVIDED);
                        }
                        statement.AddStatement(rowStatement);
                    }
                    // Remove orginal statement
                    statement.RemoveStatement(newNode);
                    return false;
                }
            }

            return false;
        }

        virtual public bool PostAction(CreateTableStatement stmto)
        {
            CreateTableStatement stmt = _NewParrent.Peek() as CreateTableStatement;
            List<CreateTableDefinition> toRemove = new List<CreateTableDefinition>();
            if (stmt.Definitions != null)
            {
                foreach (CreateTableDefinition def in stmt.Definitions)
                {
                    if (def is PrimaryKeyTableConstraint)
                    {
                        BlockStatement father = GetNearestFather();
                        AlterTableStatement nStmt = new AlterTableStatement(new DbObjectTableSource(stmt.Name, null, null, null),
                            new AddAlterTableAction(null, new List<CreateTableDefinition> 
                            {
                                def
                            }));

                        // Can do that now, because there is no replacement done in Action
                        Action(nStmt.Action as AddAlterTableAction);

                        father.AddStatement(nStmt);
                        toRemove.Add(def);
                    }
                }
                foreach (CreateTableDefinition def in toRemove)
                {
                    stmt.Definitions.Remove(def);
                }
            }
            return true;
        }

        virtual public bool PostAction(BeginTransactionStatement stmt)
        {
            (_NewParrent.Peek() as Statement).Hide = true;

            return false;
        }

        virtual public bool PreAction(WaitForStatement stmt)
        {
            (_NewParrent.Peek() as Statement).Hide = true;

            return false;
        }

        virtual public bool PreAction(TryStatement stmt)
        {
            CreateAlterProcedureStatement proc = GetNearestProcedure();
            if (proc != null)
            {
                GetNearestFather().RemoveStatement(_NewParrent.Peek() as Statement);
                proc.Statements.AddStatement(_NewParrent.Peek() as Statement);
            }

            (_NewParrent.Peek() as Statement).Hide = true;
            return true;
        }

        virtual public bool PostAction(ThrowStatement stmt)
        {
            (_NewParrent.Peek() as Statement).Hide = true;

            return false;
        }

        virtual public bool PostAction(GotoStatement stmt)
        {
            (_NewParrent.Peek() as Statement).Hide = true;

            return false;
        }

        virtual public bool PostAction(LabelStatement stmt)
        {
            (_NewParrent.Peek() as Statement).Hide = true;

            return false;
        }

        virtual public bool PostAction(DeclareStatement stmt)
        {
            BlockStatement statement;
            CreateAlterProcedureStatement proc = GetNearestProcedure();
            if (proc != null)
            {
                statement = proc.Declarations;
                proc.Declarations.AddStatement(_NewParrent.Peek() as Statement);
                if (GetNearestFather() == proc.Statements)
                {
                    proc.Statements.RemoveStatement(_NewParrent.Peek() as Statement);
                }
                else
                {
                    GetNearestFather().RemoveStatement(_NewParrent.Peek() as Statement);
                }
            }
            else
            {
                statement = GetNearestFather();
            }
            
            if (stmt.Declarations.Count > 1)
            {
                //split insert into several insert statemnts
                foreach (VariableDeclaration decl in stmt.Declarations)
                {
                    if (decl is TableVariableDeclaration)
                    {
                        TableVariableDeclaration dec = decl as TableVariableDeclaration;
                        CreateTableTypeStatement stmtCT = new CreateTableTypeStatement(new DbObject(new Identifier(IdentifierType.Plain, dec.Variable.Value + "_TYPE")), dec.Definition);
                        statement.AddStatement(stmtCT);
                    }
                    List<VariableDeclaration> list = new List<VariableDeclaration>();
                    list.Add(decl);
                    DeclareStatement nStmt = new DeclareStatement(list);
                    statement.AddStatement(nStmt);
                }
                statement.RemoveStatement(_NewParrent.Peek() as Statement);
            }
            else if (stmt.Declarations[0] is TableVariableDeclaration)
            {
                TableVariableDeclaration dec = stmt.Declarations[0] as TableVariableDeclaration;
                CreateTableTypeStatement stmtCT = new CreateTableTypeStatement(new DbObject(new Identifier(IdentifierType.Plain, dec.Variable.Value + "_TYPE")), dec.Definition);
                statement.InsertBefore(_NewParrent.Peek() as Statement, stmtCT);
            }
            else if (stmt.Declarations[0] is CursorVariableDeclaration)
            {
                ((_NewParrent.Peek() as DeclareStatement).Declarations[0] as CursorVariableDeclaration).Statement.Terminate = false;
            }
            return false;
        }

        virtual public bool PreAction(TableVariableDeclaration dec)
        {
            dec.Variable.IsArgument = false;
            return false;
        }

        virtual public bool Action(SetDateFirstVariableStatement stmt)
        {
            return ReplaceWithEmptyNode(stmt, "HANANotSupportedStatement", Note.ERR_MODIFIER, ResStr.NO_SET_DATEFIRST);
        }

        virtual public bool Action(SetDateFirstStatement stmt)
        {
            return ReplaceWithEmptyNode(stmt, "HANANotSupportedStatement", Note.ERR_MODIFIER, ResStr.NO_SET_DATEFIRST);
        }

        virtual public bool Action(SetDateFormatStatement stmt)
        {
            return ReplaceWithEmptyNode(stmt, "HANANotSupportedStatement", Note.ERR_MODIFIER, ResStr.NO_SET_DATEFORMAT);
        }

        virtual public bool Action(SetDateFormatVariableStatement stmt)
        {
            return ReplaceWithEmptyNode(stmt, "HANANotSupportedStatement", Note.ERR_MODIFIER, ResStr.NO_SET_DATEFORMAT);
        }

        virtual public bool Action(SetLockTimeoutStatement stmt)
        {
            return ReplaceWithEmptyNode(stmt, "HANANotSupportedStatement", Note.ERR_MODIFIER, String.Format(ResStr.NO_SETTING_SUPPORTED, "LOCK TIMEOUT"));
        }

        virtual public bool Action(SetSpecialStatement stmt)
        {
            string settingName = stmt.Type.ToString();
            string msg = ResStr.NO_TSQL_SETTINGS_SUPPORTED;

            if (stmt.Type >= SetOptionType.ANSI_WARNINGS && stmt.Type <= SetOptionType.XACT_ABORT)
            {
                msg = String.Format(ResStr.NO_SETTING_SUPPORTED, stmt.Type.ToString());
            }

            return ReplaceWithEmptyNode(stmt, "HANANotSupportedStatement", Note.ERR_MODIFIER, msg);
        }

        virtual public bool Action(SetIdentityInsertStatement stmt)
        {
            return ReplaceWithEmptyNode(stmt, "HANANotSupportedStatement", Note.ERR_MODIFIER, String.Format(ResStr.NO_SETTING_SUPPORTED, "IDENTITY INSERT"));
        }

        virtual public bool Action(UpdateStatisticStatement stmt)
        {
            return ReplaceWithEmptyNode(stmt, "HANANotSupportedStatement", Note.ERR_MODIFIER, ResStr.NO_UPDATE_STATISTICS);
        }

        virtual public bool PreAction(SetStatement stmt)
        {
            if (stmt.Variable != null)
                stmt.Variable.IsArgument = false;

            CreateAlterProcedureStatement proc = GetNearestProcedure();

            if (proc != null && stmt.Variable != null)
            {
                foreach (ProcedureParameter param in proc.Parameters)
                {
                    if (param is DataTypeProcedureParameter)
                    {
                        if ((param as DataTypeProcedureParameter).Name == stmt.Variable.Value)
                        {
                            (param as DataTypeProcedureParameter).InOut = true;
                        }
                    }
                }
            }
            return false;
        }


        virtual public bool PostAction(SetStatement stmt)
        {
            if (stmt.Expression is SubqueryExpression)
            {
                SelectVariableStatement nStmt = new SelectVariableStatement(new List <SelectVariableItem > {new SelectVariableItem(stmt.Variable, stmt.Operator, stmt.Expression)});
                GetNearestFather().ReplaceStatement(_NewParrent.Peek() as Statement, nStmt);
            }

            return false;
        }

        virtual public bool PreAction(ScalarVariableDeclaration decl)
        {
            if (decl.Variable != null)
                decl.Variable.IsArgument = false;

            return false;
        }

        virtual public bool PreAction(SelectVariableItem item)
        {
            if (item.Variable != null)
                item.Variable.IsArgument = false;

            return false;
        }

        virtual public bool PreAction(IfStatement stmt)
        {
            CreateAlterProcedureStatement proc = GetNearestProcedure();

            if (stmt.Condition is ComparisonExpression)
            {
                if ((stmt.Condition as ComparisonExpression).LeftExpression is SubqueryExpression)
                {
                    BlockStatement statement = GetNearestFather();
                    string variableName = VarPool.GetNewVariableName();
                    VariableExpression vExp = new VariableExpression(variableName);
                    vExp.IsArgument = false;
                    ScalarVariableDeclaration dec = new ScalarVariableDeclaration(vExp, new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Int), null);
                    DeclareStatement decStmt = new DeclareStatement(new List<VariableDeclaration> { dec as VariableDeclaration });
                    if (proc != null)
                    {
                        proc.Declarations.AddStatement(decStmt);
                    }
                    else
                    {
                        statement.InsertBefore((_NewParrent.Peek() as Statement), decStmt);
                    }
                    VariableExpression vExp2 = new VariableExpression(variableName);
                    SelectVariableItem item = new SelectVariableItem(vExp, AssignmentType.Assign, (stmt.Condition as ComparisonExpression).LeftExpression);
                    SelectVariableStatement sStmt = new SelectVariableStatement(new List<SelectVariableItem> { item });
                    statement.InsertBefore((_NewParrent.Peek() as Statement), sStmt);
                    (stmt.Condition as ComparisonExpression).LeftExpression = vExp2;
                }
            }
            else if (stmt.Condition is ExistsExpression)
            {
                string variableName = VarPool.GetNewVariableName();
                VariableExpression var1 = new VariableExpression(variableName);
                var1.IsArgument = false;
                DeclareStatement decStmt = new DeclareStatement(new List<VariableDeclaration> { new ScalarVariableDeclaration(var1, new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Int), null) });
                if (proc != null)
                {
                    proc.Declarations.AddStatement(decStmt);
                }
                else
                {
                    GetNearestFather().InsertBefore((_NewParrent.Peek() as Statement), decStmt);
                }
                SelectStatement select = new SelectStatement((stmt.Condition as ExistsExpression).Query, null, ForClauseType.None, null);
                SubqueryExpression sub = new SubqueryExpression(select);
                SelectVariableStatement nStmt = new SelectVariableStatement(new List<SelectVariableItem> { new SelectVariableItem(var1, AssignmentType.Assign, sub) });
                GetNearestFather().InsertBefore(_NewParrent.Peek() as Statement, nStmt);
                VariableExpression var2 = new VariableExpression(variableName);
                stmt.Condition = new ComparisonExpression(var2, ComparisonOperatorType.GreaterThan, new IntegerConstantExpression(0));
            }
            else if (stmt.Condition is IsNullExpression)
            {
                string variableName = VarPool.GetNewVariableName();
                VariableExpression var1 = new VariableExpression(variableName);
                var1.IsArgument = false;
                DeclareStatement decStmt = new DeclareStatement(new List<VariableDeclaration> { new ScalarVariableDeclaration(var1, new SimpleBuiltinDataType(SimpleBuiltinDataTypeType.Int), null) });
                if (proc != null)
                {
                    proc.Declarations.AddStatement(decStmt);
                }
                else
                {
                    GetNearestFather().InsertBefore((_NewParrent.Peek() as Statement), decStmt);
                }
                SelectVariableStatement nStmt = new SelectVariableStatement(new List<SelectVariableItem> { new SelectVariableItem(var1, AssignmentType.Assign, (stmt.Condition as IsNullExpression).Target) });
                GetNearestFather().InsertBefore(_NewParrent.Peek() as Statement, nStmt);
                VariableExpression var2 = new VariableExpression(variableName);
                (stmt.Condition as IsNullExpression).Target = var2;
            }
            return false;
        }

        virtual public bool PostAction(AlterTableStatement stmt)
        {
            if (stmt.Action is AddAlterTableAction)
            {
                AlterTableStatement alterStmt = (_NewParrent.Peek() as AlterTableStatement);
                AddAlterTableAction act = (alterStmt.Action as AddAlterTableAction);
                if (act.Definitions.Count > 1)
                {
                    List<CreateTableDefinition> toRemove = new List<CreateTableDefinition>();
                    foreach (CreateTableDefinition def in act.Definitions)
                    {
                        if (def is PrimaryKeyTableConstraint)
                        {
                            BlockStatement father = GetNearestFather();
                            AlterTableStatement nStmt = new AlterTableStatement(alterStmt.TableSource,
                                new AddAlterTableAction(act.WithCheck, new List<CreateTableDefinition> { def }));

                            // Can do that now, because there is no replacement done in Action
                            Action(nStmt.Action as AddAlterTableAction);

                            father.AddStatement(nStmt);
                            toRemove.Add(def);
                        }
                    }
                    foreach (CreateTableDefinition def in toRemove)
                    {
                        (alterStmt.Action as AddAlterTableAction).Definitions.Remove(def);
                    }
                }
            }
            if (stmt.Action is DropAlterTableAction)
            {
                AlterTableStatement alterStmt = (_NewParrent.Peek() as AlterTableStatement);
                DropAlterTableAction act = (alterStmt.Action as DropAlterTableAction);
                if (act.Definitions.Count > 1)
                {
                    List<DropAlterTableDefinition> toRemove = new List<DropAlterTableDefinition>();
                    foreach (DropAlterTableDefinition def in act.Definitions)
                    {
                        if (def is DropConstraintAlterTableDefinition)
                        {
                            BlockStatement father = GetNearestFather();
                            Statement nStmt = new AlterTableStatement(alterStmt.TableSource,
                                new DropAlterTableAction(new List<DropAlterTableDefinition> { def }));
                            father.AddStatement(nStmt);
                            toRemove.Add(def);
                        }
                    }

                    foreach (DropAlterTableDefinition def in toRemove)
                    {
                        (alterStmt.Action as DropAlterTableAction).Definitions.Remove(def);
                    }
                }
            }
            return false;
        }

        virtual public bool Action(OrderedColumn col)
        {
            if (col.Direction != OrderDirection.Nothing)
            {
                foreach (object o in _OldParrent)
                {
                    if (o is PrimaryKeyTableConstraint)
                    {
                        CreateNewExprDelegate create = delegate
                        {
                            OrderedColumn newCol = new OrderedColumn(col.Name, OrderDirection.Nothing);
                            newCol.AddNote(Note.MODIFIER, ResStr.NO_DESC_FOR_INDEX);
                            return newCol;
                        };

                        return CreateNewExpression(create, col);
                    }
                }
            }
            return true;
        }

        virtual public bool PostAction(TableConstraint constraint)
        {
            PrimaryKeyTableConstraint var = _NewParrent.Peek() as PrimaryKeyTableConstraint;
            if (var != null)
            {
                if (var.IndexOptions != null)
                {
                    var.AddNote(Note.MODIFIER, String.Format(ResStr.NO_PRIMARY_KEY_TABLE_CONSTRAINT, "WITH clause"));
                    var.IndexOptions = null;
                }
                if (var.OnClause != null)
                {
                    var.AddNote(Note.MODIFIER, String.Format(ResStr.NO_PRIMARY_KEY_TABLE_CONSTRAINT, "ON clause"));
                    var.OnClause = null;
                }
            }
            return true;
        }

        virtual public bool Action(RecompileExecOption execopt)
        {
            return ReplaceWithHANANotSupportedExecStatementSP(execopt, ResStr.NO_RECOMPILE_ON_HANA);
        }

        virtual public bool Action(OutputClause outcls)
        {
            return ReplaceWithHANANotSupportedOuputClause(outcls, ResStr.NO_OUTPUT_CLAUSE);
        }
        
        virtual public bool Action(AlterProcedureStatement altprocstmt)
        {
            return ReplaceWithHANANotSupportedAlterProcedureStatement(altprocstmt, ResStr.NO_ALTER_STATEMENT);
        }

        virtual public bool Action(AlterColumnAddDropAlterTableAction exp)
        {
            return ReplaceWithHANANotSupportedAlterTableAction(exp, ResStr.NO_COLUMN_CONSTRAINTS);
        }

        virtual public bool Action(CheckAlterTableAction exp)
        {
            return ReplaceWithHANANotSupportedAlterTableAction(exp, ResStr.NO_CHECK_CLAUSE);
        }

        virtual public bool Action(TriggerAlterTableAction exp)
        {
            return ReplaceWithHANANotSupportedAlterTableAction(exp, ResStr.NO_TRIGGER_CLAUSE);
        }

        virtual public bool Action(ChangeTrackingAlterTableAction exp)
        {
            return ReplaceWithHANANotSupportedAlterTableAction(exp, ResStr.NO_CHANGE_TRACKING_CLAUSE);
        }

        virtual public bool Action(SwitchPartitionAlterTableAction exp)
        {
            return ReplaceWithHANANotSupportedAlterTableAction(exp, ResStr.NO_SWITH_CLAUSE);
        }

        virtual public bool Action(SetFilestreamAlterTableAction exp)
        {
            return ReplaceWithHANANotSupportedAlterTableAction(exp, ResStr.NO_SET_CLAUSE);
        }

        virtual public bool Action(RebuildPartitionAlterTableAction exp)
        {
            return ReplaceWithHANANotSupportedAlterTableAction(exp, ResStr.NO_REBUILD_CLAUSE);
        }

        virtual public bool Action(LockEscalationTableOptionAlterTableAction exp)
        {
            return ReplaceWithHANANotSupportedAlterTableAction(exp, ResStr.NO_LOCK_ESCALATION_CLAUSE);
        }

        virtual public bool Action(FiletableTableOptionAlterTableAction exp)
        {
            return ReplaceWithHANANotSupportedAlterTableAction(exp, ResStr.NO_FILETABLE_CLAUSE);
        }

        bool ReplaceWithHANANotSupportedAlterTableAction(AlterTableAction exp, string note)
        {
            return ReplaceWithEmptyNode(exp, "HANANotSupportedAlterTableAction", Note.MODIFIER, note);
        }

        bool ReplaceWithHANANotSupportedOuputClause(OutputClause outcls, string note)
        {
            return ReplaceWithEmptyNode(outcls, "HANANotSupportedOutputClause", Note.MODIFIER, note);
        }

        bool ReplaceWithHANANotSupportedExecStatementSP(RecompileExecOption execopt, string note)
        {
            return ReplaceWithEmptyNode(execopt, "HANANotSupportedExecStatementSP", Note.MODIFIER, note);
        }

        bool ReplaceWithHANANotSupportedAlterProcedureStatement(AlterProcedureStatement alterprocstmt, string note)
        {
            return ReplaceWithEmptyNode(alterprocstmt, "HANANotSupportedAlterProcedureStatement", Note.MODIFIER, note);
        }

        virtual public bool Action(CursorSource source)
        {
            if (source.VarName != null)
            {
                (_NewParrent.Peek() as CursorSource).Name = new Identifier(IdentifierType.Plain, source.VarName.Value);
                (_NewParrent.Peek() as CursorSource).VarName = null;
                (_NewParrent.Peek() as CursorSource).AddNote(Note.MODIFIER, ResStr.NO_CURSOR_AS_VARIABLE);
            }
            return false;
        }

        virtual public bool Action(AlterColumnDefineAlterTableAction exp)
        {
            if (exp.Collation != null)
            {
                exp.AddNote(Note.MODIFIER, String.Format(ResStr.NO_COLUMN_CONSTRAINT, "COLLATION"));
                exp.Collation = null;
            }
            if (exp.IsSparse)
            {
                exp.AddNote(Note.MODIFIER, String.Format(ResStr.NO_COLUMN_CONSTRAINT, "SPARSE"));
                exp.IsSparse = false;
            }
            return Action(exp as AlterTableAction);
        }

        virtual public bool Action(AddAlterTableAction exp)
        {
            if (exp.WithCheck != null)
            {
                exp.AddNote(Note.MODIFIER, ResStr.NO_WITH_CHECK_FOR_ALTER_TABLE);
                exp.WithCheck = null;
            }

            return Action(exp as AlterTableAction);
        }

        bool ReplaceWithEmptyNode(GrammarNode oldNode, string  newNodeType, string noteType, string note)
        {
            GrammarNode newNode = (GrammarNode)System.Activator.CreateInstance(Type.GetType("Translator." + newNodeType));
            newNode.AddNote(noteType, note);

            ReplaceNode(newNode);
            return false;
        }

        bool CreateNewExpression(CreateNewExprDelegate create, GrammarNode oldExp)
        {
            GrammarNode newExpr = create();
            if (newExpr != null)
            {
                ReplaceNode(newExpr);
                return false;
            }
            else
            {
                Action(oldExp as Expression);
                return true;
            }
        }

        void ReplaceNode(GrammarNode newExpr)
        {
            if (_NewParrent.Count >= 2)
            {
                // Our new parrent is 2 pops away
                object currParrent = _NewParrent.Count > 0 ? _NewParrent.Pop() : null;
                object newParrent = _NewParrent.Peek();

                // Replace expression
                if (newParrent is IList)
                {
                    // If list, old node must be replaced
                    IList list = newParrent as IList;
                    int idx = list.IndexOf(currParrent);

                    //list[ChildInfo.Index] = newExpr;
                    list[idx] = newExpr;
                }
                else
                {
                    MethodInfo mi = newParrent.GetType().GetMethod(ChildInfo.Setter);
                    mi.Invoke(newParrent, new object[] { newExpr });
                }

                // Save replaced expression
                newExpr.ReplacedNode = currParrent as GrammarNode;

                // Push new children to stack
                _NewParrent.Push(newExpr);

                // Scan only children (they are shared with old expression right now)
                ScanChildren(newExpr);
            }
        }
    }
}
