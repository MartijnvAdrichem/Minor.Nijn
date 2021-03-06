﻿using RabbitMQ.Client.Framing;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Minor.Nijn.RabbitMQBus;

namespace Minor.Nijn.TestBus
{
    public class TestCommandSender : ICommandSender
    {
        private TestBusContext Context { get; }
        private readonly string _replyQueueName;

        public readonly ConcurrentDictionary<string, TaskCompletionSource<CommandResponseMessage>> CallbackMapper =
            new ConcurrentDictionary<string, TaskCompletionSource<CommandResponseMessage>>();

        public TestCommandSender(TestBusContext context)
        {
            Context = context;
            _replyQueueName = GenerateRandomQueueName();
            context.DeclareCommandQueue(_replyQueueName);

            new Task(() =>
            {
                var queue = context.CommandQueues[_replyQueueName];

                while (true)
                {
                    if (queue.Count <= 0)
                    {
                        Thread.Sleep(250);
                        continue;
                    }

                    var response = queue.Dequeue();

                    if (!CallbackMapper.TryRemove(response.Props.CorrelationId, out TaskCompletionSource<CommandResponseMessage> tcs))
                        return;
                    var commandResponse = response.Message;;
                    tcs.TrySetResult(commandResponse as CommandResponseMessage);

                }
            }).Start();

        }
        public Task<CommandResponseMessage> SendCommandAsync(CommandRequestMessage request, string queueName)
        {
            BasicProperties props = new BasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = _replyQueueName;

            var tcs = new TaskCompletionSource<CommandResponseMessage>();
            CallbackMapper.TryAdd(correlationId, tcs);

            Context.CommandQueues[queueName].Enqueue(new TestBusCommandMessage(request, props));

            return tcs.Task;
        }

        public void CancelCommand(string correlationId)
        {
            throw new NotImplementedException();
        }

        public string GenerateRandomQueueName()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[30];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
        public void Dispose()
        {
        }
    }
}
