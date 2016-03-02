using System;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Demo.ADTProcessing.PerfTests
{
    [TestClass]
    public class ADTProcessingTests
    {
        [TestMethod]
        public void Test()
        {
            //TODO:  Keep test going until the REST endpoint clears
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }
}
