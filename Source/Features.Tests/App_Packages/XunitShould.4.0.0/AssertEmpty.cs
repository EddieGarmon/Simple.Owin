using System;
using System.Collections;

using Xunit;
using Xunit.Sdk;

namespace XunitShould
{
    internal static partial class Should
    {
        /// <summary>
        ///     Verifies that a collection is empty.
        /// </summary>
        /// <param name="series">The series to be inspected</param>
        /// <exception cref="ArgumentNullException">Thrown when the series is null</exception>
        /// <exception cref="EmptyException">Thrown when the series is not empty</exception>
        public static void ShouldBeEmpty(this IEnumerable series) {
            Assert.Empty(series);
        }

        /// <summary>
        ///     Verifies that a series is not empty.
        /// </summary>
        /// <param name="series">The series to be inspected</param>
        /// <exception cref="ArgumentNullException">Thrown when a null series is passed</exception>
        /// <exception cref="NotEmptyException">Thrown when the series is empty</exception>
        public static void ShouldNotBeEmpty(this IEnumerable series) {
            Assert.NotEmpty(series);
        }
    }
}