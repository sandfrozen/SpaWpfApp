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

    public class AST : ASTAPI
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
                if(sourceCodeLine == "") { break; }
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
                            for (int i = 2; i < lineWords.Length - 1 && lineWords[i][0] != 125; i += 2)
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

                                ListExpr.Clear();
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
                
                switch (currentUpNode.type)
                {
                    case TNodeTypeEnum.StmtLstThen:
                        {
                            SetCurrentUpNode(ref currentUpNode, currentUpNode.up);
                        }
                        break;

                    case TNodeTypeEnum.StmtLstElse:
                        {
                            SetCurrentUpNode(ref currentUpNode, currentUpNode.up);
                            SetCurrentUpNode(ref currentUpNode, currentUpNode.up);
                        }
                        break;

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


        /// <summary>
        /// returns Parent of stmt or null if stmt does't have Parent
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        public TNode GetParent(int p_programLineNumber)
        {
            TNode tmp = NodeList.Where(p => p.programLine == p_programLineNumber).FirstOrDefault();
            TNode parent = tmp.up.up;

            if (parent.type == TNodeTypeEnum.If || parent.type == TNodeTypeEnum.While)
            {
                return parent;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// returns direct Parent and all indirect Parent of stmt or null if stmt doesn't have any ParentS
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        public List<TNode> GetParentS(int p_programLineNumber)
        {
            TNode tmp = NodeList.Where(p => p.programLine == p_programLineNumber).FirstOrDefault();
            TNode parent;
            List<TNode> parents = new List<TNode>();

            do
            {
                parent = tmp.up.up;
                if (parent.type == TNodeTypeEnum.While || parent.type == TNodeTypeEnum.If)
                {
                    parents.Add(parent);
                }
            }
            while (parent.type != TNodeTypeEnum.Procedure);

            return parents.Count() > 0 ? parents : null;
        }

        /// <summary>
        /// returns all children of stmt or null if stmt doesn't have children
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        public List<TNode> GetChildren(int p_programLineNumber)
        {
            List<TNode> children = new List<TNode>();
            TNode parent = NodeList.Where(p => p.programLine == p_programLineNumber).FirstOrDefault();
            TNode child;


            if (parent.type == TNodeTypeEnum.While)
            {
                child = parent.firstChild.rightSibling.firstChild; // first stmf of stmtLst
                do
                {
                    children.Add(child);
                    child = child.rightSibling;
                }
                while (child != null);

                return children;
            }
            else if (parent.type == TNodeTypeEnum.If)
            {
                child = parent.firstChild.rightSibling.firstChild; // first stmf of stmtLstThen
                do
                {
                    children.Add(child);
                    child = child.rightSibling;
                }
                while (child != null);

                child = parent.firstChild.rightSibling.rightSibling.firstChild; // fisrt stmt of stmtLstElse
                do
                {
                    children.Add(child);
                    child = child.rightSibling;
                }
                while (child != null);

                return children;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// returns all direct and indirect children of stmt or null if stmt doesn't have any childrenS
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        public List<TNode> GetChildrenS(int p_programLineNumber)
        {
            List<TNode> children = new List<TNode>();
            TNode parent = NodeList.Where(p => p.programLine == p_programLineNumber).FirstOrDefault();
            TNode child;

            if (parent.type != TNodeTypeEnum.While && parent.type != TNodeTypeEnum.If)
            {
                return null;
            }
            else
            {
                FindAllChildrenS(ref children, parent); // recurrere
            }

            children.Remove(children.Where(p => p.programLine == p_programLineNumber).FirstOrDefault());
            return children.Count() > 0 ? children : null;
        }

        public void FindAllChildrenS(ref List<TNode> children, TNode parent)
        {
            TNode tmp;
            List<TNode> tmpChildren;
            if (parent.firstChild is null)
            {
                return;
            }


            if (parent.type == TNodeTypeEnum.While || parent.type == TNodeTypeEnum.If || parent.type == TNodeTypeEnum.Assign || parent.type == TNodeTypeEnum.Call)
            {
                children.Add(parent);
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



        /// <summary>
        /// returns direct right sibling of stmt or null if stmt doesn't have right sibling
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        public TNode GetRightSibling(int p_programLineNumber)
        {
            TNode tmp = NodeList.Where(p => p.programLine == p_programLineNumber).FirstOrDefault();

            if(tmp.rightSibling is null)
            {
                return null;
            }
            else
            {
                return tmp.rightSibling;
            }
        }

        /// <summary>
        /// returns direct left sibling of stmt or null if stmt is the only child
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        public TNode GetLeftSibling(int p_programLineNumber)
        {
            TNode parent = NodeList.Where(p => p.programLine == p_programLineNumber).FirstOrDefault().up;
            TNode tmp = parent.firstChild;

            if(tmp.programLine == p_programLineNumber) // if stmt is the only child
            {
                return null;
            }
            else
            {
                while(tmp.rightSibling.programLine != p_programLineNumber)
                {
                    tmp = tmp.rightSibling;
                }

                return tmp;
            }
        }

        /// <summary>
        /// retrurns list of stmt that Follows stmt or null if stmt doesn't have rightSiblingS
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        public List<TNode> GetRightSiblingS(int p_programLineNumber)
        {
            TNode tmp = NodeList.Where(p => p.programLine == p_programLineNumber).FirstOrDefault();
            List<TNode> rightSiblingS = new List<TNode>();

            if (tmp.rightSibling is null)
            {
                return null;
            }
            else
            {
                do
                {
                    rightSiblingS.Add(tmp.rightSibling);
                    tmp = tmp.rightSibling;
                }
                while (tmp.rightSibling != null);

                return rightSiblingS;
            }
        }

        /// <summary>
        /// returns list of leftSiblingS or null if stmt is the only child
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        public List<TNode> GetLeftSiblingS(int p_programLineNumber)
        {
            TNode parent = NodeList.Where(p => p.programLine == p_programLineNumber).FirstOrDefault().up;
            TNode tmp = parent.firstChild;
            List<TNode> leftSiblingS = new List<TNode>();

            if (tmp.programLine == p_programLineNumber) // if stmt is the only child
            {
                return null;
            }
            else
            {
                do
                {
                    leftSiblingS.Add(tmp);
                    tmp = tmp.rightSibling;
                }
                while (tmp.programLine != p_programLineNumber);

                return leftSiblingS;
            }
        }
    }
}
