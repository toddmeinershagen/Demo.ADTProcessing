using System;

namespace Demo.ADTProcessing.Core
{
    public class Event
    {
        public int DelayTimeInMilliseconds { get; set; }
        public int ExecutionTimeInMilliseconds { get; set; }
        public bool Successful { get; set; }
    }
}
