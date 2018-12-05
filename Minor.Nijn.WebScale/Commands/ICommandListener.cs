using RabbitMQ.Client;
using System;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale
{
    public interface ICommandListener : IDisposable
    {
        string QueueName { get; }

        void DeclareQueue(IBusContext<IConnection> context);
        Task<CommandResponseMessage> Handle(CommandRequestMessage commandMessage);
        void StartListening(IMicroserviceHost host);
    }
}