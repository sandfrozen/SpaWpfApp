﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaWpfApp.Exceptions
{
    class NoResultsException : Exception
    {
        public NoResultsException(string message) : base(message)
        {
        }
    }
}