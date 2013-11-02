using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Simple.Owin.Extensions.Streams;
using Simple.Owin.Helpers;

namespace Simple.Owin
{
    using MiddlewareFunc = Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>;

    public static partial class NativeMiddleware
    {
        public static MiddlewareFunc PrintExceptions {
            get {
                const string errorTemplate = @"
<html>
        <head>
                <title>{0}</title>
        </head>
                <body>
                <h1>{0}</h1>
                <pre>{1}</pre>
        </body>
</html>";

                return (env, next) => {
                           try {
                               next(env);
                           }
                           catch (Exception e) {
                               var context = OwinContext.Get(env);
                               context.Response.Body.Write(string.Format(errorTemplate, e.Message, e.ToString()));
                           }
                           return TaskHelper.Completed();
                       };
            }
        }
    }
}