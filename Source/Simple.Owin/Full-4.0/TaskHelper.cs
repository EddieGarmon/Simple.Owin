using System.Threading.Tasks;

namespace Simple.Owin
{
    public static class TaskHelper
    {
        public static Task Completed() {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetResult(0);
            return tcs.Task;
        }
    }
}