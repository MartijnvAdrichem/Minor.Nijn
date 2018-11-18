using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;

namespace Minor.Nijn.Test.TestBus
{
    [TestClass]
    public class TestMessageSenderTest
    {

        [TestMethod]
        public void IsTopicMatchTest()
        {
            Assert.IsTrue(TestMessageSender.IsTopicMatch("Minor.Nijn.BerichtVerstuurd","Minor.Nijn.BerichtVerstuurd"));
            Assert.IsTrue(TestMessageSender.IsTopicMatch("Minor.Nijn.*","Minor.Nijn.BerichtVerstuurd"));
            Assert.IsTrue(TestMessageSender.IsTopicMatch("Minor.*.*","Minor.Nijn.BerichtVerstuurd"));
            Assert.IsTrue(TestMessageSender.IsTopicMatch("*.*.*","Minor.Nijn.BerichtVerstuurd"));
            Assert.IsTrue(TestMessageSender.IsTopicMatch("Minor.Nijn.#", "Minor.Nijn.BerichtVerstuurd"));
            Assert.IsTrue(TestMessageSender.IsTopicMatch("Minor.#", "Minor.Nijn.BerichtVerstuurd"));
            Assert.IsTrue(TestMessageSender.IsTopicMatch("#", "Minor.Nijn.BerichtVerstuurd"));
            Assert.IsTrue(TestMessageSender.IsTopicMatch("#.BerichtVerstuurd", "Minor.Nijn.BerichtVerstuurd"));
            Assert.IsTrue(TestMessageSender.IsTopicMatch("Minor.*.BerichtVerstuurd", "Minor.Nijn.BerichtVerstuurd"));

            Assert.IsFalse(TestMessageSender.IsTopicMatch("Minor.Nijn.BerichtVerstuurd.2", "Minor.Nijn.BerichtVerstuurd"));
            Assert.IsFalse(TestMessageSender.IsTopicMatch("Minor.Nijn", "Minor.Nijn.BerichtVerstuurd"));
            Assert.IsFalse(TestMessageSender.IsTopicMatch("Minor", "Minor.Nijn.BerichtVerstuurd"));
            Assert.IsFalse(TestMessageSender.IsTopicMatch("Mva.Minor.Nijn.BerichtVerstuurd", "Minor.Nijn.BerichtVerstuurd"));


        }

        [TestMethod]
        public void SendMessageWithCorrectTopicAddsToQueue()
        {
            var context = new TestBusContext();

            var sender = context.CreateMessageSender();
            context.DeclareQueue("receiver1", new List<string>{"receiver.info"});
            context.DeclareQueue("receiver2", new List<string>{"receiver.*.info"});

            var message = new EventMessage("receiver.info", "receiver");
            sender.SendMessage(message);

            Assert.AreEqual(1, context.TestQueues["receiver1"].Queue.Count);
            Assert.AreEqual(0, context.TestQueues["receiver2"].Queue.Count);
        }

        [TestMethod]
        public void MultipleMessagesAddToQueue()
        {
            var context = new TestBusContext();

            var sender = context.CreateMessageSender();
            context.DeclareQueue("receiver1", new List<string> { "receiver.info" });
            var message = new EventMessage("receiver.info", "receiver");

            sender.SendMessage(message);
            sender.SendMessage(message);
            sender.SendMessage(message);

            Assert.AreEqual(3, context.TestQueues["receiver1"].Queue.Count);
        }
    }
}
