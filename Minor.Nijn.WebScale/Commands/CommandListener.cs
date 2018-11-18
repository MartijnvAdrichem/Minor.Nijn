using Newtonsoft.Json;
using RabbitMQ.Client;
using System;

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

        public CommandMessage Handle(CommandMessage commandMessage)
        {
            var instance = Host.CreateInstanceOfType(_methodCommandInfo.ClassType);

            var param = JsonConvert.DeserializeObject(commandMessage.Message, _methodCommandInfo.MethodParameter.ParameterType);
            object result = _methodCommandInfo.MethodInfo.Invoke(instance, new[] {param});
            var resultJson = JsonConvert.SerializeObject(result);
            return new CommandMessage(resultJson, _methodCommandInfo.MethodReturnType.ToString(), commandMessage.CorrelationId);
       }

        public void Dispose()
        {
            _receiver?.Dispose();
        }
    }
}
