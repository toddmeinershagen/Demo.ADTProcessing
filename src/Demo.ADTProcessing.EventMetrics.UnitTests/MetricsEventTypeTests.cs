using System;

using Demo.ADTProcessing.Core;

using FluentAssertions;

using NUnit.Framework;

namespace Demo.ADTProcessing.EventMetrics.UnitTests
{
    [TestFixture]
    public class given_metrics_event_for_new_metrics_event_type_when_constructing
    {
        [TestCase("Test1", 225, 4146, true)]
        [TestCase("Test2", 0, 0, false)]
        public void should(string eventType, int delayInMs, int executionInMs, bool successful)
        {
            var metricsEvent = new MetricsEvent
            {
                EventType = eventType,
                DelayInMilliseconds = delayInMs,
                ExecutionInMilliseconds = executionInMs,
                Successful = successful,
                Timestamp = new DateTime(2014, 1, 1, 0, 0, 0, 0)
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
            metricsEventType.MessagesPerSecond.Should().Be(1);
        }
    }

    [TestFixture]
    public class given_metrics_event_for_existing_metrics_event_type_when_constructing
    { 
        [TestCase("Test1", 225, 414, true, 1, 2)]
        [TestCase("Test2", 0, 0, false, 2, 1)]
        [TestCase("Test3", 100, 455, true, 3, 1)]
        public void should(string eventType, int delayInMs, int executionInMs, bool successful, int numberOfSeconds, int expectedMessageRate)
        {
            var originalDelayInMs = 2;
            var originalExecutionInMs = 2;
            var originalTimestamp = new DateTime(2014, 1, 1, 0, 0, 0, 0);

            var metricsEventType = new MetricsEventType(new MetricsEvent
            {
                EventType = eventType,
                DelayInMilliseconds = originalDelayInMs,
                ExecutionInMilliseconds = originalExecutionInMs,
                Successful = true,
                Timestamp = originalTimestamp
            });

            var metricsEvent = new MetricsEvent
            {
                EventType = eventType,
                DelayInMilliseconds = delayInMs,
                ExecutionInMilliseconds = executionInMs,
                Successful = successful,
                Timestamp = metricsEventType.LastMessageTimestamp.AddSeconds(numberOfSeconds)
            };

            metricsEventType.AddEvent(metricsEvent);

            metricsEventType.Name.Should().Be(metricsEvent.EventType);
            metricsEventType.Count.Should().Be(2);
            metricsEventType.Failures.Should().Be(metricsEvent.Successful ? 0 : 1);
            metricsEventType.Successes.Should().Be(metricsEvent.Successful ? 2 : 1);
            metricsEventType.TotalDelayInMilliseconds.Should().Be(originalDelayInMs + metricsEvent.DelayInMilliseconds);
            metricsEventType.MinDelayInMilliseconds.Should().Be(Math.Min(originalDelayInMs, metricsEvent.DelayInMilliseconds));
            metricsEventType.AvgDelayInMilliseconds.Should().Be(metricsEventType.TotalDelayInMilliseconds/2);
            metricsEventType.MaxDelayInMilliseconds.Should().Be(Math.Max(originalDelayInMs, metricsEvent.DelayInMilliseconds));
            metricsEventType.TotalExecutionInMilliseconds.Should().Be(originalExecutionInMs + metricsEvent.ExecutionInMilliseconds);
            metricsEventType.MinExecutionInMilliseconds.Should().Be(Math.Min(originalExecutionInMs, metricsEvent.ExecutionInMilliseconds));
            metricsEventType.AvgExecutionInMilliseconds.Should().Be(metricsEventType.TotalExecutionInMilliseconds / 2);
            metricsEventType.MaxExecutionInMilliseconds.Should().Be(Math.Max(originalExecutionInMs, metricsEvent.ExecutionInMilliseconds));
            metricsEventType.FirstMessageTimestamp.Should().Be(originalTimestamp);
            metricsEventType.LastMessageTimestamp.Should().Be(metricsEvent.Timestamp);
            metricsEventType.MessagesPerSecond.Should().Be(expectedMessageRate);
        }
    }
}
