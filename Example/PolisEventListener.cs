using Minor.Nijn;
using Minor.Nijn.WebScale;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Example;
using Microsoft.Extensions.DependencyInjection;
using Minor.Nijn.RabbitMQBus;
using Minor.Nijn.TestBus;
using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using RabbitMQ.Client;
using Test;

namespace VoorbeeldMicroservice
{
    [EventListener("MVM.TestService.PolisEventListenerQueue")]
    [CommandListener]
    public class PolisEventListener
    {
        private readonly IDataMapper mapper;
        private readonly ICommandPublisher _commandPublisher;
        private readonly IEventPublisher _eventPublisher;

        //private readonly IDbContextOptions<PolisContext> _context;
        public PolisEventListener(IDataMapper mapper, ICommandPublisher commandPublisher, IEventPublisher eventPublisher)
        {
            this.mapper = mapper;
            _commandPublisher = commandPublisher;
            _eventPublisher = eventPublisher;
            mapper.Print();
            //_context = context;
        }

        [Command("Testje")]
<<<<<<< HEAD
        public async Task<int> CommandListner(TestCommand evt)
=======
        public int CommandListner(TestCommand evt)
>>>>>>> refs/remotes/origin/master
        {
            await Task.Delay(100);
            Console.WriteLine("************void***********");
<<<<<<< HEAD
            return 10;
=======

            return 1;
>>>>>>> refs/remotes/origin/master
        }
        [Command("TestjeAsync")]
        public async Task<int> CommandListnerAsync(TestCommand evt)
        {
            //Thread.Sleep(1000);
            //Console.WriteLine("TestCommandAsync ontvangen:");
            await Task.Delay(new Random().Next(100, 3000));
            //throw new ArgumentException("Fout");
            //return 250;
            return 10;
        }

        [Topic("MVM.Polisbeheer.PolisToegevoegd")]
        public void Handles(PolisToegevoegdEvent evt)
        {
            Console.WriteLine("Werkt dit?????????");
        }
        [Topic("#")]
        public void AuditLogger(EventMessage message)
        {
            Console.WriteLine("Audit:" + message.Message);
        }

        [Command("Testje2")]
        public int CommandListner2(TestCommand evt)
        {
            //Thread.Sleep(5000);
           //Console.WriteLine("TestCommand ontvangen:");
            return evt.i * evt.i * evt.i;
        }

        [Topic("#")]
        public void HandlesNew(EventMessage evt)
        {
            Console.WriteLine("Eventmessage voor auditlog Bericht ontvangen met #:" + evt.Message);
        }

        [Topic("MVM.Polisbeheer.*")]
        public void HandlesOld(PolisToegevoegdEvent evt)
        {
           // Console.WriteLine("Bericht ontvangen met *:");
           // Console.WriteLine(evt.Message);
        }

        [Topic("Polisbeheer.*")]
        public void HandlesNot(PolisToegevoegdEvent evt)
        {
          //  Console.WriteLine("Zou niet ontvangen moeten worden:");
          //  Console.WriteLine(evt.Message);
        }

        [Topic("Test")]
        public void Handles(HenkToegevoegdEvent evt)
        {
            _eventPublisher.Publish(new PolisToegevoegdEvent("MVM.Polisbeheer.PolisToegevoegd"));
            _eventPublisher.Publish(new PolisToegevoegdEvent("MVM.Polisbeheer.PolisToegevoegd"));
            _eventPublisher.Publish(new PolisToegevoegdEvent("MVM.Polisbeheer.PolisToegevoegd"));
            _eventPublisher.Publish(new PolisToegevoegdEvent("MVM.Polisbeheer.PolisToegevoegd"));
            _eventPublisher.Publish(new PolisToegevoegdEvent("MVM.Polisbeheer.PolisToegevoegd"));
            _eventPublisher.Publish(new PolisToegevoegdEvent("MVM.Polisbeheer.PolisToegevoegd"));
            //Console.WriteLine("Test Message ontvangen:");
            // Console.WriteLine(evt.Test);
        }
    }

    public class ReplayEventsCommand : DomainCommand
    {
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// Replay events that have occurred from and including this moment.
        /// </summary>
        public long? FromTimestamp { get; set; }

        /// <summary>
        /// Replay events that have occurred upto and including this moment.
        /// </summary>
        public long? ToTimestamp { get; set; }

        /// <summary>
        /// Replay only events from exactly this type.
        /// IF null, all event types will be replayed.
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Replay only events that match this Topic.
        /// </summary>
        public string Topic { get; set; }
    
    }

    [EventListener("MVM.TestService.PolisEventListenerQueue")]
    public class Test
    {
        [Topic("MVM.Polisbeheer.PolisToegevoegd")]
        public void Handles(PolisToegevoegdEvent evt)
        {
            //Console.WriteLine("Werkt dit?");
        }
    }

    public class PolisToegevoegdEvent : DomainEvent
    {
        public PolisToegevoegdEvent(string routingKey) : base(routingKey)
        {
        }

        [JsonProperty]
        public string Message { get; set; }
    }

    public class HenkToegevoegdEvent : DomainEvent
    {
        public HenkToegevoegdEvent(string routingKey) : base(routingKey)
        {
        }

        [JsonProperty]
        public string Test { get; set; }
    }
}
