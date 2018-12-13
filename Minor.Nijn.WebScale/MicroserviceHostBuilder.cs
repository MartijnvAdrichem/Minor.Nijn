using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    ///     Creates and Configures a MicroserviceHost
    ///     For example:
    ///     var builder = new MicroserviceHostBuilder()
    ///     .SetLoggerFactory(...)
    ///     .RegisterDependencies((services) =>
    ///     {
    ///     services.AddTransient
    ///     <IFoo, Foo>
    ///         ();
    ///         })
    ///         .WithBusOptions(new BusOptions(exchangeName: "MVM.TestExchange"))
    ///         .UseConventions();
    /// </summary>
    public class MicroserviceHostBuilder
    {
        private static IServiceCollection _services;

        private readonly ILogger _log;
        private List<CommandListener> _commandListeners;
        private IBusContext<IConnection> _context;
        private List<EventListener> _eventListeners;
        private Assembly _callingAssembly;

        public MicroserviceHostBuilder()
        {
            _log = NijnLogger.CreateLogger<MicroserviceHostBuilder>();
        }

        /// <summary>
        ///     Configures the connection to the message broker
        /// </summary>
        public MicroserviceHostBuilder WithContext(IBusContext<IConnection> context)
        {
            _context = context;
            return this;
        }

        /// <summary>
        ///     Scans the assemblies for EventListeners and adds them to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder UseConventions()
        {
            _callingAssembly = Assembly.GetCallingAssembly();

            var types = _callingAssembly.GetTypes();
            foreach (var type in types)
            {
                var eventListenerAttribute = type.GetCustomAttribute<EventListenerAttribute>();
                var commandListenerAttribute = type.GetCustomAttribute<CommandListenerAttribute>();

                if (eventListenerAttribute == null && commandListenerAttribute == null) continue;

                if (_eventListeners == null) _eventListeners = new List<EventListener>();
                if (_commandListeners == null) _commandListeners = new List<CommandListener>();

                if (eventListenerAttribute != null) BuildEventListener(eventListenerAttribute, type);

                if (commandListenerAttribute != null) BuildCommandListener(type);
            }

            return this;
        }

        /// <summary>
        ///     Builds a CommandListener class based on the class type
        /// </summary>
        /// <param name="classType"></param>
        private void BuildCommandListener(Type classType)
        {
            var methods = classType.GetMethods();

            foreach (var methodInfo in methods)
            {
                var commandAttribute = methodInfo.GetCustomAttribute<CommandAttribute>();
                if (commandAttribute == null) continue;

                var firstParam = GetParameterInfo(methodInfo);
                var returnType = methodInfo.ReturnType;

                var methodCommandInfo = new MethodCommandInfo(classType, methodInfo, firstParam, returnType,
                    commandAttribute.Queuename);
                var commandListener = new CommandListener(methodCommandInfo);

                _commandListeners.Add(commandListener);
            }
        }

        /// <summary>
        ///     Build a EventListener class based on the attribute and the class type
        /// </summary>
        /// <param name="eventListenerAttribute"></param>
        /// <param name="classType"></param>
        private void BuildEventListener(EventListenerAttribute eventListenerAttribute, Type classType)
        {
            var queueExists = _eventListeners.FirstOrDefault(el =>
                el.EventListenerAttribute.QueueName == eventListenerAttribute.QueueName);

            if (queueExists == null)
            {
                var eventListener = new EventListener
                    {EventListenerAttribute = eventListenerAttribute, Class = classType};
                BuildTopics(classType, eventListener);
                _eventListeners.Add(eventListener);
            }
            else
            {
                BuildTopics(classType, queueExists);
            }
        }

        /// <summary>
        ///     Builds all methodtopics based on the topics
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="eventListener"></param>
        private void BuildTopics(Type classType, EventListener eventListener)
        {
            var methods = classType.GetMethods();
            if (eventListener.Topics == null)
                eventListener.Topics = new Dictionary<TopicAttribute, List<MethodTopicInfo>>();


            foreach (var methodInfo in methods)
            {
                var topicAttributes = methodInfo.GetCustomAttributes<TopicAttribute>().ToList();
                if (topicAttributes.Count == 0) continue;

                var firstParam = GetParameterInfo(methodInfo);
                foreach (var topicAttribute in topicAttributes)
                    BuildMethodTopic(classType, eventListener, methodInfo, topicAttribute, firstParam);
            }
        }

        /// <summary>
        ///     Gets the paremeterInfo from a method
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private ParameterInfo GetParameterInfo(MethodInfo methodInfo)
        {
            var methodParams = methodInfo.GetParameters();
            if (methodParams.Length > 1)
                throw new InvalidOperationException(
                    "Method " + methodInfo.Name + " has multiple parameters, this is not allowed.");

            var firstParam = methodParams.Length == 0 ? null : methodParams[0];
            return firstParam;
        }

        /// <summary>
        ///     Builds the methodTopic
        /// </summary>
        /// <param name="classType"></param>
        /// <param name="eventListener"></param>
        /// <param name="methodInfo"></param>
        /// <param name="topicAttribute"></param>
        /// <param name="firstParam"></param>
        private void BuildMethodTopic(Type classType, EventListener eventListener, MethodInfo methodInfo,
            TopicAttribute topicAttribute, ParameterInfo firstParam)
        {
            var hasDefaultConstructor = classType.GetConstructor(Type.EmptyTypes) != null;

            var methodTopicInfo = new MethodTopicInfo(classType, hasDefaultConstructor,
                topicAttribute.TopicPattern, methodInfo, firstParam);

            if (eventListener.Topics.ContainsKey(topicAttribute))
                eventListener.Topics[topicAttribute].Add(methodTopicInfo);
            else
                eventListener.Topics.Add(topicAttribute, new List<MethodTopicInfo> {methodTopicInfo});
        }

        /// <summary>
        ///     Manually adds EventListeners to the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder AddEventListener<T>()
        {
            var type = typeof(T);
            var eventListenerAttribute = type.GetCustomAttribute<EventListenerAttribute>();
            if (eventListenerAttribute == null) return this;

            if (_eventListeners == null) _eventListeners = new List<EventListener>();

            BuildEventListener(eventListenerAttribute, type);

            return this;
        }

        public MicroserviceHostBuilder AddCommandListener<T>()
        {
            var type = typeof(T);
            var commandListenerAttribute = type.GetCustomAttribute<CommandListenerAttribute>();
            if (commandListenerAttribute == null) return this;

            if (_commandListeners == null) _commandListeners = new List<CommandListener>();

            BuildCommandListener(type);

            return this;
        }

        /// <summary>
        ///     Configures logging functionality for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) return this;
            NijnLogger.LoggerFactory = loggerFactory;
            return this;
        }

        /// <summary>
        ///     Configures Dependency Injection for the MicroserviceHost
        /// </summary>
        public MicroserviceHostBuilder RegisterDependencies(Action<IServiceCollection> servicesConfiguration)
        {
            _services = new ServiceCollection();
            servicesConfiguration(_services);
            return this;
        }

        /// <summary>
        ///     Creates the MicroserviceHost, based on the configurations
        /// </summary>
        /// <returns></returns>
        public MicroserviceHost CreateHost()
        {
            if (_context == null)
            {
                _log.LogError("Context is not correctly configurated");
                throw new ArgumentNullException();
            }

            var microServiceHost = new MicroserviceHost(_context, _eventListeners, _commandListeners, _services, _callingAssembly);
            return microServiceHost;
        }
    }
}