using System;

namespace Demo.ADTProcessing.Core
{
    public class MetricsEvent : IMetricsEvent
    {
        public string EventType { get; set; }
        public int DelayInMilliseconds { get; set; }
        public int ExecutionInMilliseconds { get; set; }
        public bool Successful { get; set; }
    }
}
