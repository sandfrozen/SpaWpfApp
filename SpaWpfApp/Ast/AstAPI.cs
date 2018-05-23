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
        List<TNode> GetParentX(TNode p_child, string p_father);

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
        List<TNode> GetChildrenX(TNode p_father, string p_child);

        Boolean IsParent(int p1, int p2);
        Boolean IsParentX(int p1, int p2);



        //Follows, FollowsS
        /// <summary>
        /// returns direct right sibling of stmt or -1 if stmt doesn't have right sibling
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        TNode GetRightSibling(TNode p_from, string p_to);

        /// <summary>
        /// returns direct left sibling of stmt or -1 if stmt is the only child
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        TNode GetLeftSibling(TNode p_to, string p_from);

        /// <summary>
        /// retrurns list of stmt that Follows stmt or -1 if stmt doesn't have rightSiblingS
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<TNode> GetRightSiblingX(TNode p_from, string p_to);

        /// <summary>
        /// returns list of leftSiblingS or -1 if stmt is the only child
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<TNode> GetLeftSiblingX(TNode p_to, string p_from);

        Boolean IsFollows(int p1, int p2);
        Boolean IsFollowsX(int p1, int p2);
    }
}
