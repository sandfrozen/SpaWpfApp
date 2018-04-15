using SpaWpfApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.ASTFolder
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
        private List<TNode> NodeList { get; set; }
        private PkbAPI Pkb;
        TNode rootNode = null;

        public AST(string sourceCode, PkbAPI Pkb)
        {
            this.Pkb = Pkb;
            this.NodeList = new List<TNode>();

            BuildTree(sourceCode);
        }


        private void BuildTree(string sourceCode)
        {
            string[] sourceCodeLines = sourceCode.Split('\n');
            string[] lineWords;
            int programLineNumber = 0;            
            TNode currentUpNode = null;
            TNode actualNode;

            foreach (var sourceCodeLine in sourceCodeLines)
            {
                lineWords = sourceCodeLine.Split(' ');
                switch (lineWords[0])
                {
                    case "procedure":
                        {
                            actualNode = CreateTNode(TNodeTypeEnum.Procedure, null, Pkb.GetProcIndex(lineWords[1]));
                            if (rootNode is null)
                            {
                                rootNode = CreateTNode(TNodeTypeEnum.Program, null, Pkb.GetProcIndex(lineWords[1]));
                            }
                            CreateLink(TLinkTypeEnum.Up, actualNode, rootNode);
                            CreateFirstChildOrRightSiblingLink(rootNode, actualNode);
                            SetCurrentUpNode(ref currentUpNode, actualNode);

                            //create stmtLstNode under currentUpNode
                            actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                            SetCurrentUpNode(ref currentUpNode, actualNode);
                        }
                        break;

                    case "call":
                        {
                            actualNode = CreateTNode(TNodeTypeEnum.Call, ++programLineNumber, Pkb.GetProcIndex(lineWords[1]));
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);

                            //what if end of statement???
                            HandleGoingBackOnTheTree(ref currentUpNode, lineWords);
                        }
                        break;

                    case "while":
                        {
                            actualNode = CreateTNode(TNodeTypeEnum.While, ++programLineNumber, null);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            SetCurrentUpNode(ref currentUpNode, actualNode);

                            actualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[1]));
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            CreateLink(TLinkTypeEnum.FirstChild, currentUpNode, actualNode);

                            //create stmtLstNode under currentUpNode
                            actualNode = CreateTNode(TNodeTypeEnum.StmtLst, null, null);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            SetCurrentUpNode(ref currentUpNode, actualNode);
                        }
                        break;

                    case "if":
                        {
                            actualNode = CreateTNode(TNodeTypeEnum.If, ++programLineNumber, null);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            SetCurrentUpNode(ref currentUpNode, actualNode);

                            actualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[1]));
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            CreateLink(TLinkTypeEnum.FirstChild, currentUpNode, actualNode);

                            //create stmtLstNode under currentUpNode
                            actualNode = CreateTNode(TNodeTypeEnum.StmtLstThen, null, null);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                            SetCurrentUpNode(ref currentUpNode, actualNode);
                        }
                        break;

                    case "else":
                        {
                            //create stmtLstNode under currentUpNode
                            actualNode = CreateTNode(TNodeTypeEnum.StmtLstElse, null, null);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                            SetCurrentUpNode(ref currentUpNode, actualNode);
                        }
                        break;

                    // assign
                    default:
                        {
                            actualNode = CreateTNode(TNodeTypeEnum.Assign, ++programLineNumber, null);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                            SetCurrentUpNode(ref currentUpNode, actualNode);

                            actualNode = CreateTNode(TNodeTypeEnum.Variable, null, Pkb.GetVarIndex(lineWords[0]));
                            CreateFirstChildOrRightSiblingLink(currentUpNode, actualNode);
                            CreateLink(TLinkTypeEnum.Up, actualNode, currentUpNode);

                            // create subTree of entire expression
                            TNode tmpUpNode = null, tmpActualNode = null, tmpLeftNode = null;
                            List<ExprExtraNode> ListExpr = new List<ExprExtraNode>();
                            for (int i = 2; i < lineWords.Length - 1; i += 2)
                            {
                                if (lineWords[i + 1].Equals(ConvertEnumToSign(SignEnum.Times)))
                                {
                                    tmpLeftNode = null;
                                    while (i < lineWords.Length - 1 && lineWords[i + 1].Equals(ConvertEnumToSign(SignEnum.Times)))
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
                                        CreateLink(TLinkTypeEnum.Up, tmpActualNode, tmpUpNode);
                                        CreateFirstChildOrRightSiblingLink(tmpUpNode, tmpActualNode);

                                        tmpLeftNode = tmpUpNode;
                                        i += 2;
                                    }
                                    ListExpr.Add(new ExprExtraNode(tmpUpNode, ConvertSignToEnum(lineWords[i + 1][0])));
                                    i += 2;
                                }

                                if (i < lineWords.Length - 1)
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
                                        tmpActualNode = CreateTNode((TNodeTypeEnum)Enum.Parse(typeof(TNodeTypeEnum), Enum.GetName(typeof(SignEnum), ListExpr[i].sign)), null, null);
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

                            SetCurrentUpNode(ref currentUpNode, currentUpNode.up);

                            //what if end of statement???
                            HandleGoingBackOnTheTree(ref currentUpNode, lineWords);                          
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


        private void SetCurrentUpNode(ref TNode currentUpNode, TNode newCurrentUpNode)
        {
            currentUpNode = newCurrentUpNode;
        }
        private TNode FindLastChild(TNode root)
        {
            TNode tmp = root.firstChild;
            while (tmp.rightSibling != null)
            { tmp = tmp.rightSibling; }

            return tmp;
        }


        private void HandleGoingBackOnTheTree(ref TNode currentUpNode, string[] lineWords)
        {
            int howManyLevelUp = determineHowManyLevelUp(lineWords);
            if (howManyLevelUp > 0)
            {
                FindAndSetCurrentUpNode(howManyLevelUp, ref currentUpNode);
            }
        }
        private void FindAndSetCurrentUpNode(int howManyLevelUp, ref TNode currentUpNode)
        {
            TNode lastChild;

            for (int i = 0; i < howManyLevelUp; i++)
            {
                SetCurrentUpNode(ref currentUpNode, currentUpNode.up);
                switch (currentUpNode.type)
                {
                    case TNodeTypeEnum.If:
                        lastChild = FindLastChild(currentUpNode);
                        if (lastChild.type == TNodeTypeEnum.StmtLstElse)
                        {
                            SetCurrentUpNode(ref currentUpNode, currentUpNode.up);
                        }
                        break;

                    case TNodeTypeEnum.While:
                        SetCurrentUpNode(ref currentUpNode, currentUpNode.up);
                        break;

                    case TNodeTypeEnum.Procedure:
                        SetCurrentUpNode(ref currentUpNode, currentUpNode.up);
                        break;

                    default:
                        SetCurrentUpNode(ref currentUpNode, currentUpNode.up);
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
        private bool wordIsConstant(string v)
        {
            for (int i = 0; i < v.Length; i++)
            {
                if (v[i] < 47 || v[i] > 58) { return false; }
            }

            return true;
        }

    }
}
