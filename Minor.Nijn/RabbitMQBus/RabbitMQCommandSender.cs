using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandSender : ICommandSender
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<CommandResponseMessage>> _callbackMapper =
            new ConcurrentDictionary<string, TaskCompletionSource<CommandResponseMessage>>();

        private readonly EventingBasicConsumer _consumer;

        private readonly ILogger _logger;
        private readonly string _replyQueueName;
        private bool _disposed;

        public RabbitMQCommandSender(RabbitMQBusContext context)
        {
            Channel = context.Connection.CreateModel();
            _replyQueueName = Channel.QueueDeclare("", false, false, true).QueueName;
            _logger = NijnLogger.CreateLogger<RabbitMQCommandSender>();

            _consumer = new EventingBasicConsumer(Channel);
            _consumer.Received += (model, ea) =>
            {
                if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out var tcs))
                    return;
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                var commandResponse =
                    new CommandResponseMessage(message, ea.BasicProperties.Type, ea.BasicProperties.CorrelationId);
                tcs.TrySetResult(commandResponse);
            };

            _logger.LogInformation("Created response queue with name {0}", _replyQueueName);
        }

        private IModel Channel { get; }


        public Task<CommandResponseMessage> SendCommandAsync(CommandRequestMessage request, string queueName)
        {
            CheckDisposed();

            if (queueName == _replyQueueName)
            {
                _logger.LogWarning(
                    "The queuename {0} has the same same as the reply queue name, this should not happen",
                    _replyQueueName);
                throw new ArgumentException($"The queuename {queueName} is the same as the reply queue name");
            }

            var props = Channel.CreateBasicProperties();
            props.CorrelationId = Guid.NewGuid().ToString();
            props.ReplyTo = _replyQueueName;
            props.Timestamp = new AmqpTimestamp(DateTime.Now.Ticks);
            props.Type = request.CommandType;
            var messageBytes = request.EncodeMessage();

            var tcs = new TaskCompletionSource<CommandResponseMessage>();
            _callbackMapper.TryAdd(props.CorrelationId, tcs);

            _logger.LogTrace("Sending command message with correlation id {id} and body {body} ", props.CorrelationId,
                request);

            Channel.BasicPublish("",
                queueName,
                false,
                props,
                messageBytes);

            Channel.BasicConsume(_replyQueueName,
                true,
                "",
                false,
                false,
                null,
                _consumer);

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

            if (disposing) Channel?.Dispose();

            _disposed = true;
        }

        ~RabbitMQCommandSender()
        {
            Dispose(false);
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        #endregion
    }
}