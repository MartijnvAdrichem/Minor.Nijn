using System;
using System.Collections.Generic;
using System.Text;
using Minor.Nijn.RabbitMQBus;

namespace Minor.Nijn
{
    public class CommandResponseMessage : ICommandMessage
    {
        public string Message { get; }
        public string MessageType { get; }
        public string CorrelationId { get; }

        public byte[] EncodeMessage()
        {
            return Encoding.UTF8.GetBytes(Message);
        }



        public CommandResponseMessage(string message, string messageType, string correlationId )
        {
            Message = message;
            CorrelationId = correlationId;
            MessageType = messageType;
        }

        public CommandResponseMessage(string message, Type messageType, string correlationId )
        {
            Message = message;
            CorrelationId = correlationId;
            MessageType = messageType.FullName;
        }
  
    }
}
