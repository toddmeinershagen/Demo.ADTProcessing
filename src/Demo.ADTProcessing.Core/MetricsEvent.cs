using System;

namespace Demo.ADTProcessing.Core
{
    public class MetricsEvent : IMetricsEvent
    {
        public MetricsEvent()
        {
            Timestamp = DateTime.Now;
        }
        public string EventType { get; set; }
        public int DelayInMilliseconds { get; set; }
        public int ExecutionInMilliseconds { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Successful { get; set; }
    }
}
