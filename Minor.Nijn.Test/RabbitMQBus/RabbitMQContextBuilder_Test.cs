using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Test;
using Moq;
using RabbitMQ.Client;
using System;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQContextBuilder_Test
    {
        #region WithExchange
        [TestMethod]
        public void WithExchange_WithCorrectExchangeName()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder();

            // Act
            contextBuilder.WithExchange("AMX");

            // Assert
            Assert.AreEqual("AMX", contextBuilder.ExchangeName);
        }
        #endregion

        #region WithAddress
        [TestMethod]
        public void WithAddress_WithCorrectHostnameAndPort()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder();

            // Act
            contextBuilder.WithAddress("localhost", 9000);

            // Assert
            Assert.AreEqual("localhost", contextBuilder.HostName);
            Assert.AreEqual(9000, contextBuilder.Port);
        }
        #endregion

        #region WithCredentials
        [TestMethod]
        public void WithCredentials_WithCorrectUsernameAndPassword()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder();

            // Act
            contextBuilder.WithCredentials("username", "password");

            // Assert
            Assert.AreEqual("username", contextBuilder.UserName);
            string password = TestHelper.GetPrivateField<string>(contextBuilder, "_password");
            Assert.AreEqual("password", password);
        }
        #endregion

        #region ReadFromEnvironmentVariables
        [TestMethod]
        public void ReadFromEnvironmentVariables_WithCorrectExchangeName()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder();
            Environment.SetEnvironmentVariable("ExchangeName", "AMX");

            // Act
            contextBuilder.ReadFromEnvironmentVariables();

            // Assert
            Assert.AreEqual("AMX", contextBuilder.ExchangeName);

            // Cleanup
            Environment.SetEnvironmentVariable("ExchangeName", null);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariables_WithCorrectHostName()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder();
            Environment.SetEnvironmentVariable("HostName", "localhost");

            // Act
            contextBuilder.ReadFromEnvironmentVariables();

            // Assert
            Assert.AreEqual("localhost", contextBuilder.HostName);

            // Cleanup
            Environment.SetEnvironmentVariable("HostName", null);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariables_WithCorrectPort()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder();
            Environment.SetEnvironmentVariable("Port", "9000");

            // Act
            contextBuilder.ReadFromEnvironmentVariables();

            // Assert
            Assert.AreEqual(9000, contextBuilder.Port);

            // Cleanup
            Environment.SetEnvironmentVariable("Port", null);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariables_WithCorrectUserName()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder();
            Environment.SetEnvironmentVariable("UserName", "user");

            // Act
            contextBuilder.ReadFromEnvironmentVariables();

            // Assert
            Assert.AreEqual("user", contextBuilder.UserName);

            // Cleanup
            Environment.SetEnvironmentVariable("UserName", null);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariables_WithCorrectPassword()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder();
            Environment.SetEnvironmentVariable("Password", "secret");

            // Act
            contextBuilder.ReadFromEnvironmentVariables();

            // Assert
            string password = TestHelper.GetPrivateField<string>(contextBuilder, "_password");
            Assert.AreEqual("secret", password);

            // Cleanup
            Environment.SetEnvironmentVariable("Password", null);
        }

        [TestMethod]
        public void ContextBuilderReadFromEnvironmentWithWrongPortThrowsException()
        {
            // Arrange
            Environment.SetEnvironmentVariable("EXCHANGENAME", "bus");
            Environment.SetEnvironmentVariable("USERNAME", "guest");
            Environment.SetEnvironmentVariable("HOSTNAME", "localhost");
            Environment.SetEnvironmentVariable("PASSWORD", "secret");
            Environment.SetEnvironmentVariable("PORT", "fout");
            var contextBuilder = new RabbitMQContextBuilder();

            // Act & Assert
            Assert.ThrowsException<InvalidCastException>(() =>
            {
                contextBuilder.ReadFromEnvironmentVariables();
            });
        }
        #endregion

        #region CreateContext
        [TestMethod]
        public void CreateContext_WithExchangeNameNull_ThrowsArgumentNullException()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder().WithAddress(null, 5672);

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                contextBuilder.CreateContext();
            });
        }

        [TestMethod]
        public void CreateContext_WithExchangeNameEmptyString_ThrowsArgumentException()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder().WithAddress("", 5672);

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() =>
            {
                contextBuilder.CreateContext();
            });
            Assert.AreEqual(nameof(contextBuilder.HostName) + " was empty", exception.Message);
        }

        [TestMethod]
        public void CreateContext_WithNegativePort_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder().WithAddress("localhost", -1);

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                contextBuilder.CreateContext();
            });
            Assert.AreEqual(nameof(contextBuilder.Port), exception.ParamName);
        }

        [TestMethod]
        public void CreateContext_WithPortHigherThan65535_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var contextBuilder = new RabbitMQContextBuilder().WithAddress("localhost", 65536);

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
            {
                contextBuilder.CreateContext();
            });
            Assert.AreEqual(nameof(contextBuilder.Port), exception.ParamName);
        }

        [TestMethod]
        public void CreateContext_DeclaresExchangeWithCorrectExchangeName()
        {
            // Arrange
            var channelMock = new Mock<IModel>();
            channelMock.Setup(c => c.ExchangeDeclare("Exchange3",
                                                     "topic",
                                                     false,
                                                     false,
                                                     null))
                       .Verifiable();
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);
            var connectionFactoryMock = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactoryMock.Setup(f => f.CreateConnection())
                                 .Returns(connectionMock.Object);
            var contextBuilder = new RabbitMQContextBuilder().WithExchange("Exchange3")
                                                             .WithAddress("localhost", 5672);

            // Act
            var context = contextBuilder.CreateContext(connectionFactoryMock.Object);

            // Assert
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void CreateContext_ReturnsContextWithCorrectExchangeName()
        {
            // Arrange
            var channelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);
            var connectionFactoryMock = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactoryMock.Setup(f => f.CreateConnection())
                                 .Returns(connectionMock.Object);
            var contextBuilder = new RabbitMQContextBuilder().WithExchange("Exchange3")
                                                             .WithAddress("localhost", 5672);

            // Act
            var context = contextBuilder.CreateContext(connectionFactoryMock.Object);

            // Assert
            Assert.AreEqual("Exchange3", context.ExchangeName);
        }

        [TestMethod]
        public void CreateContext_ReturnsContextWithCorrectConnection()
        {
            // Arrange
            var channelMock = new Mock<IModel>();
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(c => c.CreateModel())
                          .Returns(channelMock.Object);
            var connectionFactoryMock = new Mock<IConnectionFactory>(MockBehavior.Strict);
            connectionFactoryMock.Setup(f => f.CreateConnection())
                                 .Returns(connectionMock.Object);
            var contextBuilder = new RabbitMQContextBuilder().WithExchange("Exchange3")
                                                             .WithAddress("localhost", 5672);

            // Act
            var context = contextBuilder.CreateContext(connectionFactoryMock.Object);

            // Assert
            Assert.AreEqual(connectionMock.Object, context.Connection);
        }
        #endregion
    }
}
