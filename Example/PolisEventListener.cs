using Minor.Nijn;
using Minor.Nijn.WebScale;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Example;
using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using Test;

namespace VoorbeeldMicroservice
{
    [EventListener("MVM.TestService.PolisEventListenerQueue")]
    [CommandListener]
    public class PolisEventListener
    {
        private readonly IDataMapper mapper;
        private readonly ICommandPublisher _commandPublisher;

        //private readonly IDbContextOptions<PolisContext> _context;
        public PolisEventListener(IDataMapper mapper, ICommandPublisher commandPublisher)
        {
            this.mapper = mapper;
            _commandPublisher = commandPublisher;
            mapper.Print();
            //_context = context;
        }

        [Command("Testje")]
        public int CommandListner(TestCommand evt)
        {
            //Thread.Sleep(1000);
            Console.WriteLine("TestCommand ontvangen:");
            throw new FooException("Dit is een testexception @@@@@@@@@@@@@@@@@");
            return evt.i * evt.i;
        }
        [Command("TestjeAsync")]
        public async Task<int> CommandListnerAsync(TestCommand evt)
        {
            //Thread.Sleep(1000);
            Console.WriteLine("TestCommandAsync ontvangen:");
            await Task.Delay(new Random().Next(100, 3000));
            throw new ArgumentException("Fout");
            return evt.i * evt.i;
        }

        [Topic("MVM.Polisbeheer.PolisToegevoegd")]
        public async Task Handles(PolisToegevoegdEvent evt)
        {
            Console.WriteLine("Werkt dit?????????");
            try
            {
                var result = await _commandPublisher.Publish<long>(new TestCommand() {i = 10}, "Testje");
                Console.WriteLine(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [Command("Testje2")]
        public int CommandListner2(TestCommand evt)
        {
            //Thread.Sleep(5000);
            Console.WriteLine("TestCommand ontvangen:");
            return evt.i * evt.i * evt.i;
        }

        [Topic("#")]
        public void HandlesNew(EventMessage evt)
        {
            Console.WriteLine("Eventmessage voor auditlog Bericht ontvangen met #:");
            Console.WriteLine(evt.Message);
        }

        [Topic("MVM.Polisbeheer.*")]
        public void HandlesOld(PolisToegevoegdEvent evt)
        {
            Console.WriteLine("Bericht ontvangen met *:");
            Console.WriteLine(evt.Message);
        }

        [Topic("Polisbeheer.*")]
        public void HandlesNot(PolisToegevoegdEvent evt)
        {
            Console.WriteLine("Zou niet ontvangen moeten worden:");
            Console.WriteLine(evt.Message);
        }

        [Topic("Test")]
        public void Handles(HenkToegevoegdEvent evt)
        {
            Console.WriteLine("Test Message ontvangen:");
            Console.WriteLine(evt.Test);
        }
    }

    [EventListener("MVM.TestService.PolisEventListenerQueue")]
    public class Test
    {
        [Topic("MVM.Polisbeheer.PolisToegevoegd")]
        public void Handles(PolisToegevoegdEvent evt)
        {
            Console.WriteLine("Werkt dit?");
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
