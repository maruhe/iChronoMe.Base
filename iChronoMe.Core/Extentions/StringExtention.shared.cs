using System;

namespace iChronoMe.Core.Types
{
    public static class StringExtention
    {
        public static string WithMaxLength(this string value, int maxLength)
        {
            return value?.Substring(0, Math.Min(value.Length, maxLength));
        }
    }
}
