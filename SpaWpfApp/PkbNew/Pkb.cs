using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SpaWpfApp.PkbNew
{
    internal class Pkb : PkbAPI
    {

        public int numberOfLines
        {
            get
            {
                return ModifiesTable.Count != 0 ? ModifiesTable.ElementAt(0).LinesCount() : -1;
            }
        }
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

            ModifiesTable = new List<ListContainer>();
            UsesTable = new List<ListContainer>();
        }

        public void PrintModifiesTable()
        {
            for (int i = 0; i < VarTable.Count; i++)
            {
                Trace.Write(GetVarName(i) + "  ");
                for (int j = 0; j < numberOfLines; j++)
                {
                    Trace.Write(ModifiesTable.ElementAt(i).ValueAt(j) ? "1 " : "0 ");
                }
                Trace.WriteLine("");
            }
        }

        public void PrintUsesTable()
        {
            for (int i = 0; i < VarTable.Count; i++)
            {
                Trace.Write(GetVarName(i) + "  ");
                for (int j = 0; j < numberOfLines; j++)
                {
                    Trace.Write(UsesTable.ElementAt(i).ValueAt(j) ? "1 " : "0 ");
                }
                Trace.WriteLine("");
            }
        }

        public void PrintCallsTable()
        {
            for (int i = 0; i < ProcTable.Count; i++)
            {
                for (int j = 0; j < ProcTable.Count; j++)
                {
                    Trace.Write(CallsTable.ElementAt(i).ElementAt(j) + " ");
                }
                Trace.WriteLine("");
            }
        }

        public int GetNumberOfLines()
        {
            return numberOfLines;
        }

        #region ProcTable
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
            return ProcTable.Contains(proc) ? ProcTable.IndexOf(proc) : -1;
        }

        public string GetProcName(int index)
        {
            return index > -1 && index < ProcTable.Count ? ProcTable.ElementAt(index) : null;
        }

        public int GetNumberOfProcs()
        {
            return ProcTable.Count;
        }
        #endregion

        #region VarTable
        public void InsertVar(string var, int currentLine)
        {
            if (!VarTable.Contains(var))
            {
                VarTable.Add(var);
                InsertVarToModifiesAndUses(VarTable.IndexOf(var), var);
            }
            InsertNewLines(currentLine);
        }

        private void InsertNewLines(int currentLine)
        {
            for (int i = 0; i < ModifiesTable.Count; i++)
            {
                while (ModifiesTable.ElementAt(i).LinesCount() < currentLine)
                {
                    ModifiesTable.ElementAt(i).Add(false);
                    UsesTable.ElementAt(i).Add(false);
                }
            }
        }

        private void InsertVarToModifiesAndUses(int varIndex, string varName)
        {
            ModifiesTable.Insert(ModifiesTable.Count, new ListContainer(varIndex, varName));
            UsesTable.Insert(UsesTable.Count, new ListContainer(varIndex, varName));
        }

        public int GetVarIndex(String var)
        {
            return VarTable.Contains(var) ? VarTable.IndexOf(var) : -1;
        }

        public String GetVarName(int index)
        {
            return index > -1 && index < VarTable.Count ? VarTable.ElementAt(index) : null;
        }

        public int GetNumberOfVars()
        {
            return VarTable.Count;
        }
        #endregion

        #region CallsTable
        public void SetCalls(String proc1, String proc2, int line)
        {
            int proc1Index = GetProcIndex(proc1);
            int proc2Index = GetProcIndex(proc2);
            if (proc1Index > -1 && proc2Index > -1)
            {
                CallsTable.ElementAt(proc1Index)[proc2Index] = line;
            }
        }

        public List<String> GetCalls(String proc)
        {
            List<String> calls = new List<String>();
            int procIndex = GetProcIndex(proc);
            if (procIndex != -1)
            {
                for (int i = 0; i < ProcTable.Count; i++)
                {
                    if (CallsTable.ElementAt(i).ElementAt(procIndex) != -1)
                    {
                        calls.Add(GetProcName(i));
                    }
                }
            }
            return calls;
        }

        public List<String> GetCalled(String proc)
        {
            List<String> called = new List<String>();
            int procIndex = GetProcIndex(proc);
            if (procIndex != -1)
            {
                for (int i = 0; i < ProcTable.Count; i++)
                {
                    if (CallsTable.ElementAt(procIndex).ElementAt(i) != -1)
                    {
                        called.Add(GetProcName(i));
                    }
                }
            }
            return called;
        }

        public List<int> GetCallStmts(string proc)
        {
            List<int> calls = new List<int>();
            int procIndex = GetProcIndex(proc);
            if (procIndex != -1)
            {
                for (int i = 0; i < ProcTable.Count; i++)
                {
                    int value = CallsTable.ElementAt(i).ElementAt(procIndex);
                    if (value != -1)
                    {
                        calls.Add(value);
                    }
                }
            }
            return calls;
        }

        public int IsCalls(string proc1, string proc2)
        {
            int proc1Index = GetProcIndex(proc1);
            int proc2Index = GetProcIndex(proc2);
            if (proc1Index > -1 && proc2Index > -1)
            {
                return CallsTable.ElementAt(proc1Index).ElementAt(proc2Index);
            }
            return -1;
        }
        #endregion

        #region ModifiesTable
        public void SetModifies(String var, int line)
        {
            --line;
            int varIndex = GetVarIndex(var);
            if (varIndex != -1 && line > -1 && line < numberOfLines)
            {
                ModifiesTable.ElementAt(varIndex).Lines[line] = true;
            }
        }
        public List<int> GetModifies(String var)
        {
            List<int> lines = new List<int>();
            int varIndex = GetVarIndex(var);
            if (varIndex != -1)
            {
                for (int i = 0; i < numberOfLines; i++)
                {
                    if (ModifiesTable.ElementAt(varIndex).Lines[i])
                    {
                        lines.Add(i+1);
                    }
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
                    if (ModifiesTable.ElementAt(i).Lines[line])
                    {
                        String var = GetVarName(i);
                        if (var != null)
                        {
                            vars.Add(var);
                        }
                    }
                }
            }
            return vars;
        }

        public Boolean IsModified(String var, int line)
        {
            --line;
            int varIndex = GetVarIndex(var);
            if (varIndex != -1 && line > -1 && line < numberOfLines)
            {
                return ModifiesTable.ElementAt(varIndex).Lines[line];
            }
            return false;
        }
        #endregion

        #region UsesTable
        public void SetUses(string var, int line)
        {
            --line;
            int varIndex = GetVarIndex(var);
            if (varIndex != -1 && line > -1 && line < numberOfLines)
            {
                UsesTable.ElementAt(varIndex).Lines[line] = true;
            }
        }

        public List<int> GetUses(string var)
        {
            List<int> lines = new List<int>();
            int varIndex = GetVarIndex(var);
            if (varIndex > -1)
            {
                for (int i = 0; i < numberOfLines; i++)
                {
                    if (UsesTable.ElementAt(varIndex).Lines[i])
                    {
                        lines.Add(i + 1);
                    }
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
                    if (UsesTable.ElementAt(i).Lines[line])
                    {
                        String var = GetVarName(i);
                        if (var != null)
                        {
                            vars.Add(var);
                        }
                    }
                }
            }
            return vars;
        }

        public bool IsUsed(string var, int line)
        {
            --line;
            int varIndex = GetVarIndex(var);
            if (varIndex > -1 && line > -1 && line < numberOfLines)
            {
                return UsesTable.ElementAt(varIndex).Lines[line];
            }
            return false;
        }
        #endregion
    }
}
