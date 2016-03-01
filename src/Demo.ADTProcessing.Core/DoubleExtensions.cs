using System;

namespace Demo.ADTProcessing.Core
{
    public static class DoubleExtensions
    {
        public static int Rounded(this double value)
        {
            return Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));
        }
    }
}
