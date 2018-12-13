using System;
using System.Collections;
using System.Runtime.Serialization;

namespace Test
{
    public class TestException : Exception
    {
        public TestException(string message) : base(message)
        {
        }
    }
}
