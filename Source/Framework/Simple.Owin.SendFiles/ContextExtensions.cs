using System;
using System.Threading;
using System.Threading.Tasks;
using Simple.Owin.Helpers;

namespace Simple.Owin.SendFiles
{
    using SendFileFunc = Func<string, // File Name and path
        long, // Initial file offset
        long?, // Byte count, null for remainder of file
        CancellationToken, // Cancel
        Task // Complete
        >;

    internal static class ContextExtensions
    {
        public static Task SendFile(this IContext context, string pathAndName, long startAt = 0, long? length = null) {
            SendFileFunc send = context.Environment.GetValueOrCreate(OwinKeys.SendFile.Async, () => SendFileNaive.GetSender(context));
            return send(pathAndName, startAt, length, context.CancellationToken);
        }

        public static void SetFileSender(this IContext context, SendFileFunc send) {
            context.Environment.SetValue(OwinKeys.SendFile.Async, send);
        }
    }
}