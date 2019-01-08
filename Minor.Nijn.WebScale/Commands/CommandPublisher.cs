using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minor.Nijn.RabbitMQBus;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Commands
{
    public class CommandPublisher : ICommandPublisher
    {
        private readonly ILogger _logger;
        private readonly Assembly assembly;
        private static Assembly _previousFoundAssembly;

        public CommandPublisher(IBusContext<IConnection> context)
        {
            Sender = context.CreateCommandSender();
            _logger = NijnLogger.CreateLogger<CommandPublisher>();
             assembly = Assembly.GetEntryAssembly();
        }

        private ICommandSender Sender { get; }

        public async Task<T> Publish<T>(DomainCommand domainCommand, string queueName, string commandType = "")
        {
            domainCommand.TimeStamp = DateTime.Now.Ticks;
            var body = JsonConvert.SerializeObject(domainCommand);
            var commandMessage = new CommandRequestMessage(body, domainCommand.CorrelationId, commandType);
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

                        var type = assembly.GetType(result.MessageType) ??
                                   Assembly.GetCallingAssembly().GetType(result.MessageType) ??
                                   GetTypeFromReferencedAssemblies(result.MessageType);

                        e = Activator.CreateInstance(type, result.Message);

                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Invalid exception found {}",  result.MessageType);
                        throw new InvalidCastException(
                            $"an unknown exception occured (message {result.Message}), exception type was {result.MessageType}");
                    }

                    throw e as Exception;
                }

                if (string.IsNullOrEmpty(result.Message) || result.Message == "null")
                {
                    return default(T);
                }
                var obj = JsonConvert.DeserializeObject<T>(result.Message);
                return obj;
            }

            _logger.LogWarning("MessageID {cor} did not receive a response", domainCommand.CorrelationId);
            throw new NoResponseException("Could not get a response");
        }

        private Type GetTypeFromReferencedAssemblies(string name)
        {
            Type type = _previousFoundAssembly?.GetType(name);
            if (type != null)
            {
                return type;
            }

            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
               var loadingAssembly = Assembly.Load(referencedAssembly);
               type = loadingAssembly.GetType(name);
                if (type != null)
                {
                    _previousFoundAssembly = loadingAssembly;
                    return type;
                }
            }

            return null;
        }

        public void Dispose()
        {
            Sender?.Dispose();
        }
    }
}