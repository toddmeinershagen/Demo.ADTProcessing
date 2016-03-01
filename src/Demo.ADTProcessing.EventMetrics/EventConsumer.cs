using System;
using System.Threading.Tasks;

using Demo.ADTProcessing.Core;

using MassTransit;

namespace Demo.ADTProcessing.EventMetrics
{
    public class EventConsumer : IConsumer<MetricsEvent>
    {
        private static readonly object SyncRoot = new object();

        public Task Consume(ConsumeContext<MetricsEvent> context)
        {
            lock (SyncRoot)
            {
                var eventType = MetricsEventTypes.GetEventType(context.Message.EventType);

                if (eventType == null)
                {
                    eventType = new MetricsEventType(context.Message);
                }
                else
                {
                    eventType.AddEvent(context.Message);
                }

                MetricsEventTypes.AddOrUpdateEventType(eventType);
            }

            return Console.Out.WriteLineAsync($"{context.Message.EventType}");
        }
    }
}