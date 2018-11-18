using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;

namespace Minor.Nijn.TestBus.Test
{
    [TestClass]
    public class TestBusContextTest
    {

        [TestMethod]
        public void CreateMessageSenderReturnsNewMessageSender()
        {

        }

        [TestMethod]
        public void CreateNewQueueAddsQueueToDictionary()
        {
            TestBusContext testbusContext = new TestBusContext();

            var receiver = testbusContext.CreateMessageReceiver("receiver", null);
            receiver.DeclareQueue();

            Assert.AreEqual(1, testbusContext.TestQueues.Count);
            Assert.IsNotNull(testbusContext.TestQueues["receiver"]);
        }
    }
}
