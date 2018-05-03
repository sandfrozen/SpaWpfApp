using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Cfg
{
    public interface CfgAPI
    {
        /// <summary>
        /// returns list of Next programLines or null if parameter is last instruction in code
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<int> Next(int p_programLineNumber);

        /// <summary>
        /// returns list of NextS programLines or null if parameter is last instruction in code
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<int> NextS(int p_programLineNumber);

        /// <summary>
        /// returns list of Previous programLines or null if parameter is first instruction in code
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<int> Previous(int p_programLineNumber);

        /// <summary>
        /// returns list of PreviousS programLines or null if parameter is first instruction in code
        /// </summary>
        /// <param name="p_programLineNumber"></param>
        /// <returns></returns>
        List<int> PreviousS(int p_programLineNumber);
    }
}
