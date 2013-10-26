using System;

namespace Simple.Owin.Extensions
{
    public static class ReflectionExtensions
    {
        public static T GetDelegate<T>(this Type onType, string staticMethodName, bool ignoreCase = false, bool throwOnBindFailure = true) {
            Type delegateType = typeof(T);
            return (T)(object)Delegate.CreateDelegate(delegateType, onType, staticMethodName, ignoreCase, throwOnBindFailure);
        }

        public static T GetDelegate<T>(this object onInstance,
                                       string instanceMethodName,
                                       bool ignoreCase = false,
                                       bool throwOnBindFailure = true) {
            Type delegateType = typeof(T);
            return (T)(object)Delegate.CreateDelegate(delegateType, onInstance, instanceMethodName, ignoreCase, throwOnBindFailure);
        }
    }
}