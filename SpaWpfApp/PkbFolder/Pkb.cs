using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SpaWpfApp.PkbFolder
{
    internal class Pkb : PkbAPI
    {

        private int numberOfLines = -1;
        private List<string> ProcTable;   // [proc]
        private List<string> VarTable;    // [var ]

        private List<List<int>> CallsTable;    // [proc, proc]
        private List<ListContainer> ModifiesTable; // [var , line]
        private List<ListContainer> UsesTable;   // [var , line]

        public Pkb()
        {
            ProcTable = new List<string>();
            VarTable = new List<string>();

            CallsTable = new List<List<int>>();

            ModifiesTable = new List<ListContainer> ();
            UsesTable = new List<ListContainer> ();
        }

        public int GetNumberOfLines()
        {
            return numberOfLines;
        }

        #region ProcTable operations
        public void InsertProc(String proc)
        {
            if (!ProcTable.Contains(proc))
            {
                ProcTable.Add(proc);
                IncreaseCallsTable(proc);
            }
        }
        private void IncreaseCallsTable(string proc)
        {
            CallsTable.Insert(CallsTable.Count, new List<int>());
            int size = CallsTable.Count;

            for (int i = 0; i < CallsTable.Count; i++)
            {
                while (CallsTable.ElementAt(i).Count < size)
                {
                    CallsTable.ElementAt(i).Add(-1);
                }
            }
        }
        public int GetProcIndex(String proc)
        {
            if (ProcTable.Contains(proc))
            {
                for (int i = 0; i < ProcTable.Count; i++)
                {
                    if (ProcTable.ElementAt(i) == proc)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        public string GetProcName(int index)
        {
            if (index < ProcTable.Count)
            {
                return ProcTable.ElementAt(index);
            }
            return null;
        }
        public int GetNumberOfProcs()
        {
            return ProcTable.Count;
        }
        #endregion

        #region VarTable operations
        public void InsertVar(String var)
        {
            if (!VarTable.Contains(var))
            {
                VarTable.Add(var);
            }
        }
        public int GetVarIndex(String var)
        {
            if (VarTable.Contains(var))
            {
                for (int i = 0; i < VarTable.Count; i++)
                {
                    if (VarTable.ElementAt(i) == var)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        public String GetVarName(int index)
        {
            if (index < VarTable.Count)
            {
                return VarTable.ElementAt(index);
            }
            return null;
        }
        public int GetNumberOfVars()
        {
            return VarTable.Count;
        }
        #endregion

        public void InsertVarToModifiesAndUses(int varIndex, string varName)
        {
            ModifiesTable.Insert(ModifiesTable.Count, new ListContainer(varIndex, varName));
            UsesTable.Insert(UsesTable.Count, new ListContainer(varIndex, varName));

            int size = ModifiesTable.ElementAt(0).LinesCount();

            for (int i = 0; i < ModifiesTable.Count; i++)
            {
                while (ModifiesTable.ElementAt(i).LinesCount() < size)
                {
                    ModifiesTable.ElementAt(i).Add(false);
                    UsesTable.ElementAt(i).Add(false);
                }
            }

            if( ModifiesTable.Count == 1 )
            {

            }
        }

        public void InsertLineToModifiesAndUses(int maxLines)
        {
            //int size = ModifiesTable.ElementAt(0).Count;

            for (int i = 0; i < ModifiesTable.Count; i++)
            {
                while (ModifiesTable.ElementAt(i).LinesCount() < maxLines)
                {
                    ModifiesTable.ElementAt(i).Add(false);
                    UsesTable.ElementAt(i).Add(false);
                }
            }
        }


















        //NOTE: CallsTable operations
        public void SetCalls(String proc1, String proc2, int line)
        {
            int proc1Index = GetProcIndex(proc1);
            int proc2Index = GetProcIndex(proc2);
            if (proc1Index > -1 && proc2Index > -1)
            {
                CallsTable.ElementAt(proc1Index).Insert(proc2Index, line);
            }
        }
        public List<String> GetCalls(String proc)
        {
            List<String> calls = new List<String>();
            int procIndex = GetProcIndex(proc) - 1;
            if (procIndex != -1)
            {
                for (int i = 0; i < ProcTable.Count; i++)
                {
                    //if (CallsTable[i, procIndex] != null)
                    //{
                    //    calls.Add(GetProcName(i + 1));
                    //}
                }
            }
            return calls;
        }
        public List<String> GetCalled(String proc)
        {
            List<String> called = new List<String>();
            //int procIndex = GetProcIndex(proc)-1;
            //if (procIndex != -1)
            //{
            //    for (int i = 0; i < numberOfProcs; i++)
            //    {
            //        if (CallsTable[procIndex, i])
            //        {
            //            called.Add(GetProcName(i+1));
            //        }
            //    }
            //}
            return called;
        }
        public List<int> GetCallStmts(string proc)
        {
            List<int> calls = new List<int>();
            int procIndex = GetProcIndex(proc) - 1;
            if (procIndex != -1)
            {
                for (int i = 0; i < ProcTable.Count; i++)
                {
                    //if (CallsTable[i, procIndex] != null)
                    //{
                    //    foreach(var j in CallsTable[i, procIndex])
                    //    {
                    //        if (!calls.Contains(j))
                    //        {
                    //            calls.Add(j);
                    //        }
                    //    }
                    //}
                }
            }
            return calls;
        }
        public bool IsCalls(string proc1, string proc2)
        {
            int proc1Index = GetProcIndex(proc1) - 1;
            int proc2Index = GetProcIndex(proc2) - 1;
            if (proc1Index > -1 && proc2Index > -1)
            {
                //return CallsTable[proc1Index, proc2Index] != null ? true : false;
            }
            return false;
        }

        //NOTE: ModifiesTable operations
        public void SetModifies(String var, int line)
        {
            --line;
            int varIndex = GetVarIndex(var) - 1;
            if (varIndex != -1 && line > -1 && line < numberOfLines)
            {
                //ModifiesTable[varIndex, line] = true;
            }
        }
        public List<int> GetModifies(String var)
        {
            List<int> lines = new List<int>();
            int varIndex = GetVarIndex(var) - 1;
            if (varIndex != -1)
            {
                for (int i = 0; i < numberOfLines; i++)
                {
                    //if (ModifiesTable[varIndex, i])
                    //{
                    //    lines.Add(i + 1);
                    //}
                }
            }
            return lines;
        }
        public List<String> GetModified(int line)
        {
            --line;
            List<String> vars = new List<String>();
            if (line > -1 && line < numberOfLines)
            {
                for (int i = 0; i < VarTable.Count; i++)
                {
                    //if (ModifiesTable[i, line])
                    //{
                    //    String var = GetVarName(i + 1);
                    //    if (var != null)
                    //    {
                    //        vars.Add(var);
                    //    }
                    //}
                }
            }
            return vars;
        }
        public Boolean IsModified(String var, int line)
        {
            --line;
            int varIndex = GetVarIndex(var) - 1;
            if (varIndex != -1 && line > -1 && line < numberOfLines)
            {
                //return ModifiesTable[varIndex, line];
            }
            return false;
        }

        //NOTE: UsesTable operations
        public void SetUses(string var, int line)
        {
            --line;
            int varIndex = GetVarIndex(var) - 1;
            if (varIndex != -1 && line > 0 && line < numberOfLines)
            {
                //UsesTable[varIndex, line] = true;
            }
        }
        public List<int> GetUses(string var)
        {
            List<int> lines = new List<int>();
            int varIndex = GetVarIndex(var) - 1;
            if (varIndex > -1)
            {
                for (int i = 0; i < numberOfLines; i++)
                {
                    //if (UsesTable[varIndex, i])
                    //{
                    //    lines.Add(i + 1);
                    //}
                }
            }
            return lines;
        }
        public List<string> GetUsed(int line)
        {
            --line;
            List<String> vars = new List<String>();
            if (line > -1 && line < numberOfLines)
            {
                for (int i = 0; i < VarTable.Count; i++)
                {
                    //if (UsesTable[i, line])
                    //{
                    //    String var = GetVarName(i + 1);
                    //    if (var != null)
                    //    {
                    //        vars.Add(var);
                    //    }
                    //}
                }
            }
            return vars;
        }
        public bool IsUsed(string var, int line)
        {
            --line;
            int varIndex = GetVarIndex(var) - 1;
            if (varIndex > -1 && line > -1 && line < numberOfLines)
            {
                //return UsesTable[varIndex, line];
            }
            return false;
        }
    }
}
