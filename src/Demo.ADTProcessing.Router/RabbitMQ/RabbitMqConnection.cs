using System;

using MassTransit.RabbitMqTransport;

using RabbitMQ.Client;

namespace Demo.ADTProcessing.Router.RabbitMQ
{
    public class RabbitMqConnection : IDisposable
    {
        readonly ConnectionFactory _connectionFactory;
        IConnection _connection;
        bool _disposed;

        public RabbitMqConnection(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IConnection Connection => _connection;

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException($"RabbitMqConnection for {_connectionFactory.GetUri()}", "Cannot dispose a connection twice");

            try
            {
                Disconnect();
            }
            finally
            {
                _disposed = true;
            }
        }

        public void Connect()
        {
            Disconnect();

            _connection = _connectionFactory.CreateConnection();
        }

        public void Disconnect()
        {
            _connection.Cleanup(200, "Disconnect");
            _connection = null;
        }
    }
}