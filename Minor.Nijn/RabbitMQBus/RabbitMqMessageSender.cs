using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Framing;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageSender : IMessageSender
    {
        private readonly string _exchangeName;
        private readonly IModel _channel;
        private readonly ILogger _log;
        public RabbitMQMessageSender(RabbitMQBusContext context)
        {
            _channel = context.Connection.CreateModel();
            _exchangeName = context.ExchangeName;
            _log = NijnLogger.CreateLogger<RabbitMQMessageSender>();

        }

        public void SendMessage(IEventMessage message)
        {
            _log.LogTrace($"Sending message to routing key {message.RoutingKey ?? ""}");

            var body = message.EncodeMessage();

            var basicProperties = _channel.CreateBasicProperties();
            basicProperties.Timestamp = new RabbitMQ.Client.AmqpTimestamp(message.Timestamp == 0 ? DateTime.Now.Ticks : message.Timestamp);
            basicProperties.CorrelationId = message.CorrelationId == null ? Guid.NewGuid().ToString() : message.CorrelationId;
            basicProperties.Type = message.EventType ?? "";

            _channel.BasicPublish(exchange: _exchangeName,
                                  routingKey: message.RoutingKey,
                                  basicProperties: basicProperties,
                                  body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
        }
    }
}
