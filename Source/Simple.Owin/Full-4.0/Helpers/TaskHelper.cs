using System.Threading.Tasks;

namespace Simple.Owin.Helpers
{
    public static class TaskHelper
    {
        static TaskHelper() {
            var completed = new TaskCompletionSource<int>();
            completed.SetResult(0);
            _completed = completed.Task;

            var canceled = new TaskCompletionSource<int>();
            canceled.SetCanceled();
            _canceled = canceled.Task;
        }

        private static readonly Task _canceled;
        private static readonly Task _completed;

        public static Task Canceled() {
            return _canceled;
        }

        public static Task<T> Canceled<T>() {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        public static Task Completed() {
            return _completed;
        }

        public static Task<T> Completed<T>(T value) {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }
    }
}