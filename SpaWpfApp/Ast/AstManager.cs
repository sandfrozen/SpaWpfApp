﻿using SpaWpfApp.Enums;
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
        private List<TNode> NodeList { get; set; }
        private int[] FollowsTable;
        private int[] ParentTable;
        private PkbAPI Pkb;
        TNode rootNode = null;

        public AstManager(string sourceCode, PkbAPI Pkb)
        {
            this.Pkb = Pkb;
            this.NodeList = new List<TNode>();
            this.FollowsTable = new int[Pkb.GetNumberOfLines()];
            this.ParentTable = new int[Pkb.GetNumberOfLines()];

            BuildTree(sourceCode);
            GenerateFollowsParentTables(rootNode);
        }

        #region methods to generate followsTable and ParentTable
        private void GenerateFollowsParentTables(TNode root)
        {
            if (root.type == TNodeTypeEnum.Assign || root.type == TNodeTypeEnum.Call ||
                root.type == TNodeTypeEnum.If || root.type == TNodeTypeEnum.While)
            {
                //set parent
                this.ParentTable[(int)root.programLine - 1] = FindParent(root);

                //set follow
                this.FollowsTable[(int)root.programLine - 1] = root.rightSibling != null ? (int)root.rightSibling.programLine : -1;
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

        private int FindParent(TNode root)
        {
            while (root.up != null)
            {
                root = root.up;
                if (root.type == TNodeTypeEnum.While || root.type == TNodeTypeEnum.If)
                {
                    return (int)root.programLine;
                }
            }

            return -1;
        }
        #endregion

        #region methods to build ast
        private void BuildTree(string sourceCode)
        {
            string[] sourceCodeLines = sourceCode.Split('\n');
            string[] lineWords;
            int programLineNumber = 0;
            TNode currentUpNode = null;
            TNode actualNode = null;
            int howManyStatementsEnd = 0;

            for (int i = 0; i < sourceCodeLines.Length - 1; i++)
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
                            actualNode = CreateTNode(TNodeTypeEnum.Procedure, null, Pkb.GetProcIndex(lineWords[1]));
                            if (rootNode is null)
                            {
                                rootNode = CreateTNode(TNodeTypeEnum.Program, null, Pkb.GetProcIndex(lineWords[1]));
                            }
                            CreateLink(TLinkTypeEnum.Up, actualNode, rootNode);
                            CreateFirstChildOrRightSiblingLink(rootNode, actualNode);
                            currentUpNode = actualNode;

                            //create stmtLstNode under procedure node
                            actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null);
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
        }


        private void BuildAssign(ref TNode currentUpNode, ref TNode actualNode, ref int programLineNumber, string[] lineWords)
        {
            TNode newNode = null, leftSideOfAssignNode = null;
            newNode = CreateTNode(TNodeTypeEnum.Assign, ++programLineNumber, null);
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
            leftSideOfAssignNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[0]));
            CreateLink(TLinkTypeEnum.FirstChild, actualNode, leftSideOfAssignNode);
            CreateLink(TLinkTypeEnum.Up, leftSideOfAssignNode, actualNode);


            // create subTree of entire expression
            TNode tmpUpNode = null, tmpActualNode = null, tmpRightNode = null;
            List<ExprExtraNode> ListExpr = new List<ExprExtraNode>();
            for (int i = 2; i < lineWords.Length - 1 && lineWords[i][0] != 125; i += 2)
            {
                if (lineWords[i + 1].Equals(ConvertEnumToSign(SignEnum.Times)))
                {
                    tmpRightNode = null;
                    while (i < lineWords.Length - 1 && lineWords[i + 1].Equals(ConvertEnumToSign(SignEnum.Times)))
                    {
                        tmpUpNode = CreateTNode(TNodeTypeEnum.Times, null, null);
                        //create L
                        if (tmpRightNode is null)
                        {
                            if (WordIsConstant(lineWords[i]))
                            {
                                tmpActualNode = CreateTNode(TNodeTypeEnum.Constant, null, null);
                                tmpActualNode.value = Int32.Parse(lineWords[i]);
                            }
                            else
                            {
                                tmpActualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[i]));
                            }
                            tmpRightNode = tmpActualNode;
                        }
                        CreateLink(TLinkTypeEnum.Up, tmpRightNode, tmpUpNode);
                        CreateLink(TLinkTypeEnum.FirstChild, tmpUpNode, tmpRightNode);

                        //create P
                        if (WordIsConstant(lineWords[i + 2]))
                        {
                            tmpActualNode = CreateTNode(TNodeTypeEnum.Constant, null, null);
                            tmpActualNode.value = Int32.Parse(lineWords[i + 2]);
                        }
                        else
                        {
                            tmpActualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[i + 2]));
                        }
                        CreateLink(TLinkTypeEnum.Up, tmpActualNode, tmpUpNode);
                        CreateFirstChildOrRightSiblingLink(tmpUpNode, tmpActualNode);

                        tmpRightNode = tmpUpNode;
                        i += 2;
                    }
                    ListExpr.Add(new ExprExtraNode(tmpUpNode, ConvertSignToEnum(lineWords[i + 1][0])));
                    i += 2;
                }

                if (i < lineWords.Length - 1)
                {
                    if (WordIsConstant(lineWords[i]))
                    {
                        tmpActualNode = CreateTNode(TNodeTypeEnum.Constant, null, null);
                        tmpActualNode.value = Int32.Parse(lineWords[i]);
                    }
                    else
                    {
                        tmpActualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[i]));
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
                        tmpActualNode = CreateTNode((TNodeTypeEnum)Enum.Parse(typeof(TNodeTypeEnum), Enum.GetName(typeof(SignEnum), ListExpr[i].sign)), null, null);
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
            TNode newNode = CreateTNode(TNodeTypeEnum.Call, ++programLineNumber, Pkb.GetProcIndex(lineWords[1]));

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
            ifNodeMain = CreateTNode(TNodeTypeEnum.If, ++programLineNumber, null);

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

            variableIfNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[1]));
            CreateLink(TLinkTypeEnum.Up, variableIfNode, ifNodeMain);
            CreateLink(TLinkTypeEnum.FirstChild, ifNodeMain, variableIfNode);

            actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null);
            CreateLink(TLinkTypeEnum.Up, actualNode, ifNodeMain);
            CreateLink(TLinkTypeEnum.RightSibling, variableIfNode, actualNode);

            currentUpNode = ifNodeMain;
            #endregion

            for (++i; i < sourceCodeLines.Length - 1; i++)
            {
                lineWords = sourceCodeLines[i].Split(' ');

                switch (lineWords[0])
                {
                    case "else":
                        {
                            actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null);
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

            for (++i; i < sourceCodeLines.Length - 1; i++)
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
        private void BuildWhile(string[] sourceCodeLines, ref int i, ref TNode currentUpNode, ref TNode actualNode, ref int programLineNumber, ref int howManyStatementsEnd)
        {
            string[] lineWords = sourceCodeLines[i].Split(' ');
            TNode whileNodeMain, variableWhileNode;

            #region create node for while and remember it
            //zapamietaj
            whileNodeMain = CreateTNode(TNodeTypeEnum.While, ++programLineNumber, null);

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

            variableWhileNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[1]));
            CreateLink(TLinkTypeEnum.FirstChild, whileNodeMain, variableWhileNode);
            CreateLink(TLinkTypeEnum.Up, variableWhileNode, whileNodeMain);

            actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null);
            CreateLink(TLinkTypeEnum.RightSibling, variableWhileNode, actualNode);
            CreateLink(TLinkTypeEnum.Up, actualNode, whileNodeMain);

            currentUpNode = whileNodeMain;
            #endregion

            for (++i; i < sourceCodeLines.Length - 1; i++)
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



        private TNode CreateTNode(TNodeTypeEnum p_type, int? p_programLine, int? p_indexOfName)
        {
            TNode tmp = new TNode(p_type, p_programLine, p_indexOfName);
            NodeList.Add(tmp);
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


        #region API methods
        public int GetParent(int p_programLineNumber)
        {
            return ParentTable[p_programLineNumber - 1];
        }

        public List<int> GetParentS(int p_programLineNumber)
        {
            List<int> parents = new List<int>();
            int parent = ParentTable[p_programLineNumber - 1];
            while(parent != -1)
            {
                parents.Add(parent);
                parent = ParentTable[parent - 1];
            }
            
            if(parents.Count() == 0)
            {
                parents.Add(-1);
            }

            return parents;
        }

        public List<int> GetChildren(int p_programLineNumber)
        {
            List<int> children = new List<int>();

            for(int i=0; i<ParentTable.Length; i++)
            {
                if(ParentTable[i] == p_programLineNumber)
                {
                    children.Add(i + 1);
                }
            }

            if(children.Count()==0)
            {
                children.Add(-1);
            }

            return children;
        }

        public List<int> GetChildrenS(int p_programLineNumber)
        {
            List<int> children = new List<int>();

            FindAllChildrenS(ref children, p_programLineNumber);

            if(children.Count() == 0)
            {
                children.Add(-1);
            }
            return children;
        }

       

        public int GetRightSibling(int p_programLineNumber)
        {
            return FollowsTable[p_programLineNumber - 1];
        }

        public int GetLeftSibling(int p_programLineNumber)
        {
            for (int i = 0; i < FollowsTable.Length; i++)
            {
                if (FollowsTable[i] == p_programLineNumber)
                {
                    return i + 1;
                }
            }

            return -1;
        }

        public List<int> GetRightSiblingS(int p_programLineNumber)
        {
            List<int> rightSiblingS = new List<int>();
            int tmp = FollowsTable[p_programLineNumber - 1];

            while(tmp != -1)
            {
                rightSiblingS.Add(tmp);
                tmp = FollowsTable[tmp - 1];
            }
            
            if(rightSiblingS.Count() == 0)
            {
                rightSiblingS.Add(-1);
            }

            return rightSiblingS;

        }

        public List<int> GetLeftSiblingS(int p_programLineNumber)
        {
            List<int> leftSiblingS = new List<int>();

            do
            {
                for (int i = 0; i < FollowsTable.Length; i++)
                {
                    if (FollowsTable[i] == p_programLineNumber)
                    {
                        leftSiblingS.Add(i + 1);
                        p_programLineNumber = i + 1;
                        break;
                    }
                }
            }
            while (p_programLineNumber != -1);

            return leftSiblingS;
        }
        #endregion


        #region helpful methods for API
        public void FindAllChildrenS(ref List<int> children, TNode parent)
        {
            TNode tmp;
            List<TNode> tmpChildren;
            if (parent.firstChild is null)
            {
                return;
            }


            if (parent.type == TNodeTypeEnum.While || parent.type == TNodeTypeEnum.If || parent.type == TNodeTypeEnum.Assign || parent.type == TNodeTypeEnum.Call)
            {
                children.Add((int)parent.programLine);
            }

            tmpChildren = new List<TNode>();
            tmp = parent.firstChild;
            tmpChildren.Add(tmp);
            while (tmp.rightSibling != null)
            {
                tmp = tmp.rightSibling;
                tmpChildren.Add(tmp);
            }
            foreach (var c in tmpChildren)
            {
                FindAllChildrenS(ref children, c);
            }
        }
        private void FindAllChildrenS(ref List<int> children, int p_programLineNumber)
        {
            for (int i = 0; i < ParentTable.Length; i++)
            {
                if (ParentTable[i] == p_programLineNumber)
                {
                    if (!children.Contains(i + 1))
                    {
                        children.Add(i + 1);
                    }
                    FindAllChildrenS(ref children, i + 1);
                }
            }
        }
        #endregion
    }
}