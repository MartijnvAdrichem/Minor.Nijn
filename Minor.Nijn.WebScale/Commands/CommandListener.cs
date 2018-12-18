using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Minor.Nijn.RabbitMQBus;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Commands
{
    public class CommandListener : IDisposable, ICommandListener
    {
        private readonly MethodCommandInfo _methodCommandInfo;
        private ICommandReceiver _receiver;
        private readonly ILogger _logger;


        public CommandListener(MethodCommandInfo methodCommandInfo)
        {
            _methodCommandInfo = methodCommandInfo;
            QueueName = _methodCommandInfo.QueueName;
            _logger = NijnLogger.CreateLogger<CommandListener>();
        }

        public IMicroserviceHost Host { get; private set; }

        public string QueueName { get; }

        public void DeclareQueue(IBusContext<IConnection> context)
        {
            _receiver = context.CreateCommandReceiver(QueueName);
            _receiver.DeclareCommandQueue();
        }

        public void StartListening(IMicroserviceHost host)
        {
            Host = host;
            _receiver.StartReceivingCommands(Handle);
        }

        public async Task<CommandResponseMessage> Handle(CommandRequestMessage commandMessage)
        {
            var instance = Host.CreateInstanceOfType(_methodCommandInfo.ClassType);

            if (instance == null)
            {
                _logger.LogError("No constructor found for {0}, this could be related to your dependency injection", _methodCommandInfo.ClassType.FullName);
                throw new NoSuitableConstructorException(
                    "No suitable constructor found, make sure all your dependencies are injected");
            }


            var param = JsonConvert.DeserializeObject(commandMessage.Message,
                _methodCommandInfo.MethodParameter.ParameterType);

            var result = _methodCommandInfo.MethodInfo.Invoke(instance, new[] {param});

            if (result == null)
            {
                _logger.LogError("Method {} didnt return anything, make sure it returns a value or Task<T>", _methodCommandInfo.MethodInfo);
                throw new InvalidOperationException("Command " + _methodCommandInfo.MethodInfo.Name + " didn't return anything, make sure it returns a value or a Task<T>");
            }

            object taskResult = null;

            if (_methodCommandInfo.MethodReturnType.IsGenericType &&
                _methodCommandInfo.MethodReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                taskResult = await (dynamic)result;
            }

            var resultJson = JsonConvert.SerializeObject(taskResult ?? result);
            return new CommandResponseMessage(resultJson, _methodCommandInfo.MethodReturnType.ToString(),
                commandMessage.CorrelationId);
        }

        public void Dispose()
        {
            _receiver?.Dispose();
        }
    }
}