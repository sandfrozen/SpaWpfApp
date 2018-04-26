using SpaWpfApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.CfgFolder
{
    public class GNode
    {
        public List<GNode> nextGNodeList { get; set; }
        public List<GNode> previousGNodeList { get; set; }

        public List<int> programLineList { get; set; }

        public GNodeTypeEnum type;

        public GNode(GNodeTypeEnum p_type)
        {
            this.programLineList = new List<int>();
            this.nextGNodeList = new List<GNode>();
            this.previousGNodeList = new List<GNode>();

            this.type = p_type;
        }
    }
}
