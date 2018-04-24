using SpaWpfApp.PqlConsts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.PqlModels
{
    public class Result
    {
        string Name { get; set; }
        string Type;

        public Result(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}
