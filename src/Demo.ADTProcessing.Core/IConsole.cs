using System;

namespace Demo.ADTProcessing.Core
{
    public interface IConsole
    {
        void WriteLine(string value);
    }

    public class NullConsole : IConsole
    {
        public void WriteLine(string value)
        {}
    }

    public class OutConsole : IConsole
    {
        public void WriteLine(string value)
        {
            Console.Out.WriteLine(value);
        }
    }
}
