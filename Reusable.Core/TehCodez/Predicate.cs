using System;

namespace Reusable
{
    public static class Predicate
    {
        public static Func<string, bool> StartsWith(string value)
        {
            return left => left.StartsWith(value);
        }

        public static Func<string, bool> StartsWith(string value, StringComparison comparisonType)
        {
            return left => left.StartsWith(value, comparisonType);
        }

        public static Func<string, bool> EndsWith(string value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return left => left.EndsWith(value, comparisonType);
        }

        public static Func<string, bool> Contains(string value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return left => left.IndexOf(value, comparisonType) >= 0;
        }
    }
}
