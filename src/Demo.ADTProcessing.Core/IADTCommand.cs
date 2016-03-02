using System;

namespace Demo.ADTProcessing.Core
{
    public interface IADTCommand
    {
        int FacilityId { get; set; }
        int AccountNumber { get; set; }
        int Sequence { get; set; }
        DateTime Timestamp { get; set; }
    }
}
