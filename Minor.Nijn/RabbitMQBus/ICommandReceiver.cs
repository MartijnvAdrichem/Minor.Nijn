using System;

namespace Minor.Nijn.RabbitMQBus
{
    public interface ICommandReceiver : IDisposable
    {
        string QueueName { get; }

        void DeclareCommandQueue();
        void StartReceivingCommands(CommandReceivedCallback callback);
    }

    public delegate CommandResponseMessage CommandReceivedCallback(CommandRequestMessage commandMessage);

}
