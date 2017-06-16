using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace Translator
{
    static class TreePrinter
    {
        public static string Print(string name, object value)
        {
            StringBuilder output = new StringBuilder();
            Print(output, name, value, 0, new Dictionary<int, bool>());
            return output.ToString();
        }

        static Type[] PrintableTypes =
        {
            typeof(bool), typeof(byte), typeof(char), typeof(DateTime), typeof(decimal),
            typeof(double), typeof(float), typeof(int), typeof(long), typeof(sbyte),
            typeof(short), typeof(string), typeof(uint), typeof(ulong), typeof(ushort)
        };

        static void Print(StringBuilder output, string name, object value, int level, Dictionary<int, bool> isLast)
        {
            PrintLine(output, name, value, level, isLast);

            if (value != null)
            {
                IEnumerable enumerable = value as IEnumerable;
                if (enumerable != null && !(value is String))
                {
                    IEnumerator enumerator = enumerable.GetEnumerator();
                    int index = 0;

                    bool hasMore = enumerator.MoveNext();
                    while (hasMore)
                    {
                        object current = enumerator.Current;
                        hasMore = enumerator.MoveNext();

                        isLast[level + 1] = !hasMore;
                        Print(output, String.Format("[{0}]", index), current, level + 1, isLast);
                        index++;
                    }
                }
                else if (!(value is ValueType) && !(value is String))
                {
                    PropertyInfo lastProperty = null;

                    foreach (PropertyInfo property in value.GetType().GetProperties())
                    {
                        object[] attrs = property.GetCustomAttributes(typeof(ExcludeFromChildrenListAttribute), true);
                        if (attrs.Length > 0)
                        {
                            continue;
                        }

                        MethodInfo getter = property.GetGetMethod(false);
                        if (getter != null && getter.GetParameters().Length == 0 &&
                            getter.GetGenericArguments().Length == 0)
                        {
                            lastProperty = property;
                        }
                    }

                    foreach (PropertyInfo property in value.GetType().GetProperties())
                    {
                        object[] attrs = property.GetCustomAttributes(typeof(ExcludeFromChildrenListAttribute), true);
                        if (attrs.Length > 0)
                        {
                            continue;
                        }

                        MethodInfo getter = property.GetGetMethod(false);
                        if (getter != null && getter.GetParameters().Length == 0 &&
                            getter.GetGenericArguments().Length == 0)
                        {
                            isLast[level + 1] = (property == lastProperty);
                            Print(output, property.Name, property.GetValue(value, null), level + 1, isLast);
                        }
                    }
                }
            }
        }

        //statement: SelectStatement
        // |- SelectClause: SelectClause
        // |   |- Property1: value1
        // |   \- Property2: (null)
        // \- FromClause: IList<TableColumn>
        //     |- [0]: value3
        //     \- [1]: (null)
        //         \- Property: 42

        static void PrintLine(StringBuilder output, string name, object value, int level, Dictionary<int, bool> isLast)
        {
            for (int indent = 1; indent < level; indent++)
            {
                output.Append(isLast[indent] ? "    " : " |  ");
            }
            if (level > 0)
            {
                output.Append(isLast[level] ? " \\- " : " |- ");
            }

            string valueString;
            if (value == null)
            {
                valueString = "(null)";
            }
            else if (value.GetType().IsEnum || PrintableTypes.Any(pt => pt.IsAssignableFrom(value.GetType())))
            {
                valueString = value.ToString();
            }
            else
            {
                valueString = value.GetType().Name;
            }

            output.AppendFormat("{0}: {1}", name, valueString);
            output.AppendLine();
        }
    }
}
