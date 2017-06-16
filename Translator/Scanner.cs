using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    public class Scanner
    {
        private int cntNode = 0;
        private int level = 0;
        private const string METHOD_ACTION = "Action";
        private const string METHOD_POST_ACTION = "PostAction";
        private const string METHOD_PRE_ACTION = "PreAction";

        public GrammarNodeInfo ChildInfo { get; private set; }

        List<MethodInfo> _MethodsAction = new List<MethodInfo>();
        List<MethodInfo> _MethodsPreAction = new List<MethodInfo>();
        List<MethodInfo> _MethodsPostAction = new List<MethodInfo>();
        public Scanner()
        {
            foreach (MethodInfo method in GetType().GetMethods())
            {
                if (method.Name == METHOD_ACTION)
                {
                    ParameterInfo[] paramInfo = method.GetParameters();

                    if (paramInfo.Length == 1)
                    {
                        _MethodsAction.Add(method);
                    }
                    else
                    {
                        Debug.Assert(false, "Incorrect Action definition");
                    }
                }
                else if (method.Name == METHOD_POST_ACTION)
                {
                    ParameterInfo[] paramInfo = method.GetParameters();

                    if (paramInfo.Length == 1)
                    {
                        _MethodsPostAction.Add(method);
                    }
                    else
                    {
                        Debug.Assert(false, "Incorrect PostAction definition");
                    }
                }
                else if (method.Name == METHOD_PRE_ACTION)
                {
                    ParameterInfo[] paramInfo = method.GetParameters();

                    if (paramInfo.Length == 1)
                    {
                        _MethodsPreAction.Add(method);
                    }
                    else
                    {
                        Debug.Assert(false, "Incorrect PreAction definition");
                    }
                }
            }
        }

        public static List<Type> GetTypes(Type t)
        {
            List<Type> set = new List<Type>();
            if (t.IsPrimitive)
            {
                if (!set.Contains(t))
                {
                    set.Add(t);
                }
                return set;
            }
            else
            {
                if (!set.Contains(t))
                {
                    set.Add(t);
                }
                if (t.BaseType != null)
                {
                    var baseTypes = GetTypes(t.BaseType);
                    set.AddRange(baseTypes);
                }
                return set;
            }
        }

        public enum CallActionType { PreActionType, ActionType, PostActionType }

        public bool CallAction(GrammarNode obj, CallActionType type)
        {
            Type objType = obj.GetType();
            List<Type> objTypes = GetTypes(objType);
            List<MethodInfo> list;
            switch (type)
            {
                case CallActionType.PreActionType:
                    list = _MethodsPreAction;
                    break;
                case CallActionType.PostActionType:
                    list = _MethodsPostAction;
                    break;
                default:
                    list = _MethodsAction;
                    break;
            }

            foreach (Type t in objTypes)
            {
                MethodInfo mi = list.Find(s => s.GetParameters()[0].ParameterType == t);
                if (mi != null)
                {
                    object[] paramArray = { obj };
                    return (bool)mi.Invoke(this, paramArray);
                }
            }

            if (type == CallActionType.ActionType)
            {
                Debug.Assert(false, string.Format("Not suitable Action defined for {0}", objType));
            }
            return true;
        }

        private void DebugWrite(GrammarNode node)
        {
            Console.Write(cntNode + ".");

            for(int i = 0; i < level; ++i)
            {
                Console.Write("  ");
            }

            if (node != null)
            {
                Console.WriteLine(node.GetType().ToString());
            }
            else
            {
                Console.WriteLine("NULL value");
            }
            ++cntNode;
        }

        public virtual void Scan(GrammarNode node)
        {
            //DebugWrite(node);

            //Sometimes in lists we can have null items when grammar can't process the node, returns null
            if (node != null)
            {
                StatusReporter.ReportProgress(node);

                CallAction(node, CallActionType.PreActionType);
                if (CallAction(node, CallActionType.ActionType))
                {
                    ScanChildren(node);
                }
                CallAction(node, CallActionType.PostActionType);
            }

            ChildInfo = null;
        }

        public virtual void ScanChildren(GrammarNode node)
        {
            foreach (KeyValuePair<GrammarNode, GrammarNodeInfo> g in node.Children)
            {
                ++level;
                ChildInfo = g.Value;
                Scan(g.Key);
                --level;
            }
        }

        virtual public bool Action(GrammarNode child) { return true; }
    }
}
