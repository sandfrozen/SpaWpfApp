using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.PkbOld
{
    public interface PkbAPI
    {
        int GetNumberOfLines();

        // ProcTable
        /// <summary>
        /// Inserting procedure into table if not exists and returns index,
        /// if procedure is in table only returns index
        /// if table if full, returns -1
        /// </summary>
        /// <param name="proc">orocedure name</param>
        /// <returns></returns>
        int InsertProc(String proc);
        /// <summary>
        /// Returns procedure index in ProcTable for given procedure name
        /// </summary>
        /// <param name="proc">procedure name</param>
        /// <returns></returns>
        int GetProcIndex(String proc);
        /// <summary>
        /// Returns procedure name for given procedure index in ProcTable
        /// </summary>
        /// <param name="index">procedure index</param>
        /// <returns></returns>
        String GetProcName(int index);
        /// <summary>
        /// Returns number of procedures in PKB
        /// </summary>
        /// <returns></returns>
        int GetNumberOfProcs();
        /// <summary>
        /// Prints ProcTable (1 dimension)
        /// </summary>
        void PrintProcTable();

        // VarTable
        /// <summary>
        /// Inserting variable into table if not exists and returns index,
        /// if variable is in table only returns index
        /// if table if full, returns -1
        /// </summary>
        /// <param name="var">variable name</param>
        /// <returns></returns>
        int InsertVar(String var);
        /// <summary>
        /// Returns variable index in VarTable for given variable name
        /// </summary>
        /// <param name="var">variable name</param>
        /// <returns></returns>
        int GetVarIndex(String var);
        /// <summary>
        /// Returns variable name for given variable index in VarTable
        /// </summary>
        /// <param name="index">variable index</param>
        /// <returns></returns>
        String GetVarName(int index);
        /// <summary>
        /// Returns number of variables in PKB
        /// </summary>
        /// <returns></returns>
        int GetNumberOfVars();
        /// <summary>
        /// Prints VarTable (1 dimension)
        /// </summary>
        void PrintVarTable();

        // CallsTable
        /// <summary>
        /// sets that proc1 calls proc2
        /// </summary>
        /// <param name="proc1">procedure name which calls proc2</param>
        /// <param name="proc2">procedure name which is called by proc1</param>
        void SetCalls(String proc1, String proc2, int p_line_number);
        /// <summary>
        /// returns list of procedures that are calling 'proc'
        /// </summary>
        /// <param name="proc">procedure name</param>
        /// <returns>list of procedures that are calling 'proc'</returns>
        List<String> GetCalls(String proc);
        /// <summary>
        /// returns list of procedures that are called by 'proc'
        /// </summary>
        /// <param name="proc">procedure name</param>
        /// <returns>list of procedures that are called by 'proc'</returns>
        List<String> GetCalled(String proc);

        /// <summary>
        /// returns list of int(calls) which are calling 'proc'
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        List<int> GetCallStmts(String proc);
        
        /// <summary>
        /// returns true if proc1 calls proc2
        /// </summary>
        /// <param name="proc1">procedure name which calls proc2</param>
        /// <param name="proc2">procedure name which is called by proc1</param>
        /// <returns>true if proc1 calls proc2</returns>
        Boolean IsCalls(String proc1, String proc2);
        /// <summary>
        /// Prints CallTable (2 dimension)
        /// </summary>
        void PrintCallsTable();

        // ModifiesTable
        ///<summary>
        ///sets that variable 'var' is modified in 'line' number
        ///</summary>
        void SetModifies(String var, int line);
        ///<summary>
        ///returns list of lines that are modifying variable 'var'
        ///</summary>
        List<int> GetModifies(String var);
        ///<summary>
        ///returns list of vars that are modified in 'line'
        ///</summary>
        List<String> GetModified(int line);
        /// <summary>
        /// returns true if 'var' is modified in 'line'
        /// </summary>
        /// <param name="var">variable name</param>
        /// <param name="line">line number</param>
        /// <returns>true if 'var' is modified in 'line'</returns>
        Boolean IsModified(String var, int line);
        /// <summary>
        /// Prints ModifiesTable (2 dimension)
        /// </summary>
        void PrintModifiesTable();

        // UsesTable
        /// <summary>
        /// sets that 'var' is used in 'line' number
        /// </summary>
        /// <param name="var">variable name</param>
        /// <param name="line">line number</param>
        void SetUses(String var, int line);
        /// <summary>
        /// returns list of lines that are using 'var'
        /// </summary>
        /// <param name="var">variable name</param>
        /// <returns>list of lines that are using 'var'</returns>
        List<int> GetUses(String var);
        /// <summary>
        /// returns list of vars that are used in 'line' number
        /// </summary>
        /// <param name="line">line number</param>
        /// <returns></returns>
        List<String> GetUsed(int line);
        /// <summary>
        /// returns true if 'var' is used in 'line' number
        /// </summary>
        /// <param name="var">variable name</param>
        /// <param name="line">line number</param>
        /// <returns>true if 'var' is used in 'line'</returns>
        Boolean IsUsed(String var, int line);
        /// <summary>
        /// Prints UsesTable (2 dimension)
        /// </summary>
        void PrintUsesTable();

    }
}
