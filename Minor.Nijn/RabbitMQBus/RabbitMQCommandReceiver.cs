using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQCommandReceiver : ICommandReceiver
    {

        private IModel _channel { get; }
        public string QueueName { get; private set; }
        private readonly ILogger _log;
        private bool _disposed = false;

        public RabbitMQCommandReceiver(RabbitMQBusContext context, string queueName)
        {
            _channel = context.Connection.CreateModel();
            QueueName = queueName;
            _log = NijnLogger.CreateLogger<RabbitMQCommandReceiver>();
        }
        public void DeclareCommandQueue()
        {
            CheckDisposed();

            _log.LogInformation("Declared a queue {0} for commands", QueueName);
            _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: true, arguments: null);
            _channel.BasicQos(0, 1, false);
         
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            CheckDisposed();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(queue: QueueName,
                autoAck: false,
                consumerTag: "",
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer);

            consumer.Received +=  async (s, ea) =>
            {
                Console.WriteLine("Delivery");
                await Handle(s, ea, callback);
                Console.WriteLine("Done");

            };
     
         
        }

        public async Task Handle(object s, BasicDeliverEventArgs ea, CommandReceivedCallback callback)
        {
            var body = ea.Body;
            var props = ea.BasicProperties;
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;

            var message = Encoding.UTF8.GetString(body);
            CommandResponseMessage response = null;
            try
            {
                response = await callback(new CommandRequestMessage(message, props.CorrelationId));
                replyProps.Type = response.MessageType.ToString();
            }
            catch (Exception e)
            {
                var realException = e.InnerException;
                response = new CommandResponseMessage(realException.Message, realException.GetType().ToString(),
                    props.CorrelationId);
                replyProps.Type = realException.GetType().ToString();
            }
            finally
            {
                _channel.BasicPublish(exchange: "",
                    routingKey: props.ReplyTo,
                    mandatory: false,
                    basicProperties: replyProps,
                    body: response?.EncodeMessage());

                _channel.BasicAck(deliveryTag: ea.DeliveryTag,
                    multiple: false);
            }
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
                _channel?.Dispose();
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
