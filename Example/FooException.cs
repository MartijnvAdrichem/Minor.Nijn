using System;
using System.Collections.Generic;
using System.Text;

namespace Example
{
    [System.Serializable]
    public class FooException : Exception
    {
        public FooException(string message) : base(message)
        {
        }
    }
}
