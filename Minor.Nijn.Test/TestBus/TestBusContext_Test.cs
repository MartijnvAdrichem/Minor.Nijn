using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Minor.Nijn.TestBus.Test
{
    [TestClass]
    public class TestBusContext_Test
    {
        #region CreateMessageSender
        [TestMethod]
        public void CreateMessageSender_ReturnsTestMessageSender()
        {
            // Arrange
            TestBusContext context = new TestBusContext();

            // Act
            var sender = context.CreateMessageSender();

            // Assert
            Assert.IsInstanceOfType(sender, typeof(TestMessageSender));
        }
        #endregion

        #region CreateMessageReceiver
        [TestMethod]
        public void CreateMessageReceiver_ReturnsTestMessageReceiver()
        {
            // Arrange
            TestBusContext context = new TestBusContext();

            // Act
            var receiver = context.CreateMessageReceiver("TestQueue", new List<string> { "test.routing.key" });

            // Assert
            Assert.IsInstanceOfType(receiver, typeof(TestMessageReceiver));
        }
        #endregion

        #region CreateCommandSender
        [TestMethod]
        public void CreateCommandSender_ReturnsTestCommandSender()
        {
            // Arrange
            TestBusContext context = new TestBusContext();

            // Act
            var sender = context.CreateCommandSender();

            // Assert
            Assert.IsInstanceOfType(sender, typeof(TestCommandSender));
        }
        #endregion

        #region CreateCommandReceiver
        [TestMethod]
        public void CreateCommandReceiver_ReturnsTestCommandReceiver()
        {
            // Arrange
            TestBusContext context = new TestBusContext();

            // Act
            var receiver = context.CreateCommandReceiver("TestQueue");

            // Assert
            Assert.IsInstanceOfType(receiver, typeof(TestCommandReceiver));
        }
        #endregion

        #region DeclareQueue
        [TestMethod]
        public void DeclareQueue_AddsQueue()
        {
            // Arrange
            TestBusContext context = new TestBusContext();

            // Act
            context.DeclareQueue("TestQueue", new List<string> { "test.routing.key" });

            // Assert
            Assert.AreEqual(1, context.TestQueues.Count);
            Assert.IsNotNull(context.TestQueues["TestQueue"]);
        }
        #endregion

        #region DeclareCommandQueue
        [TestMethod]
        public void DeclareCommandQueue_AddsQueue()
        {
            // Arrange
            TestBusContext context = new TestBusContext();

            // Act
            context.DeclareCommandQueue("TestQueue");

            // Assert
            Assert.AreEqual(1, context.CommandQueues.Count);
            Assert.IsNotNull(context.CommandQueues["TestQueue"]);
        }
        #endregion
    }
}
