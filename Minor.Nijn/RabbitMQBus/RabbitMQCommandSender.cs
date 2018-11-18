using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandSender : ICommandSender
    {

        private IModel Channel { get; }
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<CommandMessage>> _callbackMapper =
                    new ConcurrentDictionary<string, TaskCompletionSource<CommandMessage>>();
        private readonly ILogger _logger;
        private bool _disposed = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseQueueName">if responseQueueName null is given then rabbitMQ will generate a name</param>
        public RabbitMQCommandSender(RabbitMQBusContext context)
        {
            CheckDisposed();

            Channel = context.Connection.CreateModel();
            _replyQueueName = Channel.QueueDeclare().QueueName;
            _logger = NijnLogger.CreateLogger<RabbitMQCommandSender>();

            _consumer = new EventingBasicConsumer(Channel);
            _consumer.Received += (model, ea) =>
            {
                if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<CommandMessage> tcs))
                    return;
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                var commandResponse = new CommandMessage(message, ea.BasicProperties.Type, ea.BasicProperties.CorrelationId);
                tcs.TrySetResult(commandResponse);
            };

            _logger.LogInformation("Created response queue with name {0}", _replyQueueName);
        }

        public Task<CommandMessage> SendCommandAsync(CommandMessage request, string queueName)
        {
            CheckDisposed();

            if (queueName == _replyQueueName)
            {
                _logger.LogWarning("The queuename {0} has the same same as the reply queue name, this should not happen", _replyQueueName);
                throw new ArgumentException($"The queuename {queueName} is the same as the reply queue name");
            }

            IBasicProperties props = Channel.CreateBasicProperties();
            string correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.Type = request.MessageType ?? "";
            props.ReplyTo = _replyQueueName;
            byte[] message = request.EncodeMessage();

            var tcs = new TaskCompletionSource<CommandMessage>();
            _callbackMapper.TryAdd(correlationId, tcs);

            _logger.LogTrace("Sending command message with correlation id {id} and body {body} ", correlationId, request);

            Channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 mandatory: false,
                                 basicProperties: props,
                                 body: message);

            Channel.BasicConsume(queue: "",
                                 autoAck: true,
                                 consumerTag: "",
                                 noLocal: false,
                                 exclusive: false,
                                 arguments: null,
                                 consumer: _consumer);

            return tcs.Task;
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

        ~RabbitMQCommandSender()
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
