using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp
{
    class Pkb : PkbAPI
    {
        //TODO: CallsTable, UsesTable
        private String[] ProcTable;   // [proc]
        private String[] VarTable;    // [var ]
        private int[,] CallsTable;    // [proc, stmtLine]
        private int[,] ModifiesTable; // [var , stmtLine]
        private int[,] UsesTable;     // [var , stmtLine]

        public Pkb(int vars, int stmts, int procs)
        {
            VarTable = new String[vars];
            ProcTable = new String[vars];

            CallsTable = new int[procs, stmts];
            ModifiesTable = new int[vars, stmts];
            UsesTable = new int[vars, stmts];
        }

        //NOTE: ProcTable operations
        public int insertProc(String proc)
        {
            if (ProcTable.Contains(proc))
            {
                for (int i = 0; i < ProcTable.Length; i++)
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
                    if (ProcTable[i] == "")
                    {
                        ProcTable[i] = proc;
                        return i;
                    }
                }
            }
            return -1;
        }

        public int getProcIndex(String proc)
        {
            if (ProcTable.Contains(proc))
            {
                for (int i = 0; i < ProcTable.Length; i++)
                {
                    if (ProcTable[i] == proc)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public String getProcName(int index)
        {
            if (index < ProcTable.Length && ProcTable[index] != "")
            {
                return ProcTable[index];
            }
            return null;
        }

        public int getProcTableSize()
        {
            return ProcTable.Length;
        }

        //NOTE: VarTable operations
        public int insertVar(String var)
        {
            if (VarTable.Contains(var))
            {
                for (int i = 0; i < VarTable.Length; i++)
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
                    if (VarTable[i] == "")
                    {
                        VarTable[i] = var;
                        return i;
                    }
                }
            }
            return -1;
        }

        public int getVarIndex(String var)
        {
            if ( VarTable.Contains(var) )
            {
                for (int i = 0; i < VarTable.Length; i++)
                {
                    if (VarTable[i] == var)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public String getVarName(int index)
        {
            if ( index < VarTable.Length && VarTable[index] != "" )
            {
                return VarTable[index];
            }
            return null;
        }

        public int getVarTableSize()
        {
            return VarTable.Length;
        }

        //NOTE: ModifiesTable operations
        public void setModifies(int stmt, String var)
        {
            int varIndex = getVarIndex(var);
            if( varIndex != -1)
            {
                ModifiesTable[varIndex, stmt] = 1;
            }
        }

        public List<int> getModifies(String var)
        {
            List<int> modifies = new List<int>();
            int varIndex = getVarIndex(var);
            if (varIndex != -1)
            { 
                for (int i = 0; i < ModifiesTable.GetLength(1); i++)
                {
                    if(ModifiesTable[varIndex,i] == 1)
                    {
                        modifies.Add(i);
                    }
                }
            }
            return modifies;
        }

        public List<String> getModified(int stmt)
        {
            List<String> modified = new List<String>();
            if( stmt < ModifiesTable.GetLength(1) )
            {
                for (int i = 0; i < ModifiesTable.GetLength(0); i++)
                {
                    if (ModifiesTable[i, stmt] == 1)
                    {
                        String var = getVarName(i);
                        if ( var != null )
                        {
                            modified.Add(var);
                        }
                    }
                }
            }
            return modified;
        }

        public Boolean isModified(int stmt, String var)
        {
            int varIndex = getVarIndex(var);
            if (varIndex != -1 && stmt < ModifiesTable.GetLength(1) )
            {
                if (ModifiesTable[varIndex, stmt] == 1)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
