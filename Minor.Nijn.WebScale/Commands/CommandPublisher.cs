using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minor.Nijn.RabbitMQBus;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly ILogger _logger;

        public CommandPublisher(IBusContext<IConnection> context, string queueName)
        {
            QueueName = queueName;
            Sender = context.CreateCommandSender();
            _logger = NijnLogger.CreateLogger<CommandPublisher>();
        }

        private ICommandSender Sender { get; }
        public string QueueName { get; set; }

        public async Task<T> Publish<T>(DomainCommand domainCommand)
        {
            domainCommand.TimeStamp = DateTime.Now.Ticks;
            var body = JsonConvert.SerializeObject(domainCommand);
            var commandMessage = new CommandRequestMessage(body, domainCommand.CorrelationId);
            var task = Sender.SendCommandAsync(commandMessage, QueueName);

            if (await Task.WhenAny(task, Task.Delay(5000)) == task)
            {
                // Task completed within timeout.
                // Consider that the task may have faulted or been canceled.
                // We re-await the task so that any exceptions/cancellation is rethrown.
                var result = await task;

                if (result.MessageType.Contains("Exception"))
                {
                    object e = null;
                    try
                    {
                        e = Activator.CreateInstance(Type.GetType(result.MessageType), result.Message);
                    }
                    catch (Exception)
                    {
                        throw new InvalidCastException(
                            $"an unknown exception occured (message {result.Message}), exception type was {result.MessageType}");
                    }

                    throw e as Exception;
                }

                var obj = JsonConvert.DeserializeObject<T>(result.Message);
                return obj;
            }

            _logger.LogWarning("MessageID {cor} did not receive a response", domainCommand.CorrelationId);
            throw new NoResponseException("Could not get a response");
        }

        public void Dispose()
        {
            Sender?.Dispose();
        }
    }
}