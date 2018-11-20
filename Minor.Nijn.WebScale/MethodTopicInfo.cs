using System;
using System.Reflection;

namespace Minor.Nijn.WebScale
{
    public class MethodTopicInfo
    {
        public MethodTopicInfo(Type classType, bool hasDefaultConstructor, string topicName, MethodInfo methodInfo, ParameterInfo methodParameter)
        {
            ClassType = classType;
            HasDefaultConstructor = hasDefaultConstructor;
            TopicName = topicName;
            MethodInfo = methodInfo;
            MethodParameter = methodParameter;
        }

        public Type ClassType { get; set; }
        public bool HasDefaultConstructor { get; set; }

        public string TopicName { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public ParameterInfo MethodParameter { get; set; }
    }
}
