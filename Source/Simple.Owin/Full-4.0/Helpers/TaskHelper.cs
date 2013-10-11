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

        public static Task Completed() {
            return _completed;
        }
    }
}