using System;
using System.Threading.Tasks;

using Demo.ADTProcessing.Core;

using MassTransit;

namespace Demo.ADTProcessing.Worker.Notifiers
{
    public class CommandBasedAccountSequenceCompleteNotifier : IAccountSequenceCompleteNotifier
    {
        public virtual Task NotifyRouter(ConsumeContext<IAccountSequenceCommand> context)
        {
            //return context.RespondAsync<IAccountSequenceCompletedEvent>(new { context.Message.QueueAddress });
            return context.Send<IAccountSequenceCompletedEvent>(new Uri(context.Message.RouterAddress), new { context.Message.QueueAddress });
        }
    }
}