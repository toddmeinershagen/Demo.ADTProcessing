using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Demo.ADTProcessing.Core;

using MassTransit;

namespace Demo.ADTProcessing.DataIngress
{
    public class DataIngress
    {
        public void Run()
        {
            Console.WriteLine("Demo.ADTProcessing.DataIngress::Hit ENTER to start.");
            Console.ReadLine();

            var bus = CreateBus();

            bus.Start();

            Console.WriteLine("Data Ingress started.  Hit ENTER to end...");

            PublishMessages(bus);

            bus.Stop();
        }

        private void PublishMessages(IBus bus)
        {
            var endpointUri = new Uri("rabbitmq://localhost/adt/Demo.ADTProcessing.Router");
            var routerEndpoint = bus.GetSendEndpoint(endpointUri).Result;

            var stopwatch = new Stopwatch();

            var currentClients = 50;
            var currentMessageRate = 360;
            var expectedClients = 300;

            while (true)
            {
                stopwatch.Reset();
                stopwatch.Start();

                var expectedRate = (currentMessageRate/currentClients)*expectedClients;
                int remainder;
                var numberOfMessages = Math.DivRem(expectedRate, 60, out remainder);

                foreach (var count in Enumerable.Range(0, numberOfMessages))
                {
                    var command = new {FacilityId = GetRandomNumber(3), AccountNumber = GetRandomNumber(10)};

                    routerEndpoint
                        .Send<IADTCommand>(command);

                    Console.WriteLine($"{command.FacilityId}-{command.AccountNumber}");
                }
                stopwatch.Stop();

                var timeLeft = TimeSpan.FromSeconds(1) - stopwatch.Elapsed;
                Thread.Sleep(timeLeft < TimeSpan.Zero ? TimeSpan.Zero : timeLeft);
            }
        }

        private static IBusControl CreateBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                sbc.Host(new Uri("rabbitmq://localhost/adt"), h =>
                {
                    h.Username("guest");
                    h.Password("");
                    h.Heartbeat(10);
                });
            });
        }

        private int GetRandomNumber(int maxNumber)
        {
            return Math.Abs(Guid.NewGuid().GetHashCode()%maxNumber) + 1;
        }
    }
}