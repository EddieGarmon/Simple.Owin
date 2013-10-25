using System;
using System.Threading.Tasks;

namespace Simple.Owin.Helpers
{
    public static class TaskHelper
    {
        static TaskHelper() {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);
            _completed = tcs.Task;
        }

        private static readonly Task _completed;

        /// <summary>
        /// Creates a completed <see cref="Task"/>.
        /// </summary>
        public static Task Completed() {
            return _completed;
        }

        /// <summary>
        /// Creates a completed <see cref="Task"/> with the specified <c>Result</c> value.
        /// </summary>
        /// <typeparam name="T">The type of the <c>Result</c> value.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>A <see cref="Task{T}"/> set to completed with the specified value as the <c>Result</c>.</returns>
        public static Task<T> Completed<T>(T value) {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }

        /// <summary>
        /// Creates a faulted <see cref="Task"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>A faulted <see cref="Task"/> with the <c>Exception</c> property set to the specified value.</returns>
        public static Task Exception(Exception exception) {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetException(exception);
            return tcs.Task;
        }
    }
}