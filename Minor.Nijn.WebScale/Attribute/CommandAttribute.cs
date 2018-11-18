using System;
using System.Collections.Generic;
using System.Text;
using Minor.Nijn.RabbitMQBus;

namespace Minor.Nijn.WebScale
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public String Queuename { get; }

        public CommandAttribute(string queuename)
        {
            Queuename = queuename;
        }
    }
}
