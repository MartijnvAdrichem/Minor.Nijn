using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using RabbitMQ.Client.Framing;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestCommandReceiverTest
    {

        [TestMethod]
        public void TestCommandReceiverCreateTest()
        {
            TestBusContext context = new TestBusContext();
            var target = context.CreateCommandReceiver("queue");

            Assert.AreEqual("queue" ,target.QueueName);
        }

        [TestMethod]
        public void DeclareCommandQueueTest()
        {
            TestBusContext context = new TestBusContext();
            var target = context.CreateCommandReceiver("queue");
            target.DeclareCommandQueue();

            Assert.AreEqual(1 ,context.CommandQueues.Count);
            Assert.AreEqual("queue" ,context.CommandQueues.First().Key);
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
                return Task.Run(() => new CommandResponseMessage(cm.Message, "", cm.CorrelationId));
            });

            context.CommandQueues["queue"].Enqueue(new TestBusCommandMessage(new CommandRequestMessage("message", null), new BasicProperties() {ReplyTo = "responseQueue"} ));

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
            receiver.StartReceivingCommands((cm) => { return Task.Run(() => new CommandResponseMessage(cm.Message, "", null));});
           Assert.ThrowsException<BusConfigurationException>(() => receiver.StartReceivingCommands((cm) => { return Task.Run(() => new CommandResponseMessage(cm.Message, "" ,cm.CorrelationId)); }));
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
        public async Task TestbusIntegratieTest()
        {
            TestBusContext context = new TestBusContext();
            var sender = context.CreateCommandSender();
            var receiver = context.CreateCommandReceiver("queue");
            receiver.DeclareCommandQueue();
           receiver.StartReceivingCommands((cm) =>
           {
               var message = "message2";
               return Task.Run(() => new CommandResponseMessage(message, typeof(string).FullName, cm.CorrelationId));
           });
            
            var mess = new CommandRequestMessage("message", null);
            var result = await sender.SendCommandAsync(mess, "queue");

            Assert.AreEqual("message2", result.Message);



        }
    }
}
