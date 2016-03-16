using System.Threading.Tasks;

using MassTransit;

namespace Demo.ADTProcessing.Router
{
    public interface IAccountSequenceNotifier
    {
        Task NotifyWorkers<T>(ConsumeContext<T> context, string address) where T : class;
    }
}