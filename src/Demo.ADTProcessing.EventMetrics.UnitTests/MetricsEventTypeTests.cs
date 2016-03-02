using System;

using Demo.ADTProcessing.Core;

using FluentAssertions;

using NUnit.Framework;

namespace Demo.ADTProcessing.EventMetrics.UnitTests
{
    [TestFixture]
    public class MetricsEventTypeTests
    {
        [TestCase("Test1", 225, 4146, true)]
        [TestCase("Test2", 0, 0, false)]
        public void given_metrics_event_for_new_metrics_event_type_when_constructing_should(string eventType, int delayInMs, int executionInMs, bool successful)
        {
            var metricsEvent = new MetricsEvent
            {
                EventType = eventType,
                DelayInMilliseconds = delayInMs,
                ExecutionInMilliseconds = executionInMs,
                Successful = successful
            };

            var metricsEventType = new MetricsEventType(metricsEvent);

            metricsEventType.Name.Should().Be(metricsEvent.EventType);
            metricsEventType.Count.Should().Be(1);
            metricsEventType.Failures.Should().Be(metricsEvent.Successful ? 0 : 1);
            metricsEventType.Successes.Should().Be(metricsEvent.Successful ? 1 : 0);
            metricsEventType.TotalDelayInMilliseconds.Should().Be(metricsEvent.DelayInMilliseconds);
            metricsEventType.MinDelayInMilliseconds.Should().Be(metricsEvent.DelayInMilliseconds);
            metricsEventType.AvgDelayInMilliseconds.Should().Be(metricsEvent.DelayInMilliseconds);
            metricsEventType.MaxDelayInMilliseconds.Should().Be(metricsEvent.DelayInMilliseconds);
            metricsEventType.TotalExecutionInMilliseconds.Should().Be(metricsEvent.ExecutionInMilliseconds);
            metricsEventType.MinExecutionInMilliseconds.Should().Be(metricsEvent.ExecutionInMilliseconds);
            metricsEventType.AvgExecutionInMilliseconds.Should().Be(metricsEvent.ExecutionInMilliseconds);
            metricsEventType.MaxExecutionInMilliseconds.Should().Be(metricsEvent.ExecutionInMilliseconds);
        }

        [TestCase("Test1", 225, 414, true)]
        [TestCase("Test2", 0, 0, false)]
        public void given_metrics_event_for_existing_metrics_event_type_when_constructing_should(string eventType, int delayInMs, int executionInMs, bool successful)
        {
            var originalDelayInMs = 2;
            var originalExecutionInMs = 2;
            var metricsEventType = new MetricsEventType(new MetricsEvent
            {
                EventType = eventType,
                DelayInMilliseconds = originalDelayInMs,
                ExecutionInMilliseconds = originalExecutionInMs,
                Successful = true
            });

            var metricsEvent = new MetricsEvent
            {
                EventType = eventType,
                DelayInMilliseconds = delayInMs,
                ExecutionInMilliseconds = executionInMs,
                Successful = successful
            };

            metricsEventType.AddEvent(metricsEvent);

            metricsEventType.Name.Should().Be(metricsEvent.EventType);
            metricsEventType.Count.Should().Be(2);
            metricsEventType.Failures.Should().Be(metricsEvent.Successful ? 0 : 1);
            metricsEventType.Successes.Should().Be(metricsEvent.Successful ? 2 : 1);
            metricsEventType.TotalDelayInMilliseconds.Should().Be(originalDelayInMs + metricsEvent.DelayInMilliseconds);
            metricsEventType.MinDelayInMilliseconds.Should().Be(Math.Min(originalDelayInMs, metricsEvent.DelayInMilliseconds));
            metricsEventType.AvgDelayInMilliseconds.Should()
                .Be(metricsEventType.TotalDelayInMilliseconds/2);
            metricsEventType.MaxDelayInMilliseconds.Should().Be(Math.Max(originalDelayInMs, metricsEvent.DelayInMilliseconds));
            metricsEventType.TotalExecutionInMilliseconds.Should().Be(originalExecutionInMs + metricsEvent.ExecutionInMilliseconds);
            metricsEventType.MinExecutionInMilliseconds.Should().Be(Math.Min(originalExecutionInMs, metricsEvent.ExecutionInMilliseconds));
            metricsEventType.AvgExecutionInMilliseconds.Should().Be(metricsEventType.TotalExecutionInMilliseconds / 2);
            metricsEventType.MaxExecutionInMilliseconds.Should().Be(Math.Max(originalExecutionInMs, metricsEvent.ExecutionInMilliseconds));
        }
    }
}
