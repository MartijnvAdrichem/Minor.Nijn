using System;
using System.Collections.Generic;
using System.Text;

namespace Example
{
    public class FunctionalException : Exception
    { 
        public FunctionalException()
        {
        }

        public FunctionalException(string message) : base(message)
        {
        }
    }
}
