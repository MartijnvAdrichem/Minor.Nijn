using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandReceiver : ICommandReceiver
    {

        private IModel Channel { get; }
        public string QueueName { get; private set; }
        private readonly ILogger _log;
        private bool _disposed = false;

        public RabbitMQCommandReceiver(RabbitMQBusContext context, string queueName)
        {
            Channel = context.Connection.CreateModel();
            QueueName = queueName;
            _log = NijnLogger.CreateLogger<RabbitMQCommandReceiver>();
        }
        public void DeclareCommandQueue()
        {
            CheckDisposed();

            _log.LogInformation("Declared a queue {0} for commands", QueueName);
            Channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            Channel.BasicQos(0, 1, false);
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            CheckDisposed();

            var consumer = new EventingBasicConsumer(Channel);
            Channel.BasicConsume(queue: QueueName,
                                 autoAck: false,
                                 consumerTag: "",
                                 noLocal: false,
                                 exclusive: false,
                                 arguments: null,
                                 consumer: consumer);

            consumer.Received += (s, ea) =>
            {
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = Channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                replyProps.Type = props.Type;
                
                var message = Encoding.UTF8.GetString(body);
                CommandMessage response = null; 
                try
                {
                    response = callback(new CommandMessage(message, props.Type, props.CorrelationId));
                    replyProps.Type = response.MessageType.ToString();
                }
                catch (Exception e)
                {
                    var ie = e.InnerException;
                    response = new CommandMessage(ie.Message, ie.GetType().ToString(), props.CorrelationId);
                    replyProps.Type = ie.GetType().ToString();
                }
                finally
                {
                    Channel.BasicPublish(exchange: "",
                                         routingKey: props.ReplyTo,
                                         mandatory: false,
                                         basicProperties: replyProps,
                                         body: response?.EncodeMessage());

                    Channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                     multiple: false);
                }
            };
            _log.LogInformation("Started listening for commands on queue {0} ", QueueName);

        }

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Channel?.Dispose();
            }

            _disposed = true;
        }

        ~RabbitMQCommandReceiver()
        {
            Dispose(false);
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
        #endregion
    }
}
