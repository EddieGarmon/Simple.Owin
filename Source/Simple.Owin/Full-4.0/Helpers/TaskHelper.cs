using System.Threading.Tasks;

namespace Simple.Owin.Helpers
{
    public static class TaskHelper
    {
        private static readonly Task _completed;

        static TaskHelper() {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);
            _completed = tcs.Task;
        }

        public static Task Completed() {
            //var tcs = new TaskCompletionSource<int>();
            //tcs.SetResult(0);
            //return tcs.Task;
            return _completed;
        }
    }
}