using RabbitMQ.Client;
using System;

namespace Minor.Nijn.WebScale
{
    public interface ICommandListener : IDisposable
    {
        string QueueName { get; }

        void DeclareQueue(IBusContext<IConnection> context);
        CommandResponseMessage Handle(CommandRequestMessage commandMessage);
        void StartListening(IMicroserviceHost host);
    }
}