using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp
{
    interface PkbAPI
    {
        int GetNumberOfLines();

        // ProcTable
        int InsertProc(String proc);
        int GetProcIndex(String proc);
        String GetProcName(int index);
        int GetNumberOfProcs();

        // VarTable
        int InsertVar(String var);
        int GetVarIndex(String var);
        String GetVarName(int index);
        int GetNumberOfVars();

        // CallsTable
        void SetCalls(String proc1, String proc2);  // sets that 'proc1' calls 'proc2'
        List<String> GetCalls(String proc);         // returns list of procedures that are calling 'proc'
        List<String> GetCalled(String proc);        // returns list of procedures that are called by 'proc'
        Boolean IsCalls(String proc1, String proc2);// returns true if proc1 calls proc2

        // ModifiesTable
        void SetModifies(String var, int line);     // sets that 'var' is modified in 'line'
        List<int> GetModifies(String var);          // returns list of lines that are modifying 'var'
        List<String> GetModified(int line);         // returns list of vars  that are modified in 'line'
        Boolean IsModified(String var, int line);   // returns true if 'var' is modified in 'line'

        // UsesTable
        void SetUses(String var, int line);         // sets that 'var' is used in 'line'
        List<int> GetUses(String var);              // returns list of lines that are using 'var'
        List<String> GetUsed(int line);             // returns list of vars  that are used in 'line'
        Boolean IsUsed(String var, int line);       // returns true if 'var' is used in 'line'

    }
}
