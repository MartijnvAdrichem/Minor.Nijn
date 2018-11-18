using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageReceiver : IMessageReceiver
    {
        private IModel _channel;
        private string _exchangeName;
        private bool _listening;
        private bool _queueDeclared;
        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }

        private readonly ILogger _log;


        public RabbitMQMessageReceiver(RabbitMQBusContext context, string queueName, IEnumerable<string> topicExpressions)
        {
            _log = NijnLogger.CreateLogger<RabbitMQMessageReceiver>();

            QueueName = queueName;
            TopicExpressions = topicExpressions;
            _channel = context.Connection.CreateModel();

            _exchangeName = context.ExchangeName;

            _listening = false;
            _queueDeclared = false;
        }

        public void DeclareQueue()
        {
            if (_queueDeclared)
            {
                _log.LogWarning($"Trying to declare a queue ({QueueName}) twice");
                throw new BusConfigurationException("Can't declare the queue multiple times");
            }

            _channel.QueueDeclare(QueueName, true, false, false);
            if (TopicExpressions == null || !TopicExpressions.Any())
            {
                _log.LogInformation($"Queue {QueueName} is now listening on default routingKey");

                _channel.QueueBind(queue: QueueName,
                    exchange: _exchangeName,
                    routingKey: "");

            }
            else
            {
                foreach (var bindingKey in TopicExpressions)
                {
                    _log.LogInformation($"Queue {QueueName} is now listening on {bindingKey}");

                    _channel.QueueBind(queue: QueueName,
                        exchange: _exchangeName,
                        routingKey: bindingKey);
                }
            }

            _queueDeclared = true;
        }

        public void StartReceivingMessages(EventMessageReceivedCallback callback)
        {
            if (_listening)
            {
                _log.LogWarning($"Trying to start listening to ({QueueName}) twice");
                throw new BusConfigurationException("Can't start listening multiple times");
            }
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                var eventMessage = new EventMessage(ea.RoutingKey, message, ea.BasicProperties.Type, ea.BasicProperties.Timestamp.UnixTime, ea.BasicProperties.CorrelationId);
                callback(eventMessage);
            };

            _channel.BasicConsume(queue: QueueName,
                autoAck: true,
                consumer: consumer);
            _listening = true;
        }

        public void Dispose()
        {
            _channel?.Close();
        }
    }
}
