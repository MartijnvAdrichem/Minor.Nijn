using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Minor.Nijn.RabbitMQBus
{
    public interface ICommandSender : IDisposable
    {
        Task<CommandResponseMessage> SendCommandAsync(CommandRequestMessage request, string queueName);
    }
}
