using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp
{
    interface PkbAPI
    {
        // VarTable
        int insertVar(String var);
        int getVarIndex(String var);
        String getVarName(int index);

        // ModifiesTable
        void setModifies(int stmt, String var);
        List<int> getModifies(String var); // returns stmts list which modifies var v
        List<String> getModified(int stmt); // returns list of vars that are modified in stmt
        Boolean isModified(int stmt, String var); // returns true if var is modified in stmt

        // NextTable
        void setNext(int actualStmt, int nextStmt);
        int getNext(int stmt);
        //int getNextS(int stmt);
        int getBefore(int stmt);
        //int getBeforeS(int stmt);
        Boolean isNext(int actualStmt, int nextStmt);
        Boolean isBefore(int beforeStmt, int actualStmt);
        //Boolean isNextS(int stmt1, int stmt2);
        //Boolean isBeforeS(int beforeStmt, int actualStmt);


        //// FollowsTable
        //void setFollows(int stmt1, int stmt2);
        //List<int> getFollows(int stmt);
        ////List<int> getFollowsS(int stmt);
        //List<int> getFollowedBy(int stmt);
        ////List<int> getFollowedByS(int stmt);
        //bool isFollow(int stmt1, int stmt2);
        ////bool isFollowS(int stmt1, int stmt2);

        //// ParentTable
        //void setParent(int stmt1, int stmt2);
        //List<int> getParent(int stmt);
        ////List<int> getParentS(int stmt);
        //List<int> getChildren(int stmt);
        ////List<int> getChildrenS(int stmt);
        //bool isParent(int stmt1, int stmt2);
        ////bool isParentS(int stmt1, int stmt2);
    }
}
