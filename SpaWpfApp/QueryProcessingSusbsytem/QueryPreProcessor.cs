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
            wordsInQuery = GetWordsInCode(query);

            for (currentIndex = 0; currentIndex < wordsInQuery.Length; currentIndex++)
            {
                if (declarationActions.Keys.Any(k => k == wordsInQuery[currentIndex]))
                {
                    declarationActions[wordsInQuery[currentIndex]]();

                }
                else
                {
                    ParserSelect();
                }
            }

            return parsedQuery;
        }

        private string[] GetWordsInCode(string query)
        {
            for(int i=0; i< query.Length; i++)
            {
                if(i < query.Length-1 && IsSeparatorChar(query[i]))
                {
                    query = query.Insert(i, " ");
                    query = query.Insert(i+2, " ");
                    i = i+ 2;
                }
            }
            string[] separators = new string[] { " ", Environment.NewLine };
            return query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        private bool IsSeparatorChar(char toCheck)
        {
            char[] chars = { '(', ')', ',', ';' };
            foreach(char c in chars)
            {
                if (c == toCheck) return true;
            }
            return false;
        }

        private bool IsTuple(string tuple)
        {
            if (!Regex.IsMatch(tuple, @"^$"))
            {
                throw new WrongQueryFromatException("Invalid tuple: " + tuple);
            }
            return true;
        }

        private bool IsElement(string element)
        {
            if (!Regex.IsMatch(element, @"^$"))
            {
                throw new WrongQueryFromatException("Invalid element: " + element);
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

        private bool IsDesignEntity(string entity)
        {
            if (!Regex.IsMatch(entity, @"^$"))
            {
                throw new WrongQueryFromatException("Invalid entity: " + entity);
            }
            return true;
        }

        private void ParseDeclaration()
        {
            string declaration = wordsInQuery[currentIndex];
            do
            {
                declaration += " " + wordsInQuery[++currentIndex];
            } while (wordsInQuery[currentIndex].Last() != ';');
            Trace.WriteLine(declaration);
            //Regex checking
            parsedQuery += declaration;
            if (wordsInQuery[currentIndex+1] == "Select")
            {
                parsedQuery += Environment.NewLine;
            } else
            {
                parsedQuery += " ";
            }
        }

        private void ParserSelect()
        {
            parsedQuery += wordsInQuery[currentIndex] + " ";
        }
    }
}
