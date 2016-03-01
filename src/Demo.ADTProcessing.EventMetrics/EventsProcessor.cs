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
            Console.WriteLine("Demo.ADTProcessing.EventsProcessor::Hit ENTER to start.");
            Console.ReadLine();

            var bus = CreateBus();
            bus.Start();

            var baseUri = new Uri("http://localhost:9090");
            using (var host = new NancyHost(baseUri))
            {
                host.Start();
                Console.WriteLine("EventsProcessor started.  Hit ENTER to end...");
                Console.ReadLine();

                bus.Stop();
            }
        }

        private static IBusControl CreateBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri("rabbitmq://localhost/adt"), h =>
                {
                    h.Username("guest");
                    h.Password("");
                    h.Heartbeat(10);
                });

                sbc.ReceiveEndpoint(host, "Demo.ADTProcessing.EventsProcessor", ep =>
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