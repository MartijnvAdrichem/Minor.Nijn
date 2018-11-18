using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.WebScale
{
    public interface IEventPublisher
    {
        void Publish(DomainEvent domainEvent);
    }
}
