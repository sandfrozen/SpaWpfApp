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

        private string query;
        private string parsedQuery;

        private string[] wordsInQuery;
        private int currentIndex;

        private Dictionary<string, string> declarations;
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
            declarations = new Dictionary<string, string>();
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
            if (wordsInQuery[currentIndex] != "such")
            {
                throw new WrongQueryFromatException("such not found: " + wordsInQuery[currentIndex]);
            }
            parsedQuery += " " + wordsInQuery[currentIndex++];
            if (wordsInQuery[currentIndex] != "that")
            {
                throw new WrongQueryFromatException("that not found: " + wordsInQuery[currentIndex]);
            }
            parsedQuery += " " + wordsInQuery[currentIndex++];

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

        private bool IsTuple(string tuple)
        {
            try
            {
                IsSynonym(tuple);
            }
            catch
            {
                if (tuple[0] != '<' || tuple.Last() != '>')
                {
                    throw new WrongQueryFromatException("Invalid tuple: " + tuple);
                }
                for (int elemIndex = 1; elemIndex < tuple.Length; elemIndex++)
                {
                    string elem = "";
                    do
                    {
                        elem += tuple[elemIndex++];
                    } while (elemIndex < tuple.Length && tuple[elemIndex] != ',' && tuple[elemIndex] != '>');
                    IsSynonym(elem);
                }
            }
            return true;
        }

        private bool IsSynonym(string synonym)
        {
            if (!Regex.IsMatch(synonym, @"^([a-zA-Z]){1}([a-zA-Z]|[0-9]|[#])*$"))
            {
                throw new WrongQueryFromatException("Invalid synonym: " + synonym);
            }
            return true;
        }

        private void CheckDeclaration(string declaration)
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
            string declaration = wordsInQuery[currentIndex] + " ";
            while (wordsInQuery[currentIndex] != ";")
            {
                declaration += wordsInQuery[++currentIndex];
                if (wordsInQuery[currentIndex] != "," && wordsInQuery[currentIndex] != ";" && wordsInQuery[currentIndex+1] != "," && wordsInQuery[currentIndex+1] != ";")
                {
                    declaration += " ";
                }
            }
            string declarationArgs = declaration.Substring(declaration.IndexOf(' ')+1);
            CheckDeclaration(declarationArgs);

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
            } while (currentIndex < wordsInQuery.Length && wordsInQuery[currentIndex] != "such");
            if (currentIndex >= wordsInQuery.Length)
            {
                throw new WrongQueryFromatException("Invalid word after tuple: " + tuple);
            }
            Trace.WriteLine(tuple);
            IsTuple(tuple);
            parsedQuery += " " + tuple;
        }
    }
}
