using System;
using System.Collections.Specialized;
using System.Configuration;

using MassTransit;

using RabbitMQ.Client;

using StructureMap;

namespace Demo.ADTProcessing.Router
{
    public class Router
    {
        private static readonly NameValueCollection AppSettings = ConfigurationManager.AppSettings;

        public void Run()
        {
            Console.WriteLine("Demo.ADTProcessing.Router::Hit ENTER to start.");
            Console.ReadLine();

            var factory = GetConnectionFactory();
            var connection = factory.CreateConnection();

            var container = new Container(cfg =>
            {
                cfg.For<IConnection>().Use(connection);
                cfg.ForConcreteType<ADTCommandConsumer>();
                cfg.ForConcreteType<AccountSequenceCompletedEventConsumer>();
            });

            var bus = CreateBus(container);

            container.Configure(cfg =>
            {
                cfg.For<IBusControl>()
                    .Use(bus);
                cfg.Forward<IBus, IBusControl>();
            });

            bus.Start();

            Console.WriteLine("Router started.  Hit ENTER to end...");
            Console.ReadLine();

            bus.Stop();
        }

        private static IBusControl CreateBus(Container container)
        {
            return Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri("rabbitmq://localhost/adt"), h =>
                {
                    h.Username("guest");
                    h.Password("");
                    h.Heartbeat(10);
                });

                sbc.ReceiveEndpoint(host, "Demo.ADTProcessing.Router", ep =>
                {
                    //ep.Exclusive = true;
                    ep.Consumer<ADTCommandConsumer>(container, cfg =>
                    {
                        var numberOfAdtCommandWorkers = AppSettings["numberOfADTCommandWorkers"].As<int>();
                        cfg.UseConcurrencyLimit(numberOfAdtCommandWorkers);
                    });

                    //ep.Exclusive = true;
                    ep.Consumer<AccountSequenceCompletedEventConsumer>(container, cfg =>
                    {
                        var numberOfAccountSequenceCompletedWorkers =
                            AppSettings["numberOfAccountSequenceCompletedWorkers"].As<int>();
                        cfg.UseConcurrencyLimit(numberOfAccountSequenceCompletedWorkers);
                    });
                });

                //NOTE:  removed the second endpoint to make sure that removals don't leave queues stranded.
                //sbc.ReceiveEndpoint(host, "Demo.ADTProcessing.Router.AccountSequenceCompleted", ep =>
                //{
                //});
            });
        }

        private static ConnectionFactory GetConnectionFactory()
        {
            return new ConnectionFactory
            {
                Uri = "amqp://guest:guest@localhost:5672/adt",
                RequestedHeartbeat = 10,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
        }
    }
}