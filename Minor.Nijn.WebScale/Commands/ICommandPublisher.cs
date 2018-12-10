using System;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale.Commands
{
    public interface ICommandPublisher : IDisposable
    {
        Task<T> Publish<T>(DomainCommand domainCommand, string queuename);
    }
}