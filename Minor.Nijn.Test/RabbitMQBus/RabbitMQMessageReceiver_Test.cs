using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Test;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQMessageReceiver_Test
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

            var context = new RabbitMQBusContext(connectionMock.Object, "TestExchange");
            var topicExpressions = new List<string> { "TestTopic" };

            // Act
            var receiver = new RabbitMQMessageReceiver(context, "TestQueue", topicExpressions);

            // Assert
            Assert.AreEqual("TestExchange", receiver.ExchangeName);
            Assert.AreEqual("TestQueue", receiver.QueueName);
            Assert.AreEqual(topicExpressions, receiver.TopicExpressions);
            IModel channel = TestHelper.GetPrivateProperty<IModel>(receiver, "Channel");
            Assert.AreEqual(channelMock.Object, channel);
        }
        #endregion

        #region DeclareQueue
        [TestMethod]
        public void DeclareQueue_CallsQueueDeclare()
        {
            // Arrange
            var channelMock = new Mock<IModel>();
            channelMock.Setup(c => c.QueueDeclare("TestQueue",
                                                  true,
                                                  false,
                                                  false,
                                                  null))
                       .Returns(new QueueDeclareOk("", 0, 0))
                       .Verifiable();

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object)
                          .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "TestExchange");
            var receiver = new RabbitMQMessageReceiver(context, "TestQueue", new List<string> { "TestTopic" });

            // Act
            receiver.DeclareQueue();

            // Assert
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void DeclareQueue_CallsQueueBindForEveryTopicExpression()
        {
            // Arrange
            var topicExpressions = new List<string> { "TestTopic1", "TestTopic2" };
            var channelMock = new Mock<IModel>();
            foreach (string topicExpression in topicExpressions)
            {
                channelMock.Setup(c => c.QueueBind("TestQueue", "TestExchange", topicExpression, null))
                           .Verifiable();
            }

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object)
                          .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "TestExchange");
            var receiver = new RabbitMQMessageReceiver(context, "TestQueue", topicExpressions);

            // Act
            receiver.DeclareQueue();

            // Assert
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void DeclareQueue_WithReceiverDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var channelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);

            var context = new RabbitMQBusContext(connectionMock.Object, "TestExchange3");
            var receiver = new RabbitMQMessageReceiver(context, "TestQueue", new List<string> { "TestTopic2" });
            receiver.Dispose();

            // Act & Assert
            Assert.ThrowsException<ObjectDisposedException>(() =>
            {
                receiver.DeclareQueue();
            });
        }
        #endregion

        #region StartReceivingMessages
        [TestMethod]
        public void StartReceivingMessages_WithCallbackNull_ThrowsArgumentNullException()
        {
            // Arrange
            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);

            var context = new RabbitMQBusContext(connectionMock.Object, "TestExchange");
            var receiver = new RabbitMQMessageReceiver(context, "TestQueue", new List<string> { "TestTopic2" });

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentNullException>(() =>
            {
                receiver.StartReceivingMessages(null);
            });
            Assert.AreEqual("callback", exception.ParamName);
        }

        [TestMethod]
        public void StartReceivingMessages_IfAlreadyStarted_ThrowsBusConfigurationException()
        {
            // Arrange
            var channelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);

            var context = new RabbitMQBusContext(connectionMock.Object, "TestExchange");
            var receiver = new RabbitMQMessageReceiver(context, "TestQueue", new List<string> { "TestTopic2" });

            // Act & Assert
            receiver.StartReceivingMessages(m => { });
            Assert.ThrowsException<BusConfigurationException>(() =>
            {
                receiver.StartReceivingMessages(m => { });
            });
        }

        [TestMethod]
        public void StartReceivingMessages_InvokesCallbackWithCorrectEventMessage()
        {
            // Arrange
            var channelMock = new Mock<IModel>();
            EventingBasicConsumer basicConsumer = null;
            channelMock.Setup(c => c.BasicConsume("TestQueue",
                                                  true,
                                                  "",
                                                  false,
                                                  false,
                                                  null,
                                                  It.IsAny<IBasicConsumer>()))
                       .Callback((string queue,
                           bool autoAck,
                           string consumerTag,
                           bool noLocal,
                           bool exclusive,
                           IDictionary<string, object> arguments,
                           IBasicConsumer consumer) =>
                       {
                           basicConsumer = consumer as EventingBasicConsumer;
                       });

            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);

            var context = new RabbitMQBusContext(connectionMock.Object, "TestExchange");
            var receiver = new RabbitMQMessageReceiver(context, "TestQueue", new List<string> { "TestTopic2" });

            // Act
            bool callbackWasInvoked = false;
            IEventMessage eventMessage = null;
            receiver.StartReceivingMessages((e) =>
            {
                callbackWasInvoked = true;
                eventMessage = e;
            });
            var properties = new BasicProperties
            {
                Type = "Test type",
                Timestamp = new AmqpTimestamp(1542183431),
                CorrelationId = "test id"
            };
            basicConsumer.HandleBasicDeliver("", 0, false, "", "routing.key", properties, Encoding.UTF8.GetBytes("test message"));

            // Assert
            Assert.IsTrue(callbackWasInvoked);
            Assert.AreEqual("routing.key", eventMessage.RoutingKey);
            Assert.AreEqual("test message", eventMessage.Message);
            Assert.AreEqual("Test type", eventMessage.EventType);
            Assert.AreEqual(1542183431, eventMessage.Timestamp);
            Assert.AreEqual("test id", eventMessage.CorrelationId);
        }

        [TestMethod]
        public void StartReceivingMessages_CallsBasicConsume()
        {
            // Arrange
            var channelMock = new Mock<IModel>();
            channelMock.Setup(c => c.BasicConsume("TestQueue",
                                                  true,
                                                  "",
                                                  false,
                                                  false,
                                                  null,
                                                  It.IsAny<IBasicConsumer>()))
                       .Verifiable();

            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);

            var context = new RabbitMQBusContext(connectionMock.Object, "TestExchange");
            var receiver = new RabbitMQMessageReceiver(context, "TestQueue", new List<string> { "TestTopic2" });

            // Act
            receiver.StartReceivingMessages((e) => { });

            // Assert
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void StartReceivingMessages_WithReceiverDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var channelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>();
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);

            var context = new RabbitMQBusContext(connectionMock.Object, "TestExchange3");
            var receiver = new RabbitMQMessageReceiver(context, "TestQueue", new List<string> { "TestTopic2" });
            receiver.Dispose();

            // Act & Assert
            Assert.ThrowsException<ObjectDisposedException>(() =>
            {
                receiver.StartReceivingMessages((e) => { });
            });
        }
        #endregion
    }
}
