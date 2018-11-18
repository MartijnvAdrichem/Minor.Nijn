using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Minor.Nijn.WebScale
{
    class NoSuitableConstructorException : Exception
    {
        public NoSuitableConstructorException()
        {
        }

        public NoSuitableConstructorException(string message) : base(message)
        {
        }

    }
}
