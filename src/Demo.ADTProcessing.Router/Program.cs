using System.Collections.Concurrent;

namespace Demo.ADTProcessing.Router
{
    class Program
    {
        public static readonly ConcurrentDictionary<string, string> Queues = new ConcurrentDictionary<string, string>();

        static void Main(string[] args)
        {
            var router = new Router();
            router.Run();
        }
    }
}
