using System;

namespace Demo.ADTProcessing.Router.RabbitMQ
{
    public interface ITransportManager
    {
        void EnsureQueue(Uri address);
    }
}