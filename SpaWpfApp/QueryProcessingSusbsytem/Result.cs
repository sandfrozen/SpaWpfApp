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

    public class Result
    {
        public DeclarationEntry[] declarationsTable { get; set; }
        public List<TNode[]> ResultTableList { get; set; } = new List<TNode[]>();

        public Boolean? ResultBoolean { get; set; } = null;

        private static Result instance;
        public static Result GetInstance()
        {
            if (instance == null)
            {
                instance = new Result();
            }
            return instance;
        }

        public void Init()
        {
            ResultTableList.Clear();
            ResultBoolean = null;

            int length = QueryPreProcessor.GetInstance().declarationsList.Count();
            int i;

            if(length > 0)
            {
                i = 0;
                declarationsTable = new DeclarationEntry[length];
                foreach(var declaration in QueryPreProcessor.GetInstance().declarationsList)
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
            return declarationsTable.Where(x => x.key == argument).Select(p => p.wasDeterminated).FirstOrDefault();
        }

        internal List<TNode> GetNodes(string argument)
        {
            List<TNode> result = new List<TNode>();
            int indexOfDeclaration = FindIndexOfDeclaration(argument);
            foreach(var resultTable in ResultTableList)
            {
                result.Add(resultTable[indexOfDeclaration]);
            }

            return result.Count() > 0 ? result : null;
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
            return ResultTableList.Any();
        }

        internal List<TNode[]> GetResultTableList()
        {
            return ResultTableList;
        }

        internal void ClearResultTableList()
        {
            this.ResultTableList.Clear();
        }

        internal void UpdateResutTableList(List<TNode[]> newResultTableList)
        {
            this.ResultTableList = newResultTableList;
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
