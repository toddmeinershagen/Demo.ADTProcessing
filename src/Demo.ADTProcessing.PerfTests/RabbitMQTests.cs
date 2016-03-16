using System;
using System.Configuration;

using NUnit.Framework;

using RabbitMQ.Client;

namespace Demo.ADTProcessing.PerfTests
{
    [TestFixture]
    public class RabbitMqTests
    {
        private IConnection _connection;

        [OneTimeSetUp]
        public void SetUp()
        {
            var brokerHostUri = ConfigurationManager.AppSettings["brokerHostUri"];
            var factory = new ConnectionFactory
            {
                Uri = brokerHostUri,
                RequestedHeartbeat = 10,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
        }

        //Run a test with 150,000 accounts active
        //Run a test with 10x accounts active (1.5 mil)
        [Test]
        public void given_number_of_existing_accounts_when_sending_message_memory_should_stay_under_target_percent(int numberOfAccounts, int targetPercent)
        {
           
        }
    }
}
