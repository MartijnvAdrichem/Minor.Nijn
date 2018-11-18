using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Creates and Configures a MicroserviceHost
    /// For example:
    ///     var builder = new MicroserviceHostBuilder()
    ///             .SetLoggerFactory(...)
    ///             .RegisterDependencies((services) =>
    ///                 {
    ///                     services.AddTransient<IFoo,Foo>();
    ///                 })
    ///             .WithBusOptions(new BusOptions(exchangeName: "MVM.TestExchange"))
    ///             .UseConventions();
    /// </summary>
    public class MicroserviceHostBuilder
    {
        private IBusContext<IConnection> _context;
        private List<EventListener> _eventListeners;
        private List<CommandListener> _commandListeners;
        static IServiceCollection _services;

        private ILogger _log;

        public MicroserviceHostBuilder()
        {
            _log = NijnLogger.CreateLogger<MicroserviceHostBuilder>();
        }
        /// <summary>
        /// Configures the connection to the message broker
        /// </summary>
        public MicroserviceHostBuilder WithContext(IBusContext<IConnection> context)
        {

            _context = context;
            return this;
        }

        /// <summary>
        /// Scans the assemblies for EventListeners and adds them to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder UseConventions()
        {
            var assembly = Assembly.GetCallingAssembly();

            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                var eventListenerAttribute = type.GetCustomAttribute<EventListenerAttribute>();
                if (eventListenerAttribute == null) continue;

                if (_eventListeners == null)
                {
                    _eventListeners = new List<EventListener>();
                }

               BuildEventListener(eventListenerAttribute, type);

                

            }
            return this;
        }

        private void BuildEventListener(EventListenerAttribute eventListenerAttribute, Type classType)
        {
            var queueExists = _eventListeners.FirstOrDefault(el =>
                el.EventListenerAttribute.QueueName == eventListenerAttribute.QueueName);
            
            if(queueExists == null)
            {
                var eventListener = new EventListener { EventListenerAttribute = eventListenerAttribute, Class = classType };
                BuildTopics(classType, eventListener);
                _eventListeners.Add(eventListener);
            }
            else
            {
                BuildTopics(classType, queueExists);
            }
        }

        private void BuildTopics(Type classType, EventListener eventListener)
        {
            var methods = classType.GetMethods();
            if (eventListener.Topics == null)
            {
                eventListener.Topics = new Dictionary<TopicAttribute, List<MethodTopicInfo>>();
            }

            if (_commandListeners == null)
            {
                _commandListeners = new List<CommandListener>();
            }

            foreach (var methodInfo in methods)
            {
                var topicAttributes = methodInfo.GetCustomAttributes<TopicAttribute>().ToList();
                var commandAttribute = methodInfo.GetCustomAttribute<CommandAttribute>();

                var methodParams = methodInfo.GetParameters();
                if (methodParams.Length > 1 && (topicAttributes.Count > 0 || commandAttribute != null))
                {
                    throw new InvalidOperationException(
                        "Method " + methodInfo.Name + " has multiple parameters, this is not allowed.");
                }

                var firstParam = (methodParams.Length == 0) ? null : methodParams[0];
                var returntype = methodInfo.ReturnType;
                foreach (var topicAttribute in topicAttributes)
                {
                    BuildMethodTopic(classType, eventListener, methodInfo, topicAttribute, firstParam);
                }

                if (commandAttribute != null)
                {
                    var methodCommandInfo = new MethodCommandInfo(classType, methodInfo, firstParam, returntype, commandAttribute.Queuename);
                    CommandListener commandListener = new CommandListener(methodCommandInfo);
                    _commandListeners.Add(commandListener);
                }
            }
        }

        private static void BuildMethodTopic(Type classType, EventListener eventListener, MethodInfo methodInfo, TopicAttribute topicAttribute, ParameterInfo firstParam)
        {
            bool hasDefaultConstructor = classType.GetConstructor(Type.EmptyTypes) != null;

            var methodTopicInfo = new MethodTopicInfo(classType, hasDefaultConstructor,
                topicAttribute.TopicPattern, methodInfo, firstParam);

            if (eventListener.Topics.ContainsKey(topicAttribute))
            {
                eventListener.Topics[topicAttribute].Add(methodTopicInfo);
            }
            else
            {
                eventListener.Topics.Add(topicAttribute, new List<MethodTopicInfo>() { methodTopicInfo });
            }
        }

        /// <summary>
        /// Manually adds EventListeners to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder AddEventListener<T>()
        {
            var type = typeof(T);
            var eventListenerAttribute = type.GetCustomAttribute<EventListenerAttribute>();
            if (eventListenerAttribute == null) return this;

            if (_eventListeners == null)
            {
                _eventListeners = new List<EventListener>();
            }

            BuildEventListener(eventListenerAttribute, type);

            return this;
        }

        /// <summary>
        /// Configures logging functionality for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) return this;
            NijnLogger.LoggerFactory = loggerFactory;
            return this;
        }

        /// <summary>
        /// Configures Dependency Injection for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder RegisterDependencies(Action<IServiceCollection> servicesConfiguration)
        {
            _services = new ServiceCollection();
            servicesConfiguration(_services);
            return this;
        }

        /// <summary>
        /// Creates the MicroserviceHost, based on the configurations
        /// </summary>
        /// <returns></returns>
        public MicroserviceHost CreateHost()
        {
            if (_context == null) 
            {
                _log.LogError("Context is not correctly configurated");
                throw new ArgumentNullException();
            }

            var microServiceHost = new MicroserviceHost(_context, _eventListeners, _commandListeners, _services);
            return microServiceHost;
        }
    }
}
