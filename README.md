# Minor.Nijn
A C# wrapper around RabbitMQ

This framework consists of 2 parts (Minor.Nijn and Minor.Nijn.Webscale). Minor.Nijn is the basic wrapper around RabbitMQ (event sending/receiving and RPC call sending/receiving). It also has a basic test environment used for integration testing.

Initialize this part of the framework by using the RabbitMQContextBuilder, for example:

```
  var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("Exchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "guest", password: "guest");
```

You can also use the environment variables (for docker). Note: the names are case sensitive.

```
USERNAME guest
PASSWORD guest
PORT 5672
HOSTNAME localhost
EXCHANGENAME exchange

```

The web-scale framework is a wrapper around the Nijn framework. With this framework you can add Topic- and Command attributes above your methods. These attributes will be converted into actual queues on the Exchange. You initialize this conversion with the following code:


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

If you are using something like ASP.NET you cannot make use of the Console.ReadKey() method, so instead of that use:


```
private ManualResetEvent _stopEvent = new ManualResetEvent(false);

    var builder = new MicroserviceHostBuilder()
    .RegisterDependencies(nijnServices =>
    {
        nijnServices.AddSingleton<IBusContext<IConnection>>(context); 
    })
    .WithContext(nijnBusContext)
    .UseConventions();


    new Thread(() =>
    {
        using (var host = builder.CreateHost())
        {
            host.StartListening();
            _stopEvent.WaitOne();
        }

    }).Start();

```

An example of a class that will be converted into queues:

```
[EventListener("DemoQueue")]
[CommandListener]
  public class SomeEventListener
  {
      private readonly IDataMapper mapper;
      
      //Make sure these dependencies are configurated in the RegisterDependencies method    
      public SomeEventListener(IDataMapper mapper)
      {
          this.mapper = mapper;
      }

      [Command("SomeCommand")] //Declares a queue and listens to incoming commands
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

To send an event use the following code, make sure to use the right topic name: 
```
        public Controller( IBusContext<IConnection> context)
        {
            _context = context;
        }

        var messageSender = new EventPublisher(_context);
            messageSender.Publish(new InheritOfDomainEvent(topicname, ...));
```


To send a command use the following code, if there is no response within 5 seconds a "NoResponseException" will be thrown
```
        SomeCommand command = new SomeCommand();
        public Controller( IBusContext<IConnection> context)
        {
            _context = context;
        }
            
        var publisher = new CommandPublisher(_context, "QueueToSendTo");
        var result = await publisher.Publish<T>(command);
```

## Testbus
The Nijn framework also has a testbus environment that will mock RabbitMQ. To activate it you need to replace the line var context = connectionBuilder.CreateContext() in your startup with context = new TestBusContext().

The testbus also gives you the opportunity to read out the existing queues (which you can use in your tests). You can do this with the following code

```
context = new TestBusContext()
context.TestQueues["queuename"].Queue //returns the actual queue which you can use Count on etc
context.CommandQueues["queuename"] //returns a command queue
context.DeclareQueue("queuename", new List<string> {"some.topic"}) //declares a queue that will listen on these topics, you can also use the context.CreateMessageReceiver() method but this will also consume the items in the queue, up to you to decide if you want it or not.
```



##### Note: This framework is made for educational purposes and is in no way perfect.
