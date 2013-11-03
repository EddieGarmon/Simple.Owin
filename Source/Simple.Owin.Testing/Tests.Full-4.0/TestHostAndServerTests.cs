using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Simple.Owin.AppPipeline;
using Simple.Owin.Helpers;
using Simple.Owin.Hosting;

using Xunit;

using XunitShould;

namespace Simple.Owin.Testing
{
    using AppFunc = Func< //
        IDictionary<string, object>, // owin request environment
        Task // completion signal
        >;
    using MiddlewareFunc = Func< //
        IDictionary<string, object>, // owin request environment
        Func<IDictionary<string, object>, Task>, // next AppFunc in pipeline
        Task // completion signal
        >;

    public class TestHostAndServerTests
    {
        [Fact]
        public void HostAppFunc() {
            AppFunc testCode = env => {
                                   OwinContext.Get(env)
                                              .TraceOutput.Write("App");
                                   return TaskHelper.Completed();
                               };
            var host = new TestHostAndServer(testCode);
            host.ProcessGet("/");
            host.TraceOutputValue.ShouldEqual("App");
        }

        [Fact]
        public void HostMiddlewareFunc() {
            MiddlewareFunc testCode = (env, next) => {
                                          OwinContext.Get(env)
                                                     .TraceOutput.Write("Middleware");
                                          return next(env);
                                      };
            var host = new TestHostAndServer(testCode);
            host.ProcessGet("/");
            host.TraceOutputValue.ShouldEqual("Middleware");
        }

        [Fact]
        public void HostPipeline() {
            var pipeline = new Pipeline();
            pipeline.Use(env => {
                             OwinContext.Get(env)
                                        .TraceOutput.Write("App");
                             return TaskHelper.Completed();
                         },
                         hostEnv => new OwinHostContext(hostEnv).TraceOutput.Write("Setup-"));
            var host = new TestHostAndServer(pipeline);
            host.ProcessGet("/");
            host.TraceOutputValue.ShouldEqual("Setup-App");
        }
    }
}