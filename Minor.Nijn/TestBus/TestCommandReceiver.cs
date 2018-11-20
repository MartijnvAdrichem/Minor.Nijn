﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Minor.Nijn.RabbitMQBus;
using RabbitMQ.Client.Framing;

namespace Minor.Nijn.TestBus
{
    public class TestCommandReceiver : ICommandReceiver
    {
        private readonly TestBusContext _testBusContext;
        private bool _isListening;
        private bool _isDeclared;

        public TestCommandReceiver(TestBusContext testBusContext, string queueName)
        {
            QueueName = queueName;
            _testBusContext = testBusContext;
        }

        public string QueueName { get; }

        public void DeclareCommandQueue()
        {
            if (_isDeclared)
            {
                throw new BusConfigurationException("Already declared queue " + QueueName);
            }

            _testBusContext.DeclareCommandQueue(QueueName);
            _isDeclared = true;
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            if (_isListening)
            {
                throw new BusConfigurationException("Already listening to queuename " + QueueName);
            }
            new Task(() =>
            {
                var queue = _testBusContext.CommandQueues[QueueName];

                while (true)
                {
                    if (queue.Count == 0)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    CommandResponseMessage response = null;
                    var command = queue.Dequeue();
                    string type = command.Props.Type;

                    try
                    {
                        response = callback.Invoke(command.Message as CommandRequestMessage);
                    }
                    catch (Exception e)
                    {
                        var realException = e.InnerException;
                        response = new CommandResponseMessage(realException.Message, realException.GetType().ToString(), command.Props.CorrelationId);
                        type = realException.GetType().FullName;
                    }
                    finally
                    {
                        _testBusContext.CommandQueues[command.Props.ReplyTo].Enqueue(new TestBusCommandMessage(response, 
                            new BasicProperties()
                            {
                                CorrelationId =  command.Props.CorrelationId,
                                Type = type
                            }));
                    }

                   
                }
            }).Start();
            _isListening = true;
        }

        public void Dispose()
        {

        }
    }
}
