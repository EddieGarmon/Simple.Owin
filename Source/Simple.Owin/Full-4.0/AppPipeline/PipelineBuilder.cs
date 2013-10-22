using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Owin.AppPipeline
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using MiddlewareFunc = Func<IDictionary<string, object>, Func<IDictionary<string, object>, Task>, Task>;

    internal class PipelineBuilder : IPipelineBuilder
    {
        private readonly Stack<MiddlewareFunc> _stack = new Stack<MiddlewareFunc>();
        private int _useCount;

        public IPipelineBuilder Use(MiddlewareFunc middleware) {
            if (middleware == null) {
                throw new ArgumentNullException("middleware");
            }
            _stack.Push(middleware);
            return this;
        }

        public AppFunc Use(AppFunc app) {
            if (app == null) {
                throw new ArgumentNullException("app");
            }
            if (Interlocked.Increment(ref _useCount) > 1) {
                throw new InvalidOperationException("PipelineBuilder may only be used once.");
            }
            while (_stack.Count > 0) {
                var middleware = _stack.Pop();
                var next = app;
                app = env => middleware(env, next);
            }
            return app;
        }
    }
}