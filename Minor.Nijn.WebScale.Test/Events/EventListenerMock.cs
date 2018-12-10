using Minor.Nijn.WebScale.Attributes;
using System.Threading;

namespace Minor.Nijn.WebScale.Test
{
    [EventListener("XMPL.ExampleQueue")]
    public class EventListenerMock
    {
        private readonly IServiceMock _service;
        public static ManualResetEvent ResetEvent { get; } = new ManualResetEvent(false);
        public static ExampleEvent ExampleEvent { get; private set; }

        public EventListenerMock(IServiceMock service)
        {
            _service = service;
        }

        [Topic("XMPL.ExampleEvent")]
        public void Handles(ExampleEvent exampleEvent)
        {
            _service.PerformAction();
            ResetEvent.Set();
            ExampleEvent = exampleEvent;
        }
    }
}
