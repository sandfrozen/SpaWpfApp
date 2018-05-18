using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Ast
{
    public interface AstAPI
    {
        //Parent, ParentS
        /// <summary>
        /// returns Parent of p_child or null if p_child does't have Parent
        /// </summary>
        /// <param name="p_child"></param>
        /// <param name="p_father"></param>
        /// <returns></returns>
        TNode GetParent(TNode p_child, string p_father);

        /// <summary>
        /// returns lis of ParentS of p_child or null if p_child does't have ParentS
        /// </summary>
        /// <param name="p_child"></param>
        /// <param name="p_father"></param>
        /// <returns></returns>
        List<TNode> GetParentS(TNode p_child, string p_father);

        /// <summary>
        /// returns all children of stmt or null if stmt doesn't have children
        /// </summary>
        /// <param name="p_father"></param>
        /// <param name="p_child"></param>
        /// <returns></returns>
        List<TNode> GetChildren(TNode p_father, string p_child);

        /// <summary>
        /// returns all childrenS of stmt or null if stmt doesn't have childrenS
        /// </summary>
        /// <param name="p_father"></param>
        /// <param name="p_child"></param>
        /// <returns></returns>
        List<TNode> GetChildrenS(TNode p_father, string p_child);

        Boolean IsParent(int p1, int p2);
        Boolean IsParentS(int p1, int p2);



        ////Follows, FollowsS
        ///// <summary>
        ///// returns direct right sibling of stmt or -1 if stmt doesn't have right sibling
        ///// </summary>
        ///// <param name="p_programLineNumber"></param>
        ///// <returns></returns>
        //int GetRightSibling(int p_programLineNumber);

        ///// <summary>
        ///// returns direct left sibling of stmt or -1 if stmt is the only child
        ///// </summary>
        ///// <param name="p_programLineNumber"></param>
        ///// <returns></returns>
        //int GetLeftSibling(int p_programLineNumber);

        ///// <summary>
        ///// retrurns list of stmt that Follows stmt or -1 if stmt doesn't have rightSiblingS
        ///// </summary>
        ///// <param name="p_programLineNumber"></param>
        ///// <returns></returns>
        //List<int> GetRightSiblingS(int p_programLineNumber);

        ///// <summary>
        ///// returns list of leftSiblingS or -1 if stmt is the only child
        ///// </summary>
        ///// <param name="p_programLineNumber"></param>
        ///// <returns></returns>
        //List<int> GetLeftSiblingS(int p_programLineNumber);

        //Boolean IsFollows(int p1, int p2);
        //Boolean IsFollowsS(int p1, int p2);
    }
}
