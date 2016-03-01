using Nancy;
using Nancy.ModelBinding;

namespace Demo.ADTProcessing.EventMetrics
{
    /// <summary>
    /// http://localhost:9090/api/MetricsEventTypes/Demo.ADTProcessing.Worker
    /// Accept: application/json
    /// </summary>
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

            Post["/Clear"] = _ =>
            {
                MetricsEventTypes.Clear();
                return Negotiate
                    .WithStatusCode(HttpStatusCode.Created);
            };
        }
    }
}