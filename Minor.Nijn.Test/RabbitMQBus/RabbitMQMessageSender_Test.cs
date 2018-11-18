using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Impl;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQMessageSender_Test
    {
        [TestMethod]
        public void SendMessageCallsBasicPublishWithCorrectMessage()
        {
            // Arrange
            var propsMock = new Mock<IBasicProperties>();

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.BasicPublish("Testxchange1",
                                                  "MyRoutingKey",
                                                  false,
                                                  propsMock.Object,
                                                  It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "MyMessage")))
                       .Verifiable();
            channelMock.Setup(c => c.CreateBasicProperties())
                       .Returns(propsMock.Object)
                       .Verifiable();

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(r => r.CreateModel())
                       .Returns(channelMock.Object)
                       .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "Testxchange1");

            var target = new RabbitMQMessageSender(context);

            // Act
            target.SendMessage(new EventMessage("MyRoutingKey", "MyMessage"));

            // Assert
            channelMock.VerifyAll();
        }
    }
}
