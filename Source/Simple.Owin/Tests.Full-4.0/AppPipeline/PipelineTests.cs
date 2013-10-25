using Simple.Owin.Helpers;
using Simple.Owin.Hosting;
using Simple.Owin.Testing;

using Xunit;

using XunitShould;

namespace Simple.Owin.AppPipeline
{
    public class PipelineTests
    {
        [Fact(Skip = "Not Finished")]
        public void BuildControlPipeline() {
            var pipeline = new Pipeline();
            pipeline.First(c => c.IsGet("/", env => TaskHelper.Completed()));
        }

        [Fact]
        public void BuildsInCorrectOrder() {
            var pipeline = new Pipeline();
            pipeline.Use((env, next) => {
                             OwinContext.Get(env)
                                        .TraceOutput.Write("Before.");
                             return next(env);
                         })
                    .Use((env, next) => {
                             next(env)
                                 .Wait();
                             OwinContext.Get(env)
                                        .TraceOutput.Write("After.");
                             return TaskHelper.Completed();
                         })
                    .Use(env => {
                             OwinContext.Get(env)
                                        .TraceOutput.Write("App.");
                             return TaskHelper.Completed();
                         });
            var host = new TestHostAndServer(pipeline);
            host.Process(TestRequest.Get("/"));
            host.TraceOutputValue.ShouldEqual("Before.App.After.");
        }

        [Fact]
        public void BuildsWithSetup() {
            var pipeline = new Pipeline();
            pipeline.Use((env, next) => {
                             OwinContext.Get(env)
                                        .TraceOutput.Write("Do Before.");
                             return next(env);
                         },
                         env => new OwinHostContext(env).TraceOutput.Write("Setup Before."))
                    .Use((env, next) => {
                             next(env)
                                 .Wait();
                             OwinContext.Get(env)
                                        .TraceOutput.Write("Do After.");
                             return TaskHelper.Completed();
                         },
                         env => new OwinHostContext(env).TraceOutput.Write("Setup After."))
                    .Use(env => {
                             OwinContext.Get(env)
                                        .TraceOutput.Write("Do App.");
                             return TaskHelper.Completed();
                         },
                         env => new OwinHostContext(env).TraceOutput.Write("Setup App."));
            var host = new TestHostAndServer(pipeline);
            host.Process(TestRequest.Get("/"));
            host.TraceOutputValue.ShouldEqual("Setup Before.Setup After.Setup App.Do Before.Do App.Do After.");
        }
    }
}