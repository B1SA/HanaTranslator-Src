using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    #region Identifier
    public class Identifier : GrammarNode
    {
        public string Name { get; set; }
        public IdentifierType Type { get; set; }

        [ExcludeFromChildrenList]
        public bool IsEmpty { get { return string.IsNullOrEmpty(Name); } }

        public Identifier(IdentifierType type, string name)
        {
            Type = type;
            Name = name;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (this.Type)
            {
                case IdentifierType.Plain:
                    asm.Add(this.Name);
                    break;
                case IdentifierType.Bracketed:
                    asm.AddToken("[");
                    asm.Add(this.Name);
                    asm.AddToken("]");
                    break;
                case IdentifierType.Quoted:
                    asm.AddToken("\"");
                    asm.Add(this.Name.Replace("\"", "\"\""));
                    asm.AddToken("\"");
                    break;
            }
            asm.End(this);
        }
    }

    public enum IdentifierType
    {
        Plain, Bracketed, Quoted
    }
    #endregion

    #region StringLiteral
    public class StringLiteral : GrammarNode
    {
        public string String { get; set; }
        public StringLiteralType Type { get; set; }

        public StringLiteral(StringLiteralType type, string str)
        {
            Type = type;
            String = str;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken(this.Type == StringLiteralType.Unicode ? "n" : "");
            asm.AddToken("'");
            asm.Add(this.String.Replace("'", "''"));
            asm.AddToken("'");
            asm.End(this);
        }
    }

    public enum StringLiteralType
    {
        ASCII, Unicode
    }
    #endregion

    #region MoneyLiteral
    public class MoneyLiteral : GrammarNode
    {
        public char Currency { get; set; }
        public decimal Value { get; set; }

        public MoneyLiteral(string str)
        {
            Currency = str[0];
            Value = Decimal.Parse(str.Substring(1));
        }
        
        override public void Assembly(Assembler asm)
        {
            // HANA does not support money type with currency.
            asm.Begin(this);
            asm.Add(Value);
            asm.End(this);
        }
    }
    #endregion

    #region RealLiteral
    public class RealLiteral : GrammarNode
    {
        public decimal Value { get; set; }
        public int Exponent { get; set; }

        public RealLiteral(string str)
        {
            string[] parts = str.Split('e', 'E');
            Value = Decimal.Parse(parts[0]);
            Exponent = parts.Length > 1 ? Int32.Parse(parts[1]) : 0;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.Add(Value);
            if (Exponent != 0)
            {
                asm.AddToken("e");
                asm.Add(Exponent);
            }
            asm.End(this);
        }
    }
    #endregion

    #region DbObject
    public class DbObject : GrammarNode
    {
        public IList<Identifier> Identifiers { get; set; }

        public DbObject()
        {
            Identifiers = new List<Identifier>();
        }

        public DbObject(Identifier identifier)
        {
            Identifiers = new List<Identifier> { identifier };
        }

        public DbObject(List<Identifier> identifiers)
        {
            Identifiers = identifiers;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            foreach (Identifier i in Identifiers)
            {
                asm.Add(i);
                if (i != Identifiers.Last())
                {
                    asm.AddToken(".");
                }
            }
            asm.End(this);
        }
    }
    #endregion

    #region DataType
    abstract public class DataType : GrammarNode
    {
    }

    abstract public class BuiltinDataType : DataType
    {
    }

    public class SimpleBuiltinDataType : BuiltinDataType
    {
        public SimpleBuiltinDataTypeType Type { get; set; }

        public SimpleBuiltinDataType(SimpleBuiltinDataTypeType type)
        {
            Type = type;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Type)
            {
                case SimpleBuiltinDataTypeType.BigInt:
                    asm.AddToken("bigint");    // TODO check
                    break;
                case SimpleBuiltinDataTypeType.Bit:
                    asm.AddToken("tinyint");
                    break;
                case SimpleBuiltinDataTypeType.Int:
                    asm.AddToken("integer");
                    break;
                case SimpleBuiltinDataTypeType.Money:
                    asm.AddToken("decimal");
                    break;
                case SimpleBuiltinDataTypeType.SmallInt:
                    asm.AddToken("smallint");
                    break;    // TODO check
                case SimpleBuiltinDataTypeType.SmallMoney:
                    asm.AddToken("smalldecimal");
                    break;
                case SimpleBuiltinDataTypeType.TinyInt:
                    asm.AddToken("tinyint");
                    break;    // TODO check
                case SimpleBuiltinDataTypeType.Real:
                    asm.AddToken("real");
                    break;    // TODO check
                case SimpleBuiltinDataTypeType.Date:
                    asm.AddToken("date");
                    break;    // TODO check
                case SimpleBuiltinDataTypeType.DateTime:
                    asm.AddToken("timestamp");
                    break;
                case SimpleBuiltinDataTypeType.SmallDateTime:
                    asm.AddToken("seconddate");
                    break;
                case SimpleBuiltinDataTypeType.Text:
                    asm.AddToken("text");
                    break;    // TODO check
                case SimpleBuiltinDataTypeType.NText:
                    asm.AddToken("nclob");
                    return;
                case SimpleBuiltinDataTypeType.Image:
                    asm.AddToken("blob");
                    break;
                case SimpleBuiltinDataTypeType.HierarchyId:
                    AddNote(Note.STRINGIFIER, ResStr.NO_TYPE_HIERARCHYID);
                    break;
                case SimpleBuiltinDataTypeType.RowVersion:
                    asm.AddToken("rowversion");
                    break;    // TODO check
                case SimpleBuiltinDataTypeType.SqlVariant:
                    AddNote(Note.STRINGIFIER, ResStr.NO_TYPE_SQL_VARIANT);
                    break;
                case SimpleBuiltinDataTypeType.TimeStamp:
                    asm.AddToken("timestamp");
                    break;    // TODO check
                case SimpleBuiltinDataTypeType.UniqueIdentifier:
                    asm.AddToken("nvarchar");
                    break;
                default:
                    throw new Exception(ResStr.MSG_INTERNAL_ERROR);
            }
            asm.End(this);
        }
    }

    public enum SimpleBuiltinDataTypeType
    {
        BigInt, Bit, Int, Money, SmallInt, SmallMoney, TinyInt, Real, Date, DateTime, SmallDateTime, Text, NText, Image,
        HierarchyId, RowVersion, SqlVariant, TimeStamp, UniqueIdentifier
    }

    public class DecimalDataType : BuiltinDataType
    {
        public int Precision { get; set; }
        public int Scale { get; set; }

        static public int PrecisionDefault = 18;
        static public int ScaleDefault = 0;

        public DecimalDataType(int precision, int scale)
        {
            Precision = precision;
            Scale = scale;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("decimal");

            if (Precision != PrecisionDefault || Scale != ScaleDefault)
            {
                asm.AddToken("(");
                asm.Add(Precision);
                asm.AddToken(",");
                asm.AddSpace();
                asm.Add(Scale);
                asm.AddToken(")");
            }
            asm.End(this);
        }
    }

    public class FloatDataType : BuiltinDataType
    {
        public int Mantissa { get; set; }

        static public int MantissaDefault = 53;

        public FloatDataType(int mantissa)
        {
            Mantissa = mantissa;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            asm.AddToken("float");
            if (Mantissa != MantissaDefault)
            {
                asm.AddToken("(");
                asm.Add(Mantissa);
                asm.AddToken(")");
            }
            asm.End(this);
        }
    }

    public class DateTimePrecisionDataType : BuiltinDataType
    {
        public DateTimePrecisionDataTypeType Type { get; set; }
        public int Precision { get; set; }

        static public int PrecisionDefault = 7;

        public DateTimePrecisionDataType(DateTimePrecisionDataTypeType type, int precision)
        {
            Type = type;
            Precision = precision;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Type)
            {
                case DateTimePrecisionDataTypeType.DateTime2:
                    asm.AddToken("timestamp");
                    break;
                case DateTimePrecisionDataTypeType.DateTimeOffset:
                    AddNote(Note.STRINGIFIER, ResStr.NO_DATETIMEOFFSET);
                    return;
                case DateTimePrecisionDataTypeType.Time:
                    asm.AddToken("time");
                    break;
            }

            // DateTime2 and Time do not support precision on Hana.
            if (Precision != PrecisionDefault)
            {
                AddNote(Note.MODIFIER, ResStr.MSG_REMOVED_LENGTHS_FOR_DATETIME2);
            }
            asm.End(this);
        }
    }


    public enum DateTimePrecisionDataTypeType
    {
        DateTime2, DateTimeOffset, Time
    }

    public class StringWithLengthDataType : BuiltinDataType
    {
        public StringWithLengthDataTypeType Type { get; set; }
        public int Length { get; set; }

        public StringWithLengthDataType(StringWithLengthDataTypeType type, int length)
        {
            Type = type;
            Length = length;
        }

        override public void Assembly(Assembler asm)
        {
            switch (Type)
            {
                case StringWithLengthDataTypeType.Char:
                    asm.AddToken("char");
                    break;
                case StringWithLengthDataTypeType.NChar:
                    asm.AddToken("nchar");
                    break;
                case StringWithLengthDataTypeType.Binary:
                    asm.AddToken("binary");
                    break;
                case StringWithLengthDataTypeType.VarChar:
                    asm.AddToken("varchar");
                    break;
                case StringWithLengthDataTypeType.NVarChar:
                    asm.AddToken("nvarchar");
                    break;
                case StringWithLengthDataTypeType.VarBinary:
                    asm.AddToken("varbinary");
                    break;
            }

            if (Length == -1)
            {
                asm.AddToken("(");
                asm.AddToken(Type == StringWithLengthDataTypeType.Binary ? "MAX" : "5000");
                asm.AddToken(")");
            }
            else if (Length != 0)
            {
                asm.AddToken("(");
                asm.Add(Length);
                asm.AddToken(")");
            }
        }
    }

    public enum StringWithLengthDataTypeType
    {
        Char, NChar, Binary, VarChar, NVarChar, VarBinary
    }

    public class GenericDataType : DataType
    {
        public Identifier Schema { get; set; }
        public Identifier Name { get; set; }

        public GenericDataType(Identifier schema, Identifier name)
        {
            Schema = schema;
            Name = name;
        }

        override public void Assembly(Assembler asm)
        {
            asm.Begin(this);
            switch (Name.Name.ToLowerInvariant())
            {
                case "xml":
                    AddNote(Note.STRINGIFIER, ResStr.NO_TYPE_XML);
                    break;
                default:
                    if (Schema != null)
                    {
                        asm.Add(Schema);
                        asm.AddToken(".");
                    }
                    asm.Add(Name);
                    break;
            }
            asm.End(this);
        }
    }
    #endregion

    #region OrderedColumn
    public class OrderedColumn : GrammarNode
    {
        public Identifier Name { get; set; }
        public OrderDirection Direction { get; set; }

        public OrderedColumn(Identifier name, OrderDirection direction)
        {
            Name = name;
            Direction = direction;
        }

        public override void Assembly(Assembler asm)
        {
            asm.Add(Name);
            switch (Direction)
            {
                case OrderDirection.Nothing:
                    asm.AddToken("");
                    break;
                case OrderDirection.Ascending:
                    asm.AddToken(" ASC");
                    break;
                case OrderDirection.Descending:
                    asm.AddToken(" DESC");
                    break;
            }
        }
    }

    public enum OrderDirection
    {
        Nothing, Ascending, Descending
    }
    #endregion

    public enum AssignmentType
    {
        AddAssign,
        SubAssign,
        MulAssign,
        DivAssign,
        ModAssign,
        AndAssign,
        XorAssign,
        OrAssign,
        Assign
    }
}
