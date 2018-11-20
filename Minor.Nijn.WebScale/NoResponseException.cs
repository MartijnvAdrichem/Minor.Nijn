using System;
using System.Collections.Generic;
using System.Text;

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
