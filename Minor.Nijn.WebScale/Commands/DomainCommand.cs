﻿namespace Minor.Nijn.WebScale.Commands
{
    public abstract class DomainCommand
    {
        public string QueueName { get; set; }
        public string CorrelationId { get; set; }
        public long TimeStamp { get; set; }
    }
}