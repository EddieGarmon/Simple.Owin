using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Simple.Owin.Extensions.Streams;
using Simple.Owin.Helpers;

namespace Simple.Owin
{
    using SendFileFunc = Func<string, // File Name and path
        long, // Initial file offset
        long?, // Byte count, null for remainder of file
        CancellationToken, // Cancel
        Task // Complete
        >;

    internal static class SendFileNaive
    {
        public static SendFileFunc GetSender(OwinContext context) {
            return (path, offset, count, cancellationToken) => {
                       var source = File.OpenRead(path);
                       long contentLength = Math.Min(source.Length - offset, count ?? long.MaxValue);
                       context.Response.Headers.ContentLength = contentLength;
                       if (contentLength <= 0) {
                           source.Close();
                           return TaskHelper.Completed();
                       }
                       source.Seek(offset, SeekOrigin.Begin);
                       return source.CopyToAsync(context.Response.Body, contentLength, cancellationToken)
                                    .ContinueWith(_ => source.Close(), TaskContinuationOptions.ExecuteSynchronously);
                   };
        }
    }
}