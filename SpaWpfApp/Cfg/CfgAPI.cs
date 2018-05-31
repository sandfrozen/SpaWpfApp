using SpaWpfApp.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Cfg
{
    public interface CfgAPI
    {
        /// <summary>
        /// returns list of Next TNode or null if parameter is last instruction in code
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<TNode> Next(TNode p_from, string p_to);

        /// <summary>
        /// returns list of NextS programLines or -1 if parameter is last instruction in code
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<TNode> NextX(TNode p_from, string p_to);

        List<TNode> Previous(TNode p_to, string p_from);
        List<TNode> PreviousX(TNode p_to, string p_from);

        Boolean IsNext(int p1, int p2);
        Boolean IsNextX(int p1, int p2);
    }
}
