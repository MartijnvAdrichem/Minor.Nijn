using System;

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
