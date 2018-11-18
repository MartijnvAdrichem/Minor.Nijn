using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Exceptions;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQContextBuilder
    {
        private ILogger _log;
        public RabbitMQContextBuilder()
        {

            _log = NijnLogger.CreateLogger<RabbitMQContextBuilder>();

        }

        public string ExchangeName { get; private set; }
        public string HostName { get; private set; }
        public int Port { get; private set; }
        public string UserName { get; private set; }
        private string _password;

        public RabbitMQContextBuilder WithExchange(string exchangeName)
        {
            _log.LogTrace("Creating Context with exchangeName " + exchangeName);
            ExchangeName = exchangeName;
            return this;    // for method chaining
        }

        public RabbitMQContextBuilder WithAddress(string hostName, int port)
        {
            _log.LogTrace("Creating Context with hostname " + hostName + " And port " + port );
            HostName = hostName;
            Port = port;
            return this;    // for method chaining
        }

        public RabbitMQContextBuilder WithCredentials(string userName, string password)
        {
            _log.LogTrace("Creating Context with username " + userName);
            UserName = userName;
            _password = password;
            return this;    // for method chaining
        }

        public RabbitMQContextBuilder ReadFromEnvironmentVariables()
        {

            UserName =  Environment.GetEnvironmentVariable("USERNAME");
            _password = Environment.GetEnvironmentVariable("PASSWORD");
            HostName = Environment.GetEnvironmentVariable("HOSTNAME");
            ExchangeName = Environment.GetEnvironmentVariable("EXCHANGENAME");

            try
            {
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("PORT"));
            }
            catch (Exception)
            {
                throw new InvalidCastException("Could not convert PORT environment variable to a number");
            }
           

            _log.LogTrace($"Creating Context with the use of environment variables. username {UserName}, hostname {HostName}, port {Port}, exchangeName {ExchangeName}");

            return this;    // for method chaining and reading from 
        }

        /// <summary>
        /// Creates a context with 
        ///  - an opened connection (based on HostName, Port, UserName and Password)
        ///  - a declared Topic-Exchange (based on ExchangeName)
        /// </summary>
        /// <returns></returns>
        public RabbitMQBusContext CreateContext()
        {
            _log.LogInformation("Creating RabbitMQ Connection");
            var factory = new ConnectionFactory() { HostName = HostName, UserName = UserName, Password = _password, Port = Port};
            try
            {
                var connection = factory.CreateConnection();
                var context = new RabbitMQBusContext(connection, ExchangeName);

                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(ExchangeName, type: "topic");
                }

                _log.LogInformation("RabbitMQ connection succesfully created");
                return context;
            }
            catch (BrokerUnreachableException e)
            {
                _log.LogError("Could not connect with the RabbitMQ Environment");
                throw e;
            }
           
        }
    }
}
