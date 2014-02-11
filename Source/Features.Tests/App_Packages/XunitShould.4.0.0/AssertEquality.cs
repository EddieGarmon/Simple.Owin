using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using Xunit.Sdk;

using XunitShould.Sdk;

namespace XunitShould
{
    internal static partial class Should
    {
        /// <summary>
        ///     Verifies that two objects are equal, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="expected">The expected value</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void ShouldEqual<T>(this T actual, T expected) {
            Assert.Equal(expected, actual);
        }

        /// <summary>
        ///     Verifies that two objects are equal, using a custom comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="actual">The value to be compared against</param>
        /// <param name="expected">The expected value</param>
        /// <param name="comparer">The comparer used to compare the two objects</param>
        /// <exception cref="EqualException">Thrown when the objects are not equal</exception>
        public static void ShouldEqual<T>(this T actual, T expected, IEqualityComparer<T> comparer) {
            Assert.Equal(expected, actual, comparer);
        }

        public static void ShouldEqual<T>(this IEnumerable<T> actual, params T[] expected) {
            ShouldEqual(actual, expected, Comparer<T>.Default);
        }

        public static void ShouldEqual<T>(this IEnumerable<T> actual, IEnumerable<T> expected) {
            ShouldEqual(actual, expected, Comparer<T>.Default);
        }

        public static void ShouldEqual<T>(this IEnumerable<T> actual, IEnumerable<T> expected, IComparer<T> comparer) {
            List<T> actualList = actual.ToList();
            List<T> expectedList = expected.ToList();

            int index = 0;
            int lastBoth = Math.Min(actualList.Count, expectedList.Count);
            for (; index < lastBoth; index++) {
                T actualItem = actualList[index];
                T expectedItem = expectedList[index];
                if (comparer.Compare(actualItem, expectedItem) != 0) {
                    throw new EnumerableEqualException(expectedItem, actualItem, index, expectedList.Count, actualList.Count);
                }
            }
            if (index != expectedList.Count) {
                throw new EnumerableEqualException(expectedList[index], null, index, expectedList.Count, actualList.Count);
            }
            if (index != actualList.Count) {
                throw new EnumerableEqualException(null, actualList[index], index, expectedList.Count, actualList.Count);
            }
        }

        /// <summary>
        ///     Verifies that a string equals a given string, using the given comparison type.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="expected">The expected.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <remarks></remarks>
        public static void ShouldEqual(this string actual,
                                       string expected,
                                       StringComparison comparisonType = StringComparison.CurrentCulture) {
            Assert.Equal(expected, actual, comparisonType.GetComparer());
        }

        public static void ShouldEqualWithinPrecision(this double actual, double expected, int precision) {
            Assert.Equal(expected, actual, precision);
        }

        public static void ShouldEqualWithinPrecision(this decimal actual, decimal expected, int precision) {
            Assert.Equal(expected, actual, precision);
        }

        /// <summary>
        ///     Verifies that two objects are not equal, using a default comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="actual">The actual object</param>
        /// <param name="expected">The expected object</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public static void ShouldNotEqual<T>(this T actual, T expected) {
            Assert.NotEqual(expected, actual);
        }

        /// <summary>
        ///     Verifies that two objects are not equal, using a custom comparer.
        /// </summary>
        /// <typeparam name="T">The type of the objects to be compared</typeparam>
        /// <param name="actual">The actual object</param>
        /// <param name="expected">The expected object</param>
        /// <param name="comparer">The comparer used to examine the objects</param>
        /// <exception cref="NotEqualException">Thrown when the objects are equal</exception>
        public static void ShouldNotEqual<T>(this T actual, T expected, IEqualityComparer<T> comparer) {
            Assert.NotEqual(expected, actual, comparer);
        }
    }
}