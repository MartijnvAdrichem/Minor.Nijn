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
    public class RabbitMQCommandReceiverTest
    {

        [TestMethod]
        public void DeclareCommandQueueTest()
        {

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.QueueDeclare("queue", false, false, true, null)).Returns(new QueueDeclareOk("queue", 0, 0))
                .Verifiable();
            channelMock.Setup(m => m.BasicQos(0, 1, false));

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(r => r.CreateModel())
                .Returns(channelMock.Object)
                .Verifiable();

            var mock = new Mock<RabbitMQBusContext>(connectionMock.Object, "bus");

            var target = new RabbitMQCommandReceiver(mock.Object, "queue");

            target.DeclareCommandQueue();

        }

        [TestMethod]
        public void RecieveCommandsCalledCorrectly()
        {
            // Arrange
            var queueName = "TestQueue1";

            var channelMock = new Mock<IModel>(MockBehavior.Strict);


            channelMock.Setup(c => c.QueueDeclare(queueName, false, false, true, null)).Returns(new QueueDeclareOk("", 0, 0))
                .Verifiable();

            channelMock.Setup(c => c.BasicQos(0,1,false)).Verifiable();

            channelMock.Setup(m => m.BasicConsume(queueName, false, "", false, false, null, It.IsAny<EventingBasicConsumer>())).Returns(queueName);


            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(r => r.CreateModel())
                .Returns(channelMock.Object)
                .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "");

            var target = new RabbitMQCommandReceiver(context, queueName);

            // Act
            target.DeclareCommandQueue();
            target.StartReceivingCommands(Callback);

            // Assert
            channelMock.VerifyAll();
        }

        private CommandResponseMessage Callback(CommandRequestMessage commandmessage)
        {
            return new CommandResponseMessage("test", typeof(string).FullName, "test");
        }
    }

}
