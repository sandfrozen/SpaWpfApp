using SpaWpfApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    class QueryPreProcessor
    {
        private string parsedQuery;

        private string[] wordsInQuery;
        private int currentIndex;

        private Dictionary<string, string> declarationsList;
        private Dictionary<string, Action> declarationActions;

        private static QueryPreProcessor instance;
        public static QueryPreProcessor GetInstance()
        {
            if (instance == null)
            {
                instance = new QueryPreProcessor();
            }
            return instance;
        }

        public QueryPreProcessor()
        {
            declarationsList = new Dictionary<string, string>();
            declarationActions = new Dictionary<string, Action>{
              {"procedure", ParseDeclaration},
              {"stmtLst", ParseDeclaration},
              {"stmt", ParseDeclaration},
              {"assign", ParseDeclaration},
              {"call", ParseDeclaration},
              {"while", ParseDeclaration},
              {"if", ParseDeclaration},
              {"variable", ParseDeclaration},
              {"constant", ParseDeclaration},
              {"prog_line", ParseDeclaration},
            };
        }

        public string Parse(string query)
        {
            declarationsList = new Dictionary<string, string>();
            parsedQuery = "";
            if (!query.Contains("Select"))
            {
                throw new WrongQueryFromatException("Select is missing.");
            }
            wordsInQuery = GetWordsInCode(query);
            if (wordsInQuery[0] != "Select")
            {
                for (currentIndex = 0; currentIndex < wordsInQuery.Length; currentIndex++)
                {
                    if (declarationActions.Keys.Any(k => k == wordsInQuery[currentIndex]))
                    {
                        declarationActions[wordsInQuery[currentIndex]]();

                    }
                    else if (wordsInQuery[currentIndex] == "Select")
                    {
                        break;
                    }
                    else
                    {
                        throw new WrongQueryFromatException("Invalid word: " + wordsInQuery[currentIndex]);
                    }
                }
            }
            if (wordsInQuery[currentIndex] != "Select")
            {
                throw new WrongQueryFromatException("Select not found: " + wordsInQuery[currentIndex]);
            }
            parsedQuery += wordsInQuery[currentIndex++];
            ParseTouple();
            while (currentIndex < wordsInQuery.Length)
            {
                if (wordsInQuery[currentIndex] == "such")
                {
                    parsedQuery += " " + wordsInQuery[currentIndex++];
                    if (wordsInQuery[currentIndex] == "that")
                    {
                        parsedQuery += " " + wordsInQuery[currentIndex++];
                    }
                    else
                    {
                        throw new WrongQueryFromatException("\"that\" after \"such\" not found: " + wordsInQuery[currentIndex]);
                    }
                }

                else if (wordsInQuery[currentIndex] == "with")
                {
                    parsedQuery += " " + wordsInQuery[currentIndex];
                }
                else if (wordsInQuery[currentIndex] == "pattern")
                {
                    parsedQuery += " " + wordsInQuery[currentIndex];
                }
                else
                {
                    throw new WrongQueryFromatException("(such that|with|pattern) not found: " + wordsInQuery[currentIndex]);
                }

                break;
            }
            

            Trace.WriteLine("Declarations:");
            foreach (var v in declarationsList)
            {
                Trace.WriteLine(v.Value + " " + v.Key);
            }

            return parsedQuery;
        }

        private string[] GetWordsInCode(string query)
        {
            for (int i = 0; i < query.Length; i++)
            {
                if (i < query.Length - 1 && IsSeparatorChar(query[i]))
                {
                    query = query.Insert(i, " ");
                    query = query.Insert(i + 2, " ");
                    i = i + 2;
                }
            }
            string[] separators = new string[] { " ", Environment.NewLine };
            return query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        private bool IsSeparatorChar(char toCheck)
        {
            char[] chars = { '(', ')', ',', ';', '<', '>' };
            foreach (char c in chars)
            {
                if (c == toCheck) return true;
            }
            return false;
        }

        private void CkeckIsDeclared(string synonym)
        {
            if (!declarationsList.Keys.Contains(synonym))
            {
                throw new QueryException("Synonym used in Select is not declared: " + synonym);
            }
        }

        private void CheckSingleTuple(string tuple)
        {
            if (tuple.First() == '<' && tuple.Last() == '>')
            {
                CheckMultipleTuple(tuple);
            }
            else
            {
                if (tuple.Contains("."))
                {
                    string synonym = tuple.Substring(0, tuple.IndexOf('.'));
                    CkeckIsDeclared(synonym);
                    string attrName = tuple.Substring(tuple.IndexOf('.') + 1);
                    string designEntity = declarationsList[synonym];
                    
                    switch (attrName)
                    {
                        case "stmt#":
                            if (designEntity != "assign")
                            {
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            }
                            break;
                        case "procName":
                            if (designEntity != "procedure")
                            {
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            }
                            break;
                        case "varName":
                        case "value":
                            if (designEntity != "variable")
                            {
                                throw new QueryException("Synonym: " + synonym + " can't be used with: " + attrName);
                            }
                            break;
                        default:
                            throw new QueryException("Attrybute name: " + attrName + " is unknown");
                            break;
                    }
                }
                else
                {
                    CkeckIsDeclared(tuple);
                }
            }
        }

        private void CheckMultipleTuple(string tuple)
        {
            for (int elemIndex = 1; elemIndex < tuple.Length; elemIndex++)
            {
                string elem = "";
                do
                {
                    elem += tuple[elemIndex++];
                } while (elemIndex < tuple.Length && tuple[elemIndex] != ',' && tuple[elemIndex] != '>');
                CheckSingleTuple(elem);
            }
        }

        private void IsSynonym(string synonym)
        {
            if (!Regex.IsMatch(synonym, @"^([a-zA-Z]){1}([a-zA-Z]|[0-9]|[#])*$"))
            {
                throw new WrongQueryFromatException("Invalid synonym: " + synonym);
            }
        }

        private void CheckSynonyms(string declaration)
        {
            try
            {
                IsSynonym(declaration);
            }
            catch
            {
                for (int declarationIndex = 0; declarationIndex < declaration.Length - 1; declarationIndex++)
                {
                    string synonym = "";
                    do
                    {
                        synonym += declaration[declarationIndex++];
                    } while (declarationIndex < declaration.Length && declaration[declarationIndex] != ',' && declaration[declarationIndex] != ';');

                    IsSynonym(synonym);
                }
            }
        }

        private void ParseDeclaration()
        {
            List<String> synonymsList = new List<string>();
            string declaration = wordsInQuery[currentIndex] + " ";
            while (wordsInQuery[currentIndex] != ";")
            {
                declaration += wordsInQuery[++currentIndex];
                if (wordsInQuery[currentIndex] != "," && wordsInQuery[currentIndex] != ";")
                {
                    synonymsList.Add(wordsInQuery[currentIndex]);
                }
                if (wordsInQuery[currentIndex] != "," && wordsInQuery[currentIndex] != ";" && wordsInQuery[currentIndex + 1] != "," && wordsInQuery[currentIndex + 1] != ";")
                {
                    declaration += " ";
                }
            }
            string designEntity = declaration.Substring(0, declaration.IndexOf(' '));
            string synonyms = declaration.Substring(declaration.IndexOf(' ') + 1);
            CheckSynonyms(synonyms);

            try
            {
                foreach (string synonym in synonymsList)
                {
                    declarationsList.Add(synonym, designEntity);
                }
            }
            catch
            {
                throw new QueryException("Synonyms can't have same names.\nCheck declaration:  " + declaration);
            }


            parsedQuery += declaration;
            if (wordsInQuery[currentIndex + 1] == "Select")
            {
                parsedQuery += Environment.NewLine;
            }
            else
            {
                parsedQuery += " ";
            }
        }

        private void ParseTouple()
        {
            string tuple = "";
            do
            {
                tuple += wordsInQuery[currentIndex++];
            } while (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] != "such" && wordsInQuery[currentIndex] != "with" && wordsInQuery[currentIndex] != "pattern");

            CheckSingleTuple(tuple);
            parsedQuery += " " + tuple;
        }
    }
}
