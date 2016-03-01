using System.Collections.Concurrent;

namespace Demo.ADTProcessing.EventMetrics
{
    public class MetricsEventTypes
    {
        private static readonly ConcurrentDictionary<string, MetricsEventType> Items = new ConcurrentDictionary<string, MetricsEventType>();
         
        public static MetricsEventType GetEventType(string name)
        {
            MetricsEventType eventType;
            Items.TryGetValue(name, out eventType);
            return eventType;
        }

        public static void AddOrUpdateEventType(MetricsEventType eventType)
        {
            Items.AddOrUpdate(eventType.Name, eventType, (key, metric) => eventType);
        }
    }
}