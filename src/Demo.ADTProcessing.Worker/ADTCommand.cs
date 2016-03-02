using System;

using Demo.ADTProcessing.Core;

namespace Demo.ADTProcessing.Worker
{
    public class ADTCommand : IADTCommand
    {
        public int FacilityId { get; set; }
        public int AccountNumber { get; set; }
        public int Sequence { get; set; }
        public DateTime Timestamp { get; set; }
    }
}