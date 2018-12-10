using Newtonsoft.Json;

namespace Minor.Nijn.WebScale.Test
{
    public class ExampleEvent : DomainEvent
    {
        public ExampleEvent(string routingKey) : base(routingKey)
        {
        }

        [JsonProperty]
        public string Message { get; set; }
    }
}
