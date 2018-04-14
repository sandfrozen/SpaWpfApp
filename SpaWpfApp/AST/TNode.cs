using SpaWpfApp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.AST
{
    public class TNode
    {
        public TNodeTypeEnum type { get; set; }
        public string name { get; set; }
        public int programLine { get; set; }

        public TNode up { get; set; }
        public TNode firstChild { get; set; }
        public TNode rightSibling { get; set; }
    }
}
