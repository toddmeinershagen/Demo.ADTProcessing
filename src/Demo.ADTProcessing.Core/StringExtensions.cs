namespace System
{
    public static class StringExtensions
    {
        public static T As<T>(this string value) where T : IConvertible
        {
            if (value == null)
            {
                throw new NullReferenceException(nameof(value));
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
