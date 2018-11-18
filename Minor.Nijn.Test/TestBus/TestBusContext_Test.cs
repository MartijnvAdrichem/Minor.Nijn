using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Minor.Nijn.TestBus.Test
{
    [TestClass]
    public class TestBusContext_Test
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
