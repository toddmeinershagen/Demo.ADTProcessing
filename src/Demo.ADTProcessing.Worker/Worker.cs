using System;
using System.Collections.Specialized;
using System.Configuration;

using MassTransit;

using RabbitMQ.Client;

using StructureMap;

namespace Demo.ADTProcessing.Worker
{
    public class Worker
    {
        private static readonly NameValueCollection AppSettings = ConfigurationManager.AppSettings;

        public void Run()
        {
            var workerQueueName = AppSettings["workerQueueName"];
            Console.WriteLine($"{workerQueueName}::Hit ENTER to start.");
            Console.ReadLine();

            var factory = GetConnectionFactory();
            var connection = factory.CreateConnection();

            var container = new Container(cfg =>
            {
                cfg.For<IConnection>().Use(connection);
                cfg.ForConcreteType<AccountSequenceCommandConsumer>();
            });

            var bus = CreateBus(container);

            bus.Start();
            Console.WriteLine($"{workerQueueName}(s) started.  Hit ENTER to end...");
            Console.ReadLine();
            bus.Stop();
        }

        private static IBusControl CreateBus(Container container)
        {
            return Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var busHostUri = AppSettings["busHostUri"];
                var workerQueueName = AppSettings["workerQueueName"];
                var host = sbc.Host(new Uri(busHostUri), h =>
                {
                    h.Username("guest");
                    h.Password("");
                    h.Heartbeat(10);
                });

                sbc.ReceiveEndpoint(host, workerQueueName, ep =>
                {
                    ep.Consumer<AccountSequenceCommandConsumer>(container, cfg =>
                    {
                        var numberOfWorkers = AppSettings["numberOfWorkers"].As<int>();
                        cfg.UseConcurrencyLimit(numberOfWorkers);
                    });
                });
            });
        }

        private static ConnectionFactory GetConnectionFactory()
        {
            var brokerHostUri = AppSettings["brokerHostUri"];
            return new ConnectionFactory
            {
                Uri = brokerHostUri,
                RequestedHeartbeat = 10,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
        }
    }
}