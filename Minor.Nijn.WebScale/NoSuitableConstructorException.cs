using System;

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
