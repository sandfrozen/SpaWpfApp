using SpaWpfApp.Ast;
using SpaWpfApp.Enums;
using SpaWpfApp.PkbNew;
using SpaWpfApp.QueryProcessingSusbsytem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Cfg
{
    public class CfgManager : CfgAPI
    {
        public List<ProcedureCfg> CfgList { get; set; }
        private PkbAPI pkb;

        private static CfgManager instance;
        public static CfgManager GetInstance()
        {
            if (instance == null)
            {
                instance = new CfgManager();
            }
            return instance;
        }

        public CfgManager() { }

        public void GenerateStructure(string sourceCode, PkbAPI pkb)
        {
            this.CfgList = new List<ProcedureCfg>();
            this.pkb = pkb;

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

            for (int i = 0; i < sourceCodeLines.Length; i++)
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
                            actualCfgStructure = new ProcedureCfg(pkb.GetProcIndex(lineWords[1]));
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
                actualNode = new GNode(GNodeTypeEnum.StmtLst, ++programLineNumber, actualCfgStructure.GNodeList.Count());
                actualCfgStructure.GNodeList.Add(actualNode);
            }
            else if(actualNode != null && actualNode.type == GNodeTypeEnum.Ghost)
            {
                currentPreviousNode = actualNode;
                actualNode = new GNode(GNodeTypeEnum.StmtLst, ++programLineNumber, actualCfgStructure.GNodeList.Count());
                actualCfgStructure.GNodeList.Add(actualNode);

                currentPreviousNode.nextGNodeList.Add(actualNode);
            }
            else if (actualNode.type == GNodeTypeEnum.While || actualNode.type == GNodeTypeEnum.If) // jesli pierwsza instrukcja w while/if
            {
                currentPreviousNode = actualNode;
                actualNode = new GNode(GNodeTypeEnum.StmtLst, ++programLineNumber, actualCfgStructure.GNodeList.Count());
                actualCfgStructure.GNodeList.Add(actualNode);

                currentPreviousNode.nextGNodeList.Add(actualNode);
                //if(currentPreviousNode.type == GNodeTypeEnum.While) { actualNode.previousGNodeList.Add(currentPreviousNode); }
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

            actualNode = new GNode(GNodeTypeEnum.If, ++programLineNumber, actualCfgStructure.GNodeList.Count());
            actualCfgStructure.GNodeList.Add(actualNode);

            // zapamietaj
            ifNodeMain = actualNode;

            if (currentPreviousNode != null)
            {
                currentPreviousNode.nextGNodeList.Add(actualNode);
                //actualNode.previousGNodeList.Add(currentPreviousNode);
            }
            #endregion

            for (++i; i < sourceCodeLines.Length; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "else":
                        {
                            BuildElse(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);

                            ghostNode = new GNode(GNodeTypeEnum.Ghost, actualCfgStructure.GNodeList.Count());
                            actualCfgStructure.GNodeList.Add(ghostNode);

                            actualNode.nextGNodeList.Add(ghostNode);
                            endNodeThenSection.nextGNodeList.Add(ghostNode);

                            //ghostNode.previousGNodeList.Add(actualNode);
                            //ghostNode.previousGNodeList.Add(endNodeThenSection);

                            currentPreviousNode = null;
                            actualNode = ghostNode;

                            return;
                        }
                        break;

                    case "while":
                        {
                            BuildWhile(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);

                            if (howManyStatementsEnd > 0) // end of then section
                            {
                                endNodeThenSection = actualNode;
                                actualNode = ifNodeMain;
                                --howManyStatementsEnd;
                            }
                        }
                        break;

                    case "if":
                        {
                            BuildIf(sourceCodeLines, ref i, ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber, ref howManyStatementsEnd);


                            if (howManyStatementsEnd > 0) // end of then section
                            {
                                endNodeThenSection = actualNode;
                                actualNode = ifNodeMain;
                                --howManyStatementsEnd;
                            }
                        }
                        break;

                    default:
                        {
                            BuildAssignCall(ref currentPreviousNode, ref actualNode, ref actualCfgStructure, ref programLineNumber);

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

            for (++i; i < sourceCodeLines.Length; i++)
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

            actualNode = new GNode(GNodeTypeEnum.While, ++programLineNumber, actualCfgStructure.GNodeList.Count());
            actualCfgStructure.GNodeList.Add(actualNode);

            //zapamietaj
            whileNodeMain = actualNode;

            if (currentPreviousNode != null)
            {
                currentPreviousNode.nextGNodeList.Add(actualNode);
                //actualNode.previousGNodeList.Add(currentPreviousNode);
            }
            currentPreviousNode = actualNode;
            #endregion

            for (++i; i < sourceCodeLines.Length; i++)
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
        public List<TNode> Next(TNode p_from, string p_to)
        {
            if (p_from is null) { return null; }

            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_to);
            TNode tmp;
            List<TNode> resultList = new List<TNode>();
            ProcedureCfg cfg = FindCfg((int)p_from.programLine);
            int programLinesInNode;

            GNode actual = cfg.GNodeList.Where(p => p.programLineList.Contains((int)p_from.programLine)).FirstOrDefault();

            programLinesInNode = actual.programLineList.Count();
            if (programLinesInNode > 1) // jesli next w tym samym nodzie
            {
                for (int i = 0; i < programLinesInNode; i++)
                {
                    if (actual.programLineList[i] == (int)p_from.programLine &&
                        (i + 1) < programLinesInNode)
                    {
                        tmp = AstManager.GetInstance().FindNode(actual.programLineList.ElementAt(i + 1));
                        if (acceptableType.Contains(tmp.type)) { resultList.Add(tmp); }

                        break;
                    }
                }
            }
            else if (actual.nextGNodeList != null)
            {
                foreach (var nodeNext in actual.nextGNodeList)
                {
                    FindAndAddNextResult(nodeNext, ref resultList, acceptableType);
                }
            }
            else
            {
            }

            return resultList.Count > 0 ? resultList : null;
        }

        public List<TNode> NextX(TNode p_from, string p_to)
        {
            if (p_from is null) { return null; }

            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_to);


            List<TNode> resultList = new List<TNode>();
            ProcedureCfg cfg = FindCfg((int)p_from.programLine);
            List<int>[] visitorsTable = new List<int>[cfg.GNodeList.Count()];
            for (int i = 0; i < visitorsTable.Length; i++) { visitorsTable[i] = new List<int>(); }
            int programLinesInNode;
            TNode tmp;

            GNode actual = cfg.GNodeList.Where(p => p.programLineList.Contains((int)p_from.programLine)).FirstOrDefault();

            programLinesInNode = actual.programLineList.Count();

            if (programLinesInNode > 1) // jesli nextS w tym samym nodzie
            {
                for (int i = 0; i < programLinesInNode; i++)
                {
                    if (actual.programLineList[i] == (int)p_from.programLine &&
                        (i + 1) < programLinesInNode)
                    {
                        for (int j = i + 1; j < programLinesInNode; j++)
                        {
                            tmp = AstManager.GetInstance().FindNode(actual.programLineList.ElementAt(j));
                            if (acceptableType.Contains(tmp.type)) { resultList.Add(tmp); }
                        }
                    }
                }
            }

            FindAndAddAllNextSInNextNodes(actual, ref resultList, ref visitorsTable, acceptableType);


            return resultList.Count > 0 ? resultList : null; ;
        }


        public List<TNode> Previous(TNode p_to, string p_from)
        {
            if (p_to is null) { return null; }
            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_from);

            List<TNode> resultList = new List<TNode>();
            TNode tmp;
            ProcedureCfg cfg = FindCfg((int)p_to.programLine);
            int programLinesInNode;

            GNode actual = cfg.GNodeList.Where(p => p.programLineList.Contains((int)p_to.programLine)).FirstOrDefault();

            programLinesInNode = actual.programLineList.Count();
            if (programLinesInNode > 1) // jesli previous w tym samym nodzie
            {
                for (int i = 0; i < programLinesInNode; i++)
                {
                    if (actual.programLineList[i] == (int)p_to.programLine &&
                        i != 0)
                    {
                        tmp = AstManager.GetInstance().FindNode(actual.programLineList.ElementAt(i - 1));
                        if (acceptableType.Contains(tmp.type)) { resultList.Add(tmp); }

                    }
                }
            }
            else if (actual.previousGNodeList != null)
            {
                foreach (var nodePrevious in actual.previousGNodeList)
                {
                    FindAndAddPreviousResult(nodePrevious, ref resultList, acceptableType);
                }
            }

            return resultList.Count > 0 ? resultList : null;
        }

        public List<TNode> PreviousX(TNode p_to, string p_from)
        {
            if (p_to is null) { return null; }

            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_from);
            List<TNode> candidates;

            if (p_from != "_")
            {
                candidates = AstManager.GetInstance().GetNodes(QueryPreProcessor.GetInstance().declarationsList[p_from]);
            }
            else
            {
                candidates = AstManager.GetInstance().GetNodes("_");
            }
            List<TNode> resultList = new List<TNode>();

            if (candidates is null) { return null; }

            foreach(var c in candidates)
            {
                if(IsNextX((int)c.programLine, (int)p_to.programLine))
                {
                    resultList.Add(c);
                }
            }
            
            return resultList.Count > 0 ? resultList : null;
        }

        public bool IsNext(int p1, int p2)
        {
            if (OutOfRange(p1) || OutOfRange(p2)) { return false; }
            List<TNode> nextList = this.Next(AstManager.GetInstance().FindNode(p1), p2.ToString());

            foreach (var v in nextList)
            {
                if (v.programLine == p2)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsNextX(int p1, int p2)
        {
            if (OutOfRange(p1) || OutOfRange(p2)) { return false; }

            ProcedureCfg cfg = FindCfg(p1);
            int programLinesInNode;
            Boolean findedNextX = false;
            List<int>[] visitorsTable = new List<int>[cfg.GNodeList.Count()];
            for (int i = 0; i < visitorsTable.Length; i++) { visitorsTable[i] = new List<int>(); }

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
                            if (actual.programLineList.ElementAt(j) == p2)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            CheckIfNextSInNextNodes(actual, p2, ref findedNextX, ref visitorsTable);
            return findedNextX;
        }

        private void CheckIfNextSInNextNodes(GNode actual, int p2, ref Boolean findedNextX, ref List<int>[] visitedTable)
        {
            foreach (var n in actual.nextGNodeList)
            {
                if (!visitedTable[n.index].Contains(n.index))
                {
                    visitedTable[n.index].Add(n.index);
                    if (n.type != GNodeTypeEnum.Ghost)
                    {
                        foreach (var i in n.programLineList)
                        {
                            if (i == p2)
                            {
                                findedNextX = true;
                                return;
                            }
                        }
                    }

                    if (n.nextGNodeList.Count() > 0)
                    {
                        if (!(n.nextGNodeList[0].type != GNodeTypeEnum.Ghost
                            && n.type != GNodeTypeEnum.Ghost
                            && n.programLineList[0] > n.nextGNodeList.First().programLineList[0]))
                        {
                            CheckIfNextSInNextNodes(n, p2, ref findedNextX, ref visitedTable);
                        }
                    }
                }
            }
        }

        #endregion


        #region helpful methods for API
        private ProcedureCfg FindCfg(int programLineNumber)
        {
            if (programLineNumber < 1) { return null; }

            for (int i = 0; i < CfgList.Count(); i++)
            {
                if (programLineNumber >= CfgList[i].GNodeList.First().programLineList.First() &&
                    programLineNumber <= CfgList[i].lastProgramLineNumber)
                {
                    return CfgList[i];
                }
            }
            return null;
        }
        private void FindAndAddNextResult(GNode nodeNext, ref List<TNode> resultList, List<TNodeTypeEnum> acceptableType)
        {
            TNode tmp;
            if (nodeNext.type != GNodeTypeEnum.Ghost)
            {
                tmp = AstManager.GetInstance().FindNode(nodeNext.programLineList[0]);
                if (acceptableType.Contains(tmp.type))
                {
                    resultList.Add(tmp);
                }
                return;
            }

            foreach (var p in nodeNext.nextGNodeList)
            {
                FindAndAddNextResult(p, ref resultList, acceptableType);
            }

        }
        private void FindAndAddPreviousResult(GNode nodePrevious, ref List<TNode> resultList, List<TNodeTypeEnum> acceptableType)
        {
            TNode tmp;
            if (nodePrevious.type != GNodeTypeEnum.Ghost)
            {
                tmp = AstManager.GetInstance().FindNode(nodePrevious.programLineList.Last());
                if (acceptableType.Contains(tmp.type)) { resultList.Add(tmp); }

                return;
            }

            foreach (var p in nodePrevious.previousGNodeList)
            {
                FindAndAddPreviousResult(p, ref resultList, acceptableType);
            }

        }
        private void FindAndAddAllNextSInNextNodes(GNode actual, ref List<TNode> resultList, ref List<int>[] visitedTable, List<TNodeTypeEnum> acceptableType)
        {
            TNode tmp;

            foreach (var n in actual.nextGNodeList)
            {
                if (!visitedTable[n.index].Contains(n.index))
                {
                    visitedTable[n.index].Add(n.index);

                    if (n.type != GNodeTypeEnum.Ghost)
                    {
                        foreach (var i in n.programLineList)
                        {
                            tmp = AstManager.GetInstance().FindNode(i);
                            if (acceptableType.Contains(tmp.type) && !resultList.Contains(tmp)) { resultList.Add(tmp); }
                        }
                    }
                    if (n.nextGNodeList.Count() > 0)
                    {
                        FindAndAddAllNextSInNextNodes(n, ref resultList, ref visitedTable, acceptableType);
                    }
                }
            }
        }
        private void FindAndAddAllPreviousSInPreviousNodes(GNode actual, ref List<TNode> resultList, List<TNodeTypeEnum> acceptableType)
        {
            TNode tmp;
            foreach (var p in actual.previousGNodeList)
            {
                if (p.type != GNodeTypeEnum.Ghost)
                {
                    if (resultList.Contains(AstManager.GetInstance().FindNode(p.programLineList[0]))) // jesli resultList zawiera element z tego noda, to ścieżki powrotów się złączyły i należy przerwać powielaną ściężkę
                    {
                        return;
                    }
                    for (int i = p.programLineList.Count() - 1; i >= 0; i--)
                    {
                        tmp = AstManager.GetInstance().FindNode(p.programLineList[i]);
                        if (acceptableType.Contains(tmp.type)) { resultList.Add(tmp); }

                    }
                }

                if (p.previousGNodeList.Count() > 0)
                {
                    FindAndAddAllPreviousSInPreviousNodes(p, ref resultList, acceptableType);
                }
            }
        }
        #endregion

        private List<TNodeTypeEnum> DetermineAcceptableTypes(string arg)
        {
            List<TNodeTypeEnum> result = new List<TNodeTypeEnum>();
            int tmp;

            if (arg == "_" || Int32.TryParse(arg, out tmp))
            {
                foreach (TNodeTypeEnum v in Enum.GetValues(typeof(TNodeTypeEnum)))
                {
                    result.Add(v);
                }
                return result;
            }

            string argType = QueryPreProcessor.GetInstance().declarationsList[arg];
            switch (argType)
            {
                case "procedure":
                    {
                        result.Add(TNodeTypeEnum.Procedure);
                    }
                    break;

                case "call":
                    {
                        result.Add(TNodeTypeEnum.Call);
                    }
                    break;

                case "assign":
                    {
                        result.Add(TNodeTypeEnum.Assign);
                    }
                    break;

                case "if":
                    {
                        result.Add(TNodeTypeEnum.If);
                    }
                    break;

                case "while":
                    {
                        result.Add(TNodeTypeEnum.While);
                    }
                    break;

                case "stmt":
                case "stmtLst":
                case "prog_line":
                    {
                        result.Add(TNodeTypeEnum.Call);
                        result.Add(TNodeTypeEnum.While);
                        result.Add(TNodeTypeEnum.If);
                        result.Add(TNodeTypeEnum.Assign);
                    }
                    break;
            }

            return result;
        }
        private bool OutOfRange(int lineNumber)
        {
            return (lineNumber < 1 || lineNumber > CfgList.LastOrDefault().lastProgramLineNumber) ? true : false;
        }
    }


}
