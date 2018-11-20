using System;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale
{
    public interface ICommandPublisher : IDisposable
    {
        string QueueName { get; set; }
        Task<T> Publish<T>(DomainCommand domainCommand);
    }
}