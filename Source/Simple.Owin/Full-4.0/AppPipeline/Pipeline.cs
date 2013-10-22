using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Simple.Owin.Helpers;

namespace Simple.Owin.AppPipeline
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using MiddlewareFunc = Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>;

    public static class Pipeline
    {
        public static AppFunc ReturnDone {
            get { return environment => TaskHelper.Completed(); }
        }

        public static AppFunc ReturnNotFound {
            get {
                return environment => {
                           OwinContext context = OwinContext.Get(environment);
                           context.Response.Status = Status.Is.NotFound;
                           return TaskHelper.Completed();
                       };
            }
        }

        public static IPipelineBuilder Use(MiddlewareFunc middleware) {
            var builder = new PipelineBuilder();
            builder.Use(middleware);
            return builder;
        }
    }
}