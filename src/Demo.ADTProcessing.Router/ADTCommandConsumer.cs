using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Demo.ADTProcessing.Core;
using Demo.ADTProcessing.Router.RabbitMQ;

using MassTransit;

using RabbitMQ.Client;

namespace Demo.ADTProcessing.Router
{
    public class ADTCommandConsumer : IConsumer<IADTCommand>
    {
        private readonly IConnection _connection;

        public ADTCommandConsumer(IConnection connection)
        {
            _connection = connection;
        }

        public Task Consume(ConsumeContext<IADTCommand> context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine($"{context.Message.FacilityId}-{context.Message.AccountNumber}");

            var key = GetKey(context.Message);
            var endpointAddressUrl = $"rabbitmq://localhost/adt/Demo.ADTProcessing.Router.{key}";
            var originalTimestamp = context.Message.Timestamp;

            //NOTE:     Sad case - worker tries to read from a queue that was just deleted after sequence command was sent.
            //          Had to increase the lock because the worker started having issues trying to read from a queue that had been deleted.  May want to do this as a single thread
            //          that processes both types of messages so that there is no need for synchronization.
            //NOTE:     Would like to limit this lock, by only locking for creation of queue and send to temporary queue...the rest can remain outside of that.
            lock (Lock.SyncRoot)
            {
                if (Program.Queues.ContainsKey(endpointAddressUrl) == false)
                {
                    //lock (Lock.SyncRoot)
                    //{
                        //NOTE:     Sad case - delete queue before publish to bound queue
                        //          Originally, put a lock around just the bind and around the delete to make sure that a queue doesn't get deleted and you try to publish to it.
                        BindQueue(endpointAddressUrl);

                        context
                            .Publish<IAccountSequenceCommand>(new {QueueAddress = endpointAddressUrl})
                            .Wait();
                    //}
                    while (Program.Queues.TryAdd(endpointAddressUrl, endpointAddressUrl) == false)
                    {
                    }
                }

                var endpointUri = new Uri(endpointAddressUrl);

                
                context.Message.Timestamp = DateTime.Now;
                context
                    .GetSendEndpoint(endpointUri).Result
                    .Send(context.Message).Wait();
            }

            stopwatch.Stop();


            var delay = (DateTime.Now - originalTimestamp).TotalMilliseconds.Rounded();
            var execution = stopwatch.Elapsed.TotalMilliseconds.Rounded();

            return context
                .Publish<MetricsEvent>(
                    new
                    {
                        EventType = "Demo.ADTProcessing.Router",
                        DelayInMilliseconds = delay,
                        ExecutionInMilliseconds = execution,
                        Successful = true
                    });
        }

        public string GetKey(IADTCommand command)
        {
            return $"{command.FacilityId}-{command.AccountNumber}";
        }

        private void BindQueue(string addressUrl)
        {
            using (var channel = _connection.CreateModel())
            {
                var name = RabbitMqEndpointAddress.Parse(addressUrl).Name;
                var queue = channel.QueueDeclare(name, true, false, false, null);
                
                channel.ExchangeDeclare(name, ExchangeType.Fanout, true);
                channel.QueueBind(queue, name, "");

                channel.Close(200, "ok");
            }
        }
    }
}