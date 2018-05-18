using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Cfg
{
    public class ProcedureCfg
    {
        public List<GNode> GNodeList { get; set; }

        public int lastProgramLineNumber { get; set; }

        public ProcedureCfg()
        {
            this.GNodeList = new List<GNode>();
        }
    }
}
