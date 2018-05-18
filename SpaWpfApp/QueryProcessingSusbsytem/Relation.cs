using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    class Relation
    {
        public string type { get; set; }
        public string arg1 { get; set; }
        public string arg2 { get; set; }

        public Relation(string type, string arg1, string arg2)
        {
            this.type = type;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }


        override public string ToString()
        {
            return type + "(" + arg1 + "," + arg2 + ")"; 
        }
    }
}
