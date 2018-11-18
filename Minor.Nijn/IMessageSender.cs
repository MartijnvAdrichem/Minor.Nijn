using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn
{
    public interface IMessageSender : IDisposable
    {
        void SendMessage(IEventMessage message);
    }
}
