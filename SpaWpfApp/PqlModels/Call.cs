using SpaWpfApp.PqlConsts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.PqlModels
{
    public class Call
    {
        string CallType { get; set; }
        //List<ArgType> Args { get; set; }

        public Call(string callType)
        {
            this.CallType = callType;
        }
    }
}
