﻿using System;

using Demo.ADTProcessing.Core;

namespace Demo.ADTProcessing.EventMetrics
{
    public class MetricsEventType
    {
        public MetricsEventType(MetricsEvent metricsEvent)
        {
            Name = metricsEvent.EventType;

            MinDelayInMilliseconds = metricsEvent.DelayInMilliseconds;
            MaxDelayInMilliseconds = metricsEvent.DelayInMilliseconds;
            TotalDelayInMilliseconds = metricsEvent.DelayInMilliseconds;

            MinExecutionInMilliseconds = metricsEvent.ExecutionInMilliseconds;
            MaxExecutionInMilliseconds = metricsEvent.ExecutionInMilliseconds;
            TotalExecutionInMilliseconds = metricsEvent.ExecutionInMilliseconds;

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

        public void AddEvent(MetricsEvent metricsEvent)
        {
            if (metricsEvent.DelayInMilliseconds < MinDelayInMilliseconds)
                MinDelayInMilliseconds = metricsEvent.DelayInMilliseconds;

            if (metricsEvent.DelayInMilliseconds > MaxDelayInMilliseconds)
                MaxDelayInMilliseconds = metricsEvent.DelayInMilliseconds;

            TotalDelayInMilliseconds = TotalDelayInMilliseconds + metricsEvent.DelayInMilliseconds;

            if (metricsEvent.ExecutionInMilliseconds < MinExecutionInMilliseconds)
                MinExecutionInMilliseconds = metricsEvent.ExecutionInMilliseconds;

            if (metricsEvent.ExecutionInMilliseconds > MaxExecutionInMilliseconds)
                MaxExecutionInMilliseconds = metricsEvent.ExecutionInMilliseconds;

            TotalExecutionInMilliseconds = TotalExecutionInMilliseconds + metricsEvent.ExecutionInMilliseconds;

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
        public int AvgDelayInMilliseconds => TotalDelayInMilliseconds / Count;
        public int MaxDelayInMilliseconds { get; set; }
        public int TotalDelayInMilliseconds { get; set; }

        public int MinExecutionInMilliseconds { get; set; }

        public int AvgExecutionInMilliseconds => TotalExecutionInMilliseconds/Count;

        public int MaxExecutionInMilliseconds { get; set; }
        public int TotalExecutionInMilliseconds { get; set; }
        public int Successes { get; set; }
        public int Failures { get; set; }
        public int Count { get; set; }
    }
}