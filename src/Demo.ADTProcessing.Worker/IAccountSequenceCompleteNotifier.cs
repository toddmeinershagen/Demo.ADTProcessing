using System.Threading.Tasks;

using Demo.ADTProcessing.Core;

using MassTransit;

namespace Demo.ADTProcessing.Worker
{
    public interface IAccountSequenceCompleteNotifier
    {
        Task NotifyRouter(ConsumeContext<IAccountSequenceCommand> context);
    }
}