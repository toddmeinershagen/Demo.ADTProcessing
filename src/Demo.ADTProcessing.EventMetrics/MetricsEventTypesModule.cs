using Nancy;
using Nancy.ModelBinding;

namespace Demo.ADTProcessing.EventMetrics
{
    public class MetricsEventTypesModule : NancyModule
    {
        public MetricsEventTypesModule()
            : base("api/MetricsEventTypes")
        {
            Get["/{eventType}"] = _ =>
            {
                MetricsEventType eventType = MetricsEventTypes.GetEventType(_.eventType);
                return Negotiate
                    .WithModel(eventType)
                    .WithStatusCode(HttpStatusCode.OK);
            };

            Post[""] = _ =>
            {
                var eventType = this.Bind<MetricsEventType>();
                MetricsEventTypes.AddOrUpdateEventType(eventType);

                string url = $"{Request.Url.SiteBase}{Request.Path}/{eventType.Name}";
                return Negotiate
                    .WithHeader("Location", url)
                    .WithStatusCode(HttpStatusCode.Created);
            };
        }
    }
}