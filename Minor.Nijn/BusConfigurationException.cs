using System;
using System.Runtime.Serialization;

namespace Minor.Nijn
{
    [Serializable]
    public class BusConfigurationException : Exception
    {
        public BusConfigurationException()
        {
        }

        public BusConfigurationException(string message) : base(message)
        {
        }

        public BusConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BusConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}