using SpaWpfApp.Ast;
using SpaWpfApp.Cfg;
using SpaWpfApp.Enums;
using SpaWpfApp.Exceptions;
using SpaWpfApp.PkbNew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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

            List<Condition> rpList = new List<Condition>();
            List<With> wList = new List<With>();
            List<With> w2synonymList = new List<With>();

            foreach (var condition in conditionsList)
            {
                if (condition is With)
                {
                    wList.Add((With)condition);
                    if (((With)condition).leftType != Entity._int && ((With)condition).leftType != Entity._string &&
                        ((With)condition).rightType != Entity._int && ((With)condition).rightType != Entity._string)
                    {
                        w2synonymList.Add((With)condition);
                    }
                }
                else { rpList.Add(condition); }
            }

            if (w2synonymList.Any())
            {
                foreach (var w2synonym in w2synonymList)
                {
                    wList.Remove(w2synonym);
                    wList.Add(w2synonym);
                }
            }

            conditionsList.Clear();
            foreach (var rp in rpList)
            {
                conditionsList.Add(rp);
            }
            foreach (var w2 in wList)
            {
                conditionsList.Add(w2);
            }

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

                        case Relation.Uses:
                            Uses(relation);
                            break;

                        case Relation.Calls:
                            Calls(relation);
                            break;
                        case Relation.CallsX:
                            CallsX(relation);
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



        private void CallsX(Relation relation)
        {
            List<TNode> candidates, candidates2;
            List<TNode> resultList = new List<TNode>();
            List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();


            if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                bool result = astManager.GetNodes(Entity.call) is null ? false : true;

                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._ && relation.arg2type == Entity._string)
            {
                var calls = astManager.GetNodes(Entity.call);
                if (calls != null)
                {
                    foreach (var c in calls)
                    {
                        if (c.indexOfName == pkb.GetProcIndex(relation.arg2.Trim('"')))
                        {
                            UpdateResultTable(true);
                            return;
                        }
                    }
                }
                UpdateResultTable(false);
                return;
            }
            else if (relation.arg1type == Entity._ && relation.arg2type == Entity.procedure)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                {
                    candidates2 = DeepCopy(queryResult.GetNodes(relation.arg2.Trim('"')));
                }
                else
                {
                    candidates2 = DeepCopy(astManager.GetNodes(Entity.procedure));
                }

                var calls = astManager.GetNodes(Entity.call);
                if (calls != null)
                {
                    foreach (var c in calls)
                    {
                        foreach (var can in candidates2)
                        {
                            if (c.indexOfName == can.indexOfName && !resultList.Contains(can))
                            {
                                resultList.Add(can);
                            }
                        }
                    }
                }

                UpdateResultTable(resultList, relation.arg2);
            }
            else if (relation.arg1type == Entity._string && relation.arg2type == Entity._)
            {
                Boolean result = pkb.GetCalled(relation.arg1.Trim('"')).Any();
                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._string && relation.arg2type == Entity._string)
            {
                Boolean result = false;
                CallsXCheckIfCalls(relation.arg1.Trim('"'), relation.arg2.Trim('"'), ref result);

                UpdateResultTable(result);
                return;
            }
            else if (relation.arg1type == Entity._string && relation.arg2type == Entity.procedure)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                {
                    candidates2 = DeepCopy(queryResult.GetNodes(relation.arg2.Trim('"')));
                }
                else
                {
                    candidates2 = DeepCopy(astManager.GetNodes(Entity.procedure));
                }

                if (candidates2 is null)
                {
                    UpdateResultTable(null, relation.arg2);
                    return;
                }

                Boolean result;
                foreach (var p in candidates2)
                {
                    result = false;
                    CallsXCheckIfCalls(relation.arg1.Trim('"'), pkb.GetProcName((int)p.indexOfName), ref result);
                    if (result && !resultList.Contains(p))
                    {
                        resultList.Add(p);
                    }
                }
                UpdateResultTable(resultList, relation.arg2);
            }
            else if (relation.arg1type == Entity.procedure && relation.arg2type == Entity._)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1.Trim('"')))
                {
                    candidates = DeepCopy(queryResult.GetNodes(relation.arg1.Trim('"')));
                }
                else
                {
                    candidates = DeepCopy(astManager.GetNodes(Entity.procedure));
                }

                if (candidates is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }

                foreach (var c in candidates)
                {
                    if (pkb.GetCalled(pkb.GetProcName((int)c.indexOfName)).Any() && !resultList.Contains(c))
                    {
                        resultList.Add(c);
                    }
                }

                UpdateResultTable(resultList, relation.arg1);
                return;
            }
            else if (relation.arg1type == Entity.procedure && relation.arg2type == Entity._string)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1.Trim('"')))
                {
                    candidates = DeepCopy(queryResult.GetNodes(relation.arg1.Trim('"')));
                }
                else
                {
                    candidates = DeepCopy(astManager.GetNodes(Entity.procedure));
                }

                if (candidates is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }

                Boolean result;
                foreach (var c in candidates)
                {
                    result = false;
                    CallsXCheckIfCalls(pkb.GetProcName((int)c.indexOfName), relation.arg2.Trim('"'), ref result);
                    if (result && !resultList.Contains(c))
                    {
                        resultList.Add(c);
                    }
                }

                UpdateResultTable(resultList, relation.arg1);
                return;
            }
            else if (relation.arg1type == Entity.procedure && relation.arg2type == Entity.procedure)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1.Trim('"')))
                {
                    candidates = DeepCopy(queryResult.GetNodes(relation.arg1.Trim('"')));
                }
                else
                {
                    candidates = DeepCopy(astManager.GetNodes(Entity.procedure));
                }


                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                {
                    candidates2 = DeepCopy(queryResult.GetNodes(relation.arg2.Trim('"')));
                }
                else
                {
                    candidates2 = DeepCopy(astManager.GetNodes(Entity.procedure));
                }

                if (candidates is null || candidates2 is null)
                {
                    UpdateResultTable(null, relation.arg1, relation.arg2);
                    return;
                }

                Boolean result;
                foreach (var c1 in candidates)
                {
                    foreach (var c2 in candidates2)
                    {
                        result = false;
                        CallsXCheckIfCalls(pkb.GetProcName((int)c1.indexOfName), pkb.GetProcName((int)c2.indexOfName), ref result);
                        if (result && !resultListTuple.Contains((c1, c2)))
                        {
                            resultListTuple.Add((c1, c2));
                        }
                    }
                }

                UpdateResultTable(resultListTuple, relation.arg1, relation.arg2);
                return;
            }
        }

        private void CallsXCheckIfCalls(string name1, string name2, ref bool result)
        {
            var calls = pkb.GetCalled(name1);

            foreach (var c in calls)
            {
                if (c == name2)
                {
                    result = true;
                    return;
                }

                CallsXCheckIfCalls(c, name2, ref result);
            }
        }

        private void CheckIfCallsX(string p1, string p2, ref List<(TNode, TNode)> resultListTuple, TNode c1, TNode c2)
        {
            if (pkb.IsCalls(p1, p2) != -1 && !resultListTuple.Contains((c1, c2)))
            {
                resultListTuple.Add((c1, c2));
                return;
            }

            foreach (var v in pkb.GetCalled(p1))
            {
                CheckIfCallsX(v, p2, ref resultListTuple, c1, c2);
            }
        }

        private void CheckIfCallsX(string p1, string p2, ref List<TNode> resultList, TNode can)
        {
            if (pkb.IsCalls(p1, p2) != -1 && !resultList.Contains(can))
            {
                resultList.Add(can);
                return;
            }

            foreach (var v in pkb.GetCalled(p1))
            {
                CheckIfCallsX(v, p2, ref resultList, can);
            }
        }

        private void CheckIfCallsX(string p1, string p2, ref Boolean result)
        {
            if (pkb.IsCalls(p1, p2) != -1)
            {
                result = true;
                return;
            }

            foreach (var v in pkb.GetCalled(p1))
            {
                CheckIfCallsX(v, p2, ref result);
            }
        }

        private void Calls(Relation relation)
        {
            List<TNode> candidates, candidates2;
            List<TNode> resultList = new List<TNode>();
            List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

            #region Calls(_, )
            if (relation.arg1type == Entity._)
            {
                if (relation.arg2type == Entity._)
                {
                    bool result = astManager.GetNodes(Entity.call) is null ? false : true;
                    UpdateResultTable(result);
                    return;
                }

                if (relation.arg2type == Entity._string)
                {
                    var calls = astManager.GetNodes(Entity.call);
                    if (calls is null) { UpdateResultTable(false); return; }

                    foreach (var c in calls)
                    {
                        if (c.indexOfName == pkb.GetProcIndex(relation.arg2.Trim('"')))
                        {
                            UpdateResultTable(true);
                            return;
                        }
                    }

                    UpdateResultTable(false);
                    return;
                }

                if (relation.arg2type == Entity.procedure)
                {
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                    {
                        candidates2 = DeepCopy(queryResult.GetNodes(relation.arg2.Trim('"')));
                    }
                    else
                    {
                        candidates2 = DeepCopy(astManager.GetNodes(Entity.procedure));
                    }

                    var calls = astManager.GetNodes(Entity.call);
                    if (calls is null || candidates2 is null) { UpdateResultTable(null, relation.arg2); return; }

                    foreach (var c in calls)
                        foreach (var p in candidates2)
                        {
                            if (c.indexOfName == p.indexOfName)
                            {
                                resultList.Add(p);
                            }
                        }

                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
            }
            #endregion

            #region Calls(string, )
            if (relation.arg1type == Entity._string)
            {
                var indexOfProcedure = pkb.GetProcIndex(relation.arg1.Trim('"'));
                TNode procedure = null;
                if (indexOfProcedure != -1)
                {
                    procedure = astManager.GetNodes(Entity.procedure).Where(p => p.indexOfName == indexOfProcedure).FirstOrDefault();
                }


                if (relation.arg2type == Entity._)
                {
                    if (procedure is null) { UpdateResultTable(false); return; }

                    if (indexOfProcedure != -1)
                    {
                        var proc = pkb.GetCalled(relation.arg1.Trim('"'));
                        if (proc.Any())
                        {
                            UpdateResultTable(true);
                            return;
                        }
                    }

                    UpdateResultTable(false);
                    return;
                }

                if (relation.arg2type == Entity._string)
                {
                    if (pkb.IsCalls(relation.arg1, relation.arg2) != -1)
                    {
                        UpdateResultTable(true);
                        return;
                    }

                    UpdateResultTable(false);
                    return;
                }

                if (relation.arg2type == Entity.procedure)
                {
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                    {
                        candidates2 = DeepCopy(queryResult.GetNodes(relation.arg2.Trim('"')));
                    }
                    else
                    {
                        candidates2 = DeepCopy(astManager.GetNodes(Entity.procedure));
                    }

                    if (procedure is null || candidates2 is null) { UpdateResultTable(null, relation.arg2); return; }

                    foreach (var p in candidates2)
                    {
                        if (pkb.IsCalls(relation.arg1.Trim('"'), pkb.GetProcName((int)p.indexOfName)) != -1)
                        {
                            resultList.Add(p);
                        }
                    }

                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
            }
            #endregion

            #region Calls(p, )
            if (relation.arg1type == Entity.procedure)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1.Trim('"')))
                {
                    candidates = DeepCopy(queryResult.GetNodes(relation.arg1.Trim('"')));
                }
                else
                {
                    candidates = DeepCopy(astManager.GetNodes(Entity.procedure));
                }


                if (relation.arg2type == Entity._)
                {
                    if (candidates is null) { UpdateResultTable(null, relation.arg1); return; }

                    foreach (var p in candidates)
                    {
                        var calls = pkb.GetCalled(pkb.GetProcName((int)p.indexOfName));
                        if (calls.Any())
                        {
                            resultList.Add(p);
                        }
                    }

                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }

                if (relation.arg2type == Entity._string)
                {
                    if (candidates is null) { UpdateResultTable(null, relation.arg1); return; }

                    foreach (var p in candidates)
                    {
                        var calls = pkb.GetCalled(pkb.GetProcName((int)p.indexOfName));
                        foreach (var c in calls)
                        {
                            if (c == relation.arg2.Trim('"'))
                            {
                                resultList.Add(p);
                            }
                        }
                    }

                    UpdateResultTable(resultList, relation.arg1);
                    return;
                }

                if (relation.arg2type == Entity.procedure)
                {
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                    {
                        candidates2 = DeepCopy(queryResult.GetNodes(relation.arg2.Trim('"')));
                    }
                    else
                    {
                        candidates2 = DeepCopy(astManager.GetNodes(Entity.procedure));
                    }

                    if (candidates is null || candidates2 is null) { UpdateResultTable(null, relation.arg1, relation.arg2); return; }

                    foreach (var p in candidates)
                        foreach (var p2 in candidates2)
                        {
                            if (pkb.IsCalls(pkb.GetProcName((int)p.indexOfName), pkb.GetProcName((int)p2.indexOfName)) != -1 && !resultListTuple.Contains((p, p2)))
                            {
                                resultListTuple.Add((p, p2));
                            }
                        }

                    UpdateResultTable(resultListTuple, relation.arg1, relation.arg2);
                    return;
                }
            }
            #endregion            
        }





        private void Uses(Relation relation)
        {
            List<TNode> candidates, candidates2;
            List<TNode> resultList = new List<TNode>();
            List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

            #region Uses(_, )
            if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                var allIfWhileAssigns = astManager.GetAllWhileIfAsigns();
                if (allIfWhileAssigns != null)
                {
                    foreach (var aiw in allIfWhileAssigns)
                    {
                        switch (aiw.type)
                        {
                            case TNodeTypeEnum.If:
                            case TNodeTypeEnum.While:
                                UpdateResultTable(true);
                                return;
                            case TNodeTypeEnum.Assign:
                                if (Regex.IsMatch(aiw.info, @"[a-zA-Z]"))
                                {
                                    UpdateResultTable(true);
                                    return;
                                }
                                break;
                        }
                    }
                }
                UpdateResultTable(false);
                return;
            }

            if (relation.arg1type == Entity._ && relation.arg2type == Entity.variable)
            {
                var allIfWhileAssigns = astManager.GetAllWhileIfAsigns();

                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidates = queryResult.GetNodes(relation.arg2);
                }
                else
                {
                    candidates = astManager.GetNodes(relation.arg2type);
                }

                if (allIfWhileAssigns != null && candidates != null)
                {
                    foreach (var aiw in allIfWhileAssigns)
                        foreach (var c in candidates)
                        {
                            switch (aiw.type)
                            {
                                case TNodeTypeEnum.If:
                                case TNodeTypeEnum.While:
                                    if (aiw.firstChild.indexOfName == c.indexOfName && !resultList.Contains(c))
                                    {
                                        resultList.Add(c);
                                        break;
                                    }
                                    break;
                                case TNodeTypeEnum.Assign:
                                    if (CInfoContainsVarable(aiw.info, pkb.GetVarName((int)c.indexOfName)) && !resultList.Contains(c))
                                    {
                                        resultList.Add(c);
                                        break;
                                    }
                                    break;
                            }
                        }
                }

                UpdateResultTable(resultList, relation.arg2);
                return;
            }

            if (relation.arg1type == Entity._ && relation.arg2type == Entity._string)
            {
                var allIfWhileAssigns = astManager.GetAllWhileIfAsigns();

                if (allIfWhileAssigns != null)
                {
                    foreach (var aiw in allIfWhileAssigns)
                    {
                        switch (aiw.type)
                        {
                            case TNodeTypeEnum.If:
                            case TNodeTypeEnum.While:
                                if (aiw.firstChild.indexOfName == pkb.GetVarIndex(relation.arg2.Trim('"')))
                                {
                                    UpdateResultTable(true);
                                    return;
                                }
                                break;
                            case TNodeTypeEnum.Assign:
                                if (CInfoContainsVarable(aiw.info, relation.arg2.Trim('"')))
                                {
                                    UpdateResultTable(true);
                                    return;
                                }
                                break;
                        }
                    }
                }

                UpdateResultTable(false);
                return;
            }
            #endregion

            #region Uses(int, )
            if (relation.arg1type == Entity._int)
            {
                TNode tnode = astManager.FindNode(Int32.Parse(relation.arg1));
                candidates = new List<TNode>();
                candidates = astManager.GetAllWhileIfAssignsUnder(tnode);


                if (relation.arg2type == Entity._)
                {
                    if (candidates is null) { UpdateResultTable(false); return; }

                    foreach (var c in candidates)
                    {
                        switch (c.type)
                        {
                            case TNodeTypeEnum.If:
                            case TNodeTypeEnum.While:
                                UpdateResultTable(true);
                                return;

                            case TNodeTypeEnum.Assign:
                                if (Regex.IsMatch(c.info, @"[a-zA-Z]"))
                                {
                                    UpdateResultTable(true);
                                    return;
                                }
                                break;
                        }
                    }
                    UpdateResultTable(false);
                    return;
                }

                if (relation.arg2type == Entity._string)
                {

                    if (candidates is null) { UpdateResultTable(false); return; }

                    foreach (var c in candidates)
                    {
                        switch (c.type)
                        {
                            case TNodeTypeEnum.If:
                            case TNodeTypeEnum.While:
                                if (c.firstChild.indexOfName == pkb.GetVarIndex(relation.arg2.Trim('"')))
                                {
                                    UpdateResultTable(true);
                                    return;
                                }
                                break;
                            case TNodeTypeEnum.Assign:
                                if (CInfoContainsVarable(c.info, relation.arg2.Trim('"')))
                                {
                                    UpdateResultTable(true);
                                    return;
                                }
                                break;
                        }
                    }
                    UpdateResultTable(false);
                    return;
                }

                if (relation.arg2type == Entity.variable)
                {
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                    {
                        candidates2 = queryResult.GetNodes(relation.arg2);
                    }
                    else
                    {
                        candidates2 = astManager.GetNodes(Entity.variable);
                    }

                    if (candidates is null || candidates2 is null) { UpdateResultTable(null, relation.arg2); return; }

                    foreach (var a in candidates) // assignsIfWhile
                    {
                        foreach (var v in candidates2) // variables
                        {
                            switch (a.type)
                            {
                                case TNodeTypeEnum.If:
                                case TNodeTypeEnum.While:
                                    if (a.firstChild.indexOfName == v.indexOfName && !resultList.Contains(v))
                                    {
                                        resultList.Add(v);
                                    }
                                    break;
                                case TNodeTypeEnum.Assign:
                                    if (CInfoContainsVarable(a.info, pkb.GetVarName((int)v.indexOfName)) && !resultList.Contains(v))
                                    {
                                        resultList.Add(v);
                                    }
                                    break;
                            }
                        }
                    }
                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
            }
            #endregion

            #region Uses(string, )
            if (relation.arg1type == Entity._string)
            {
                var indexOfProcedure = pkb.GetProcIndex(relation.arg1.Trim('"'));
                TNode procedure = null;
                if (indexOfProcedure != -1)
                {
                    procedure = astManager.GetNodes(Entity.procedure).Where(p => p.indexOfName == indexOfProcedure).FirstOrDefault();
                }

                if (relation.arg2type == Entity._)
                {
                    if (procedure != null)
                    {
                        var allWhileIfAssigns = astManager.GetAllWhileIfAssignsUnder(procedure);
                        foreach (var aiw in allWhileIfAssigns)
                        {
                            switch (aiw.type)
                            {
                                case TNodeTypeEnum.If:
                                case TNodeTypeEnum.While:
                                    UpdateResultTable(true);
                                    return;
                                case TNodeTypeEnum.Assign:
                                    if (Regex.IsMatch(aiw.info, @"[a-zA-Z]"))
                                    {
                                        UpdateResultTable(true);
                                        return;
                                    }
                                    break;
                            }
                        }
                    }
                    UpdateResultTable(false);
                    return;
                }

                if (relation.arg2type == Entity._string)
                {
                    if (procedure != null)
                    {
                        var allAssignsIfWhile = astManager.GetAllWhileIfAssignsUnder(procedure);
                        if (allAssignsIfWhile != null)
                        {
                            foreach (var aiw in allAssignsIfWhile)
                            {
                                switch (aiw.type)
                                {
                                    case TNodeTypeEnum.If:
                                    case TNodeTypeEnum.While:
                                        if (aiw.firstChild.indexOfName == pkb.GetVarIndex(relation.arg2.Trim('"')))
                                        {
                                            UpdateResultTable(true);
                                            return;
                                        }
                                        break;
                                    case TNodeTypeEnum.Assign:
                                        if (CInfoContainsVarable(aiw.info, relation.arg2.Trim('"')))
                                        {
                                            UpdateResultTable(true);
                                            return;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    UpdateResultTable(false);
                    return;
                }

                if (relation.arg2type == Entity.variable)
                {
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                    {
                        candidates2 = queryResult.GetNodes(relation.arg2);
                    }
                    else
                    {
                        candidates2 = astManager.GetNodes(Entity.variable);
                    }

                    if (candidates2 is null || procedure is null) { UpdateResultTable(null, relation.arg2); return; }


                    var allIfWhileAssigns = astManager.GetAllWhileIfAssignsUnder(procedure);
                    if (allIfWhileAssigns != null)
                    {
                        foreach (var aiw in allIfWhileAssigns)
                            foreach (var c in candidates2)
                            {
                                switch (aiw.type)
                                {
                                    case TNodeTypeEnum.If:
                                    case TNodeTypeEnum.While:
                                        if (aiw.firstChild.indexOfName == c.indexOfName && !resultList.Contains(c))
                                        {
                                            resultList.Add(c);
                                            break;
                                        }
                                        break;
                                    case TNodeTypeEnum.Assign:
                                        if (CInfoContainsVarable(aiw.info, pkb.GetVarName((int)c.indexOfName)) && !resultList.Contains(c))
                                        {
                                            resultList.Add(c);
                                            break;
                                        }
                                        break;
                                }
                            }
                    }

                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
            }
            #endregion

            #region Uses(*, )
            if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
            {
                candidates = queryResult.GetNodes(relation.arg1);
            }
            else
            {
                candidates = astManager.GetNodes(relation.arg1type);
            }

            if (relation.arg2type == Entity._)
            {
                if (candidates is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }

                foreach (var c in candidates)
                {
                    var allAssignsWhileIf = astManager.GetAllWhileIfAssignsUnder(c);
                    if (allAssignsWhileIf != null)
                    {
                        foreach (var aiw in allAssignsWhileIf)
                        {
                            switch (aiw.type)
                            {
                                case TNodeTypeEnum.If:
                                case TNodeTypeEnum.While:
                                    if (!resultList.Contains(c)) { resultList.Add(c); break; }
                                    break;
                                case TNodeTypeEnum.Assign:
                                    if (Regex.IsMatch(aiw.info, @"[a-zA-Z]") && !resultList.Contains(c))
                                    {
                                        resultList.Add(c);
                                        break;
                                    }
                                    break;
                            }
                        }
                    }
                }
                UpdateResultTable(resultList, relation.arg1);
                return;
            }

            if (relation.arg2type == Entity._string)
            {
                if (candidates is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }
                else
                {
                    foreach (var c in candidates)
                    {
                        var allIfWhileAssigns = astManager.GetAllWhileIfAssignsUnder(c);
                        if (allIfWhileAssigns != null)
                        {
                            foreach (var aiw in allIfWhileAssigns)
                            {
                                switch (aiw.type)
                                {
                                    case TNodeTypeEnum.If:
                                    case TNodeTypeEnum.While:
                                        if (aiw.firstChild.indexOfName == pkb.GetVarIndex(relation.arg2.Trim('"')) && !resultList.Contains(c))
                                        {
                                            resultList.Add(c);
                                            break;
                                        }
                                        break;
                                    case TNodeTypeEnum.Assign:
                                        if (CInfoContainsVarable(aiw.info, relation.arg2.Trim('"')) && !resultList.Contains(c))
                                        {
                                            resultList.Add(c);
                                            break;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                UpdateResultTable(resultList, relation.arg1);
                return;
            }

            if (relation.arg2type == Entity.variable)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                {
                    candidates2 = queryResult.GetNodes(relation.arg2);
                }
                else
                {
                    candidates2 = astManager.GetNodes(Entity.variable);
                }

                if (candidates2 is null || candidates is null) { UpdateResultTable(null, relation.arg1, relation.arg2); return; }

                foreach (var c in candidates)
                {
                    var allIfWhileAssigns = astManager.GetAllWhileIfAssignsUnder(c);
                    if (allIfWhileAssigns != null)
                    {
                        foreach (var aiw in allIfWhileAssigns)
                            foreach (var c2 in candidates2)
                            {
                                switch (aiw.type)
                                {
                                    case TNodeTypeEnum.If:
                                    case TNodeTypeEnum.While:
                                        if (aiw.firstChild.indexOfName == c2.indexOfName && !resultListTuple.Contains((c, c2)))
                                        {
                                            resultListTuple.Add((c, c2));
                                            break;
                                        }
                                        break;
                                    case TNodeTypeEnum.Assign:
                                        if (CInfoContainsVarable(aiw.info, pkb.GetVarName((int)c2.indexOfName)) && !resultListTuple.Contains((c, c2)))
                                        {
                                            resultListTuple.Add((c, c2));
                                            break;
                                        }
                                        break;
                                }
                            }
                    }
                }

                UpdateResultTable(resultListTuple, relation.arg1, relation.arg2);
                return;
            }
            #endregion
        }
        private bool CInfoContainsVarable(string info, string arg2)
        {
            arg2 = arg2.Trim('"');
            if (info.Contains(arg2))
            {
                int indexOfStart = info.IndexOf(arg2);
                int indexOfEnd = indexOfStart + arg2.Length - 1;

                if (indexOfStart != 0)
                {
                    if (info[indexOfStart - 1] != 42 && info[indexOfStart - 1] != 43 && info[indexOfStart - 1] != 45)
                    {
                        return false;
                    }
                }

                if (indexOfEnd != info.Length - 1)
                {
                    if (info[indexOfEnd + 1] != 42 && info[indexOfEnd + 1] != 43 && info[indexOfEnd + 1] != 45)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private void DoWith(With with)
        {
            List<TNode> candidates, candidates2;
            List<TNode> resultList = new List<TNode>();
            List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

            if (with.leftType == Entity._int)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(with.right))
                {
                    candidates = DeepCopy(queryResult.GetNodes(with.right));
                }
                else
                {
                    candidates = DeepCopy(astManager.GetNodes(with.rightType));
                }

                foreach (var c in candidates)
                {
                    if (TheSame(c, Int32.Parse(with.left)) && !resultList.Contains(c))
                    {
                        resultList.Add(c);
                    }
                }

                UpdateResultTable(resultList, with.right);
                return;
            }

            else if (with.rightType == Entity._int)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(with.left))
                {
                    candidates = DeepCopy(queryResult.GetNodes(with.left));
                }
                else
                {
                    candidates = DeepCopy(astManager.GetNodes(with.leftType));
                }

                if (candidates != null)
                {
                    foreach (var c in candidates)
                    {
                        if (TheSame(c, Int32.Parse(with.right)) && !resultList.Contains(c))
                        {
                            resultList.Add(c);
                        }
                    }
                }

                UpdateResultTable(resultList, with.left);
                return;
            }

            else if (with.leftType == Entity._string)
            {

                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(with.right))
                {
                    candidates = DeepCopy(queryResult.GetNodes(with.right));
                }
                else
                {
                    candidates = DeepCopy(astManager.GetNodes(with.rightType));
                }

                if (candidates is null)
                {
                    UpdateResultTable(null, with.right);
                    return;
                }

                string rt = with.rightType;
                RemoveDotFromNameIfItIsAttrRef(ref rt);
                switch (rt)
                {
                    case Entity.procedure:
                    case Entity.call:
                        foreach (var c in candidates)
                        {
                            if (pkb.GetProcName((int)c.indexOfName) == with.left.Trim('"') && !resultList.Contains(c))
                            {
                                resultList.Add(c);
                            }
                        }
                        break;
                    case Entity.variable:
                        foreach (var c in candidates)
                        {
                            if (pkb.GetVarName((int)c.indexOfName) == with.left.Trim('"') && !resultList.Contains(c))
                            {
                                resultList.Add(c);
                            }
                        }
                        break;
                }

                UpdateResultTable(resultList, with.right);
                return;
            }

            else if (with.rightType == Entity._string)
            {

                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(with.left))
                {
                    candidates = queryResult.GetNodes(with.left);
                }
                else
                {
                    candidates = astManager.GetNodes(with.leftType);
                }

                if (candidates is null)
                {
                    UpdateResultTable(null, with.left);
                    return;
                }

                string lt = with.leftType;
                RemoveDotFromNameIfItIsAttrRef(ref lt);
                switch (lt)
                {
                    case Entity.procedure:
                    case Entity.call:
                        foreach (var c in candidates)
                        {
                            if (pkb.GetProcName((int)c.indexOfName) == with.right.Trim('"') && !resultList.Contains(c))
                            {
                                resultList.Add(c);
                            }
                        }
                        break;
                    case Entity.variable:
                        foreach (var c in candidates)
                        {
                            if (pkb.GetVarName((int)c.indexOfName) == with.right.Trim('"') && !resultList.Contains(c))
                            {
                                resultList.Add(c);
                            }
                        }
                        break;
                }

                UpdateResultTable(resultList, with.left);
                return;
            }

            else
            {
                List<(object, string)> leftCollection = new List<(object, string)>();
                List<(object, string)> rightCollection = new List<(object, string)>();

                #region determine candidates
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(with.left))
                {
                    candidates = queryResult.GetNodes(with.left);
                }
                else
                {
                    candidates = astManager.GetNodes(with.leftType);
                }

                if (candidates is null)
                {
                    UpdateResultTable(null, with.left);
                    return;
                }


                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(with.right))
                {
                    candidates2 = queryResult.GetNodes(with.right);
                }
                else
                {
                    candidates2 = astManager.GetNodes(with.rightType);
                }

                if (candidates2 is null)
                {
                    UpdateResultTable(null, with.right);
                    return;
                }
                #endregion

                #region determine objects from candidates
                string leftTypeX = with.leftType, rightTypeX = with.rightType;
                RemoveDotFromNameIfItIsAttrRef(ref leftTypeX);
                RemoveDotFromNameIfItIsAttrRef(ref rightTypeX);
                if (with.left.Contains('.') && leftTypeX == Entity.call || leftTypeX == Entity.procedure)
                {
                    foreach (var c in candidates)
                    {
                        leftCollection.Add((pkb.GetProcName((int)c.indexOfName), Entity._string));
                    }
                }
                else if (leftTypeX == Entity.variable)
                {
                    foreach (var c in candidates)
                    {
                        leftCollection.Add((pkb.GetVarName((int)c.indexOfName), Entity._string));
                    }
                }
                else if (leftTypeX == Entity.stmtLst)
                {
                    foreach (var c in candidates)
                    {
                        leftCollection.Add((c.programLine, "int"));
                    }
                }
                else if (leftTypeX == Entity.constant)
                {
                    foreach (var c in candidates)
                    {
                        leftCollection.Add((c.value, "int"));
                    }
                }
                else
                {
                    foreach (var c in candidates)
                    {
                        leftCollection.Add((c.programLine, "int"));
                    }
                }


                if (with.right.Contains('.') && rightTypeX == Entity.call || rightTypeX == Entity.procedure)
                {
                    foreach (var c in candidates2)
                    {
                        rightCollection.Add((pkb.GetProcName((int)c.indexOfName), Entity._string));
                    }
                }
                else if (rightTypeX == Entity.variable)
                {
                    foreach (var c in candidates2)
                    {
                        rightCollection.Add((pkb.GetVarName((int)c.indexOfName), Entity._string));
                    }
                }
                else if (rightTypeX == Entity.stmtLst)
                {
                    foreach (var c in candidates2)
                    {
                        rightCollection.Add((c.programLine, "int"));
                    }
                }
                else if (rightTypeX == Entity.constant)
                {
                    foreach (var c in candidates2)
                    {
                        rightCollection.Add((c.value, "int"));
                    }
                }
                else
                {
                    foreach (var c in candidates2)
                    {
                        rightCollection.Add((c.programLine, "int"));
                    }
                }
                #endregion

                for (int i = 0; i < candidates.Count; i++)
                    for (int j = 0; j < candidates2.Count; j++)
                    {
                        if (TheSame(leftCollection[i], rightCollection[j]) && !resultListTuple.Contains((candidates[i], candidates2[j])))
                        {
                            resultListTuple.Add((candidates[i], candidates2[j]));
                        }
                    }

                UpdateResultTable(resultListTuple, with.left, with.right);
                return;
            }

        }

        private bool TheSame(TNode c, int v)
        {
            switch (c.type)
            {
                case TNodeTypeEnum.Constant:
                    return c.value == v;
                default:
                    return c.programLine == v;
            }
        }

        private bool TheSame((object, string) p1, (object, string) p2)
        {
            switch (p1.Item2)
            {
                case "string":
                    return (string)p1.Item1 == (string)p2.Item1;
                case "int":
                    return (int)p1.Item1 == (int)p2.Item1;
            }
            return false;
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
                            listA = DeepCopy(queryResult.GetNodes(pattern.synonym));
                        }
                        else
                        {
                            listA = DeepCopy(astManager.GetAllAssigns());
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
                            if (i.firstChild.indexOfName == pkb.GetVarIndex(pattern.arg1.Trim('"'))
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

            return copy.Any() ? copy : null;
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

            #region Modifies(_, )
            if (relation.arg1type == Entity._ && relation.arg2type == Entity._)
            {
                if (astManager.GetAllAssigns() != null) { UpdateResultTable(true); }
                else { UpdateResultTable(false); }
                return;
            }

            if (relation.arg1type == Entity._ && relation.arg2type == Entity.variable)
            {
                var allAsigns = astManager.GetAllAssigns();
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidates = queryResult.GetNodes(relation.arg2);
                }
                else
                {
                    candidates = astManager.GetNodes(relation.arg2type);
                }
                if (allAsigns != null && candidates != null)
                {
                    foreach (var c in candidates)
                    {
                        foreach (var a in allAsigns)
                            if (a.firstChild.indexOfName == c.indexOfName && !resultList.Contains(c))
                            {
                                resultList.Add(c);
                                break;
                            }
                    }
                }

                UpdateResultTable(resultList, relation.arg2);
            }

            if (relation.arg1type == Entity._ && relation.arg2type == Entity._string)
            {
                var allAsigns = astManager.GetAllAssigns();
                if (allAsigns != null)
                {
                    int indexOfVariable = pkb.GetVarIndex(relation.arg2);
                    foreach (var a in allAsigns)
                    {
                        if (a.firstChild.indexOfName == indexOfVariable)
                        {
                            UpdateResultTable(true);
                            return;
                        }
                    }
                }

                UpdateResultTable(false);
            }
            #endregion


            #region Modifies(int, )
            if (relation.arg1type == Entity._int)
            {
                TNode tnode = astManager.FindNode(Int32.Parse(relation.arg1));
                candidates = new List<TNode>();
                candidates = astManager.GetAllAssignUnder(tnode);
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
                        candidates2 = queryResult.GetNodes(relation.arg2);
                    }
                    else
                    {
                        candidates2 = astManager.GetNodes(Entity.variable);
                    }

                    if (candidates2 is null) { UpdateResultTable(null, relation.arg2); return; }

                    foreach (var a in candidates) // assigns
                    {
                        foreach (var v in candidates2) // variables
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
            #endregion

            #region Modifies(string, )
            if (relation.arg1type == Entity._string)
            {
                var indexOfProcedure = pkb.GetProcIndex(relation.arg1.Trim('"'));
                TNode procedure = null;
                if (indexOfProcedure != -1)
                {
                    procedure = astManager.GetNodes(Entity.procedure).Where(p => p.indexOfName == indexOfProcedure).FirstOrDefault();
                }

                if (relation.arg2type == Entity._)
                {
                    if (procedure != null)
                    {
                        if (astManager.GetAllAssignUnder(procedure) != null)
                        {
                            UpdateResultTable(true);
                            return;
                        }

                    }
                    UpdateResultTable(false);
                    return;
                }

                if (relation.arg2type == Entity._string)
                {
                    if (procedure != null)
                    {
                        var allAssigns = astManager.GetAllAssignUnder(procedure);
                        foreach (var a in allAssigns)
                        {
                            if (a.firstChild.indexOfName == pkb.GetVarIndex(relation.arg2.Trim('"')))
                            {
                                UpdateResultTable(true);
                                break;
                            }
                        }
                    }
                    UpdateResultTable(false);
                    return;
                }

                if (relation.arg2type == Entity.variable)
                {
                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                    {
                        candidates2 = queryResult.GetNodes(relation.arg2);
                    }
                    else
                    {
                        candidates2 = astManager.GetNodes(Entity.variable);
                    }

                    if (candidates2 is null || procedure is null) { UpdateResultTable(null, relation.arg2); return; }


                    var allAssigns = astManager.GetAllAssignUnder(procedure);
                    foreach (var a in allAssigns)
                        foreach (var c2 in candidates2)
                        {
                            if (a.firstChild.indexOfName == c2.indexOfName && !resultList.Contains(c2))
                            {
                                resultList.Add(c2);
                            }
                        }

                    UpdateResultTable(resultList, relation.arg2);
                    return;
                }
            }
            #endregion

            #region Modifies(*, )
            if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
            {
                candidates = queryResult.GetNodes(relation.arg1);
            }
            else
            {
                candidates = astManager.GetNodes(relation.arg1type);
            }

            if (relation.arg2type == Entity._)
            {
                if (candidates is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }

                foreach (var c in candidates)
                {
                    var allAssigns = astManager.GetAllAssignUnder(c);
                    foreach (var a in allAssigns)
                    {
                        resultList.Add(c);
                        break;
                    }
                }
                UpdateResultTable(resultList, relation.arg1);
                return;
            }

            if (relation.arg2type == Entity._string)
            {
                if (candidates is null)
                {
                    UpdateResultTable(null, relation.arg1);
                    return;
                }
                else
                {
                    foreach (var c in candidates)
                    {
                        var allAssigns = astManager.GetAllAssignUnder(c);
                        foreach (var a in allAssigns)
                        {
                            if (a.firstChild.indexOfName == pkb.GetVarIndex(relation.arg2.Trim('"')) && !resultList.Contains(c))
                            {
                                resultList.Add(c);
                                break;
                            }
                        }
                    }
                }
                UpdateResultTable(resultList, relation.arg1);
                return;
            }

            if (relation.arg2type == Entity.variable)
            {
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2.Trim('"')))
                {
                    candidates2 = queryResult.GetNodes(relation.arg2);
                }
                else
                {
                    candidates2 = astManager.GetNodes(Entity.variable);
                }

                if (candidates2 is null || candidates is null) { UpdateResultTable(null, relation.arg1, relation.arg2); return; }

                foreach (var c in candidates)
                {
                    var allAssigns = astManager.GetAllAssignUnder(c);
                    foreach (var a in allAssigns)
                        foreach (var c2 in candidates2)
                        {
                            if (a.firstChild.indexOfName == c2.indexOfName && !resultListTuple.Contains((a, c2)))
                            {
                                resultListTuple.Add((c, c2));
                            }
                        }
                }

                UpdateResultTable(resultListTuple, relation.arg1, relation.arg2);
                return;
            }
            #endregion
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
                var nodes = astManager.GetAllNodesWithLineNumbers();
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
                            if (cfgManager.IsNextX(Int32.Parse(relation.arg1), (int)c.programLine) && !resultList.Contains(c))
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
                    if (tmp != null)
                    {
                        foreach (var tr in tmp)
                        {
                            resultList.Add(tr);
                        }
                    }
                    UpdateResultTable(resultList, relation.arg2);
                    return;
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
                            if (cfgManager.IsNextX((int)c.programLine, Int32.Parse(relation.arg2)) && !resultList.Contains(c))
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
                    if (tmp != null)
                    {
                        foreach (var tr in tmp)
                        {
                            resultList.Add(tr);
                        }
                    }
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
                    fromList = astManager.GetAllNodesWithLineNumbers();

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
                                if (tmp != null)
                                {
                                    foreach (var tr in tmp)
                                    {
                                        if (!resultList.Contains(tr))
                                        {
                                            resultList.Add(tr);
                                        }
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
                    #region NextX(*, _)
                    if (relation.arg2 == Entity._)
                    {
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFrom = queryResult.GetNodes(relation.arg1);
                        }
                        else
                        {
                            candidateForFrom = astManager.GetNodes(relation.arg1type);
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
                    candidateForFrom = astManager.GetNodes(relation.arg1type);
                }

                if (candidateForFrom is null)
                {
                    UpdateResultTable(null, relation.arg1, relation.arg2);
                    return;
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
                var nodes = astManager.GetAllNodesWithLineNumbers();
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
                            if (cfgManager.IsNext(Int32.Parse(relation.arg1), (int)c.programLine) && !resultList.Contains(c))
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
                            if (cfgManager.IsNext((int)c.programLine, Int32.Parse(relation.arg2)) && !resultList.Contains(c))
                            {
                                resultList.Add(c);
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
                    fromList = astManager.GetAllNodesWithLineNumbers();

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
                                if (tmp != null)
                                {
                                    foreach (var tr in tmp)
                                    {
                                        resultList.Add(tr);
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
                    #region Next(*, _)
                    if (relation.arg2 == Entity._)
                    {
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFrom = queryResult.GetNodes(relation.arg1);
                        }
                        else
                        {
                            candidateForFrom = astManager.GetNodes(relation.arg1type);
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
                    candidateForFrom = astManager.GetNodes(relation.arg1type);
                }

                if (candidateForFrom is null)
                {
                    UpdateResultTable(null, relation.arg1, relation.arg2);
                    return;
                }

                //candidates for to
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = queryResult.GetNodes(relation.arg2);
                }
                else
                {
                    candidateForTo = astManager.GetNodes(relation.arg2type);
                }

                if (candidateForFrom is null || candidateForTo is null)
                {
                    UpdateResultTable(null, relation.arg1, relation.arg2);
                }

                foreach (var c in candidateForFrom)
                    foreach (var c2 in candidateForTo)
                    {
                        if (cfgManager.IsNext((int)c.programLine, (int)c2.programLine))
                        {
                            resultListTuple.Add((c, c2));
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
                var nodes = astManager.GetAllNodesWithLineNumbers();
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
                            if (astManager.IsFollowsX(Int32.Parse(relation.arg1), (int)c.programLine) && !resultList.Contains(c))
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
                            if (astManager.IsFollowsX((int)c.programLine, Int32.Parse(relation.arg2)) && !resultList.Contains(c))
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
                    fromList = astManager.GetAllNodesWithLineNumbers();

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
                                if (tmpResult != null)
                                {
                                    foreach (var tr in tmpResult)
                                    {
                                        if (!resultList.Contains(tr))
                                        {
                                            resultList.Add(tr);
                                        }
                                    }
                                }
                            }
                            UpdateResultTable(resultList, relation.arg2);
                            return;
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
                            candidateForFrom = astManager.GetNodes(relation.arg1type);
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
                    candidateForFrom = astManager.GetNodes(relation.arg1type);
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
                var nodes = astManager.GetAllNodesWithLineNumbers();
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
                            if (astManager.IsFollows(Int32.Parse(relation.arg1), (int)c.programLine) && !resultList.Contains(c))
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
                            if (astManager.IsFollows((int)c.programLine, Int32.Parse(relation.arg2)) && !resultList.Contains(c))
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
                    fromList = astManager.GetAllNodesWithLineNumbers();

                    if (fromList != null)
                    {
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForTo = DeepCopy(queryResult.GetNodes(relation.arg2));
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
                                if (tmp != null)
                                {
                                    resultList.Add(tmp);
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
                    #region Follows(*, _)
                    if (relation.arg2 == Entity._)
                    {
                        if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFrom = queryResult.GetNodes(relation.arg1);
                        }
                        else
                        {
                            candidateForFrom = astManager.GetNodes(relation.arg1type);
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
                    candidateForFrom = DeepCopy(queryResult.GetNodes(relation.arg1));
                }
                else
                {
                    candidateForFrom = DeepCopy(astManager.GetNodes(relation.arg1type));
                }

                if (candidateForFrom is null)
                {
                    UpdateResultTable(null, relation.arg1, relation.arg2);
                    return;
                }

                //candidates for to
                if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForTo = DeepCopy(queryResult.GetNodes(relation.arg2));
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
                            if (astManager.IsParent(Int32.Parse(relation.arg1), (int)c.programLine) && !resultList.Contains(c))
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
                        var result = astManager.GetParent(child);
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
                            if (astManager.IsParent((int)c.programLine, Int32.Parse(relation.arg2)) && !resultList.Contains(c))
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

                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                    {
                        candidateForChildren = queryResult.GetNodes(relation.arg2);
                    }
                    else
                    {
                        candidateForChildren = astManager.GetNodes(relation.arg2type);
                    }

                    if (fathers is null || candidateForChildren is null)
                    {
                        UpdateResultTable(null, relation.arg2);
                        return;
                    }

                    foreach (var c in candidateForChildren)
                    {
                        if (astManager.GetParent(c) != null)
                        {
                            resultList.Add(c);
                        }
                    }

                    UpdateResultTable(resultList, relation.arg2);
                    return;
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
                }
                else
                {
                    candidateForChildren = astManager.GetNodes(relation.arg2type);
                }

                if (candidateForFather is null || candidateForChildren is null)
                {
                    UpdateResultTable(null, relation.arg1, relation.arg2);
                    return;
                }

                foreach (var father in candidateForFather)
                    foreach (var child in candidateForChildren)
                    {
                        if (astManager.IsParent((int)father.programLine, (int)child.programLine))
                        {
                            resultListTuple.Add((father, child));
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
                            if (astManager.IsParentX(Int32.Parse(relation.arg1), (int)c.programLine) && !resultList.Contains(c))
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
                            if (astManager.IsParentX((int)c.programLine, Int32.Parse(relation.arg2)) && !resultList.Contains(c))
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


                    if (queryResult.HasRecords() && queryResult.DeclarationWasDeterminated(relation.arg2))
                    {
                        candidateForChildren = queryResult.GetNodes(relation.arg2);
                    }
                    else
                    {
                        candidateForChildren = astManager.GetNodes(relation.arg2type);
                    }

                    if (fathers is null || candidateForChildren is null)
                    {
                        UpdateResultTable(null, relation.arg2);
                        return;
                    }

                    foreach (var child in candidateForChildren)
                    {
                        if (astManager.GetParentX(child) != null)
                        {
                            resultList.Add(child);
                        }
                    }

                    UpdateResultTable(resultList, relation.arg2);
                    return;
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
                }
                else
                {
                    candidateForChildren = astManager.GetNodes(relation.arg2type);
                }

                if (candidateForFather is null || candidateForChildren is null)
                {
                    UpdateResultTable(null, relation.arg1, relation.arg2);
                    return;
                }

                foreach (var father in candidateForFather)
                    foreach (var child in candidateForChildren)
                    {
                        if (astManager.IsParentX((int)father.programLine, (int)child.programLine))
                        {
                            resultListTuple.Add((father, child));
                        }
                    }

                UpdateResultTable(resultListTuple, relation.arg1, relation.arg2);
                return;
                #endregion
            }
        }




        public void UpdateResultTable(List<(TNode, TNode)> resultListTuple, string firstArgument, string secondArgument)
        {
            RemoveDotFromNameIfItIsAttrRef(ref firstArgument);
            RemoveDotFromNameIfItIsAttrRef(ref secondArgument);

            queryResult.SetDeclarationWasDeterminated(firstArgument);
            queryResult.SetDeclarationWasDeterminated(secondArgument);

            if (resultListTuple != null && resultListTuple.Any())
            {
                List<TNode[]> newResultTableList = new List<TNode[]>();
                int indexOfDeclarationFirstArgument = queryResult.FindIndexOfDeclaration(firstArgument);
                int indexOfDeclarationSecondArgument = queryResult.FindIndexOfDeclaration(secondArgument);

                if (queryResult.HasRecords())
                {
                    var earlierResultRecords = queryResult.GetResultTableList();
                    TNode[] newRecord;
                    foreach (var result in resultListTuple)
                    {
                        foreach (var record in earlierResultRecords)
                        {
                            newRecord = new TNode[queryResult.declarationsTable.Length];
                            for (int i = 0; i < record.Length; i++)
                            {
                                newRecord[i] = record[i];
                            }
                            newRecord[indexOfDeclarationFirstArgument] = result.Item1;
                            newRecord[indexOfDeclarationSecondArgument] = result.Item2;
                            newResultTableList.Add(newRecord);
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

        }

        public void UpdateResultTable(List<TNode> resultList, string argumentLookingFor)
        {
            RemoveDotFromNameIfItIsAttrRef(ref argumentLookingFor);

            if (!queryResult.HasRecords() && queryResult.declarationsTable.Where(p => p.wasDeterminated).Any())
            {
                return;
            }

            queryResult.SetDeclarationWasDeterminated(argumentLookingFor);

            if (resultList != null && resultList.Any())
            {
                List<TNode[]> newResultTableList = new List<TNode[]>();
                TNode[] newRecord;
                int indexOfDeclaration = queryResult.FindIndexOfDeclaration(argumentLookingFor);

                if (queryResult.HasRecords())
                {
                    var earlierResultRecords = queryResult.GetResultTableList();

                    foreach (var result in resultList)
                    {
                        foreach (var record in earlierResultRecords)
                        {
                            newRecord = new TNode[record.Length];
                            for (int i = 0; i < record.Length; i++)
                            {
                                newRecord[i] = record[i];
                            }
                            newRecord[indexOfDeclaration] = result;
                            newResultTableList.Add(newRecord);
                        }
                    }
                }
                else
                {
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
        }

        public void UpdateResultTable(bool p_result)
        {
            if (actualCondition is With)
            {
                var relation = (Relation)actualCondition;
                if (relation.arg1type != Entity._string && relation.arg1type != Entity._)
                {
                    queryResult.SetDeclarationWasDeterminated(relation.arg1);
                }
                if (relation.arg2type != Entity._string && relation.arg2type != Entity._)
                {
                    queryResult.SetDeclarationWasDeterminated(relation.arg2);
                }
            }

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

        private void RemoveDotFromNameIfItIsAttrRef(ref string s)
        {
            if (s.Contains('.')) { s = s.Substring(0, s.IndexOf('.')); }
        }
    }
}
