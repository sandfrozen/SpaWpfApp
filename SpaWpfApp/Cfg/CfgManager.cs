using SpaWpfApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Cfg
{
    public class CfgManager : CfgAPI
    {
        List<ProcedureCfg> CfgList;


        public CfgManager(string sourceCode)
        {
            this.CfgList = new List<ProcedureCfg>();

            this.BuildCfgList(sourceCode);
        }


        #region methods to build Cfg
        private void BuildCfgList(string p_sourceCode)
        {
            GNode currentPreviousNode = null;
            GNode actualNode = null;
            ProcedureCfg actualCfgStructure = null;
            string[] sourceCodeLines;
            string[] lineWords;
            int programLineNumber = 0;
            int howManyStatementsEnd = 0;

            sourceCodeLines = p_sourceCode.Split('\n');

            for (int i = 0; i < sourceCodeLines.Length - 1; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');
                switch (lineWords[0])
                {
                    case "if":
                        {
                            BuildIf(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);
                        }
                        break;

                    case "while":
                        {
                            BuildWhile(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);
                        }
                        break;

                    case "procedure":
                        {
                            if (actualCfgStructure != null)
                            {
                                actualCfgStructure.lastProgramLineNumber = programLineNumber;
                                CfgList.Add(actualCfgStructure);
                            }
                            actualCfgStructure = new ProcedureCfg();
                            actualNode = null;
                            currentPreviousNode = null;
                        }
                        break;

                    default:
                        {
                            BuildAssignCall(ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber);
                        }
                        break;
                }
            }

            actualCfgStructure.lastProgramLineNumber = programLineNumber;
            this.CfgList.Add(actualCfgStructure);
        }

        private void BuildAssignCall(ref GNode currentPreviousNode, ref GNode actualNode, ref ProcedureCfg actualCfgStructure, ref int programLineNumber)
        {
            if (actualNode == null) // jesli pierwsza instrukcja w procedurze
            {
                actualNode = new GNode(GNodeTypeEnum.StmtLst, ++programLineNumber);
                actualCfgStructure.GNodeList.Add(actualNode);
            }
            else if (actualNode.type != GNodeTypeEnum.StmtLst) // jesli pierwsza instrukcja w while/if
            {
                currentPreviousNode = actualNode;
                actualNode = new GNode(GNodeTypeEnum.StmtLst, ++programLineNumber);
                actualCfgStructure.GNodeList.Add(actualNode);

                currentPreviousNode.nextGNodeList.Add(actualNode);
                actualNode.previousGNodeList.Add(currentPreviousNode);
            }
            else
            {
                actualNode.programLineList.Add(++programLineNumber);
            }
        }
        private void BuildIf(string[] sourceCodeLines, ref int i, ref GNode currentPreviousNode, ref GNode actualNode, ref ProcedureCfg actualCfgStructure, ref int programLineNumber, ref int howManyStatementsEnd)
        {
            string[] lineWords;
            GNode ifNodeMain, endNodeThenSection = null, ghostNode;

            #region create node for if and remember this node and endNodeThenSection
            currentPreviousNode = actualNode;

            actualNode = new GNode(GNodeTypeEnum.If, ++programLineNumber);
            actualCfgStructure.GNodeList.Add(actualNode);

            // zapamietaj
            ifNodeMain = actualNode;

            if (currentPreviousNode != null)
            {
                currentPreviousNode.nextGNodeList.Add(actualNode);
                actualNode.previousGNodeList.Add(currentPreviousNode);
            }
            #endregion

            for (++i; i < sourceCodeLines.Length - 1; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "else":
                        {
                            BuildElse(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);

                            ghostNode = new GNode(GNodeTypeEnum.Ghost);
                            actualCfgStructure.GNodeList.Add(ghostNode);

                            actualNode.nextGNodeList.Add(ghostNode);
                            endNodeThenSection.nextGNodeList.Add(ghostNode);

                            ghostNode.previousGNodeList.Add(actualNode);
                            ghostNode.previousGNodeList.Add(endNodeThenSection);

                            currentPreviousNode = null;
                            actualNode = ghostNode;

                            return;
                        }
                        break;

                    case "while":
                        {
                            BuildWhile(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);
                        }
                        break;

                    case "if":
                        {
                            BuildIf(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);
                        }
                        break;

                    default:
                        {
                            BuildAssignCall( ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber);

                            if (EndOfStatement(lineWords, ref howManyStatementsEnd)) // end of then section
                            {
                                endNodeThenSection = actualNode;
                                actualNode = ifNodeMain;
                                --howManyStatementsEnd;
                            }
                        }
                        break;
                }
            }
        }
        private void BuildElse(string[] sourceCodeLines, ref int i, ref GNode currentPreviousNode, ref GNode actualNode, ref ProcedureCfg actualCfgStructure, ref int programLineNumber, ref int howManyStatementsEnd)
        {
            string[] lineWords;

            for (++i; i < sourceCodeLines.Length - 1; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "while":
                        {
                            BuildWhile(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);
                            if (howManyStatementsEnd > 0)
                            {
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;

                    case "if":
                        {
                            BuildIf(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);

                            if (howManyStatementsEnd > 0)
                            {
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;

                    default:
                        {
                            BuildAssignCall(ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber);

                            if (EndOfStatement(lineWords, ref howManyStatementsEnd))
                            {
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;
                }
            }
        }
        private void BuildWhile(string[] sourceCodeLines, ref int i, ref GNode currentPreviousNode, ref GNode actualNode, ref ProcedureCfg actualCfgStructure, ref int programLineNumber, ref int howManyStatementsEnd)
        {
            string[] lineWords;
            GNode whileNodeMain;

            #region create node for while and remember it
            currentPreviousNode = actualNode;

            actualNode = new GNode(GNodeTypeEnum.While, ++programLineNumber);
            actualCfgStructure.GNodeList.Add(actualNode);

            //zapamietaj
            whileNodeMain = actualNode;

            if (currentPreviousNode != null)
            {
                currentPreviousNode.nextGNodeList.Add(actualNode);
                actualNode.previousGNodeList.Add(currentPreviousNode);
            }
            currentPreviousNode = actualNode;
            #endregion

            for (++i; i < sourceCodeLines.Length - 1; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "while":
                        {
                            BuildWhile(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);
                            if (howManyStatementsEnd > 0)
                            {
                                CloseWhileLoop(ref whileNodeMain, ref actualNode, ref currentPreviousNode, ref howManyStatementsEnd);
                                return;
                            }
                        }
                        break;

                    case "if":
                        {
                            BuildIf(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);
                            if (howManyStatementsEnd > 0)
                            {
                                CloseWhileLoop(ref whileNodeMain, ref actualNode, ref currentPreviousNode, ref howManyStatementsEnd);
                                return;
                            }
                        }
                        break;

                    default:
                        {
                            BuildAssignCall(ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber);

                            if (EndOfStatement(lineWords, ref howManyStatementsEnd))
                            {
                                CloseWhileLoop(ref whileNodeMain, ref actualNode, ref currentPreviousNode, ref howManyStatementsEnd);
                                return;
                            }
                        }
                        break;
                }
            }
        }

        private void CloseWhileLoop(ref GNode whileNodeMain, ref GNode actualNode, ref GNode currentPreviousNode, ref int howManyStatementsEnd)
        {
            actualNode.nextGNodeList.Add(whileNodeMain);
            whileNodeMain.previousGNodeList.Add(actualNode);

            currentPreviousNode = null;
            actualNode = whileNodeMain;
            --howManyStatementsEnd;
        }
        private bool EndOfStatement(string[] lineWords, ref int howManyStatementsEnd)
        {
            howManyStatementsEnd = DetermineHowManyStatementsEnd(lineWords);

            return howManyStatementsEnd > 0 ? true : false;
        }
        private int DetermineHowManyStatementsEnd(string[] lineWords)
        {
            int i = lineWords.Length - 1;
            int closeBracketCounter = 0;

            while (lineWords[i--][0] == 125) // '}'
            { closeBracketCounter++; }

            return closeBracketCounter;
        }
        #endregion


        #region API methods
        public List<int> Next(int p_programLineNumber)
        {
            List<int> resultList = new List<int>();
            ProcedureCfg cfg = FindCfg(p_programLineNumber);
            int programLinesInNode;

            GNode actual = cfg.GNodeList.Where(p => p.programLineList.Contains(p_programLineNumber)).FirstOrDefault();

            programLinesInNode = actual.programLineList.Count();
            if (programLinesInNode > 1) // jesli next w tym samym nodzie
            {
                for (int i = 0; i < programLinesInNode; i++)
                {
                    if (actual.programLineList[i] == p_programLineNumber &&
                        (i + 1) < programLinesInNode)
                    {
                        resultList.Add(actual.programLineList.ElementAt(i + 1));
                        break;
                    }
                }
            }
            else if (actual.nextGNodeList != null)
            {
                foreach (var nodeNext in actual.nextGNodeList)
                {
                    FindAndAddNextResult(nodeNext, ref resultList);
                }
            }
            else
            {
                resultList.Add(-1);
            }

            return resultList;
        }

        public List<int> NextS(int p_programLineNumber)
        {
            List<int> resultList = new List<int>();
            ProcedureCfg cfg = FindCfg(p_programLineNumber);
            int programLinesInNode;

            GNode actual = cfg.GNodeList.Where(p => p.programLineList.Contains(p_programLineNumber)).FirstOrDefault();

            programLinesInNode = actual.programLineList.Count();

            if (programLinesInNode > 1) // jesli nextS w tym samym nodzie
            {
                for (int i = 0; i < programLinesInNode; i++)
                {
                    if (actual.programLineList[i] == p_programLineNumber &&
                        (i + 1) < programLinesInNode)
                    {
                        for (int j = i + 1; j < programLinesInNode; j++)
                        {
                            resultList.Add(actual.programLineList.ElementAt(j));
                        }
                    }
                }
            }

            FindAndAddAllNextSInNextNodes(actual, ref resultList);

            if(resultList.Count() == 0)
            {
                resultList.Add(-1);
            }

            return resultList;
        }


        public List<int> Previous(int p_programLineNumber)
        {
            List<int> resultList = new List<int>();
            ProcedureCfg cfg = FindCfg(p_programLineNumber);
            int programLinesInNode;

            GNode actual = cfg.GNodeList.Where(p => p.programLineList.Contains(p_programLineNumber)).FirstOrDefault();

            programLinesInNode = actual.programLineList.Count();
            if (programLinesInNode > 1) // jesli previous w tym samym nodzie
            {
                for (int i = 0; i < programLinesInNode; i++)
                {
                    if (actual.programLineList[i] == p_programLineNumber &&
                        i != 0)
                    {
                        resultList.Add(actual.programLineList.ElementAt(i - 1));
                    }
                }
            }
            else if (actual.previousGNodeList != null)
            {
                foreach (var nodePrevious in actual.previousGNodeList)
                {
                    FindAndAddPreviousResult(nodePrevious, ref resultList);
                }
            }
            else
            {
                resultList.Add(-1);
            }

            return resultList;
        }

        public List<int> PreviousS(int p_programLineNumber)
        {
            List<int> resultList = new List<int>();
            ProcedureCfg cfg = FindCfg(p_programLineNumber);
            int programLinesInNode;

            GNode actual = cfg.GNodeList.Where(p => p.programLineList.Contains(p_programLineNumber)).FirstOrDefault();

            programLinesInNode = actual.programLineList.Count();

            if (programLinesInNode > 1) // jesli previousS w tym samym nodzie
            {
                for (int i = 1; i < programLinesInNode; i++)
                {
                    if (actual.programLineList[i] == p_programLineNumber)
                    {
                        for (int j = i - 1; j >= 0; j--)
                        {
                            resultList.Add(actual.programLineList.ElementAt(j));
                        }
                    }
                }
            }

            FindAndAddAllPreviousSInPreviousNodes(actual, ref resultList);

            if(resultList.Count() == 0)
            {
                resultList.Add(-1);
            }
            return resultList;
        }

        public bool IsNext(int p1, int p2)
        {
            List<int> nextList = this.Next(p1);

            foreach(var v in nextList)
            {
                if(v == p2)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsNextS(int p1, int p2)
        {
            ProcedureCfg cfg = FindCfg(p1);
            int programLinesInNode;

            GNode actual = cfg.GNodeList.Where(p => p.programLineList.Contains(p1)).FirstOrDefault();

            programLinesInNode = actual.programLineList.Count();

            if (programLinesInNode > 1) // jesli nextS w tym samym nodzie
            {
                for (int i = 0; i < programLinesInNode; i++)
                {
                    if (actual.programLineList[i] == p1 &&
                        (i + 1) < programLinesInNode)
                    {
                        for (int j = i + 1; j < programLinesInNode; j++)
                        {
                            if(actual.programLineList.ElementAt(j) == p2)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return CheckIfNextSInNextNodes(actual, p2);
        }

        private Boolean CheckIfNextSInNextNodes(GNode actual, int p2)
        {
            foreach (var n in actual.nextGNodeList)
            {
                if (n.type != GNodeTypeEnum.Ghost)
                {
                    foreach (var i in n.programLineList)
                    {
                        if(i == p2)
                        {
                            return true;
                        }
                    }
                }

                if (n.nextGNodeList.Count() > 0)
                {
                    CheckIfNextSInNextNodes(n, p2);
                }
            }

            return false;
        }

        #endregion


        #region helpful methods for API
        private ProcedureCfg FindCfg(int programLineNumber)
        {
            if (programLineNumber < 1) { return null; }

            for (int i = 0; i < CfgList.Count(); i++)
            {
                if (programLineNumber >= CfgList[i].GNodeList.First().programLineList.First() &&
                    programLineNumber < CfgList[i].lastProgramLineNumber)
                {
                    return CfgList[i];
                }
            }
            return null;
        }
        private void FindAndAddNextResult(GNode nodeNext, ref List<int> resultList)
        {
            if (nodeNext.type != GNodeTypeEnum.Ghost)
            {
                resultList.Add(nodeNext.programLineList[0]);
                return;
            }

            foreach (var n in nodeNext.nextGNodeList)
            {
                FindAndAddNextResult(n, ref resultList);
            }
        }
        private void FindAndAddPreviousResult(GNode nodePrevious, ref List<int> resultList)
        {
            if (nodePrevious.type != GNodeTypeEnum.Ghost)
            {
                resultList.Add(nodePrevious.programLineList.Last());
                return;
            }

            foreach (var p in nodePrevious.previousGNodeList)
            {
                FindAndAddPreviousResult(p, ref resultList);
            }
        }
        private void FindAndAddAllNextSInNextNodes(GNode actual, ref List<int> resultList)
        {
            foreach (var n in actual.nextGNodeList)
            {
                if (n.type != GNodeTypeEnum.Ghost)
                {
                    foreach (var i in n.programLineList)
                    {
                        resultList.Add(i);
                    }
                }

                if (n.nextGNodeList.Count() > 0)
                {
                    FindAndAddAllNextSInNextNodes(n, ref resultList);
                }
            }
        }
        private void FindAndAddAllPreviousSInPreviousNodes(GNode actual, ref List<int> resultList)
        {
            foreach (var p in actual.previousGNodeList)
            {
                if (p.type != GNodeTypeEnum.Ghost)
                {
                    if (resultList.Contains(p.programLineList[0])) // jesli resultList zawiera element z tego noda, to ścieżki powrotów się złączyły i należy przerwać powielaną ściężkę
                    {
                        return;
                    }
                    for (int i = p.programLineList.Count() - 1; i >= 0; i--)
                    {
                        resultList.Add(p.programLineList[i]);
                    }
                }

                if (p.previousGNodeList.Count() > 0)
                {
                    FindAndAddAllPreviousSInPreviousNodes(p, ref resultList);
                }
            }
        }
        #endregion
    }
}
