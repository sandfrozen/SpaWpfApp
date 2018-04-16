using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.PqlConsts
{
    public class CallType
    {
        //public static readonly string FOLLOWS = "follows";
        //public static readonly string FOLLOWSS = "follows*";
        //public static readonly string MODIFIES = "modifies";
        //public static readonly string PARENT = "parent";
        //public static readonly string PARENTS = "parent*";
        //public static readonly string USES = "uses";

        public List<string> List { get; set; }

        public CallType()
        {
            List = new List<string>();
            List.Add("follows");
            List.Add("follows*");
            List.Add("modifies");
            List.Add("parent");
            List.Add("parent*");
            List.Add("uses");
        }
    }
}
