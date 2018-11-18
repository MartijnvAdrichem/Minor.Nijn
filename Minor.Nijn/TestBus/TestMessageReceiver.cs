using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minor.Nijn.TestBus
{
    public class TestMessageReceiver : IMessageReceiver
    {
        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }
        public TestBusContext Context { get; }
        private bool _queueDeclared;
        private bool _startedListening;

        public TestMessageReceiver(TestBusContext context, string queueName, IEnumerable<string> topicExpressions)
        {
            Context = context;
            QueueName = queueName;
            TopicExpressions = topicExpressions;
        }

        public void DeclareQueue()
        {
            if (_queueDeclared)
            {
                throw new BusConfigurationException("Cannot declare the queue twice");
            }

            Context.DeclareQueue(QueueName, TopicExpressions);

            _queueDeclared = true;
        }

        public void Dispose()
        {
        }

        public void StartReceivingMessages(EventMessageReceivedCallback callback)
        {
            if (_startedListening)
            {
                throw new BusConfigurationException("Cannot start receiving messages twice");
            }

            if (!Context.TestQueues.ContainsKey(QueueName))
            {
                throw new KeyNotFoundException("Queue " + QueueName + " does not exists");
            }

            new Task(() =>
            {
                var queue = Context.TestQueues[QueueName].Queue;

                while (true)
                {
                    if (queue.Count == 0)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                     callback.Invoke(queue.Dequeue());
                }
            }).Start();
            _startedListening = true;
        }
    }
}
