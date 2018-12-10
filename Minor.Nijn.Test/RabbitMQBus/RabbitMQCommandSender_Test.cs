using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.RabbitMQBus;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.Test.RabbitMQBus
{
    [TestClass]
    public class RabbitMQCommandSender_Test
    {
        [TestMethod]
        public void SendCommandCallsBasicPublishWithCorrectMessage()
        {
            // Arrange
            var queueName = "MyRoutingKey";
            var replyQueueName = "replyQueueName";
            var timeStamp = new AmqpTimestamp(DateTime.Now.Ticks);

            var propsMock = new Mock<IBasicProperties>();
            propsMock.Setup(x => x.CorrelationId).Returns("test1");
            propsMock.Setup(x => x.ReplyTo).Returns(replyQueueName);
            propsMock.Setup(x => x.Timestamp).Returns(timeStamp);
            

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.BasicPublish("",
                    queueName,
                    false,
                    propsMock.Object,
                    It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "MyMessage")))
                .Verifiable();

            channelMock.Setup(c => c.CreateBasicProperties())
                .Returns(propsMock.Object)
                .Verifiable();

            channelMock.Setup(c => c.QueueDeclare("", false, false, true, null)).Returns(new QueueDeclareOk(replyQueueName, 0, 0))
                .Verifiable();

            channelMock.Setup(m =>
                m.BasicConsume(replyQueueName, true, "", false, false, null, It.IsAny<AsyncEventingBasicConsumer>())).Returns("");


            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(r => r.CreateModel())
                .Returns(channelMock.Object)
                .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "");

            var target = new RabbitMQCommandSender(context);

            // Act
            target.SendCommandAsync(new CommandRequestMessage("MyMessage", "test"), queueName);

            // Assert
            channelMock.VerifyAll();
        }
    }
}
