using System;
using System.Linq;
using System.Threading.Tasks;

using Demo.ADTProcessing.Core;
using Demo.ADTProcessing.Router.RabbitMQ;

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
                    if (Program.Queues.ContainsKey(context.Message.QueueAddress))
                    {
                        lock (Lock.SyncRoot)
                        {
                            DeleteQueue(context.Message.QueueAddress);
                        }
                        string queueAddress;
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

                channel.Close(200, "Ok");
            }

            return Console.Out.WriteLineAsync($"{context.Message.QueueAddress}");
        }

        private void DeleteQueue(string addressUrl)
        {
            using (var channel = _connection.CreateModel())
            {
                var name = RabbitMqEndpointAddress.Parse(addressUrl).Name;
                channel.QueueDelete(name);
                channel.ExchangeDelete(name);
                channel.Close(200, "ok");
            }
        }
    }
}