using System;

namespace Minor.Nijn.WebScale
{
    public class NoResponseException : Exception
    {
        public NoResponseException()
        {
        }

        public NoResponseException(string message) : base(message)
        {
        }
    }
}