using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.PqlConsts
{
    public class ArgType
    {
        //public static readonly string STMT = "stmt";
        //public static readonly string ASSIGN = "assign";
        //public static readonly string WHILE = "while";

        public List<string> List { get; set; }

        public ArgType()
        {
            List = new List<string>();
            List.Add("stmt");
            List.Add("assign");
            List.Add("while");
        }
    }
}
