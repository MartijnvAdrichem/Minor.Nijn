using System;

namespace Minor.Nijn.WebScale
{
    public abstract class DomainCommand
    {
        public string QueueName { get; set; }
        public string CorrelationId { get; set; }

        public Type ConvertResponseToType { get; set; }
        public long TimeStamp { get; set; }
    }
}
