using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.TestBus
{
    public class TestBusQueue
    {
        public Queue<IEventMessage> Queue { get; } = new Queue<IEventMessage>();
        public IEnumerable<string> TopicExpressions;

        public TestBusQueue(IEnumerable<string> topicExpressions)
        {
            TopicExpressions = topicExpressions;
        }

    }
}
