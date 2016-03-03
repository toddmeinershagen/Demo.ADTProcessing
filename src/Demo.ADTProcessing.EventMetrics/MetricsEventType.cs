using System;

using Demo.ADTProcessing.Core;

using Newtonsoft.Json;

using NLog;

namespace Demo.ADTProcessing.EventMetrics
{
    public class MetricsEventType
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MetricsEventType(IMetricsEvent metricsEvent)
        {
            Name = metricsEvent.EventType;

            MinDelayInMilliseconds = metricsEvent.DelayInMilliseconds;
            MaxDelayInMilliseconds = metricsEvent.DelayInMilliseconds;
            TotalDelayInMilliseconds = metricsEvent.DelayInMilliseconds;

            MinExecutionInMilliseconds = metricsEvent.ExecutionInMilliseconds;
            MaxExecutionInMilliseconds = metricsEvent.ExecutionInMilliseconds;
            TotalExecutionInMilliseconds = metricsEvent.ExecutionInMilliseconds;

            FirstMessageTimestamp = metricsEvent.Timestamp;
            LastMessageTimestamp = metricsEvent.Timestamp;

            if (metricsEvent.Successful)
            {
                Successes++;
            }
            else
            {
                Failures++;
            }

            Count = 1;
        }

        public void AddEvent(IMetricsEvent metricsEvent)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(JsonConvert.SerializeObject(metricsEvent));
            }

            MinDelayInMilliseconds = Math.Min(MinDelayInMilliseconds, metricsEvent.DelayInMilliseconds);
            MaxDelayInMilliseconds = Math.Max(MaxDelayInMilliseconds, metricsEvent.DelayInMilliseconds);

            TotalDelayInMilliseconds = TotalDelayInMilliseconds + metricsEvent.DelayInMilliseconds;

            MinExecutionInMilliseconds = Math.Min(MinExecutionInMilliseconds, metricsEvent.ExecutionInMilliseconds);
            MaxExecutionInMilliseconds = Math.Max(MaxExecutionInMilliseconds, metricsEvent.ExecutionInMilliseconds);

            TotalExecutionInMilliseconds = TotalExecutionInMilliseconds + metricsEvent.ExecutionInMilliseconds;

            LastMessageTimestamp = metricsEvent.Timestamp;

            if (metricsEvent.Successful)
            {
                Successes = Successes + 1;
            }
            else
            {
                Failures = Failures + 1;
            }

            Count = Count + 1;
        }

        public string Name { get; set; }
        public int MinDelayInMilliseconds { get; set; }
        public int AvgDelayInMilliseconds => TotalDelayInMilliseconds/Count;
        public int MaxDelayInMilliseconds { get; set; }
        public int TotalDelayInMilliseconds { get; set; }

        public int MinExecutionInMilliseconds { get; set; }

        public int AvgExecutionInMilliseconds => TotalExecutionInMilliseconds/Count;

        public int MaxExecutionInMilliseconds { get; set; }
        public int TotalExecutionInMilliseconds { get; set; }
        public int Successes { get; set; }
        public int Failures { get; set; }
        public int Count { get; set; }
        public DateTime FirstMessageTimestamp { get; set; }
        public DateTime LastMessageTimestamp { get; set; }

        public int MessagesPerSecond
        {
            get
            {
                var interval = LastMessageTimestamp - FirstMessageTimestamp;
                if (interval == TimeSpan.Zero)
                {
                    return Count;
                }
                else
                {
                    return (Count / interval.TotalSeconds).Rounded();
                }
            }
        }
    }
}