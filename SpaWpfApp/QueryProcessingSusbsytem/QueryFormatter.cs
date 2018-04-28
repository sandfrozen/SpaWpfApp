using SpaWpfApp.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    class QueryFormatter
    {
        private string query;
        private static QueryFormatter instance;
        public static QueryFormatter GetInstance()
        {
            if (instance == null)
            {
                instance = new QueryFormatter();
            }
            return instance;
        }

        public string Format(string query)
        {
            this.query = query;
            string[] wordsInQuery = GetWordsInQuery();
            string formattedQuery = "";

            for (int currentIndex = 0; currentIndex < wordsInQuery.Length; currentIndex++)
            {
                formattedQuery += wordsInQuery[currentIndex];
                if (wordsInQuery[currentIndex].Last() == ';')
                {
                    if (currentIndex < wordsInQuery.Length - 1)
                    {
                        if (wordsInQuery[currentIndex + 1] == "Select")
                        {
                            formattedQuery += Environment.NewLine;
                        }
                        else
                        {
                            formattedQuery += ParserHelpers.space;
                        }
                    }
                }
                else if (currentIndex < wordsInQuery.Length - 1 && wordsInQuery[currentIndex+1] != "," && wordsInQuery[currentIndex + 1] != ";" && wordsInQuery[currentIndex + 1] != "(" && wordsInQuery[currentIndex + 1] != ")")
                {
                    //new[] { ",", ";", "(", ")" }.Any(c => wordsInQuery[currentIndex + 1].Contains(c))
                    formattedQuery += ParserHelpers.space;
                }
            }

            return formattedQuery;
        }

        private string[] GetWordsInQuery()
        {
            string[] separators = new string[] { " ", Environment.NewLine };
            return query.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
