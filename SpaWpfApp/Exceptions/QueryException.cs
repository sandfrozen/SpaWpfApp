﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Exceptions
{
    public class QueryException : Exception
    {

        public QueryException(string message) : base(message)
        {
        }
    }
}
