using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.CfgFolder
{
    public class Cfg
    {
        public List<GNode> GNodeList { get; set; }

        public Cfg()
        {
            this.GNodeList = new List<GNode>();
        }
    }
}
