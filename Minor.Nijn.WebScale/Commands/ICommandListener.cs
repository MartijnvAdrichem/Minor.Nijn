using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Commands
{
    public interface ICommandListener : IDisposable
    {
        string QueueName { get; }

        void DeclareQueue(IBusContext<IConnection> context);
        Task<CommandResponseMessage> Handle(CommandRequestMessage commandMessage);
        void StartListening(IMicroserviceHost host);
    }
}