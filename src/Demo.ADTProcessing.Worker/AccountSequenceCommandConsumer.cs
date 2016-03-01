using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
using MassTransit.Util;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Demo.ADTProcessing.Worker
{
    public class AccountSequenceCommandConsumer : IConsumer<IAccountSequenceCommand>
    {
        private readonly NameValueCollection _appSettings = ConfigurationManager.AppSettings;
        private readonly IConnection _connection;
        private readonly JsonSerializerSettings _settings;

        public AccountSequenceCommandConsumer(IConnection connection)
        {
            _connection = connection;
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new InterfaceProxyConverter(new DynamicImplementationBuilder()));
            _settings.Converters.Add(new ListJsonConverter());
            _settings.Converters.Add(new MessageDataJsonConverter());
            _settings.ContractResolver = new JsonContractResolver();
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

                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var envelopeJson = Encoding.UTF8.GetString(args.Body);
                        var envelope = JsonConvert.DeserializeObject<MessageEnvelope>(envelopeJson, _settings);
                        var message = envelope.Message as JObject;

                        DoWork(context, counter);
                        channel.BasicAck(args.DeliveryTag, false);

                        stopwatch.Stop();

                        var delay = Round((DateTime.Now - message["timestamp"].Value<DateTime>()).TotalMilliseconds);
                        var execution = Round(stopwatch.Elapsed.TotalMilliseconds);

                        context
                            .Publish<MetricsEvent>(
                                new
                                {
                                    EventType = "Demo.ADTProcessing.Worker",
                                    DelayInMilliseconds = delay,
                                    ExecutionInMilliseconds = execution,
                                    Successful = true
                                })
                            .Wait();

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

        private int Round(double value)
        {
            return Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));
        }

        private void DoWork(ConsumeContext<IAccountSequenceCommand> context, int counter)
        {
            Console.WriteLine($"{counter:0#}::{context.Message.QueueAddress}");

            var maxDelayInProcessing = _appSettings["maxProcessingDelayInSeconds"].As<int>();
            Thread.Sleep(TimeSpan.FromSeconds(GetRandomNumber(maxDelayInProcessing)));
    }

        private int GetRandomNumber(int maxNumber)
        {
            if (maxNumber == 0)
                return 0;

            return Math.Abs(Guid.NewGuid().GetHashCode() % maxNumber) + 1;
        }
    }

    public class ADTCommand : IADTCommand
    {
        public int FacilityId { get; set; }
        public int AccountNumber { get; set; }
        public DateTime Timestamp { get; set; }
    }
}