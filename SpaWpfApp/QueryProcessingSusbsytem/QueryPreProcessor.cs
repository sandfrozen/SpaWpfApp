using SpaWpfApp.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
              {"assignment", ParseAssignmentDeclaration},
              {"while", ParseAssignmentDeclaration},
              {"procedure", ParseAssignmentDeclaration},
              {"statement", ParseAssignmentDeclaration},
            };
        }

        public string Parse(string query)
        {
            this.query = query;
            wordsInQuery = GetWordsInCode();

            for (currentIndex = 0; currentIndex < wordsInQuery.Length; currentIndex++)
            {
                Trace.WriteLine(wordsInQuery[currentIndex]);
                if (declarationActions.Keys.Any(k => k == wordsInQuery[currentIndex]))
                {
                    declarationActions[wordsInQuery[currentIndex]]();

                }
                if (wordsInQuery[currentIndex] == "Select")
                {

                }


                //char afterWord = ' ';
                //if (wordsInQuery[i].Last() == ';')
                //{
                //    afterWord = '\n';
                //}
                //parsedSourceCode += wordsInQuery[i] + afterWord;

            }

            return parsedQuery;
        }

        private string[] GetWordsInCode()
        {
            string[] separators = new string[] { " ", Environment.NewLine };
            return query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }

        private void parseIdentificator(string name)
        {
            if (Char.IsLetter(name[0]))
            {
                for (int i = 1; i < name.Length; i++)
                {
                    if (!Char.IsLetterOrDigit(name[i]))
                    {
                        throw new QueryException("Wrong identificator name: " + name);
                    }
                }
            }
            else
            {
                throw new Exception();
            }
        }

        private void ParseAssignmentDeclaration()
        {
            Trace.WriteLine("ParseAssignmentDeclaration: " + wordsInQuery[currentIndex] + " " + wordsInQuery[currentIndex + 1]);
            do
            {
                currentIndex++;
                string a = wordsInQuery[currentIndex];
                a = a.Substring(0, a.Length - 1);
                parseIdentificator(a);
                Trace.WriteLine("Assignment: " + a);
                declarations.Add(a, "assignment");
            } while (wordsInQuery[currentIndex].Last() != ';');
        }

        private void ParseSelect()
        {
            Trace.WriteLine("ParseAssignmentDeclaration: " + wordsInQuery[currentIndex]);
        }
    }
}
