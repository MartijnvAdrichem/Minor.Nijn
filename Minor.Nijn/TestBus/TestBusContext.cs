using System;
using System.Collections.Generic;
using System.Text;
using Minor.Nijn.RabbitMQBus;
using RabbitMQ.Client;

namespace Minor.Nijn.TestBus
{
    public class TestBusContext : IBusContext<IConnection>
    {
        public IConnection Connection { get; }

        public string ExchangeName { get; }

        public Dictionary<string, TestBusQueue> TestQueues { get; set; }
        public Dictionary<string, Queue<TestBusCommandMessage>> CommandQueues { get; set; }

        public TestBusContext()
        {
            TestQueues = new Dictionary<string, TestBusQueue>();
            CommandQueues = new Dictionary<string, Queue<TestBusCommandMessage>>();
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            return new TestMessageReceiver(this, queueName, topicExpressions);
        }

        public ICommandSender CreateCommandSender()
        {
            return new TestCommandSender(this);
        }

        public ICommandReceiver CreateCommandReceiver(string queueName)
        {
            return new TestCommandReceiver(this, queueName);
        }

        public IMessageSender CreateMessageSender()
        {
            return new TestMessageSender(this);
        }

        public void Dispose()
        {
        }

        public void DeclareQueue(string queueName, IEnumerable<string> topics)
        {
            if (!TestQueues.ContainsKey(queueName))
            {
                TestQueues[queueName] = new TestBusQueue(topics);
            }
        }


        public void DeclareCommandQueue(string queueName)
        {
            if (!CommandQueues.ContainsKey(queueName))
            {
                CommandQueues[queueName] = new Queue<TestBusCommandMessage>();
            }
        }

    }
}
