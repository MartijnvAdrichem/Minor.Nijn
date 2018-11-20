using Newtonsoft.Json;
using RabbitMQ.Client;
using System;

namespace Minor.Nijn.WebScale
{
    public class EventPublisher : IEventPublisher, IDisposable
    {
        private IMessageSender Sender { get; }
        public EventPublisher(IBusContext<IConnection> context)
        {
            Sender = context.CreateMessageSender();
        }

        public void Publish(DomainEvent domainEvent)
        {
            var body = JsonConvert.SerializeObject(domainEvent);
            var eventMessage = new EventMessage(domainEvent.RoutingKey, body);
            Sender.SendMessage(eventMessage);
        }

        public void Dispose()
        {
            Sender?.Dispose();
        }
    }
}
