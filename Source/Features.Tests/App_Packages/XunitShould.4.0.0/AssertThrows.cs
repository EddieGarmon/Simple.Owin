using System;

using Xunit;

using XunitShould.Sdk;

namespace XunitShould
{
    internal static partial class Should
    {
        public static void ShouldThrow<T>(this Action testCode, string message = null) where T : Exception {
            var thrown = Assert.Throws<T>(() => testCode());
            if (message != null) {
                Assert.Equal(message, thrown.Message);
            }
        }

        public static void ShouldThrow<T>(this Action testCode, T exception) where T : Exception {
            var thrown = Assert.Throws<T>(() => testCode());
            Assert.Equal(exception, thrown, ExceptionComparer.Instance);
        }

        public static void ShouldThrow<T>(this Func<object> testCode, string message = null) where T : Exception {
            var thrown = Assert.Throws<T>(() => testCode());
            if (message != null) {
                Assert.Equal(message, thrown.Message);
            }
        }

        public static void ShouldThrow<T>(this Func<object> testCode, T exception) where T : Exception {
            var thrown = Assert.Throws<T>(() => testCode());
            Assert.Equal(exception, thrown, ExceptionComparer.Instance);
        }
    }
}