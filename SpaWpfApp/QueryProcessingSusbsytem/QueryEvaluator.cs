using SpaWpfApp.Ast;
using SpaWpfApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    public class QueryEvaluator
    {
        private static QueryEvaluator instance;
        private AstManager astManager;
        private Result result;
        private QueryPreProcessor queryPreProcessor;
        private Relation actualRelation;

        public static QueryEvaluator GetInstance()
        {
            if (instance == null)
            {
                instance = new QueryEvaluator();
            }
            return instance;
        }

        public QueryEvaluator()
        {
            this.astManager = AstManager.GetInstance();
            this.result = Result.GetInstance();
            this.queryPreProcessor = QueryPreProcessor.GetInstance(); ;
        }


        public void Evaluate(List<Relation> relationList)
        {
            result.Init();

            foreach (var relation in relationList)
            {
                switch (relation.type)
                {
                    case Relation.Parent:
                        Parent(relation);
                        break;

                    case Relation.ParentX:
                        ParentX(relation);
                        break;

                    case Relation.Follows:
                        Follows(relation);
                        break;
                    case Relation.FollowsX:
                        FolowsX(relation);
                        break;
                }
                Result r = result; // do testów, potem do usunięcia ta linia
            }
        }

        private void FolowsX(Relation relation)
        {
            List<TNode> candidateForFrom, candidateForTo;
            List<TNode> resultList = new List<TNode>();
            actualRelation = relation;


            #region FollowsX(int, int), FollowsX(_, _)
            if (relation.arg1type == Entity._int && relation.arg2type == Entity._int)
            {
                bool result = astManager.IsFollowsX(Int32.Parse(relation.arg1), Int32.Parse(relation.arg2));

                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                var nodes = astManager.NodeList;
                List<TNode> result;
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        result = astManager.GetRightSiblingX(node, relation.arg2);
                        if (result != null)
                        {
                            UpdateResultTable(true);
                            return;
                        }
                    }
                }

                UpdateResultTable(false);
                return;
            }
            #endregion

            else if (relation.arg1type == Entity._int)
            {
                TNode from = astManager.FindNode(Int32.Parse(relation.arg1));

                #region FollowsX(int, _)
                if (relation.arg2type == Entity._)
                {
                    if (from is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = astManager.GetRightSiblingX(from, relation.arg2) != null ? true : false;
                        UpdateResultTable(result);
                        return;
                    }
                }
                #endregion

                #region FollowsX(int, *)
                if (from is null)
                {
                    UpdateResultTable(null, relation.arg2);
                    return;
                }
                else if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = result.GetNodes(relation.arg2);
                    if (candidateForTo != null)
                    {
                        foreach (var c in candidateForTo)
                        {
                            if (astManager.IsFollowsX(Int32.Parse(relation.arg1), (int)c.programLine))
                            {
                                resultList.Add(c);
                            }
                        }
                    }
                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
                else
                {
                    resultList = astManager.GetRightSiblingX(from, relation.arg2);
                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
                #endregion
            }

            else if (relation.arg2type == Entity._int)
            {
                TNode to = astManager.FindNode(Int32.Parse(relation.arg2));

                #region FollowsX(_, int)
                if (relation.arg1type == Entity._)
                {
                    if (to is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = astManager.GetLeftSiblingX(to, relation.arg1);
                        UpdateResultTable(result != null ? true : false);
                        return;
                    }
                }
                #endregion

                #region FollowsX(*, int)
                if (to is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }
                else if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = result.GetNodes(relation.arg1);
                    if (candidateForFrom != null)
                    {
                        foreach (var c in candidateForFrom)
                        {
                            if (astManager.IsFollowsX((int)c.programLine, Int32.Parse(relation.arg2)))
                            {
                                resultList.Add(c);
                                break;
                            }
                        }
                    }
                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }
                else
                {
                    resultList = astManager.GetLeftSiblingX(to, relation.arg1);
                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }
                #endregion
            }

            else
            {
                List<TNode> fromList = null;
                List<TNode> tmpResult = null;

                #region FollowsX(_, *)
                if (relation.arg1type == Entity._)
                {
                    fromList = astManager.NodeList;

                    if (fromList != null)
                    {
                        if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForTo = result.GetNodes(relation.arg2);
                            if (candidateForTo != null)
                            {
                                foreach (var from in fromList)
                                {
                                    foreach (var to in candidateForTo)
                                    {
                                        if (astManager.IsFollowsX((int)from.programLine, (int)to.programLine))
                                        {
                                            resultList.Add(to);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var from in fromList)
                            {
                                tmpResult = astManager.GetRightSiblingX(from, relation.arg2);
                                UpdateResultTable(resultList, relation.arg2);
                                return;
                            }
                        }

                        UpdateResultTable(resultList, relation.arg2);
                        return;
                    }
                    else
                    {
                        UpdateResultTable(null, relation.arg2);
                        return;
                    }

                }
                #endregion

                else
                {
                    #region FollowsX(*, _)
                    if (relation.arg2 == Entity._)
                    {
                        if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFrom = result.GetNodes(relation.arg1);
                        }
                        else
                        {
                            candidateForFrom = astManager.NodeList;
                        }

                        if (candidateForFrom != null)
                        {
                            foreach (var from in candidateForFrom)
                            {
                                var hasRightSibling = astManager.GetRightSiblingX(from, relation.arg2) != null ? true : false;
                                if (hasRightSibling) { resultList.Add(from); }
                            }
                        }

                        UpdateResultTable(resultList, relation.arg1);
                        return;
                    }
                    #endregion
                }

                #region FollowsX(*, *)
                List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

                //candidates for from
                if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = result.GetNodes(relation.arg1);
                }
                else
                {
                    candidateForFrom = astManager.NodeList;
                }

                //candidates for to
                if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = result.GetNodes(relation.arg2);
                    if (candidateForTo != null)
                    {
                        for (int i = 0; i < candidateForFrom.Count(); i++)
                        {
                            if (astManager.IsFollowsX((int)candidateForFrom[i].programLine, (int)candidateForTo[i].programLine))
                            {
                                resultListTuple.Add((candidateForFrom[i], candidateForTo[i]));
                            }
                        }
                    }
                }
                else
                {
                    foreach (var from in candidateForFrom)
                    {
                        tmpResult = astManager.GetRightSiblingX(from, relation.arg2);
                        if (tmpResult != null)
                        {
                            foreach (var to in tmpResult)
                            {
                                resultListTuple.Add((from, to));
                            }
                        }
                    }
                }

                UpdateResultTable(resultListTuple, relation.arg1, relation.arg2);
                return;
                #endregion
            }
        }


        private void Follows(Relation relation)
        {
            List<TNode> candidateForFrom, candidateForTo;
            List<TNode> resultList = new List<TNode>();
            actualRelation = relation;


            #region Follows(int, int), Follows(_, _)
            if (relation.arg1type == Entity._int && relation.arg2type == Entity._int)
            {
                bool result = astManager.IsFollows(Int32.Parse(relation.arg1), Int32.Parse(relation.arg2));

                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                var nodes = astManager.NodeList;
                TNode result;
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        result = astManager.GetRightSibling(node, relation.arg2);
                        if (result != null)
                        {
                            UpdateResultTable(true);
                            return;
                        }
                    }
                }

                UpdateResultTable(false);
                return;
            }
            #endregion

            else if (relation.arg1type == Entity._int)
            {
                TNode from = astManager.FindNode(Int32.Parse(relation.arg1));

                #region Follows(int, _)
                if (relation.arg2type == Entity._)
                {
                    if (from is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = astManager.GetRightSibling(from, relation.arg2) != null ? true : false;
                        UpdateResultTable(result);
                        return;
                    }
                }
                #endregion

                #region Follows(int, *)
                if (from is null)
                {
                    UpdateResultTable(null, relation.arg2);
                    return;
                }
                else if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = result.GetNodes(relation.arg2);
                    if (candidateForTo != null)
                    {
                        foreach (var c in candidateForTo)
                        {
                            if (astManager.IsFollows(Int32.Parse(relation.arg1), (int)c.programLine))
                            {
                                resultList.Add(c);
                            }
                        }
                    }
                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
                else
                {
                    TNode tmp = astManager.GetRightSibling(from, relation.arg2);
                    if (tmp != null) { resultList.Add(tmp); }
                    UpdateResultTable(resultList, relation.arg2);
                }
                #endregion
            }

            else if (relation.arg2type == Entity._int)
            {
                TNode to = astManager.FindNode(Int32.Parse(relation.arg2));

                #region Follows(_, int)
                if (relation.arg1type == Entity._)
                {
                    if (to is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = astManager.GetLeftSibling(to, relation.arg1);
                        UpdateResultTable(result != null ? true : false);
                        return;
                    }
                }
                #endregion

                #region Follows(*, int)
                if (to is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }
                else if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = result.GetNodes(relation.arg1);
                    if (candidateForFrom != null)
                    {
                        foreach (var c in candidateForFrom)
                        {
                            if (astManager.IsFollows((int)c.programLine, Int32.Parse(relation.arg2)))
                            {
                                resultList.Add(c);
                                break;
                            }
                        }
                    }
                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }
                else
                {
                    TNode tmp = astManager.GetLeftSibling(to, relation.arg1);
                    if (tmp != null) { resultList.Add(tmp); }
                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }
                #endregion
            }

            else
            {
                List<TNode> fromList = null;
                List<TNode> tmpResult = null;

                #region Follows(_, *)
                if (relation.arg1type == Entity._)
                {
                    fromList = astManager.NodeList;

                    if (fromList != null)
                    {
                        if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForTo = result.GetNodes(relation.arg2);
                            if (candidateForTo != null)
                            {
                                foreach (var from in fromList)
                                {
                                    foreach (var to in candidateForTo)
                                    {
                                        if (astManager.IsFollows((int)from.programLine, (int)to.programLine))
                                        {
                                            resultList.Add(to);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var from in fromList)
                            {
                                TNode tmp = astManager.GetRightSibling(from, relation.arg2);
                                if (tmp != null) { resultList.Add(tmp); }
                            }
                        }

                        UpdateResultTable(resultList, relation.arg2);
                        return;
                    }
                    else
                    {
                        UpdateResultTable(null, relation.arg2);
                        return;
                    }

                }
                #endregion

                else
                {
                    #region Follows(*, _)
                    if (relation.arg2 == Entity._)
                    {
                        if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFrom = result.GetNodes(relation.arg1);
                        }
                        else
                        {
                            candidateForFrom = astManager.NodeList;
                        }

                        if (candidateForFrom != null)
                        {
                            foreach (var from in candidateForFrom)
                            {
                                var hasRightSibling = astManager.GetRightSibling(from, relation.arg2) != null ? true : false;
                                if (hasRightSibling) { resultList.Add(from); }
                            }
                        }

                        UpdateResultTable(resultList, relation.arg1);
                        return;
                    }
                    #endregion
                }

                #region Follows(*, *)
                List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

                //candidates for from
                if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = result.GetNodes(relation.arg1);
                }
                else
                {
                    candidateForFrom = astManager.NodeList;
                }

                //candidates for to
                if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = result.GetNodes(relation.arg2);
                    if (candidateForTo != null)
                    {
                        for (int i = 0; i < candidateForFrom.Count(); i++)
                        {
                            if (astManager.IsFollows((int)candidateForFrom[i].programLine, (int)candidateForTo[i].programLine))
                            {
                                resultListTuple.Add((candidateForFrom[i], candidateForTo[i]));
                            }
                        }
                    }
                }
                else
                {
                    foreach (var from in candidateForFrom)
                    {
                        TNode tmp = astManager.GetRightSibling(from, relation.arg2);
                        if (tmp != null)
                        {
                            resultListTuple.Add((from, tmp));
                        }
                    }
                }

                UpdateResultTable(resultListTuple, relation.arg1, relation.arg2);
                return;
                #endregion
            }
        }


        private void Parent(Relation relation)
        {
            List<TNode> candidateForChildren, candidateForFather;
            List<TNode> resultList = new List<TNode>();
            actualRelation = relation;


            #region Parent(int, int), Parent(_, _)
            if (relation.arg1type == Entity._int && relation.arg2type == Entity._int)
            {
                bool result = astManager.IsParent(Int32.Parse(relation.arg1), Int32.Parse(relation.arg2));

                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                var fathers = astManager.GetAllParents();
                List<TNode> result;
                if (fathers != null)
                {
                    foreach (var f in fathers)
                    {
                        result = astManager.GetChildren(f, relation.arg2);
                        if (result != null)
                        {
                            UpdateResultTable(true);
                            return;
                        }
                    }
                }

                UpdateResultTable(false);
                return;
            }
            #endregion

            else if (relation.arg1type == Entity._int)
            {
                TNode father = astManager.FindFather(Int32.Parse(relation.arg1));

                #region Parent(int, _)
                if (relation.arg2type == Entity._)
                {
                    if (father is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = astManager.GetChildren(father, relation.arg2) != null ? true : false;
                        UpdateResultTable(result);
                        return;
                    }
                }
                #endregion

                #region Parent(int, *)
                if (father is null)
                {
                    UpdateResultTable(null, relation.arg2);
                    return;
                }
                else if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForChildren = result.GetNodes(relation.arg2);
                    if (candidateForChildren != null)
                    {
                        foreach (var c in candidateForChildren)
                        {
                            if (astManager.IsParent(Int32.Parse(relation.arg1), (int)c.programLine))
                            {
                                resultList.Add(c);
                            }
                        }
                    }
                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
                else
                {
                    resultList = astManager.GetChildren(father, relation.arg2);
                    UpdateResultTable(resultList, relation.arg2);
                }
                #endregion
            }

            else if (relation.arg2type == Entity._int)
            {
                TNode child = astManager.FindNode(Int32.Parse(relation.arg2));

                #region Parent(_, int)
                if (relation.arg1type == Entity._)
                {
                    if (child is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = astManager.GetParent(child, relation.arg1);
                        UpdateResultTable(result != null ? true : false);
                        return;
                    }
                }
                #endregion

                #region Parent(*, int)
                if (child is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }
                else if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFather = result.GetNodes(relation.arg1);
                    if (candidateForFather != null)
                    {
                        foreach (var c in candidateForFather)
                        {
                            if (astManager.IsParent((int)c.programLine, Int32.Parse(relation.arg2)))
                            {
                                resultList.Add(c);
                                break;
                            }
                        }
                    }
                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }
                else
                {
                    TNode tmp = astManager.GetParent(child, relation.arg1);
                    if (tmp != null) { resultList.Add(tmp); }
                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }
                #endregion
            }

            else
            {
                List<TNode> fathers = null;
                List<TNode> tmpResult = null;

                #region Parent(_, *)
                if (relation.arg1type == Entity._)
                {
                    fathers = astManager.GetAllParents();

                    if (fathers != null)
                    {
                        if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForChildren = result.GetNodes(relation.arg2);
                            if (candidateForChildren != null)
                            {
                                foreach (var father in fathers)
                                {
                                    foreach (var child in candidateForChildren)
                                    {
                                        if (astManager.IsParent((int)father.programLine, (int)child.programLine))
                                        {
                                            resultList.Add(child);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var father in fathers)
                            {
                                tmpResult = astManager.GetChildren(father, relation.arg2);
                                if (tmpResult != null)
                                {
                                    foreach (var child in tmpResult)
                                    {
                                        resultList.Add(child);
                                    }
                                }
                            }
                        }

                        UpdateResultTable(resultList, relation.arg2);
                        return;
                    }
                    else
                    {
                        UpdateResultTable(null, relation.arg2);
                        return;
                    }

                }
                #endregion

                else
                {
                    #region Parent(*, _)
                    if (relation.arg2 == Entity._)
                    {
                        if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFather = result.GetNodes(relation.arg1);
                        }
                        else
                        {
                            if (relation.arg1type == Entity.stmt)
                            {
                                candidateForFather = astManager.GetAllParents();
                            }
                            else if (relation.arg1type == Entity._if)
                            {
                                candidateForFather = astManager.GetAllIf();
                            }
                            else
                            {
                                candidateForFather = astManager.GetAllWhile();
                            }
                        }

                        if (candidateForFather != null)
                        {
                            foreach (var father in candidateForFather)
                            {
                                var hasChildren = astManager.GetChildren(father, relation.arg2) != null ? true : false;
                                if (hasChildren) { resultList.Add(father); }
                            }
                        }

                        UpdateResultTable(resultList, relation.arg1);
                        return;
                    }
                    #endregion
                }

                #region Parent(*, *)
                List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

                //candidates for fathers
                if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFather = result.GetNodes(relation.arg1);
                }
                else
                {
                    if (relation.arg1type == Entity.stmt)
                    {
                        candidateForFather = astManager.GetAllParents();
                    }
                    else if (relation.arg1type == Entity._if)
                    {
                        candidateForFather = astManager.GetAllIf();
                    }
                    else
                    {
                        candidateForFather = astManager.GetAllWhile();
                    }
                }

                //candidates for children
                if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForChildren = result.GetNodes(relation.arg2);
                    if (candidateForChildren != null)
                    {
                        for (int i = 0; i < candidateForFather.Count(); i++)
                        {
                            if (astManager.IsParent((int)candidateForFather[i].programLine, (int)candidateForChildren[i].programLine))
                            {
                                resultListTuple.Add((candidateForFather[i], candidateForChildren[i]));
                            }
                        }
                    }
                }
                else
                {
                    foreach (var father in candidateForFather)
                    {
                        tmpResult = astManager.GetChildren(father, relation.arg2);
                        if (tmpResult != null)
                        {
                            foreach (var child in tmpResult)
                            {
                                resultListTuple.Add((father, child));
                            }
                        }
                    }
                }

                UpdateResultTable(resultListTuple, relation.arg1, relation.arg2);
                return;
                #endregion
            }
        }

        private void ParentX(Relation relation)
        {
            List<TNode> candidateForChildren, candidateForFather;
            List<TNode> resultList = new List<TNode>();
            actualRelation = relation;


            #region ParentX(int, int), ParentX(_, _)
            if (relation.arg1type == Entity._int && relation.arg2type == Entity._int)
            {
                bool result = astManager.IsParentX(Int32.Parse(relation.arg1), Int32.Parse(relation.arg2));

                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                var fathers = astManager.GetAllParents();
                List<TNode> result;
                if (fathers != null)
                {
                    foreach (var f in fathers)
                    {
                        result = astManager.GetChildrenX(f, relation.arg2);
                        if (result != null)
                        {
                            UpdateResultTable(true);
                            return;
                        }
                    }
                }

                UpdateResultTable(false);
                return;
            }
            #endregion

            else if (relation.arg1type == Entity._int)
            {
                TNode father = astManager.FindFather(Int32.Parse(relation.arg1));

                #region ParentX(int, _)
                if (relation.arg2type == Entity._)
                {
                    if (father is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = astManager.GetChildrenX(father, relation.arg2) != null ? true : false;
                        UpdateResultTable(result);
                        return;
                    }
                }
                #endregion

                #region ParentX(int, *)
                if (father is null)
                {
                    UpdateResultTable(null, relation.arg2);
                    return;
                }
                else if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForChildren = result.GetNodes(relation.arg2);
                    if (candidateForChildren != null)
                    {
                        foreach (var c in candidateForChildren)
                        {
                            if (astManager.IsParentX(Int32.Parse(relation.arg1), (int)c.programLine))
                            {
                                resultList.Add(c);
                            }
                        }
                    }
                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
                else
                {
                    resultList = astManager.GetChildrenX(father, relation.arg2);
                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
                #endregion
            }

            else if (relation.arg2type == Entity._int)
            {
                TNode child = astManager.FindNode(Int32.Parse(relation.arg2));

                #region ParentX(_, int)
                if (relation.arg1type == Entity._)
                {
                    if (child is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = astManager.GetParentX(child, relation.arg1);
                        UpdateResultTable(result != null ? true : false);
                        return;
                    }
                }
                #endregion

                #region ParentX(*, int)
                if (child is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }
                else if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFather = result.GetNodes(relation.arg1);
                    if (candidateForFather != null)
                    {
                        foreach (var c in candidateForFather)
                        {
                            if (astManager.IsParentX((int)c.programLine, Int32.Parse(relation.arg2)))
                            {
                                resultList.Add(c);
                                break;
                            }
                        }
                    }
                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }
                else
                {
                    resultList = astManager.GetParentX(child, relation.arg1);
                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }
                #endregion
            }

            else
            {
                List<TNode> fathers = null;
                List<TNode> tmpResult = null;

                #region ParentX(_, *)
                if (relation.arg1type == Entity._)
                {
                    fathers = astManager.GetAllParents();

                    if (fathers != null)
                    {
                        if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForChildren = result.GetNodes(relation.arg2);
                            if (candidateForChildren != null)
                            {
                                foreach (var father in fathers)
                                {
                                    foreach (var child in candidateForChildren)
                                    {
                                        if (astManager.IsParentX((int)father.programLine, (int)child.programLine))
                                        {
                                            resultList.Add(child);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var father in fathers)
                            {
                                tmpResult = astManager.GetChildrenX(father, relation.arg2);
                                if (tmpResult != null)
                                {
                                    foreach (var child in tmpResult)
                                    {
                                        resultList.Add(child);
                                    }
                                }
                            }
                        }

                        UpdateResultTable(resultList, relation.arg2);
                        return;
                    }
                    else
                    {
                        UpdateResultTable(null, relation.arg2);
                        return;
                    }

                }
                #endregion

                else
                {
                    #region ParentX(*, _)
                    if (relation.arg2 == Entity._)
                    {
                        if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFather = result.GetNodes(relation.arg1);
                        }
                        else
                        {
                            if (relation.arg1type == Entity.stmt)
                            {
                                candidateForFather = astManager.GetAllParents();
                            }
                            else if (relation.arg1type == Entity._if)
                            {
                                candidateForFather = astManager.GetAllIf();
                            }
                            else
                            {
                                candidateForFather = astManager.GetAllWhile();
                            }
                        }

                        if (candidateForFather != null)
                        {
                            foreach (var father in candidateForFather)
                            {
                                var hasChildren = astManager.GetChildrenX(father, relation.arg2) != null ? true : false;
                                if (hasChildren) { resultList.Add(father); }
                            }
                        }

                        UpdateResultTable(resultList, relation.arg1);
                        return;
                    }
                    #endregion
                }

                #region ParentX(*, *)
                List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

                //candidates for fathers
                if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFather = result.GetNodes(relation.arg1);
                }
                else
                {
                    if (relation.arg1type == Entity.stmt)
                    {
                        candidateForFather = astManager.GetAllParents();
                    }
                    else if (relation.arg1type == Entity._if)
                    {
                        candidateForFather = astManager.GetAllIf();
                    }
                    else
                    {
                        candidateForFather = astManager.GetAllWhile();
                    }
                }

                //candidates for children
                if (result.HasRecords() && result.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForChildren = result.GetNodes(relation.arg2);
                    if (candidateForChildren != null)
                    {
                        for (int i = 0; i < candidateForFather.Count(); i++)
                        {
                            if (astManager.IsParentX((int)candidateForFather[i].programLine, (int)candidateForChildren[i].programLine))
                            {
                                resultListTuple.Add((candidateForFather[i], candidateForChildren[i]));
                            }
                        }
                    }
                }
                else
                {
                    foreach (var father in candidateForFather)
                    {
                        tmpResult = astManager.GetChildrenX(father, relation.arg2);
                        if (tmpResult != null)
                        {
                            foreach (var child in tmpResult)
                            {
                                resultListTuple.Add((father, child));
                            }
                        }
                    }
                }

                UpdateResultTable(resultListTuple, relation.arg1, relation.arg2);
                return;
                #endregion
            }
        }


        private void UpdateResultTable(List<(TNode, TNode)> resultListTuple, string firstArgument, string secondArgument)
        {
            if (resultListTuple != null && resultListTuple.Any())
            {
                List<TNode[]> newResultTableList = new List<TNode[]>();
                int indexOfDeclarationFirstArgument = result.FindIndexOfDeclaration(firstArgument);
                int indexOfDeclarationSecondArgument = result.FindIndexOfDeclaration(secondArgument);

                if (result.HasRecords())
                {
                    var earlierResultRecords = result.GetResultTableList();

                    foreach (var result in resultListTuple)
                    {
                        foreach (var record in earlierResultRecords)
                        {
                            record[indexOfDeclarationFirstArgument] = result.Item1;
                            record[indexOfDeclarationSecondArgument] = result.Item2;
                            newResultTableList.Add(record);
                        }
                    }
                }
                else
                {
                    TNode[] newRecord;
                    foreach (var res in resultListTuple)
                    {
                        newRecord = new TNode[result.declarationsTable.Length];
                        newRecord[indexOfDeclarationFirstArgument] = res.Item1;
                        newRecord[indexOfDeclarationSecondArgument] = res.Item2;
                        newResultTableList.Add(newRecord);

                    }
                }

                result.UpdateResutTableList(newResultTableList);
            }
            else
            {
                result.ClearResultTableList();
                FinishQueryEvaluator();
            }

            result.SetDeclarationWasDeterminated(firstArgument);
            result.SetDeclarationWasDeterminated(secondArgument);
        }

        private void UpdateResultTable(List<TNode> resultList, string argumentLookingFor)
        {
            if (resultList != null && resultList.Any())
            {
                List<TNode[]> newResultTableList = new List<TNode[]>();
                int indexOfDeclaration = result.FindIndexOfDeclaration(argumentLookingFor);

                if (result.HasRecords())
                {
                    var earlierResultRecords = result.GetResultTableList();

                    foreach (var result in resultList)
                    {
                        foreach (var record in earlierResultRecords)
                        {
                            record[indexOfDeclaration] = result;
                            newResultTableList.Add(record);
                        }
                    }
                }
                else
                {
                    TNode[] newRecord;
                    foreach (var res in resultList)
                    {
                        newRecord = new TNode[result.declarationsTable.Length];
                        newRecord[indexOfDeclaration] = res;
                        newResultTableList.Add(newRecord);
                    }
                }

                result.UpdateResutTableList(newResultTableList);
            }
            else
            {
                result.ClearResultTableList();
                FinishQueryEvaluator();
            }

            result.SetDeclarationWasDeterminated(argumentLookingFor);
        }

        private void UpdateResultTable(bool p_result)
        {
            if (queryPreProcessor.ReturnTypeIsBoolean())
            {
                result.ResultBoolean = p_result;
            }
            else
            {
                if (p_result is false)
                {
                    result.ClearResultTableList();
                }
            }

            if (p_result is false)
            {
                FinishQueryEvaluator();
            }

        }

        private void FinishQueryEvaluator()
        {
            throw new NoResultsException("Relation " + actualRelation.ToString() + " has no results.");
        }
    }
}
