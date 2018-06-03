using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Cfg
{
    public class ProcedureCfg
    {
        public int IndexOfProcedureName { get; set; }
        public List<GNode> GNodeList { get; set; }

        public int lastProgramLineNumber { get; set; }

        public ProcedureCfg(int index)
        {
            this.GNodeList = new List<GNode>();
            this.IndexOfProcedureName = index;
        }
    }
}
