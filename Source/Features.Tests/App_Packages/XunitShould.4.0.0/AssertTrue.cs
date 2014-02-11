using System;

using Xunit.Sdk;

namespace XunitShould
{
    internal static partial class Should
    {
        /// <summary>
        ///     Verifies that an expression is true.
        /// </summary>
        /// <param name="condition">The condition to be inspected</param>
        /// <exception cref="TrueException">Thrown when the condition is false</exception>
        public static void ShouldBeTrue(this bool condition) {
            if (!condition) {
                throw new TrueException(null);
            }
        }

        /// <summary>
        ///     Verifies that an expression is true.
        /// </summary>
        /// <param name="condition">The condition to be inspected</param>
        /// <param name="userMessage">The message to be shown when the condition is false</param>
        /// <exception cref="TrueException">Thrown when the condition is false</exception>
        public static void ShouldBeTrue(this bool condition, string userMessage) {
            if (!condition) {
                throw new TrueException(userMessage);
            }
        }

        /// <summary>
        ///     Verifies that an expression is true.
        /// </summary>
        /// <param name="condition">The condition to be inspected</param>
        /// <param name="messageGenerator">The message to be shown when the condition is false</param>
        /// <exception cref="TrueException">Thrown when the condition is false</exception>
        public static void ShouldBeTrue(this bool condition, Func<string> messageGenerator) {
            if (!condition) {
                throw new TrueException(messageGenerator());
            }
        }
    }
}