using System;
using System.Collections.Specialized;
using System.Configuration;

using MassTransit;

using Nancy.Hosting.Self;

namespace Demo.ADTProcessing.EventMetrics
{
    public class EventsProcessor
    {
        private static readonly NameValueCollection AppSettings = ConfigurationManager.AppSettings;

        public void Run()
        {
            var eventProcessorQueueName = AppSettings["eventProcessorQueueName"];
            Console.WriteLine($"{eventProcessorQueueName}::Hit ENTER to start.");
            Console.ReadLine();

            var bus = CreateBus();
            bus.Start();

            var restHostUri = AppSettings["restHostUri"];
            var baseUri = new Uri(restHostUri);
            using (var host = new NancyHost(baseUri))
            {
                host.Start();
                Console.WriteLine($"{eventProcessorQueueName} started.  Hit ENTER to end...");
                Console.ReadLine();

                bus.Stop();
            }
        }

        private static IBusControl CreateBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var busHostUri = new Uri(AppSettings["busHostUri"]);
                var username = busHostUri.UserInfo.Split(':')[0];
                var password = busHostUri.UserInfo.Split(':')[1];
                var eventProcessorQueueName = AppSettings["eventProcessorQueueName"];

                var host = sbc.Host(busHostUri, h =>
                {
                    h.Username(username);
                    h.Password(password);
                    h.Heartbeat(10);
                });

                sbc.ReceiveEndpoint(host, eventProcessorQueueName, ep =>
                {
                    //ep.Exclusive = true;
                    ep.Consumer<EventConsumer>(cfg =>
                    {
                        var numberOfEventsProcessors = AppSettings["numberOfEventsProcessors"].As<int>();
                        cfg.UseConcurrencyLimit(numberOfEventsProcessors);
                    });
                });
            });
        }
    }
}