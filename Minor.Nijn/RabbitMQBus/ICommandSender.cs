using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Minor.Nijn.RabbitMQBus
{
    public interface ICommandSender : IDisposable
    {
        Task<CommandMessage> SendCommandAsync(CommandMessage request, string queueName);

    }
}
