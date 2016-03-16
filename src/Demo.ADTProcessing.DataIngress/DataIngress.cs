using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;

using Demo.ADTProcessing.Core;

using MassTransit;
using MassTransit.Transports;

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
            var accountSequences = GetAccountSequences(testFacilities, testAccounts);

            while (true)
            {
                stopwatch.Reset();
                stopwatch.Start();

                var expectedRatePerMinute = AppSettings["expectedRatePerMinute"].As<int>();
                int remainder;
                var expectedRatePerSecond = Math.DivRem(expectedRatePerMinute, 60, out remainder);

                foreach (var count in Enumerable.Range(0, expectedRatePerSecond))
                {
                    var facility = GetRandomNumber(testFacilities);
                    var account = GetRandomNumber(testAccounts);
                    var command = new {FacilityId = facility, AccountNumber = account, Sequence = ++accountSequences[GetKey(facility, account)], Timestamp = DateTime.Now};

                    routerEndpoint
                        .Send<IADTCommand>(command);

                    Console.Out.WriteLineAsync($"{command.FacilityId}-{command.AccountNumber}");
                }
                stopwatch.Stop();

                var timeLeft = TimeSpan.FromSeconds(1) - stopwatch.Elapsed;
                Thread.Sleep(timeLeft < TimeSpan.Zero ? TimeSpan.Zero : timeLeft);
            }
        }

        private IDictionary<string, int> GetAccountSequences(int facilities, int accounts)
        {
            var accountSequences = new Dictionary<string, int>(facilities*accounts);
            for (var facilityIndex = 1; facilityIndex <= facilities; facilityIndex++)
            {
                for (var accountIndex = 1; accountIndex <= accounts; accountIndex++)
                {
                    accountSequences.Add(GetKey(facilityIndex, accountIndex), 0);
                }
            }
            return accountSequences;
        }

        private string GetKey(int facility, int account)
        {
            return $"{facility}-{account}";
        }

        private static IBusControl CreateBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var busHostUri = new Uri(AppSettings["busHostUri"]);
                var username = busHostUri.UserInfo.Split(':')[0];
                var password = busHostUri.UserInfo.Split(':')[1];

                sbc.Host(busHostUri, h =>
                {
                    h.Username(username);
                    h.Password(password);
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