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
        }
    }
}
