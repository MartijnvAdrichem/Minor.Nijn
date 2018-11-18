using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client.Framing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Minor.Nijn.TestBus.Test
{
    [TestClass]
    public class TestCommandReceiver_Test
    {
        [TestMethod]
        public void TestCommandReceiverCreateTest()
        {
            TestBusContext context = new TestBusContext();
            var target = context.CreateCommandReceiver("queue");

            Assert.AreEqual("queue", target.QueueName);
        }

        [TestMethod]
        public void DeclareCommandQueueTest()
        {
            TestBusContext context = new TestBusContext();
            var target = context.CreateCommandReceiver("queue");
            target.DeclareCommandQueue();

            Assert.AreEqual(1, context.CommandQueues.Count);
            Assert.AreEqual("queue", context.CommandQueues.First().Key);
        }

        [TestMethod]
        public void StartReceivingCommandsTest()
        {
            TestBusContext context = new TestBusContext();
            var target = context.CreateCommandReceiver("queue");
            target.DeclareCommandQueue();
            context.DeclareCommandQueue("responseQueue");
            var autoReset = new AutoResetEvent(false);

            target.StartReceivingCommands((cm) =>
            {
                autoReset.Set();
                return cm;
            });

            context.CommandQueues["queue"].Enqueue(new TestBusCommandMessage(new CommandMessage("message", null, null), new BasicProperties() { ReplyTo = "responseQueue" }));

            bool succes = autoReset.WaitOne(5000);
            Assert.IsTrue(succes);
            Assert.AreEqual(0, context.CommandQueues["queue"].Count);
            Thread.Sleep(100);
            Assert.AreEqual(1, context.CommandQueues["responseQueue"].Count);
        }

        [TestMethod]
        public void StartListeningTwiceThrowsException()
        {
            TestBusContext context = new TestBusContext();
            var receiver = context.CreateCommandReceiver("queue");
            receiver.DeclareCommandQueue();
            receiver.StartReceivingCommands((cm) => { return cm; });
            Assert.ThrowsException<BusConfigurationException>(() => receiver.StartReceivingCommands((cm) => { return cm; }));
        }

        [TestMethod]
        public void DeclaringQueueTwiceThrowsException()
        {
            TestBusContext context = new TestBusContext();
            var receiver = context.CreateCommandReceiver("queue");
            receiver.DeclareCommandQueue();

            Assert.ThrowsException<BusConfigurationException>(() => receiver.DeclareCommandQueue());
        }

        [TestMethod]
        public async Task TestBusIntegrationTest()
        {
            TestBusContext context = new TestBusContext();
            var sender = context.CreateCommandSender();
            var receiver = context.CreateCommandReceiver("queue");
            receiver.DeclareCommandQueue();
            receiver.StartReceivingCommands((cm) =>
            {
                var message = "message2";
                return new CommandMessage(message, cm.MessageType, cm.CorrelationId);
            });

            var mess = new CommandMessage("message", null, null);
            var result = await sender.SendCommandAsync(mess, "queue");

            Assert.AreEqual("message2", result.Message);
        }
    }
}
