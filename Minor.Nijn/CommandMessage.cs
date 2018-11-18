using System.Text;

namespace Minor.Nijn
{
    public class CommandMessage
    {
        public string Message { get; }
        public string MessageType { get; }
        public string CorrelationId { get; }

        public CommandMessage(string message, string messageType, string correlationId)
        {
            Message = message;
            MessageType = messageType;
            CorrelationId = correlationId;
        }

        public byte[] EncodeMessage()
        {
            return Encoding.UTF8.GetBytes(Message);
        }
    }
}
