using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Exceptions
{
    class WrongQueryFromatException : Exception
    {
        public WrongQueryFromatException(string message) : base(message)
        {
        }
    }
}
