using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQMessageReceiver_Test
    {
        [TestMethod]
        public void RecieveMessagesCalledCorrectly()
        {
            // Arrange
            var queueName = "TestQueue1";
            var exchangeName = "Testxchange1";

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.QueueBind(queueName, exchangeName, "", null))
                .Verifiable();

            channelMock.Setup(c => c.QueueDeclare(queueName, true, false, false, null)).Returns(new QueueDeclareOk("", 0, 0))
                .Verifiable();

            channelMock.Setup(m => m.BasicConsume(queueName, true, "", false, false, null, It.IsAny<EventingBasicConsumer>())).Returns(queueName);


            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(r => r.CreateModel())
                .Returns(channelMock.Object)
                .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, exchangeName);

            var target = new RabbitMQMessageReceiver(context, queueName, null);

            // Act
            target.DeclareQueue();
            target.StartReceivingMessages(message =>
            {

            });

            // Assert
            channelMock.VerifyAll();
        }
    }
}
