using SpaWpfApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Cfg
{
    public class GNode
    {
        public GNodeTypeEnum type;
        public List<int> programLineList { get; set; }
        public List<GNode> nextGNodeList { get; set; }
        public List<GNode> previousGNodeList { get; set; }


        public GNode() { }

        public GNode(GNodeTypeEnum p_type)
        {
            init();
            this.type = p_type;
        }

        public GNode(GNodeTypeEnum p_type, int p_lineNumber)
        {
            init();
            this.type = p_type;
            this.programLineList.Add(p_lineNumber);
        }


        private void init()
        {
            this.programLineList = new List<int>();
            this.nextGNodeList = new List<GNode>();
            this.previousGNodeList = new List<GNode>();
        }
    }
}
