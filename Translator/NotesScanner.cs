using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Translator
{
    public class NotesScanner : Scanner
    {
        private Dictionary<string, int> info = new Dictionary<string, int>();
        private String[] Filter = null;
        private bool WriteNotes = true;
        public TextWriter Writer = null;
        public List<string> List = null;
        Stack<object> _Stack = new Stack<object>();
        Statement RootStatement = null;

        public NotesScanner(TextWriter writer)
        {
            Writer = writer;
            Filter = new String[] { };    
        }

        public NotesScanner(List<string> list)
        {
            List = list;
            Filter = new String[] { };
        }

        public void SetFilter(String[] filter)
        {
            Filter = filter;
        }

        private bool IsInFilter(string id)
        {
            bool ret = true;
            if (Filter.Length > 0)
            {
                ret = Array.IndexOf(Filter, id) > -1;
            }

            return ret;
        }

        public int NotesCount(string key)
        {
            return info.ContainsKey(key) ? info[key] : 0;
        }

        public void WriteNotesToWritter(bool write)
        {
            WriteNotes = write;
        }

        public void ClearSummaryInfo()
        {
            info.Clear();
        }

        public void DisplaySummaryInfo(TextWriter infoWritter, Dictionary<String, String> messages)
        {
            if (info.Count == 0)
            {
                //infoWritter.WriteLine("No notes/warnings/errors reported to translation of this statements.");
            }
            else
            {
                foreach (KeyValuePair<string, int> pair in info)
                {
                    if (IsInFilter(pair.Key))
                    {
                        if (messages.ContainsKey(pair.Key))
                        {
                            infoWritter.WriteLine("{0} {1}", messages[pair.Key], pair.Value);
                        }
                        else
                        {
                            infoWritter.WriteLine("{0}, {1}", pair.Key, pair.Value);
                        }
                    }
                }    
            }
        }

        public int GetMsgCount(string messageId)
        {
            if (info.ContainsKey(messageId))
            {
                return info[messageId];
            }

            return 0;
        }

        Statement GetNearestStatement()
        {
            foreach (object obj in _Stack)
            {
                if (obj is Statement && (obj as Statement).Terminate)
                    return obj as Statement;
            }
            return null;
        }

        public override void Scan(GrammarNode node)
        {
            if (node is Statement && RootStatement == null)
            {
                RootStatement = node as Statement;
            }
            _Stack.Push(node);
            base.Scan(node);
            _Stack.Pop();
        }

        public override bool Action(GrammarNode node)
        {
            foreach (Note note in node.TranslationNotes)
            {
                if (IsInFilter(note.ID))
                {
                    if (info.ContainsKey(note.ID)) {
                        info[note.ID] += 1;
                    }
                    else {
                        info[note.ID] = 1;
                    }

                    if (WriteNotes && RootStatement == GetNearestStatement())
                    {
                        if (Writer != null)
                        {
                            Writer.WriteLine("--[Note:{0}] " + note.Value, note.ID);
                        }
                        else if (List != null)
                        {
                            List.Add(string.Format("--[Note:{0}] {1}", note.ID, note.Value));
                        }
                        else
                        {
                            Console.WriteLine(note.Value);
                        }
                    }
                }
            }

            return true;
        }
    }
}