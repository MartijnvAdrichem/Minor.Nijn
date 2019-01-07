using System.Text;
using Minor.Nijn.TestBus;

namespace Minor.Nijn
{
    public class CommandRequestMessage : ICommandMessage
    {
        public string Message { get; }
        public string CorrelationId { get; }

        public string CommandType { get;}
        
        public byte[] EncodeMessage()
        {
            return Encoding.UTF8.GetBytes(Message);
        }

        public CommandRequestMessage(string message, string correlationId, string commandType = "")
        {
            Message = message;
            CorrelationId = correlationId;
            CommandType = commandType;
        }

    }
}
