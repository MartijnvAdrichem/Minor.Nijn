using System;
using System.Threading.Tasks;

namespace Minor.Nijn
{
    public interface ICommandSender : IDisposable
    {
        Task<CommandMessage> SendCommandAsync(CommandMessage request, string queueName);
    }
}
