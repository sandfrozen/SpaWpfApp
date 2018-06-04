using SpaWpfApp.Ast;
using SpaWpfApp.PkbNew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    public class QueryProjector
    {
        private QueryResult queryResult;
        private QueryPreProcessor queryPreProcessor;
        private QueryEvaluator queryEvaluator;
        private AstManager astManager;
        public PkbAPI Pkb { get; set; }

        private static QueryProjector instance;

        public string result { get; set; }

        public static QueryProjector GetInstance()
        {
            if (instance == null)
            {
                instance = new QueryProjector();
                instance.queryPreProcessor = QueryPreProcessor.GetInstance();
                instance.queryResult = QueryResult.GetInstance();
                instance.astManager = AstManager.GetInstance();
                instance.queryEvaluator = QueryEvaluator.GetInstance();
            }
            return instance;
        }

        public void Init()
        {

        }

        public string PrintResult()
        {
            result = string.Empty;

            if (queryPreProcessor.ReturnTypeIsBoolean())
            {
                result = queryResult.resultBoolean.ToString();
                return result;
            }
            else if (queryPreProcessor.returnList.Count == 1)
            {
                var returnElement = queryPreProcessor.returnList.First();
                if (!returnElement.Key.Contains('.'))
                {
                    List<TNode> resultList = new List<TNode>();
                    if (queryResult.DeclarationWasDeterminated(returnElement.Key))
                    {
                        int indexOfDeclaration = queryResult.FindIndexOfDeclaration(returnElement.Key);
                        foreach (var record in queryResult.resultTableList)
                        {
                            resultList.Add(record[indexOfDeclaration]);
                        }
                        resultList = resultList.Distinct().ToList();
                    }
                    else
                    {
                        resultList = astManager.GetNodes(returnElement.Value);
                    }

                    if (!resultList.Any())
                    {
                        result = "none";
                        return result;
                    }
                    else if (returnElement.Value == Entity.procedure)
                    {
                        foreach (var p in resultList)
                        {
                            result += Pkb.GetProcName((int)p.indexOfName) + ", ";
                        }
                    }
                    else if (returnElement.Value == Entity.constant)
                    {
                        foreach (var cs in resultList)
                        {
                            result += cs.value + ", ";
                        }
                    }
                    else if (returnElement.Value == Entity.variable)
                    {
                        foreach (var v in resultList)
                        {
                            result += Pkb.GetVarName((int)v.indexOfName) + ", ";
                        }
                    }
                    else
                    {
                        foreach (var r in resultList)
                        {
                            result += r.programLine + ", ";
                        }
                    }


                    result = result.Substring(0, result.Length - 2);
                    return result;
                }
                else
                {
                    List<TNode> resultList = new List<TNode>();
                    string key = returnElement.Key.Substring(0, returnElement.Key.IndexOf('.'));
                    string attr = returnElement.Key.Substring(returnElement.Key.IndexOf('.') + 1);
                    if (queryResult.DeclarationWasDeterminated(key))
                    {
                        int indexOfDeclaration = queryResult.FindIndexOfDeclaration(key);
                        foreach (var record in queryResult.resultTableList)
                        {
                            resultList.Add(record[indexOfDeclaration]);
                        }
                        resultList = resultList.Distinct().ToList();
                    }
                    else
                    {
                        resultList = astManager.GetNodes(returnElement.Value);
                    }

                    if (!resultList.Any())
                    {
                        result = "none";
                        return result;
                    }
                    else if (returnElement.Value == Entity.procedure || returnElement.Value == Entity.call)
                    {
                        foreach (var pc in resultList)
                        {
                            result += Pkb.GetProcName((int)pc.indexOfName) + ", ";
                        }
                    }
                    else if (returnElement.Value == Entity.constant)
                    {
                        foreach (var cs in resultList)
                        {
                            result += cs.value + ", ";
                        }
                    }
                    else if (returnElement.Value == Entity.variable)
                    {
                        foreach (var v in resultList)
                        {
                            result += Pkb.GetVarName((int)v.indexOfName) + ", ";
                        }
                    }
                    else
                    {
                        foreach (var r in resultList)
                        {
                            result += r.programLine + ", ";
                        }
                    }


                    result = result.Substring(0, result.Length - 2);
                    return result;
                }
            }
            else
            {
                var returnList = new Dictionary<string, string>();
                string newKey;
                foreach (var v in queryPreProcessor.returnList)
                {
                    if (v.Key.Contains('.'))
                    {
                        newKey = v.Key.Substring(0, v.Key.IndexOf('.'));
                    }
                    else { newKey = v.Key; }

                    returnList.Add(newKey, v.Value);
                }

                List<TNode> collection;
                foreach(var v in returnList)
                {
                    if (!queryResult.DeclarationWasDeterminated(v.Key))
                    {
                        collection = astManager.GetNodes(v.Value);
                        queryEvaluator.UpdateResultTable(collection, v.Key);
                    }
                }

                List<int> indexList = new List<int>();
                foreach(var v in returnList)
                {
                    indexList.Add(queryResult.FindIndexOfDeclaration(v.Key));
                }

                foreach(var r in queryResult.resultTableList)
                {
                    foreach(var i in indexList)
                    {
                        switch (queryResult.declarationsTable[i].value)
                        {
                            case Entity.procedure:
                            case Entity.call:
                                result += Pkb.GetProcName((int)r[i].indexOfName) + " ";
                                break;
                            case Entity.variable:
                                result += Pkb.GetVarName((int)r[i].indexOfName) + " ";
                                break;
                            case Entity.constant:
                                result += r[i].value + " ";
                                break;
                            default:
                                result += r[i].programLine + " ";
                                break;
                        }
                    }

                    result = result.Substring(0, result.Length - 1);
                    result += ", ";
                }

                result = result.Substring(0, result.Length - 2);
                return result;
            }
        }
    }
}
