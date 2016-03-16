namespace Demo.ADTProcessing.Core
{
    public interface IAccountSequenceCommand
    {
        string QueueAddress { get; set; }
        string RouterAddress { get; set; }
    }
}