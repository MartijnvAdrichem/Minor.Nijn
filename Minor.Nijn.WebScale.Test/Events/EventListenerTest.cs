using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.WebScale.Events;
using Moq;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public class EventListenerTest
    {
        [TestMethod]
        public void EventListenerIsCalledWithCorrectEvent()
        {
            var serviceMock = new Mock<IServiceMock>(MockBehavior.Strict);
            serviceMock.Setup(d => d.PerformAction());

            using (var context = new TestBusContext())
            {
                var builder = new MicroserviceHostBuilder()
                    .RegisterDependencies((services) =>
                    {
                        services.AddSingleton(serviceMock.Object);
                    })
                    .WithContext(context)
                    .UseConventions();

                using (var host = builder.CreateHost())
                {
                    host.StartListening();

                    var eventPublisher = new EventPublisher(context);
                    var exampleEvent = new ExampleEvent("XMPL.ExampleEvent")
                    {
                        Message = "Example message."
                    };
                    eventPublisher.Publish(exampleEvent);
                }

                var eventListenerIsCalled = EventListenerMock.ResetEvent.WaitOne(1000);
                Assert.IsTrue(eventListenerIsCalled);
                var actualEvent = EventListenerMock.ExampleEvent;
                Assert.AreEqual("XMPL.ExampleEvent", actualEvent.RoutingKey);
                Assert.AreEqual("Example message.", actualEvent.Message);
                serviceMock.VerifyAll();
            }
        }
    }
}
