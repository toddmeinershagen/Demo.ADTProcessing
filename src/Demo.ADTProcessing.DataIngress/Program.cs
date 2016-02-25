using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Demo.ADTProcessing.DataIngress
{
    class Program
    {
        static void Main(string[] args)
        {
            var dataIngress = new DataIngress();
            dataIngress.Run();
        }
    }
}
