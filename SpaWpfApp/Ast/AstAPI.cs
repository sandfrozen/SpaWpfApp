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
        /// returns Parent of stmt or null if stmt does't have Parent
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        TNode GetParent(int p_programLineNumber);

        /// <summary>
        /// returns direct Parent and all indirect Parent of stmt or null if stmt doesn't have any ParentS
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<TNode> GetParentS(int p_programLineNumber);

        /// <summary>
        /// returns all children of stmt or null if stmt doesn't have children
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<TNode> GetChildren(int p_programLineNumber);

        /// <summary>
        /// returns all direct and indirect children of stmt or null if stmt doesn't have any childrenS
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<TNode> GetChildrenS(int p_programLineNumber);




        //Follows, FollowsS
        /// <summary>
        /// returns direct right sibling of stmt or null if stmt doesn't have right sibling
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        TNode GetRightSibling(int p_programLineNumber);

        /// <summary>
        /// returns direct left sibling of stmt or null if stmt is the only child
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        TNode GetLeftSibling(int p_programLineNumber);

        /// <summary>
        /// retrurns list of stmt that Follows stmt or null if stmt doesn't have rightSiblingS
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<TNode> GetRightSiblingS(int p_programLineNumber);

        /// <summary>
        /// returns list of leftSiblingS or null if stmt is the only child
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<TNode> GetLeftSiblingS(int p_programLineNumber);
    }
}
