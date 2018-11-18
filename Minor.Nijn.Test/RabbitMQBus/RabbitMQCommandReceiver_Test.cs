using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQCommandReceiver_Test
    {
        [TestMethod]
        public void DeclareCommandQueueTest()
        {
            // Arrange
            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            channelMock.Setup(c => c.QueueDeclare("queue", false, false, false, null))
                       .Returns(new QueueDeclareOk("queue", 0, 0))
                       .Verifiable();
            channelMock.Setup(m => m.BasicQos(0, 1, false));

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(r => r.CreateModel())
                          .Returns(channelMock.Object)
                          .Verifiable();

            var contextMock = new Mock<RabbitMQBusContext>(connectionMock.Object, "bus");

            var receiver = new RabbitMQCommandReceiver(contextMock.Object, "queue");

            receiver.DeclareCommandQueue();
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
