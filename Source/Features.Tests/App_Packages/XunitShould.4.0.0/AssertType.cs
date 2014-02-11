using System;

using Xunit;
using Xunit.Sdk;

namespace XunitShould
{
    internal static partial class Should
    {
        public static void ShouldBeAssignableTo<T>(this object @object) {
            Assert.IsAssignableFrom<T>(@object);
        }

        public static void ShouldBeAssignableTo(this object @object, Type type) {
            Assert.IsAssignableFrom(type, @object);
        }

        /// <summary>
        ///     Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <typeparam name="T">The type the object should be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <returns>The object, casted to type T when successful</returns>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static T ShouldBeInstanceOf<T>(this object @object) {
            return Assert.IsType<T>(@object);
        }

        /// <summary>
        ///     Verifies that an object is exactly the given type (and not a derived type).
        /// </summary>
        /// <param name="object">The object to be evaluated</param>
        /// <param name="expectedType">The type the object should be</param>
        /// <exception cref="IsTypeException">Thrown when the object is not the given type</exception>
        public static void ShouldBeInstanceOf(this object @object, Type expectedType) {
            Assert.IsType(expectedType, @object);
        }

        /// <summary>
        ///     Verifies that an object is not exactly the given type.
        /// </summary>
        /// <typeparam name="T">The type the object should not be</typeparam>
        /// <param name="object">The object to be evaluated</param>
        /// <exception cref="IsTypeException">Thrown when the object is the given type</exception>
        public static void ShouldNotBeInstanceOf<T>(this object @object) {
            Assert.IsNotType<T>(@object);
        }

        /// <summary>
        ///     Verifies that an object is not exactly the given type.
        /// </summary>
        /// <param name="object">The object to be evaluated</param>
        /// <param name="expectedType">The type the object should not be</param>
        /// <exception cref="IsTypeException">Thrown when the object is the given type</exception>
        public static void ShouldNotBeInstanceOf(this object @object, Type expectedType) {
            Assert.IsNotType(expectedType, @object);
        }
    }
}