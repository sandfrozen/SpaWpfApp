using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.CfgFolder
{
    public class GNode
    {
        public GNode nextGNode { get; set; }
        public GNode next2GNode { get; set; } // for else node
        public GNode previousGNode { get; set; }

        List<int> programLineList { get; set; }

        public GNode()
        {
            this.programLineList = new List<int>();
        }
    }
}
