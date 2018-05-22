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
        public string arg1type { get; set; }
        public string arg2 { get; set; }
        public string arg2type { get; set; }

        public Relation(string type, string arg1, string arg1type, string arg2, string arg2type)
        {
            this.type = type;
            this.arg1 = arg1;
            this.arg1type = arg1type;
            this.arg2 = arg2;
            this.arg2type = arg2type;
        }


        override public string ToString()
        {
            return this.type + "(" + this.arg1 + " (" + this.arg1type + "), " + this.arg2 + " (" + this.arg2type + "))";
        }

        // const means static
        public const string Modifies = "Modifies";
        public const string ModifiesX = "Modifies*";
        public static List<string> ModifiesArgs1 = new List<string> { Entity._int, Entity.stmt, Entity.assign, Entity.procedure, Entity.prog_line, Entity._string, Entity._while };
        public static List<string> ModifiesArgs2 = new List<string> { Entity.variable, Entity._string, Entity._ };

        public const string Uses = "Uses";
        public const string UsesX = "Uses*";
        public static List<string> UsesArgs1 = new List<string> { Entity.stmt, Entity.procedure, Entity._int, Entity._string, Entity.prog_line, Entity.assign, Entity._ };
        public static List<string> UsesArgs2 = new List<string> { Entity.variable, Entity._, Entity._string };

        public const string Calls = "Calls";
        public const string CallsX = "Calls*";
        public static List<string> CallsArgs1 = new List<string> { Entity.procedure, Entity._, Entity._int, Entity.prog_line };
        public static List<string> CallsArgs2 = new List<string> { Entity.procedure, Entity._, Entity._int, Entity.prog_line };

        public const string Parent = "Parent";
        public const string ParentX = "Parent*";
        public static List<string> ParentArgs1 = new List<string> { Entity._int, Entity._, Entity.stmt, Entity._while, Entity._if };
        public static List<string> ParentArgs2 = new List<string> { Entity._int, Entity._, Entity.stmt, Entity._while, Entity._if, Entity.assign, Entity.constant };

        public const string Follows = "Follows";
        public const string FollowsX = "Follows*";
        public static List<string> FollowsArgs1 = new List<string> { Entity.stmt, Entity.assign, Entity._int, Entity._while, Entity.prog_line, Entity._if };
        public static List<string> FollowsArgs2 = new List<string> { Entity.stmt, Entity.assign, Entity._int, Entity._while, Entity.prog_line, Entity._if };

        public const string Next = "Next";
        public const string NextX = "Next*";
        public static List<string> NextArgs1 = new List<string> { Entity._int, Entity.stmt, Entity.prog_line, Entity.assign };
        public static List<string> NextArgs2 = new List<string> { Entity._int, Entity.stmt, Entity.prog_line, Entity.assign };

        public const string Affects = "Affects";
        public const string AffectsX = "Affects*";
        public static List<string> AffectsArgs1 = new List<string> { Entity._int, Entity.assign };
        public static List<string> AffectsArgs2 = new List<string> { Entity._int, Entity.assign };
    }
}
