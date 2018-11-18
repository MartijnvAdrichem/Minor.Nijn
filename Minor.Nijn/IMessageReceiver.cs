using System;
using System.Collections.Generic;

namespace Minor.Nijn
{
    public interface IMessageReceiver : IDisposable
    {
        string QueueName { get; }
        IEnumerable<string> TopicExpressions { get; }

        void DeclareQueue();
        void StartReceivingMessages(EventMessageReceivedCallback Callback);
    }

    public delegate void EventMessageReceivedCallback(IEventMessage eventMessage);
}
