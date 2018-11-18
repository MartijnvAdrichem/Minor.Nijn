using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQMessageSender : IMessageSender
    {
        public string ExchangeName { get; }
        private IModel Channel { get; }
        private readonly ILogger _log;
        private bool disposed = false;

        public RabbitMQMessageSender(RabbitMQBusContext context)
        {
            Channel = context.Connection.CreateModel();
            ExchangeName = context.ExchangeName;
            _log = NijnLogger.CreateLogger<RabbitMQMessageSender>();
        }

        public void SendMessage(IEventMessage message)
        {
            CheckDisposed();

            _log.LogTrace($"Sending message to routing key {message.RoutingKey ?? ""}");

            byte[] body = message.EncodeMessage();

            IBasicProperties basicProperties = Channel.CreateBasicProperties();
            basicProperties.Timestamp = new AmqpTimestamp(message.Timestamp == 0 ? DateTime.Now.Ticks : message.Timestamp);
            basicProperties.CorrelationId = message.CorrelationId ?? Guid.NewGuid().ToString();
            basicProperties.Type = message.EventType ?? "";

            Channel.BasicPublish(exchange: ExchangeName,
                                 routingKey: message.RoutingKey,
                                 mandatory: false,
                                 basicProperties: basicProperties,
                                 body: body);
        }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Channel?.Dispose();
            }

            disposed = true;
        }

        ~RabbitMQMessageSender()
        {
            Dispose(false);
        }

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
        #endregion
    }
}
