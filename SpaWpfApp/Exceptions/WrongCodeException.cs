using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SpaWpfApp.Exceptions
{
    class WrongCodeException : Exception
    {
        public WrongCodeException()
        {
        }

        public WrongCodeException(string message) : base(message)
        {
        }

        public WrongCodeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WrongCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
