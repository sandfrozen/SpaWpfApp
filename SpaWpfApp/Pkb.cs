using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp
{
    class Pkb : PkbAPI
    {
        private String[] VarTable; // variables
        private int[,] ModifiesTable; // variables / statemtents.numbers
        private int[,] NextTable; // statements.numbers / statements.numbers

        public Pkb(int numberOfVars, int numberOfStmts)
        {
            VarTable = new String[numberOfVars];
            ModifiesTable = new int[numberOfVars,numberOfStmts];
            NextTable = new int[numberOfStmts, numberOfStmts];
        }

        // VarTable
        public int insertVar(String var)
        {
            if ( !VarTable.Contains(var) )
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

        // ModifiesTable
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
            List<String> modified = new List<string>();
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

        // NextTable
        public void setNext(int actualStmt, int nextStmt)
        {
            if(actualStmt < NextTable.GetLength(0) && nextStmt < NextTable.GetLength(1) )
            {
                NextTable[actualStmt, nextStmt] = 1;
            }
        }
        public int getNext(int stmt)
        {
            if( stmt < NextTable.GetLength(0) )
            {
                for(int i = 0; i<NextTable.GetLength(1); i++)
                {
                    if( NextTable[stmt, i] == 1)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        public int getBefore(int stmt)
        {
            if (stmt < NextTable.GetLength(1))
            {
                for (int i = 0; i < NextTable.GetLength(0); i++)
                {
                    if (NextTable[i, stmt] == 1)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        public Boolean isNext(int actualStmt, int nextStmt)
        {
            if (actualStmt < NextTable.GetLength(0) && nextStmt < NextTable.GetLength(1))
            {
                return NextTable[actualStmt, nextStmt] == 1;
            }
            return false;
        }
        public Boolean isBefore(int beforeStmt, int actualStmt)
        {
            if (beforeStmt < NextTable.GetLength(0) && actualStmt < NextTable.GetLength(1))
            {
                return NextTable[beforeStmt, actualStmt] == 1;
            }
            return false;
        }

    }
}
