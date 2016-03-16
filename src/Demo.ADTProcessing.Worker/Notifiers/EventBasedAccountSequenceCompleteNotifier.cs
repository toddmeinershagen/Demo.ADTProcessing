using System.Threading.Tasks;

using Demo.ADTProcessing.Core;

using MassTransit;

namespace Demo.ADTProcessing.Worker.Notifiers
{
    public class EventBasedAccountSequenceCompleteNotifier : IAccountSequenceCompleteNotifier
    {
        public virtual Task NotifyRouter(ConsumeContext<IAccountSequenceCommand> context)
        {
            return context.Send<IAccountSequenceCompletedEvent>(context.SourceAddress, new { context.Message.QueueAddress });
        }
    }
}