﻿using System;
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

namespace Demo.ADTProcessing.Worker
{
    public class AccountSequenceCommandConsumer : IConsumer<IAccountSequenceCommand>
    {
        private static readonly NameValueCollection AppSettings = ConfigurationManager.AppSettings;
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
            Console.Out.WriteLine($"{GetQueueName(context)}");

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

                while (consumer.Queue.Dequeue(receiveTimeoutInMiliseconds, out args) && counter < maxMessagesToProcess)
                {
                    var successful = true;
                    var delay = 0;
                    counter++;

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    try
                    {
                        var envelopeJson = Encoding.UTF8.GetString(args.Body);
                        var envelope = JsonConvert.DeserializeObject<MessageEnvelope>(envelopeJson, _settings);
                        var message = envelope.Message as JObject;

                        if (message != null)
                        {
                            var timestamp = message["timestamp"].Value<DateTime>();
                            delay = (DateTime.Now - timestamp).TotalMilliseconds.Rounded();
                        }

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
                }
            }
        }

        private static void SendMetricsEvent(ConsumeContext<IAccountSequenceCommand> context, int delay, int execution, bool successful)
        {
            var workerQueueName = AppSettings["workerQueueName"];
            context
                .Publish<IMetricsEvent>(
                    new
                    {
                        EventType = workerQueueName,
                        DelayInMilliseconds = delay,
                        ExecutionInMilliseconds = execution,
                        Successful = true
                    })
                .Wait();
        }

        private void DoWork(ConsumeContext<IAccountSequenceCommand> context, int counter)
        {
            Console.WriteLine($"{counter:0#}::{GetQueueName(context)}");

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