using SpaWpfApp.PqlConsts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.PqlModels
{
    public class PqlQueryTree
    {
        public List<Result> Results { get; set; }
        //public List<Call> Calls { get; set; }

        public PqlQueryTree(QueryPreprocessor qP)
        {
            CallType cT = new CallType();
            ArgType aT = new ArgType();
            foreach (var line in qP.Query.Lines)
            {
                var keys = line.Split(' ');
                foreach(var key in keys)
                {
                    if (aT.List.Contains(key))
                    {

                    }

                    if (cT.List.Contains(key))
                    {

                    }
                }

            }
        }
    }
}
