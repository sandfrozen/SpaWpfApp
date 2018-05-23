using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SpaWpfApp.PkbOld
{
    public class Pkb : PkbAPI
    {
        private int numberOfLines;
        private int numberOfProcs;
        private int numberofVars;

        //TODO: CallsTable, UsesTable
        private String[] ProcTable;   // [proc]
        private String[] VarTable;    // [var ]

        private List<int>[,] CallsTable;    // [proc, proc]
        private Boolean[,] ModifiesTable; // [var , line]
        private Boolean[,] UsesTable;   // [var , line]

        public Pkb(int numberOfLines, int numberOfProcs, int numberOfVars)
        {
            this.numberOfLines = numberOfLines;
            this.numberOfProcs = numberOfProcs;
            this.numberofVars = numberOfVars;

            this.VarTable = new String[numberOfVars];
            this.ProcTable = new String[numberOfProcs];

            this.CallsTable = new List<int>[numberOfProcs, numberOfProcs];
            this.ModifiesTable = new Boolean[numberOfVars, numberOfLines];
            this.UsesTable = new Boolean[numberOfVars, numberOfLines];
        }

        public int GetNumberOfLines()
        {
            return numberOfLines;
        }

        //NOTE: ProcTable operations
        public int InsertProc(String proc)
        {
            Trace.WriteLine("==================== " + proc);
            if (ProcTable.Contains(proc))
            {
                for (int i = 0; i < numberOfProcs; i++)
                {
                    if (ProcTable[i] == proc)
                    {
                        return i + 1;
                    }
                }
            }
            else
            {
                for (int i = 0; i < ProcTable.Length; i++)
                {
                    if (String.IsNullOrEmpty(ProcTable[i]))
                    {
                        ProcTable[i] = proc;
                        return i + 1;
                    }
                }
            }
            return -1;
        }
        public int GetProcIndex(String proc)
        {
            if (ProcTable.Contains(proc))
            {
                for (int i = 0; i < numberOfProcs; i++)
                {
                    if (ProcTable[i] == proc)
                    {
                        return i + 1;
                    }
                }
            }
            return -1;
        }
        public String GetProcName(int index)
        {
            --index;
            if (index < numberOfProcs && ProcTable[index] != "")
            {
                return ProcTable[index];
            }
            return null;
        }
        public int GetNumberOfProcs()
        {
            return numberOfProcs;
        }

        //NOTE: VarTable operations
        public int InsertVar(String var)
        {
            if (VarTable.Contains(var))
            {
                for (int i = 0; i < numberofVars; i++)
                {
                    if (VarTable[i] == var)
                    {
                        return i + 1;
                    }
                }
            }
            else
            {
                for (int i = 0; i < numberofVars; i++)
                {
                    if (String.IsNullOrEmpty(VarTable[i]))
                    {
                        VarTable[i] = var;
                        return i + 1;
                    }
                }
            }
            return -1;
        }
        public int GetVarIndex(String var)
        {
            if (VarTable.Contains(var))
            {
                for (int i = 0; i < numberofVars; i++)
                {
                    if (VarTable[i] == var)
                    {
                        return i + 1;
                    }
                }
            }
            return -1;
        }
        public String GetVarName(int index)
        {
            --index;
            if (index < numberofVars && VarTable[index] != "")
            {
                return VarTable[index];
            }
            return null;
        }
        public int GetNumberOfVars()
        {
            return numberofVars;
        }

        //NOTE: CallsTable operations
        public void SetCalls(String proc1, String proc2, int p_line_number)
        {
            int proc1Index = GetProcIndex(proc1) - 1;
            int proc2Index = GetProcIndex(proc2) - 1;
            if (proc1Index > -1 && proc2Index > -1)
            {
                if (CallsTable[proc1Index, proc2Index] is null)
                {
                    CallsTable[proc1Index, proc2Index] = new List<int>();
                }
                CallsTable[proc1Index, proc2Index].Add(p_line_number);
            }
        }
        public List<String> GetCalls(String proc)
        {
            List<String> calls = new List<String>();
            int procIndex = GetProcIndex(proc) - 1;
            if (procIndex != -1)
            {
                for (int i = 0; i < numberOfProcs; i++)
                {
                    if (CallsTable[i, procIndex] != null)
                    {
                        calls.Add(GetProcName(i + 1));
                    }
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
                for (int i = 0; i < numberOfProcs; i++)
                {
                    if (CallsTable[i, procIndex] != null)
                    {
                        foreach(var j in CallsTable[i, procIndex])
                        {
                            if (!calls.Contains(j))
                            {
                                calls.Add(j);
                            }
                        }
                    }
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
                return CallsTable[proc1Index, proc2Index] != null ? true : false;
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
                ModifiesTable[varIndex, line] = true;
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
                    if (ModifiesTable[varIndex, i])
                    {
                        lines.Add(i + 1);
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
                for (int i = 0; i < numberofVars; i++)
                {
                    if (ModifiesTable[i, line])
                    {
                        String var = GetVarName(i + 1);
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
            int varIndex = GetVarIndex(var) - 1;
            if (varIndex != -1 && line > -1 && line < numberOfLines)
            {
                return ModifiesTable[varIndex, line];
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
                UsesTable[varIndex, line] = true;
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
                    if (UsesTable[varIndex, i])
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
                for (int i = 0; i < numberofVars; i++)
                {
                    if (UsesTable[i, line])
                    {
                        String var = GetVarName(i + 1);
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
            int varIndex = GetVarIndex(var) - 1;
            if (varIndex > -1 && line > -1 && line < numberOfLines)
            {
                return UsesTable[varIndex, line];
            }
            return false;
        }

        public void PrintProcTable()
        {
            Trace.WriteLine("- - - - - - - - -");
            Trace.WriteLine("ProcTable");
            for (int i = 1; i <= GetNumberOfProcs(); i++)
            {
                Trace.WriteLine(i + " " + GetProcName(i));
            }
        }

        public void PrintVarTable()
        {
            Trace.WriteLine("- - - - - - - - -");
            Trace.WriteLine("VarTable");
            for (int i = 1; i <= GetNumberOfProcs(); i++)
            {
                Trace.WriteLine(i + " " + GetVarName(i));
            }
        }

        public void PrintCallsTable()
        {
            Trace.WriteLine("- - - - - - - - -");
            Trace.WriteLine("CallsTable");
            Trace.Write("    ");
            for (int i = 1; i <= GetNumberOfProcs(); i++)
            {
                Trace.Write(string.Format("{0,-3}", i.ToString()));
            }
            Trace.Write("\n");
            for (int i = 1; i <= GetNumberOfProcs(); i++)
            {
                Trace.Write(string.Format("{0,-3} ", i.ToString()));
                for (int j = 1; j <= GetNumberOfProcs(); j++)
                {
                    Trace.Write(string.Format("{0,-3}", (IsCalls(GetProcName(i), GetProcName(j)) ? "1" : "0")));
                }
                Trace.Write("\n");
            }
        }

        public void PrintModifiesTable()
        {
            Trace.WriteLine("- - - - - - - - -");
            Trace.WriteLine("ModifiesTable");
            for (int i = 1; i <= GetNumberOfVars(); i++)
            {
                for (int j = 1; j <= GetNumberOfLines(); j++)
                {
                    Trace.Write(IsModified(GetVarName(i), j) ? "1 " : "0 ");
                }
                Trace.Write("\n");
            }
        }

        public void PrintUsesTable()
        {
            Trace.WriteLine("- - - - - - - - -");
            Trace.WriteLine("UsesTable");
            for (int i = 1; i <= GetNumberOfVars(); i++)
            {
                for (int j = 1; j <= GetNumberOfLines(); j++)
                {
                    Trace.Write(IsUsed(GetVarName(i), j) ? "1 " : "0 ");
                }
                Trace.Write("\n");
            }
        }

    }
}
