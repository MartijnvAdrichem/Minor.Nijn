using System.Collections.Generic;

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
