using Demo.ADTProcessing.Core;

using MassTransit;

namespace Demo.ADTProcessing.Router
{
    public interface IAccountSequenceNotifier
    {
        void NotifyWorkers<T>(ConsumeContext<T> context, string address) where T : class;
    }

    public class AccountSequenceNotifier : IAccountSequenceNotifier
    {
        public virtual void NotifyWorkers<T>(ConsumeContext<T> context, string address) where T : class
        {
            context
                .Publish<IAccountSequenceCommand>(new { QueueAddress = address })
                //NOTE:  Do not want to send/respond between routers and workers for two reasons:
                //       * Responses go back to a temporary bus queue so that the main bus can forward it to the individual router.
                //       * Responses go back to a temporary bus queue that might go down in the middle of processing and those messages would be lost.
                //.Send<IAccountSequenceCommand>(workerQueueUri, new { QueueAddress = address, RouterAddress = context.DestinationAddress.OriginalString })
                //_workerEndpoint.Send<IAccountSequenceCommand>(new { QueueAddress = address, RouterAddress = context.DestinationAddress.OriginalString })
                .Wait();
        }
    }
}