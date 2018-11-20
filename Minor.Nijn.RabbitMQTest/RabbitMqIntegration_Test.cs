using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.RabbitMQBus;

namespace Minor.Nijn.RabbitMQTest
{
    [Ignore]
    [TestClass]
    public class RabbitMqIntegration_Test
    {
        private RabbitMQBusContext _context;
        
        [TestInitialize]
        public void Setup()
        {
            var contextBuilder = new RabbitMQContextBuilder();
            _context = contextBuilder.WithCredentials("guest", "guest").WithAddress("localhost", 5672).WithExchange("TestExchange1").CreateContext();
        }

        [Ignore]
        [TestMethod]
        public void SendAndReceiveWithTopicsCorrectly()
        {
            //Arrange
            IEventMessage receivedMessage = null;
            AutoResetEvent flag = new AutoResetEvent(false);

            var sender = _context.CreateMessageSender();
            var receiver = _context.CreateMessageReceiver("TestQueue", new List<string>() { "test" });
            receiver.DeclareQueue();

            var message = new EventMessage("test", "TestMessage");
            //Act
            sender.SendMessage(message);

            receiver.StartReceivingMessages(eventMessage =>
            {
                receivedMessage = eventMessage;
                flag.Set();
            });

            flag.WaitOne();

            //Assert
            Assert.IsNotNull(receivedMessage);
            Assert.AreEqual("TestMessage", receivedMessage.Message);
        }

        [Ignore]
        [TestMethod]
        public void ThrowsExceptionWithMultipleQueueDeclare()
        {
            //Arrange

            var receiver = _context.CreateMessageReceiver("TestQueue", new List<string>() { "test" });

            //Act
            var action = new Action(() =>
            {
                receiver.DeclareQueue();
                receiver.DeclareQueue();
            });

            //Assert
            var exception =  Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual("Can't declare the queue multiple times", exception.Message);
        }

        [Ignore]
        [TestMethod]
        public void ThrowsExceptionWhenListeningMultipleTimes()
        {
            //Arrange

            var receiver = _context.CreateMessageReceiver("TestQueue", new List<string>() { "test" });
            receiver.DeclareQueue();

            //Act
            var action = new Action(() =>
            {
                receiver.StartReceivingMessages(message =>
                {

                });
                receiver.StartReceivingMessages(message =>
                {

                });
            });

            //Assert
            var exception = Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual("Can't start listening multiple times", exception.Message);
        }

        [Ignore]
        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }
    }
}
