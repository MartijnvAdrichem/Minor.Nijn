using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.RabbitMQBus;

namespace Minor.Nijn.Test.RabbitMQBus
{
    [TestClass]
    public class RabbitMQContextBuilderTest
    {

        [TestMethod]
        public void ContextBuilderSetExchangeTest()
        {
            var target = new RabbitMQContextBuilder();
            target.WithExchange("bus");
            
            Assert.AreEqual("bus", target.ExchangeName);
        }

        [TestMethod]
        public void ContextBuilderSetUsernameAndPasswordTest()
        {
            var target = new RabbitMQContextBuilder();
            target.WithCredentials("guest", "secret");
            
            Assert.AreEqual("guest", target.UserName);
        }

        [TestMethod]
        public void ContextBuilderSetAdressTest()
        {
            var target = new RabbitMQContextBuilder();
            target.WithAddress("localhost", 1234);
            
            Assert.AreEqual("localhost", target.HostName);
            Assert.AreEqual(1234, target.Port);
        }


        [TestMethod]
        public void ContextBuilderReadFromEnvironment()
        {
            Environment.SetEnvironmentVariable("EXCHANGENAME", "bus");
            Environment.SetEnvironmentVariable("USERNAME", "guest");
            Environment.SetEnvironmentVariable("HOSTNAME", "localhost");
            Environment.SetEnvironmentVariable("PASSWORD", "secret");
            Environment.SetEnvironmentVariable("PORT", "1234");
            
            var target = new RabbitMQContextBuilder();
            target.ReadFromEnvironmentVariables();


            Assert.AreEqual("bus", target.ExchangeName);
            Assert.AreEqual("guest", target.UserName);
            Assert.AreEqual("localhost", target.HostName);
            Assert.AreEqual(1234, target.Port);
        }

        [TestMethod]
        public void ContextBuilderReadFromEnvironmentWithWrongPortThrowsException()
        {
            Environment.SetEnvironmentVariable("EXCHANGENAME", "bus");
            Environment.SetEnvironmentVariable("USERNAME", "guest");
            Environment.SetEnvironmentVariable("HOSTNAME", "localhost");
            Environment.SetEnvironmentVariable("PASSWORD", "secret");
            Environment.SetEnvironmentVariable("PORT", "fout");
            

            var target = new RabbitMQContextBuilder();
            Assert.ThrowsException<InvalidCastException>(() => { target.ReadFromEnvironmentVariables(); });
        }

     
    }
}
