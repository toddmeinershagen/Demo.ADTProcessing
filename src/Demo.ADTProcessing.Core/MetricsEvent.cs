using System;

namespace Demo.ADTProcessing.Core
{
    public interface IMetricsEvent
    {
        string EventType { get; set; }
        int DelayInMilliseconds { get; set; }
        int ExecutionInMilliseconds { get; set; }
        bool Successful { get; set; }
    }
}
