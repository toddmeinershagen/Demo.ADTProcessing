using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Demo.ADTProcessing.Core;

using MassTransit;

namespace Demo.ADTProcessing.DataIngress
{
    public class DataIngress
    {
        private static readonly NameValueCollection AppSettings = ConfigurationManager.AppSettings;

        public void Run()
        {
            var dataIngressQueueName = AppSettings["dataIngressQueueName"];
            Console.WriteLine($"{dataIngressQueueName}::Hit ENTER to start.");
            Console.ReadLine();

            var bus = CreateBus();

            bus.Start();

            Console.WriteLine($"{dataIngressQueueName} started.  Hit ENTER to end...");

            PublishMessages(bus);

            bus.Stop();
        }

        private void PublishMessages(IBus bus)
        {
            var busHostUri = AppSettings["busHostUri"];
            var routerQueueName = AppSettings["routerQueueName"];
            var endpointUri = new Uri($"{busHostUri}/{routerQueueName}");
            var routerEndpoint = bus.GetSendEndpoint(endpointUri).Result;

            var stopwatch = new Stopwatch();

            var testFacilities = AppSettings["testFacilities"].As<int>();
            var testAccounts = AppSettings["testAccounts"].As<int>();

            while (true)
            {
                stopwatch.Reset();
                stopwatch.Start();

                var expectedRatePerMinute = AppSettings["expectedRatePerMinute"].As<int>();
                int remainder;
                var expectedRatePerSecond = Math.DivRem(expectedRatePerMinute, 60, out remainder);

                foreach (var count in Enumerable.Range(0, expectedRatePerSecond))
                {
                    var command = new {FacilityId = GetRandomNumber(testFacilities), AccountNumber = GetRandomNumber(testAccounts), Timestamp = DateTime.Now};

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
                var busHostUri = AppSettings["busHostUri"];
                sbc.Host(new Uri(busHostUri), h =>
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