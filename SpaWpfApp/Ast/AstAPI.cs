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
        /// returns programLine of Parent of stmt or -1 if stmt does't have Parent
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        int GetParent(int p_programLineNumber);

        /// <summary>
        /// returns programLine of direct ParentS of stmt or -1 if stmt doesn't have any ParentS
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<int> GetParentS(int p_programLineNumber);

        /// <summary>
        /// returns all children of stmt or -1 if stmt doesn't have children
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<int> GetChildren(int p_programLineNumber);

        /// <summary>
        /// returns all direct and indirect children of stmt or -1 if stmt doesn't have any childrenS
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<int> GetChildrenS(int p_programLineNumber);




        //Follows, FollowsS
        /// <summary>
        /// returns direct right sibling of stmt or -1 if stmt doesn't have right sibling
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        int GetRightSibling(int p_programLineNumber);

        /// <summary>
        /// returns direct left sibling of stmt or -1 if stmt is the only child
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        int GetLeftSibling(int p_programLineNumber);

        /// <summary>
        /// retrurns list of stmt that Follows stmt or -1 if stmt doesn't have rightSiblingS
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<int> GetRightSiblingS(int p_programLineNumber);

        /// <summary>
        /// returns list of leftSiblingS or -1 if stmt is the only child
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<int> GetLeftSiblingS(int p_programLineNumber);
    }
}
