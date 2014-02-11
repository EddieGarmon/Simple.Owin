using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Sdk;

namespace XunitShould
{
    internal static partial class Should
    {
        /// <summary>
        ///     Verifies that a series contains a given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be verified</typeparam>
        /// <param name="series">The series to be inspected</param>
        /// <param name="expected">The object expected to be in the series</param>
        /// <exception cref="ContainsException">Thrown when the object is not present in the series</exception>
        public static void ShouldContain<T>(this IEnumerable<T> series, T expected) {
            Assert.Contains(expected, series);
        }

        /// <summary>
        ///     Verifies that a series contains a given object, using a comparer.
        /// </summary>
        /// <typeparam name="T">The type of the object to be verified</typeparam>
        /// <param name="series">The series to be inspected</param>
        /// <param name="expected">The object expected to be in the series</param>
        /// <param name="comparer">The comparer used to equate objects in the series with the expected object</param>
        /// <exception cref="ContainsException">Thrown when the object is not present in the series</exception>
        public static void ShouldContain<T>(this IEnumerable<T> series, T expected, IEqualityComparer<T> comparer) {
            Assert.Contains(expected, series, comparer);
        }

        /// <summary>
        ///     Verifies that a string contains a given sub-string, using the given comparison type.
        /// </summary>
        /// <param name="actual">The string to be inspected</param>
        /// <param name="fragment">The sub-string expected to be in the string</param>
        /// <param name="comparisonType">The type of string comparison to perform</param>
        /// <exception cref="ContainsException">Thrown when the sub-string is not present inside the string</exception>
        /// <remarks></remarks>
        public static void ShouldContain(this string actual,
                                         string fragment,
                                         StringComparison comparisonType = StringComparison.CurrentCulture) {
            Assert.Contains(fragment, actual, comparisonType);
        }

        /// <summary>
        ///     Verifies that a string ends with a given sub-string, using the given comparison type.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="ending">The expected end.</param>
        /// <param name="stringComparison">The string comparison.</param>
        /// <remarks></remarks>
        public static void ShouldEndWith(this string actual,
                                         string ending,
                                         StringComparison stringComparison = StringComparison.CurrentCulture) {
            if (actual.Length < ending.Length) {
                throw new EqualException(ending, actual);
            }
            string temp = actual.Substring(actual.Length - ending.Length);
            Assert.Equal(ending, temp, stringComparison.GetComparer());
        }

        public static T ShouldHaveSingle<T>(this IEnumerable<T> actual, Predicate<T> filter) {
            return Assert.Single(actual, filter);
        }

        /// <summary>
        ///     Verifies that a series does not contain a given object.
        /// </summary>
        /// <typeparam name="T">The type of the object to be compared</typeparam>
        /// <param name="expected">The object that is expected not to be in the series</param>
        /// <param name="series">The series to be inspected</param>
        /// <exception cref="DoesNotContainException">Thrown when the object is present inside the series</exception>
        public static void ShouldNotContain<T>(this IEnumerable<T> series, T expected) {
            Assert.DoesNotContain(expected, series);
        }

        /// <summary>
        ///     Verifies that a series does not contain a given object, using a comparer.
        /// </summary>
        /// <typeparam name="T">The type of the object to be compared</typeparam>
        /// <param name="expected">The object that is expected not to be in the series</param>
        /// <param name="series">The series to be inspected</param>
        /// <param name="comparer">The comparer used to equate objects in the series with the expected object</param>
        /// <exception cref="DoesNotContainException">Thrown when the object is present inside the series</exception>
        public static void ShouldNotContain<T>(this IEnumerable<T> series, T expected, IEqualityComparer<T> comparer) {
            Assert.DoesNotContain(expected, series, comparer);
        }

        /// <summary>
        ///     Verifies that a string does not contain a given sub-string, using the given comparison type.
        /// </summary>
        /// <param name="actual">The string to be inspected</param>
        /// <param name="fragment">The sub-string which is expected not to be in the string</param>
        /// <param name="comparisonType">The type of string comparison to perform</param>
        /// <exception cref="DoesNotContainException">Thrown when the sub-string is present inside the given string</exception>
        public static void ShouldNotContain(this string actual,
                                            string fragment,
                                            StringComparison comparisonType = StringComparison.CurrentCulture) {
            Assert.DoesNotContain(fragment, actual, comparisonType);
        }

        public static void ShouldNotEndWith(this string actual,
                                            string ending,
                                            StringComparison comparisonType = StringComparison.CurrentCulture) {
            if (actual.Length < ending.Length) {
                return;
            }
            string temp = actual.Substring(actual.Length - ending.Length);
            Assert.NotEqual(ending, temp, comparisonType.GetComparer());
        }

        public static void ShouldNotStartWith(this string actual,
                                              string begining,
                                              StringComparison comparisonType = StringComparison.CurrentCulture) {
            if (actual.Length < begining.Length) {
                return;
            }
            string temp = actual.Substring(0, begining.Length);
            Assert.NotEqual(begining, temp, comparisonType.GetComparer());
        }

        /// <summary>
        ///     Verifies that a string starts with a given sub-string, using the given comparison type.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="begining">The expected start.</param>
        /// <param name="comparisonType">The string comparison.</param>
        /// <remarks></remarks>
        public static void ShouldStartWith(this string actual,
                                           string begining,
                                           StringComparison comparisonType = StringComparison.CurrentCulture) {
            if (actual.Length < begining.Length) {
                throw new EqualException(begining, actual);
            }
            string temp = actual.Substring(0, begining.Length);
            Assert.Equal(begining, temp, comparisonType.GetComparer());
        }
    }
}