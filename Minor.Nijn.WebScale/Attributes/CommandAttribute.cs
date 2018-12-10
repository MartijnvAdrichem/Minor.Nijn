using System;

namespace Minor.Nijn.WebScale.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string queuename)
        {
            Queuename = queuename;
        }

        public string Queuename { get; }
    }
}