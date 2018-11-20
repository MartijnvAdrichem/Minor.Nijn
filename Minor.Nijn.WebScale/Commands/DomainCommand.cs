using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.WebScale
{
    public abstract class DomainCommand
    {
        public string QueueName { get; set; }
        public string CorrelationId { get; set; }
        public long TimeStamp { get; set; }
    }
}
