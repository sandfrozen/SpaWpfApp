using SpaWpfApp.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    public struct DeclarationEntry
    {
        public string key { get; set; }
        public string value { get; set; }
        public Boolean wasDeterminated { get; set; }

        public DeclarationEntry(string p_key, string p_value)
        {
            this.key = p_key;
            this.value = p_value;
            wasDeterminated = false;
        }
    }

    public class QueryResult
    {
        public DeclarationEntry[] declarationsTable { get; set; }
        public List<TNode[]> resultTableList { get; set; } = new List<TNode[]>();

        public Boolean? resultBoolean { get; set; } = null;

        public Boolean resultIsBoolean
        {
            get
            {
                return queryPreProcessor.ReturnTypeIsBoolean();
            }
        }

        QueryPreProcessor queryPreProcessor = QueryPreProcessor.GetInstance();

        private static QueryResult instance;

        public static QueryResult GetInstance()
        {
            if (instance == null)
            {
                instance = new QueryResult();
            }
            return instance;
        }

        public void Init()
        {
            resultTableList.Clear();
            resultBoolean = null;

            int length = queryPreProcessor.declarationsList.Count();
            int i;

            if(length > 0)
            {
                i = 0;
                declarationsTable = new DeclarationEntry[length];
                foreach(var declaration in queryPreProcessor.declarationsList)
                {
                    declarationsTable[i++] = new DeclarationEntry(declaration.Key, declaration.Value);
                }
            }
            else
            {
                declarationsTable = null;
            }
        }

        internal bool DeclarationsExists()
        {
            return declarationsTable != null ? true : false;
        }

        internal bool DeclarationWasDeterminated(string argument)
        {
            argument = EnsureWithoutDot(argument);
            return declarationsTable.Where(x => x.key == argument).Select(p => p.wasDeterminated).FirstOrDefault();
        }

        internal List<TNode> GetNodes(string argument)
        {
            List<TNode> result = new List<TNode>();
            argument = EnsureWithoutDot(argument);
            int indexOfDeclaration = FindIndexOfDeclaration(argument);
            foreach(var resultTable in resultTableList)
            {
                result.Add(resultTable[indexOfDeclaration]);
            }

            return result.Count() > 0 ? result : null;
        }

        private string EnsureWithoutDot(string right)
        {
            return right.Contains('.') ? right.Substring(0, right.IndexOf('.')) : right;
        }
        public int FindIndexOfDeclaration(string argument)
        {
            for(int i=0; i<declarationsTable.Length; i++)
            {
                if(declarationsTable[i].key == argument)
                {
                    return i;
                }
            }

            return -1;
        }

        internal bool HasRecords()
        {
            return resultTableList.Any();
        }

        internal List<TNode[]> GetResultTableList()
        {
            return resultTableList;
        }

        internal void ClearResultTableList()
        {
            this.resultTableList.Clear();
        }

        internal void UpdateResutTableList(List<TNode[]> newResultTableList)
        {
            this.resultTableList = newResultTableList;
        }

        internal void SetDeclarationWasDeterminated(string argument)
        {
            for(int i=0; i<declarationsTable.Length; i++)
            {
                if(declarationsTable[i].key == argument)
                {
                    declarationsTable[i].wasDeterminated = true;
                }
            }
        }
    }
}
