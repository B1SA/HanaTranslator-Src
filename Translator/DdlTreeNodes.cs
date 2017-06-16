using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    #region CreateTable
    #region CreateTableStatement
    public class CreateTableStatement : Statement
    {
        public DbObject Name { get; set; }
        public bool IsFileTable { get; set; }
        public IList<CreateTableDefinition> Definitions { get; set; }
        public PartitionOrFileGroup OnClause { get; set; }
        public PartitionOrFileGroup TextImageOnClause { get; set; }
        public PartitionOrFileGroup FileStreamOnClause { get; set; }
        public IList<CreateTableOption> TableOptions { get; set; }
        public QuerySpecification AsQuerySpecification { get; set; }

        public CreateTableStatement(DbObject name, bool isFileTable, IList<CreateTableDefinition> definitions, PartitionOrFileGroup onClause,
            PartitionOrFileGroup textImageOnClause, PartitionOrFileGroup fileStreamOnClause, IList<CreateTableOption> tableOptions, QuerySpecification asQuerySpecification = null)
        {
            Name = name;
            IsFileTable = isFileTable;
            Definitions = definitions;
            OnClause = onClause;
            TextImageOnClause = textImageOnClause;
            FileStreamOnClause = fileStreamOnClause;
            TableOptions = tableOptions;
            AsQuerySpecification = asQuerySpecification;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("CREATE COLUMN TABLE");
            asm.AddSpace();
            if (Name != null)
            {
                asm.Add(Name);
            }
            asm.IncreaseIndentation();
            if (IsFileTable)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_AS_FILETABLE);
            }
            if (AsQuerySpecification != null)
            {
                asm.AddSpace();
                asm.AddToken("AS");
                asm.AddSpace();
                asm.AddToken("(");
                asm.Add(AsQuerySpecification);
                asm.AddToken(")");
                asm.DecreaseIndentation();
                asm.End(this);
                return;
            }
            if (Definitions != null)
            {
                asm.AddSpace();
                asm.AddToken("(");
                if (Definitions.Count != 0)
                {
                    foreach (CreateTableDefinition def in Definitions)
                    {
                        if (def is TableConstraint)
                        {
                            asm.AddSpace();
                            asm.AddToken("CONSTRAINT");
                            asm.AddSpace();
                        }
                        asm.Add(def);
                        if (def != Definitions.Last())
                        {
                            asm.AddToken(",");
                            asm.AddSpace();
                            asm.Breakable();
                        }
                    }
                }
                else
                {
                    AddNote(Note.STRINGIFIER, ResStr.WARN_NO_COLUMNS_IN_CREATE);
                }
                asm.AddToken(")");
            }
            if (OnClause != null)
            {
                AddNote(Note.STRINGIFIER, String.Format(ResStr.CLAUSE_NOT_IN_CREATE_TABLE, "ON"));
            }
            if (TextImageOnClause != null)
            {
                AddNote(Note.STRINGIFIER, String.Format(ResStr.CLAUSE_NOT_IN_CREATE_TABLE, "TEXTIMAGE_ON"));
            }
            if (FileStreamOnClause != null)
            {
                AddNote(Note.STRINGIFIER, String.Format(ResStr.CLAUSE_NOT_IN_CREATE_TABLE, "FILESTREAM_ON"));
            }
            if (TableOptions != null)
            {
                AddNote(Note.STRINGIFIER, String.Format(ResStr.CLAUSE_NOT_IN_CREATE_TABLE, "WITH"));
            }
            asm.DecreaseIndentation();
            asm.End(this);
        }
    }
    #endregion

    #region CreateTableDefinition
    abstract public class CreateTableDefinition : GrammarNode
    {
    }

    #region ColumnDefinition
    public class ColumnDefinition : CreateTableDefinition
    {
        public Identifier Name { get; set; }
        public DataType DataType { get; set; }
        public IList<ColumnDefinitionModifier> Modifiers { get; set; }

        public ColumnDefinition(Identifier name, DataType dataType, IList<ColumnDefinitionModifier> modifiers)
        {
            Name = name;
            DataType = dataType;
            Modifiers = modifiers;
        }
  
        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Name);
            asm.AddSpace();
            if (DataType != null)
            {
                asm.Add(DataType);
            }
            if (Modifiers != null)
            {
                asm.AddSpace();
                foreach (ColumnDefinitionModifier mod in Modifiers)
                {
                    asm.Add(mod);
                    if (mod != Modifiers.Last())
                    {
                        asm.AddSpace();
                    }
                    
                }
            }
            asm.End(this);
        }
    }
    #endregion
    #endregion

    #region ColumnDefinitionModifier
    abstract public class ColumnDefinitionModifier : GrammarNode
    {
    }

    #region SimpleColumnDefinitionModifier
    public class SimpleColumnDefinitionModifier : ColumnDefinitionModifier
    {
        public SimpleColumnDefinitionModifierType Type { get; set; }

        public SimpleColumnDefinitionModifier(SimpleColumnDefinitionModifierType type)
        {
            Type = type;
        }
   
        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Type)
            {
                case SimpleColumnDefinitionModifierType.FileStream:
                    AddNote(Note.STRINGIFIER, ResStr.NO_FILESTREAM_CLAUSE);
                    break;
                case SimpleColumnDefinitionModifierType.Null:
                    asm.AddToken("NULL");
                    break;
                case SimpleColumnDefinitionModifierType.NotNull:
                    asm.AddToken("NOT NULL");
                    break;
                case SimpleColumnDefinitionModifierType.RowGuidCol:
                    AddNote(Note.STRINGIFIER, ResStr.NO_ROWGUIDCOL_CLAUSE);
                    break;
                case SimpleColumnDefinitionModifierType.Sparse:
                    AddNote(Note.STRINGIFIER, ResStr.NO_SPARSE_CLAUSE);
                    break;
            }
            asm.End(this);
        }
    }

    public enum SimpleColumnDefinitionModifierType
    {
        FileStream, Null, NotNull, RowGuidCol, Sparse
    }
    #endregion

    #region CollationColumnDefinitionModifier
    public class CollationColumnDefinitionModifier : ColumnDefinitionModifier
    {
        public Identifier Name { get; set; }

        public CollationColumnDefinitionModifier(Identifier name)
        {
            Name = name;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            AddNote(Note.STRINGIFIER, ResStr.NO_COLLATE_CLAUSE);
            asm.End(this);
        }
    }
    #endregion

    #region IdentityColumnDefinitionModifier
    public class IdentityColumnDefinitionModifier : ColumnDefinitionModifier
    {
        public decimal Seed { get; set; }
        public decimal Increment { get; set; }
        public bool NotForReplication { get; set; }

        public IdentityColumnDefinitionModifier(decimal seed, decimal increment, bool notForReplication)
        {
            Seed = seed;
            Increment = increment;
            NotForReplication = notForReplication;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            AddNote(Note.STRINGIFIER, ResStr.NO_IDENTITY_CLAUSE);
            asm.End(this);
        }
    }
    #endregion

    #region ConstraintColumnDefinitionModifier
    public class ConstraintColumnDefinitionModifier : ColumnDefinitionModifier
    {
        public ColumnConstraint Constraint { get; set; }

        public ConstraintColumnDefinitionModifier(ColumnConstraint constraint)
        {
            Constraint = constraint;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Constraint);
            asm.End(this);
        }
    }
    #endregion

    #region ComputedColumnDefinition
    public class ComputedColumnDefinition : CreateTableDefinition
    {
        public Identifier Name { get; set; }
        public Expression Expression { get; set; }
        public ComputedColumnPersistenceType Type { get; set; }
        public ColumnConstraint Constraint { get; set; }

        public ComputedColumnDefinition(Identifier name, Expression expression, ComputedColumnPersistenceType type, ColumnConstraint constraint)
        {
            Name = name;
            Expression = expression;
            Type = type;
            Constraint = constraint;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            AddNote(Note.STRINGIFIER, ResStr.NO_COMPUTED_COLUMN);
            asm.End(this);
        }
    }

    public enum ComputedColumnPersistenceType
    {
        NotPersisted, Persisted, PersistedNotNull
    }
    #endregion
    #endregion

    #region PartitionOrFileGroup
    abstract public class PartitionOrFileGroup : GrammarNode
    {
    }

    public class Partition : PartitionOrFileGroup
    {
        public Identifier Scheme { get; set; }
        public Identifier Column { get; set; }

        public Partition(Identifier scheme, Identifier column)
        {
            Scheme = scheme;
            Column = column;
        }
    }

    public class FileGroup : PartitionOrFileGroup
    {
        public Identifier Name { get; set; }

        public FileGroup(Identifier name)
        {
            Name = name;
        }
    }
    #endregion

    #region CreateTableOption
    abstract public class CreateTableOption : GrammarNode
    {
    }

    public class DataCompressionCreateTableOption : CreateTableOption
    {
        public DataCompressionClause DataCompressionClause { get; set; }

        public DataCompressionCreateTableOption(DataCompressionClause dataCompressionClause)
        {
            DataCompressionClause = dataCompressionClause;
        }
    }

    public class StringCreateTableOption : CreateTableOption
    {
        public StringCreateTableOptionType Type { get; set; }
        public StringLiteral StringLiteral { get; set; }

        public StringCreateTableOption(StringCreateTableOptionType type, StringLiteral stringLiteral)
        {
            Type = type;
            StringLiteral = stringLiteral;
        }

        // With FiletableCollateFilename if StringLiteral is null, it means database_default.
    }

    public enum StringCreateTableOptionType
    {
        FiletableDirectory, FiletableCollateFilename, FiletablePrimaryKeyConstraintName,
        FiletableStreamidUniqueConstraintName, FiletableFullpathUniqueConstraintName
    }
    #endregion

    #region ColumnConstraint
    abstract public class ColumnConstraint : GrammarNode
    {
        public Identifier Name { get; set; }
    }

    public class PrimaryKeyColumnConstraint : ColumnConstraint
    {
        public PrimaryKeyUniqueClause PrimaryKeyUniqueClause { get; set; }
        public IList<IndexOption> IndexOptions { get; set; }
        public PartitionOrFileGroup OnClause { get; set; }

        public PrimaryKeyColumnConstraint(PrimaryKeyUniqueClause primaryKeyUniqueClause, IList<IndexOption> indexOptions, PartitionOrFileGroup onClause)
        {
            PrimaryKeyUniqueClause = primaryKeyUniqueClause;
            IndexOptions = indexOptions;
            OnClause = onClause;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(PrimaryKeyUniqueClause);
            if (IndexOptions != null)
            {
                AddNote(Note.STRINGIFIER, String.Format(ResStr.NO_PRIMARY_KEY_COLUMN_CONSTRAINT, "Index option"));
            }
            if (OnClause != null)
            {
                AddNote(Note.STRINGIFIER, String.Format(ResStr.NO_PRIMARY_KEY_COLUMN_CONSTRAINT, "ON clause"));
            }
            asm.End(this);
        }
    }

    public class ForeignKeyColumnConstraint : ColumnConstraint
    {
        public DbObject Target { get; set; }
        public Identifier TargetColumn { get; set; }
        public ForeignKeyAction OnDeleteAction { get; set; }
        public ForeignKeyAction OnUpdateAction { get; set; }
        public bool NotForReplication { get; set; }

        public ForeignKeyColumnConstraint(DbObject target, Identifier targetColumn, 
            ForeignKeyAction onDeleteAction, ForeignKeyAction onUpdateAction, bool notForReplication)
        {
            Target = target;
            TargetColumn = targetColumn;
            OnDeleteAction = onDeleteAction;
            OnUpdateAction = onUpdateAction;
            NotForReplication = notForReplication;
        }
 

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            AddNote(Note.STRINGIFIER, String.Format(ResStr.NO_COLUMN_CONSTRAINT, "FOREIGN KEY"));
            asm.End(this);
        }
    }

    public class CheckColumnConstraint : ColumnConstraint
    {
        public Expression Expression { get; set; }
        public bool NotForReplication { get; set; }

        public CheckColumnConstraint(Expression expression, bool notForReplication)
        {
            Expression = expression;
            NotForReplication = notForReplication;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            AddNote(Note.STRINGIFIER, String.Format(ResStr.NO_COLUMN_CONSTRAINT, "CHECK"));
            asm.End(this);
        }
    }

    public class DefaultColumnConstraint : ColumnConstraint
    {
        public Expression Expression { get; set; }

        public DefaultColumnConstraint(Expression expression)
        {
            Expression = expression;
        }
 
        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("DEFAULT");
            asm.AddSpace();
            asm.Add(Expression);
            asm.End(this);
        }

    }
    #endregion

    #region TableConstraint
    abstract public class TableConstraint : CreateTableDefinition
    {
        public Identifier Name { get; set; }
    }

    public class PrimaryKeyTableConstraint : TableConstraint
    {
        public PrimaryKeyUniqueClause PrimaryKeyUniqueClause { get; set; }
        public IList<OrderedColumn> Columns { get; set; }
        public IList<IndexOption> IndexOptions { get; set; }
        public PartitionOrFileGroup OnClause { get; set; }

        public PrimaryKeyTableConstraint(PrimaryKeyUniqueClause primaryKeyUniqueClause, IList<OrderedColumn> columns,
            IList<IndexOption> indexOptions, PartitionOrFileGroup onClause)
        {
            PrimaryKeyUniqueClause = primaryKeyUniqueClause;
            Columns = columns;
            IndexOptions = indexOptions;
            OnClause = onClause;
        }
        
        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (Name != null)
            {
                asm.Add(Name);
                asm.AddSpace();
            }
            asm.Add(PrimaryKeyUniqueClause);
            asm.AddSpace();
            if (Columns.Count != 0)
            {
                asm.AddToken("(");
                foreach (OrderedColumn col in Columns)
                {
                    asm.Add(col);
                    if (col != Columns.Last())
                    {
                        asm.AddToken(",");
                        asm.AddSpace();
                    }
                }
                asm.AddToken(")");
            }
            else
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_COLUMNS_DEFINED_FOR_PK);
                asm.End(this);
                return;
            }

            asm.End(this);
        }
    }

    public class ForeignKeyTableConstraint : TableConstraint
    {
        public IList<Identifier> SourceColumns { get; set; }
        public DbObject Target { get; set; }
        public IList<Identifier> TargetColumns { get; set; }
        public ForeignKeyAction OnDeleteAction { get; set; }
        public ForeignKeyAction OnUpdateAction { get; set; }
        public bool NotForReplication { get; set; }

        public ForeignKeyTableConstraint(IList<Identifier> sourceColumns, DbObject target, IList<Identifier> targetColumns,
            ForeignKeyAction onDeleteAction, ForeignKeyAction onUpdateAction, bool notForReplication)
        {
            SourceColumns = sourceColumns;
            Target = target;
            TargetColumns = targetColumns;
            OnDeleteAction = onDeleteAction;
            OnUpdateAction = onUpdateAction;
            NotForReplication = notForReplication;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            AddNote(Note.STRINGIFIER, String.Format(ResStr.NO_TABLE_CONSTRAINT, "FOREIGN KEY"));
            asm.End(this);
        }
    }

    public class CheckTableConstraint : TableConstraint
    {
        public Expression Expression { get; set; }
        public bool NotForReplication { get; set; }

        public CheckTableConstraint(Expression expression, bool notForReplication)
        {
            Expression = expression;
            NotForReplication = notForReplication;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            AddNote(Note.STRINGIFIER, String.Format(ResStr.NO_TABLE_CONSTRAINT, "CHECK"));
            asm.End(this);
        }
    }

    public class DefaultTableConstraint : TableConstraint
    {
        public Expression Expression { get; set; }
        public IList<Identifier> Columns { get; set; }
        public bool WithValues { get; set; }

        public DefaultTableConstraint(Expression expression, IList<Identifier> columns, bool withValues)
        {
            Expression = expression;
            Columns = columns;
            WithValues = withValues;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            AddNote(Note.STRINGIFIER, String.Format(ResStr.NO_TABLE_CONSTRAINT, "DEFAULT"));
            asm.End(this);
        }
    }
    #endregion

    #region PrimaryKeyUniqueClause
    public class PrimaryKeyUniqueClause : GrammarNode
    {
        public PrimaryKeyOrUnique Type { get; set; }
        public bool Clustered { get; set; }

        public PrimaryKeyUniqueClause(PrimaryKeyOrUnique type, bool clustered)
        {
            Type = type;
            Clustered = clustered;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Type)
            {
                case PrimaryKeyOrUnique.PrimaryKey:
                    if (Clustered == false)
                    {
                        AddNote(Note.STRINGIFIER, String.Format(ResStr.NO_PRIMARY_KEY_COLUMN_CONSTRAINT, "NONCLUSTERED clause"));
                    }
                    asm.AddToken("PRIMARY KEY");
                    break;
                case PrimaryKeyOrUnique.Unique:
                    if (Clustered == true)
                    {
                        AddNote(Note.STRINGIFIER, ResStr.NO_CLUSTERED_FOR_UNIQUE_COLUMN);
                    }
                    asm.AddToken("UNIQUE");
                    break;
            }
            asm.End(this);
        }
    }

    public enum PrimaryKeyOrUnique
    {
        PrimaryKey, Unique
    }

    public enum ForeignKeyAction
    {
        NoAction, Cascade, SetNull, SetDefault
    }
    #endregion

    #region DataCompressionClause
    public class DataCompressionClause : GrammarNode
    {
        public DataCompressionType Type { get; set; }
        public IList<DataCompressionPartition> Partitions { get; set; }

        public DataCompressionClause(DataCompressionType type, IList<DataCompressionPartition> partitions)
        {
            Type = type;
            Partitions = partitions;
        }
    }

    public enum DataCompressionType
    {
        None, Row, Page
    }
    #endregion

    #region DataCompressionPartition
    public class DataCompressionPartition : GrammarNode
    {
        public int From { get; set; }
        public int To { get; set; }

        public DataCompressionPartition(int from, int to)
        {
            From = from;
            To = to;
        }
    }
    #endregion

    #region IndexOption
    abstract public class IndexOption : GrammarNode
    {
    }

    public class SimpleIndexOption : IndexOption
    {
        public SimpleIndexOptionType Type { get; set; }
        public bool Enabled { get; set; }

        public SimpleIndexOption(SimpleIndexOptionType type, bool enabled)
        {
            Type = type;
            Enabled = enabled;
        }
    }

    public enum SimpleIndexOptionType
    {
        PadIndex, IgnoreDupKey, StatisticsNorecompute, AllowRowLocks, AllowPageLocks, SortInTempDB, Online, DropExisting
    }

    public class FillFactorIndexOption : IndexOption
    {
        public int FillFactor { get; set; }

        public FillFactorIndexOption(int fillFactor)
        {
            FillFactor = fillFactor;
        }
    }

    public class MaxDopIndexOption : IndexOption
    {
        public int MaxDop { get; set; }

        public MaxDopIndexOption(int maxDop)
        {
            MaxDop = maxDop;
        }
    }

    public class DataCompressionIndexOption : IndexOption
    {
        public DataCompressionClause DataCompressionClause { get; set; }

        public DataCompressionIndexOption(DataCompressionClause dataCompressionClause)
        {
            DataCompressionClause = dataCompressionClause;
        }
    }

    public class PartitionOrFileGroupIndexOption : IndexOption
    {
        public PartitionOrFileGroupIndexOptionType Type { get; set; }
        public PartitionOrFileGroup PartitionOrFileGroup { get; set; }

        public PartitionOrFileGroupIndexOption(PartitionOrFileGroupIndexOptionType type, PartitionOrFileGroup partitionOrFileGroup)
        {
            Type = type;
            PartitionOrFileGroup = partitionOrFileGroup;
        }
    }

    public enum PartitionOrFileGroupIndexOptionType
    {
        MoveTo, FileStreamOn
    }
    #endregion
    #endregion

    #region AlterTable
    #region AlterTableStatement
    public class AlterTableStatement : Statement
    {
        public DbObjectTableSource TableSource { get; set; }
        public AlterTableAction Action { get; set; }

        public AlterTableStatement(DbObjectTableSource tableSource, AlterTableAction action)
        {
            TableSource = tableSource;
            Action = action;
        }
       
        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (Action != null)
            {
                asm.AddToken("ALTER TABLE");
                if (TableSource != null)
                {
                    asm.AddSpace();
                    asm.Add(TableSource);
                }
                asm.AddSpace();
                asm.Add(Action);
            }
            else
            {
                AddNote(Note.STRINGIFIER, ResStr.WARN_NO_ACTION_IN_ALTER_TABLE);
            }
            asm.End(this);
        }
    }
    #endregion

    #region AlterTableAction
    abstract public class AlterTableAction : GrammarNode
    {
    }

    public class AlterColumnDefineAlterTableAction : AlterTableAction
    {
        public Identifier Name { get; set; }
        public DataType Type { get; set; }
        public Identifier Collation { get; set; }
        public bool? IsNull { get; set; }
        public bool IsSparse { get; set; }

        public AlterColumnDefineAlterTableAction(Identifier name, DataType type, Identifier collation, bool? isNull, bool isSparse)
        {
            Name = name;
            Type = type;
            Collation = collation;
            IsNull = isNull;
            IsSparse = isSparse;
        }
       
        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("ALTER");
            asm.AddSpace();
            asm.AddToken("(");
            asm.Add(Name);
            if (Type != null)
            {
                asm.AddSpace();
                asm.Add(Type);
            }
            if (IsNull != null)
            {
                if (IsNull == true)
                {
                    asm.AddSpace();
                    asm.AddToken("NULL");
                }
                else
                {
                    asm.AddSpace();
                    asm.AddToken("NOT");
                    asm.AddSpace();
                    asm.AddToken("NULL");
                }
            }
            asm.AddToken(")");
            asm.End(this);
        }
    }

    public class AlterColumnAddDropAlterTableAction : AlterTableAction
    {
        public Identifier Name { get; set; }
        public AddOrDrop Action { get; set; }
        public AlterColumnModifier Modifier { get; set; }

        public AlterColumnAddDropAlterTableAction(Identifier name, AddOrDrop action, AlterColumnModifier modifier)
        {
            Name = name;
            Action = action;
            Modifier = modifier;
        }
    } 

    public enum AddOrDrop
    {
        Add, Drop
    }

    public enum AlterColumnModifier
    {
        RowGuidCol, Persisted, NotForReplication, Sparse
    }

    public class AddAlterTableAction : AlterTableAction
    {
        public bool? WithCheck { get; set; }
        public IList<CreateTableDefinition> Definitions { get; set; }

        public AddAlterTableAction(bool? withCheck, IList<CreateTableDefinition> definitions)
        {
            WithCheck = withCheck;
            Definitions = definitions;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (Definitions.Count != 0)
            {
                asm.AddToken("ADD");
                asm.AddSpace();
                if (Definitions[0] is PrimaryKeyTableConstraint)
                {
                    asm.AddToken("CONSTRAINT");
                    asm.AddSpace();
                    asm.Add(Definitions[0]);
                }
                else
                {
                    asm.AddToken("(");
                    foreach (CreateTableDefinition def in Definitions)
                    {
                        asm.Add(def);
                        if (def != Definitions.Last())
                        {
                            asm.AddToken(",");
                            asm.AddSpace();
                        }
                    }
                    asm.AddToken(")");
                }
            }
            asm.End(this);
        }
    }

    public class DropAlterTableAction : AlterTableAction
    {
        public IList<DropAlterTableDefinition> Definitions { get; set; }

        public DropAlterTableAction(IList<DropAlterTableDefinition> definitions)
        {
            Definitions = definitions;
        }        
 
        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (Definitions.Count != 0)
            {
                asm.AddToken("DROP");
                asm.AddSpace();
                if (Definitions[0] is DropConstraintAlterTableDefinition)
                {
                    asm.AddToken("CONSTRAINT");
                    asm.AddSpace();
                    asm.Add(Definitions[0]);
                }
                else
                {
                    asm.AddToken("(");
                    foreach (DropAlterTableDefinition def in Definitions)
                    {
                        asm.Add(def);
                        if (def != Definitions.Last())
                        {
                            asm.AddToken(",");
                            asm.AddSpace();
                        }
                    }
                    asm.AddToken(")");
                }
            }
            asm.End(this);
        }
    }
    #endregion

    abstract public class DropAlterTableDefinition : GrammarNode
    {
    }

    public class DropConstraintAlterTableDefinition : DropAlterTableDefinition
    {
        public Identifier Name { get; set; }
        public IList<DropClusteredConstraintOption> Options { get; set; }

        public DropConstraintAlterTableDefinition(Identifier name, IList<DropClusteredConstraintOption> options)
        {
            Name = name;
            Options = options;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Name);
            asm.End(this);
        }
    }

    public class DropColumnAlterTableDefinition : DropAlterTableDefinition
    {
        public Identifier Name { get; set; }

        public DropColumnAlterTableDefinition(Identifier name)
        {
            Name = name;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Name);
            asm.End(this);
        }
    }

    abstract public class DropClusteredConstraintOption : GrammarNode
    {
    }

    public class MaxDopDropClusteredConstraintOption : DropClusteredConstraintOption
    {
        public int MaxDop { get; set; }

        public MaxDopDropClusteredConstraintOption(int maxDop)
        {
            MaxDop = maxDop;
        }
    }

    public class OnlineDropClusteredConstraintOption : DropClusteredConstraintOption
    {
        public bool Online { get; set; }

        public OnlineDropClusteredConstraintOption(bool online)
        {
            Online = online;
        }
    }

    public class MoveToDropClusteredConstraintOption : DropClusteredConstraintOption
    {
        public PartitionOrFileGroup Location { get; set; }

        public MoveToDropClusteredConstraintOption(PartitionOrFileGroup location)
        {
            Location = location;
        }
    }

    public class CheckAlterTableAction : AlterTableAction
    {
        public bool? WithCheck { get; set; }
        public bool IsCheck { get; set; }

        // Empty list means "ALL" constraints.
        public IList<Identifier> Constraints { get; set; }

        public CheckAlterTableAction(bool? withCheck, bool isCheck, IList<Identifier> constraints)
        {
            WithCheck = withCheck;
            IsCheck = isCheck;
            Constraints = constraints;
        }
    }

    public class TriggerAlterTableAction : AlterTableAction
    {
        public bool Enable { get; set; }

        // Empty list means "ALL" triggers.
        public IList<Identifier> Triggers { get; set; }

        public TriggerAlterTableAction(bool enable, IList<Identifier> triggers)
        {
            Enable = enable;
            Triggers = triggers;
        }
    }

    public class ChangeTrackingAlterTableAction : AlterTableAction
    {
        public bool Enable { get; set; }
        public bool TrackColumnsUpdated { get; set; }

        public ChangeTrackingAlterTableAction(bool enable, bool trackColumnsUpdated)
        {
            Enable = enable;
            TrackColumnsUpdated = trackColumnsUpdated;
        }
    }

    public class SwitchPartitionAlterTableAction : AlterTableAction
    {
        public Expression FromPartition { get; set; }
        public DbObject To { get; set; }
        public Expression ToPartition { get; set; }

        public SwitchPartitionAlterTableAction(Expression fromPartition, DbObject to, Expression toPartition)
        {
            FromPartition = fromPartition;
            To = to;
            ToPartition = toPartition;
        }
    }

    public class SetFilestreamAlterTableAction : AlterTableAction
    {
        public PartitionOrFileGroup Location { get; set; }

        public SetFilestreamAlterTableAction(PartitionOrFileGroup location)
        {
            Location = location;
        }
    }

    public class RebuildPartitionAlterTableAction : AlterTableAction
    {
        // If partition is null, it means ALL partitions.
        public RebuildPartitionClause Partition { get; set; }
        public IList<IndexOption> IndexOptions { get; set; }

        public RebuildPartitionAlterTableAction(RebuildPartitionClause partition, IList<IndexOption> indexOptions)
        {
            Partition = partition;
            IndexOptions = indexOptions;
        }
    }

    public class RebuildPartitionClause : GrammarNode
    {
        public int PartitionNumber { get; set; }

        public RebuildPartitionClause(int partitionNumber)
        {
            PartitionNumber = partitionNumber;
        }
    }

    public class LockEscalationTableOptionAlterTableAction : AlterTableAction
    {
        public LockEscalationType Type { get; set; }

        public LockEscalationTableOptionAlterTableAction(LockEscalationType type)
        {
            Type = type;
        }
    }

    public enum LockEscalationType
    {
        Auto, Table, Disable
    }

    public class FiletableTableOptionAlterTableAction : AlterTableAction
    {
        public bool? EnableNamespace { get; set; }
        public StringLiteral Directory { get; set; }

        public FiletableTableOptionAlterTableAction(bool? enableNamespace, StringLiteral directory)
        {
            EnableNamespace = enableNamespace;
            Directory = directory;
        }
    }
    #endregion

    #region DropTableStatement
    public class DropTableStatement : Statement
    {
        public IList<DbObjectTableSource> TableSources { get; set; }

        public DropTableStatement(IList<DbObjectTableSource> tablesSources)
        {
            TableSources = tablesSources;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (TableSources != null && TableSources.Count > 0)
            {
                asm.AddToken("DROP");
                asm.AddSpace();
                asm.AddToken("TABLE");
                asm.AddSpace();
                foreach (DbObjectTableSource table in TableSources)
                {
                    asm.Add(table);
                    if (table != TableSources.Last())
                    {
                        asm.AddToken(",");
                        asm.AddSpace();
                    }
                }
            }
            asm.End(this);
        }
    }
    #endregion

    #region CreateIndexStatement
    public class CreateIndexStatement : Statement
    {
        public bool Unique { get; set; }
        public bool? Clustered { get; set; }
        public Identifier Name { get; set; }
        public DbObjectIndexTarget IndexTarget { get; set; }
        public IList<OrderedColumn> IndexColumns { get; set; }
        public IList<Identifier> IncludeColumns { get; set; }
        public Expression Condition { get; set; }
        public IList<IndexOption> Options { get; set; }
        public PartitionOrFileGroup OnClause { get; set; }
        public PartitionOrFileGroup FileStreamOnClause { get; set; }

        public CreateIndexStatement(bool unique, bool? clustered, Identifier name, DbObjectIndexTarget target, IList<OrderedColumn> indexColumns,
            IList<Identifier> includeColumns, Expression condition, IList<IndexOption> options, PartitionOrFileGroup onClause,
            PartitionOrFileGroup fileStreamOnClause)
        {
            Unique = unique;
            Clustered = clustered;
            Name = name;
            IndexTarget = target;
            IndexColumns = indexColumns;
            IncludeColumns = includeColumns;
            Condition = condition;
            Options = options;
            OnClause = onClause;
            FileStreamOnClause = fileStreamOnClause;
        }

        public override void  Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("CREATE");
            asm.AddToken(Unique ? " UNIQUE" : string.Empty);
            asm.AddSpace();
            asm.AddToken("INDEX");
            asm.AddSpace();
            asm.Add(Name);
            asm.AddSpace();
            asm.AddToken("ON");
            asm.AddSpace();
            asm.Add(IndexTarget);
            if (Name == null)
            {
                AddNote(Note.STRINGIFIER, ResStr.WARN_NO_INDEX_NAME_IN_CREATE);
                asm.End(this);
                return;
            }
            if (IndexTarget == null)
            {
                AddNote(Note.STRINGIFIER, ResStr.WARN_NO_TABLE_NAME_IN_CREATE_INDEX);
                asm.End(this);
                return;
            }

            if (IndexColumns != null && IndexColumns.Count > 0)
            {
                asm.AddToken("(");
                foreach (OrderedColumn col in IndexColumns)
                {
                    asm.Add(col);
                    if (col != IndexColumns.Last())
                    {
                        asm.AddToken(",");
                    }
                }
                asm.AddToken(")");
            }
            else
            {
                AddNote(Note.STRINGIFIER, ResStr.WARN_NO_COLUMNS_IN_CREATE_INDEX);
            }

            if (Clustered != null)
            {
                if (Clustered.Value == true)
                {
                    AddNote(Note.STRINGIFIER, ResStr.NO_CLUSTERED_INDEX);
                }
                else
                {
                    AddNote(Note.STRINGIFIER, ResStr.WARN_NO_NONCLUSTERED);
                }
            }
            if (IncludeColumns != null && IncludeColumns.Count > 0)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_INCLUDE_CLAUSE);
            }
            if (Condition != null)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_WHERE_CLAUSE_FOR_CREATE_INDEX);
            }
            if (Options != null)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_WITH_ON_HANA);
            }
            if (OnClause != null)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_ON_CLAUSE_FOR_CREATE_INDEX);
            }
            if (FileStreamOnClause != null)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_FILETSTREAM_ON_FOR_CREATE_INDEX);
            }
            asm.End(this);
        }
    }
    #endregion

    public class AlterIndexStatement : Statement
    {
        public Identifier Index { get; set; }
        public DbObjectTableSource TableSource { get; set; }
        public AlterIndexAction Action { get; set; }

        public AlterIndexStatement(Identifier index, DbObjectTableSource tableSource, AlterIndexAction action)
        {
            Index = index;
            TableSource = tableSource;
            Action = action;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("ALTER INDEX");
            asm.AddSpace();
            asm.Add(Index);

            if (Action != null)
            {
                asm.AddSpace();
                asm.Add(Action);
            }
            asm.End(this);
        }
    }

    abstract public class AlterIndexAction : GrammarNode
    {
    }

    public class RebuildAlterIndexAction : AlterIndexAction
    {
        public RebuildPartitionClause Partition { get; set; }
        public IList<IndexOption> Options { get; set; }

        public RebuildAlterIndexAction(RebuildPartitionClause partition, IList<IndexOption> options)
        {
            Partition = partition;
            Options = options;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("REBUILD");
            asm.End(this);
        }
    }

    public class DisableAlterIndexAction : AlterIndexAction
    {
        public DisableAlterIndexAction()
        {
        }
    }

    public class ReorganizeAlterIndexAction : AlterIndexAction
    {
        public int Partition { get; set; }
        public bool LobCompaction { get; set; }

        public ReorganizeAlterIndexAction(int partition, bool lobCompaction)
        {
            Partition = partition;
            LobCompaction = lobCompaction;
        }
    }

    public class SetAlterIndexAction : AlterIndexAction
    {
        public IList<IndexOption> Options { get; set; }

        public SetAlterIndexAction(IList<IndexOption> options)
        {
            Options = options;
        }
    }

    public class DropIndexStatement : Statement
    {
        public IList<DropIndexAction> Actions { get; set; }

        public DropIndexStatement(IList<DropIndexAction> actions)
        {
            Actions = actions;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("DROP INDEX");

            if (Actions.Count == 1)
            {
                asm.AddSpace();
                asm.Add(Actions[0]);
            }
            asm.End(this);
        }
    }

    public class DropIndexAction : GrammarNode
    {
        public Identifier Index { get; set; }
        public DbObjectTableSource TableSource { get; set; }
        public IList<IndexOption> Options { get; set; }

        public DropIndexAction(Identifier index, DbObjectTableSource tableSource, IList<IndexOption> options = null)
        {
            Index = index;
            TableSource = tableSource;
            Options = options;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Index);
            if (Options != null)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_OPTIONS_FOR_DROP_INDEX);
            }
            asm.End(this);
        }
    }

    // Common base class for CREATE VIEW and ALTER VIEW, since they are almost identical.
    abstract public class CreateAlterViewStatement : Statement
    {
        public DbObject Name { get; set; }
        public IList<Identifier> Columns { get; set; }
        public IList<ViewAttribute> Attributes { get; set; }
        public SelectStatement Statement { get; set; }
        public bool CheckOption { get; set; }

        public CreateAlterViewStatement(DbObject name, IList<Identifier> columns, IList<ViewAttribute> attributes,
            SelectStatement statement, bool checkOption)
        {
            Name = name;
            Columns = columns;
            Attributes = attributes;
            Statement = statement;
            CheckOption = checkOption;
        }
    }

    public enum ViewAttribute
    {
        Encryption, SchemaBinding, ViewMetadata
    }

    public class CreateViewStatement : CreateAlterViewStatement
    {
        public CreateViewStatement(DbObject name, IList<Identifier> columns, IList<ViewAttribute> attributes,
            SelectStatement statement, bool checkOption)
            : base(name, columns, attributes, statement, checkOption)
        {
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add("CREATE VIEW");
            asm.AddSpace();
            asm.Add(Name);
            asm.AddSpace();

            if (Columns != null)
            {
                asm.Add("(");
                foreach (Identifier col in Columns)
                {
                    asm.Add(col);
                    if (col != Columns.Last())
                    {
                        asm.Add(", ");
                    }
                }
                asm.Add(")");
                asm.AddSpace();
            }

            asm.Add("AS");
            asm.AddSpace();
            asm.Add(Statement);
            asm.End(this);
        }
    }

    public class AlterViewStatement : CreateAlterViewStatement
    {
        public AlterViewStatement(DbObject name, IList<Identifier> columns, IList<ViewAttribute> attributes,
            SelectStatement statement, bool checkOption)
            : base(name, columns, attributes, statement, checkOption)
        {
        }

        public override void Assembly(Assembler asm)
        {
            //ALTER VIEW is not supported in HANA
        }
    }

    public class DropViewStatement : Statement
    {
        public IList<DbObject> Views { get; set; }

        public DropViewStatement(IList<DbObject> views)
        {
            Views = views;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add("DROP VIEW");
            if (Views.Count == 1)
            {
                asm.AddSpace();
                asm.Add(Views[0]);
            }
            asm.End(this);
        }
    }

    abstract public class CreateTypeStatement : Statement
    {
    }

    public class CreateBaseTypeStatement : CreateTypeStatement
    {
        public DbObject Name { get; set; }
        public DataType Type { get; set; }
        public bool IsNull { get; set; }

        public CreateBaseTypeStatement (DbObject name, DataType type, bool isNull)
        {
            Name = name;
            Type = type;
            IsNull = isNull;
        }
    }

    public class CreateTableTypeStatement : CreateTypeStatement
    {
        public DbObject Name { get; set; }
        public IList<CreateTableDefinition> Definitions { get; set; }

        public CreateTableTypeStatement(DbObject name, IList<CreateTableDefinition> definitions)
        {
            Name = name;
            Definitions = definitions;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("CREATE TYPE");
            asm.AddSpace();
            asm.Add(Name);
            asm.AddSpace();
            asm.AddToken("AS TABLE (");
            foreach (CreateTableDefinition def in Definitions)
            {
                asm.Add(def);
                if (def != Definitions.Last())
                {
                    asm.AddToken(", ");
                }
            }
            asm.AddToken(")");
            asm.End(this);
        }
    }

    public class DropTypeStatement : Statement
    {
        public DbObject Name { get; set; }

        public DropTypeStatement(DbObject name)
        {
            Name = name;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("DROP TYPE");
            asm.AddSpace();
            asm.Add(Name);
            asm.End(this);
        }
    }

    public enum FunctionOptionType
    {
        ENCRYPTION,
	    SCHEMABINDING,
	    RETURNS_NULL_ON_NULL_INPUT,
	    CALLED_ON_NULL_INPUT, 
        EXECUTE_AS
    }

    public abstract class FunctionOption : GrammarNode
    {
    }

    public class SimpleFunctionOption : FunctionOption
    {
        public FunctionOptionType Type { get; set; }

        public SimpleFunctionOption(FunctionOptionType type)
        {
            Type = type;
        }
    }

    public class ExecuteAsFunctionOption : FunctionOption
    {
        public ExecuteAsContext ExecContext { get; set; }

        public ExecuteAsFunctionOption(ExecuteAsContext executeAs)
        {
            ExecContext = executeAs;
        }
    }

    abstract public class CreateFunctionStatement : Statement
    {
        public DbObject Name { get; set; }
        public FunctionOption FunctionOption { get; set; }
        public IList<ProcedureParameter> FunctionParams { get; set; }
        public BlockStatement FunctionBody { get; set; }
    }

    public class CreateScalarFunctionStatement : CreateFunctionStatement
    {
        public DataType ReturnType { get; set; }

        public CreateScalarFunctionStatement(DbObject name, List<ProcedureParameter> functionParams, DataType returnType, FunctionOption functionOption, BlockStatement functionBody)
        {
            Name = name;
            FunctionParams = functionParams;
            FunctionOption = functionOption;
            FunctionBody = functionBody;

            ReturnType = returnType;
        }
    }

    public class CreateTableValuedFunctionStatement : CreateFunctionStatement
    {
        public SelectStatement QueryStatement { get; set; }

        public CreateTableValuedFunctionStatement(DbObject name, List<ProcedureParameter> functionParams, FunctionOption functionOption, SelectStatement queryStmt )
        {
            Name = name;
            FunctionParams = functionParams;
            FunctionOption = functionOption;

            QueryStatement = queryStmt;
        }
    }

    public class CreateMultiStatementFunctionStatement : CreateFunctionStatement
    {
        public VariableExpression ReturnVariable { get; set; }
        public List<CreateTableDefinition> TableDefinitions { get; set; }

        public CreateMultiStatementFunctionStatement(DbObject name, IList<ProcedureParameter> functionParams, VariableExpression returnVariable, 
                List<CreateTableDefinition> tableDefinitions,  FunctionOption functionOption, BlockStatement functionBody)
        {
            Name = name;
            FunctionParams = functionParams;
            FunctionOption = functionOption;
            FunctionBody = functionBody;

            ReturnVariable = returnVariable;
            TableDefinitions = tableDefinitions;
        }
    }

    abstract public class CreateAlterProcedureStatement : Statement
    {
        public DbObject Name { get; set; }
        public int Number { get; set; }
        public IList<ProcedureParameter> Parameters { get; set; }
        public List<ProcedureOption> Options { get; set; }
        public bool ForReplication { get; set; }
        public BlockStatement Declarations { get; set; }
        public BlockStatement Statements { get; set; }

        public CreateAlterProcedureStatement(DbObject name, int number, IList<ProcedureParameter> parameters,
            List<ProcedureOption> options, bool forReplication, IList<Statement> statements)
        {
            Name = name;
            Number = number;
            Parameters = parameters;
            Options = options;
            ForReplication = forReplication;
            Declarations = new BlockStatement();
            Statements = new BlockStatement(statements);
        }
    }

    public class CreateProcedureStatement : CreateAlterProcedureStatement
    {
        public CreateProcedureStatement(DbObject name, int number, IList<ProcedureParameter> parameters,
            List<ProcedureOption> options, bool forReplication, IList<Statement> statements)
            : base(name, number, parameters, options, forReplication, statements)
        {
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("CREATE PROCEDURE");
            asm.AddSpace();
            asm.Add(Name);
            if (Parameters != null)
            {
                asm.AddSpace();
                asm.AddToken("(");
                foreach (ProcedureParameter param in Parameters)
                {
                    asm.Add(param);
                    if (param != Parameters.Last())
                    {
                        asm.AddToken(", ");
                    }
                }
                asm.AddToken(")");
            }
            asm.AddSpace();
            asm.NewLine();
            asm.AddToken("AS");
            if (Declarations != null)
            {
                asm.AddSpace();
                asm.NewLine();
                asm.Add(Declarations);
            }
            asm.NewLine();
            asm.AddToken("BEGIN");
            asm.IncreaseIndentation();
            asm.AddSpace();
            asm.Add(Statements);
            asm.DecreaseIndentation();
            asm.NewLine();
            asm.AddToken("END");
            asm.End(this);
        }
    }

    public class AlterProcedureStatement : CreateAlterProcedureStatement
    {
        public AlterProcedureStatement(DbObject name, int number, IList<ProcedureParameter> parameters,
            List<ProcedureOption> options, bool forReplication, IList<Statement> statements)
            : base(name, number, parameters, options, forReplication, statements)
        {
        }
    }

    abstract public class ProcedureParameter : GrammarNode
    {
    }

    public class DataTypeProcedureParameter : ProcedureParameter
    {
        public string Name { get; set; }
        public DataType Type { get; set; }
        public Expression DefaultValue { get; set; }
        public bool Output { get; set; }
        public bool InOut { get; set; }
        public bool ReadOnly { get; set; }

        public DataTypeProcedureParameter(string name, DataType type, Expression defaultValue, bool output, bool readOnly)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Output = output;
            ReadOnly = readOnly;
            InOut = false;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);

            if (ReadOnly)
            {
                asm.AddToken("IN");
            }
            else if (Output)
            {
                asm.AddToken("OUT");
            }
            else
            {
                if (InOut)
                {
                    asm.AddToken("INOUT");
                }
                else
                {
                    asm.AddToken("IN");
                }
            }
            asm.AddSpace();
            asm.Add(Name);
            asm.AddSpace();
            asm.Add(Type);
            if (DefaultValue != null)
            {
                AddNote(Note.STRINGIFIER, ResStr.NO_DEFAULT_VALUES_PROCEDURE_INPUT_PARAMETERS);
            }
            asm.End(this);
        }
    }

    public class CursorProcedureParameter : ProcedureParameter
    {
        public CursorProcedureParameter()
        {
        }
    }

    abstract public class ProcedureOption : GrammarNode
    {
    }

    public class SimpleProcedureOption : ProcedureOption
    {
        public SimpleProcedureOptionType Type { get; set; }

        public SimpleProcedureOption(SimpleProcedureOptionType type)
        {
            Type = type;
        }
    }

    public enum SimpleProcedureOptionType
    {
        Encryption, Recompile
    }

    public class ExecuteAsProcedureOption : ProcedureOption
    {
        public ExecuteAsContext Context { get; set; }

        public ExecuteAsProcedureOption(ExecuteAsContext context)
        {
            Context = context;
        }
    }

    abstract public class ExecuteAsContext : GrammarNode
    {
    }

    public class SimpleExecuteAsContext : ExecuteAsContext
    {
        public SimpleExecuteAsContextType Type { get; set; }

        public SimpleExecuteAsContext(SimpleExecuteAsContextType type)
        {
            Type = type;
        }
    }

    public enum SimpleExecuteAsContextType
    {
        Caller, Self, Owner
    }

    public class StringExecuteAsContext : ExecuteAsContext
    {
        public StringLiteral User { get; set; }

        public StringExecuteAsContext(StringLiteral user)
        {
            User = user;
        }
    }

    public class DropProcedureStatement : Statement
    {
        public List<DbObject> Names { get; set; }

        public DropProcedureStatement(List<DbObject> names)
        {
            Names = names;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            foreach (DbObject name in Names)
            {
                asm.AddToken("DROP PROCEDURE");
                asm.AddSpace();
                asm.Add(name);
            }
            asm.End(this);
        }
    }

    public class CreateTriggerStatement : Statement
    {
    }

    public class CreateDmlTriggerStatement : CreateTriggerStatement
    {
        public DbObject Name { get; set; }
        public DbObject Table { get; set; }
        public List<DmlTriggerOption> TriggerDmlOptions { get; set; }
        public TriggerApplType ApplType { get; set; }
        public List<TriggerDmlOperationType> TriggerDmlOperations { get; set; }
        public bool NotForReplication { get; set; }
        public TriggerAction Action { get; set; }

        public CreateDmlTriggerStatement(DbObject name, DbObject table, List<DmlTriggerOption> triggerDmlOptions,
            TriggerApplType applType, List<TriggerDmlOperationType> triggerDmlOperations, bool notForReplication, TriggerAction action)
        {
            Name = name;
            Table = table;
            TriggerDmlOptions = triggerDmlOptions;
            ApplType = applType;
            TriggerDmlOperations = triggerDmlOperations;
            NotForReplication = notForReplication;
            Action = action;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("CREATE");
            asm.AddSpace();
            asm.AddToken("TRIGGER");
            asm.AddSpace();
            asm.Add(Name);
            asm.AddSpace();
            switch (ApplType)
            {
                case TriggerApplType.AFTER:
                    asm.AddToken("AFTER");
                    break;
                default:
                    AddNote(Note.STRINGIFIER, String.Format(ResStr.TYPE_NOT_SUPPORTED, ApplType.ToString()));
                    break;
            }
            asm.AddSpace();
            foreach (TriggerDmlOperationType type in TriggerDmlOperations)
            {
                switch (type)
                {
                    case TriggerDmlOperationType.INSERT:
                        asm.AddToken("INSERT");
                        break;
                    case TriggerDmlOperationType.DELETE:
                        asm.AddToken("DELETE");
                        break;
                    case TriggerDmlOperationType.UPDATE:
                        asm.AddToken("UPDATE");
                        break;
                }
                if (TriggerDmlOperations.Last() != type)
                {
                    asm.AddToken(",");
                    asm.AddSpace();
                }
            }
            asm.AddSpace();
            asm.AddToken("ON");
            asm.AddSpace();
            asm.Add(Table);
            asm.AddSpace();
            asm.Add(Action);
            asm.End(this);
        }
    }

    public class DmlTriggerOption : GrammarNode
    {
        public bool? Encryption  { get; set; }
        public ExecuteAsContext Context { get; set; }

        public DmlTriggerOption (bool encryption)
        {
            Encryption = encryption;
            Context = null;
        }

        public DmlTriggerOption (ExecuteAsContext context)
        {
            Context = context;
            Encryption = null;
        }
    }

    public enum TriggerApplType
    {
        FOR, AFTER, INSTEAD_OF
    }

    public enum TriggerDmlOperationType
    {
        INSERT, UPDATE, DELETE
    }

    public class TriggerAction : GrammarNode
    {
        public DbObject Procedure { get; set; }
        public BlockStatement Statements { get; set; }

        public TriggerAction(DbObject procedure)
        {
            Procedure = procedure;
            Statements = null;
        }

        public TriggerAction(BlockStatement statements)
        {
            Statements = statements;
            Procedure = null;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            if (Statements != null)
            {
                asm.AddToken("BEGIN");
                asm.AddSpace();
                asm.Add(Statements);
                asm.AddSpace();
                asm.NewLine();
                asm.AddToken("END");
            }
            asm.End(this);
        }
    }

    public class DropTriggerStatement : Statement
    {
        public DbObject Name { get; set; }

        public DropTriggerStatement(DbObject name)
        {
            Name = name;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("DROP");
            asm.AddSpace();
            asm.AddToken("TRIGGER");
            asm.AddSpace();
            asm.Add(Name);
            asm.End(this);
        }
    }

    public class RaisErrorStatement : Statement
    {
        public List<ExecParam> Parameters {get; set;}

        public RaisErrorStatement (List<ExecParam> parameters)
        {
            Parameters = parameters;
        }
    }
}
