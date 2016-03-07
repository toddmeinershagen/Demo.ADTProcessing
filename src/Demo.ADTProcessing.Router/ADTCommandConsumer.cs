using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading.Tasks;

using Demo.ADTProcessing.Core;

using MassTransit;

namespace Demo.ADTProcessing.Router
{
    public class ADTCommandConsumer : IConsumer<IADTCommand>
    {
        private readonly NameValueCollection _appSettings = ConfigurationManager.AppSettings;

        public Task Consume(ConsumeContext<IADTCommand> context)
        {
            //TODO:  This is just a quick way to distribute messages.  The algorithm can be more significant with the real thing.
            var busHostUri = _appSettings["busHostUri"];
            var numberOfRouterQueues = _appSettings["numberOfRouterQueues"].As<int>();
            var routerQueueName = _appSettings["routerQueueName"];
            var routerQueueNumber = context.Message.FacilityId%numberOfRouterQueues + 1;
            var routerQueueEndpoint = new Uri($"{busHostUri}/{routerQueueName}-{routerQueueNumber}");

            return context
                .GetSendEndpoint(routerQueueEndpoint).Result
                .Send(context.Message);
        }
    }
}