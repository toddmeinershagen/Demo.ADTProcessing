using System;
using System.Linq;
using System.Threading.Tasks;

using Demo.ADTProcessing.Core;

using MassTransit;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace Demo.ADTProcessing.Worker
{
    public class AccountSequenceCommandConsumer : IConsumer<IAccountSequenceCommand>
    {
        private readonly IConnection _connection;

        public AccountSequenceCommandConsumer(IConnection connection)
        {
            _connection = connection;
        }

        public Task Consume(ConsumeContext<IAccountSequenceCommand> context)
        {
            Console.Out.WriteLine($"{context.Message.QueueAddress}");

            ProcessMessages(context);

            return context.Publish<IAccountSequenceCompletedEvent>(new {context.Message.QueueAddress});
        }

        private void ProcessMessages(ConsumeContext<IAccountSequenceCommand> context)
        {
            using (var channel = _connection.CreateModel())
            {
                //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                var address = new Uri(context.Message.QueueAddress);
                var queueName = address.Segments.Last();

                using (var mre = new ManualResetEventSlim(false))
                using (var timer = new System.Timers.Timer(1000))
                {
                    timer.Elapsed += (sender, args) =>
                    {
                        mre.Set();
                    };

                    var counter = 0;
                    var consumer = new EventingBasicConsumer(channel);
                    
                    consumer.Received += (sender, args) =>
                    {
                        timer.Stop();

                        counter++;
                        DoWork(context, counter);
                        channel.BasicAck(args.DeliveryTag, false);

                        //if (_counter == 7)
                        //{
                        //    mre.Set();
                        //}

                        timer.Start();
                    };

                    channel.QueueDeclarePassive(queueName);
                    channel.BasicConsume(queueName, false, consumer);
                    WaitHandle.WaitAll(new[] {mre.WaitHandle}, Timeout.Infinite);
                }

                //BasicGetResult result;

                //do
                //{
                //    counter++;

                //    result = channel.BasicGet(queueName, false);

                //    DoWork(context, counter);

                //    if (result != null)
                //        channel.BasicAck(result.DeliveryTag, false);

                //} while (result != null);
            }
        }

        private void DoWork(ConsumeContext<IAccountSequenceCommand> context, int counter)
        {
            Console.WriteLine($"{counter:0#}::{context.Message.QueueAddress}");
            //Thread.Sleep(TimeSpan.FromSeconds(GetRandomNumber(4)));
        }

        private int GetRandomNumber(int maxNumber)
        {
            return Math.Abs(Guid.NewGuid().GetHashCode() % maxNumber) + 1;
        }
    }
}