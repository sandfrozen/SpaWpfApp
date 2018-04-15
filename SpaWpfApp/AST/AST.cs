using SpaWpfApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.AST
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

    public class AST
    {
        public List<TNode> NodeList { get; set; }
        PkbAPI Pkb;

        public AST(string sourceCode, PkbAPI Pkb)
        {
            this.Pkb = Pkb;
            this.NodeList = new List<TNode>();
            string[] sourceCodeLines = sourceCode.Split('\n');
            string[] lineWords;
            int howManyLevelUp = 0;
            int programLineNumber = 0;
            TNode rootNode = null;
            TNode currentUpNode = null;
            TNode actualNode;
            TNode lastChild;

            foreach (var sourceCodeLine in sourceCodeLines)
            {
                lineWords = sourceCodeLine.Split(' ');

                switch (lineWords[0])
                {
                    case "procedure":
                        actualNode = CreateTNode(TNodeTypeEnum.Procedure, null, Pkb.GetProcIndex(lineWords[1]));
                        if (rootNode is null)
                        {
                            rootNode = CreateTNode(TNodeTypeEnum.Program, null, Pkb.GetProcIndex(lineWords[1]));
                        }
                        CreateLink(TLinkTypeEnum.Up, actualNode, rootNode);
                        CreateFirstChildOrRightSiblingLink(rootNode, actualNode);

                        //create stmtLstNode under currentUpNode
                        actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null);
                        CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                        SetCurrentUpNode(currentUpNode, actualNode);

                        break;

                    case "call":
                        actualNode = CreateTNode(TNodeTypeEnum.Call, ++programLineNumber, null);
                        CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                        CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);

                        //what if end of statement???
                        HandleGoingBackOnTheTree(currentUpNode, lineWords);

                        break;

                    case "while":
                        actualNode = CreateTNode(TNodeTypeEnum.While, ++programLineNumber, null);
                        CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                        CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                        SetCurrentUpNode(currentUpNode, actualNode);

                        actualNode = CreateTNode(TNodeTypeEnum.Variable, ++programLineNumber, Pkb.GetVarIndex(lineWords[1]));
                        CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                        CreateLink(TLinkTypeEnum.FirstChild, currentUpNode, actualNode);

                        //create stmtLstNode under currentUpNode
                        actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null);
                        CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                        SetCurrentUpNode(currentUpNode, actualNode);

                        CreateLink(TLinkTypeEnum.RightSibling, currentUpNode.up.firstChild, currentUpNode);

                        break;

                    case "if":
                        actualNode = CreateTNode(TNodeTypeEnum.If, ++programLineNumber, null);
                        CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                        CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                        SetCurrentUpNode(currentUpNode, actualNode);

                        actualNode = CreateTNode(TNodeTypeEnum.Variable, ++programLineNumber, Pkb.GetVarIndex(lineWords[1]));
                        CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                        CreateLink(TLinkTypeEnum.FirstChild, currentUpNode, actualNode);

                        //create stmtLstNode under currentUpNode
                        actualNode = CreateTNode(TNodeTypeEnum.StmtLstThen, null, null);
                        CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                        CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                        SetCurrentUpNode(currentUpNode, actualNode);

                        break;

                    case "else":

                        //create stmtLstNode under currentUpNode
                        actualNode = CreateTNode(TNodeTypeEnum.StmtLstElse, null, null);
                        CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                        CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                        SetCurrentUpNode(currentUpNode, actualNode);

                        break;

                    // assign
                    default:
                        {
                            actualNode = CreateTNode(TNodeTypeEnum.Assign, ++programLineNumber, null);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                            SetCurrentUpNode(currentUpNode, actualNode);

                            actualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[0]));
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);

                            ///////
                            TNode tmpUpNode = null, tmpActualNode = null, tmpLeftNode = null;
                            List<ExprExtraNode> ListExpr = new List<ExprExtraNode>();
                            for (int i = 2; i < lineWords.Length - 1; i++)
                            {
                                if (lineWords[i + 1].Equals(SignEnum.Times))
                                {
                                    tmpLeftNode = null;
                                    while (i < lineWords.Length - 2 && lineWords[i + 1].Equals(SignEnum.Times))
                                    {
                                        tmpUpNode = CreateTNode(TNodeTypeEnum.Times, null, null);
                                        //create L
                                        if (tmpLeftNode is null)
                                        {
                                            if (wordIsConstant(lineWords[i]))
                                            {
                                                tmpActualNode = CreateTNode(TNodeTypeEnum.Constant, null, null);
                                                tmpActualNode.value = Int32.Parse(lineWords[i]);
                                            }
                                            else
                                            {
                                                tmpActualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[i]));
                                            }
                                            tmpLeftNode = tmpActualNode;
                                        }
                                        CreateLink(TLinkTypeEnum.Up, tmpLeftNode, tmpUpNode);
                                        CreateLink(TLinkTypeEnum.FirstChild, tmpUpNode, tmpLeftNode);

                                        //create P
                                        if (wordIsConstant(lineWords[i + 2]))
                                        {
                                            tmpActualNode = CreateTNode(TNodeTypeEnum.Constant, null, null);
                                            tmpActualNode.value = Int32.Parse(lineWords[i + 2]);
                                        }
                                        else
                                        {
                                            tmpActualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[i + 2]));
                                        }
                                        CreateLink(TLinkTypeEnum.Up, tmpLeftNode, tmpUpNode);
                                        CreateFirstChildOrRightSiblingLink(tmpUpNode, tmpActualNode);

                                        tmpLeftNode = currentUpNode;
                                        i += 2;
                                    }
                                    ListExpr.Add(new ExprExtraNode(tmpUpNode, ConvertSignToEnum(lineWords[i + 1][0])));
                                    i += 2;
                                }

                                if (lineWords[i][0] != ';')
                                {
                                    if (wordIsConstant(lineWords[i]))
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

                            tmpLeftNode = ListExpr[0].TNode;
                            if (ListExpr.Count() > 1)
                            {
                                for (int i = 0; i < ListExpr.Count(); i++)
                                {
                                    if (ListExpr[i].sign != SignEnum.Semicolon)
                                    {
                                        tmpActualNode = CreateTNode((TNodeTypeEnum)ListExpr[i].sign, null, null);
                                        CreateLink(TLinkTypeEnum.Up, tmpLeftNode, tmpActualNode);
                                        CreateLink(TLinkTypeEnum.Up, ListExpr[i + 1].TNode, tmpActualNode);
                                        CreateLink(TLinkTypeEnum.FirstChild, tmpActualNode, tmpLeftNode);
                                        CreateLink(TLinkTypeEnum.RightSibling, tmpLeftNode, ListExpr[i + 1].TNode);

                                        tmpLeftNode = tmpActualNode;
                                    }
                                }
                            }

                            CreateLink(TLinkTypeEnum.Up, tmpLeftNode, currentUpNode);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, tmpLeftNode);

                            //what if end of statement???
                            HandleGoingBackOnTheTree(currentUpNode, lineWords);
                            break;
                        }
                }
            }
        }

        private void HandleGoingBackOnTheTree(TNode currentUpNode, string[] lineWords)
        {
            int howManyLevelUp = determineHowManyLevelUp(lineWords);
            if (howManyLevelUp > 0)
            {
                FindAndSetCurrentUpNode(howManyLevelUp, currentUpNode);
            }
        }

        private void FindAndSetCurrentUpNode(int howManyLevelUp, TNode currentUpNode)
        {
            TNode lastChild;

            for (int i = 0; i < howManyLevelUp; i++)
            {
                SetCurrentUpNode(currentUpNode, currentUpNode.up);
                switch (currentUpNode.type)
                {
                    case TNodeTypeEnum.If:
                        lastChild = FindLastChild(currentUpNode);
                        if (lastChild.type == TNodeTypeEnum.StmtLstElse)
                        {
                            SetCurrentUpNode(currentUpNode, currentUpNode.up);
                        }
                        break;

                    case TNodeTypeEnum.While:
                        break;

                    case TNodeTypeEnum.Procedure:
                        break;

                    default:
                        SetCurrentUpNode(currentUpNode, currentUpNode.up);
                        break;
                }

            }
        }

        private int determineHowManyLevelUp(string[] lineWords)
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

        private bool wordIsConstant(string v)
        {
            for (int i = 0; i < v.Length - 1; i++)
            {
                if (v[i] < 47 || v[i] > 58) { return false; }
            }

            return true;
        }

        private void SetCurrentUpNode(TNode currentUpNode, TNode newCurrentUpNode)
        {
            currentUpNode = newCurrentUpNode;
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

        private TNode FindLastChild(TNode root)
        {
            TNode tmp = root.firstChild;
            while (tmp.rightSibling != null)
            { tmp = tmp.rightSibling; }

            return tmp;
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
    }
}
