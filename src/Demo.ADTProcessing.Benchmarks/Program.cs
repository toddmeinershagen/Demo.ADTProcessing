using System;

using BenchmarkDotNet.Running;

using Demo.ADTProcessing.PerfTests;

namespace Demo.ADTProcessing.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RabbitQueueProvider>();
            Console.ReadLine();
        }
    }
}
