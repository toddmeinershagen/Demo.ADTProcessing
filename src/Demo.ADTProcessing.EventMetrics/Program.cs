namespace Demo.ADTProcessing.EventMetrics
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new EventsProcessor();
            processor.Run();
        }
    }
}
