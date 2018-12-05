using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minor.Nijn.RabbitMQBus;

namespace Minor.Nijn.WebScale
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly ILogger _logger;

        public CommandPublisher(IBusContext<IConnection> context)
        {
            Sender = context.CreateCommandSender();
            _logger = NijnLogger.CreateLogger<CommandPublisher>();
        }

        private ICommandSender Sender { get; }

        public async Task<T> Publish<T>(DomainCommand domainCommand, string queueName)
        {
            domainCommand.TimeStamp = DateTime.Now.Ticks;
            var body = JsonConvert.SerializeObject(domainCommand);
            var commandMessage = new CommandRequestMessage(body, domainCommand.CorrelationId);
            var task = Sender.SendCommandAsync(commandMessage, queueName);

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
                        var type = Assembly.GetCallingAssembly().GetType(result.MessageType);
                        e = Activator.CreateInstance(type, result.Message);
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