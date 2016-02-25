using System;
using System.Threading.Tasks;

using Demo.ADTProcessing.Core;
using Demo.ADTProcessing.Router.RabbitMQ;

using MassTransit;

using RabbitMQ.Client;

namespace Demo.ADTProcessing.Router
{
    public class ADTCommandConsumer : IConsumer<IADTCommand>
    {
        private readonly IConnection _connection;

        public ADTCommandConsumer(IConnection connection)
        {
            _connection = connection;
        }

        public Task Consume(ConsumeContext<IADTCommand> context)
        {
            Console.WriteLine($"{context.Message.FacilityId}-{context.Message.AccountNumber}");

            var key = GetKey(context.Message);
            var endpointAddressUrl = $"rabbitmq://localhost/adt/Demo.ADTProcessing.Router.{key}";

            if (Program.Queues.ContainsKey(endpointAddressUrl) == false)
            {
                BindQueue(endpointAddressUrl);

                context
                    .Publish<IAccountSequenceCommand>(new { QueueAddress = endpointAddressUrl })
                    .Wait();

                while(Program.Queues.TryAdd(endpointAddressUrl, endpointAddressUrl) == false) {};
            }

            var endpointUri = new Uri(endpointAddressUrl);

            return context
                .GetSendEndpoint(endpointUri).Result
                .Send(context.Message);
        }

        public string GetKey(IADTCommand command)
        {
            return $"{command.FacilityId}-{command.AccountNumber}";
        }

        private void BindQueue(string addressUrl)
        {
            using (var model = _connection.CreateModel())
            {
                var name = RabbitMqEndpointAddress.Parse(addressUrl).Name;
                var queue = model.QueueDeclare(name, true, false, false, null);
                
                model.ExchangeDeclare(name, ExchangeType.Fanout, true);
                model.QueueBind(queue, name, "");

                model.Close(200, "ok");
            }
        }
    }
}