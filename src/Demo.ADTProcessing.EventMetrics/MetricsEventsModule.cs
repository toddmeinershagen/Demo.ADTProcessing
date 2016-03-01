using Demo.ADTProcessing.Core;

using Nancy;
using Nancy.ModelBinding;

namespace Demo.ADTProcessing.EventMetrics
{
    public class MetricsEventsModule : NancyModule
    {
        private static readonly object SyncRoot = new object();

        public MetricsEventsModule()
            : base("api/MetricsEvents")
        {
            Post[""] = _ =>
            {

                var metricsEvent = this.Bind<MetricsEvent>();
                lock (SyncRoot)
                {
                    var eventType = MetricsEventTypes.GetEventType(metricsEvent.EventType);

                    if (eventType == null)
                    {
                        eventType = new MetricsEventType(metricsEvent);
                    }
                    else
                    {
                        eventType.AddEvent(metricsEvent);
                    }

                    MetricsEventTypes.AddOrUpdateEventType(eventType);
                }

                return Negotiate
                    .WithStatusCode(HttpStatusCode.Created);
            };
        }
    }
}