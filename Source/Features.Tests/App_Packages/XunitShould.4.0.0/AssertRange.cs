using System;
using System.Collections.Generic;

using XunitShould.Sdk;

namespace XunitShould
{
    internal static partial class Should
    {
        public static void ShouldBeGreaterThan<T>(this T actual, T expected) where T : IComparable<T> {
            if (actual.CompareTo(expected) <= 0) {
                throw new InRangeException(actual, expected, false, null, false);
            }
        }

        public static void ShouldBeGreaterThan<T>(this T actual, T expected, IComparer<T> comparer) {
            if (comparer.Compare(actual, expected) <= 0) {
                throw new InRangeException(actual, expected, false, null, false);
            }
        }

        public static void ShouldBeGreaterThanOrEqualTo<T>(this T actual, T expected) where T : IComparable<T> {
            if (actual.CompareTo(expected) < 0) {
                throw new InRangeException(actual, expected, true, null, false);
            }
        }

        public static void ShouldBeGreaterThanOrEqualTo<T>(this T actual, T expected, IComparer<T> comparer) {
            if (comparer.Compare(actual, expected) < 0) {
                throw new InRangeException(actual, expected, true, null, false);
            }
        }

        public static void ShouldBeInRange<T>(this T actual, T low, T high) where T : IComparable<T> {
            ShouldBeInRange(actual, low, true, high, true);
        }

        public static void ShouldBeInRange<T>(this T actual, T low, T high, IComparer<T> comparer) {
            ShouldBeInRange(actual, low, true, high, true, comparer);
        }

        public static void ShouldBeInRange<T>(this T actual, T low, bool lowInclusive, T high, bool highInclusive) where T : IComparable<T> {
            int compareLow = actual.CompareTo(low);
            int compareHigh = actual.CompareTo(high);
            if ((lowInclusive && compareLow < 0) || (!lowInclusive && compareLow <= 0) || (highInclusive && compareHigh > 0) ||
                (!highInclusive && compareHigh >= 0)) {
                throw new InRangeException(actual, low, lowInclusive, high, highInclusive);
            }
        }

        public static void ShouldBeInRange<T>(this T actual, T low, bool lowInclusive, T high, bool highInclusive, IComparer<T> comparer) {
            int compareLow = comparer.Compare(actual, low);
            int compareHigh = comparer.Compare(actual, high);
            if ((lowInclusive && compareLow < 0) || (!lowInclusive && compareLow <= 0) || (highInclusive && compareHigh > 0) ||
                (!highInclusive && compareHigh >= 0)) {
                throw new InRangeException(actual, low, lowInclusive, high, highInclusive);
            }
        }

        public static void ShouldBeLessThan<T>(this T actual, T expected) where T : IComparable<T> {
            if (actual.CompareTo(expected) >= 0) {
                throw new InRangeException(actual, null, false, expected, false);
            }
        }

        public static void ShouldBeLessThan<T>(this T actual, T expected, IComparer<T> comparer) {
            if (comparer.Compare(actual, expected) >= 0) {
                throw new InRangeException(actual, null, false, expected, false);
            }
        }

        public static void ShouldBeLessThanOrEqualTo<T>(this T actual, T expected) where T : IComparable<T> {
            if (actual.CompareTo(expected) > 0) {
                throw new InRangeException(actual, null, false, expected, true);
            }
        }

        public static void ShouldBeLessThanOrEqualTo<T>(this T actual, T expected, IComparer<T> comparer) {
            if (comparer.Compare(actual, expected) > 0) {
                throw new InRangeException(actual, null, false, expected, true);
            }
        }

        public static void ShouldNotBeInRange<T>(this T actual, T low, T high) where T : IComparable<T> {
            ShouldNotBeInRange(actual, low, true, high, true);
        }

        public static void ShouldNotBeInRange<T>(this T actual, T low, T high, IComparer<T> comparer) {
            ShouldNotBeInRange(actual, low, true, high, true, comparer);
        }

        public static void ShouldNotBeInRange<T>(this T actual, T low, bool lowInclusive, T high, bool highInclusive)
            where T : IComparable<T> {
            int compareLow = actual.CompareTo(low);
            int compareHigh = actual.CompareTo(high);
            if (((lowInclusive && compareLow >= 0) || (!lowInclusive && compareLow > 0)) &&
                ((highInclusive && compareHigh <= 0) || (!highInclusive && compareHigh < 0))) {
                throw new NotInRangeException(actual, low, lowInclusive, high, highInclusive);
            }
        }

        public static void ShouldNotBeInRange<T>(this T actual, T low, bool lowInclusive, T high, bool highInclusive, IComparer<T> comparer) {
            int compareLow = comparer.Compare(actual, low);
            int compareHigh = comparer.Compare(actual, high);
            if (((lowInclusive && compareLow >= 0) || (!lowInclusive && compareLow > 0)) &&
                ((highInclusive && compareHigh <= 0) || (!highInclusive && compareHigh < 0))) {
                throw new NotInRangeException(actual, low, lowInclusive, high, highInclusive);
            }
        }
    }
}