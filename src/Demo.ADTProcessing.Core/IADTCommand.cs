using System.Security.Cryptography.X509Certificates;

namespace Demo.ADTProcessing.Core
{
    public interface IADTCommand
    {
        int FacilityId { get; set; }
        int AccountNumber { get; set; }
    }
}
