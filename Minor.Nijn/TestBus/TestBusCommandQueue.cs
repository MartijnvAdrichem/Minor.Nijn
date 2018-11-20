using RabbitMQ.Client.Framing;

namespace Minor.Nijn.TestBus
{
    public class TestBusCommandMessage
    {
        public TestBusCommandMessage(ICommandMessage message, BasicProperties props)
        {
            Message = message;
            Props = props;
        }

        public ICommandMessage Message { get; set; }
        public BasicProperties Props { get; set; }
    }
}
