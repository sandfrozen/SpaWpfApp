using SpaWpfApp.Cfg;
using SpaWpfApp.Enums;
using SpaWpfApp.PkbNew;
using SpaWpfApp.QueryProcessingSusbsytem;
//using SpaWpfApp.PkbOld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Ast
{
    public struct ExprExtraNode
    {
        public TNode TNode { get; set; }
        public SignEnum sign { get; set; }

        public ExprExtraNode(TNode p_TNode, SignEnum p_sign)
        {
            this.TNode = p_TNode;
            this.sign = p_sign;
        }
    }

    public class AstManager : AstAPI
    {
        public List<TNode> NodeList { get; set; }
        private List<TNode> NodeWithLineNumberList;
        private List<TNode> WhileList, IfList, AssignList, CallList, ProcedureList, ConstantList, VariableList, StmtLstList;
        private TNode[] FollowsTable;
        private TNode[] ParentTable;
        private PkbAPI Pkb;
        TNode rootNode;
        public int lastProgramLineNumber { get; set; }
        public Dictionary<string, string> declarationsList { get; set; }

        private static AstManager instance;
        public static AstManager GetInstance()
        {
            if (instance == null)
            {
                instance = new AstManager();
            }
            return instance;
        }

        public AstManager() { }

        public void GenerateStructures(string sourceCode, PkbAPI Pkb)
        {
            this.Pkb = Pkb;
            this.NodeList = new List<TNode>();
            this.NodeWithLineNumberList = new List<TNode>();
            this.WhileList = new List<TNode>();
            this.IfList = new List<TNode>();
            this.AssignList = new List<TNode>();
            this.CallList = new List<TNode>();
            this.ProcedureList = new List<TNode>();
            this.ConstantList = new List<TNode>();
            this.VariableList = new List<TNode>();
            this.StmtLstList = new List<TNode>();
            for (int i = 0; i < Pkb.GetNumberOfVars(); i++)
            {
                VariableList.Add(new TNode(Enums.TNodeTypeEnum.Variable, null, i, null));
            }
            this.FollowsTable = new TNode[Pkb.GetNumberOfLines()];
            this.ParentTable = new TNode[Pkb.GetNumberOfLines()];
            this.rootNode = null;

            BuildTree(sourceCode);
            GenerateFollowsParentTables(rootNode);
        }




        #region methods to build ast
        private void BuildTree(string sourceCode)
        {
            string[] sourceCodeLines = sourceCode.Split('\n');
            string[] lineWords;
            int programLineNumber = 0;
            TNode currentUpNode = null;
            TNode actualNode = null;
            int howManyStatementsEnd = 0;

            for (int i = 0; i < sourceCodeLines.Length; i++)
            {
                if (sourceCodeLines[i] == "") { break; }
                lineWords = sourceCodeLines[i].Split(' ');
                switch (lineWords[0])
                {
                    case "if":
                        {
                            BuildIf(sourceCodeLines, ref i, ref currentUpNode, ref actualNode, ref programLineNumber, ref howManyStatementsEnd);
                        }
                        break;

                    case "while":
                        {
                            BuildWhile(sourceCodeLines, ref i, ref currentUpNode, ref actualNode, ref programLineNumber, ref howManyStatementsEnd);
                        }
                        break;

                    case "call":
                        {
                            BuildCall(ref currentUpNode, ref actualNode, ref programLineNumber, lineWords);
                        }
                        break;

                    case "procedure":
                        {
                            actualNode = CreateTNode(TNodeTypeEnum.Procedure, null, Pkb.GetProcIndex(lineWords[1]), null, programLineNumber);
                            if (rootNode is null)
                            {
                                rootNode = CreateTNode(TNodeTypeEnum.Program, null, Pkb.GetProcIndex(lineWords[1]), null, programLineNumber);
                            }
                            CreateLink(TLinkTypeEnum.Up, actualNode, rootNode);
                            CreateFirstChildOrRightSiblingLink(rootNode, actualNode);
                            currentUpNode = actualNode;

                            //create stmtLstNode under procedure node
                            actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null, null, programLineNumber);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                        }
                        break;

                    default:
                        {
                            BuildAssign(ref currentUpNode, ref actualNode, ref programLineNumber, lineWords);
                        }
                        break;
                }
            }

            this.lastProgramLineNumber = programLineNumber;
        }

        internal List<TNode> GetAllWhileIfAssignsUnder(TNode UpNodeX, string candidatesType)
        {
            List<TNode> resultList = new List<TNode>();
            List<TNode> tmpAssigns = new List<TNode>();
            List<TNode> tmpStmt = new List<TNode>();

            if (candidatesType.ToLower() == Entity.prog_line) { candidatesType = Enum.GetName(typeof(TNodeTypeEnum), UpNodeX.type); }
            switch (candidatesType.ToLower())
            {
                case Entity.assign:
                    resultList.Add(UpNodeX);
                    return resultList;
                case Entity._if:
                case Entity._while:
                case Entity.stmt:
                    {
                        resultList.Add(UpNodeX);

                        tmpAssigns = GetChildrenXOfType(UpNodeX, Entity.assign);
                        if (tmpAssigns != null)
                        {
                            foreach (var ta in tmpAssigns)
                            {
                                if (!resultList.Contains(ta)) { resultList.Add(ta); }
                            }
                        }

                        tmpStmt = GetChildrenXOfType(UpNodeX, Entity.stmt);
                        if (tmpStmt != null)
                        {
                            foreach (var ts in tmpStmt)
                            {
                                if (!resultList.Contains(ts)) { resultList.Add(ts); }
                            }
                        }

                        var tmpCalls = GetChildrenXOfType(UpNodeX, Entity.call);
                        if (tmpCalls != null)
                        {
                            var allWhileIfAssigns = GetAllWhileIfAsigns();
                            foreach (var tc in tmpCalls)
                            {
                                AddWhileIfAssignUnderCall(ref resultList, Pkb.GetProcName((int)tc.indexOfName), allWhileIfAssigns);
                            }
                        }


                        return resultList;
                    }
                    break;

                case Entity.procedure:
                    {
                        var allWhileIfAssigns = GetAllWhileIfAsigns();
                        var cfgProcedure = CfgManager.GetInstance().CfgList.Where(x => x.IndexOfProcedureName == UpNodeX.indexOfName).First();
                        int lineNumberFirst = cfgProcedure.GNodeList.First().programLineList[0];
                        int lineNumberLast = cfgProcedure.lastProgramLineNumber;

                        foreach (var awi in allWhileIfAssigns)
                        {
                            if (awi.programLine >= lineNumberFirst && awi.programLine <= lineNumberLast && !resultList.Contains(awi))
                            {
                                resultList.Add(awi);
                            }

                        }

                        return resultList;
                    }
                    break;

                case Entity.call:
                    {
                        var allWhileIfAssigns = GetAllWhileIfAsigns();
                        AddWhileIfAssignUnderCall(ref resultList, Pkb.GetProcName((int)UpNodeX.indexOfName), allWhileIfAssigns);

                        return resultList;
                    }
                    break;
            }

            return null;
        }

        private void AddWhileIfAssignUnderCall(ref List<TNode> resultList, string procName, List<TNode> allWhileIfAssigns)
        {
            var cfgProcedure = CfgManager.GetInstance().CfgList.Where(x => x.IndexOfProcedureName == Pkb.GetProcIndex(procName)).First();
            int lineNumberFirst = cfgProcedure.GNodeList.First().programLineList[0];
            int lineNumberLast = cfgProcedure.lastProgramLineNumber;

            foreach (var wia in allWhileIfAssigns)
            {
                if (wia.programLine >= lineNumberFirst && wia.programLine <= lineNumberLast && !resultList.Contains(wia))
                {
                    resultList.Add(wia);
                }
            }
            var called = Pkb.GetCalled(procName);
            if (called != null && called.Any())
            {
                foreach (var p in called)
                {
                    AddWhileIfAssignUnderCall(ref resultList, p, allWhileIfAssigns);
                }
            }
        }

        internal List<TNode> GetAllWhileIfAsigns()
        {
            var result = DeepCopy(GetAllAssigns());
            result.AddRange(GetAllParents());
            return result;
        }

        private List<TNode> DeepCopy(List<TNode> listA)
        {
            List<TNode> copy = new List<TNode>();
            foreach (var assign in listA)
            {
                copy.Add(assign);
            }

            return copy;
        }


        private void BuildAssign(ref TNode currentUpNode, ref TNode actualNode, ref int programLineNumber, string[] lineWords)
        {
            TNode newNode = null, leftSideOfAssignNode = null;
            newNode = CreateTNode(TNodeTypeEnum.Assign, ++programLineNumber, null, null, programLineNumber);
            string left = string.Join("", lineWords);
            newNode.info = left.Substring(2, left.IndexOf(';') - 2).Trim();
            if (actualNode.type == TNodeTypeEnum.StmtLst) // jesli pierwsza instrukcja w stmtLst
            {
                currentUpNode = actualNode;
                CreateLink(TLinkTypeEnum.FirstChild, currentUpNode, newNode);
            }
            else
            {
                CreateLink(TLinkTypeEnum.RightSibling, actualNode, newNode);
            }
            CreateLink(TLinkTypeEnum.Up, newNode, currentUpNode);
            actualNode = newNode;

            //node forVariable
            leftSideOfAssignNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[0]), null, programLineNumber);
            CreateLink(TLinkTypeEnum.FirstChild, actualNode, leftSideOfAssignNode);
            CreateLink(TLinkTypeEnum.Up, leftSideOfAssignNode, actualNode);


            // create subTree of entire expression
            TNode tmpUpNode = null, tmpActualNode = null, tmpRightNode = null;
            List<ExprExtraNode> ListExpr = new List<ExprExtraNode>();
            for (int i = 2; i < lineWords.Length && lineWords[i][0] != 125; i += 2)
            {
                if (lineWords[i + 1].Equals(ConvertEnumToSign(SignEnum.Times)))
                {
                    tmpRightNode = null;
                    while (i < lineWords.Length && lineWords[i + 1].Equals(ConvertEnumToSign(SignEnum.Times)))
                    {
                        tmpUpNode = CreateTNode(TNodeTypeEnum.Times, null, null, null, programLineNumber);
                        //create L
                        if (tmpRightNode is null)
                        {
                            if (WordIsConstant(lineWords[i]))
                            {
                                tmpActualNode = CreateTNode(TNodeTypeEnum.Constant, null, null, Int32.Parse(lineWords[i]), programLineNumber);
                            }
                            else
                            {
                                tmpActualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[i]), null, programLineNumber);
                            }
                            tmpRightNode = tmpActualNode;
                        }
                        CreateLink(TLinkTypeEnum.Up, tmpRightNode, tmpUpNode);
                        CreateLink(TLinkTypeEnum.FirstChild, tmpUpNode, tmpRightNode);

                        //create P
                        if (WordIsConstant(lineWords[i + 2]))
                        {
                            tmpActualNode = CreateTNode(TNodeTypeEnum.Constant, null, null, Int32.Parse(lineWords[i + 2]), programLineNumber);
                        }
                        else
                        {
                            tmpActualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[i + 2]), null, programLineNumber);
                        }
                        CreateLink(TLinkTypeEnum.Up, tmpActualNode, tmpUpNode);
                        CreateFirstChildOrRightSiblingLink(tmpUpNode, tmpActualNode);

                        tmpRightNode = tmpUpNode;
                        i += 2;
                    }
                    ListExpr.Add(new ExprExtraNode(tmpUpNode, ConvertSignToEnum(lineWords[i + 1][0])));
                    i += 2;
                }

                if (i < lineWords.Length)
                {
                    if (WordIsConstant(lineWords[i]))
                    {
                        tmpActualNode = CreateTNode(TNodeTypeEnum.Constant, null, null, Int32.Parse(lineWords[i]), programLineNumber);
                    }
                    else
                    {
                        tmpActualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[i]), null, programLineNumber);
                    }
                    ListExpr.Add(new ExprExtraNode(tmpActualNode, ConvertSignToEnum(lineWords[i + 1][0])));
                }
            }

            tmpRightNode = ListExpr[0].TNode;
            if (ListExpr.Count() > 1)
            {
                for (int i = 0; i < ListExpr.Count(); i++)
                {
                    if (ListExpr[i].sign != SignEnum.Semicolon)
                    {
                        tmpActualNode = CreateTNode((TNodeTypeEnum)Enum.Parse(typeof(TNodeTypeEnum), Enum.GetName(typeof(SignEnum), ListExpr[i].sign)), null, null, null, programLineNumber);
                        CreateLink(TLinkTypeEnum.Up, tmpRightNode, tmpActualNode);
                        CreateLink(TLinkTypeEnum.Up, ListExpr[i + 1].TNode, tmpActualNode);
                        CreateLink(TLinkTypeEnum.FirstChild, tmpActualNode, tmpRightNode);
                        CreateLink(TLinkTypeEnum.RightSibling, tmpRightNode, ListExpr[i + 1].TNode);

                        tmpRightNode = tmpActualNode;
                    }
                }

                ListExpr.Clear();
            }

            CreateLink(TLinkTypeEnum.Up, tmpRightNode, actualNode);
            CreateLink(TLinkTypeEnum.RightSibling, leftSideOfAssignNode, tmpRightNode);
        }
        private void BuildCall(ref TNode currentUpNode, ref TNode actualNode, ref int programLineNumber, string[] lineWords)
        {
            TNode newNode = CreateTNode(TNodeTypeEnum.Call, ++programLineNumber, Pkb.GetProcIndex(lineWords[1]), null, programLineNumber);

            if (actualNode.type == TNodeTypeEnum.StmtLst)
            {
                currentUpNode = actualNode;
                CreateLink(TLinkTypeEnum.FirstChild, currentUpNode, newNode);
            }
            else
            {
                CreateLink(TLinkTypeEnum.RightSibling, actualNode, newNode);
            }
            CreateLink(TLinkTypeEnum.Up, newNode, currentUpNode);

            actualNode = newNode;
        }
        private void BuildIf(string[] sourceCodeLines, ref int i, ref TNode currentUpNode, ref TNode actualNode, ref int programLineNumber, ref int howManyStatementsEnd)
        {
            string[] lineWords = sourceCodeLines[i].Split(' ');
            TNode ifNodeMain, variableIfNode;

            #region create node for if, variableIf, stmtLst and remember node if
            //zapamietaj
            ifNodeMain = CreateTNode(TNodeTypeEnum.If, ++programLineNumber, null, null, programLineNumber);

            if (actualNode.type == TNodeTypeEnum.StmtLst)
            {
                currentUpNode = actualNode;
                CreateLink(TLinkTypeEnum.FirstChild, currentUpNode, ifNodeMain);
            }
            else
            {
                CreateLink(TLinkTypeEnum.RightSibling, actualNode, ifNodeMain);
            }
            CreateLink(TLinkTypeEnum.Up, ifNodeMain, currentUpNode);

            variableIfNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[1]), null, programLineNumber);
            CreateLink(TLinkTypeEnum.Up, variableIfNode, ifNodeMain);
            CreateLink(TLinkTypeEnum.FirstChild, ifNodeMain, variableIfNode);

            actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null, null, programLineNumber);
            CreateLink(TLinkTypeEnum.Up, actualNode, ifNodeMain);
            CreateLink(TLinkTypeEnum.RightSibling, variableIfNode, actualNode);

            currentUpNode = ifNodeMain;
            #endregion

            for (++i; i < sourceCodeLines.Length; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "else":
                        {
                            actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null, null, programLineNumber);
                            CreateLink(TLinkTypeEnum.Up, actualNode, ifNodeMain);
                            CreateLink(TLinkTypeEnum.RightSibling, variableIfNode.rightSibling, actualNode);
                            currentUpNode = actualNode.up;

                            BuildElse(sourceCodeLines, ref i, ref currentUpNode, ref actualNode, ref programLineNumber, ref howManyStatementsEnd);

                            actualNode = ifNodeMain;
                            currentUpNode = actualNode.up;

                            return;
                        }
                        break;

                    case "while":
                        {
                            BuildWhile(sourceCodeLines, ref i, ref currentUpNode, ref actualNode, ref programLineNumber, ref howManyStatementsEnd);
                        }
                        break;

                    case "if":
                        {
                            BuildIf(sourceCodeLines, ref i, ref currentUpNode, ref actualNode, ref programLineNumber, ref howManyStatementsEnd);
                        }
                        break;

                    case "call":
                        {
                            BuildCall(ref currentUpNode, ref actualNode, ref programLineNumber, lineWords);

                            if (EndOfStatement(lineWords, ref howManyStatementsEnd)) // end of then section
                            {
                                actualNode = ifNodeMain;
                                currentUpNode = actualNode.up;
                                --howManyStatementsEnd;
                            }
                        }
                        break;

                    default:
                        {
                            BuildAssign(ref currentUpNode, ref actualNode, ref programLineNumber, lineWords);

                            if (EndOfStatement(lineWords, ref howManyStatementsEnd)) // end of then section
                            {
                                actualNode = ifNodeMain;
                                currentUpNode = actualNode.up;
                                --howManyStatementsEnd;
                            }
                        }
                        break;
                }
            }
        }
        private void BuildElse(string[] sourceCodeLines, ref int i, ref TNode currentUpNode, ref TNode actualNode, ref int programLineNumber, ref int howManyStatementsEnd)
        {
            string[] lineWords;

            for (++i; i < sourceCodeLines.Length; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "while":
                        {
                            BuildWhile(sourceCodeLines, ref i, ref currentUpNode, ref actualNode, ref programLineNumber, ref howManyStatementsEnd);
                            if (howManyStatementsEnd > 0)
                            {
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;

                    case "if":
                        {
                            BuildIf(sourceCodeLines, ref i, ref currentUpNode, ref actualNode, ref programLineNumber, ref howManyStatementsEnd);

                            if (howManyStatementsEnd > 0)
                            {
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;

                    case "call":
                        {
                            BuildCall(ref currentUpNode, ref actualNode, ref programLineNumber, lineWords);

                            if (EndOfStatement(lineWords, ref howManyStatementsEnd))
                            {
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;
                    default:
                        {
                            BuildAssign(ref currentUpNode, ref actualNode, ref programLineNumber, lineWords);

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

        internal List<TNode> GetAllAssignUnder(TNode UpNodeX, string candidatesType)
        {
            List<TNode> resultList = new List<TNode>();
            List<TNode> tmpAssigns = new List<TNode>();

            

            if (candidatesType.ToLower() == Entity.prog_line) { candidatesType = Enum.GetName(typeof(TNodeTypeEnum), UpNodeX.type); }
            switch (candidatesType.ToLower())
            {
                case Entity.assign:
                    resultList.Add(UpNodeX);
                    return resultList;
                case Entity._if:
                case Entity._while:
                case Entity.stmt:
                    {
                        tmpAssigns = GetChildrenXOfType(UpNodeX, Entity.assign);
                        if (tmpAssigns != null)
                        {
                            foreach (var ta in tmpAssigns)
                            {
                                if (!resultList.Contains(ta)) { resultList.Add(ta); }
                            }
                        }

                        var tmpCalls = GetChildrenXOfType(UpNodeX, Entity.call);
                        if (tmpCalls != null)
                        {
                            var allAssigns = GetAllAssigns();
                            foreach (var tc in tmpCalls)
                            {
                                AddAssignUnderCall(ref resultList, Pkb.GetProcName((int)tc.indexOfName), allAssigns);
                            }
                        }


                        return resultList;
                    }
                    break;

                case Entity.procedure:
                    {
                        var allAssigns = GetAllAssigns();
                        var cfgProcedure = CfgManager.GetInstance().CfgList.Where(x => x.IndexOfProcedureName == UpNodeX.indexOfName).First();
                        int lineNumberFirst = cfgProcedure.GNodeList.First().programLineList[0];
                        int lineNumberLast = cfgProcedure.lastProgramLineNumber;

                        foreach (var a in allAssigns)
                        {
                            if (a.programLine >= lineNumberFirst && a.programLine <= lineNumberLast && !resultList.Contains(a))
                            {
                                resultList.Add(a);
                            }

                        }

                        return resultList;
                    }
                    break;

                case Entity.call:
                    {
                        var allAssigns = GetAllAssigns();
                        AddAssignUnderCall(ref resultList, Pkb.GetProcName((int)UpNodeX.indexOfName), allAssigns);

                        return resultList;
                    }
                    break;
            }

            return null;
        }

        private List<TNode> GetChildrenXOfType(TNode iw, string type)
        {
            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypesFromType(type);
            List<TNode> children = new List<TNode>();

            if (iw.programLine is null || (iw.type != TNodeTypeEnum.If && iw.type != TNodeTypeEnum.While))
            {
                return null;
            }

            FindAllChildrenS(ref children, iw, acceptableType);

            return children.Count() > 0 ? children : null;
        }

        private List<TNodeTypeEnum> DetermineAcceptableTypesFromType(string type)
        {
            List<TNodeTypeEnum> result = new List<TNodeTypeEnum>();

            switch (type.ToLower())
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

        private void AddAssignUnderCall(ref List<TNode> resultList, string procName, List<TNode> allAssigns)
        {
            var cfgProcedure = CfgManager.GetInstance().CfgList.Where(x => x.IndexOfProcedureName == Pkb.GetProcIndex(procName)).First();
            int lineNumberFirst = cfgProcedure.GNodeList.First().programLineList[0];
            int lineNumberLast = cfgProcedure.lastProgramLineNumber;

            foreach (var a in allAssigns)
            {
                if (a.programLine >= lineNumberFirst && a.programLine <= lineNumberLast && !resultList.Contains(a))
                {
                    resultList.Add(a);
                }
            }
            var called = Pkb.GetCalled(procName);
            if (called != null && called.Any())
            {
                foreach (var p in called)
                {
                    AddAssignUnderCall(ref resultList, p, allAssigns);
                }
            }
        }

        private void BuildWhile(string[] sourceCodeLines, ref int i, ref TNode currentUpNode, ref TNode actualNode, ref int programLineNumber, ref int howManyStatementsEnd)
        {
            string[] lineWords = sourceCodeLines[i].Split(' ');
            TNode whileNodeMain, variableWhileNode;

            #region create node for while and remember it
            //zapamietaj
            whileNodeMain = CreateTNode(TNodeTypeEnum.While, ++programLineNumber, null, null, programLineNumber);

            if (actualNode.type == TNodeTypeEnum.StmtLst)
            {
                currentUpNode = actualNode;
                CreateLink(TLinkTypeEnum.FirstChild, currentUpNode, whileNodeMain);
            }
            else
            {
                CreateLink(TLinkTypeEnum.RightSibling, actualNode, whileNodeMain);
            }
            CreateLink(TLinkTypeEnum.Up, whileNodeMain, currentUpNode);

            variableWhileNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[1]), null, programLineNumber);
            CreateLink(TLinkTypeEnum.FirstChild, whileNodeMain, variableWhileNode);
            CreateLink(TLinkTypeEnum.Up, variableWhileNode, whileNodeMain);

            actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null, null, programLineNumber);
            CreateLink(TLinkTypeEnum.RightSibling, variableWhileNode, actualNode);
            CreateLink(TLinkTypeEnum.Up, actualNode, whileNodeMain);

            currentUpNode = whileNodeMain;
            #endregion

            for (++i; i < sourceCodeLines.Length; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "while":
                        {
                            BuildWhile(sourceCodeLines, ref i, ref currentUpNode, ref actualNode, ref programLineNumber, ref howManyStatementsEnd);
                            if (howManyStatementsEnd > 0)
                            {
                                actualNode = whileNodeMain;
                                currentUpNode = actualNode.up;
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;

                    case "if":
                        {
                            BuildIf(sourceCodeLines, ref i, ref currentUpNode, ref actualNode, ref programLineNumber, ref howManyStatementsEnd);
                            if (howManyStatementsEnd > 0)
                            {
                                actualNode = whileNodeMain;
                                currentUpNode = actualNode.up;
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;

                    case "call":
                        {
                            BuildCall(ref currentUpNode, ref actualNode, ref programLineNumber, lineWords);
                            if (EndOfStatement(lineWords, ref howManyStatementsEnd))
                            {
                                actualNode = whileNodeMain;
                                currentUpNode = actualNode.up;
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;

                    default:
                        {
                            BuildAssign(ref currentUpNode, ref actualNode, ref programLineNumber, lineWords);

                            if (EndOfStatement(lineWords, ref howManyStatementsEnd))
                            {
                                actualNode = whileNodeMain;
                                currentUpNode = actualNode.up;
                                --howManyStatementsEnd;
                                return;
                            }
                        }
                        break;
                }
            }
        }



        private TNode CreateTNode(TNodeTypeEnum p_type, int? p_programLine, int? p_indexOfName, int? p_value, int actualProgramLineNumber)
        {
            TNode tmp = new TNode(p_type, p_programLine, p_indexOfName, p_value);
            NodeList.Add(tmp);

            switch (p_type)
            {
                case TNodeTypeEnum.Assign:
                    AssignList.Add(tmp);
                    NodeWithLineNumberList.Add(tmp);
                    break;
                case TNodeTypeEnum.Call:
                    CallList.Add(tmp);
                    NodeWithLineNumberList.Add(tmp);
                    break;
                case TNodeTypeEnum.While:
                    WhileList.Add(tmp);
                    NodeWithLineNumberList.Add(tmp);
                    break;
                case TNodeTypeEnum.If:
                    IfList.Add(tmp);
                    NodeWithLineNumberList.Add(tmp);
                    break;
                case TNodeTypeEnum.Procedure:
                    ProcedureList.Add(tmp);
                    break;

                case TNodeTypeEnum.Constant:
                    var dfv = ConstantList.Where(x => x.value == tmp.value).ToList();
                    if (!ConstantList.Where(x => x.value == tmp.value).Any()) { ConstantList.Add(tmp); }
                    break;

                case TNodeTypeEnum.StmtLst:
                    this.StmtLstList.Add(new TNode(TNodeTypeEnum.StmtLst, actualProgramLineNumber + 1, null, null));
                    break;
            }

            return tmp;
        }
        private void CreateLink(TLinkTypeEnum p_linkType, TNode p_nodeFrom, TNode p_nodeTo)
        {
            switch (p_linkType)
            {
                case TLinkTypeEnum.Up:
                    p_nodeFrom.up = p_nodeTo;
                    break;

                case TLinkTypeEnum.FirstChild:
                    p_nodeFrom.firstChild = p_nodeTo;
                    break;

                case TLinkTypeEnum.RightSibling:
                    p_nodeFrom.rightSibling = p_nodeTo;
                    break;
            }
        }
        private void CreateFirstChildOrRightSiblingLink(TNode currentUpNode, TNode actualNode)
        {
            if (currentUpNode.firstChild != null)
            {
                CreateLink(TLinkTypeEnum.RightSibling, FindLastChild(currentUpNode), actualNode);
            }
            else
            {
                CreateLink(TLinkTypeEnum.FirstChild, currentUpNode, actualNode);
            }
        }
        private bool EndOfStatement(string[] lineWords, ref int howManyStatementsEnd)
        {
            howManyStatementsEnd = DetermineHowManyStatementsEnd(lineWords);

            return howManyStatementsEnd > 0 ? true : false;
        }
        private TNode FindLastChild(TNode root)
        {
            TNode tmp = root.firstChild;
            while (tmp.rightSibling != null)
            { tmp = tmp.rightSibling; }

            return tmp;
        }
        private int DetermineHowManyStatementsEnd(string[] lineWords)
        {
            int i = lineWords.Length - 1;
            int closeBracketCounter = 0;

            while (lineWords[i--][0] == 125) // '}'
            { closeBracketCounter++; }

            return closeBracketCounter;
        }
        private SignEnum ConvertSignToEnum(char v)
        {
            switch (v)
            {
                case '+': return SignEnum.Plus;
                case '-': return SignEnum.Minus;
                case '*': return SignEnum.Times;
                default: return SignEnum.Semicolon;
            }
        }
        private string ConvertEnumToSign(SignEnum v)
        {
            switch (v)
            {
                case SignEnum.Plus: return "+";
                case SignEnum.Minus: return "-";
                case SignEnum.Times: return "*";
                default: return ";";
            }
        }
        private bool WordIsConstant(string v)
        {
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] < 47 || v[i] > 58) { return false; }
            }

            return true;
        }
        #endregion

        #region methods to generate followsTable and ParentTable
        private void GenerateFollowsParentTables(TNode root)
        {
            if (root.type == TNodeTypeEnum.Assign || root.type == TNodeTypeEnum.Call ||
                root.type == TNodeTypeEnum.If || root.type == TNodeTypeEnum.While)
            {
                //set parent
                this.ParentTable[(int)root.programLine - 1] = FindParent(root);

                //set follow
                this.FollowsTable[(int)root.programLine - 1] = root.rightSibling != null ? root.rightSibling : null;
            }


            if (root.firstChild == null || root.type == TNodeTypeEnum.Assign)
            {
                return;
            }

            foreach (var v in GetChilds(root))
            {
                GenerateFollowsParentTables(v);
            }
        }

        private List<TNode> GetChilds(TNode root)
        {
            List<TNode> list = new List<TNode>();
            root = root.firstChild;

            do
            {
                list.Add(root);
                root = root.rightSibling;
            }
            while (root != null);

            return list;
        }

        private TNode FindParent(TNode root)
        {
            while (root.up != null)
            {
                root = root.up;
                if (root.type == TNodeTypeEnum.While || root.type == TNodeTypeEnum.If)
                {
                    return root;
                }
            }

            return null;
        }
        #endregion

        #region API methods

        public TNode GetParent(TNode p_child, string p_father)
        {
            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_father);
            TNode findedParent;

            if (p_child.programLine is null) { return null; }

            findedParent = ParentTable[(int)p_child.programLine - 1];

            if (findedParent is null) { return null; }

            if (acceptableType.Contains(findedParent.type))
            {
                return findedParent;
            }
            else
            {
                return null;
            }

        }

        public List<TNode> GetParentX(TNode p_child, string p_father)
        {
            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_father);
            List<TNode> parents = new List<TNode>();
            TNode findedParent;

            if (p_child.programLine is null)
            {
                return null;
            }
            if (((int)p_child.programLine - 1) < 0 || ((int)p_child.programLine - 1) > (ParentTable.Length - 1))
            {
                return null;
            }

            findedParent = ParentTable[(int)p_child.programLine - 1];
            while (findedParent != null)
            {
                if (acceptableType.Contains(findedParent.type))
                {
                    parents.Add(findedParent);
                }
                findedParent = ParentTable[(int)findedParent.programLine - 1];
            }

            return parents.Count() > 0 ? parents : null;
        }

        public List<TNode> GetChildren(TNode p_father, string p_child)
        {
            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_child);
            List<TNode> children = new List<TNode>();

            if (p_father.programLine is null || (p_father.type != TNodeTypeEnum.If && p_father.type != TNodeTypeEnum.While))
            {
                return null;
            }

            for (int i = 0; i < ParentTable.Length; i++)
            {
                if (ParentTable[i] == p_father && acceptableType.Contains(FindNode(i + 1).type))
                {
                    children.Add(FindNode(i + 1));
                }
            }

            return children.Count() > 0 ? children : null;
        }

        public List<TNode> GetChildrenX(TNode p_father, string p_child)
        {
            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_child);
            List<TNode> children = new List<TNode>();

            if (p_father.programLine is null || (p_father.type != TNodeTypeEnum.If && p_father.type != TNodeTypeEnum.While))
            {
                return null;
            }

            FindAllChildrenS(ref children, p_father, acceptableType);

            return children.Count() > 0 ? children : null;
        }

        public bool IsParent(int p1, int p2)
        {
            if (OutOfRange(p1) || OutOfRange(p2)) { return false; }

            return ParentTable[p2 - 1] == NodeList.Where(x => x.programLine == p1).FirstOrDefault() ? true : false;
        }

        public bool IsParentX(int p1, int p2)
        {
            if (OutOfRange(p1) || OutOfRange(p2)) { return false; }

            TNode parent;

            parent = ParentTable[p2 - 1];

            while (parent != null)
            {
                if (parent == NodeList.Where(x => x.programLine == p1).FirstOrDefault())
                {
                    return true;
                }

                parent = ParentTable[(int)parent.programLine - 1];
            }

            return false;
        }



        public TNode GetRightSibling(TNode p_from, string p_to)
        {
            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_to);
            TNode findedTo;

            if (p_from.programLine is null) { return null; }

            findedTo = FollowsTable[(int)p_from.programLine - 1];
            if (findedTo is null) { return null; }

            if (acceptableType.Contains(findedTo.type))
            {
                return findedTo;
            }
            else
            {
                return null;
            }
        }

        public TNode GetLeftSibling(TNode p_to, string p_from)
        {
            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_from);

            if (p_to.programLine is null) { return null; }

            for (int i = 0; i < FollowsTable.Length; i++)
            {
                if (FollowsTable[i] == p_to && acceptableType.Contains(FindNode(i + 1).type))
                {
                    return FindNode(i + 1);
                }
            }

            return null;
        }

        public List<TNode> GetRightSiblingX(TNode p_from, string p_to)
        {
            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_to);
            List<TNode> rightSiblingX = new List<TNode>();

            if (p_from.programLine is null) { return null; }
            TNode tmp = FollowsTable[(int)p_from.programLine - 1];

            while (tmp != null)
            {
                if (acceptableType.Contains(tmp.type)) { rightSiblingX.Add(tmp); }
                tmp = FollowsTable[(int)tmp.programLine - 1];
            }

            return rightSiblingX.Any() ? rightSiblingX : null;

        }

        public List<TNode> GetLeftSiblingX(TNode p_to, string p_from)
        {
            List<TNodeTypeEnum> acceptableType = DetermineAcceptableTypes(p_from);
            List<TNode> leftSiblingX = new List<TNode>();

            var children = GetChilds(p_to.up);

            foreach (var child in children)
            {
                if (child == p_to) { break; }
                if (acceptableType.Contains(child.type))
                {
                    leftSiblingX.Add(child);
                }
            }

            return leftSiblingX.Any() ? leftSiblingX : null;
        }

        public bool IsFollows(int p1, int p2)
        {
            if (OutOfRange(p1) || OutOfRange(p2)) { return false; }

            return FollowsTable[p1 - 1] == NodeList.Where(x => x.programLine == p2).FirstOrDefault() ? true : false;
        }

        public bool IsFollowsX(int p1, int p2)
        {
            if (OutOfRange(p1) || OutOfRange(p2)) { return false; }

            TNode toTmp, to;

            to = NodeList.Where(x => x.programLine == p2).FirstOrDefault();
            toTmp = FollowsTable[p1 - 1];

            while (toTmp != null)
            {
                if (toTmp == to)
                {
                    return true;
                }

                toTmp = FollowsTable[(int)toTmp.programLine - 1];
            }

            return false;
        }
        #endregion


        #region helpful methods for API
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

        private void FindAllChildrenS(ref List<TNode> children, TNode p_father, List<TNodeTypeEnum> acceptableType)
        {
            if(p_father is null)
            {
                return;
            }

            TNode tmp = p_father.firstChild;
            if (tmp is null)
            {
                return;
            }
            do
            {
                if (acceptableType.Contains(tmp.type) && !children.Contains(tmp))
                {
                    children.Add(tmp);
                }
                if (tmp.type != TNodeTypeEnum.Assign)
                {
                    FindAllChildrenS(ref children, tmp, acceptableType);
                }
                tmp = tmp.rightSibling;
            }
            while (tmp != null);

            //for (int i = 0; i < ParentTable.Length; i++)
            //{
            //    if (ParentTable[i] == p_father && acceptableType.Contains(FindNode(i + 1).type))
            //    {
            //        if (!children.Contains(NodeList.Where(x => x.programLine == i + 1).FirstOrDefault()))
            //        {
            //            children.Add(FindNode(i + 1));
            //        }
            //        FindAllChildrenS(ref children, NodeList.Where(x => x.programLine == i + 1).FirstOrDefault(), acceptableType);
            //    }
            //}
        }
        #endregion


        //private List<string> GetNodeTypeNames()
        //{
        //    var table = Enum.GetNames(typeof(TNodeTypeEnum));
        //    for (int i = 0; i < table.Length; i++)
        //    {
        //        table[i] = table[i].Substring(0, 1).ToLower() + table[i].Substring(1);
        //    }

        //    return table.ToList();
        //}


        public TNode FindFather(int programLineNumber)
        {
            foreach (var w in WhileList)
            {
                if (w.programLine == programLineNumber)
                {
                    return w;
                }
            }

            foreach (var i in IfList)
            {
                if (i.programLine == programLineNumber)
                {
                    return i;
                }
            }

            return null;
        }

        public TNode FindNode(int programLineNumber)
        {
            TNode result = NodeList.Where(x => x.programLine == programLineNumber).FirstOrDefault();

            return result;
        }

        internal List<TNode> GetAllParents()
        {
            List<TNode> fathers = new List<TNode>();
            foreach (var w in WhileList)
            {
                fathers.Add(w);
            }

            foreach (var i in IfList)
            {
                fathers.Add(i);
            }

            return fathers.Count() > 0 ? DeepCopy(fathers): null;
        }

        internal List<TNode> GetAllWhile()
        {
            return WhileList.Count() > 0 ? DeepCopy(WhileList): null;
        }

        internal List<TNode> GetAllIf()
        {
            return IfList.Count() > 0 ? DeepCopy(IfList): null;
        }

        internal List<TNode> GetAllAssigns()
        {
            return AssignList.Count() > 0 ? DeepCopy(AssignList): null;
        }

        internal List<TNode> GetAllNodesWithLineNumbers()
        {
            return NodeWithLineNumberList.Count() > 0 ? DeepCopy(NodeWithLineNumberList) : null;
        }

        private bool OutOfRange(int lineNumber)
        {
            return (lineNumber < 1 || lineNumber > lastProgramLineNumber) ? true : false;
        }

        public List<TNode> GetNodes(string nodesType)
        {
            RemoveDotFromNameIfItIsAttrRef(ref nodesType);
            switch (nodesType)
            {
                case Entity._if:
                    return DeepCopy(IfList);

                case Entity._while:
                    return DeepCopy(WhileList);

                case Entity.assign:
                    return DeepCopy(AssignList);

                case Entity.prog_line:
                case Entity.stmt:
                    return DeepCopy(this.NodeWithLineNumberList);

                case Entity.procedure:
                    return DeepCopy(ProcedureList);

                case Entity.constant:
                    return DeepCopy(ConstantList);

                case Entity.variable:
                    return DeepCopy(VariableList);

                case Entity.call:
                    return DeepCopy(this.CallList);

                case Entity.stmtLst:
                    return DeepCopy(this.StmtLstList);
            }

            return null;
        }

        private void RemoveDotFromNameIfItIsAttrRef(ref string s)
        {
            if (s.Contains('.')) { s = s.Substring(0, s.IndexOf('.')); }
        }
    }
}
