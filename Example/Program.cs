using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Minor.Nijn.RabbitMQBus;
using Minor.Nijn.WebScale;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Minor.Nijn;
using Minor.Nijn.TestBus;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using RabbitMQ.Client;

namespace VoorbeeldMicroservice
{
    public class Program
    {
        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            
            //Deprecated method, maar kan even niet anders
            ConsoleLoggerOptions options = new ConsoleLoggerOptions();
            loggerFactory.AddProvider(
                new ConsoleLoggerProvider(
                    (text, logLevel) => logLevel >= LogLevel.Debug, true));

            //192.168.99.100
            var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "Kantilever", password: "Kant1lever");



            var context = new TestBusContext();
            
                var builder = new MicroserviceHostBuilder()
                    .SetLoggerFactory(loggerFactory)
                    .RegisterDependencies((services) =>
                    {
                        services.AddTransient<IDataMapper, SinaasAppelDataMapper>();
                        services.AddTransient<ICommandPublisher, CommandPublisher>();
                        services.AddSingleton<IBusContext<IConnection>>(context);

                    })
                    .WithContext(context)
                    .UseConventions();


                    var host = builder.CreateHost();
                
                    host.StartListeningInOtherThread();

                    Console.WriteLine("ServiceHost is listening to incoming events...");
                    Console.WriteLine("Press any key to quit.");

                    var publisher = new EventPublisher(context);
                    var commandpublisher = new CommandPublisher(context);
                    publisher.Publish(new PolisToegevoegdEvent("MVM.Polisbeheer.PolisToegevoegd") {Message = "Hey"});
                    publisher.Publish(new HenkToegevoegdEvent("Test") {Test = "Oi"});
                    var result =  commandpublisher.Publish<int>(new TestCommand(), "Testje").Result;
                    Console.WriteLine("Result " + result);
        }

        private async static Task Test(IBusContext<IConnection> context)
        {
            CommandPublisher commandPublisher = new CommandPublisher(context);

            ReplayEventsCommand replayEventsCommand = new ReplayEventsCommand()
            {
                ToTimestamp = DateTime.Now.Ticks,
                ExchangeName = "fddfgf",
            };

            var result = commandPublisher.Publish<bool>(replayEventsCommand, "AuditlogReplayService", "Minor.WSA.AuditLog.Commands.ReplayEventsCommand").Result;

           
            //Console.WriteLine($"{multiply} result:" + result1);
        }
    }

    internal class HaalVoorraadUitMagazijnCommand : DomainCommand
    {
        public int I { get; }
        public int I1 { get; }

        public HaalVoorraadUitMagazijnCommand(int i, int i1)
        {
            I = i;
            I1 = i1;
        }
    }
}
