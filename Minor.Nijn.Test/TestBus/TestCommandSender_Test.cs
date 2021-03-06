﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.Nijn.TestBus.Test
{
    [TestClass]
    public class TestCommandSender_Test
    {
        [TestMethod]
        public void CreateTestCommandSenderDeclaresNewQueue()
        {
            TestBusContext context = new TestBusContext();
            var sender = context.CreateCommandSender();

            Assert.IsInstanceOfType(typeof(TestCommandSender), sender.GetType().BaseType);
            Assert.AreEqual(1, context.CommandQueues.Count);
        }

        [TestMethod]
        public async Task SendCommandAsync()
        {
            TestBusContext context = new TestBusContext();
            var sender = (TestCommandSender)context.CreateCommandSender();

            var generatedQueue = context.CommandQueues.First().Value;

            context.DeclareCommandQueue("queue");

            var message = new CommandRequestMessage("message", null);
            var task = sender.SendCommandAsync(message, "queue");
            Assert.AreEqual(1, sender.CallbackMapper.Count);

            var dequeue = context.CommandQueues["queue"].Dequeue();
            generatedQueue.Enqueue(new TestBusCommandMessage(new CommandResponseMessage("message", typeof(string).FullName, null), dequeue.Props));

            var result = await task;

            Assert.AreEqual("message", result.Message);
        }

        [TestMethod]
        public void GenerateRandomQueueNameTest()
        {
            TestBusContext context = new TestBusContext();
            var sender = (TestCommandSender)context.CreateCommandSender();

            var result = sender.GenerateRandomQueueName();
            Assert.AreEqual(30, result.Length);
        }
    }
}
