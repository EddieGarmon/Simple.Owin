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

        public static Task<T> Completed<T>(T value) {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }

        public static Task Completed() {
            return _completed;
        }
    }
}