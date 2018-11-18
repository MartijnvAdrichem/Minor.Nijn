using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client.Framing;

namespace Minor.Nijn.TestBus
{
    public class TestBusCommandMessage
    {
        public TestBusCommandMessage(CommandMessage message, BasicProperties props)
        {
            Message = message;
            Props = props;
        }

        public CommandMessage Message { get; set; }
        public BasicProperties Props { get; set; }
    }
}
