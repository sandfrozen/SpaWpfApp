using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    class QueryPreProcessor
    {
        //private static QueryPreProcessor instance;
        private string pqlQuery;
        private string parsedSourceCode;

        //private QueryPreProcessor()
        //{

        //}

        //public static QueryPreProcessor Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //        {
        //            instance = new QueryPreProcessor();
        //        }
        //        return instance;
        //    }
        //}

        public string Parse(string pqlQuery)
        {
            this.pqlQuery = pqlQuery;
            string[] wordsInQuery = GetWordsInCode();

            for (int i = 0; i < wordsInQuery.Length; i++)
            {
                char afterWord = ' ';
                if (wordsInQuery[i].Last() == ';')
                {
                    afterWord = '\n';
                }
                parsedSourceCode += wordsInQuery[i] + afterWord;

            }

            return parsedSourceCode;
        }

        private string[] GetWordsInCode()
        {
            string[] separators = new string[] { " ", Environment.NewLine };
            return pqlQuery.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
