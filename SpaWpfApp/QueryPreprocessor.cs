using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpaWpfApp
{
    public class QueryPreprocessor
    {
        public Query Query { get; set; }

        public QueryPreprocessor(string query)
        {
            this.Query = new Query(query);
        }


    }

    public class Query
    {
        public List<String> Lines { get; set; }

        public Query(string query)
        {
            this.Lines = SeparateLines(query);
        }

        /// <summary>
        /// Parsing Query to Lines
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<String> SeparateLines(string query)
        {
            List<String> tmpList = new List<String>();
            string[] lines = Regex.Split(query, "\r\n");
            foreach (var line in lines)
            {
                string[] partsOfLine = Regex.Split(line, ";");
                foreach(var part in partsOfLine)
                {
                    var keys = part.Split(' ');
                    foreach(var key in keys)
                        tmpList.Add(key.ToLower().Replace('(', ' ').Replace(')', ' ').Trim());
                }
            }
            tmpList.RemoveAll(x=>x == string.Empty);
            List<String> result = new List<String>();
            foreach(var tmp in tmpList)
            {
                var keys = tmp.Split(',');
                foreach (var key in keys)
                    result.Add(key);
            }
            result.RemoveAll(x => x == string.Empty);
            return result;
        }
    }
}
