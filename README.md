# Minor.Nijn
A C# wrapper around RabbitMQ

this framework consists of 2 parts (Minor.Nijn and Minor.Nijn.Webscale)
Minor.Nijn is the basic wrapper around RabbitMQ (event sending/receiving and RPC call sending/receiving).
It also has a basic test environment used for integration testing.

Initialize this part of the framework with using the RabbitMQContextBuilder, for example:

```
  var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("Exchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "guest", password: "guest");
```

The WebScale framework is a wrapper around the Nijn framework. With this framework you can add Topic and Command attributes above your methods. These attributes will be converted into actual queues on the Exchange. You initialize this with the following code

```
ILoggerFactory loggerFactory = new LoggerFactory();
loggerFactory.AddProvider(
    new ConsoleLoggerProvider(
        (text, logLevel) => logLevel >= LogLevel.Debug, true));

var connectionBuilder = new RabbitMQContextBuilder()
        .WithExchange("Exchange")
        .WithAddress("localhost", 5672)
        .WithCredentials(userName: "guest", password: "guest");


using (var context = connectionBuilder.CreateContext())
{
    var builder = new MicroserviceHostBuilder()
        .SetLoggerFactory(loggerFactory)
        .RegisterDependencies((services) =>
            {
                //dependencies
            })
            .WithContext(context)
            .UseConventions();


    using (var host = builder.CreateHost())
    {  
        host.StartListening();

        Console.WriteLine("ServiceHost is listening to incoming events...");
        Console.WriteLine("Press any key to quit.");
        Console.ReadKey();
    }
}


```

An example of a class that will be converted into queues:

```
[EventListener("DemoQueue")]
  public class SomeEventListener
  {
      private readonly IDataMapper mapper;
      
      //Make sure these dependencies are configurated in the RegisterDependencies method    
      public SomeEventListener(IDataMapper mapper)
      {
          this.mapper = mapper;
      }

      [Command("SomeCommand")] //Declares a queue and listens to incoming events
      public int CommandListener(SomeCommand command)
      {
        //
      }

      //opens a topic (in the "DemoQueue") that will route all events that match this topic to this method.
      [Topic("Some.Thing.SomethingAdded")] 
      public void SomeEventListener(SomeEvent event)
      {
        //
      }
```


##### Note: This framework is made for learning purposes and is in no way perfect.
