using System;

namespace Minor.Nijn
{
    public interface IMessageSender : IDisposable
    {
        void SendMessage(IEventMessage message);
    }
}
