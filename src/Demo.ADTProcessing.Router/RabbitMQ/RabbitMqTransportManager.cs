using System;

using RabbitMQ.Client;

namespace Demo.ADTProcessing.Router.RabbitMQ
{
    public class RabbitMqTransportManager : ITransportManager, IDisposable
    {
        private readonly RabbitMqEndpointManagement _management;

        public RabbitMqTransportManager(Uri address)
        {
            var factory = new ConnectionFactory
            {
                UserName = address.UserInfo.Split(':')[0],
                Password = address.UserInfo.Split(':')[1],
                HostName = address.Host,
                Port = address.Port,
                VirtualHost = address.Segments[0]
            };

            _management = new RabbitMqEndpointManagement(factory.CreateConnection());
        }

        public void EnsureQueue(Uri address)
        {
            var name = RabbitMqEndpointAddress.Parse(address).Name;
            _management.BindQueue(name, name, ExchangeType.Fanout, "", null);
        }

        public void Dispose()
        {
            _management.Dispose();
        }
    }
}