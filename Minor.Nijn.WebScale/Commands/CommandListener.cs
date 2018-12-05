using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using Minor.Nijn.RabbitMQBus;

namespace Minor.Nijn.WebScale
{
    public class CommandListener : IDisposable, ICommandListener
    {

        public string QueueName { get; }
        private ICommandReceiver _receiver;
        private readonly MethodCommandInfo _methodCommandInfo;
        public IMicroserviceHost Host { get; private set; }

        public CommandListener(MethodCommandInfo methodCommandInfo)
        {
            _methodCommandInfo = methodCommandInfo;
            QueueName = _methodCommandInfo.QueueName;
        }

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
                    throw new NoSuitableConstructorException(
                        "No suitable constructor found, make sure all your dependencies are injected");
                }

                var param = JsonConvert.DeserializeObject(commandMessage.Message,
                    _methodCommandInfo.MethodParameter.ParameterType);

                object result = _methodCommandInfo.MethodInfo.Invoke(instance, new[] {param});
                object taskResult = null;

                if (_methodCommandInfo.MethodReturnType.IsGenericType &&
                    _methodCommandInfo.MethodReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    taskResult = await ((dynamic) result);
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
