using System;

namespace XunitShould
{
    internal static class StringComparisonExtensions
    {
        public static StringComparer GetComparer(this StringComparison comparisonType) {
            switch (comparisonType) {
                case StringComparison.CurrentCulture:
                    return StringComparer.CurrentCulture;

                case StringComparison.CurrentCultureIgnoreCase:
                    return StringComparer.CurrentCultureIgnoreCase;

                case StringComparison.InvariantCulture:
                    return StringComparer.InvariantCulture;

                case StringComparison.InvariantCultureIgnoreCase:
                    return StringComparer.InvariantCultureIgnoreCase;

                case StringComparison.Ordinal:
                    return StringComparer.Ordinal;

                case StringComparison.OrdinalIgnoreCase:
                    return StringComparer.OrdinalIgnoreCase;

                default:
                    throw new ArgumentOutOfRangeException("comparisonType");
            }
        }
    }
}