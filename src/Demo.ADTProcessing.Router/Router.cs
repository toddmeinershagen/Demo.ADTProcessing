using System;
using System.Collections.Specialized;
using System.Configuration;

using Demo.ADTProcessing.Core;

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
            var routerQueueName = AppSettings["routerQueueName"];
            Console.WriteLine($"{routerQueueName}::Hit ENTER to start.");
            Console.ReadLine();

            var factory = GetConnectionFactory();
            var connection = factory.CreateConnection();

            var container = new Container(cfg =>
            {
                cfg.For<IConnection>().Use(connection);
                cfg.For<IConsole>().Use<NullConsole>();
                cfg.ForConcreteType<ADTCommandConsumer>();
                cfg.ForConcreteType<AccountSequenceCompletedEventConsumer>();
            });

            //TODO:  May need to check to see if a router is already up on another box...and if so, to shut down.
            var bus = CreateBus(container);

            container.Configure(cfg =>
            {
                cfg.For<IBusControl>()
                    .Use(bus);
                cfg.Forward<IBus, IBusControl>();
            });

            bus.Start();

            Console.WriteLine($"{routerQueueName} started.  Hit ENTER to end...");
            Console.ReadLine();

            bus.Stop();
        }

        private static IBusControl CreateBus(Container container)
        {
            return Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var busHostUri = new Uri(AppSettings["busHostUri"]);
                var username = busHostUri.UserInfo.Split(':')[0];
                var password = busHostUri.UserInfo.Split(':')[1];

                var host = sbc.Host(busHostUri, h =>
                {
                    h.Username(username);
                    h.Password(password);
                    h.Heartbeat(10);
                });

                //TODO:  May need to configure additional endpoints to handle specific facilities because the router will get behind.
                var routerQueueName = AppSettings["routerQueueName"];
                sbc.ReceiveEndpoint(host, routerQueueName, ep =>
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

                //NOTE:  removed the second endpoint to make sure that removals don't leave queues stranded.  (was also causing skipped messages in the ADT command processor.)
                //sbc.ReceiveEndpoint(host, "Demo.ADTProcessing.Router.AccountSequenceCompleted", ep =>
                //{

                //});
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