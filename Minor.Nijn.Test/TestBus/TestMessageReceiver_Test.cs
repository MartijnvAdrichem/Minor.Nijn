using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Minor.Nijn.TestBus.Test
{
    [TestClass]
    public class TestMessageReceiver_Test
    {
        [TestMethod]
        public void MessageReceiverCallbackGetsCalledWhenMessageIsAddedOnQueue()
        {
            var context = new TestBusContext();

            var receiver = context.CreateMessageReceiver("receiver", new List<string> { "#" });
            var autoResetEvent = new AutoResetEvent(false);
            IEventMessage message = null;

            receiver.DeclareQueue();
            receiver.StartReceivingMessages((e) =>
            {
                message = e;
                autoResetEvent.Set();
            });

            context.TestQueues["receiver"].Queue.Enqueue(new EventMessage("message", "message sending"));

            autoResetEvent.WaitOne();

            Assert.IsNotNull(message);
            Assert.AreEqual("message sending", message.Message);
        }

        [TestMethod]
        public void MessageReceiverCallbackGetsCalledMultipleTimesWhenListeningLonger()
        {
            var context = new TestBusContext();

            var receiver = context.CreateMessageReceiver("receiver", new List<string> { "#" });
            var autoResetEvent = new AutoResetEvent(false);
            var messages = new List<IEventMessage>();

            receiver.DeclareQueue();

            context.TestQueues["receiver"].Queue.Enqueue(new EventMessage("message", "1"));
            context.TestQueues["receiver"].Queue.Enqueue(new EventMessage("message", "2"));
            context.TestQueues["receiver"].Queue.Enqueue(new EventMessage("message", "3"));

            receiver.StartReceivingMessages((e) =>
            {
                messages.Add(e);
                autoResetEvent.Set();
            });


            autoResetEvent.WaitOne(500);
            autoResetEvent.WaitOne(500);
            autoResetEvent.WaitOne(500);

            Assert.AreEqual(3, messages.Count);
            Assert.IsTrue(messages.Any(m => m.Message == "1"));
            Assert.IsTrue(messages.Any(m => m.Message == "2"));
            Assert.IsTrue(messages.Any(m => m.Message == "3"));
        }

        [TestMethod]
        public void MessageReceiverCallingQueueDeclaredTwiceThrowsException()
        {
            var context = new TestBusContext();

            var receiver = context.CreateMessageReceiver("receiver", new List<string> { "#" });

            receiver.DeclareQueue();

            Assert.ThrowsException<BusConfigurationException>(() => receiver.DeclareQueue());
        }

        [TestMethod]
        public void MessageReceiverCallingStartReceivingTwiceThrowsException()
        {
            var context = new TestBusContext();

            var receiver = context.CreateMessageReceiver("receiver", new List<string> { "#" });
            receiver.DeclareQueue();
            receiver.StartReceivingMessages(null);

            Assert.ThrowsException<BusConfigurationException>(() => receiver.StartReceivingMessages(null));
        }

        [TestMethod]
        public void MessageReceiverReturnsExceptionWhenNoQueueIsDefined()
        {
            var context = new TestBusContext();

            var receiver = context.CreateMessageReceiver("receiver", new List<string> { "#" });

            Assert.ThrowsException<KeyNotFoundException>(() => receiver.StartReceivingMessages(null));
        }
    }
}
