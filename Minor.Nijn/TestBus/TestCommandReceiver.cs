using RabbitMQ.Client.Framing;
using System;
using System.Threading;
using System.Threading.Tasks;
using Minor.Nijn.RabbitMQBus;

namespace Minor.Nijn.TestBus
{
    public class TestCommandReceiver : ICommandReceiver
    {
        private TestBusContext Context { get; }
        private bool _isListening;
        private bool _isDeclared;

        public TestCommandReceiver(TestBusContext context, string queueName)
        {
            QueueName = queueName;
            Context = context;
        }

        public string QueueName { get; }

        public void DeclareCommandQueue()
        {
            if (_isDeclared)
            {
                throw new BusConfigurationException("Already declared queue " + QueueName);
            }

            Context.DeclareCommandQueue(QueueName);
            _isDeclared = true;
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            if (_isListening)
            {
                throw new BusConfigurationException("Already listening to queuename " + QueueName);
            }
            new Task(async () =>
            {
                var queue = Context.CommandQueues[QueueName];

                while (true)
                {
                    if (queue.Count == 0)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    CommandResponseMessage response = null;
                    var command = queue.Dequeue();
                    string type = command.Props.Type;

                    try
                    {
                        response = await callback.Invoke(command.Message as CommandRequestMessage);
                    }
                    catch (Exception e)
                    {
                        var realException = e.InnerException;
                        response = new CommandResponseMessage(realException.Message, realException.GetType().ToString(), command.Props.CorrelationId);
                        type = realException.GetType().FullName;
                    }
                    finally
                    {
                        Context.CommandQueues[command.Props.ReplyTo].Enqueue(new TestBusCommandMessage(response,
                            new BasicProperties()
                            {
                                CorrelationId =  command.Props.CorrelationId,
                                Type = type
                            }));
                    }


                }
            }).Start();
            _isListening = true;
        }

        public void Dispose()
        {
        }
    }
}
