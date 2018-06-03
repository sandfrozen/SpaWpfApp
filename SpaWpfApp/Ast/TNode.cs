using SpaWpfApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Ast
{
    public class TNode
    {
        public TNodeTypeEnum type { get; set; }
        public int? indexOfName { get; set; }
        public int? programLine { get; set; }

        public int? value { get; set; }
        public string info { get; set; }

        public TNode up { get; set; }
        public TNode firstChild { get; set; }
        public TNode rightSibling { get; set; }

        public TNode(TNodeTypeEnum p_type, int? p_programLine, int? p_indexOfName, int? p_value)
        {
            this.type = p_type;
            this.programLine = p_programLine;
            this.indexOfName = p_indexOfName;
            this.value = p_value;
        }
    }
}
