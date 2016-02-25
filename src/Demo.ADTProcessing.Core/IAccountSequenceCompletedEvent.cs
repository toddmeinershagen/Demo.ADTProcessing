namespace Demo.ADTProcessing.Core
{
    public interface IAccountSequenceCompletedEvent
    {
        string QueueAddress { get; set; }
    }
}
