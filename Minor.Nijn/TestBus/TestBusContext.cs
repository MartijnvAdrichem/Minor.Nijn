﻿using System.Collections.Concurrent;
using RabbitMQ.Client;
using System.Collections.Generic;
using Minor.Nijn.RabbitMQBus;

namespace Minor.Nijn.TestBus
{
    public class TestBusContext : IBusContext<IConnection>
    {
        public IConnection Connection { get; }
        public string ExchangeName { get; }
        public Dictionary<string, TestBusQueue> TestQueues { get; set; }
        public ConcurrentDictionary<string, Queue<TestBusCommandMessage>> CommandQueues { get; set; }

        public TestBusContext()
        {
            TestQueues = new Dictionary<string, TestBusQueue>();
            CommandQueues = new ConcurrentDictionary<string, Queue<TestBusCommandMessage>>();
        }

        public IMessageSender CreateMessageSender()
        {
            return new TestMessageSender(this);
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
                CommandQueues.TryAdd(queueName, new Queue<TestBusCommandMessage>());
            }
        }

        public void Dispose()
        {
        }
    }
}
