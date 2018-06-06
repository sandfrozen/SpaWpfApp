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
        public int index { get; set; }


        public GNode() { }

        public GNode(GNodeTypeEnum p_type, int p_index)
        {
            init();
            this.type = p_type;
            this.index = p_index;
        }

        public GNode(GNodeTypeEnum p_type, int p_lineNumber, int p_index)
        {
            init();
            this.type = p_type;
            this.programLineList.Add(p_lineNumber);
            this.index = p_index;
        }


        private void init()
        {
            this.programLineList = new List<int>();
            this.nextGNodeList = new List<GNode>();
            this.previousGNodeList = new List<GNode>();
        }
    }
}
