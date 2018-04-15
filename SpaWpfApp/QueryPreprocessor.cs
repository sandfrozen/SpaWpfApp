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
            this.Lines = SeparateLines(query.ToLower());
        }

        public List<String> SeparateLines(string query)
        {
            List<String> result = new List<String>();
            string[] lines = Regex.Split(query, "\r\n");
            foreach (var line in lines)
            {
                string[] partsOfLine = Regex.Split(line, ";");
                foreach(var part in partsOfLine)
                {
                    result.Add(part.Trim());
                }
            }
            result.RemoveAll(x=>x == string.Empty);
            return result;
        }
    }
}
