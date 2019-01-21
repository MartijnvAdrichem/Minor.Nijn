using System;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Events
{
    public class EventPublisher : IEventPublisher, IDisposable
    {
        private readonly IBusContext<IConnection> _context;

        public EventPublisher(IBusContext<IConnection> context)
        {
            _context = context;
            Sender = context.CreateMessageSender();
        }

        private IMessageSender Sender { get; }

        public void Dispose()
        {
            Sender?.Dispose();
        }

        public void Publish(DomainEvent domainEvent)
        {
            if (_context.DontPublishEvents) return;

            var body = JsonConvert.SerializeObject(domainEvent);
            var eventMessage = new EventMessage(domainEvent.RoutingKey, body);
            Sender.SendMessage(eventMessage);
        }
    }
}