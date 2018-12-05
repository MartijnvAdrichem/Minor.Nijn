using System;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale
{
    public interface ICommandPublisher : IDisposable
    {
        Task<T> Publish<T>(DomainCommand domainCommand, string queuename);
    }
}