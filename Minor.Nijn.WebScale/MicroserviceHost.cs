using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    ///     Listens to incoming events and dispatches them to the appropriate handler
    /// </summary>
    public class MicroserviceHost : IMicroserviceHost
    {
        private readonly List<CommandListener> _commandListeners;
        private static  Assembly _callingAssembly;
        private readonly List<EventListener> _eventListeners;
        public IBusContext<IConnection> Context;


        public MicroserviceHost(IBusContext<IConnection> context, List<EventListener> eventListeners,
            List<CommandListener> commandListeners, IServiceCollection provider, Assembly callingAssembly)
        {
            Context = context;

            if (eventListeners == null) eventListeners = new List<EventListener>();
            if (commandListeners == null) commandListeners = new List<CommandListener>();

            _eventListeners = eventListeners;
            _commandListeners = commandListeners;
            _callingAssembly = callingAssembly;

            if (provider != null) Provider = provider.BuildServiceProvider();
        }

        public IServiceProvider Provider { get; }

        public void StartListening()
        {
            foreach (var eventListener in _eventListeners)
            {
                var methodTopics = eventListener.Topics.Keys.Select(x => x.TopicPattern);

                eventListener.StartListening(this);
                eventListener.MessageReceiver =
                    Context.CreateMessageReceiver(eventListener.EventListenerAttribute.QueueName, methodTopics);
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

        public static Exception CreateException(string messageType, object message)
        {
            var type = _callingAssembly.GetType(messageType);
            return Activator.CreateInstance(type, message) as Exception;
        }
    }
}