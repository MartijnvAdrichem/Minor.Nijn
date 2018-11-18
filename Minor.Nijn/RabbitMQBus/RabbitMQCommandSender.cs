using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandSender : ICommandSender
    {

        private readonly IModel _channel;
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<CommandMessage>> _callbackMapper =
                    new ConcurrentDictionary<string, TaskCompletionSource<CommandMessage>>();

        private readonly ILogger _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseQueueName">if responseQueueName null is given then rabbitMQ will generate a name</param>
        public RabbitMQCommandSender(RabbitMQBusContext context)
        {
            _channel = context.Connection.CreateModel();
            _replyQueueName = _channel.QueueDeclare().QueueName;
            _logger = NijnLogger.CreateLogger<RabbitMQCommandSender>();
            
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (model, ea) =>
            {
                if (!_callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<CommandMessage> tcs))
                    return;
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                var commandResponse = new CommandMessage(message, ea.BasicProperties.Type, ea.BasicProperties.CorrelationId);
                tcs.TrySetResult(commandResponse);
            };

            _logger.LogInformation("Created response queue with name {0}", _replyQueueName );
        }

       
        public Task<CommandMessage> SendCommandAsync(CommandMessage request, string queueName)
        {
            if (queueName == _replyQueueName)
            {
                _logger.LogWarning("The queuename {0} has the same same as the reply queue name, this should not happen", _replyQueueName);
                throw new ArgumentException($"The queuename {queueName} is the same as the reply queue name");
            }

            IBasicProperties props = _channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.Type = request.MessageType == null ? "" : request.MessageType;
            props.ReplyTo = _replyQueueName;
            var messageBytes = request.EncodeMessage();

            var tcs = new TaskCompletionSource<CommandMessage>();
            _callbackMapper.TryAdd(correlationId, tcs);

            _logger.LogTrace("Sending command message with correlation id {id} and body {body} ", correlationId, request);

            _channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: props,
                body: messageBytes);

            _channel.BasicConsume(
                consumer: _consumer,
                queue: _replyQueueName,
                autoAck: true);

             return tcs.Task;
        }

        public void Dispose()
        {
            _channel?.Close();
        }

    }
}
