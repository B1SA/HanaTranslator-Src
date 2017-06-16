using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Translator
{
    public class CharacterCaseFixer : Scanner
    {
        enum ObjectType
        {
            COLUMN = 0,
            TABLE,
            PROCEDURE,
            INDEX,
            TYPE,
            VIEW,
            CONSTRAINT,
            TRIGGER,
            SCHEMA,
            OTHER
        };

        string[] ObjectTypeName = 
        {
            "Column",
            "Table",
            "Procedure",
            "Index",
            "Type",
            "View",
            "Constraint",
            "Trigger",
            "Schema",
            "Other"
        };

        #region IdentifierRequest
        class IdentifierRequest
        {
            public string mName = string.Empty;
            public List<string> mTables = new List<string>();
            public List<DbObject> mObjects = new List<DbObject>();

            public bool HaveSameName(DbObject node)
            {
                return mName.Equals(node.Identifiers.Last().Name, StringComparison.CurrentCultureIgnoreCase);
            }

            public void RemoveTable(string fullTableName)
            {
                if (string.IsNullOrEmpty(fullTableName))
                {
                    return;
                }

                foreach (string table in mTables)
                {
                    if (table.Equals(fullTableName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        mTables.Remove(table);
                        break;
                    }
                }
            }

            public static string GetSimpleFullName(DbObject node)
            {
                return node.Identifiers.Select(s => s.Name).DefaultIfEmpty().Aggregate((aggr, item) => aggr + "." + item);
            }

            public bool ContainsTable(string fullTablename)
            {
                foreach (string table in mTables)
                {
                    if (table == fullTablename)
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool EqualsTables(List<DbObjectTableSource> availTables)
            {
                if (mTables.Count != availTables.Count)
                {
                    return false;
                }

                foreach (string table in mTables)
                {
                    foreach (DbObjectTableSource tableObject in availTables)
                    {
                        string fullName = GetSimpleFullName(tableObject.DbObject);

                        if (fullName != table)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }
        #endregion

        #region ObjectNode
        class DbObjectNode
        {
            public string identifier;
            public ObjectType objectType;
            public DbObject grammarNode;

            public DbObjectNode(string name, ObjectType objType, DbObject node)
            {
                identifier = name;
                objectType = objType;
                grammarNode = node;
            }
        }
        #endregion

        public List<KeyValuePair<DbObject, List<DbObjectTableSource>>> mColTables = new List<KeyValuePair<DbObject, List<DbObjectTableSource>>>();
        private Stack<IList<DbObjectTableSource>> mTablesContext = new Stack<IList<DbObjectTableSource>>();
        private List<DbObjectNode> mTableNodes = new List<DbObjectNode>();
        private List<DbObjectNode> mProcedureNodes = new List<DbObjectNode>();
        private List<DbObjectNode> mViewNodes = new List<DbObjectNode>();
        private List<DbObject> mToSkip = new List<DbObject>();
        private List<KeyValuePair<DbObject, List<DbObjectTableSource>>> mIndexNodes = new List<KeyValuePair<DbObject, List<DbObjectTableSource>>>();
        private List<KeyValuePair<DbObject, List<DbObjectTableSource>>> mConstraintNodes = new List<KeyValuePair<DbObject, List<DbObjectTableSource>>>();
        private List<DbObjectNode> mTypeNodes = new List<DbObjectNode>();
        private List<DbObjectNode> mTriggerNodes = new List<DbObjectNode>();

        private List<GrammarNode> querySpecifications = new List<GrammarNode>();

        string mServer;
        string mSchema;
        string mUID;
        string mPasswd;

        public CharacterCaseFixer(string server, string schema, string uid, string passwd)
        {
            mServer = server;
            mSchema = schema;
            mUID = uid;
            mPasswd = passwd;
        }

        #region Handling available tables
        private void AddTablesContext(GrammarNode node)
        {
            dynamic dnode = (dynamic)node;
            mTablesContext.Push(GetAvailTables(dnode));
        }

        private void RemoveTablesContext(GrammarNode node)
        {
            mTablesContext.Pop();
        }

        private List<DbObjectTableSource> GetPosibleTables(string tableAlias)
        {
            List<DbObjectTableSource> ret = new List<DbObjectTableSource>();
            if (string.IsNullOrEmpty(tableAlias))
            {
                //it could be any of available tables in context
                foreach (IList<DbObjectTableSource> list in mTablesContext)
                {
                    if (list == null)
                    {
                        continue;
                    }

                    foreach (DbObjectTableSource ts in list)
                    {
                        ret.Add(ts);
                    }
                }
            }
            else
            {
                //find the nearest same table alias
                foreach (IList<DbObjectTableSource> list in mTablesContext)
                {
                    if (list == null)
                    {
                        continue;
                    }

                    foreach (DbObjectTableSource ts in list)
                    {
                        bool found = (ts.Alias != null && ts.Alias.Name.ToLower() == tableAlias.ToLower());
                        if (found == false)
                        {
                            string fullName = IdentifierRequest.GetSimpleFullName(ts.DbObject);
                            if (tableAlias.ToLower() == fullName.ToLower())
                            {
                                found = true;
                            }
                            else
                            {
                                if (ts.DbObject.Identifiers.Count > 1 && ts.DbObject.Identifiers[ts.DbObject.Identifiers.Count - 1].Name.ToLower() == tableAlias.ToLower())
                                {
                                    found = true;
                                }
                            }
                        }

                        if (found == true)
                        {
                            ret.Add(ts);
                            return ret;
                        }
                    }
                }
            }

            return ret;
        }

        private IList<DbObjectTableSource> GetAvailTables(GrammarNode node)
        {
            IList<DbObjectTableSource> ret = new List<DbObjectTableSource>();

            if (node is UpdateStatement)
            {
                UpdateStatement upd = (UpdateStatement)node;
                dynamic ts = upd.TableSource;
                AddAvailTable(ts, ret);
                AddAvailTables(upd.FromClause, ret);
            }

            if (node is DeleteStatement)
            {
                dynamic ts = ((DeleteStatement)node).Table;
                AddAvailTable(ts, ret);
            }

            if (node is InsertStatement)
            {
                InsertStatement insert = (InsertStatement)node;
                if (insert.InsertTarget is DbObjectInsertTarget)
                {
                    AddAvailTable(((DbObjectInsertTarget)insert.InsertTarget).TableSource, ret);
                }
            }

            if (node is CreateIndexStatement)
            {
                CreateIndexStatement stmt = (CreateIndexStatement)node;
                if (stmt.IndexTarget is DbObjectIndexTarget)
                {
                    AddAvailTable(stmt.IndexTarget.TableSource, ret);
                }
            }

            if (node is DropIndexStatement)
            {
                DropIndexStatement stmt = (DropIndexStatement)node;
                foreach (DropIndexAction action in stmt.Actions)
                {
                    if (action.TableSource != null)
                    {
                        AddAvailTable(action.TableSource, ret);
                    }
                }
            }

            if (node is AlterIndexStatement)
            {
                AlterIndexStatement stmt = (AlterIndexStatement)node;
                if (stmt.TableSource != null)
                {
                    // It will be removed in Modifier, so probably useless code
                    AddAvailTable(stmt.TableSource, ret);
                }
            }

            if (node is AlterTableStatement)
            {
                AlterTableStatement stmt = (AlterTableStatement)node;
                if (stmt.TableSource != null)
                {
                    AddAvailTable(stmt.TableSource, ret);
                }
            }

            if (node is DropTableStatement)
            {
                DropTableStatement stmt = (DropTableStatement)node;
                foreach (DbObjectTableSource tableSource in stmt.TableSources)
                {
                    if (tableSource != null)
                    {
                        AddAvailTable(tableSource, ret);
                    }
                }
            }

            TrimTables(ref ret);
            return (ret.Count == 0) ? null : ret;
        }

        private void TrimTables(ref IList<DbObjectTableSource> ret)
        {
            for (int i = 0; i < ret.Count; i++)
            {
                DbObjectTableSource table = ret[i];
                if (table.Alias != null)
                {
                    for (int j = 0; j < ret.Count; j++)
                    {
                        DbObjectTableSource source = ret[j];
                        for (int k = 0; k < source.DbObject.Identifiers.Count; k++)
                        {
                            Identifier iden = source.DbObject.Identifiers[k];
                            if (iden.Name.ToLower() == table.Alias.Name.ToLower())
                            {
                                ret.Remove(source);
                                j--;
                                i--;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void TrimTables(List<DbObjectNode> mTableNodes)
        {
            foreach (KeyValuePair<DbObject, List<DbObjectTableSource>> source in mColTables)
            {
                foreach (DbObjectTableSource alias in source.Value)
                {
                    if (alias.Alias != null)
                    {
                        foreach (DbObjectNode obj in mTableNodes)
                        {
                            if (obj.identifier == alias.Alias.Name)
                            {
                                mTableNodes.RemoveAt(mTableNodes.IndexOf(obj));
                                return;
                            }
                        }
                    }
                }
            }
        }

        private bool AddAvailTables(IList<TableSource> fromClause, IList<DbObjectTableSource> list)
        {
            if (fromClause == null)
            {
                return false;
            }

            foreach (TableSource tableSource in fromClause)
            {
                dynamic ts = (dynamic)tableSource;
                AddAvailTable(ts, list);
            }

            return true;
        }

        private bool AddAvailTable(GrammarNode node, IList<DbObjectTableSource> list)
        {
            //empty place holder for type we don't support
            return false;
        }

        private bool AddAvailTable(DbObjectTableSource node, IList<DbObjectTableSource> list)
        {
            list.Add(node);
            return true;
        }

        private bool AddAvailTable(NestedParensTableSource node, IList<DbObjectTableSource> list)
        {
            dynamic ts = node.TableSource;
            return AddAvailTable(ts, list);
        }

        private bool AddAvailTable(JoinedTableSource joinedTable, IList<DbObjectTableSource> list)
        {
            dynamic rightTable = joinedTable.RightTableSource;
            dynamic leftTable = joinedTable.LeftTableSource;
            AddAvailTable(rightTable, list);
            AddAvailTable(leftTable, list);

            return true;
        }

        private IList<DbObjectTableSource> GetAvailTables(QuerySpecification query)
        {
            IList<DbObjectTableSource> ret = new List<DbObjectTableSource>();
            AddAvailTables(query.FromClause, ret);

            return (ret.Count == 0) ? null : ret;
        }

        private IList<DbObjectTableSource> GetAvailTables(OperatorQueryExpression query)
        {
            return null;
        }

        #endregion

        #region AlterIndexStatement
        public virtual bool Action(AlterIndexStatement node)
        {
            AddTablesContext(node);
            FixIdentifier(new DbObject(node.Index), ObjectType.INDEX, node.Index);
            return false;
        }
        #endregion

        #region DropIndexStatement
        public virtual bool Action(DropIndexStatement node)
        {
            AddTablesContext(node);
            if (node.Actions.Count > 0)
            {
                FixIdentifier(new DbObject(node.Actions[0].Index), ObjectType.INDEX, node.Actions[0].Index);
            }
            return false;
        }
        #endregion

        public virtual bool Action(DropViewStatement stmt)
        {
            if (stmt.Views.Count > 0)
            {
                FixIdentifiers(stmt.Views[0], ObjectType.VIEW, stmt.Views[0].Identifiers);
            }
            return false;
        }

        public virtual bool Action(AlterViewStatement stmt)
        {
            FixIdentifiers(stmt.Name, ObjectType.VIEW, stmt.Name.Identifiers);
            return false;
        }

        public virtual bool Action(CreateViewStatement stmt)
        {
            FixIdentifiers(stmt.Name, ObjectType.OTHER, stmt.Name.Identifiers);
            mToSkip.Add(stmt.Name);
            return true;
        }

        public virtual bool Action(CreateDmlTriggerStatement stmt)
        {
            FixIdentifiers(stmt.Name, ObjectType.OTHER, stmt.Name.Identifiers);
            FixIdentifiers(stmt.Table, ObjectType.TABLE, stmt.Table.Identifiers);
            mToSkip.Add(stmt.Name);
            mToSkip.Add(stmt.Table);
            return true;
        }

        public virtual bool Action(DropTriggerStatement stmt)
        {
            FixIdentifiers(stmt.Name, ObjectType.TRIGGER, stmt.Name.Identifiers);
            return false;
        }

        public virtual bool Action(CreateTableTypeStatement node)
        {
            FixIdentifiers(node.Name, ObjectType.OTHER, node.Name.Identifiers);
            mToSkip.Add(node.Name);
            return true;
        }

        public virtual bool Action(CreateBaseTypeStatement node)
        {
            FixIdentifiers(node.Name, ObjectType.TYPE, node.Name.Identifiers);
            return false;
        }

        public virtual bool Action(DropTypeStatement node)
        {
            FixIdentifiers(node.Name, ObjectType.TYPE, node.Name.Identifiers);
            return false;
        }

        public virtual bool Action(SetIdentityInsertStatement stmt)
        {
            FixIdentifiers(stmt.DBObject, ObjectType.TABLE, stmt.DBObject.Identifiers);
            return false;
        }

        public virtual bool Action(IdentifierSelectAlias alias)
        {
            alias.Identifier.Type = IdentifierType.Quoted;
            return false;
        }

        public virtual bool Action(OrderedColumn col)
        {
            col.Name.Type = IdentifierType.Quoted;
            return false;
        }

        public virtual bool Action(ColumnDefinition node)
        {
            // There is only one Identifier, and therefore there should be no problem with removing
            FixIdentifier(new DbObject(node.Name), ObjectType.OTHER, node.Name);
            return false;
        }

        public virtual bool Action(ComputedColumnDefinition node)
        {
            FixIdentifier(new DbObject(node.Name), ObjectType.OTHER, node.Name);
            return false;
        }

        public virtual bool Action(CreateTableStatement node)
        {
            FixIdentifiers(node.Name, ObjectType.OTHER, node.Name.Identifiers);
            mToSkip.Add(node.Name);
            return true;
        }

        public virtual bool Action(ForeignKeyColumnConstraint node)
        {
            //todo: this should be cleared in modifier, imho
            // Unsupported grammar node on HANA - nothing to check
            return false;
        }

        public virtual bool Action(ForeignKeyTableConstraint node)
        {
            //todo: this should be cleared in modifier, imho
            // Unsupported grammar node on HANA - nothing to check
            return false;
        }

        public virtual bool Action(PrimaryKeyTableConstraint node)
        {
            // IdentifierObject.OTHER is used, no correction for ADD/CREATE
            if (node.Name != null)
            {
                FixIdentifier(new DbObject(node.Name), ObjectType.OTHER, node.Name);
            }
            foreach (OrderedColumn oc in node.Columns)
            {
                FixIdentifier(new DbObject(oc.Name), ObjectType.OTHER, oc.Name);
            }
            return false;
        }

        public virtual bool Action(CreateIndexStatement stmt)
        {
            AddTablesContext(stmt);

            FixIdentifier(new DbObject(stmt.Name), ObjectType.OTHER, stmt.Name);
            FixIdentifiers(stmt.IndexTarget.TableSource.DbObject, ObjectType.TABLE, stmt.IndexTarget.TableSource.DbObject.Identifiers);
            foreach (OrderedColumn col in stmt.IndexColumns)
            {
                FixIdentifier(new DbObject(col.Name), ObjectType.COLUMN, col.Name);
            }
            return false;
        }

        public virtual bool PostAction(CreateIndexStatement stmt)
        {
            RemoveTablesContext(stmt);
            return false;
        }

        public virtual bool Action(InsertStatement node)
        {
            AddTablesContext(node);

            if (node.ColumnList != null)
            {
                foreach (Identifier column in node.ColumnList)
                {
                    FixIdentifier(new DbObject(column), ObjectType.COLUMN, column);
                }
            }

            return true;
        }

        public virtual bool Action(CreateProcedureStatement node)
        {
            FixIdentifiers(node.Name, ObjectType.OTHER, node.Name.Identifiers);
            mToSkip.Add(node.Name);
            return true;
        }

        public virtual bool Action(DropProcedureStatement node)
        {
            if (node.Names.Count > 0)
            {
                FixIdentifiers(node.Names[0], ObjectType.PROCEDURE, node.Names[0].Identifiers);
            }

            return false;
        }

        public virtual bool Action(DbObjectInsertTarget node)
        {
            FixIdentifiers(node.TableSource.DbObject, ObjectType.TABLE, node.TableSource.DbObject.Identifiers);
            return false;
        }

        #region AlterTableStatement
        public virtual bool Action(AlterTableStatement node)
        {
            AddTablesContext(node);
            FixIdentifiers(node.TableSource.DbObject, ObjectType.TABLE, node.TableSource.DbObject.Identifiers);

            dynamic dynAction = node.Action;
            Action(dynAction);

            return false;
        }

        public virtual bool PostAction(AlterTableStatement node)
        {
            RemoveTablesContext(node);
            return false;
        }

        public virtual bool Action(AlterColumnAddDropAlterTableAction action)
        {
            if (action.Action == AddOrDrop.Drop)
            {
                FixIdentifier(new DbObject(action.Name), ObjectType.COLUMN, action.Name);
            }
            else
            {
                FixIdentifier(new DbObject(action.Name), ObjectType.OTHER, action.Name);
            }
            return false;
        }

        public virtual bool Action(AlterColumnDefineAlterTableAction action)
        {
            FixIdentifier(new DbObject(action.Name), ObjectType.OTHER, action.Name);
            return false;
        }

        public virtual bool Action(DropColumnAlterTableDefinition action)
        {
            FixIdentifier(new DbObject(action.Name), ObjectType.COLUMN, action.Name);
            return false;
        }

        public virtual bool Action(DropConstraintAlterTableDefinition action)
        {
            FixIdentifier(new DbObject(action.Name), ObjectType.CONSTRAINT, action.Name);
            return false;
        }

        public virtual bool Action(AddAlterTableAction node)
        {
            foreach (CreateTableDefinition cd in node.Definitions)
            {
                if (cd is ColumnDefinition)
                {
                    Action(cd as ColumnDefinition);
                }

                if (cd is PrimaryKeyTableConstraint)
                {
                    Action(cd as PrimaryKeyTableConstraint);
                }
            }
            return false;
        }

        public virtual bool Action(DropAlterTableAction node)
        {
            foreach (DropAlterTableDefinition dropDef in node.Definitions)
            {
                dynamic dropColDef = dropDef;
                Action(dropColDef);
            }
            return false;
        }
        #endregion

        public virtual bool Action(DbObjectExecModuleName node)
        {
            FixIdentifiers(node.Name, ObjectType.PROCEDURE, node.Name.Identifiers);
            return false;
        }

        #region Statement
        public virtual bool Action(Statement node)
        {
            if (node is UpdateStatement || node is DeleteStatement || node is DropTableStatement)
            {
                AddTablesContext(node);
            }

            return true;
        }

        public virtual bool PostAction(Statement node)
        {
            if (node is UpdateStatement || node is DeleteStatement || node is DropTableStatement || node is DropIndexStatement || node is AlterIndexStatement || node is InsertStatement)
            {
                RemoveTablesContext(node);
            }

            return true;
        }
        #endregion

        public virtual bool Action(QuerySpecification node)
        {
            //To cover nested subqueries in statements
            //For 'standard' queries we use Action for SelectStatement
            if (querySpecifications.Contains(node) == false)
            {
                AddTablesContext(node);
            }
            return true;
        }

        public virtual bool PostAction(QuerySpecification node)
        {
            if (querySpecifications.Contains(node) == false)
            {
                //Remove table context only for nested queries
                //For 'standard' queries we use PostAction for SelectStatement
                RemoveTablesContext(node);
            }
            return true;
        }

        public virtual bool Action(SelectStatement node)
        {
            //Add context table on SelectStatement level to cover Group by and Order by

            AddTablesContext(node.QueryExpression);
            querySpecifications.Add(node.QueryExpression);
            return true;
        }

        public virtual bool PostAction(SelectStatement node)
        {
            querySpecifications.Remove(node.QueryExpression);
            RemoveTablesContext(node.QueryExpression);
            return true;
        }

        public virtual bool Action(SqlStartStatement node)
        {
            return false;
        }

        public virtual bool Action(DbObjectTableSource source)
        {
            FixIdentifiers(source.DbObject, ObjectType.TABLE, source.DbObject.Identifiers);
            return false;
        }

        public virtual bool Action(GenericDataType type)
        {
            if (type.Schema != null)
            {
                type.Schema.Type = IdentifierType.Quoted;
                if (type.Name != null)
                {
                    type.Name.Type = IdentifierType.Quoted;
                }
            }
            else
            {
                type.Name.Type = IdentifierType.Plain;
            }
            return false;
        }

        public virtual bool Action(DbObject dbObject)
        {
            if (mToSkip.Contains(dbObject) == false)
            {
                FixIdentifiers(dbObject, ObjectType.COLUMN, dbObject.Identifiers);
            }
            return false;
        }

        public virtual bool Action(DbObjectExpression expression)
        {
            return Action(expression.Value);
        }

        #region Handle DBObject to Node list for scanning

        List<KeyValuePair<DbObject, List<DbObjectTableSource>>> GetDbObjectTableNodeListByType(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.COLUMN:
                    return mColTables;
                case ObjectType.INDEX:
                    return mIndexNodes;

                case ObjectType.CONSTRAINT:
                    return mConstraintNodes;
            }
            return null;
        }

        List<DbObjectNode> GetDbObjectNodeListByType(ObjectType type)
        {
            switch (type)
            {
                case ObjectType.TRIGGER:
                    return mTriggerNodes;

                case ObjectType.TABLE:
                    return mTableNodes;

                case ObjectType.TYPE:
                    return mTypeNodes;

                case ObjectType.VIEW:
                    return mViewNodes;

                case ObjectType.PROCEDURE:
                    return mProcedureNodes;
            }

            return null;
        }

        bool CheckAndQuoteIdentifiers(IList<Identifier> identifiers, ObjectType type, string objName, DbObject node)
        {
            if (identifiers.DefaultIfEmpty().Last() != null)
            {
                if (Config.UseCaseFixer == false || type == ObjectType.OTHER)
                {
                    QuoteLastIdentifierIfNeeded(identifiers);
                }
            }
            else
            {
                AddNote(node, Note.DEBUG_CASEFIXER, "Object " + type.ToString() + " '" + objName + "' has no identifiers");
                return false;
            }

            return true;
        }

        void AddDbObjectToNodesList(IList<Identifier> identifiers, ObjectType type, string objName, DbObject node)
        {
            if (CheckAndQuoteIdentifiers(identifiers, type, objName, node))
            {
                List<DbObjectNode> list = GetDbObjectNodeListByType(type);

                if (list != null)
                {
                    DbObjectNode objNode = new DbObjectNode(objName, ObjectType.TYPE, node as DbObject);
                    list.Add(objNode);
                }
            }
        }

        void AddDbObjectToTableNodesList(IList<Identifier> identifiers, ObjectType type, string objName, DbObject node)
        {
            if (CheckAndQuoteIdentifiers(identifiers, type, objName, node))
            {
                List<KeyValuePair<DbObject, List<DbObjectTableSource>>> list = GetDbObjectTableNodeListByType(type);
                if (list != null)
                {
                    string table = (objName.LastIndexOf('.') > 0) ? objName.Substring(0, objName.LastIndexOf('.')) : string.Empty;
                    list.Add(new KeyValuePair<DbObject, List<DbObjectTableSource>>(node, GetPosibleTables(table)));
                }
            }
        }

        #endregion

        #region FixIdentifier
        void FixIdentifier(DbObject node, ObjectType type, Identifier identifier)
        {
            FixIdentifiers(node, type, new List<Identifier> { identifier });
        }

        void FixIdentifiers(DbObject node, ObjectType type, IList<Identifier> identifiers)
        {
            string objName = string.Empty;
            for (int j = 0; j < identifiers.Count; j++)
            {
                if (identifiers[j].Name == "dbo" || identifiers[j].Name == "[dbo]" || string.IsNullOrEmpty(identifiers[j].Name))
                {
                    identifiers.RemoveAt(j);
                    j--;
                    continue;
                }
                if (identifiers[j].Type == IdentifierType.Bracketed)
                    identifiers[j].Type = IdentifierType.Quoted;
                objName += identifiers[j].Name + ".";
            }

            objName = objName.TrimEnd('.');

            switch (type)
            {
                case ObjectType.COLUMN:
                case ObjectType.CONSTRAINT:
                case ObjectType.INDEX:
                    AddDbObjectToTableNodesList(identifiers, type, objName, node);
                    break;

                case ObjectType.TYPE:
                case ObjectType.VIEW:
                case ObjectType.TRIGGER:
                case ObjectType.PROCEDURE:
                case ObjectType.OTHER:
                    AddDbObjectToNodesList(identifiers, type, objName, node);
                    break;

                case ObjectType.TABLE:
                    if (objName.ToUpper() != "DUMMY")
                    {
                        AddDbObjectToNodesList(identifiers, type, objName, node);
                        TrimTables(mTableNodes);
                    }
                    break;
            }

            AddNote(node, Note.DEBUG_CASEFIXER, "Object " + type.ToString() + " '" + objName + "' cought");
        }

        private bool QuoteLastIdentifierIfNeeded(IList<Identifier> identifiers)
        {
            Identifier toFix = identifiers.DefaultIfEmpty().Last();
            if (toFix.Name.ToUpperInvariant() != toFix.Name)
            {
                toFix.Type = IdentifierType.Quoted;
                return true;
            }

            return false;
        }

        private void AddNote(DbObject node, string id, string note)
        {
            if (node != null)
            {
                if (node.Identifiers.Last() != null)
                {
                    node.Identifiers.Last().AddNote(id, note);
                }
                else
                {
                    node.AddNote(id, note);
                }
            }
        }

        public void ClearIdentifiersTables()
        {
            mTablesContext.Clear();
            mColTables.Clear();
            mProcedureNodes.Clear();
            mViewNodes.Clear();
            mIndexNodes.Clear();
            mTableNodes.Clear();
            mConstraintNodes.Clear();
            mTypeNodes.Clear();
            mTriggerNodes.Clear();

            mToSkip.Clear();
        }

        public string CorrectIdentifiers()
        {
            string res = string.Empty;
            DbUtil util = DbUtil.GetSingleton(mServer, mSchema, mUID, mPasswd);

            if (util.IsConnected)
            {
                List<IdentifierRequest> uniqueList = CreateUniqueRequestsList(mColTables);
                ScanIdentifiers(util, uniqueList, ObjectType.COLUMN);

                ScanObjects(util, ObjectType.TYPE, mTypeNodes);
                ScanObjects(util, ObjectType.TABLE, mTableNodes);
                ScanObjects(util, ObjectType.PROCEDURE, mProcedureNodes);
                ScanObjects(util, ObjectType.VIEW, mViewNodes);
                ScanObjects(util, ObjectType.TRIGGER, mTriggerNodes);

                uniqueList = CreateUniqueRequestsList(mIndexNodes);
                ScanIdentifiers(util, uniqueList, ObjectType.INDEX);

                uniqueList = CreateUniqueRequestsList(mConstraintNodes);
                ScanIdentifiers(util, uniqueList, ObjectType.CONSTRAINT);
            }
            else
            {
                res = util.ConnectionInfo;
            }

            return res;
        }

        private Dictionary<string, HashSet<string>> GroupObjectBySchema(List<DbObjectNode> objsToGroup)
        {
            Dictionary<string, HashSet<string>> ret = new Dictionary<string, HashSet<string>>();

            //empty is default schema
            string upperSchema = mSchema.ToUpper();
            ret[upperSchema] = new HashSet<string>();

            foreach (DbObjectNode objNode in objsToGroup)
            {
                //already processed
                if (string.IsNullOrEmpty(objNode.identifier) == false)
                {
                    string[] idens = objNode.identifier.Split('.');

                    if (idens.Length == 2)
                    {
                        if (ret.ContainsKey(idens[0].ToUpper()) == false)
                        {
                            ret[idens[0].ToUpper()] = new HashSet<string>();
                        }

                        ret[idens[0].ToUpper()].Add(idens[1].ToUpper());
                    }
                    else
                    {
                        ret[upperSchema].Add(idens[0].ToUpper());
                    }
                }
            }

            return ret;
        }

        private void ScanObjects(DbUtil util, ObjectType objType, List<DbObjectNode> objToCheck)
        {
            if (objToCheck.Count == 0)
            {
                return;
            }

            Dictionary<string, HashSet<string>> groupedIdens = GroupObjectBySchema(objToCheck);

            foreach (KeyValuePair<string, HashSet<string>> pair in groupedIdens)
            {
                StatusReporter.ReportProgress();
                util.LoadObjectNames(pair.Key, ObjectTypeName[(int)objType].ToUpper(), pair.Value);
            }

            foreach (DbObjectNode tn in objToCheck)
            {
                // some could be already processed durign checking the columns
                if (string.IsNullOrEmpty(tn.identifier) == false)
                {
                    string[] names = util.GetCachedObjectName(tn.identifier, ObjectTypeName[(int)objType].ToUpper());

                    if (names[0] == null)
                    {
                        //no object found
                        AddNote(tn.grammarNode, Note.CASEFIXER, String.Format(ResStr.CF_OBJECT_NOT_FOUND, ObjectTypeName[(int)objType], tn.identifier));
                        tn.grammarNode.Identifiers.Last().Type = IdentifierType.Quoted;
                    }
                    else
                    {
                        string clearIden = tn.identifier;
                        foreach (DbObjectNode tn2 in objToCheck)
                        {
                            if (tn2.identifier == clearIden)
                            {
                                Identifier iden = tn2.grammarNode.Identifiers[0];

                                if (tn2.grammarNode.Identifiers.Count == 2)
                                {
                                    //cover schema name
                                    if (iden.Name != names[0])
                                    {
                                        AddNote(tn2.grammarNode, Note.CASEFIXER, String.Format(ResStr.CF_OBJECT_NAME_FIXED, ObjectTypeName[(int)ObjectType.SCHEMA], iden.Name, names[0]));
                                        iden.Name = names[0];
                                    }

                                    iden = tn2.grammarNode.Identifiers[1];
                                }

                                //object name
                                if (iden.Name != names[1])
                                {
                                    AddNote(tn2.grammarNode, Note.CASEFIXER, String.Format(ResStr.CF_OBJECT_NAME_FIXED, ObjectTypeName[(int)objType], iden.Name, names[1]));
                                    iden.Name = names[1];

                                    if (iden.Name.ToUpper() != iden.Name)
                                    {
                                        //case sensitive table name - change identifier to quoted
                                        iden.Type = IdentifierType.Quoted;
                                    }
                                }
                                else if (iden.Type == IdentifierType.Plain && iden.Name.ToUpper() != iden.Name)
                                {
                                    iden.Type = IdentifierType.Quoted;
                                }

                                tn2.identifier = string.Empty;
                            }
                        }
                    }
                }
            }
        }

        private void ScanIdentifiers(DbUtil util, List<IdentifierRequest> listToVerify, ObjectType objType)
        {
            if (listToVerify.Count == 0)
            {
                return;
            }

            List<string> tableList = new List<string>();
            foreach (IdentifierRequest ir in listToVerify)
            {
                foreach (string table in ir.mTables)
                {
                    if (tableList.Find(s => s == table) == null)
                    {
                        tableList.Add(table);
                    }
                }
            }
            if (objType == ObjectType.INDEX)
            {
                tableList.Add(string.Empty);
            }

            List<IdentifierRequest> irBatch = new List<IdentifierRequest>();
            List<string> nameBatch = new List<string>();

            foreach (string table in tableList)
            {
                irBatch.Clear();
                nameBatch.Clear();

                //collect columns for one table
                foreach (IdentifierRequest ir in listToVerify)
                {
                    if (string.IsNullOrEmpty(ir.mName) == false)
                    {
                        if ((string.IsNullOrEmpty(table) && ir.mTables.Count == 0)
                            || string.IsNullOrEmpty(table) == false && ir.ContainsTable(table))
                        {
                            irBatch.Add(ir);
                            nameBatch.Add(ir.mName);
                        }
                    }
                }

                if (irBatch.Count == 0)
                {
                    //Table without column to solve
                    continue;
                }

                SortedDictionary<string, List<string>> nameTable = new SortedDictionary<string, List<string>>();
                List<string> ret = null;

                if (objType == ObjectType.COLUMN)
                {
                    StatusReporter.ReportProgress();
                    ret = util.GetColumnsNames(ref nameTable, table, nameBatch);
                }
                else
                {
                    StatusReporter.ReportProgress();
                    ret = util.GetIndexesNames(ref nameTable, table, nameBatch);
                }

                FixNames(ret, table, irBatch, objType);
            }

            //names which left are not found in DB
            foreach (IdentifierRequest ir in listToVerify)
            {
                if (string.IsNullOrEmpty(ir.mName) == false)
                {
                    //not found in DB 
                    foreach (DbObject dbObj in ir.mObjects)
                    {

                        switch (objType)
                        {
                            case ObjectType.COLUMN:
                                AddNote(dbObj, Note.CASEFIXER, String.Format(ResStr.CF_COLUMN_NOT_FOUND, ir.mName));
                                break;

                            case ObjectType.INDEX:
                                AddNote(dbObj, Note.CASEFIXER, String.Format(ResStr.CF_OBJECT_NOT_FOUND, ObjectTypeName[(int)objType], ir.mName));
                                break;

                            case ObjectType.CONSTRAINT:
                                AddNote(dbObj, Note.CASEFIXER, String.Format(ResStr.CF_CONSTRAINT_NOT_FOUND, ir.mName));
                                break;
                        }
                        QuoteLastIdentifierIfNeeded(dbObj.Identifiers);
                    }
                }
            }
        }

        private void FixNames(List<string> nameTable, string currTable, List<IdentifierRequest> irBatch, ObjectType objType)
        {
            foreach (string str in nameTable)
            {
                int idx = str.LastIndexOf(".");
                string tableName = str.Substring(0, idx);
                string name = str.Substring(idx + 1);

                foreach (IdentifierRequest ir in irBatch)
                {
                    if (ir.mName.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        FixDbObject(ir, str, objType);
                        ir.mTables.Clear();
                        ir.mName = string.Empty;
                    }
                    else
                    {
                        ir.RemoveTable(tableName);
                    }
                }
            }

            if (nameTable.Count == 0)
            {
                //invalid table?
                foreach (IdentifierRequest ir in irBatch)
                {
                    ir.RemoveTable(currTable);
                }
            }
        }

        private void FixDbObject(IdentifierRequest ir, string fullName, ObjectType objType)
        {
            string[] names = fullName.Split('.');

            foreach (DbObject dbObj in ir.mObjects)
            {
                if (dbObj != null)
                {
                    int i = 0;
                    int idx = dbObj.Identifiers.Count - 1;

                    //table name (could be changed too
                    foreach (string newName in names.Reverse())
                    {
                        if (dbObj.Identifiers[idx].Name != newName && dbObj.Identifiers[idx].Name.Equals(newName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (i == 0)
                            {
                                if (objType == ObjectType.COLUMN)
                                {
                                    //column name
                                    AddNote(dbObj, Note.CASEFIXER, String.Format(ResStr.CF_COLUMN_NAME_FIXED, dbObj.Identifiers[idx].Name, newName));
                                }
                                else
                                {
                                    //index & constraint name
                                    AddNote(dbObj, Note.CASEFIXER, String.Format(ResStr.CF_OBJECT_NAME_FIXED, ObjectTypeName[(int)objType], dbObj.Identifiers[idx].Name, newName));
                                }
                            }

                            if (i == 1)
                            {
                                //table name
                                AddNote(dbObj, Note.CASEFIXER, String.Format(ResStr.CF_OBJECT_NAME_FIXED, ObjectTypeName[(int)ObjectType.TABLE], dbObj.Identifiers[idx].Name, newName));
                            }

                            dbObj.Identifiers[idx].Name = newName;
                        }

                        if (dbObj.Identifiers[idx].Name == newName && newName != newName.ToUpper())
                        {
                            //we have to quote the name
                            dbObj.Identifiers[idx].Type = IdentifierType.Quoted;
                        }

                        ++i;
                        --idx;

                        if (idx < 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        private List<IdentifierRequest> CreateUniqueRequestsList(List<KeyValuePair<DbObject, List<DbObjectTableSource>>> list)
        {
            List<IdentifierRequest> uniqueRequests = new List<IdentifierRequest>();

            foreach (KeyValuePair<DbObject, List<DbObjectTableSource>> record in list)
            {
                DbObject node = record.Key;
                if (node == null)
                {
                    continue;
                }

                List<DbObjectTableSource> availTables = record.Value;
                IdentifierRequest dbRec = null;
                bool addNew = true;

                foreach (IdentifierRequest rq in uniqueRequests)
                {
                    if (rq.HaveSameName(node))
                    {
                        // column name already in list, check the tables
                        if (rq.EqualsTables(availTables))
                        {
                            rq.mObjects.Add(node);
                            addNew = false;
                        }
                    }
                }

                if (addNew)
                {
                    dbRec = new IdentifierRequest();
                    dbRec.mName = node.Identifiers.Last().Name;

                    foreach (DbObjectTableSource tSource in availTables)
                    {
                        dbRec.mTables.Add(IdentifierRequest.GetSimpleFullName(tSource.DbObject));
                    }

                    dbRec.mObjects.Add(node);
                    uniqueRequests.Add(dbRec);
                }
            }

            return uniqueRequests;
        }
        #endregion
    }
}
