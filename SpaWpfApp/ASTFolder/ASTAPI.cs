using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.ASTFolder
{
    public interface ASTAPI
    {
        //Parent, ParentS
        TNode GetParent(int p_programLineNumber);
        List<TNode> GetParentS(int p_programLineNumber);
        List<TNode> GetChildren(int p_programLineNumber);
        List<TNode> GetChildrenS(int p_programLineNumber);

        //Follows, FollowsS
        TNode GetRightSibling(int p_programLineNumber);
        TNode GetLeftSibling(int p_programLineNumber);
        List<TNode> GetRightSiblingS(int p_programLineNumber);
        List<TNode> GetLeftSiblingS(int p_programLineNumber);
    }
}
