using System;

using Xunit.Sdk;

namespace XunitShould
{
    internal static partial class Should
    {
        /// <summary>
        ///     Verifies that the condition is false.
        /// </summary>
        /// <param name="condition">The condition to be tested</param>
        /// <exception cref="FalseException">Thrown if the condition is not false</exception>
        public static void ShouldBeFalse(this bool condition) {
            if (condition) {
                throw new FalseException(null);
            }
        }

        /// <summary>
        ///     Verifies that the condition is false.
        /// </summary>
        /// <param name="condition">The condition to be tested</param>
        /// <param name="userMessage">The message to show when the condition is not false</param>
        /// <exception cref="FalseException">Thrown if the condition is not false</exception>
        public static void ShouldBeFalse(this bool condition, string userMessage) {
            if (condition) {
                throw new FalseException(userMessage);
            }
        }

        /// <summary>
        ///     Verifies that the condition is false.
        /// </summary>
        /// <param name="condition">The condition to be tested</param>
        /// <param name="messageGenerator">The message to show when the condition is not false</param>
        /// <exception cref="FalseException">Thrown if the condition is not false</exception>
        public static void ShouldBeFalse(this bool condition, Func<string> messageGenerator) {
            if (condition) {
                throw new FalseException(messageGenerator());
            }
        }
    }
}