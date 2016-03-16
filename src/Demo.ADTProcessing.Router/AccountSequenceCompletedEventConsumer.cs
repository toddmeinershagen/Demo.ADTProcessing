using System;
using System.Collections.Specialized;
using System.Configuration;
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
        private readonly IConsole _console;
        private readonly IAccountSequenceNotifier _accountSequenceNotifier;
        private readonly ISendEndpoint _workerEndpoint;
        private readonly NameValueCollection _appSettings = ConfigurationManager.AppSettings;

        public AccountSequenceCompletedEventConsumer(IConnection connection, IConsole console, IAccountSequenceNotifier accountSequenceNotifier, ISendEndpoint workerEndpoint)
        {
            _connection = connection;
            _console = console;
            _accountSequenceNotifier = accountSequenceNotifier;
            _workerEndpoint = workerEndpoint;
        }

        public Task Consume(ConsumeContext<IAccountSequenceCompletedEvent> context)
        {
            try
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

                                string queueAddress;
                                while (Program.Queues.TryRemove(context.Message.QueueAddress, out queueAddress) == false)
                                {
                                }
                            }
                        }
                    }
                    else
                    {
                        //var busHostUri = _appSettings["busHostUri"];
                        //var workerQueueName = _appSettings["workerQueueName"];
                        //var workerQueueUri = new Uri($"{busHostUri}/{workerQueueName}");

                        try
                        {
                            _accountSequenceNotifier.NotifyWorkers(context, context.Message.QueueAddress);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            throw;
                        }
                    }

                    channel.Close(200, "Ok");
                }

                return Task.Factory.StartNew(() => _console.WriteLine($"{context.Message.QueueAddress}"));
            }
            catch (Exception ex)
            {
                _console.WriteLine("AccountSequenceCompletedEventConsumer::" + ex.Message);
                throw;
            }
            
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