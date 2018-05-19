using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.QueryProcessingSusbsytem
{
    class Relation
    {
        // Object
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

        // const means static
        public const string Modifies = "Modifies";
        public const string ModifiesX = "Modifies*";
        public const string Uses = "Uses";
        public const string UsesX = "Uses*";
        public const string Calls = "Calls";
        public const string CallsX = "Calls*";
        public const string Parent = "Parent";
        public const string ParentX = "Parent*";
        public const string Follows = "Follows";
        public const string FollowsX = "Follows*";
        public const string Next = "Next";
        public const string NextX = "Next*";
        public const string Affects = "Affects";
        public const string AffectsX = "Affects*";
    }
}
