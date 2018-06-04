using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SpaWpfApp.Exceptions
{
    public class SourceCodeException : Exception
    {
        public SourceCodeException()
        {
        }

        public SourceCodeException(string message) : base(message)
        {
        }

        public SourceCodeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SourceCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
