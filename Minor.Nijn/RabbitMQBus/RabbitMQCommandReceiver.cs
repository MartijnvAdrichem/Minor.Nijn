using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandReceiver : ICommandReceiver
    {

        private IModel channel;
        public string QueueName { get; private set; }

        private readonly ILogger _log;

        public RabbitMQCommandReceiver(RabbitMQBusContext context, string queueName)
        {
            channel = context.Connection.CreateModel();
            QueueName = queueName;
            _log = NijnLogger.CreateLogger<RabbitMQCommandReceiver>();
        }
        public void DeclareCommandQueue()
        {
            _log.LogInformation("Declared a queue {0} for commands", QueueName);
            channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: true, arguments: null);
            channel.BasicQos(0, 1, false);
         
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

            consumer.Received += (model, ea) =>
            {

                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                
                var message = Encoding.UTF8.GetString(body);
                CommandResponseMessage response = null; 
                try
                {
                    response = callback(new CommandRequestMessage(message, props.CorrelationId));
                    replyProps.Type = response.MessageType.ToString();
                }
                catch (Exception e)
                {
                    var realException = e.InnerException;
                    response = new CommandResponseMessage(realException.Message, realException.GetType().ToString(), props.CorrelationId);
                    replyProps.Type = realException.GetType().ToString();
                }
                finally
                {
                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: response?.EncodeMessage());
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            _log.LogInformation("Started listening for commands on queue {0} ", QueueName);

        }

        public void Dispose()
        {
            channel?.Close();
        }


    }


}
