using System;

using MassTransit;

using RabbitMQ.Client;

using StructureMap;

namespace Demo.ADTProcessing.Worker
{
    public class Worker
    {
        public void Run()
        {
            Console.WriteLine("Demo.ADTProcessing.Worker::Hit ENTER to start.");
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
            Console.WriteLine("Worker(s) started.  Hit ENTER to end...");
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

                sbc.ReceiveEndpoint(host, "Demo.ADTProcessing.Worker", ep =>
                {
                    ep.Consumer<AccountSequenceCommandConsumer>(container, cfg =>
                    {
                        cfg.UseConcurrencyLimit(16);
                    });
                });
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