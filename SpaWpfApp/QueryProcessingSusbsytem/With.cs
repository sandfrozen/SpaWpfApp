using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    public class With : Condition
    {
        public string left { get; set; }
        public string leftType { get; set; }
        public string right { get; set; }
        public string rightType { get; set; }

        public With(string left, string leftType, string right, string rightType)
        {
            this.left = left;
            this.leftType = leftType;
            this.right = right;
            this.rightType = rightType;
        }


        override public string ToString()
        {
            return this.left + "=" + this.right;
        }
    }
}
