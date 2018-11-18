using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Minor.Nijn.TestBus
{
    public class TestMessageSender : IMessageSender
    {
        public TestBusContext BusContext { get; }

        public TestMessageSender(TestBusContext busContext)
        {
            BusContext = busContext;
        }

        public void SendMessage(IEventMessage message)
        {
            //casmva.info.bla
            //casmva.#
            var senderExpression = message.RoutingKey ?? "";

            foreach (var testQueue in BusContext.TestQueues.Values)
            {
                foreach (var topicExpression in testQueue.TopicExpressions)
                {
                    if (IsTopicMatch(topicExpression, senderExpression)) {                    
                        testQueue.Queue.Enqueue(message);
                        break;
                    }
                }
            }
        }

        public static bool IsTopicMatch(string s, string matchWith)
        {
            string regexString = s.Replace(".", @"\.").Replace("#", ".+").Replace("*", "[^.]*");
            regexString = "^" + regexString + "$";
            var regex = new Regex(regexString);

            return regex.IsMatch(matchWith);
        }

        public void Dispose()
        {
        }
    }
}
