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
        //[TestMethod]
        //public void RecieveMessagesCalledCorrectly()
        //{
        //    // Arrange
        //    var channelMock = new Mock<IModel>(MockBehavior.Strict);
        //    channelMock.Setup(c => c.QueueBind("TestQueue1", "Testxchange1", "", null))
        //        .Verifiable();

        //    var consumer = new EventingBasicConsumer(channelMock.Object);
        //    //string queue, bool autoAck, string consumerTag, bool noLocal, bool exclusive, IDictionary< string, object> arguments, IBasicConsumer consumer
        //    channelMock.Setup(c => c.BasicConsume("TestQueue1", true, "", false, false, null, consumer));

        //    channelMock.Setup(c => c.ExchangeDeclare("Testxchange1", "topic", false, false, null))
        //        .Verifiable();

        //    var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
        //    connectionMock.Setup(r => r.CreateModel())
        //        .Returns(channelMock.Object)
        //        .Verifiable();

        //    var context = new RabbitMQBusContext(connectionMock.Object, "Testxchange1");

        //    var target = new RabbitMQMessageReceiver(context, "TestQueue1", null);

        //    // Act
        //    target.DeclareQueue();
        //    target.StartReceivingMessages(message =>
        //    {

        //    });

        //    // Assert
        //    channelMock.VerifyAll();
        //}
    }
}
