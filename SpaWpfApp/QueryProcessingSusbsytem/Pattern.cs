using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    class Pattern : Condition
    {
        // It looks like:
        // pattern a("x", "2")
        // a is synonym
        // "x" is arg1
        // "2" is arg2
        public string synonym { get; set; }
        public string synonymType { get; set; }
        public string arg1 { get; set; }
        public string arg1type { get; set; }
        public string arg2 { get; set; }
        public string arg2type { get; set; }

        //arg3 for 'if' is always '_'

        public Pattern(string synonym, string synonymType, string arg1, string arg1type, string arg2, string arg2type)
        {
            this.synonym = synonym;
            this.synonymType = synonymType;
            this.arg1 = arg1;
            this.arg1type = arg1type;
            this.arg2 = arg2;
            this.arg2type = arg2type;
        }


        override public string ToString()
        {
            return this.synonym + "(" + this.arg1 + ", " + this.arg2 + ( synonymType == "if" ? ", _" : "" ) + ")";
        }

    }
}
