using System;
using System.Threading.Tasks;

namespace Minor.Nijn.RabbitMQBus
{
    public interface ICommandReceiver : IDisposable
    {
        string QueueName { get; }

        void DeclareCommandQueue();
        void StartReceivingCommands(CommandReceivedCallback callback);
    }

    public delegate Task<CommandResponseMessage> CommandReceivedCallback(CommandRequestMessage commandMessage);

}
