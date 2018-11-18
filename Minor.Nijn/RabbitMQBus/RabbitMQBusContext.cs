using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client.Framing.Impl;
using Microsoft.Extensions.Logging;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQBusContext : IBusContext<IConnection>
    {
        public IConnection Connection { get; }

        public string ExchangeName { get; }

        private ILogger _log;

        public RabbitMQBusContext(IConnection connection, string exchangeName)
        {
            _log = NijnLogger.CreateLogger<RabbitMQBusContext>();
            Connection = connection;
            ExchangeName = exchangeName;
        }

        public IMessageSender CreateMessageSender()
        {
            _log.LogInformation("Creating new RabbitMQ Message Sender");
            var messageSender = new RabbitMQMessageSender(this);
            return messageSender;
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            _log.LogInformation("Creating new RabbitMQ Message Receiver");
            var messageReciever = new RabbitMQMessageReceiver(this, queueName, topicExpressions);
            return messageReciever;
        }

        public ICommandSender CreateCommandSender()
        {
            _log.LogInformation("Creating new RabbitMQ Command Sender");
            var commandSender = new RabbitMQCommandSender(this);
            return commandSender;
        }

        public ICommandReceiver CreateCommandReceiver(string queueName)
        {
            _log.LogInformation("Creating new RabbitMQ Command receiver");
            var commandReceiver = new RabbitMQCommandReceiver(this, queueName);
            return commandReceiver;
        }

        public void Dispose()
        {
            Connection.Close();
        }
    }
}
