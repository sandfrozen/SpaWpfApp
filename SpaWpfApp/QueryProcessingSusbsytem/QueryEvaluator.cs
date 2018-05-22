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
            int fatherArgumentInt, childArgumentInt;
            List<TNode> candidateForChildren, candidateForFather;
            List<TNode> resultList = new List<TNode>();


            #region Parent(int, int), Parent(_, _)
            if (Int32.TryParse(fatherArgument, out fatherArgumentInt) && Int32.TryParse(childArgument, out childArgumentInt))
            {
                bool result = astManager.IsParent(fatherArgumentInt, childArgumentInt);

                UpdateResultTable(result);
                return;
            }
            else if (fatherArgument == "_" && childArgument == "_")
            {
                var fathers = astManager.GetAllParents();
                List<TNode> result;
                if (fathers != null)
                {
                    foreach (var f in fathers)
                    {
                        result = astManager.GetChildren(f, childArgument);
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

            else if (Int32.TryParse(fatherArgument, out fatherArgumentInt))
            {
                TNode father = astManager.FindFather(fatherArgumentInt);

                #region Parent(int, _)
                if (childArgument == "_")
                {
                    if (father is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = astManager.GetChildren(father, childArgument) != null ? true : false;
                        UpdateResultTable(result);
                        return;
                    }
                }
                #endregion

                #region Parent(int, *)
                if (father is null)
                {
                    UpdateResultTable(null, childArgument);
                    return;
                }
                else if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(childArgument))
                {
                    candidateForChildren = Result.GetInstance().GetNodes(childArgument);
                    if (candidateForChildren != null)
                    {
                        foreach (var c in candidateForChildren)
                        {
                            if (astManager.IsParent(fatherArgumentInt, (int)c.programLine))
                            {
                                resultList.Add(c);
                            }
                        }
                    }
                    UpdateResultTable(resultList, childArgument);
                    return;
                }
                else
                {
                    resultList = astManager.GetChildren(father, childArgument);
                }
                #endregion
            }

            else if (Int32.TryParse(childArgument, out childArgumentInt))
            {
                TNode child = astManager.FindNode(childArgumentInt);

                #region Parent(_, int)
                if (fatherArgument == "_")
                {
                    if (child is null)
                    {
                        UpdateResultTable(false);
                        return;
                    }
                    else
                    {
                        var result = astManager.GetParent(child, fatherArgument);
                        UpdateResultTable(result != null ? true : false);
                        return;
                    }
                }
                #endregion

                #region Parent(*, int)
                if (child is null)
                {
                    UpdateResultTable(null, fatherArgument);
                    return;
                }
                else if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(fatherArgument))
                {
                    candidateForFather = Result.GetInstance().GetNodes(fatherArgument);
                    if (candidateForFather != null)
                    {
                        foreach (var c in candidateForFather)
                        {
                            if (astManager.IsParent((int)c.programLine, childArgumentInt))
                            {
                                resultList.Add(c);
                                break;
                            }
                        }
                    }
                    UpdateResultTable(resultList, fatherArgument);
                    return;
                }
                else
                {
                    TNode tmp = astManager.GetParent(child, fatherArgument);
                    if (tmp != null) { resultList.Add(tmp); }
                    UpdateResultTable(resultList, fatherArgument);
                    return;
                }
                #endregion
            }

            else
            {
                List<TNode> fathers = null;
                string fatherArgumentType;
                List<TNode> tmpResult = null;

                #region Parent(_, *)
                if (fatherArgument == "_")
                {
                    fathers = astManager.GetAllParents();

                    if (fathers != null)
                    {
                        if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(childArgument))
                        {
                            candidateForChildren = Result.GetInstance().GetNodes(childArgument);
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
                                tmpResult = astManager.GetChildren(father, childArgument);
                                if (tmpResult != null)
                                {
                                    foreach (var child in tmpResult)
                                    {
                                        resultList.Add(child);
                                    }
                                }
                            }
                        }

                        UpdateResultTable(resultList, childArgument);
                        return;
                    }
                    else
                    {
                        UpdateResultTable(null, childArgument);
                        return;
                    }

                }
                #endregion

                else
                {
                    #region Parent(*, _)
                    if (childArgument == "_")
                    {
                        if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(fatherArgument))
                        {
                            candidateForFather = Result.GetInstance().GetNodes(fatherArgument);
                        }
                        else
                        {
                            fatherArgumentType = QueryPreProcessor.GetInstance().declarationsList[fatherArgument];
                            if (fatherArgumentType == "stmt")
                            {
                                candidateForFather = astManager.GetAllParents();
                            }
                            else if (fatherArgumentType == "if")
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
                                var hasChildren = astManager.GetChildren(father, childArgument) != null ? true : false;
                                if (hasChildren) { resultList.Add(father); }
                            }
                        }

                        UpdateResultTable(resultList, fatherArgument);
                        return;
                    }
                    #endregion
                }

                #region Parent(*, *)
                List<(TNode, TNode)> resultListTuple = new List<(TNode, TNode)>();

                //candidates for fathers
                if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(fatherArgument))
                {
                    candidateForFather = Result.GetInstance().GetNodes(fatherArgument);
                }
                else
                {
                    fatherArgumentType = QueryPreProcessor.GetInstance().declarationsList[fatherArgument];
                    if (fatherArgumentType == "stmt")
                    {
                        candidateForFather = astManager.GetAllParents();
                    }
                    else if (fatherArgumentType == "if")
                    {
                        candidateForFather = astManager.GetAllIf();
                    }
                    else
                    {
                        candidateForFather = astManager.GetAllWhile();
                    }
                }

                //candidates for children
                if (Result.GetInstance().HasRecords() && Result.GetInstance().DeclarationWasDeterminated(childArgument))
                {
                    candidateForChildren = Result.GetInstance().GetNodes(childArgument);
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
                        tmpResult = astManager.GetChildren(father, childArgument);
                        if (tmpResult != null)
                        {
                            foreach (var child in tmpResult)
                            {
                                resultListTuple.Add((father, child));
                            }
                        }
                    }
                }

                UpdateResultTable(resultListTuple, fatherArgument, childArgument);
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
            throw new NoResultsException("Relation blablabla has no results.");
        }
    }
}
