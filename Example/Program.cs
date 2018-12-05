using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Minor.Nijn.RabbitMQBus;
using Minor.Nijn.WebScale;
using System;
using System.Threading.Tasks;
using Minor.Nijn;
using Minor.Nijn.TestBus;
using RabbitMQ.Client;

namespace VoorbeeldMicroservice
{
    public class Program
    {
        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(
                new ConsoleLoggerProvider(
                    (text, logLevel) => logLevel >= LogLevel.Debug, true));

            //192.168.99.100
            var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithAddress("192.168.99.100", 5672)
                    .WithCredentials(userName: "guest", password: "guest");


            using (var context = connectionBuilder.CreateContext())
            {
                var builder = new MicroserviceHostBuilder()
                    .SetLoggerFactory(loggerFactory)
                    .RegisterDependencies((services) =>
                    {
                      services.AddTransient<IDataMapper, SinaasAppelDataMapper>();
                    })
                    .WithContext(context)
                    .UseConventions();

                using (var host = builder.CreateHost())
                {
                    host.StartListening();

                    Console.WriteLine("ServiceHost is listening to incoming events...");
                    Console.WriteLine("Press any key to quit.");

                    var publisher = new EventPublisher(context);
                    publisher.Publish(new PolisToegevoegdEvent("MVM.Polisbeheer.PolisToegevoegd") {Message = "Hey"});
                    publisher.Publish(new HenkToegevoegdEvent("Test") {Test = "Oi"});

                    int i = 0;
                    while (true)
                    {

                        Console.ReadKey();
                        Test(context, i);
                        Test(context, i * 100);
                        i++;
                    }
                }
            }
        }

        private static async Task Test(IBusContext<IConnection> context, int multiply)
        {
            CommandPublisher commandPublisher = new CommandPublisher(context);
            var testcommand = new TestCommand() { i = new Random().Next(99,100) * multiply };

            Console.WriteLine($"{multiply} sending");


            var result1 = await commandPublisher.Publish<int>(testcommand, "TestjeAsync");

            Console.WriteLine($"{multiply} result:" + result1);
        }
    }
}
