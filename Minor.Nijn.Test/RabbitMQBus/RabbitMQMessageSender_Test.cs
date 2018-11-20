using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Test;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using System;
using System.Text;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQMessageSender_Test
    {
        #region Constructor
        [TestMethod]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);

            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);

            var context = new RabbitMQBusContext(connectionMock.Object, "ExchangeName");

            // Act
            var sender = new RabbitMQMessageSender(context);

            // Assert
            Assert.AreEqual("ExchangeName", sender.ExchangeName);
            IModel channel = TestHelper.GetPrivateProperty<IModel>(sender, "Channel");
            Assert.AreEqual(channelMock.Object, channel);
        }
        #endregion

        #region SendMessage
        [TestMethod]
        public void SendMessage_CallsBasicPublish_WithCorrectRoutingKey()
        {
            // Arrange
            var propsMock = new Mock<IBasicProperties>();

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.BasicPublish("Testexchange1",
                                                  It.Is<string>(k => k == "MyRoutingKey"),
                                                  false,
                                                  propsMock.Object,
                                                  It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "MyMessage")))
                       .Verifiable();

            channelMock.Setup(c => c.CreateBasicProperties())
                       .Returns(propsMock.Object)
                       .Verifiable();

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object)
                          .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "Testexchange1");

            var sender = new RabbitMQMessageSender(context);

            // Act
            sender.SendMessage(new EventMessage("MyRoutingKey", "MyMessage"));

            // Assert
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void SendMessage_CallsBasicPublish_WithCorrectExchangeName()
        {
            // Arrange
            var propsMock = new Mock<IBasicProperties>();

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.BasicPublish(It.Is<string>(k => k == "Testexchange1"),
                                                  "MyRoutingKey",
                                                  false,
                                                  propsMock.Object,
                                                  It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "MyMessage")))
                       .Verifiable();

            channelMock.Setup(c => c.CreateBasicProperties())
                       .Returns(propsMock.Object)
                       .Verifiable();

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object)
                          .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "Testexchange1");

            var sender = new RabbitMQMessageSender(context);

            // Act
            sender.SendMessage(new EventMessage("MyRoutingKey", "MyMessage"));

            // Assert
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void SendMessage_CallsBasicPublish_WithCorrectMessage()
        {
            // Arrange
            var propsMock = new Mock<IBasicProperties>();

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.BasicPublish("Testexchange1",
                                                  "MyRoutingKey",
                                                  false,
                                                  propsMock.Object,
                                                  It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "MyMessage")))
                       .Verifiable();

            channelMock.Setup(c => c.CreateBasicProperties())
                       .Returns(propsMock.Object)
                       .Verifiable();

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object)
                          .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "Testexchange1");

            var sender = new RabbitMQMessageSender(context);

            // Act
            sender.SendMessage(new EventMessage("MyRoutingKey", "MyMessage"));

            // Assert
            channelMock.VerifyAll();
        }

        // TODO BasicProperties are Correctly Set
        [TestMethod]
        public void SendMessage_CallsBasicPublish_WithCorrectTypeInBasicProperties()
        {
            // Arrange
            IBasicProperties props = new BasicProperties();

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.CreateBasicProperties())
                       .Returns(props)
                       .Verifiable();

            channelMock.Setup(c => c.BasicPublish("Testxchange1",
                                                  "MyRoutingKey",
                                                  false,
                                                  It.Is<IBasicProperties>(p => p.Type == "Test"),
                                                  It.IsAny<Byte[]>()))
                       .Verifiable();

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object)
                          .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "Testxchange1");

            var sender = new RabbitMQMessageSender(context);

            // Act
            sender.SendMessage(new EventMessage("MyRoutingKey", "MyMessage", "Test"));

            // Assert
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void SendMessage_CallsBasicPublish_WithCorrectTimestampInBasicProperties()
        {
            // Arrange
            IBasicProperties props = new BasicProperties();

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.CreateBasicProperties())
                       .Returns(props)
                       .Verifiable();

            channelMock.Setup(c => c.BasicPublish("Testxchange1",
                                                  "MyRoutingKey",
                                                  false,
                                                  It.Is<IBasicProperties>(p => p.Timestamp.UnixTime == 9),
                                                  It.IsAny<Byte[]>()))
                       .Verifiable();

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object)
                          .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "Testxchange1");

            var sender = new RabbitMQMessageSender(context);

            // Act
            sender.SendMessage(new EventMessage("MyRoutingKey", "MyMessage", "Test", 9));

            // Assert
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void SendMessage_CallsBasicPublish_WithCorrectCorrelationIdInBasicProperties()
        {
            // Arrange
            IBasicProperties props = new BasicProperties();

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.CreateBasicProperties())
                       .Returns(props)
                       .Verifiable();

            channelMock.Setup(c => c.BasicPublish("Testxchange1",
                                                  "MyRoutingKey",
                                                  false,
                                                  It.Is<IBasicProperties>(p => p.CorrelationId == "correlationID"),
                                                  It.IsAny<Byte[]>()))
                       .Verifiable();

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                                 .Returns(channelMock.Object)
                                 .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "Testxchange1");

            var sender = new RabbitMQMessageSender(context);

            // Act
            sender.SendMessage(new EventMessage("MyRoutingKey", "MyMessage", "Test", 9, "correlationID"));

            // Assert
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void SendMessage_WithSenderDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var channelMock = new Mock<IModel>();

            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);

            var context = new RabbitMQBusContext(connectionMock.Object, "Testexchange1");

            var sender = new RabbitMQMessageSender(context);
            sender.Dispose();

            // Act & Assert
            Assert.ThrowsException<ObjectDisposedException>(() =>
            {
                sender.SendMessage(new EventMessage("MyRoutingKey", "MyMessage"));
            });
        }
        #endregion
    }
}
