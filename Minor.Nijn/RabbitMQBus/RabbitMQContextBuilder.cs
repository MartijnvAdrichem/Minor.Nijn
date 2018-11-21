using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;

namespace Minor.Nijn.RabbitMQBus
{
    public class RabbitMQContextBuilder
    {
        public string ExchangeName { get; private set; }
        public string HostName { get; private set; }
        public int Port { get; private set; }
        public string UserName { get; private set; }
        private string _password;
        private ILogger _log;

        public RabbitMQContextBuilder()
        {
            _log = NijnLogger.CreateLogger<RabbitMQContextBuilder>();
        }

        public RabbitMQContextBuilder WithExchange(string exchangeName)
        {
            _log.LogTrace("Creating Context with exchangeName " + exchangeName);
            ExchangeName = exchangeName;
            return this;
        }

        public RabbitMQContextBuilder WithAddress(string hostName, int port)
        {
            _log.LogTrace("Creating Context with hostname " + hostName + " And port " + port );
            HostName = hostName;
            Port = port;
            return this;
        }

        public RabbitMQContextBuilder WithCredentials(string userName, string password)
        {
            _log.LogTrace("Creating Context with username " + userName);
            UserName = userName;
            _password = password;
            return this;
        }

        public RabbitMQContextBuilder ReadFromEnvironmentVariables()
        {
            if (TryGetFromEnvironmentVariable("EXCHANGENAME", out var exchangeName))
            {
                ExchangeName = exchangeName;
            }
            if (TryGetFromEnvironmentVariable("HOSTNAME", out var hostName))
            {
                HostName = hostName;
            }
            if (TryGetFromEnvironmentVariable("PORT", out var portString))
            {
                try
                {
                    Port = Convert.ToInt32(portString);
                }
                catch (Exception)
                {
                    throw new InvalidCastException("Could not convert PORT environment variable to a number");
                }
            }
            if (TryGetFromEnvironmentVariable("USERNAME", out var userName))
            {
                UserName = userName;
            }
            if (TryGetFromEnvironmentVariable("PASSWORD", out var password))
            {
                _password = password;
            }

            _log.LogTrace($"Creating Context with the use of environment variables. username {UserName}, hostname {HostName}, port {Port}, exchangeName {ExchangeName}");

            return this;
        }

        /// <summary>
        /// Creates a context with 
        ///  - an opened connection (based on HostName, Port, UserName and Password)
        ///  - a declared Topic-Exchange (based on ExchangeName)
        /// </summary>
        /// <returns></returns>
        public RabbitMQBusContext CreateContext(IConnectionFactory factory = null)
        {
            _log.LogInformation("Creating RabbitMQ Connection");
            if (HostName == null)
            {
                throw new ArgumentNullException(nameof(HostName));
            }
            if (HostName == "")
            {
                throw new ArgumentException(nameof(HostName) + " was empty");
            }
            if (Port < 0 || Port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(Port));
            }

            factory = factory ?? new ConnectionFactory()
            {
                HostName = HostName,
                UserName = UserName,
                Password = _password,
                Port = Port
            };

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

        private bool TryGetFromEnvironmentVariable(string key, out string variable)
        {
            variable = Environment.GetEnvironmentVariable(key);
            if (variable == null)
            {
                return false;
            }

            return true;
        }
    }
}
