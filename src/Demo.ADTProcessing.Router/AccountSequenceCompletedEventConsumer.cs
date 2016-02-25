using System;
using System.Linq;
using System.Threading.Tasks;

using Demo.ADTProcessing.Core;

using MassTransit;

using RabbitMQ.Client;

namespace Demo.ADTProcessing.Router
{
    public class AccountSequenceCompletedEventConsumer : IConsumer<IAccountSequenceCompletedEvent>
    {
        private readonly IConnection _connection;

        public AccountSequenceCompletedEventConsumer(IConnection connection)
        {
            _connection = connection;
        }

        public Task Consume(ConsumeContext<IAccountSequenceCompletedEvent> context)
        {
            using (var channel = _connection.CreateModel())
            {
                var address = new Uri(context.Message.QueueAddress);
                var queueName = address.Segments.Last();
                var queue = channel.QueueDeclarePassive(queueName);

                if (queue.MessageCount == 0)
                {
                    string queueAddress;
                    if (Program.Queues.ContainsKey(context.Message.QueueAddress))
                    {
                        while (Program.Queues.TryRemove(context.Message.QueueAddress, out queueAddress) == false)
                        {
                        }
                    }
                }
                else
                {
                    context
                    .Publish<IAccountSequenceCommand>(new { QueueAddress = address })
                    .Wait();
                }
            }

            return Console.Out.WriteLineAsync($"{context.Message.QueueAddress}");
        }
    }
}