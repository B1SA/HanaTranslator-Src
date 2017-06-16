using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    class Report : Scanner
    {
        GrammarNode _OldNode = null;
        GrammarNode _NewNode = null;
        string _HTML = string.Empty;
        public string HTML
        {
            get { return _HTML; }
        }

        public virtual void Scan(GrammarNode oldNode, GrammarNode newNode)
        {
            _HTML = string.Empty;

            _NewNode = newNode;
            _OldNode = oldNode;

            _HTML += "<HTML><HEAD></HEAD><BODY>";
            Scan(oldNode);
            Scan(newNode);
            _HTML += "</BODY></HTML>";
        }

        #region GrammarNode
        public virtual bool PreAction(GrammarNode child)
        {
            _HTML += string.Format("<DIV CLASS=\"{0}\">", child.GetType().Name);
            return true;
        }
        public virtual bool PostAction(GrammarNode child)
        {
            _HTML += "</DIV>";
            return true;
        }

        public override bool Action(GrammarNode child)
        {
            Stringifier str = new Stringifier();
            if (child.ReplacedNode != null)
            {
                FindOldNode(child.ReplacedNode);
            }
            else
            {
            }
            child.Assembly(str);
            _HTML += string.Format("{0}", str.Statement);

            return true;
        }
        #endregion

        GrammarNode FindOldNode(GrammarNode findNode)
        {
            return _OldNode;
        }

        #region BasicTreeNodes

        #region Identifier
        // NOTHING TODO
        #endregion

        #region StringLiteral
        // NOTHING TODO
        #endregion

        #region DbObject
        public virtual bool Action(DbObject child)
        {
            if (child.Identifiers != null)
            {
                Stringifier str = new Stringifier();
                child.Assembly(str);
                _HTML += str.Statement;
            }

            return false;
        }
        #endregion

        #region DataType
        public virtual bool Action(GenericDataType child)
        {
            if (child.Schema != null)
            {
                Scan(child.Schema);
                _HTML += ".";
            }
            Scan(child.Name);

            return false;
        }
        #endregion
        #endregion  // BasicTreeNodes

        #region TreeNodes
        #region DbObjectInsertTarget
        // NOTHING TODO
        #endregion

        #region RowSetFunctionInsertTarget
        // TODO
        #endregion

        #region UpdateStatement
        // TODO
        #endregion

        #region SelectStatement
        public virtual bool Action(SelectStatement child)
        {
            if (child.QueryExpression != null)
            {
                Scan(child.QueryExpression);
            }
            return false;
        }
        #endregion

        #region SetItemColumn
        // TODO
        #endregion

        #region SetItemVariable
        // TODO
        #endregion

        #region SetColumnAssignmentEquals
        // TODO
        #endregion

        #region SetColumnAssignmentOperator
        // TODO
        #endregion

        #region SetVariableColumnAssignment
        // TODO
        #endregion

        #region SetVariableAssignment
        // TODO
        #endregion

        #region WithCommonTable
        // TODO
        #endregion

        #region OperatorQueryExpression
        // TODO
        #endregion

        #region QuerySpecification
        public virtual bool Action(QuerySpecification child)
        {
            if (child.SelectClause != null)
            {
                Scan(child.SelectClause);
            }
            if (child.FromClause != null)
            {
                _HTML += " FROM ";
                for (int i = 0; i < child.FromClause.Count; i++)
                {
                    if (i > 0)
                    {
                        _HTML += " ";
                    }
                    Scan(child.FromClause[i]);
                }
            }
            if (child.WhereClause != null)
            {
                _HTML += " WHERE ";
                Scan(child.WhereClause);
            }
            if (child.GroupByClause != null)
            {
                _HTML += " GROUP BY ";
                for (int i = 0; i < child.GroupByClause.Count; i++)
                {
                    if (i > 0)
                    {
                        _HTML += ",";
                    }
                    Scan(child.GroupByClause[i]);
                }
            }
            if (child.HavingClause != null)
            {
                _HTML += " HAVING ";
                Scan(child.HavingClause);
            }
            if (child.OrderByClause != null)
            {
                Stringifier str = new Stringifier();
                str.AddSpace();
                str.Add("ORDER BY");
                str.AddSpace();
                foreach (OrderByItem item in child.OrderByClause)
                {
                    str.Add(item);
                    if (item != child.OrderByClause.Last())
                    {
                        str.Add(",");
                        str.AddSpace();
                    }
                }
                _HTML += str.Statement;
            }
            return false;
        }
        #endregion

        #region SelectClause
        public virtual bool Action(SelectClause child)
        {
            _HTML += "SELECT ";
            if (child.IsDistinct)
            {
                _HTML += "DISTINCT ";
            }
            else
            {
                _HTML += "ALL ";
            }
            if (child.TopClause != null)
            {
                Scan(child.TopClause);
            }

            if (child.SelectItems != null)
            {
                for (int i = 0; i < child.SelectItems.Count; i++)
                {
                    if (i > 0)
                    {
                        _HTML += ",";
                    }
                    Scan(child.SelectItems[i]);
                }
            }
   
            return false;
        }
        #endregion

        #region TopClause
        public virtual bool Action(TopClause child)
        {
            _HTML += "TOP ";
            if (child.TopCount != null)
            {
                Scan(child.TopCount);
            }
            if (child.IsPercent)
            {
                _HTML += " PERCENT";
            }
            if (child.IsWithTies)
            {
                _HTML += " WITH TIES";
            }
            _HTML += " ";

            return false;
        }
        #endregion

        #region WildcardSelectItem
        // TODO
        #endregion

        #region TableWildcardSelectItem
        public virtual bool Action(TableWildcardSelectItem child)
        {
            if (child.Table != null)
            {
                for (int i = 0; i < child.Table.Identifiers.Count; i++)
                {
                    if (i > 0)
                    {
                        _HTML += ",";
                    }
                    Scan(child.Table.Identifiers[i]);
                }
            }
            _HTML += ".*";

            return false;
        }
        #endregion

        #region ExpressionSelectItem
        public virtual bool Action(ExpressionSelectItem child)
        {
            if (child.Expression != null)
            {
                Scan(child.Expression);
            }
            if (child.Alias != null)
            {
                _HTML += " AS ";
                Scan(child.Alias);
            }

            return false;
        }
        #endregion

        #region StringLiteralSelectAlias
        public virtual bool Action(StringLiteralSelectAlias child)
        {
            if (child.String != null)
            {
                Scan(child.String);
            }

            return false;
        }
        #endregion
        #region JoinedTableSource
        #endregion

        #endregion // TreeNodes

        #region ExpressionTreeNodes

        #region DbObjectExpression
        public virtual bool Action(DbObjectExpression child)
        {
            if (child.Value != null)
            {
                Scan(child.Value);
            }

            return false;
        }
        #endregion
        #region StringConstantExpression
        public virtual bool Action(StringConstantExpression child)
        {
            if (child.Value != null)
            {
                Scan(child.Value);
            }

            return false;
        }
        #endregion

        #endregion  // ExpressionTreeNodes
    }
}
