using System;

using RabbitMQ.Client;

namespace Demo.ADTProcessing.Router.RabbitMQ
{
    public static class ConnectionFactoryExtensions
    {
        public static Uri GetUri(this ConnectionFactory factory)
        {
            return new UriBuilder("rabbitmq", factory.HostName, factory.Port, factory.VirtualHost)
            {
                UserName = factory.UserName,
                Password = factory.Password,
            }.Uri;
        }
    }
}