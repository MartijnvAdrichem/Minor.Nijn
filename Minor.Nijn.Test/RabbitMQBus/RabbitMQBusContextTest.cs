using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.RabbitMQBus;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.Impl;
using QueueDeclareOk = RabbitMQ.Client.QueueDeclareOk;

namespace Minor.Nijn.Test.RabbitMQBus
{
    [TestClass]
    public class RabbitMQBusContextTest
    {
        private IConnection connection;
        [TestInitialize]
        public void Initialize()
        {
            Mock connectionMock = new Mock<IConnection>();
            connection = (IConnection)connectionMock.Object;

        }
        [TestMethod]
        public void CreateMessageSenderReturnsNewMessageSender()
        {
            var context = new RabbitMQBusContext(connection, "bus");
            var result = context.CreateMessageSender().GetType();

            Assert.IsInstanceOfType(typeof(RabbitMQMessageSender), result.BaseType);
        }

        [TestMethod]
        public void CreateMessageReceiverReturnsNewMessageReceiver()
        {
            var context = new RabbitMQBusContext(connection, "bus");
            var result = context.CreateMessageReceiver("testQueue", new List<string>() {"#"});

            Assert.IsInstanceOfType(typeof(RabbitMQMessageReceiver), result.GetType().BaseType);
        }

        [TestMethod]
        public void CreateCommandReceiverReturnsNewCommandReceiver()
        {

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(r => r.CreateModel())
                .Returns(channelMock.Object)
                .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "bus");
            var result = context.CreateCommandReceiver("queue");

            Assert.IsInstanceOfType(typeof(RabbitMQCommandReceiver), result.GetType().BaseType);
        }
    }
}
