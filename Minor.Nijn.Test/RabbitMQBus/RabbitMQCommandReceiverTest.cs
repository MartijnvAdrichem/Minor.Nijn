using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.RabbitMQBus;
using Moq;
using RabbitMQ.Client;

namespace Minor.Nijn.Test.RabbitMQBus
{
    [TestClass]
    public class RabbitMQCommandReceiverTest
    {

        [TestMethod]
        public void DeclareCommandQueueTest()
        {

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.QueueDeclare("queue", false, false, false, null)).Returns(new QueueDeclareOk("queue", 0, 0))
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

//        [TestMethod]
//        public void StarListeningInCommandReceiverTest()
//        {
//
//            var channelMock = new Mock<IModel>(MockBehavior.Strict);
//            channelMock.Setup(c => c.QueueDeclare("queue", false, false, false, null)).Returns(new QueueDeclareOk("queue", 0, 0))
//                .Verifiable();
//            channelMock.Setup(m => m.BasicQos(0, 1, false));
//            channelMock.Setup(m => m.BasicConsume())
//            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
//            connectionMock.Setup(r => r.CreateModel())
//                .Returns(channelMock.Object)
//                .Verifiable();
//
//            var mock = new Mock<RabbitMQBusContext>(connectionMock.Object, "bus");
//
//            var target = new RabbitMQCommandReceiver(mock.Object, "queue");
//
//            target.DeclareCommandQueue("queue");
//            target.StartReceivingCommands((CommandMessage mess) => { return mess; });
//        }
    }

}
