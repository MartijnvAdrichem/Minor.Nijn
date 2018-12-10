using System;

namespace Minor.Nijn.WebScale
{
    internal class NoSuitableConstructorException : Exception
    {
        public NoSuitableConstructorException()
        {
        }

        public NoSuitableConstructorException(string message) : base(message)
        {
        }
    }
}