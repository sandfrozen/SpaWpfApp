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
        }

        internal void Init()
        {
            //clear resultTable
        }

        public void Evaluate(List<Relation> relationList)
        {
            Result.GetInstance().Init();

            foreach (var relation in relationList)
            {
                switch (relation.type)
                {
                    case Relation.Parent:
                        Parent(relation);
                        Result r = Result.GetInstance(); // do testów, potem do usunięcia ta linia
                        break;
                }
            }
        }

        internal void Parent(Relation relation)
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
                else if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForChildren = Result.GetInstance().GetNodes(relation.arg2);
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
                else if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFather = Result.GetInstance().GetNodes(relation.arg1);
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
                        if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(relation.arg2))
                        {
                            candidateForChildren = Result.GetInstance().GetNodes(relation.arg2);
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
                    if (relation.arg2 == "_")
                    {
                        if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(relation.arg1))
                        {
                            candidateForFather = Result.GetInstance().GetNodes(relation.arg1);
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
                if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(relation.arg1))
                {
                    candidateForFather = Result.GetInstance().GetNodes(relation.arg1);
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
                if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(relation.arg2))
                {
                    candidateForChildren = Result.GetInstance().GetNodes(relation.arg2);
                    if (candidateForChildren != null)
                    {
                        for(int i=0; i< candidateForFather.Count(); i++)
                        {
                            if(astManager.IsParent((int)candidateForFather[i].programLine, (int)candidateForChildren[i].programLine))
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


        private void UpdateResultTable(List<(TNode, TNode)> resultListTuple, string firstArgument, string secondArgument)
        {
            if (resultListTuple != null && resultListTuple.Any())
            {
                List<TNode[]> newResultTableList = new List<TNode[]>();
                int indexOfDeclarationFirstArgument = Result.GetInstance().FindIndexOfDeclaration(firstArgument);
                int indexOfDeclarationSecondArgument = Result.GetInstance().FindIndexOfDeclaration(secondArgument);

                if (Result.GetInstance().HasRecords())
                {
                    var earlierResultRecords = Result.GetInstance().GetResultTableList();

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
                    foreach (var result in resultListTuple)
                    {
                        newRecord = new TNode[Result.GetInstance().declarationsTable.Length];
                        newRecord[indexOfDeclarationFirstArgument] = result.Item1;
                        newRecord[indexOfDeclarationSecondArgument] = result.Item2;
                        newResultTableList.Add(newRecord);

                    }
                }

                Result.GetInstance().UpdateResutTableList(newResultTableList);
            }
            else
            {
                Result.GetInstance().ClearResultTableList();
                FinishQueryEvaluator();
            }

            Result.GetInstance().SetDeclarationWasDeterminated(firstArgument);
            Result.GetInstance().SetDeclarationWasDeterminated(secondArgument);
        }

        private void UpdateResultTable(List<TNode> resultList, string argumentLookingFor)
        {
            if (resultList != null && resultList.Any())
            {
                List<TNode[]> newResultTableList = new List<TNode[]>();
                int indexOfDeclaration = Result.GetInstance().FindIndexOfDeclaration(argumentLookingFor);

                if (Result.GetInstance().HasRecords())
                {
                    var earlierResultRecords = Result.GetInstance().GetResultTableList();

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
                    foreach (var result in resultList)
                    {
                        newRecord = new TNode[Result.GetInstance().declarationsTable.Length];
                        newRecord[indexOfDeclaration] = result;
                        newResultTableList.Add(newRecord);
                    }
                }

                Result.GetInstance().UpdateResutTableList(newResultTableList);
            }
            else
            {
                Result.GetInstance().ClearResultTableList();
                FinishQueryEvaluator();
            }

            Result.GetInstance().SetDeclarationWasDeterminated(argumentLookingFor);
        }

        private void UpdateResultTable(bool result)
        {
            if (QueryPreProcessor.GetInstance().ReturnTypeIsBoolean())
            {
                Result.GetInstance().ResultBoolean = result;
            }
            else
            {
                if (result is false)
                {
                    Result.GetInstance().ClearResultTableList();
                }
            }

            if(result is false)
            {
                FinishQueryEvaluator();
            }

        }

        private void FinishQueryEvaluator()
        {
            //make relation string
            throw new NoResultsException("Relation " + actualRelation.ToString() + " has no results.");
        }
    }
}
