using System;
using System.Reflection;

namespace Minor.Nijn.WebScale
{
    public class MethodCommandInfo
    {
        public MethodCommandInfo(Type classType, MethodInfo methodInfo, ParameterInfo methodParameter, Type methodReturnType, string queueName)
        {
            ClassType = classType;
            MethodInfo = methodInfo;
            MethodParameter = methodParameter;
            QueueName = queueName;
            MethodReturnType = methodReturnType;
        }

        public String QueueName { get; set; }
        public Type ClassType { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public ParameterInfo MethodParameter { get; set; }
        public Type MethodReturnType { get; set; }
    }
}
