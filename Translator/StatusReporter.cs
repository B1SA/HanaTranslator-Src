using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Translator
{
    public static class StatusReporter
    {
        private static TextWriter       writer       = null;

        // Progress step variables
        private static string           stepMsg      = string.Empty;
        private static string           nodeMsg      = string.Empty;
        private static int              nodeCnt      = 0;
        private static int              stepCnt      = 0;

        // Summary status variables
        private static Statement        inputQueries = null;
        private static int              numInQueries = 0;
        private static Statement        outputQueries = null;
        private static int              numOutQueries = 0;
        //private static int              numConversions = 0;
        //private static int              numCaseFixing = 0;

        public static int InputQueriesCount
        {
            get { return numInQueries; }
        }

        public static int OutputQueriesCount
        {
            get { return numOutQueries; }
        }

        public static Statement InputQueries
        {
            get { return inputQueries; }
        }

        public static Statement OutputQueries
        {
            get { return outputQueries; }
        }

        public static void Initialize(TextWriter textWriter)
        {
            writer = textWriter;
        }

        public static int Finish()
        {
            writer = null;
            return nodeCnt;
        }

        public static int GetCount()
        {
            return nodeCnt;
        }

        public static bool IsInitialized()
        {
            return writer != null;
        }

        public static void SetStage(string stageName, string stepMessage)
        {
            if (IsInitialized())
            {
                writer.WriteLine(stageName);
                nodeMsg = stepMessage;
                stepMsg = string.Empty;
                nodeCnt = 0;
                stepCnt = 0;
            }
        }

        public static void SetStage(string stepMessage)
        {
            if (IsInitialized())
            {
                stepMsg = stepMessage;
                nodeMsg = string.Empty;
                nodeCnt = 0;
                stepCnt = 0;
            }
        }

        public static void ReportProgress()
        {
            if (IsInitialized() )
            {
                ++stepCnt;
                ProgressPrint(stepMsg, stepCnt);
            }
        }

        public static void ReportProgress(GrammarNode node)
        {
            if (IsInitialized() && IsNodeCountable(node))
            {
                ++nodeCnt;
                ProgressPrint(nodeMsg, nodeCnt);
            }
        }

        private static void ProgressPrint(string msg, int value)
        {
            if (msg != string.Empty)
            {
                if ((value % 10) == 0 || value == 1)
                {
                    writer.WriteLine();
                    string padStr = (value < 10) ? "  " : " ";
                    if (value >= 100)
                    {
                        padStr = "";
                    }
                    writer.Write("      {0} {1}{2}", msg, padStr, value);
                }
                else
                {
                    writer.Write(" {0}", value % 10);
                }
            }
        }

        public static void Message(string msg)
        {
            if (IsInitialized())
            {
                writer.WriteLine(msg);
            }
        }

        private static bool IsNodeCountable(GrammarNode node)
        {
            if (node == null || !(node is Statement) || node is BlockStatement || node is SqlStartStatement)
            {
                return false;
            }

            if (node is BreakStatement || node is GoStatement || node is ContinueStatement)
            {
                return false;
            }

            //Console.WriteLine("Node type = " + node.GetType().Name);
            return true;
        }

        public static void SetInputQueries(Statement rootStatement)
        {
            inputQueries = rootStatement;

            Scanner sc = new Scanner();
            sc.Scan(inputQueries);
            numInQueries = StatusReporter.GetCount();

            //First statement can be StartSQLStatement
            if (numInQueries > 1)
            {
                --numInQueries;
            }
        }

        public static void SetOutputQueries(Statement rootStatement, int queriesCount)
        {
            outputQueries = rootStatement;
            numOutQueries = queriesCount;
        }
    }
}
