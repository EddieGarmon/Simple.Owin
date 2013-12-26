using Xunit;
using Xunit.Sdk;

namespace XunitShould
{
    internal static partial class Should
    {
        /// <summary>
        ///     Verifies that an object reference is null.
        /// </summary>
        /// <param name="object">The object to be inspected</param>
        /// <exception cref="NullException">Thrown when the object reference is not null</exception>
        public static void ShouldBeNull(this object @object) {
            Assert.Null(@object);
        }

        /// <summary>
        ///     Verifies that two objects are the same instance.
        /// </summary>
        /// <param name="actual">The actual object instance</param>
        /// <param name="expected">The expected object instance</param>
        /// <exception cref="SameException">Thrown when the objects are not the same instance</exception>
        public static void ShouldBeSameAs(this object actual, object expected) {
            Assert.Same(expected, actual);
        }

        /// <summary>
        ///     Verifies that an object reference is not null.
        /// </summary>
        /// <param name="object">The object to be validated</param>
        /// <exception cref="NotNullException">Thrown when the object is not null</exception>
        public static void ShouldNotBeNull(this object @object) {
            Assert.NotNull(@object);
        }

        /// <summary>
        ///     Verifies that two objects are not the same instance.
        /// </summary>
        /// <param name="actual">The actual object instance</param>
        /// <param name="expected">The expected object instance</param>
        /// <exception cref="NotSameException">Thrown when the objects are the same instance</exception>
        public static void ShouldNotBeSameAs(this object actual, object expected) {
            Assert.NotSame(expected, actual);
        }
    }
}