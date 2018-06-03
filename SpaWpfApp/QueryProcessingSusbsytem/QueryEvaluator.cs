﻿using SpaWpfApp.Ast;
using SpaWpfApp.Cfg;
using SpaWpfApp.Exceptions;
using SpaWpfApp.PkbNew;
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
        private CfgManager cfgManager;
        public QueryResult queryResult { get; }
        private QueryPreProcessor queryPreProcessor;
        private Condition actualCondition;
        public PkbAPI pkb { get; set; }

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
            this.queryResult = QueryResult.GetInstance();
            this.queryPreProcessor = QueryPreProcessor.GetInstance(); ;
            this.cfgManager = CfgManager.GetInstance();
        }


        public void Evaluate(List<Condition> conditionsList)
        {
            queryResult.Init();

            foreach (var condition in conditionsList)
            {
                actualCondition = condition;
                if (condition is Relation)
                {
                    var relation = (Relation)condition;
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

                        case Relation.Next:
                            Next(relation);
                            break;
                        case Relation.NextX:
                            NextX(relation);
                            break;

                        case Relation.Modifies:
                            Modifies(relation);
                            break;
                    }
                }
                else if (condition is Pattern)
                {
                    var pattern = (Pattern)condition; // dalej obsluga pattern
                    DoPattern(pattern);
                }
                else if (condition is With)
                {
                    var with = (With)condition; // dalej obsluga with
                    DoWith(with);
                }
                QueryResult r = queryResult; // do testów, potem do usunięcia ta linia
            }

            HandleBooleanReturn();
        }

        private void DoWith(With with)
        {
            if (with.leftType.Contains('.'))
            {
                string leftType = with.leftType.Substring(0, with.leftType.IndexOf('.'));
                string leftSynonym = with.left.Substring(0, with.left.IndexOf('.'));
                string leftAttrName = with.left.Substring(with.left.IndexOf('.'));

                List<TNode> candidates;
                List<TNode> resultList = new List<TNode>();

                if (leftType == Entity.assign || leftType == Entity.stmt || leftType == Entity._if || leftType == Entity._while)
                {
                    int rightValue = Int32.Parse(with.right);
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(leftSynonym))
                    {
                        candidates = queryResult.GetNodes(leftSynonym);
                    }
                    else
                    {
                        candidates = astManager.GetNodes(leftType);
                    }

                    if (candidates is null)
                    {
                        UpdateResultTable(null, leftSynonym);
                    }

                    foreach (var c in candidates)
                    {
                        if (c.programLine == rightValue)
                        {
                            resultList.Add(c);
                        }
                    }

                    UpdateResultTable(resultList, leftSynonym);
                    return;
                }

                if (leftType == Entity.procedure)
                {
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(leftSynonym))
                    {
                        candidates = queryResult.GetNodes(leftSynonym);
                    }
                    else
                    {
                        candidates = astManager.GetNodes(leftType);
                    }

                    if (candidates is null)
                    {
                        UpdateResultTable(null, leftSynonym);
                    }

                    foreach (var c in candidates)
                    {
                        if (c.indexOfName == pkb.GetProcIndex(with.right))
                        {
                            resultList.Add(c);
                        }
                    }

                    UpdateResultTable(resultList, leftSynonym);
                }

                if (leftType == Entity.call)
                {
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(leftSynonym))
                    {
                        candidates = queryResult.GetNodes(leftSynonym);
                    }
                    else
                    {
                        candidates = astManager.GetNodes(leftType);
                    }

                    if (candidates is null)
                    {
                        UpdateResultTable(null, leftSynonym);
                    }

                    foreach (var c in candidates)
                    {
                        if (c.indexOfName == pkb.GetProcIndex(with.right))
                        {
                            resultList.Add(c);
                        }
                    }

                    UpdateResultTable(resultList, leftSynonym);
                }

                if (leftType == Entity.variable)
                {
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(leftSynonym))
                    {
                        candidates = queryResult.GetNodes(leftSynonym);
                    }
                    else
                    {
                        candidates = astManager.GetNodes(Entity.variable);
                    }

                    if (candidates is null)
                    {
                        UpdateResultTable(null, leftSynonym);
                    }

                    foreach (var c in candidates)
                    {
                        if (c.indexOfName == pkb.GetProcIndex(with.right))
                        {
                            resultList.Add(c);
                        }
                    }

                    UpdateResultTable(resultList, leftSynonym);
                }

                if (leftType == Entity.constant)
                {
                    int rightValue = Int32.Parse(with.right);

                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(leftSynonym))
                    {
                        candidates = queryResult.GetNodes(leftSynonym);
                    }
                    else
                    {
                        candidates = astManager.GetNodes(leftType);
                    }

                    if (candidates is null)
                    {
                        UpdateResultTable(null, leftSynonym);
                    }

                    foreach (var c in candidates)
                    {
                        if (c.value == rightValue)
                        {
                            resultList.Add(c);
                        }
                    }

                    UpdateResultTable(resultList, leftSynonym);
                }
            }
        }


        private void DoPattern(Pattern pattern)
        {
            List<TNode> result = new List<TNode>();

            switch (pattern.synonymType)
            {
                case Entity.assign:
                    {
                        List<TNode> listA;
                        #region set listA
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(pattern.synonym))
                        {
                            listA = queryResult.GetNodes(pattern.synonym);
                        }
                        else
                        {
                            listA = astManager.GetAllAssigns();
                        }

                        if (pattern.arg1type == Entity._string)
                        {
                            List<TNode> copyListA = DeepCopy(listA);
                            foreach (var assign in copyListA)
                            {
                                if (assign.firstChild.indexOfName != pkb.GetVarIndex(pattern.arg1.Substring(1, pattern.arg1.Length - 2)))
                                {
                                    listA.Remove(assign);
                                }
                            }
                        }
                        #endregion

                        if (pattern.arg2type == Entity._)
                        {
                            result = listA;
                            UpdateResultTable(result, pattern.synonym);
                            return;
                        }
                        else
                        {
                            if (listA != null)
                            {
                                foreach (var assign in listA)
                                {
                                    if (AssignRightContainsPatter(assign, pattern.arg2))
                                    {
                                        result.Add(assign);
                                    }
                                }
                            }
                            UpdateResultTable(result, pattern.synonym);
                            return;
                        }


                    }
                    break;

                case Entity._while:
                    {
                        List<TNode> listW;
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(pattern.synonym))
                        {
                            listW = queryResult.GetNodes(pattern.synonym);
                        }
                        else
                        {
                            listW = astManager.GetAllWhile();
                        }

                        if (listW is null)
                        {
                            UpdateResultTable(result, pattern.synonym);
                            return;
                        }

                        if (pattern.arg1type == Entity._)
                        {
                            result = listW;
                            UpdateResultTable(result, pattern.synonym);
                            return;
                        }

                        foreach (var w in listW)
                        {
                            if (w.firstChild.indexOfName == pkb.GetVarIndex(pattern.arg1.Replace("\"", ""))
                                && !result.Contains(w))
                            {
                                result.Add(w);
                            }
                        }

                        UpdateResultTable(result, pattern.synonym);
                        return;

                    }
                    break;

                case Entity._if:
                    {
                        List<TNode> listIf;
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(pattern.synonym))
                        {
                            listIf = queryResult.GetNodes(pattern.synonym);
                        }
                        else
                        {
                            listIf = astManager.GetAllIf();
                        }

                        if (listIf is null)
                        {
                            UpdateResultTable(result, pattern.synonym);
                            return;
                        }

                        if (pattern.arg1type == Entity._)
                        {
                            result = listIf;
                            UpdateResultTable(result, pattern.synonym);
                            return;
                        }

                        foreach (var i in listIf)
                        {
                            if (i.firstChild.indexOfName == pkb.GetVarIndex(pattern.arg1.Replace("\"", ""))
                                && !result.Contains(i))
                            {
                                result.Add(i);
                            }
                        }

                        UpdateResultTable(result, pattern.synonym);
                        return;

                    }
                    break;
            }
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

        private bool AssignRightContainsPatter(TNode assign, string arg2)
        {
            string pattern = arg2.Replace("\"", "").Trim();
            string rightSide = assign.info.Trim();

            if (pattern.StartsWith("_") && pattern.EndsWith("_"))
            {
                return rightSide.Contains(pattern.Replace("_", ""));
            }
            else if (pattern.StartsWith("_"))
            {
                return rightSide.EndsWith(pattern.Replace("_", ""));
            }
            else if (pattern.EndsWith("_"))
            {
                return rightSide.StartsWith(pattern.Replace("_", ""));
            }
            else
            {
                return rightSide == pattern;
            }
        }

        private void Modifies(Relation relation)
        {
            List<TNode> candidates, candidates2;
            List<TNode> resultList = new List<TNode>();
            List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

            if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                if (astManager.GetAllAssigns() != null) { UpdateResultTable(true); }
                return;
            }

            if (relation.arg1type == Entity._int)
            {
                TNode tnode = astManager.FindNode(Int32.Parse(relation.arg1));
                candidates = new List<TNode>();
                candidates.Add(tnode);
                if (tnode.type != Enums.TNodeTypeEnum.Assign)
                {
                    candidates = astManager.GetAllAssignUnder(tnode, Enum.GetName(typeof(Enums.TNodeTypeEnum), tnode.type));
                }

                if (candidates is null || (candidates != null && !candidates.Any())) { UpdateResultTable(false); return; }

                if (relation.arg2type == Entity._)
                {
                    UpdateResultTable(true);
                    return;
                }

                if (relation.arg2type == Entity._string)
                {
                    foreach (var a in candidates)
                    {
                        if (a.firstChild.indexOfName == pkb.GetVarIndex(relation.arg2.Trim('"'))) { UpdateResultTable(true); return; }
                    }

                    UpdateResultTable(false);
                    return;
                }

                if (relation.arg2type == Entity.variable)
                {
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                    {
                        candidates2 = queryResult.GetNodes(relation.arg2.Trim('"'));
                    }
                    else
                    {
                        candidates2 = astManager.GetNodes(Entity.variable);
                    }

                    if (candidates2 is null) { UpdateResultTable(null, relation.arg2); return; }

                    foreach (var a in candidates)
                    {
                        foreach (var v in candidates2)
                        {
                            if (v.indexOfName == a.firstChild.indexOfName && !resultList.Contains(v))
                            {
                                resultList.Add(v);
                            }
                        }
                    }
                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
            }




            if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
            {
                candidates = queryResult.GetNodes(relation.arg1);
            }
            else
            {
                candidates = astManager.GetNodes(relation.arg1type);
            }
            //if (candidates != null && relation.arg1type != Entity.assign) { candidates = astManager.GetAllAssignUnder(candidates, relation.arg1type); }


            if (candidates is null) { UpdateResultTable(null, relation.arg1); return; }

            if (relation.arg2type == Entity._)
            {
                foreach (var c in candidates)
                {
                    var assignsUnder = astManager.GetAllAssignUnder(c, relation.arg1type);
                    if (assignsUnder != null && assignsUnder.Any())
                    {
                        resultList.Add(c);
                    }
                }
                UpdateResultTable(resultList, relation.arg1);
                return;
            }

            if (relation.arg2type == Entity._string)
            {
                foreach (var c in candidates)
                {
                    var assignsUnder = astManager.GetAllAssignUnder(c, relation.arg1type);
                    foreach (var au in assignsUnder)
                    {
                        if (au.firstChild.indexOfName == pkb.GetVarIndex(relation.arg2.Trim('"')))
                        {
                            resultList.Add(c);
                            break;
                        }
                    }
                }

                UpdateResultTable(resultList, relation.arg1);
                return;
            }

            if (relation.arg2type == Entity.variable)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidates2 = queryResult.GetNodes(relation.arg2);
                }
                else
                {
                    candidates2 = astManager.GetNodes(relation.arg2type);
                }

                if (candidates2 is null) { UpdateResultTable(null, relation.arg1, relation.arg2); return; }

                foreach (var c in candidates)
                {
                    var assignsUnder = astManager.GetAllAssignUnder(c, relation.arg1type);
                    foreach (var au in assignsUnder)
                    {
                        foreach (var v in candidates2)
                        {
                            if (au.firstChild.indexOfName == v.indexOfName && !resultListTuple.Contains((c, v)))
                            {
                                resultListTuple.Add((c, v));
                            }
                        }
                    }


                }

                UpdateResultTable(resultListTuple, relation.arg1, relation.arg2);
                return;
            }
        }


        private void NextX(Relation relation)
        {
            List<TNode> candidateForFrom, candidateForTo;
            List<TNode> resultList = new List<TNode>();


            #region NextX(int, int), NextX(_, _)
            if (relation.arg1type == Entity._int && relation.arg2type == Entity._int)
            {
                bool result = cfgManager.IsNextX(Int32.Parse(relation.arg1), Int32.Parse(relation.arg2));

                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                var nodes = astManager.NodeWithLineNumberList;
                List<TNode> result = new List<TNode>();
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        result = cfgManager.NextX(node, relation.arg2);
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

                #region NexXt(int, _)
                if (relation.arg2type == Entity._)
                {
                    if (from is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = cfgManager.NextX(from, relation.arg2) != null ? true : false;
                        UpdateResultTable(result);
                        return;
                    }
                }
                #endregion

                #region NextX(int, *)
                if (from is null)
                {
                    UpdateResultTable(null, relation.arg2);
                    return;
                }
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = queryResult.GetNodes(relation.arg2);
                    if (candidateForTo != null)
                    {
                        foreach (var c in candidateForTo)
                        {
                            if (cfgManager.IsNextX(Int32.Parse(relation.arg1), (int)c.programLine))
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
                    List<TNode> tmp = cfgManager.NextX(from, relation.arg2);
                    if (tmp != null) { resultList = tmp; }
                    UpdateResultTable(resultList, relation.arg2);
                }
                #endregion
            }

            else if (relation.arg2type == Entity._int)
            {
                TNode to = astManager.FindNode(Int32.Parse(relation.arg2));

                #region Next(_, int)
                if (relation.arg1type == Entity._)
                {
                    if (to is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = cfgManager.PreviousX(to, relation.arg1);
                        UpdateResultTable(result != null ? true : false);
                        return;
                    }
                }
                #endregion

                #region Next(*, int)
                if (to is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = queryResult.GetNodes(relation.arg1);
                    if (candidateForFrom != null)
                    {
                        foreach (var c in candidateForFrom)
                        {
                            if (cfgManager.IsNextX((int)c.programLine, Int32.Parse(relation.arg2)))
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
                    List<TNode> tmp = cfgManager.PreviousX(to, relation.arg1);
                    if (tmp != null) { resultList = tmp; }
                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }
                #endregion
            }

            else
            {
                List<TNode> fromList = null;
                List<TNode> tmpResult = null;

                #region Next(_, *)
                if (relation.arg1type == Entity._)
                {
                    fromList = astManager.NodeWithLineNumberList;

                    if (fromList != null)
                    {
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForTo = queryResult.GetNodes(relation.arg2);
                            if (candidateForTo != null)
                            {
                                foreach (var from in fromList)
                                {
                                    foreach (var to in candidateForTo)
                                    {
                                        if (cfgManager.IsNextX((int)from.programLine, (int)to.programLine))
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
                                List<TNode> tmp = cfgManager.NextX(from, relation.arg2);
                                if (tmp != null) { resultList = tmp; }
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
                    #region NextX(*, _)
                    if (relation.arg2 == Entity._)
                    {
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFrom = queryResult.GetNodes(relation.arg1);
                        }
                        else
                        {
                            candidateForFrom = astManager.NodeWithLineNumberList;
                        }

                        if (candidateForFrom != null)
                        {
                            foreach (var from in candidateForFrom)
                            {
                                var hasRightSibling = cfgManager.NextX(from, relation.arg2) != null ? true : false;
                                if (hasRightSibling) { resultList.Add(from); }
                            }
                        }

                        UpdateResultTable(resultList, relation.arg1);
                        return;
                    }
                    #endregion
                }

                #region Next(*, *)
                List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

                //candidates for from
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = queryResult.GetNodes(relation.arg1);
                }
                else
                {
                    candidateForFrom = astManager.NodeWithLineNumberList;
                }

                //candidates for to
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = queryResult.GetNodes(relation.arg2);
                    if (candidateForTo != null)
                    {
                        for (int i = 0; i < candidateForFrom.Count(); i++)
                        {
                            if (cfgManager.IsNextX((int)candidateForFrom[i].programLine, (int)candidateForTo[i].programLine))
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
                        List<TNode> tmp = cfgManager.NextX(from, relation.arg2);
                        if (tmp != null)
                        {
                            foreach (var to in tmp)
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

        private void Next(Relation relation)
        {
            List<TNode> candidateForFrom, candidateForTo;
            List<TNode> resultList = new List<TNode>();


            #region Next(int, int), Next(_, _)
            if (relation.arg1type == Entity._int && relation.arg2type == Entity._int)
            {
                bool result = cfgManager.IsNext(Int32.Parse(relation.arg1), Int32.Parse(relation.arg2));

                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                var nodes = astManager.NodeWithLineNumberList;
                List<TNode> result = new List<TNode>();
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        result = cfgManager.Next(node, relation.arg2);
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

                #region Next(int, _)
                if (relation.arg2type == Entity._)
                {
                    if (from is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = cfgManager.Next(from, relation.arg2) != null ? true : false;
                        UpdateResultTable(result);
                        return;
                    }
                }
                #endregion

                #region Next(int, *)
                if (from is null)
                {
                    UpdateResultTable(null, relation.arg2);
                    return;
                }
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = queryResult.GetNodes(relation.arg2);
                    if (candidateForTo != null)
                    {
                        foreach (var c in candidateForTo)
                        {
                            if (cfgManager.IsNext(Int32.Parse(relation.arg1), (int)c.programLine))
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
                    List<TNode> tmp = cfgManager.Next(from, relation.arg2);
                    if (tmp != null) { resultList = tmp; }
                    UpdateResultTable(resultList, relation.arg2);
                }
                #endregion
            }

            else if (relation.arg2type == Entity._int)
            {
                TNode to = astManager.FindNode(Int32.Parse(relation.arg2));

                #region Next(_, int)
                if (relation.arg1type == Entity._)
                {
                    if (to is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = cfgManager.Previous(to, relation.arg1);
                        UpdateResultTable(result != null ? true : false);
                        return;
                    }
                }
                #endregion

                #region Next(*, int)
                if (to is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = queryResult.GetNodes(relation.arg1);
                    if (candidateForFrom != null)
                    {
                        foreach (var c in candidateForFrom)
                        {
                            if (cfgManager.IsNext((int)c.programLine, Int32.Parse(relation.arg2)))
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
                    List<TNode> tmp = cfgManager.Previous(to, relation.arg1);
                    if (tmp != null) { resultList = tmp; }
                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }
                #endregion
            }

            else
            {
                List<TNode> fromList = null;
                List<TNode> tmpResult = null;

                #region Next(_, *)
                if (relation.arg1type == Entity._)
                {
                    fromList = astManager.NodeWithLineNumberList;

                    if (fromList != null)
                    {
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForTo = queryResult.GetNodes(relation.arg2);
                            if (candidateForTo != null)
                            {
                                foreach (var from in fromList)
                                {
                                    foreach (var to in candidateForTo)
                                    {
                                        if (cfgManager.IsNext((int)from.programLine, (int)to.programLine))
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
                                List<TNode> tmp = cfgManager.Next(from, relation.arg2);
                                if (tmp != null) { resultList = tmp; }
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
                    #region Next(*, _)
                    if (relation.arg2 == Entity._)
                    {
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFrom = queryResult.GetNodes(relation.arg1);
                        }
                        else
                        {
                            candidateForFrom = astManager.NodeWithLineNumberList;
                        }

                        if (candidateForFrom != null)
                        {
                            foreach (var from in candidateForFrom)
                            {
                                var hasRightSibling = cfgManager.Next(from, relation.arg2) != null ? true : false;
                                if (hasRightSibling) { resultList.Add(from); }
                            }
                        }

                        UpdateResultTable(resultList, relation.arg1);
                        return;
                    }
                    #endregion
                }

                #region Next(*, *)
                List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

                //candidates for from
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = queryResult.GetNodes(relation.arg1);
                }
                else
                {
                    candidateForFrom = astManager.NodeWithLineNumberList;
                }

                //candidates for to
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = queryResult.GetNodes(relation.arg2);
                    if (candidateForTo != null)
                    {
                        for (int i = 0; i < candidateForFrom.Count(); i++)
                        {
                            if (cfgManager.IsNext((int)candidateForFrom[i].programLine, (int)candidateForTo[i].programLine))
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
                        List<TNode> tmp = cfgManager.Next(from, relation.arg2);
                        if (tmp != null)
                        {
                            foreach (var to in tmp)
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



        private void FolowsX(Relation relation)
        {
            List<TNode> candidateForFrom, candidateForTo;
            List<TNode> resultList = new List<TNode>();


            #region FollowsX(int, int), FollowsX(_, _)
            if (relation.arg1type == Entity._int && relation.arg2type == Entity._int)
            {
                bool result = astManager.IsFollowsX(Int32.Parse(relation.arg1), Int32.Parse(relation.arg2));

                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                var nodes = astManager.NodeWithLineNumberList;
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
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = queryResult.GetNodes(relation.arg2);
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
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = queryResult.GetNodes(relation.arg1);
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
                    fromList = astManager.NodeWithLineNumberList;

                    if (fromList != null)
                    {
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForTo = queryResult.GetNodes(relation.arg2);
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
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFrom = queryResult.GetNodes(relation.arg1);
                        }
                        else
                        {
                            candidateForFrom = astManager.NodeWithLineNumberList;
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
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = queryResult.GetNodes(relation.arg1);
                }
                else
                {
                    candidateForFrom = astManager.NodeWithLineNumberList;
                }

                //candidates for to
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = queryResult.GetNodes(relation.arg2);
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


            #region Follows(int, int), Follows(_, _)
            if (relation.arg1type == Entity._int && relation.arg2type == Entity._int)
            {
                bool result = astManager.IsFollows(Int32.Parse(relation.arg1), Int32.Parse(relation.arg2));

                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                var nodes = astManager.NodeWithLineNumberList;
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
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = queryResult.GetNodes(relation.arg2);
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
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = queryResult.GetNodes(relation.arg1);
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
                    fromList = astManager.NodeWithLineNumberList;

                    if (fromList != null)
                    {
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForTo = queryResult.GetNodes(relation.arg2);
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
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFrom = queryResult.GetNodes(relation.arg1);
                        }
                        else
                        {
                            candidateForFrom = astManager.NodeWithLineNumberList;
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
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFrom = queryResult.GetNodes(relation.arg1);
                }
                else
                {
                    candidateForFrom = astManager.NodeWithLineNumberList;
                }

                //candidates for to
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = queryResult.GetNodes(relation.arg2);
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
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForChildren = queryResult.GetNodes(relation.arg2);
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
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFather = queryResult.GetNodes(relation.arg1);
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
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForChildren = queryResult.GetNodes(relation.arg2);
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
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFather = queryResult.GetNodes(relation.arg1);
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
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFather = queryResult.GetNodes(relation.arg1);
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
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForChildren = queryResult.GetNodes(relation.arg2);
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
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForChildren = queryResult.GetNodes(relation.arg2);
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
                else if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFather = queryResult.GetNodes(relation.arg1);
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
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForChildren = queryResult.GetNodes(relation.arg2);
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
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFather = queryResult.GetNodes(relation.arg1);
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
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFather = queryResult.GetNodes(relation.arg1);
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
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForChildren = queryResult.GetNodes(relation.arg2);
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
                int indexOfDeclarationFirstArgument = queryResult.FindIndexOfDeclaration(firstArgument);
                int indexOfDeclarationSecondArgument = queryResult.FindIndexOfDeclaration(secondArgument);

                if (queryResult.HasRecords())
                {
                    var earlierResultRecords = queryResult.GetResultTableList();

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
                        newRecord = new TNode[queryResult.declarationsTable.Length];
                        newRecord[indexOfDeclarationFirstArgument] = res.Item1;
                        newRecord[indexOfDeclarationSecondArgument] = res.Item2;
                        newResultTableList.Add(newRecord);

                    }
                }

                queryResult.UpdateResutTableList(newResultTableList);
            }
            else
            {
                queryResult.ClearResultTableList();
                FinishQueryEvaluator();
            }

            queryResult.SetDeclarationWasDeterminated(firstArgument);
            queryResult.SetDeclarationWasDeterminated(secondArgument);
        }

        private void UpdateResultTable(List<TNode> resultList, string argumentLookingFor)
        {
            if (resultList != null && resultList.Any())
            {
                List<TNode[]> newResultTableList = new List<TNode[]>();
                int indexOfDeclaration = queryResult.FindIndexOfDeclaration(argumentLookingFor);

                if (queryResult.HasRecords())
                {
                    var earlierResultRecords = queryResult.GetResultTableList();

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
                        newRecord = new TNode[queryResult.declarationsTable.Length];
                        newRecord[indexOfDeclaration] = res;
                        newResultTableList.Add(newRecord);
                    }
                }

                queryResult.UpdateResutTableList(newResultTableList);
            }
            else
            {
                queryResult.ClearResultTableList();
                FinishQueryEvaluator();
            }

            queryResult.SetDeclarationWasDeterminated(argumentLookingFor);
        }

        private void UpdateResultTable(bool p_result)
        {
            if (queryResult.resultIsBoolean)
            {
                queryResult.resultBoolean = p_result;
            }

            if (p_result is false)
            {
                queryResult.ClearResultTableList();
                FinishQueryEvaluator();
            }
        }

        private void FinishQueryEvaluator()
        {
            if (queryResult.resultIsBoolean && queryResult.resultBoolean is null) // sytuacja, kiedy zapytanie jest typu boolean ale relacje zwracaly wartosci do resultTable
            {
                queryResult.resultBoolean = queryResult.resultTableList.Count > 0 ? true : false;
            }
            throw new NoResultsException("Condition " + actualCondition.ToString() + " has no results.");
        }

        private void HandleBooleanReturn()
        {
            if (queryResult.resultIsBoolean && queryResult.resultBoolean is null) // sytuacja, kiedy zapytanie jest typu boolean ale relacje zwracaly wartosci do resultTable
            {
                queryResult.resultBoolean = queryResult.resultTableList.Count > 0 ? true : false;
            }
        }
    }
}
