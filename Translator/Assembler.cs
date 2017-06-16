using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    abstract public class Assembler
    {
        public const string FLAG_HANA_CONCAT = "HANAConcat";

        private HashSet<string> flags   = new HashSet<string>();

        abstract public void Clear();

        virtual public void NewLine()
        {
        }
        virtual public void IncreaseIndentation()
        {
        }
        virtual public void DecreaseIndentation()
        {
        }
        virtual public void Breakable()
        {
        }
        virtual public void Begin(GrammarNode node)
        {
        }
        virtual public void End(GrammarNode node)
        {
        }

        public void SetFlag(string flag)
        {
            flags.Add(flag);
        }

        public void ClearFlag(string flag)
        {
            flags.Remove(flag);
        }

        public bool IsFlag(string flag)
        {
            return flags.Contains(flag);
        }

        abstract public void AddSpace();
        abstract public void AddToken(string right);
        abstract public void Add(string right);
        abstract public void Add(decimal right);
        abstract public void Add(int right);

        virtual public void Add(GrammarNode node)
        {
            node.Assembly(this);
        }

        virtual public void Add(Statement stmt)
        {
            stmt.Assembly(this);
        }

        virtual public void HandleComments(GrammarNode node)
        {
        }
    }
}
