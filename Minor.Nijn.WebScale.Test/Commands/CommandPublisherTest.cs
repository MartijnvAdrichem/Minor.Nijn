using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.RabbitMQBus;
using Minor.Nijn.WebScale.Commands;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public partial class CommandPublisherTest
    {

        [TestMethod]
        public async Task CommandPublisherPublishTest()
        {

            var sender = new Mock<ICommandSender>(MockBehavior.Strict);
            var responseCommand = new TestCommand() {Message = "message2"};

            sender.Setup(m => m.SendCommandAsync(It.IsAny<CommandRequestMessage>(), "queue")).Returns(
                 Task.FromResult(new CommandResponseMessage(JsonConvert.SerializeObject(responseCommand), typeof(TestCommand).FullName, null)));

            var iBusContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            iBusContextMock.Setup(m => m.CreateCommandSender()).Returns(sender.Object);

            var target = new CommandPublisher(iBusContextMock.Object);
            var testCommand = new TestCommand();

            TestCommand result = await target.Publish<TestCommand>(testCommand, "queue");

            Assert.IsInstanceOfType(result, typeof(TestCommand));
            Assert.AreEqual("message2", (result as TestCommand).Message);
        }

        [TestMethod]
        public async Task CommandPublisherPublishExceptionTest()
        {

            var sender = new Mock<ICommandSender>(MockBehavior.Strict);
            var responseCommand = new TestCommand() { Message = "message2" };

            sender.Setup(m => m.SendCommandAsync(It.IsAny<CommandRequestMessage>(), "queue")).Returns(
                Task.FromResult(new CommandResponseMessage("error", typeof(ArgumentException).FullName, null)));

            var iBusContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            iBusContextMock.Setup(m => m.CreateCommandSender()).Returns(sender.Object);

            var target = new CommandPublisher(iBusContextMock.Object);
            var testCommand = new TestCommand();

             var exception = await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                 {
                     await target.Publish<TestCommand>(testCommand, "queue");
                 });
            Assert.AreEqual("error", exception.Message);
               
        }

        [TestMethod]
        public async Task CommandPublisherPublishUnknownExceptionThrowsInvalidCastException()
        {

            var sender = new Mock<ICommandSender>(MockBehavior.Strict);
            var responseCommand = new TestCommand() { Message = "message2" };

            sender.Setup(m => m.SendCommandAsync(It.IsAny<CommandRequestMessage>(), "queue")).Returns(
                Task.FromResult(new CommandResponseMessage("error", "RandomException", null)));

            var iBusContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            iBusContextMock.Setup(m => m.CreateCommandSender()).Returns(sender.Object);

            var target = new CommandPublisher(iBusContextMock.Object);
            var testCommand = new TestCommand();

            var exception = await Assert.ThrowsExceptionAsync<InvalidCastException>(async () =>
            {
                await target.Publish<TestCommand>(testCommand, "queue");
            });
            Assert.AreEqual("an unknown exception occured (message error), exception type was RandomException", exception.Message);

        }
    }
}
