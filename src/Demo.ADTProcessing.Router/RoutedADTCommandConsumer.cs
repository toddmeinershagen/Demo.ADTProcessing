using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

using Demo.ADTProcessing.Core;
using Demo.ADTProcessing.Router.RabbitMQ;

using MassTransit;

using RabbitMQ.Client;

namespace Demo.ADTProcessing.Router
{
    public class RoutedADTCommandConsumer : IConsumer<IADTCommand>
    {
        private readonly IConnection _connection;
        private readonly IConsole _console;
        private readonly IAccountSequenceNotifier _accountSequenceNotifier;
        private readonly ISendEndpoint _workerEndpoint;
        private readonly NameValueCollection _appSettings = ConfigurationManager.AppSettings;

        public RoutedADTCommandConsumer(IConnection connection, IConsole console, IAccountSequenceNotifier accountSequenceNotifier, ISendEndpoint workerEndpoint)
        {
            _connection = connection;
            _console = console;
            _accountSequenceNotifier = accountSequenceNotifier;
            _workerEndpoint = workerEndpoint;
        }

        public Task Consume(ConsumeContext<IADTCommand> context)
        {
            try
            {
                var delay = (DateTime.Now - context.Message.Timestamp).TotalMilliseconds.Rounded();
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                _console.WriteLine($"{context.Message.FacilityId}-{context.Message.AccountNumber}");

                var busHostUri = _appSettings["busHostUri"];
                var routerQueueName = _appSettings["routerQueueName"];

                var key = GetKey(context.Message);
                var endpointAddressUrl = $"{busHostUri}/{routerQueueName}.{key}";
                bool queueDoesNotExist;

                //NOTE:     Sad case - worker tries to read from a queue that was just deleted after sequence command was sent.
                //          Had to increase the lock because the worker started having issues trying to read from a queue that had been deleted.  May want to do this as a single thread
                //          that processes both types of messages so that there is no need for synchronization.
                //NOTE:     Would like to limit this lock, by only locking for creation of queue and send to temporary queue...the rest can remain outside of that.
                lock (Lock.SyncRoot)
                {
                    queueDoesNotExist = Program.Queues.ContainsKey(endpointAddressUrl) == false;

                    if (queueDoesNotExist)
                    {
                        //lock (Lock.SyncRoot)
                        //{
                        //NOTE:     Sad case - delete queue before publish to bound queue
                        //          Originally, put a lock around just the bind and around the delete to make sure that a queue doesn't get deleted and you try to publish to it.
                        BindQueue(endpointAddressUrl);
                        //}
                    }

                    var endpointUri = new Uri(endpointAddressUrl);

                    //NOTE:  Should we update the timestamp or just measure from the first time the message was inserted?
                    //context.Message.Timestamp = DateTime.Now;
                    context
                        .Send(endpointUri, context.Message).Wait();
                }

                if (queueDoesNotExist)
                {
                    //var workerQueueName = _appSettings["workerQueueName"];
                    //var workerQueueUri = new Uri($"{busHostUri}/{workerQueueName}");

                    _accountSequenceNotifier.NotifyWorkers(context, endpointAddressUrl).Wait();

                    while (Program.Queues.TryAdd(endpointAddressUrl, endpointAddressUrl) == false)
                    {
                    }
                }

                stopwatch.Stop();

                var execution = stopwatch.Elapsed.TotalMilliseconds.Rounded();

                return context
                    .Publish<IMetricsEvent>(
                        new MetricsEvent()
                        {
                            EventType = routerQueueName,
                            DelayInMilliseconds = delay,
                            ExecutionInMilliseconds = execution,
                            Successful = true
                        });
            }
            catch (Exception ex)
            {
                _console.WriteLine("RoutedADTCommandConsumer::" + ex.Message);
                throw;
            }
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
                //var args = new Dictionary<string, object> { { "x-expires", 15000 } };
                //var queue = channel.QueueDeclare(name, true, false, false, args);

                channel.ExchangeDeclare(name, ExchangeType.Fanout, true);
                //channel.ExchangeDeclare(name, ExchangeType.Fanout, true, false, args);
                channel.QueueBind(queue, name, "");

                channel.Close(200, "ok");
            }
        }
    }
}