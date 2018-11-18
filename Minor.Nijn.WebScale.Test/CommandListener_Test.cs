using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Linq;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public class CommandListener_Test
    {
        [TestMethod]
        public void CommandListenerConstructorTest()
        {
            var methodCommandInfo = new MethodCommandInfo(null, null, null, null, "queue");
            var target = new CommandListener(methodCommandInfo);

            Assert.AreEqual("queue", target.QueueName);
        }

        [TestMethod]
        public void DeclareQueueTest()
        {
            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(m => m.DeclareCommandQueue()).Verifiable();

            var mock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            mock.Setup(m => m.CreateCommandReceiver("queue")).Returns(commandReceiverMock.Object);

            var methodCommandInfo = new MethodCommandInfo(null, null, null,null, "queue");
            var target = new CommandListener(methodCommandInfo);

            target.DeclareQueue(mock.Object);

        }

        private class TestClass
        {
            public bool Called { get; private set; }
            public TestCommand TestCalled(TestCommand command)
            {
                Called = true;
                return new TestCommand() {Message = "Message2"};
            }
        }

        private class TestCommand : DomainCommand
        {
            public string Message { get; set; }
        }

        [TestMethod]
        public void StartListeningToCommandsTest()
        {
            var hostMock = new Mock<IMicroserviceHost>();
            hostMock.Setup(m => m.CreateInstanceOfType(typeof(TestClass))).Returns(new TestClass());

            var methodCommandInfo = new MethodCommandInfo(null, null, null, null, "queue");
            var target = new CommandListener(methodCommandInfo);

            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(m => m.DeclareCommandQueue()).Verifiable();
            commandReceiverMock.Setup(m => m.StartReceivingCommands(target.Handle));

            var iBusContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            iBusContextMock.Setup(m => m.CreateCommandReceiver("queue")).Returns(commandReceiverMock.Object);
           
            target.DeclareQueue(iBusContextMock.Object);
            target.StartListening(hostMock.Object);

        }

        [TestMethod]
        public void HandleTest()
        {
            var methodCommandInfo = new MethodCommandInfo(typeof(TestClass), 
                typeof(TestClass).GetMethod("TestCalled"), 
                typeof(TestClass).GetMethod("TestCalled").GetParameters().First(),
                typeof(TestClass).GetMethod("TestCalled").ReturnType,
                "queue");
           var target = new CommandListener(methodCommandInfo);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(m => m.CreateInstanceOfType(typeof(TestClass))).Returns(new TestClass());

            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(m => m.DeclareCommandQueue()).Verifiable();
            commandReceiverMock.Setup(m => m.StartReceivingCommands(target.Handle));

            var iBusContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            iBusContextMock.Setup(m => m.CreateCommandReceiver("queue")).Returns(commandReceiverMock.Object);

            target.DeclareQueue(iBusContextMock.Object);
            target.StartListening(hostMock.Object);

            TestCommand command = new TestCommand() {Message = "message"};

            var message = new CommandMessage(JsonConvert.SerializeObject(command), typeof(TestCommand).FullName, null);

            var result = target.Handle(message);

            var objectResult = JsonConvert.DeserializeObject<TestCommand>(result.Message);
            Assert.AreEqual("Message2", objectResult.Message);
        }
    }
}
