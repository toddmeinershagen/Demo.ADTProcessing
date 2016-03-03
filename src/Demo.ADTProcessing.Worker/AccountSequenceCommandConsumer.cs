using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Demo.ADTProcessing.Core;

using MassTransit;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

using MassTransit.Internals.Reflection;
using MassTransit.Serialization;
using MassTransit.Serialization.JsonConverters;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NLog;

namespace Demo.ADTProcessing.Worker
{
    public class AccountSequenceCommandConsumer : IConsumer<IAccountSequenceCommand>
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly NameValueCollection AppSettings = ConfigurationManager.AppSettings;
        private readonly IConnection _connection;
        private readonly IConsole _console;

        public AccountSequenceCommandConsumer(IConnection connection, IConsole console)
        {
            _connection = connection;
            _console = console;
        }

        public Task Consume(ConsumeContext<IAccountSequenceCommand> context)
        {
            _console.WriteLine($"{GetQueueName(context)}");

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
                var receiveTimeoutInMiliseconds = AppSettings["receiveTimeoutInMilliseconds"].As<int>();
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(queueName, false, consumer);
                var counter = 0;
                BasicDeliverEventArgs args;
                var maxMessagesToProcess = AppSettings["maxMessagesToProcess"].As<int>();

                var stopwatch = new Stopwatch();

                while (counter < maxMessagesToProcess && consumer.Queue.Dequeue(receiveTimeoutInMiliseconds, out args))
                {
                    var pickupTimestamp = DateTime.Now;
                    var successful = true;
                    var delay = 0;
                    IADTCommand adtCommand = null;
                    counter++;

                    stopwatch.Reset();
                    stopwatch.Start();

                    try
                    {
                        var envelopeJson = Encoding.UTF8.GetString(args.Body);
                        var envelope = JObject.Parse(envelopeJson);
                        adtCommand = envelope.SelectToken("message")?.ToObject<ADTCommand>();
                        delay = (pickupTimestamp - adtCommand.Timestamp).TotalMilliseconds.Rounded();

                        DoWork(context, counter);
                        channel.BasicAck(args.DeliveryTag, false);
                    }
                    catch (Exception)
                    {
                        successful = false;
                    }

                    stopwatch.Stop();
                    var execution = stopwatch.Elapsed.TotalMilliseconds.Rounded();

                    SendMetricsEvent(context, delay, execution, successful);

                    if (Logger.IsInfoEnabled && adtCommand != null)
                        Logger.Info($"{adtCommand.FacilityId},{adtCommand.AccountNumber},{adtCommand.Sequence}");
                }
            }
        }

        private static void SendMetricsEvent(ConsumeContext<IAccountSequenceCommand> context, int delay, int execution, bool successful)
        {
            var workerQueueName = AppSettings["workerQueueName"];
            context
                .Publish<IMetricsEvent>(
                    new MetricsEvent
                    {
                        EventType = workerQueueName,
                        DelayInMilliseconds = delay,
                        ExecutionInMilliseconds = execution,
                        Successful = successful
                    })
                .Wait();
        }

        private void DoWork(ConsumeContext<IAccountSequenceCommand> context, int counter)
        {
            _console.WriteLine($"{counter:0#}::{GetQueueName(context)}");

            var maxDelayInProcessing = AppSettings["maxProcessingDelayInSeconds"].As<int>();
            Thread.Sleep(TimeSpan.FromSeconds(GetRandomNumber(maxDelayInProcessing)));
        }

        private string GetQueueName(ConsumeContext<IAccountSequenceCommand> context)
        {
            return new Uri(context.Message.QueueAddress).Segments.LastOrDefault();
        }

        private int GetRandomNumber(int maxNumber)
        {
            if (maxNumber == 0)
                return 0;

            return Math.Abs(Guid.NewGuid().GetHashCode() % maxNumber) + 1;
        }
    }
}