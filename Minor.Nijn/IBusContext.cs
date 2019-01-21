using System;
using System.Collections.Generic;
using Minor.Nijn.RabbitMQBus;

namespace Minor.Nijn
{
    public interface IBusContext<TConnection> : IDisposable
    {
        TConnection Connection { get; }
        string ExchangeName { get; }

        bool DontPublishEvents { get; set; }
        IMessageSender CreateMessageSender();
        IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions);

        ICommandSender CreateCommandSender();
        ICommandReceiver CreateCommandReceiver(string queueName);
    }
}
