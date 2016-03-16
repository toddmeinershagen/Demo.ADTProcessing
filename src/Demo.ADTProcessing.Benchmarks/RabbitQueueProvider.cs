using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using BenchmarkDotNet.Analyzers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

using RabbitMQ.Client;

namespace Demo.ADTProcessing.PerfTests
{
    [Config(typeof(Config))]
    public class RabbitQueueProvider
    {
        private IConnection _connection;
        private ConnectionFactory _factory;

        private class Config : ManualConfig
        {
            public Config()
            {
                var targetCount = ConfigurationManager.AppSettings["targetCount"].As<int>();
                Add(Job.Dry
                    .WithTargetCount(targetCount));
                Add(ConsoleLogger.Default);
                Add(PropertyColumn.Method, PropertyColumn.TargetCount, StatisticColumn.Min, StatisticColumn.Mean, StatisticColumn.StdDev, StatisticColumn.Max);
                Add(CsvExporter.Default, MarkdownExporter.GitHub);
                Add(EnvironmentAnalyser.Default);
                UnionRule = ConfigUnionRule.AlwaysUseLocal;
            }
        }

        [Setup]
        public void Setup()
        {
            try
            {
                _connection = CreateConnection();
                SetupExistingQueues(DesiredNumberOfExistingQueues);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        //NOTE:  Would like to make this configurable, but Benchmark framework does not bring app.config into new CLR when running.
        public const string Host = "rcm41vqperapp01";
        public const string User = "perfuser";
        public const string Password = "perfuser1";

        private IConnection CreateConnection()
        {
            var brokerHostUri = $"amqp://{User}:{Password}@{Host}:5672/adt";
            _factory = new ConnectionFactory
            {
                Uri = brokerHostUri,
                RequestedHeartbeat = 10,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            return _factory.CreateConnection();
        }

        private void SetupExistingQueues(int desiredNumberOfQueues)
        {
            var existingNumberOfQueues = GetExistingNumberOfQueues();
            var numberOfQueuesToCreate = Math.Max(desiredNumberOfQueues -  existingNumberOfQueues, 0);
            var actions = new List<Action>();

            foreach (var item in Enumerable.Range(1, numberOfQueuesToCreate))
            {
                Action action = () =>
                {
                    var name = Guid.NewGuid().ToString();
                    CreateQueue(name);
                    Console.Out.WriteLine($"Done - {item}");
                };
                actions.Add(action);
            }

            Parallel.Invoke(actions.ToArray());
        }

        private int GetExistingNumberOfQueues()
        {
            RabbitOverview overview = null;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var byteArray = Encoding.ASCII.GetBytes($"{User}:{Password}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(byteArray));

                var result = client.GetAsync(new Uri($"http://{Host}:15672/api/overview")).Result;
                if (result.IsSuccessStatusCode)
                {
                    overview = result.Content.ReadAsAsync<RabbitOverview>().Result;
                }
            }

            var existingNumberOfQueues = overview?.object_totals.queues ?? 0;
            return existingNumberOfQueues;
        }

        public class RabbitOverview
        {
            public ObjectTotals object_totals { get; set; }

            public class ObjectTotals
            {
                public int queues { get; set; }
            }
        }

        [Params(0, 150, 1500, 15000)]
        public int DesiredNumberOfExistingQueues { get; set; }

        [Benchmark]
        public void CreateAndDestroyQueue()
        {
            var name = Guid.NewGuid().ToString();
            CreateQueue(name);
            DestroyQueue(name);
            _connection.Dispose();
        }

        private void CreateQueue(string name)
        {
            using (var channel = _connection.CreateModel())
            {
                var queue = channel.QueueDeclare(name, true, false, false, null);
                channel.ExchangeDeclare(name, ExchangeType.Fanout, true);
                channel.QueueBind(queue, name, "");

                channel.Close(200, "OK");
            }
        }

        private void DestroyQueue(string name)
        {
            using (var channel = _connection.CreateModel())
            {
                channel.QueueDelete(name);
                channel.ExchangeDelete(name);

                channel.Close(200, "ok");
            }
        }
    }
}
