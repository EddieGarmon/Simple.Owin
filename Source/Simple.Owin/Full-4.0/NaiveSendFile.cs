using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Simple.Owin.Extensions;
using Simple.Owin.Helpers;

namespace Simple.Owin
{
    using SendFileFunc = Func<string, // File Name and path
        long, // Initial file offset
        long?, // Byte count, null for remainder of file
        CancellationToken, // Cancel
        Task // Complete
        >;

    internal static class NaiveSendFile
    {
        public static SendFileFunc GetSender(OwinContext context) {
            return (path, offset, count, cancellationToken) => {
                       using (var source = File.OpenRead(path)) {
                           long contentLength = Math.Min(source.Length - offset, count ?? long.MaxValue);
                           context.Response.Headers.ContentLength = contentLength;
                           if (contentLength > 0) {
                               source.Seek(offset, SeekOrigin.Begin);

                               const int bufferSize = 32768;
                               var buffer = new byte[bufferSize];
                               long sent = 0;
                               while (true) {
                                   int readLength = (contentLength - sent > bufferSize) ? bufferSize : (int)(contentLength - sent);
                                   Task<int> reader = source.ReadAsync(buffer, 0, readLength);
                                   reader.Wait(cancellationToken);
                                   if (cancellationToken.IsCancellationRequested) {
                                       return TaskHelper.Completed();
                                   }
                                   int bytesRead = reader.Result;
                                   if (bytesRead != 0) {
                                       context.Response.Body.WriteAsync(buffer, 0, bytesRead)
                                              .Wait(cancellationToken);
                                       sent += bytesRead;
                                       if (cancellationToken.IsCancellationRequested) {
                                           return TaskHelper.Completed();
                                       }
                                   }
                                   else {
                                       break;
                                   }
                               }
                           }
                           return TaskHelper.Completed();
                       }
                   };
        }
    }
}