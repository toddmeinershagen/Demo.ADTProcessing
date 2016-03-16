using System.Threading.Tasks;

using Demo.ADTProcessing.Core;

using MassTransit;

namespace Demo.ADTProcessing.Router.Notifiers
{
    public class CommandBasedAccountSequenceNotifier : IAccountSequenceNotifier
    {
        private readonly ISendEndpoint _workerEndpoint;

        public CommandBasedAccountSequenceNotifier(ISendEndpoint workerEndpoint)
        {
            _workerEndpoint = workerEndpoint;
        }

        public virtual Task NotifyWorkers<T>(ConsumeContext<T> context, string address) where T : class
        {
            //var workerQueueName = _appSettings["workerQueueName"];
            //var workerQueueUri = new Uri($"{busHostUri}/{workerQueueName}");
            //return context.Send<IAccountSequenceCommand>(workerQueueUri, new { QueueAddress = address, RouterAddress = context.DestinationAddress.OriginalString })

            return
                _workerEndpoint.Send<IAccountSequenceCommand>(
                    new {QueueAddress = address, RouterAddress = context.DestinationAddress.OriginalString});
        }
    }
}