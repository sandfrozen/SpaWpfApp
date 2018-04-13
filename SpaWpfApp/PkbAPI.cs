using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp
{
    interface PkbAPI
    {
        // ProcTable
        int insertProc(String proc);
        int getProcIndex(String proc);
        String getProcName(int index);
        int getProcTableSize();

        // VarTable
        int insertVar(String var);
        int getVarIndex(String var);
        String getVarName(int index);
        int getVarTableSize();

        // ModifiesTable
        void setModifies(int stmt, String var);
        List<int> getModifies(String var); // returns stmts list which modifies var v
        List<String> getModified(int stmt); // returns list of vars that are modified in stmt
        Boolean isModified(int stmt, String var); // returns true if var is modified in stmt


    }
}
