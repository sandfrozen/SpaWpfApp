using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp
{
    public class Pkb : PkbAPI
    {
        private int numberOfLines;
        private int numberOfProcs;
        private int numberofVars;

        //TODO: CallsTable, UsesTable
        private String[] ProcTable;   // [proc]
        private String[] VarTable;    // [var ]

        private Boolean[,] CallsTable;    // [proc, proc]
        private Boolean[,] ModifiesTable; // [var , line]
        private Boolean[,] UsesTable;     // [var , line]

        public Pkb(int numberOfLines, int numberOfProcs, int numberOfVars)
        {
            this.numberOfLines = numberOfLines;
            this.numberOfProcs = numberOfProcs;
            this.numberofVars = numberOfVars;

            this.VarTable = new String[numberOfVars];
            this.ProcTable = new String[numberOfProcs];

            this.CallsTable = new Boolean[numberOfProcs, numberOfProcs];
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
            if (ProcTable.Contains(proc))
            {
                for (int i = 0; i < numberOfProcs; i++)
                {
                    if (ProcTable[i] == proc)
                    {
                        return i;
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
                        return i;
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
                        return i;
                    }
                }
            }
            return -1;
        }

        public String GetProcName(int index)
        {
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
                        return i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < VarTable.Length; i++)
                {
                    if (String.IsNullOrEmpty(VarTable[i]))
                    {
                        VarTable[i] = var;
                        return i;
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
                        return i;
                    }
                }
            }
            return -1;
        }

        public String GetVarName(int index)
        {
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
        /// <summary>
        /// sets that proc1 calls proc2
        /// </summary>
        /// <param name="proc1">procedure name which calls proc2</param>
        /// <param name="proc2">procedure name which is called by proc1</param>
        public void SetCalls(String proc1, String proc2)
        {
            int proc1Index = GetProcIndex(proc1);
            int proc2Index = GetProcIndex(proc2);
            if (proc1Index != -1 && proc2Index != -1)
            {
                CallsTable[proc1Index, proc2Index] = true;
            }
        }

        /// <summary>
        /// returns list of procedures that are calling 'proc'
        /// </summary>
        /// <param name="proc">procedure name</param>
        /// <returns>list of procedures that are calling 'proc'</returns>
        public List<String> GetCalls(String proc)
        {
            List<String> calls = new List<String>();
            int procIndex = GetProcIndex(proc);
            if (procIndex != -1)
            {
                for (int i = 0; i < numberOfProcs; i++)
                {
                    if (CallsTable[i, procIndex])
                    {
                        calls.Add(GetProcName(i));
                    }
                }
            }
            return calls;
        }

        /// <summary>
        /// returns list of procedures that are called by 'proc'
        /// </summary>
        /// <param name="proc">procedure name</param>
        /// <returns>list of procedures that are called by 'proc'</returns>
        public List<String> GetCalled(String proc)
        {
            List<String> called = new List<String>();
            int procIndex = GetProcIndex(proc);
            if (procIndex != -1)
            {
                for (int i = 0; i < numberOfProcs; i++)
                {
                    if (CallsTable[procIndex, i])
                    {
                        called.Add(GetProcName(i));
                    }
                }
            }
            return called;
        }

        /// <summary>
        /// returns true if proc1 calls proc2
        /// </summary>
        /// <param name="proc1">procedure name which calls proc2</param>
        /// <param name="proc2">procedure name which is called by proc1</param>
        /// <returns>true if proc1 calls proc2</returns>
        public bool IsCalls(string proc1, string proc2)
        {
            int proc1Index = GetProcIndex(proc1);
            int proc2Index = GetProcIndex(proc2);
            if (proc1Index != -1 && proc2Index != -1)
            {
                return CallsTable[proc1Index, proc2Index];
            }
            return false;
        }

        //NOTE: ModifiesTable operations
        ///<summary>
        ///sets that variable 'var' is modified in 'line' number
        ///</summary>
        public void SetModifies(String var, int line)
        {
            int varIndex = GetVarIndex(var);
            if (varIndex != -1)
            {
                ModifiesTable[varIndex, line] = true;
            }
        }

        ///<summary>
        ///returns list of lines that are modifying variable 'var'
        ///</summary>
        public List<int> GetModifies(String var)
        {
            List<int> lines = new List<int>();
            int varIndex = GetVarIndex(var);
            if (varIndex != -1)
            {
                for (int i = 0; i < numberOfLines; i++)
                {
                    if (ModifiesTable[varIndex, i])
                    {
                        lines.Add(i);
                    }
                }
            }
            return lines;
        }

        ///<summary>
        ///returns list of vars that are modified in 'line'
        ///</summary>
        public List<String> GetModified(int line)
        {
            List<String> vars = new List<String>();
            if (line > -1 && line < numberOfLines)
            {
                for (int i = 0; i < numberofVars; i++)
                {
                    if (ModifiesTable[i, line])
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

        /// <summary>
        /// returns true if 'var' is modified in 'line'
        /// </summary>
        /// <param name="var">variable name</param>
        /// <param name="line">line number</param>
        /// <returns>true if 'var' is modified in 'line'</returns>
        public Boolean IsModified(String var, int line)
        {
            int varIndex = GetVarIndex(var);
            if (varIndex != -1 && line > -1 && line < numberOfLines)
            {
                return ModifiesTable[varIndex, line];
            }
            return false;
        }

        //NOTE: UsesTable operations
        /// <summary>
        /// sets that 'var' is used in 'line'
        /// </summary>
        /// <param name="var">variable name</param>
        /// <param name="line">line number</param>
        public void SetUses(string var, int line)
        {
            int varIndex = GetVarIndex(var);
            if (varIndex != -1)
            {
                UsesTable[varIndex, line] = true;
            }
        }

        /// <summary>
        /// returns list of lines that are using 'var'
        /// </summary>
        /// <param name="var">variable name</param>
        /// <returns>list of lines that are using 'var'</returns>
        public List<int> GetUses(string var)
        {
            List<int> lines = new List<int>();
            int varIndex = GetVarIndex(var);
            if (varIndex != -1)
            {
                for (int i = 0; i < numberOfLines; i++)
                {
                    if (UsesTable[varIndex, i])
                    {
                        lines.Add(i);
                    }
                }
            }
            return lines;
        }

        /// <summary>
        /// returns list of vars that are used in 'line'
        /// </summary>
        /// <param name="line">line number</param>
        /// <returns></returns>
        public List<string> GetUsed(int line)
        {
            List<String> vars = new List<String>();
            if (line > -1 && line < numberOfLines)
            {
                for (int i = 0; i < numberofVars; i++)
                {
                    if (UsesTable[i, line])
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

        /// <summary>
        /// returns true if 'var' is used in 'line'
        /// </summary>
        /// <param name="var">variable name</param>
        /// <param name="line">line number</param>
        /// <returns>true if 'var' is used in 'line'</returns>
        public bool IsUsed(string var, int line)
        {
            int varIndex = GetVarIndex(var);
            if (varIndex != -1 && line > -1 && line < numberOfLines)
            {
                return UsesTable[varIndex, line];
            }
            return false;
        }
    }
}
