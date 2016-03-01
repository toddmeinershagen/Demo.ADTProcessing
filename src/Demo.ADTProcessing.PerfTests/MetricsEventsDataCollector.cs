using System.Xml;

using Demo.ADTProcessing.Core;
using Demo.ADTProcessing.EventMetrics;

using Microsoft.VisualStudio.TestTools.Execution;

namespace Demo.ADTProcessing.PerfTests
{
    [DataCollectorTypeUri("datacollector://MetricsEventsDataCollector/1.0")]
    [DataCollectorFriendlyName("MetricsEventsDataCollector", isResourceName: false)]
    public class MetricsEventsDataCollector : DataCollector
    {
        private DataCollectionSink _sink;
        private DataCollectionEvents _events;
        private DataCollectionLogger _logger;

        public override void Initialize(XmlElement configurationElement, DataCollectionEvents events, DataCollectionSink dataSink,
            DataCollectionLogger logger, DataCollectionEnvironmentContext environmentContext)
        {
            _events = events;
            _events.DataRequest += Events_DataRequest;
            _events.SessionEnd += Events_SessionEnd;
        }

        private void Events_SessionEnd(object sender, SessionEndEventArgs e)
        {
            _logger.SendData(e.Context, new MetricsEventData(new MetricsEventType() {Count = 1, TotalDelayInMilliseconds = 600, TotalExecutionInMilliseconds = 500}));
            //throw new System.NotImplementedException();
        }

        private void Events_DataRequest(object sender, DataRequestEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing) return;

            _events.DataRequest -= Events_DataRequest;
            _events.SessionEnd -= Events_SessionEnd;
        }
    }

    public class MetricsEventData : CustomCollectorData
    {
        private readonly MetricsEventType _eventType;

        public MetricsEventData(MetricsEventType eventType)
        {
            _eventType = eventType;
        }

        public int AvgDelayInMilliseconds => _eventType.AvgDelayInMilliseconds;

        public int AvgExecutionInMilliseconds => _eventType.AvgExecutionInMilliseconds;
    }
}
