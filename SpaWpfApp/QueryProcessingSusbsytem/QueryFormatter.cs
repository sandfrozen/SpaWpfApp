using SpaWpfApp.Exceptions;
using SpaWpfApp.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    class QueryFormatter
    {
        private string formattedQuery;
        private string[] wordsInQuery;
        private int currentIndex = 0;

        private static QueryFormatter instance;
        public static QueryFormatter GetInstance()
        {
            if (instance == null)
            {
                instance = new QueryFormatter();
            }
            return instance;
        }
        public QueryFormatter()
        {
        }

        public string Format0(string query)
        {
            wordsInQuery = GetWordsInQuery(query);
            if( !wordsInQuery.Contains("Select") )
            {
                throw new WrongQueryFromatException("Select not found");
            }

            formattedQuery = "";
            currentIndex = 0;

            DeclarationsFormatter();
            SelectFormatter();

            return formattedQuery;
        }

        public void DeclarationsFormatter()
        {
            while (wordsInQuery[currentIndex] != "Select")
            {
                DeclarationFormatter();
                currentIndex++;
            } 
        }

        public void DeclarationFormatter()
        {
            string declaration = wordsInQuery[currentIndex];
            do
            {
                declaration += " " + wordsInQuery[++currentIndex];

                if (wordsInQuery[currentIndex].Last() != ',' && wordsInQuery[currentIndex].Last() != ';')
                {
                    if (wordsInQuery[currentIndex+1] == "," || wordsInQuery[currentIndex+1] == ";")
                    {
                        declaration += wordsInQuery[++currentIndex];
                    }

                }

            } while (wordsInQuery[currentIndex].Last() != ';');

            //Regex
            if (!Regex.IsMatch(declaration, @"^([a-z]+)\s{1}([a-zA-Z0-9]+[,]{1}\s{1})*([a-zA-Z0-9]+[;]{1}){1}$"))
            {
                throw new WrongQueryFromatException("Declaration is incorrect: \" " + declaration + " \"");
            }

            if (wordsInQuery[currentIndex + 1] == "Select")
            {
                declaration += Environment.NewLine;
            }
            else
            {
                declaration += " ";
            }
            formattedQuery += declaration;
        }

        public void SelectFormatter()
        {
            do
            {
                formattedQuery += wordsInQuery[currentIndex++] + " ";
            } while (currentIndex < wordsInQuery.Length && (wordsInQuery[currentIndex] != ";" || wordsInQuery[currentIndex].Last() == ';'));
            formattedQuery = formattedQuery.Remove(formattedQuery.Length-1);
        }

        private string[] GetWordsInQuery(string query)
        {
            string[] separators = new string[] { " ", Environment.NewLine };
            return query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
