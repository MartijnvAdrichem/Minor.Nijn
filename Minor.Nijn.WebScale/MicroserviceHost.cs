using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;
        private readonly List<EventListener> _eventListeners;
        public IBusContext<IConnection> Context;

        private bool _queuesDeclared = false;

        public DateTime LastReceivedMessageTime = DateTime.Now;

        private bool _exitOnTimeout;
        private TimeSpan _timeout;
        private ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        private bool _isDisposed; 
        public MicroserviceHost(IBusContext<IConnection> context, List<EventListener> eventListeners,
            List<CommandListener> commandListeners, IServiceCollection provider, Assembly callingAssembly, bool exitOnTimeout, TimeSpan timeout)
        {
            Context = context;

            if (eventListeners == null) eventListeners = new List<EventListener>();
            if (commandListeners == null) commandListeners = new List<CommandListener>();

            _eventListeners = eventListeners;
            _commandListeners = commandListeners;
            _callingAssembly = callingAssembly;
            _exitOnTimeout = exitOnTimeout;
            _timeout = timeout;
            _logger = NijnLogger.CreateLogger<MicroserviceHost>();

            if (provider != null) Provider = provider.BuildServiceProvider();
        }

        public IServiceProvider Provider { get; }

        public void CreateQueues()
        {
            foreach (var eventListener in _eventListeners)
            {
                var methodTopics = eventListener.Topics.Keys.Select(x => x.TopicPattern);

                eventListener.StartListening(this);
                eventListener.MessageReceiver =
                    Context.CreateMessageReceiver(eventListener.EventListenerAttribute.QueueName, methodTopics);
                eventListener.MessageReceiver.DeclareQueue();
            }

            foreach (var commandListener in _commandListeners)
            {
                commandListener.DeclareQueue(Context);
            }

            _queuesDeclared = true;
        }

        public void StartListening()
        {
            if (!_queuesDeclared)
            {
                CreateQueues();
            }

            foreach (var eventListener in _eventListeners)
            {
                eventListener.MessageReceiver.StartReceivingMessages(eventListener.Handle);
            }

            foreach (var commandListener in _commandListeners)
            {
                commandListener.StartListening(this);
            }

            if (_exitOnTimeout)
            {
                LastReceivedMessageTime = DateTime.Now;
                CheckIdle();
            }
        }

        public void StartListeningInOtherThread()
        {
            new Thread(() =>
            {
                StartListening();
                _manualResetEvent.WaitOne();

            }).Start();
        }
        
        public void Dispose()
        {
            _manualResetEvent.Set();
            Context.Dispose();
            _eventListeners.ForEach(e => e.Dispose());
            _commandListeners.ForEach(e => e.Dispose());
        }

        public object CreateInstanceOfType(Type type)
        {
            try
            {
                var instance = ActivatorUtilities.CreateInstance(Provider, type);
                return instance;
            }
            catch(InvalidOperationException e)
            {
                _logger.LogError("Could not make instance of class {}", e.Message);
                return null;
            }
        }

        public static Exception CreateException(string messageType, object message)
        {
            var type = _callingAssembly.GetType(messageType);
            return Activator.CreateInstance(type, message) as Exception;
        }

        private void CheckIdle()
        {
            new Thread(() =>
            {
                while (!_isDisposed)
                {
                    if (IsIdle(_timeout))
                    {
                        _logger.LogWarning("TIMED OUT: shutting down connection with " + Context.ExchangeName);
                        Dispose();
                        return;
                    }

                    Thread.Sleep(1000);
                }
            }).Start();
        }

        public bool IsIdle(TimeSpan timeout)
        {
           return DateTime.Now - LastReceivedMessageTime > timeout;
        }
    }
}