using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IMicroserviceHost
    {
        public IBusContext<IConnection> Context;
        private readonly List<EventListener> _eventListeners;
        private readonly List<CommandListener> _commandListeners;

        public IServiceProvider Provider { get; }


        public MicroserviceHost(IBusContext<IConnection> context, List<EventListener> eventListeners, List<CommandListener> commandListeners, IServiceCollection provider)
        {
            Context = context;

            if(eventListeners == null) eventListeners = new List<EventListener>();
            if(commandListeners == null) commandListeners = new List<CommandListener>();

            _eventListeners = eventListeners;
            _commandListeners = commandListeners;

            if (provider != null)
            {
                Provider = provider.BuildServiceProvider();
            } 
            

        }

        public void StartListening()
        {
                foreach (var eventListener in _eventListeners)
                {
                    var methodTopics = eventListener.Topics.Keys.Select(x => x.TopicPattern);

                    eventListener.StartListening(this);
                    eventListener.MessageReceiver = Context.CreateMessageReceiver(eventListener.EventListenerAttribute.QueueName, methodTopics);
                    eventListener.MessageReceiver.DeclareQueue();
                    
                    eventListener.MessageReceiver.StartReceivingMessages(eventListener.Handle);
                }

                foreach (var commandListener in _commandListeners)
                {
                    commandListener.DeclareQueue(Context);
                    commandListener.StartListening(this);
                }
        }




        public void Dispose()
        {
            Context.Dispose();
            _eventListeners.ForEach(e => e.Dispose());
            _commandListeners.ForEach(e => e.Dispose());
        }

        public object CreateInstanceOfType(Type type)
        {
            return ActivatorUtilities.CreateInstance(Provider, type);
        }
    }
}
